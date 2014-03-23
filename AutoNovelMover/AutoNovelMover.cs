using System;
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
using System.Reflection;
using System.Net;

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
        private List<string> targetDirList = new List<string>();

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
            copyProgressBar.Value = 0;
            // 복사 타겟폴더를 읽어옵니다.
            StringBuilder tmpRetVal = new StringBuilder(2000);
            GetPrivateProfileString("Folder", "SelectedPath", "", tmpRetVal, 2000, "./Parameter.ini");
            targetDir.Text = tmpRetVal.ToString();
            // 복사 / 이동 옵션 체크
            GetPrivateProfileString("Option", "CopyOption", "", tmpRetVal, 2000, "./Parameter.ini");
            CopyRadioBtn.Checked = tmpRetVal.ToString().Equals("COPY") || string.IsNullOrEmpty(tmpRetVal.ToString());
            MoveRadioBtn.Checked = tmpRetVal.ToString().Equals("MOVE");
            // 제목 갱신
            RefreshFormTitle();
            // 버전 체크
            CheckVersion();
        }

        /// <summary>
        /// 버전을 체크하고 최신버전을 갱신합니다.
        /// 버전체크는 GitHub에서 받아서 갱신하며, 최신파일은 임의의 장소에 올려서 받습니다.
        /// </summary>
        private void CheckVersion()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(VersionFileDownCompleted);
            // 버전은 GitHub에서 받아온다.
            webClient.DownloadFileAsync(new Uri("https://raw.github.com/dbswhdrbs/AutoNovelMover/master/Version.txt"), @"./Version.txt");
        }

        private void VersionFileDownCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                // 버전은 GitHub에서 받아온다.
                System.IO.StreamReader file = new StreamReader(@"./Version.txt", Encoding.Default);
                if (file == null) { return; }

                string version = file.ReadLine();
                string udpateWeb = file.ReadLine();
                string udpateList = file.ReadToEnd();

                Version checkVersion = new Version(version);

                if (Assembly.GetEntryAssembly().GetName().Version < checkVersion && string.IsNullOrEmpty(udpateWeb) == false)
                {
                    if (MessageBox.Show(string.Format("현재 버전과 웹에 등록된 버전이 다릅니다.\n현재 버전 : {0}, 체크된 버전 : {1}\n새버전을 다운받으시겠습니까?\n(브라우저 다운로드폴더에 받아집니다.)",
                        Assembly.GetEntryAssembly().GetName().Version.ToString(), checkVersion.ToString()), "버전 체크",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == System.Windows.Forms.DialogResult.Yes)
                    {
                        // TODO: 저장장소를 못구해서, 버전올릴때마다 따로 올려주고 주소를 갱신해줘야한다.
                        System.Diagnostics.Process.Start(udpateWeb);
                        MessageBox.Show(udpateList);
                    }
                }

                file.Close();
                // 버전체크하고 버전파일은 지운다.
                System.IO.File.Delete(@"./Version.txt");
            }
            catch (Exception except)
            {
                MessageBox.Show(except.ToString());
            }
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
                                foreach (var file in Directory.GetFiles(fileName))
                                {
                                    AddNovelItem(file);
                                }

                                continue;
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
                    RefreshNovelNumber();
                    // 자동 스크롤
                    NovelListView.Items[NovelListView.Items.Count - 1].EnsureVisible();
                    // 진행률에 현재 추가된 소설 수 갱신
                    RefreshFormTitle();
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
                string convertFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                int lastIndex = convertFileName.LastIndexOf(" ");
                // 공백이 검색되지 않았을경우 파일명 그대로 넣는다.
                if (lastIndex != -1)
                {
                    convertFileName = convertFileName.Substring(0, lastIndex);
                }
                // 복사될 폴더이름
                newItem.SubItems.Add(convertFileName);
                newItem.SubItems.Add(GetFileSize(fileInfo.Length));
                // 리스튜뷰에 아이템 추가
                NovelListView.Items.Add(newItem);

                novelFileInfos.Add(fileInfo.Name, fileInfo);
            }
            else
            {
                ListViewItem newItem = new ListViewItem((LogListview.Items.Count + 1).ToString());
                newItem.SubItems.Add(string.Format("[{0}] 파일을 중복추가 하였습니다. 무시처리 됩니다.", fileInfo.Name));
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

        /// <summary>
        /// byte를 실 용량사이즈로 변환합니다.
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
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
                    targetDirList.Clear();
                    for (int i = 0; i < NovelListView.Items.Count; ++i)
                    {
                        targetDirList.Add(NovelListView.Items[i].SubItems[2].Text);
                    }
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
                RefreshFormTitle();
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
                if (NovelListView.Items.Count != novelFileInfos.Values.Count)
                {
                    working = false;
                    MessageBox.Show("파일 요소의 구성정보가 잘못되었습니다.");
                    return;
                }

                foreach (var file in novelFileInfos.Values)
                {
                    // 파일이름의 디렉토리 검색
                    DirectoryInfo searchDir = new DirectoryInfo(string.Format("{0}\\{1}", dirInfo.FullName, targetDirList[currentCopyCount - 1]));
                    if (searchDir.Exists == false)
                    {
                        // 폴더 생성
                        searchDir.Create();
                    }

                    string copyFile = string.Format("{0}\\{1}", searchDir.FullName, file.Name);
                    try
                    {
                        // 소설을 타겟폴더로 복사합니다.
                        if (CopyRadioBtn.Checked)
                        {
                            File.Copy(file.FullName, copyFile, true);
                        }
                        else
                        {
                            File.Move(file.FullName, copyFile);
                        }
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
                string result = string.Format("모든 파일을 [{0}] 폴더내에 {1}하였습니다.", dirInfo.FullName, CopyRadioBtn.Checked ? "복사" : "이동");
                MessageBox.Show(result);
            }
        }

        /// <summary>
        /// 복사하려는 리스트 목록을 초기화 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            if (working) { return; }

            novelFileInfos.Clear();
            NovelListView.Items.Clear();
            LogListview.Items.Clear();

            RefreshFormTitle();
        }

        /// <summary>
        /// 리스트뷰에서 마우스 우클릭으로 아이템 제거처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (working) { return; }

            // 마우스 오른쪽 클릭
            if (e.Button.Equals(MouseButtons.Right))
            {
                // 메뉴 생성
                ContextMenu menu = new ContextMenu();
                // 특정 요소의 복사할 폴더수정
                MenuItem changeDir = new MenuItem("폴더 수정", (senders, es) =>
                {
                    if (NovelListView.SelectedItems.Count > 0)
                    {
                        string tmpChangeName = NovelListView.SelectedItems[0].SubItems[2].Text;
                        if (InputBox.Show("폴더 수정", "수정할 폴더명을 입력해 주세요.", ref tmpChangeName) == System.Windows.Forms.DialogResult.OK)
                        {
                            int count = NovelListView.SelectedItems.Count;
                            for (int i = 0; i < count; ++i)
                            {
                                ListViewItem changeItem = NovelListView.SelectedItems[i];
                                changeItem.SubItems[2].Text = tmpChangeName;
                            }
                        }
                    }
                }
                );
                menu.MenuItems.Add(changeDir);

                // 특정 요소를 삭제
                MenuItem removeItem = new MenuItem("파일 삭제", (senders, es) =>
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

                            RefreshNovelNumber();
                            RefreshFormTitle();
                            // 진행률에 현재 추가된 소설 수 갱신
                            RefreshFormTitle();
                        }
                    }
                );
                menu.MenuItems.Add(removeItem);
                // 메뉴 생성
                menu.Show(NovelListView, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// 소설 리스트뷰의 번호를 갱신합니다.
        /// </summary>
        void RefreshNovelNumber()
        {
            for (int i = 0; i < NovelListView.Items.Count; ++i)
            {
                NovelListView.Items[i].SubItems[0].Text = (i + 1).ToString();
            }
        }

        /// <summary>
        /// 폼 타이틀에 정보를 갱신합니다.
        /// </summary>
        void RefreshFormTitle()
        {
            long totalLength = novelFileInfos.Sum(x => x.Value.Length);
            int novelProgressCounter = novelFileInfos.Count > 0 ? (int)((float)copyProgressBar.Value / (float)novelFileInfos.Count * 100f) : 0;
            Text = string.Format("AutoNovelMover {0} - Files : {1} / {2} ({3}%), Size : {4}", Assembly.GetEntryAssembly().GetName().Version.ToString(),
                copyProgressBar.Value, novelFileInfos.Count, novelProgressCounter, GetFileSize(totalLength));
        }

        /// <summary>
        /// 프로그램이 종료될때, 복사 스레드가 돌고있다면, 종료처리하고 타켓폴더를 저장합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoNovelMover_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (copyThread != null && copyThread.IsAlive)
            {
                copyThread.Abort();
            }

            // 타겟폴더를 저장합니다.
            WritePrivateProfileString("Folder", "SelectedPath", targetDir.Text, "./Parameter.ini");
            // 복사/이동 옵션
            string copyOption = CopyRadioBtn.Checked ? "COPY" : "MOVE";
            WritePrivateProfileString("Option", "CopyOption", copyOption, "./Parameter.ini");
        }

        /// <summary>
        /// 소설리스트를 CTRL + A로 전체선택 가능하도록 지원
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                NovelListView.MultiSelect = true;
                foreach (ListViewItem item in NovelListView.Items)
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        /// 복사 라디오 버튼 상태 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (CopyRadioBtn.Checked)
            {
                AutoCopyStart.Text = "자동 복사 시작";
            }
        }

        /// <summary>
        /// 이동 라디오 버튼 상태 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (MoveRadioBtn.Checked)
            {
                AutoCopyStart.Text = "자동 이동 시작";
            }
        }
    }
}
