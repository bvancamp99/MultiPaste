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

        public Config(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            this.ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG");
        }

        /// <summary>
        /// config file directory
        /// </summary>
        public string ConfigFile { get; }

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
