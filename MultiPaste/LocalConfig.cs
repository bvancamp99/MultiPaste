using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    class LocalConfig
    {
        private static ToolStripMenuItem winStartup; // whether or not MultiPaste runs on Windows startup
        private static ToolStripMenuItem showText; // display/hide text items
        private static ToolStripMenuItem showFiles; // display/hide file items
        private static ToolStripMenuItem showImages; // display/hide image items
        private static ToolStripMenuItem showAudio; // display/hide audio items
        private static ToolStripMenuItem showCustom; // display/hide custom items

        private static string configFile; // config file directory

        public LocalConfig(MainWindow mainWindow)
        {
            LocalConfig.winStartup = mainWindow.GetWinStartup();
            LocalConfig.showText = mainWindow.GetShowText();
            LocalConfig.showFiles = mainWindow.GetShowFiles();
            LocalConfig.showImages = mainWindow.GetShowImages();
            LocalConfig.showAudio = mainWindow.GetShowAudio();
            LocalConfig.showCustom = mainWindow.GetShowCustom();

            LocalConfig.configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG");

            // read from CONFIG file and update items accordingly
            this.FromFile();
        }

        public static string GetConfigFile()
        {
            return LocalConfig.configFile;
        }

        public void UpdateFile()
        {
            // init FileStream to write to file
            FileStream fileStream = new FileStream(LocalConfig.configFile, FileMode.Create);

            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (fileStream)
            {
                // write applicable bools to file
                fileStream.WriteByte(Convert.ToByte(LocalConfig.winStartup.Checked));
            }
        }

        public void WinStartupRegistry()
        {
            // establish the registry key for Windows startup; bool set to true to allow write access
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // add to or delete from Windows startup processes depending on bool
            if (LocalConfig.winStartup.Checked)
                registryKey.SetValue("MultiPaste", System.Reflection.Assembly.GetEntryAssembly().Location);
            else
                registryKey.DeleteValue("MultiPaste", false);
        }

        private void FromFile()
        {
            // read from config file to get user config information
            FileStream fileStream = new FileStream(LocalConfig.configFile, FileMode.OpenOrCreate);
            using (fileStream)
            {
                // if length is 0, the file was probably removed or misplaced; set to default values
                if (fileStream.Length == 0)
                    LocalConfig.winStartup.Checked = true;
                // else read bytes and assign to the appropriate properties
                else
                    LocalConfig.winStartup.Checked = Convert.ToBoolean((byte)fileStream.ReadByte());
            }

            // update registry for winStartup
            this.WinStartupRegistry();
        }
    }
}
