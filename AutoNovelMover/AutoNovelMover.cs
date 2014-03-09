﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace AutoNovelMover
{
    public partial class AutoNovelMover : Form
    {
        // ---- ini 파일 의 읽고 쓰기를 위한 API 함수 선언 ----
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(    // ini Read 함수
                    String section,
                    String key,
                    String def,
                    StringBuilder retVal,
                    int size,
                    String filePath);

        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(  // ini Write 함수
                    String section,
                    String key,
                    String val,
                    String filePath);

        /// <summary>
        /// 소설의 파일정보를 저장해 놓는다.
        /// </summary>
        protected Dictionary<string, FileInfo> novelFileInfos = new Dictionary<string, FileInfo>();
        // 진행바 값을 보내기 위한 콜백
        public delegate void SetProgCallBack(int value);
        // 쓰레드
        private System.Threading.Thread copyThread;
        // 복사중인지 체크
        private bool working { get; set; }

        public AutoNovelMover()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 폼 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoNovelMover_Load(object sender, EventArgs e)
        {
            NovelListView.View = View.Details;
            // 컬럼 전체 선택
            NovelListView.FullRowSelect = true;
            // 그리드라인 그리기
            NovelListView.GridLines = true;
            NovelListView.MultiSelect = true;

            NovelListView.Columns.Add("번호", 40, HorizontalAlignment.Right);
            NovelListView.Columns.Add("파일명", 180, HorizontalAlignment.Left);
            NovelListView.Columns.Add("복사(생성)될 폴더명", 155, HorizontalAlignment.Left);
            NovelListView.Columns.Add("사이즈", 70, HorizontalAlignment.Right);

            // 로그창
            LogListview.View = View.Details;
            LogListview.FullRowSelect = false;
            LogListview.GridLines = true;

            LogListview.Columns.Add("번호", 40, HorizontalAlignment.Right);
            LogListview.Columns.Add("예외사항", 420, HorizontalAlignment.Left);

            working = false;
            // 복사 타겟폴더를 읽어옵니다.
            StringBuilder tmpRetVal = new StringBuilder(2000);
            GetPrivateProfileString("Folder", "SelectedPath", "", tmpRetVal, 2000, "./Parameter.ini");
            targetDir.Text = tmpRetVal.ToString();
        }

        /// <summary>
        /// 리스트뷰에 드래그 앤 드롭을 했을때, 리스트뷰로 구성해준다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelListView_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                {
                    NovelListView.BeginUpdate();

                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    foreach (string fileName in files)
                    {
                        if (novelFileInfos.ContainsKey(fileName)) { continue; }

                        // 디렉토리면 디렉토리에 맞게 처리
                        if (Directory.Exists(fileName))
                        {
                            string[] dirs = Directory.GetDirectories(fileName);
                            // 폴더를 드래그앤드롭 한거면, 폴더내에 리스트를 리스트로 구성한다.
                            if (dirs == null || dirs.Length == 0)
                            {
                                return;
                            }

                            foreach (var dirFolderName in dirs)
                            {
                                foreach (var file in Directory.GetFiles(dirFolderName))
                                {
                                    AddNovelItem(file);
                                }
                            }
                        }
                        else if (File.Exists(fileName)) // 파일이면 파일에 맞게 처리
                        {
                            AddNovelItem(fileName);
                        }
                    }

                    NovelListView.EndUpdate();
                    // 자동 스크롤
                    NovelListView.Items[NovelListView.Items.Count - 1].EnsureVisible();
                    // 진행률에 현재 추가된 소설 수 갱신
                    progressText.Text = string.Format("0 / {0} (0%)", novelFileInfos.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "리스트 구성 에러");
            }
        }

        /// <summary>
        /// 파일정보를 리스트로 구성합니다.
        /// </summary>
        /// <param name="fileName"></param>
        private void AddNovelItem(string fileName)
        {
            // 파일의 정보를 읽어온다.
            FileInfo fileInfo = new FileInfo(fileName);
            if (novelFileInfos.ContainsKey(fileInfo.Name) == false)
            {
                // 새로운 리스트 아이템 구성
                ListViewItem newItem = new ListViewItem((novelFileInfos.Count + 1).ToString());
                newItem.SubItems.Add(fileInfo.Name);
                // 파일확장자를 제외한 파일이름만 추출
                string folderMame = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                int lastIndex = folderMame.LastIndexOf(" ");
                folderMame = folderMame.Substring(0, lastIndex);
                // 복사될 폴더이름
                newItem.SubItems.Add(folderMame);
                newItem.SubItems.Add(GetFileSize(fileInfo.Length));
                // 리스튜뷰에 아이템 추가
                NovelListView.Items.Add(newItem);

                novelFileInfos.Add(fileInfo.Name, fileInfo);
            }
            else
            {
                ListViewItem newItem = new ListViewItem((LogListview.Items.Count + 1).ToString());
                newItem.SubItems.Add(string.Format("[{0}] 소설을 중복추가 하였습니다. 무시처리 됩니다.", fileInfo.Name));
                LogListview.Items.Add(newItem);
                // 자동 스크롤
                LogListview.Items[LogListview.Items.Count - 1].EnsureVisible();
            }
        }

        private void NovelListView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
            }
        }

        private void NovelListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private string GetFileSize(double byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        /// <summary>
        /// 소설을 복사할 폴더를 선택가능한 다이얼로그 박스를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            targetDir.Text = dialog.SelectedPath;
            // 타겟폴더를 저장합니다.
            WritePrivateProfileString("Folder", "SelectedPath", targetDir.Text, "./Parameter.ini");
        }

        private void AutoCopyStart_Click(object sender, EventArgs e)
        {
            // 넣을 소설이 없거나, 타겟 디렉토리가 없다면 무시한다.
            if (novelFileInfos.Count <= 0)
            {
                MessageBox.Show("자동 복사할 파일목록이 없습니다.");
                return;
            }

            if (string.IsNullOrEmpty(targetDir.Text))
            {
                MessageBox.Show("타겟 폴더를 지정하지 않았습니다.");
                return;
            }

            try
            {
                if (working) { return; }

                // 타겟 디렉토리를 검색합니다.
                DirectoryInfo dirInfo = new DirectoryInfo(targetDir.Text);
                if (dirInfo.Exists)
                {
                    // 진행률의 최대값을 복사하려는 파일의 갯수로 지정
                    copyProgressBar.Maximum = novelFileInfos.Count;
                    // 복사 쓰레드 시작
                    copyThread = new Thread(new ThreadStart(ProgressCopy));
                    copyThread.Start();
                    working = true;
                }
                else
                {
                    MessageBox.Show("유효하지 않은 복사 디렉토리 입니다.", "자동 복사 에러");
                }
            }
            catch (Exception ex)
            {
                working = false;
                MessageBox.Show(ex.Message, "자동 복사 에러");
            }
        }

        /// <summary>
        /// 복사 진행률을 표시한다.
        /// </summary>
        /// <param name="value"></param>
        private void SetProgBar(int value)
        {
            if (copyProgressBar.InvokeRequired)
            {
                if (copyThread.IsAlive)
                {
                    // 콜백 생성
                    SetProgCallBack callback = new SetProgCallBack(SetProgBar);
                    Invoke(callback, new object[] { value });
                }
            }
            else
            {
                copyProgressBar.Value = value;
                progressText.Text = string.Format("{0} / {1} ({2}%)", value, novelFileInfos.Count,
                    (int)((float)value / (float)novelFileInfos.Count * 100f));
            }
        }

        /// <summary>
        /// 쓰레드로 파일복사 시작
        /// </summary>
        private void ProgressCopy()
        {
            // 타겟 디렉토리를 검색합니다.
            DirectoryInfo dirInfo = new DirectoryInfo(targetDir.Text);
            if (dirInfo.Exists)
            {
                int currentCopyCount = 1;
                foreach (var file in novelFileInfos.Values)
                {
                    // 파일확장자를 제외한 파일이름만 추출
                    string fileName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                    int lastIndex = fileName.LastIndexOf(" ");
                    // 예외처리
                    if (lastIndex == -1)
                    {
                        currentCopyCount++;
                        continue;
                    }

                    fileName = fileName.Substring(0, lastIndex);
                    // 파일이름의 디렉토리 검색
                    DirectoryInfo searchDir = new DirectoryInfo(string.Format("{0}\\{1}", dirInfo.FullName, fileName));
                    if (searchDir.Exists == false)
                    {
                        // 폴더 생성
                        searchDir.Create();
                    }

                    string copyFile = string.Format("{0}\\{1}", searchDir.FullName, file.Name);
                    try
                    {
                        // 소설을 타겟폴더로 복사합니다.
                        File.Copy(file.FullName, copyFile, true);
                        File.SetAttributes(copyFile, FileAttributes.Normal);
                    }
                    catch (Exception ex)
                    {
                        ListViewItem newItem = new ListViewItem((LogListview.Items.Count + 1).ToString());
                        newItem.SubItems.Add(ex.Message);
                        LogListview.Items.Add(newItem);
                        // 자동 스크롤
                        LogListview.Items[LogListview.Items.Count - 1].EnsureVisible();
                    }

                    // 진행률 표시
                    SetProgBar(currentCopyCount++);
                }

                working = false;
                MessageBox.Show(string.Format("모든 소설을 [{0}] 폴더내에 복사하였습니다.", dirInfo.FullName));
            }
        }

        /// <summary>
        /// 복사하려는 리스트 목록을 초기화 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            novelFileInfos.Clear();
            NovelListView.Items.Clear();
            LogListview.Items.Clear();

            progressText.Text = "0 / 0 (0%)";
        }

        /// <summary>
        /// 리스트뷰에서 마우스 우클릭으로 아이템 제거처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelListView_MouseClick(object sender, MouseEventArgs e)
        {
            // 마우스 오른쪽 클릭
            if (e.Button.Equals(MouseButtons.Right))
            {
                // 메뉴 생성
                ContextMenu menu = new ContextMenu();
                MenuItem removeItem = new MenuItem("삭제", (senders, es) =>
                    {
                        if (NovelListView.SelectedItems.Count > 0)
                        {
                            int count = NovelListView.SelectedItems.Count;
                            for (int i = 0; i < count; ++i)
                            {
                                ListViewItem item = NovelListView.SelectedItems[0];
                                novelFileInfos.Remove(item.SubItems[1].Text);
                                NovelListView.SelectedItems[0].Remove();
                            }
                        }
                    }
                );

                menu.MenuItems.Add(removeItem);
                menu.Show(NovelListView, new Point(e.X, e.Y));
            }
        }

        private void AutoNovelMover_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (copyThread != null && copyThread.IsAlive)
            {
                copyThread.Abort();
            }

            // 타겟폴더를 저장합니다.
            WritePrivateProfileString("Folder", "SelectedPath", targetDir.Text, "./Parameter.ini");
        }
    }
}
