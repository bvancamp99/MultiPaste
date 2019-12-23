namespace MultiPaste
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.helpItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.dispDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.dispTextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispImagesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispAudioItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispCustomItem = new System.Windows.Forms.ToolStripMenuItem();
            this.winStartupItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBox = new System.Windows.Forms.ListBox();
            this.removeBtn = new System.Windows.Forms.Button();
            this.notifLabel = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDown,
            this.configDropDown});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(609, 33);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileDropDown
            // 
            this.fileDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpItem,
            this.clearItem,
            this.exitItem});
            this.fileDropDown.Name = "fileDropDown";
            this.fileDropDown.Size = new System.Drawing.Size(54, 29);
            this.fileDropDown.Text = "File";
            // 
            // helpItem
            // 
            this.helpItem.Name = "helpItem";
            this.helpItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.helpItem.Size = new System.Drawing.Size(289, 34);
            this.helpItem.Text = "Help";
            this.helpItem.Click += new System.EventHandler(this.HelpItem_Click);
            // 
            // clearItem
            // 
            this.clearItem.Name = "clearItem";
            this.clearItem.Size = new System.Drawing.Size(289, 34);
            this.clearItem.Text = "Clear All Copied Items";
            this.clearItem.Click += new System.EventHandler(this.ClearItem_Click);
            // 
            // exitItem
            // 
            this.exitItem.Name = "exitItem";
            this.exitItem.ShortcutKeyDisplayString = "";
            this.exitItem.Size = new System.Drawing.Size(289, 34);
            this.exitItem.Text = "Exit";
            this.exitItem.Click += new System.EventHandler(this.ExitItem_Click);
            // 
            // configDropDown
            // 
            this.configDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dispDropDown,
            this.winStartupItem});
            this.configDropDown.Name = "configDropDown";
            this.configDropDown.Size = new System.Drawing.Size(81, 29);
            this.configDropDown.Text = "Config";
            // 
            // dispDropDown
            // 
            this.dispDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dispTextItem,
            this.dispFilesItem,
            this.dispImagesItem,
            this.dispAudioItem,
            this.dispCustomItem});
            this.dispDropDown.Name = "dispDropDown";
            this.dispDropDown.Size = new System.Drawing.Size(342, 34);
            this.dispDropDown.Text = "Which type of data to show?";
            // 
            // dispTextItem
            // 
            this.dispTextItem.Checked = true;
            this.dispTextItem.CheckOnClick = true;
            this.dispTextItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dispTextItem.Name = "dispTextItem";
            this.dispTextItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.dispTextItem.Size = new System.Drawing.Size(207, 34);
            this.dispTextItem.Text = "Text";
            this.dispTextItem.Click += new System.EventHandler(this.DispTextItem_Click);
            // 
            // dispFilesItem
            // 
            this.dispFilesItem.Checked = true;
            this.dispFilesItem.CheckOnClick = true;
            this.dispFilesItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dispFilesItem.Name = "dispFilesItem";
            this.dispFilesItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.dispFilesItem.Size = new System.Drawing.Size(207, 34);
            this.dispFilesItem.Text = "Files";
            this.dispFilesItem.Click += new System.EventHandler(this.DispFilesItem_Click);
            // 
            // dispImagesItem
            // 
            this.dispImagesItem.Checked = true;
            this.dispImagesItem.CheckOnClick = true;
            this.dispImagesItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dispImagesItem.Name = "dispImagesItem";
            this.dispImagesItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.dispImagesItem.Size = new System.Drawing.Size(207, 34);
            this.dispImagesItem.Text = "Images";
            this.dispImagesItem.Click += new System.EventHandler(this.DispImagesItem_Click);
            // 
            // dispAudioItem
            // 
            this.dispAudioItem.Checked = true;
            this.dispAudioItem.CheckOnClick = true;
            this.dispAudioItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dispAudioItem.Name = "dispAudioItem";
            this.dispAudioItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.dispAudioItem.Size = new System.Drawing.Size(207, 34);
            this.dispAudioItem.Text = "Audio";
            this.dispAudioItem.Click += new System.EventHandler(this.DispAudioItem_Click);
            // 
            // dispCustomItem
            // 
            this.dispCustomItem.Checked = true;
            this.dispCustomItem.CheckOnClick = true;
            this.dispCustomItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dispCustomItem.Name = "dispCustomItem";
            this.dispCustomItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.dispCustomItem.Size = new System.Drawing.Size(207, 34);
            this.dispCustomItem.Text = "Custom";
            this.dispCustomItem.Click += new System.EventHandler(this.DispCustomItem_Click);
            // 
            // winStartupItem
            // 
            this.winStartupItem.CheckOnClick = true;
            this.winStartupItem.Name = "winStartupItem";
            this.winStartupItem.Size = new System.Drawing.Size(342, 34);
            this.winStartupItem.Text = "Run on Windows Startup";
            this.winStartupItem.Click += new System.EventHandler(this.WinStartupItem_Click);
            // 
            // listBox
            // 
            this.listBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox.FormattingEnabled = true;
            this.listBox.HorizontalScrollbar = true;
            this.listBox.ItemHeight = 28;
            this.listBox.Location = new System.Drawing.Point(18, 106);
            this.listBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(571, 340);
            this.listBox.TabIndex = 1;
            this.listBox.DoubleClick += new System.EventHandler(this.ListBox_DoubleClick);
            // 
            // removeBtn
            // 
            this.removeBtn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.removeBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removeBtn.Location = new System.Drawing.Point(435, 42);
            this.removeBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(154, 51);
            this.removeBtn.TabIndex = 3;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = false;
            this.removeBtn.Click += new System.EventHandler(this.RemoveBtn_Click);
            // 
            // notifLabel
            // 
            this.notifLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.notifLabel.Location = new System.Drawing.Point(178, 55);
            this.notifLabel.Name = "notifLabel";
            this.notifLabel.Size = new System.Drawing.Size(249, 23);
            this.notifLabel.TabIndex = 4;
            this.notifLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "MultiPaste";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(122, 68);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(121, 32);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.HelpItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(121, 32);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitItem_Click);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 446);
            this.Controls.Add(this.notifLabel);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "MultiPaste";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainWindow_KeyDown);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileDropDown;
        private System.Windows.Forms.ToolStripMenuItem clearItem;
        private System.Windows.Forms.ToolStripMenuItem exitItem;
        private System.Windows.Forms.ToolStripMenuItem configDropDown;
        private System.Windows.Forms.ToolStripMenuItem winStartupItem;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Button removeBtn;
        private System.Windows.Forms.Label notifLabel;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dispDropDown;
        private System.Windows.Forms.ToolStripMenuItem dispTextItem;
        private System.Windows.Forms.ToolStripMenuItem dispFilesItem;
        private System.Windows.Forms.ToolStripMenuItem dispImagesItem;
        private System.Windows.Forms.ToolStripMenuItem dispAudioItem;
        private System.Windows.Forms.ToolStripMenuItem dispCustomItem;
        private System.Windows.Forms.ToolStripMenuItem helpItem;
    }
}

