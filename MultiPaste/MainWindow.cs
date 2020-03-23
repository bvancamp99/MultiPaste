using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MultiPaste
{
    partial class MainWindow : Form
    {
        private const string SEARCH_DEFAULT = "Search for an item..."; // store searchTextBox's default text

        public MainWindow()
        {
            // required method for Designer support
            this.InitializeComponent();

            // hand this instance to the static classes that utilize MainWindow
            MsgLabel.MainWindow = this;
            Config.MainWindow = this;
            LocalClipboard.MainWindow = this;

            // read from CONFIG file and update items accordingly
            Config.FromFile();

            // read from CLIPBOARD file and write to local clipboard
            LocalClipboard.FromFile();

            // init WndProc event hook
            _ = new GlobalEventHook(this);

            wnd = new PopupWindow();
        }

        public PopupWindow wnd { get; }

        /// <summary>
        /// defines the possible color themes for MultiPaste
        /// </summary>
        public enum ColorTheme
        {
            Light,
            Dark
        }

        /// <summary>
        /// combobox that allows selecting the color theme for MultiPaste
        /// </summary>
        public ToolStripComboBox ColorThemeBox
        {
            get { return this.colorThemeBox; }
        }

        /// <summary>
        /// box that holds the visible clipboard items
        /// </summary>
        public ListBox ListBox
        {
            get { return this.listBox; }
        }

        /// <summary>
        /// label used to notify the user of program changes
        /// </summary>
        public Label Label
        {
            get { return this.notifLabel; }
        }

        /// <summary>
        /// menu item that determines whether to start MultiPaste minimized
        /// 
        /// default is unchecked
        /// </summary>
        public ToolStripMenuItem StartMinimized
        {
            get { return this.startMinimizedItem; }
        }

        /// <summary>
        /// menu item that determines whether to start MultiPaste on Windows startup
        /// 
        /// default is checked
        /// </summary>
        public ToolStripMenuItem WinStartup
        {
            get { return this.winStartupItem; }
        }

        /// <summary>
        /// menu item that determines whether to wrap to the opposite extreme 
        /// index when the up arrow key is pressed on top index or down arrow 
        /// is pressed on bottom index
        /// 
        /// default is unchecked
        /// </summary>
        public ToolStripMenuItem WrapKeys
        {
            get { return this.wrapKeysItem; }
        }

        /// <summary>
        /// menu item that determines whether to move the listbox's selected
        /// index to the that of the just-copied item, i.e. index 0
        /// 
        /// default is checked
        /// </summary>
        public ToolStripMenuItem MoveToCopied
        {
            get { return this.moveToCopiedItem; }
        }

        /// <summary>
        /// menu item that determines whether to move the listbox's selected 
        /// index to that of the item just moved to the top/bottom
        /// 
        /// default is checked
        /// </summary>
        public ToolStripMenuItem ChangeTopBottom
        {
            get { return this.changeTopBottomItem; }
        }

        ///// <summary>
        ///// menu item that determines whether to show/hide text items in the listbox
        ///// 
        ///// default is checked
        ///// </summary>
        //public ToolStripMenuItem ShowText
        //{
        //    get { return this.showTextItem; }
        //}

        ///// <summary>
        ///// menu item that determines whether to show/hide file items in the listbox
        ///// 
        ///// default is checked
        ///// </summary>
        //public ToolStripMenuItem ShowFiles
        //{
        //    get { return this.showFilesItem; }
        //}

        ///// <summary>
        ///// menu item that determines whether to show/hide image items in the listbox
        ///// 
        ///// default is checked
        ///// </summary>
        //public ToolStripMenuItem ShowImages
        //{
        //    get { return this.showImagesItem; }
        //}

        ///// <summary>
        ///// menu item that determines whether to show/hide audio items in the listbox
        ///// 
        ///// default is checked
        ///// </summary>
        //public ToolStripMenuItem ShowAudio
        //{
        //    get { return this.showAudioItem; }
        //}

        ///// <summary>
        ///// menu item that determines whether to show/hide custom items in the listbox
        ///// 
        ///// default is checked
        ///// </summary>
        //public ToolStripMenuItem ShowCustom
        //{
        //    get { return this.showCustomItem; }
        //}
    }
}
