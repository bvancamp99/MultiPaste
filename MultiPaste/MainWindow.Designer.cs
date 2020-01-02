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
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.winStartupItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wrapKeysItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToCopiedItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeTopBottomItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.colorThemeBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultConfigItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispDropDown = new System.Windows.Forms.ToolStripMenuItem();
            this.showTextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showImagesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAudioItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCustomItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileDropDown,
            this.configDropDown,
            this.optionsToolStripMenuItem});
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
            this.toolStripSeparator2,
            this.exitItem});
            this.fileDropDown.Name = "fileDropDown";
            this.fileDropDown.Size = new System.Drawing.Size(54, 29);
            this.fileDropDown.Text = "File";
            this.fileDropDown.DropDownClosed += new System.EventHandler(this.RootToolStripMenuItem_DropDownClosed);
            this.fileDropDown.DropDownOpening += new System.EventHandler(this.RootToolStripMenuItem_DropDownOpening);
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
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(286, 6);
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
            this.winStartupItem,
            this.wrapKeysItem,
            this.moveToCopiedItem,
            this.changeTopBottomItem,
            this.toolStripSeparator1,
            this.colorThemeBox,
            this.toolStripSeparator3,
            this.defaultConfigItem});
            this.configDropDown.Name = "configDropDown";
            this.configDropDown.Size = new System.Drawing.Size(81, 29);
            this.configDropDown.Text = "Config";
            this.configDropDown.DropDownClosed += new System.EventHandler(this.RootToolStripMenuItem_DropDownClosed);
            this.configDropDown.DropDownOpening += new System.EventHandler(this.RootToolStripMenuItem_DropDownOpening);
            // 
            // winStartupItem
            // 
            this.winStartupItem.CheckOnClick = true;
            this.winStartupItem.Name = "winStartupItem";
            this.winStartupItem.Size = new System.Drawing.Size(499, 34);
            this.winStartupItem.Text = "Run on Windows Startup";
            this.winStartupItem.Click += new System.EventHandler(this.WinStartupItem_Click);
            // 
            // wrapKeysItem
            // 
            this.wrapKeysItem.CheckOnClick = true;
            this.wrapKeysItem.Name = "wrapKeysItem";
            this.wrapKeysItem.Size = new System.Drawing.Size(499, 34);
            this.wrapKeysItem.Text = "Wrap Up/Down Arrow Keys";
            this.wrapKeysItem.Click += new System.EventHandler(this.WrapKeysItem_Click);
            // 
            // moveToCopiedItem
            // 
            this.moveToCopiedItem.CheckOnClick = true;
            this.moveToCopiedItem.Name = "moveToCopiedItem";
            this.moveToCopiedItem.Size = new System.Drawing.Size(499, 34);
            this.moveToCopiedItem.Text = "Move Index to Copied Item";
            this.moveToCopiedItem.Click += new System.EventHandler(this.MoveToCopiedItem_Click);
            // 
            // changeTopBottomItem
            // 
            this.changeTopBottomItem.CheckOnClick = true;
            this.changeTopBottomItem.Name = "changeTopBottomItem";
            this.changeTopBottomItem.Size = new System.Drawing.Size(499, 34);
            this.changeTopBottomItem.Text = "Change Index when Moving Item to Top/Bottom";
            this.changeTopBottomItem.Click += new System.EventHandler(this.ChangeTopBottomItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(496, 6);
            // 
            // colorThemeBox
            // 
            this.colorThemeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorThemeBox.Items.AddRange(new object[] {
            "Light",
            "Dark"});
            this.colorThemeBox.Name = "colorThemeBox";
            this.colorThemeBox.Size = new System.Drawing.Size(121, 33);
            this.colorThemeBox.ToolTipText = "Select color theme";
            this.colorThemeBox.DropDownClosed += new System.EventHandler(this.ColorThemeBox_DropDownClosed);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(496, 6);
            // 
            // defaultConfigItem
            // 
            this.defaultConfigItem.Name = "defaultConfigItem";
            this.defaultConfigItem.Size = new System.Drawing.Size(499, 34);
            this.defaultConfigItem.Text = "Set All to Default Settings";
            this.defaultConfigItem.Click += new System.EventHandler(this.DefaultConfigItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dispDropDown});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(142, 29);
            this.optionsToolStripMenuItem.Text = "Other Options";
            this.optionsToolStripMenuItem.DropDownClosed += new System.EventHandler(this.RootToolStripMenuItem_DropDownClosed);
            this.optionsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.RootToolStripMenuItem_DropDownOpening);
            // 
            // dispDropDown
            // 
            this.dispDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTextItem,
            this.showFilesItem,
            this.showImagesItem,
            this.showAudioItem,
            this.showCustomItem});
            this.dispDropDown.Name = "dispDropDown";
            this.dispDropDown.Size = new System.Drawing.Size(342, 34);
            this.dispDropDown.Text = "Which type of data to show?";
            // 
            // showTextItem
            // 
            this.showTextItem.Checked = true;
            this.showTextItem.CheckOnClick = true;
            this.showTextItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showTextItem.Name = "showTextItem";
            this.showTextItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.showTextItem.Size = new System.Drawing.Size(207, 34);
            this.showTextItem.Text = "Text";
            this.showTextItem.Click += new System.EventHandler(this.ShowTextItem_Click);
            // 
            // showFilesItem
            // 
            this.showFilesItem.Checked = true;
            this.showFilesItem.CheckOnClick = true;
            this.showFilesItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showFilesItem.Name = "showFilesItem";
            this.showFilesItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.showFilesItem.Size = new System.Drawing.Size(207, 34);
            this.showFilesItem.Text = "Files";
            this.showFilesItem.Click += new System.EventHandler(this.ShowFilesItem_Click);
            // 
            // showImagesItem
            // 
            this.showImagesItem.Checked = true;
            this.showImagesItem.CheckOnClick = true;
            this.showImagesItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showImagesItem.Name = "showImagesItem";
            this.showImagesItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.showImagesItem.Size = new System.Drawing.Size(207, 34);
            this.showImagesItem.Text = "Images";
            this.showImagesItem.Click += new System.EventHandler(this.ShowImagesItem_Click);
            // 
            // showAudioItem
            // 
            this.showAudioItem.Checked = true;
            this.showAudioItem.CheckOnClick = true;
            this.showAudioItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showAudioItem.Name = "showAudioItem";
            this.showAudioItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.showAudioItem.Size = new System.Drawing.Size(207, 34);
            this.showAudioItem.Text = "Audio";
            this.showAudioItem.Click += new System.EventHandler(this.ShowAudioItem_Click);
            // 
            // showCustomItem
            // 
            this.showCustomItem.Checked = true;
            this.showCustomItem.CheckOnClick = true;
            this.showCustomItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCustomItem.Name = "showCustomItem";
            this.showCustomItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.showCustomItem.Size = new System.Drawing.Size(207, 34);
            this.showCustomItem.Text = "Custom";
            this.showCustomItem.Click += new System.EventHandler(this.ShowCustomItem_Click);
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
            this.removeBtn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.removeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removeBtn.Location = new System.Drawing.Point(436, 45);
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
        private System.Windows.Forms.ToolStripMenuItem helpItem;
        private System.Windows.Forms.ToolStripMenuItem wrapKeysItem;
        private System.Windows.Forms.ToolStripMenuItem moveToCopiedItem;
        private System.Windows.Forms.ToolStripMenuItem changeTopBottomItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dispDropDown;
        private System.Windows.Forms.ToolStripMenuItem showTextItem;
        private System.Windows.Forms.ToolStripMenuItem showFilesItem;
        private System.Windows.Forms.ToolStripMenuItem showImagesItem;
        private System.Windows.Forms.ToolStripMenuItem showAudioItem;
        private System.Windows.Forms.ToolStripMenuItem showCustomItem;
        private System.Windows.Forms.ToolStripMenuItem defaultConfigItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripComboBox colorThemeBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

