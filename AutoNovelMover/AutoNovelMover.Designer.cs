namespace AutoNovelMover
{
    partial class AutoNovelMover
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoNovelMover));
            this.NovelListView = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.targetDir = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.Button();
            this.AutoCopyStart = new System.Windows.Forms.Button();
            this.copyProgressBar = new System.Windows.Forms.ProgressBar();
            this.ClearBtn = new System.Windows.Forms.Button();
            this.LogListview = new System.Windows.Forms.ListView();
            this.progressTitle = new System.Windows.Forms.Label();
            this.optionGroup = new System.Windows.Forms.GroupBox();
            this.CopyRadioBtn = new System.Windows.Forms.RadioButton();
            this.MoveRadioBtn = new System.Windows.Forms.RadioButton();
            this.optionGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // NovelListView
            // 
            this.NovelListView.AllowDrop = true;
            this.NovelListView.Location = new System.Drawing.Point(13, 41);
            this.NovelListView.Name = "NovelListView";
            this.NovelListView.Size = new System.Drawing.Size(466, 234);
            this.NovelListView.TabIndex = 0;
            this.NovelListView.UseCompatibleStateImageBehavior = false;
            this.NovelListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.NovelListView_DragDrop);
            this.NovelListView.DragOver += new System.Windows.Forms.DragEventHandler(this.NovelListView_DragOver);
            this.NovelListView.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.NovelListView_QueryContinueDrag);
            this.NovelListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NovelListView_KeyDown);
            this.NovelListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NovelListView_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "복사 디렉토리 경로 :";
            // 
            // targetDir
            // 
            this.targetDir.Location = new System.Drawing.Point(135, 12);
            this.targetDir.Name = "targetDir";
            this.targetDir.Size = new System.Drawing.Size(248, 21);
            this.targetDir.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Location = new System.Drawing.Point(389, 11);
            this.openFileDialog.Name = "openFileDialog";
            this.openFileDialog.Size = new System.Drawing.Size(89, 23);
            this.openFileDialog.TabIndex = 3;
            this.openFileDialog.Text = "폴더 찾기";
            this.openFileDialog.UseVisualStyleBackColor = true;
            this.openFileDialog.Click += new System.EventHandler(this.openFileDialog_Click);
            // 
            // AutoCopyStart
            // 
            this.AutoCopyStart.Location = new System.Drawing.Point(249, 458);
            this.AutoCopyStart.Name = "AutoCopyStart";
            this.AutoCopyStart.Size = new System.Drawing.Size(230, 34);
            this.AutoCopyStart.TabIndex = 4;
            this.AutoCopyStart.Text = "자동 복사 시작";
            this.AutoCopyStart.UseVisualStyleBackColor = true;
            this.AutoCopyStart.Click += new System.EventHandler(this.AutoCopyStart_Click);
            // 
            // copyProgressBar
            // 
            this.copyProgressBar.Location = new System.Drawing.Point(82, 279);
            this.copyProgressBar.Name = "copyProgressBar";
            this.copyProgressBar.Size = new System.Drawing.Size(396, 22);
            this.copyProgressBar.TabIndex = 5;
            // 
            // ClearBtn
            // 
            this.ClearBtn.Location = new System.Drawing.Point(13, 458);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(230, 34);
            this.ClearBtn.TabIndex = 6;
            this.ClearBtn.Text = "목록 초기화";
            this.ClearBtn.UseVisualStyleBackColor = true;
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // LogListview
            // 
            this.LogListview.Location = new System.Drawing.Point(13, 307);
            this.LogListview.Name = "LogListview";
            this.LogListview.Size = new System.Drawing.Size(466, 98);
            this.LogListview.TabIndex = 7;
            this.LogListview.UseCompatibleStateImageBehavior = false;
            // 
            // progressTitle
            // 
            this.progressTitle.AutoSize = true;
            this.progressTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.progressTitle.Location = new System.Drawing.Point(11, 285);
            this.progressTitle.Name = "progressTitle";
            this.progressTitle.Size = new System.Drawing.Size(65, 12);
            this.progressTitle.TabIndex = 8;
            this.progressTitle.Text = "진행 사항 :";
            // 
            // optionGroup
            // 
            this.optionGroup.Controls.Add(this.MoveRadioBtn);
            this.optionGroup.Controls.Add(this.CopyRadioBtn);
            this.optionGroup.Location = new System.Drawing.Point(12, 409);
            this.optionGroup.Name = "optionGroup";
            this.optionGroup.Size = new System.Drawing.Size(466, 43);
            this.optionGroup.TabIndex = 9;
            this.optionGroup.TabStop = false;
            this.optionGroup.Text = "복사 / 이동 옵션";
            // 
            // CopyRadioBtn
            // 
            this.CopyRadioBtn.AutoSize = true;
            this.CopyRadioBtn.Checked = true;
            this.CopyRadioBtn.Location = new System.Drawing.Point(10, 20);
            this.CopyRadioBtn.Name = "CopyRadioBtn";
            this.CopyRadioBtn.Size = new System.Drawing.Size(75, 16);
            this.CopyRadioBtn.TabIndex = 0;
            this.CopyRadioBtn.TabStop = true;
            this.CopyRadioBtn.Text = "파일 복사";
            this.CopyRadioBtn.UseVisualStyleBackColor = true;
            this.CopyRadioBtn.CheckedChanged += new System.EventHandler(this.CopyRadioBtn_CheckedChanged);
            // 
            // MoveRadioBtn
            // 
            this.MoveRadioBtn.AutoSize = true;
            this.MoveRadioBtn.Location = new System.Drawing.Point(91, 20);
            this.MoveRadioBtn.Name = "MoveRadioBtn";
            this.MoveRadioBtn.Size = new System.Drawing.Size(75, 16);
            this.MoveRadioBtn.TabIndex = 1;
            this.MoveRadioBtn.Text = "파일 이동";
            this.MoveRadioBtn.UseVisualStyleBackColor = true;
            this.MoveRadioBtn.CheckedChanged += new System.EventHandler(this.MoveRadioBtn_CheckedChanged);
            // 
            // AutoNovelMover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(491, 500);
            this.Controls.Add(this.optionGroup);
            this.Controls.Add(this.progressTitle);
            this.Controls.Add(this.LogListview);
            this.Controls.Add(this.ClearBtn);
            this.Controls.Add(this.copyProgressBar);
            this.Controls.Add(this.AutoCopyStart);
            this.Controls.Add(this.openFileDialog);
            this.Controls.Add(this.targetDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NovelListView);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoNovelMover";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoNovelMover v1.4";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AutoNovelMover_FormClosing);
            this.Load += new System.EventHandler(this.AutoNovelMover_Load);
            this.optionGroup.ResumeLayout(false);
            this.optionGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView NovelListView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox targetDir;
        private System.Windows.Forms.Button openFileDialog;
        private System.Windows.Forms.Button AutoCopyStart;
        private System.Windows.Forms.ProgressBar copyProgressBar;
        private System.Windows.Forms.Button ClearBtn;
        private System.Windows.Forms.ListView LogListview;
        private System.Windows.Forms.Label progressTitle;
        private System.Windows.Forms.GroupBox optionGroup;
        private System.Windows.Forms.RadioButton MoveRadioBtn;
        private System.Windows.Forms.RadioButton CopyRadioBtn;
    }
}

