namespace Client.WinForms
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutMain = new TableLayoutPanel();
            panelTop = new Panel();
            groupBoxInput = new GroupBox();
            flowLayoutInput = new FlowLayoutPanel();
            btnAddFrame = new Button();
            btnAddFromFile = new Button();
            btnAddText = new Button();
            groupBoxProcess = new GroupBox();
            flowLayoutProcess = new FlowLayoutPanel();
            btnFormText = new Button();
            btnFormImage = new Button();
            groupBoxSettings = new GroupBox();
            flowLayoutSettings = new FlowLayoutPanel();
            btnToggleEdit = new Button();
            chkNormalize = new CheckBox();
            groupBoxExport = new GroupBox();
            flowLayoutExport = new FlowLayoutPanel();
            btnSaveTxt = new Button();
            btnSavePng = new Button();
            btnOpenFolder = new Button();
            tabControl = new TabControl();
            tabText = new TabPage();
            richText = new RichTextBox();
            tabImage = new TabPage();
            panelImageScroll = new Panel();
            picturePreview = new PictureBox();
            panelScreenshots = new Panel();
            statusBar = new StatusStrip();
            statusIconLabel = new ToolStripStatusLabel();
            statusLabel = new ToolStripStatusLabel();
            tableLayoutMain.SuspendLayout();
            panelTop.SuspendLayout();
            groupBoxInput.SuspendLayout();
            flowLayoutInput.SuspendLayout();
            groupBoxProcess.SuspendLayout();
            flowLayoutProcess.SuspendLayout();
            groupBoxSettings.SuspendLayout();
            flowLayoutSettings.SuspendLayout();
            groupBoxExport.SuspendLayout();
            flowLayoutExport.SuspendLayout();
            tabControl.SuspendLayout();
            tabText.SuspendLayout();
            tabImage.SuspendLayout();
            panelImageScroll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutMain
            // 
            tableLayoutMain.ColumnCount = 2;
            tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutMain.Controls.Add(panelTop, 0, 0);
            tableLayoutMain.Controls.Add(tabControl, 0, 1);
            tableLayoutMain.Controls.Add(panelScreenshots, 1, 1);
            tableLayoutMain.Controls.Add(statusBar, 0, 2);
            tableLayoutMain.Dock = DockStyle.Fill;
            tableLayoutMain.Location = new Point(0, 0);
            tableLayoutMain.Name = "tableLayoutMain";
            tableLayoutMain.RowCount = 3;
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutMain.Size = new Size(1350, 600);
            tableLayoutMain.TabIndex = 0;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.WhiteSmoke;
            tableLayoutMain.SetColumnSpan(panelTop, 2);
            panelTop.Controls.Add(groupBoxInput);
            panelTop.Controls.Add(groupBoxProcess);
            panelTop.Controls.Add(groupBoxSettings);
            panelTop.Controls.Add(groupBoxExport);
            panelTop.Dock = DockStyle.Fill;
            panelTop.Location = new Point(3, 3);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(8);
            panelTop.Size = new Size(1344, 79);
            panelTop.TabIndex = 0;
            // 
            // groupBoxInput
            // 
            groupBoxInput.Controls.Add(flowLayoutInput);
            groupBoxInput.Location = new Point(12, 8);
            groupBoxInput.Name = "groupBoxInput";
            groupBoxInput.Size = new Size(349, 60);
            groupBoxInput.TabIndex = 0;
            groupBoxInput.TabStop = false;
            groupBoxInput.Text = "Входные данные";
            // 
            // flowLayoutInput
            // 
            flowLayoutInput.Controls.Add(btnAddFrame);
            flowLayoutInput.Controls.Add(btnAddFromFile);
            flowLayoutInput.Controls.Add(btnAddText);
            flowLayoutInput.Dock = DockStyle.Fill;
            flowLayoutInput.Location = new Point(3, 19);
            flowLayoutInput.Name = "flowLayoutInput";
            flowLayoutInput.Padding = new Padding(5, 0, 5, 0);
            flowLayoutInput.Size = new Size(343, 38);
            flowLayoutInput.TabIndex = 0;
            // 
            // btnAddFrame
            // 
            btnAddFrame.AutoSize = true;
            btnAddFrame.Location = new Point(8, 3);
            btnAddFrame.Name = "btnAddFrame";
            btnAddFrame.Size = new Size(88, 25);
            btnAddFrame.TabIndex = 0;
            btnAddFrame.Text = "Создать кадр";
            btnAddFrame.Click += btnCreateFrame_Click;
            // 
            // btnAddFromFile
            // 
            btnAddFromFile.AutoSize = true;
            btnAddFromFile.Location = new Point(102, 3);
            btnAddFromFile.Name = "btnAddFromFile";
            btnAddFromFile.Size = new Size(122, 25);
            btnAddFromFile.TabIndex = 1;
            btnAddFromFile.Text = "Добавить картинку";
            btnAddFromFile.Click += btnAddFromFile_Click;
            // 
            // btnAddText
            // 
            btnAddText.AutoSize = true;
            btnAddText.Location = new Point(230, 3);
            btnAddText.Name = "btnAddText";
            btnAddText.Size = new Size(100, 25);
            btnAddText.TabIndex = 2;
            btnAddText.Text = "Добавить текст";
            btnAddText.Click += btnAddText_Click;
            // 
            // groupBoxProcess
            // 
            groupBoxProcess.Controls.Add(flowLayoutProcess);
            groupBoxProcess.Location = new Point(364, 8);
            groupBoxProcess.Name = "groupBoxProcess";
            groupBoxProcess.Size = new Size(314, 60);
            groupBoxProcess.TabIndex = 1;
            groupBoxProcess.TabStop = false;
            groupBoxProcess.Text = "Обработка";
            // 
            // flowLayoutProcess
            // 
            flowLayoutProcess.Controls.Add(btnFormText);
            flowLayoutProcess.Controls.Add(btnFormImage);
            flowLayoutProcess.Dock = DockStyle.Fill;
            flowLayoutProcess.Location = new Point(3, 19);
            flowLayoutProcess.Name = "flowLayoutProcess";
            flowLayoutProcess.Padding = new Padding(5, 0, 5, 0);
            flowLayoutProcess.Size = new Size(308, 38);
            flowLayoutProcess.TabIndex = 0;
            // 
            // btnFormText
            // 
            btnFormText.AutoSize = true;
            btnFormText.Location = new Point(8, 3);
            btnFormText.Name = "btnFormText";
            btnFormText.Size = new Size(132, 25);
            btnFormText.TabIndex = 0;
            btnFormText.Text = "Сформировать текст";
            btnFormText.Click += btnFormText_Click;
            // 
            // btnFormImage
            // 
            btnFormImage.AutoSize = true;
            btnFormImage.Location = new Point(146, 3);
            btnFormImage.Name = "btnFormImage";
            btnFormImage.Size = new Size(154, 25);
            btnFormImage.TabIndex = 1;
            btnFormImage.Text = "Сформировать картинку";
            btnFormImage.Click += btnFormImage_Click;
            // 
            // groupBoxSettings
            // 
            groupBoxSettings.Controls.Add(flowLayoutSettings);
            groupBoxSettings.Location = new Point(1024, 8);
            groupBoxSettings.Name = "groupBoxSettings";
            groupBoxSettings.Size = new Size(294, 60);
            groupBoxSettings.TabIndex = 2;
            groupBoxSettings.TabStop = false;
            groupBoxSettings.Text = "Настройки";
            // 
            // flowLayoutSettings
            // 
            flowLayoutSettings.Controls.Add(btnToggleEdit);
            flowLayoutSettings.Controls.Add(chkNormalize);
            flowLayoutSettings.Dock = DockStyle.Fill;
            flowLayoutSettings.Location = new Point(3, 19);
            flowLayoutSettings.Name = "flowLayoutSettings";
            flowLayoutSettings.Padding = new Padding(5, 0, 5, 0);
            flowLayoutSettings.Size = new Size(288, 38);
            flowLayoutSettings.TabIndex = 0;
            // 
            // btnToggleEdit
            // 
            btnToggleEdit.AutoSize = true;
            btnToggleEdit.Location = new Point(8, 3);
            btnToggleEdit.Name = "btnToggleEdit";
            btnToggleEdit.Size = new Size(119, 25);
            btnToggleEdit.TabIndex = 0;
            btnToggleEdit.Text = "Разрешить редакт.";
            btnToggleEdit.Click += btnToggleEdit_Click;
            // 
            // chkNormalize
            // 
            chkNormalize.AutoSize = true;
            chkNormalize.Checked = true;
            chkNormalize.CheckState = CheckState.Checked;
            chkNormalize.Location = new Point(133, 3);
            chkNormalize.Name = "chkNormalize";
            chkNormalize.Size = new Size(146, 19);
            chkNormalize.TabIndex = 1;
            chkNormalize.Text = "Нормализация текста";
            // 
            // groupBoxExport
            // 
            groupBoxExport.Controls.Add(flowLayoutExport);
            groupBoxExport.Location = new Point(681, 8);
            groupBoxExport.Name = "groupBoxExport";
            groupBoxExport.Size = new Size(337, 60);
            groupBoxExport.TabIndex = 3;
            groupBoxExport.TabStop = false;
            groupBoxExport.Text = "Сохранение и экспорт";
            // 
            // flowLayoutExport
            // 
            flowLayoutExport.Controls.Add(btnSaveTxt);
            flowLayoutExport.Controls.Add(btnSavePng);
            flowLayoutExport.Controls.Add(btnOpenFolder);
            flowLayoutExport.Dock = DockStyle.Fill;
            flowLayoutExport.Location = new Point(3, 19);
            flowLayoutExport.Name = "flowLayoutExport";
            flowLayoutExport.Padding = new Padding(5, 0, 5, 0);
            flowLayoutExport.Size = new Size(331, 38);
            flowLayoutExport.TabIndex = 0;
            // 
            // btnSaveTxt
            // 
            btnSaveTxt.AutoSize = true;
            btnSaveTxt.Location = new Point(8, 3);
            btnSaveTxt.Name = "btnSaveTxt";
            btnSaveTxt.Size = new Size(98, 25);
            btnSaveTxt.TabIndex = 0;
            btnSaveTxt.Text = "Сохранить TXT";
            btnSaveTxt.Click += btnSaveTxt_Click;
            // 
            // btnSavePng
            // 
            btnSavePng.AutoSize = true;
            btnSavePng.Location = new Point(112, 3);
            btnSavePng.Name = "btnSavePng";
            btnSavePng.Size = new Size(103, 25);
            btnSavePng.TabIndex = 1;
            btnSavePng.Text = "Сохранить PNG";
            btnSavePng.Click += btnSavePng_Click;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.AutoSize = true;
            btnOpenFolder.Location = new Point(221, 3);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(99, 25);
            btnOpenFolder.TabIndex = 2;
            btnOpenFolder.Text = "Открыть папку";
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabText);
            tabControl.Controls.Add(tabImage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(3, 88);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1144, 481);
            tabControl.TabIndex = 1;
            // 
            // tabText
            // 
            tabText.Controls.Add(richText);
            tabText.Location = new Point(4, 24);
            tabText.Name = "tabText";
            tabText.Padding = new Padding(3);
            tabText.Size = new Size(1136, 453);
            tabText.TabIndex = 0;
            tabText.Text = "Текст";
            tabText.UseVisualStyleBackColor = true;
            // 
            // richText
            // 
            richText.BorderStyle = BorderStyle.None;
            richText.Dock = DockStyle.Fill;
            richText.Location = new Point(3, 3);
            richText.Name = "richText";
            richText.ReadOnly = true;
            richText.Size = new Size(1130, 447);
            richText.TabIndex = 0;
            richText.Text = "";
            // 
            // tabImage
            // 
            tabImage.Controls.Add(panelImageScroll);
            tabImage.Location = new Point(4, 24);
            tabImage.Name = "tabImage";
            tabImage.Padding = new Padding(3);
            tabImage.Size = new Size(1136, 453);
            tabImage.TabIndex = 1;
            tabImage.Text = "Картинка";
            tabImage.UseVisualStyleBackColor = true;
            // 
            // panelImageScroll
            // 
            panelImageScroll.AutoScroll = true;
            panelImageScroll.BackColor = Color.DimGray;
            panelImageScroll.Controls.Add(picturePreview);
            panelImageScroll.Dock = DockStyle.Fill;
            panelImageScroll.Location = new Point(3, 3);
            panelImageScroll.Name = "panelImageScroll";
            panelImageScroll.Size = new Size(1130, 447);
            panelImageScroll.TabIndex = 0;
            // 
            // picturePreview
            // 
            picturePreview.BackColor = Color.White;
            picturePreview.Location = new Point(20, 20);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new Size(100, 50);
            picturePreview.TabIndex = 0;
            picturePreview.TabStop = false;
            // 
            // panelScreenshots
            // 
            panelScreenshots.AutoScroll = true;
            panelScreenshots.BackColor = Color.LightGray;
            panelScreenshots.BorderStyle = BorderStyle.FixedSingle;
            panelScreenshots.Dock = DockStyle.Fill;
            panelScreenshots.Location = new Point(1153, 88);
            panelScreenshots.Name = "panelScreenshots";
            panelScreenshots.Padding = new Padding(10);
            panelScreenshots.Size = new Size(194, 481);
            panelScreenshots.TabIndex = 2;
            // 
            // statusBar
            // 
            tableLayoutMain.SetColumnSpan(statusBar, 2);
            statusBar.Items.AddRange(new ToolStripItem[] { statusIconLabel, statusLabel });
            statusBar.Location = new Point(0, 578);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(1350, 22);
            statusBar.TabIndex = 3;
            // 
            // statusIconLabel
            // 
            statusIconLabel.Margin = new Padding(5, 3, 0, 2);
            statusIconLabel.Name = "statusIconLabel";
            statusIconLabel.Size = new Size(0, 17);
            // 
            // statusLabel
            // 
            statusLabel.Margin = new Padding(3, 3, 5, 2);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1322, 17);
            statusLabel.Spring = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 600);
            Controls.Add(tableLayoutMain);
            Name = "FormMain";
            Text = "Frame Merger - Скриншоты";
            Load += FormMain_Load;
            tableLayoutMain.ResumeLayout(false);
            tableLayoutMain.PerformLayout();
            panelTop.ResumeLayout(false);
            groupBoxInput.ResumeLayout(false);
            flowLayoutInput.ResumeLayout(false);
            flowLayoutInput.PerformLayout();
            groupBoxProcess.ResumeLayout(false);
            flowLayoutProcess.ResumeLayout(false);
            flowLayoutProcess.PerformLayout();
            groupBoxSettings.ResumeLayout(false);
            flowLayoutSettings.ResumeLayout(false);
            flowLayoutSettings.PerformLayout();
            groupBoxExport.ResumeLayout(false);
            flowLayoutExport.ResumeLayout(false);
            flowLayoutExport.PerformLayout();
            tabControl.ResumeLayout(false);
            tabText.ResumeLayout(false);
            tabImage.ResumeLayout(false);
            panelImageScroll.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutMain;
        private Panel panelScreenshots;
        private Panel panelTop;
        private Button btnAddFrame;
        private Button btnAddFromFile;
        private Button btnAddText;
        private Button btnFormText;
        private Button btnFormImage;
        private Button btnSaveTxt;
        private Button btnSavePng;
        private Button btnOpenFolder;
        private Button btnToggleEdit;
        private CheckBox chkNormalize;
        private GroupBox groupBoxInput;
        private FlowLayoutPanel flowLayoutInput;
        private GroupBox groupBoxProcess;
        private FlowLayoutPanel flowLayoutProcess;
        private GroupBox groupBoxSettings;
        private FlowLayoutPanel flowLayoutSettings;
        private GroupBox groupBoxExport;
        private FlowLayoutPanel flowLayoutExport;
        private TabControl tabControl;
        private TabPage tabText;
        private TabPage tabImage;
        private RichTextBox richText;
        private Panel panelImageScroll;
        private PictureBox picturePreview;
        private StatusStrip statusBar;
        private ToolStripStatusLabel statusIconLabel;
        private ToolStripStatusLabel statusLabel;
    }
}
