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
        public MainWindow()
        {
            // required method for Designer support
            this.InitializeComponent();

            // init myConfig, used for user config information
            this.Config = new Config(this);

            // read from CONFIG file and update items accordingly
            this.Config.FromFile();

            // init myClipboard, the driver of the clipboard history function
            this.Clipboard = new LocalClipboard(this);

            // read from CLIPBOARD file and write to local clipboard
            this.Clipboard.FromFile();

            // init msgLabel, used for messaging the user
            this.MsgLabel = new MsgLabel(this);

            // initialize custom event hook that will handle clipboard changes and keyboard input
            this.EventHook = new GlobalEventHook(this);
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
        /// menu item that determines whether to start MultiPaste on Windows startup
        /// </summary>
        public ToolStripMenuItem WinStartup
        {
            get { return this.winStartupItem; }
        }
        
        /// <summary>
        /// menu item that determines whether to show/hide text items in the listbox
        /// </summary>
        public ToolStripMenuItem ShowText
        {
            get { return this.showTextItem; }
        }

        /// <summary>
        /// menu item that determines whether to show/hide file items in the listbox
        /// </summary>
        public ToolStripMenuItem ShowFiles
        {
            get { return this.showTextItem; }
        }

        /// <summary>
        /// menu item that determines whether to show/hide image items in the listbox
        /// </summary>
        public ToolStripMenuItem ShowImages
        {
            get { return this.showTextItem; }
        }

        /// <summary>
        /// menu item that determines whether to show/hide audio items in the listbox
        /// </summary>
        public ToolStripMenuItem ShowAudio
        {
            get { return this.showTextItem; }
        }

        /// <summary>
        /// menu item that determines whether to show/hide custom items in the listbox
        /// </summary>
        public ToolStripMenuItem ShowCustom
        {
            get { return this.showTextItem; }
        }

        /// <summary>
        /// controls user config information
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// the driver of the clipboard history function
        /// </summary>
        public LocalClipboard Clipboard { get; }

        /// <summary>
        /// custom label that messages the user via Label and Timer
        /// </summary>
        public MsgLabel MsgLabel { get; }

        /// <summary>
        /// WndProc event hook to detect clipboard changes and user input
        /// </summary>
        public GlobalEventHook EventHook { get; }
    }
}
