using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    class Config
    {
        private readonly MainWindow mainWindow; // store MainWindow instance to access its variables

        //private static ToolStripMenuItem winStartup; // whether or not MultiPaste runs on Windows startup
        //private static ToolStripMenuItem showText; // display/hide text items
        //private static ToolStripMenuItem showFiles; // display/hide file items
        //private static ToolStripMenuItem showImages; // display/hide image items
        //private static ToolStripMenuItem showAudio; // display/hide audio items
        //private static ToolStripMenuItem showCustom; // display/hide custom items

        //private static string configFile; // config file directory

        //public Config(MainWindow mainWindow)
        //{
        //    this.mainWindow.WinStartup = mainWindow.GetWinStartup();
        //    Config.showText = mainWindow.GetShowText();
        //    Config.showFiles = mainWindow.GetShowFiles();
        //    Config.showImages = mainWindow.GetShowImages();
        //    Config.showAudio = mainWindow.GetShowAudio();
        //    Config.showCustom = mainWindow.GetShowCustom();

        //    this.ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG");

        //    // read from CONFIG file and update items accordingly
        //    this.FromFile();
        //}

        public Config(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            this.ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG");
        }

        /// <summary>
        /// config file directory
        /// </summary>
        public string ConfigFile { get; }

        //public static string GetConfigFile()
        //{
        //    return this.ConfigFile;
        //}

        public void UpdateFile()
        {
            // init FileStream to write to file
            FileStream fileStream = new FileStream(this.ConfigFile, FileMode.Create);

            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (fileStream)
            {
                // write applicable bools to file
                fileStream.WriteByte(Convert.ToByte(this.mainWindow.WinStartup.Checked));
            }
        }

        //public void RestrictTypes()
        //{
        //    // first, clear the visual part of the local clipboard
        //    ListBox myListBox = this.mainWindow.ListBox;
        //    myListBox.Items.Clear();

        //    // add each item back to the listbox if its type is allowed
        //    bool allowType;
        //    Dictionary<string, ClipboardItem> myDict = this.mainWindow.Clipboard.Dict;
        //    StringCollection myKeys = this.mainWindow.Clipboard.Keys;
        //    foreach (string key in myKeys)
        //    {
        //        allowType = (this.mainWindow.ShowText.Checked && myDict[key].Type == ClipboardItem.TypeEnum.Text)
        //            || (this.mainWindow.ShowFiles.Checked && myDict[key].Type == ClipboardItem.TypeEnum.FileDropList)
        //            || (this.mainWindow.ShowImages.Checked && myDict[key].Type == ClipboardItem.TypeEnum.Image)
        //            || (this.mainWindow.ShowAudio.Checked && myDict[key].Type == ClipboardItem.TypeEnum.Audio)
        //            || (this.mainWindow.ShowCustom.Checked && myDict[key].Type == ClipboardItem.TypeEnum.Custom);

        //        // if type is allowed, add to the listbox
        //        if (allowType)
        //            myListBox.Items.Add(key);
        //    }
        //}

        public void WinStartupRegistry()
        {
            // establish the registry key for Windows startup; bool set to true to allow write access
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // add to or delete from Windows startup processes depending on bool
            if (this.mainWindow.WinStartup.Checked)
                registryKey.SetValue("MultiPaste", System.Reflection.Assembly.GetEntryAssembly().Location);
            else
                registryKey.DeleteValue("MultiPaste", false);
        }

        public void FromFile()
        {
            // read from config file to get user config information
            FileStream fileStream = new FileStream(this.ConfigFile, FileMode.OpenOrCreate);
            using (fileStream)
            {
                // if length is 0, the file was probably removed or misplaced; set to default values
                if (fileStream.Length == 0)
                    this.mainWindow.WinStartup.Checked = true;
                // else read bytes and assign to the appropriate properties
                else
                    this.mainWindow.WinStartup.Checked = Convert.ToBoolean((byte)fileStream.ReadByte());
            }

            // update registry for winStartup
            this.WinStartupRegistry();
        }
    }
}
