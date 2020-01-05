using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    class Config
    {
        private const int CONFIG_SIZE = 5; // num bytes that a valid CONFIG file would be; depends on number of config settings

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
                // check for valid CONFIG file size
                if (fileStream.Length == Config.CONFIG_SIZE)
                {
                    // read bytes and assign to the appropriate properties
                    this.mainWindow.WinStartup.Checked = Convert.ToBoolean(fileStream.ReadByte());
                    this.mainWindow.WrapKeys.Checked = Convert.ToBoolean(fileStream.ReadByte());
                    this.mainWindow.MoveToCopied.Checked = Convert.ToBoolean(fileStream.ReadByte());
                    this.mainWindow.ChangeTopBottom.Checked = Convert.ToBoolean(fileStream.ReadByte());

                    // read ColorTheme from file
                    this.mainWindow.ColorThemeBox.SelectedIndex = fileStream.ReadByte();
                }
                // else CONFIG file is invalid
                else
                {
                    // load default config
                    this.LoadDefaults(fileStream);
                }
            }

            // update registry for WinStartup
            this.WinStartupRegistry();

            // update ColorTheme of MultiPaste
            this.ChangeTheme();
        }

        public void UpdateFile()
        {
            // init FileStream to write to file
            FileStream fileStream = new FileStream(this.ConfigFile, FileMode.Create);

            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (fileStream)
            {
                // call helper with fileStream as arg
                this.UpdateFile(fileStream);
            }
        }

        /// <summary>
        /// UpdateFile with an initialized FileStream as a parameter
        /// 
        /// This method is used if the caller already has a FileStream 
        /// instance to the CONFIG file.
        /// </summary>
        /// <param name="fileStream">FileStream to the CONFIG file</param>
        private void UpdateFile(FileStream fileStream)
        {
            // write applicable bools to file
            fileStream.WriteByte(Convert.ToByte(this.mainWindow.WinStartup.Checked));
            fileStream.WriteByte(Convert.ToByte(this.mainWindow.WrapKeys.Checked));
            fileStream.WriteByte(Convert.ToByte(this.mainWindow.MoveToCopied.Checked));
            fileStream.WriteByte(Convert.ToByte(this.mainWindow.ChangeTopBottom.Checked));

            // write selected ColorTheme to file
            fileStream.WriteByte((byte)this.mainWindow.ColorThemeBox.SelectedIndex);
        }

        public void LoadDefaults()
        {
            // init FileStream to write to file
            FileStream fileStream = new FileStream(this.ConfigFile, FileMode.Create);

            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (fileStream)
            {
                // call helper with fileStream as arg
                this.LoadDefaults(fileStream);
            }
        }

        /// <summary>
        /// LoadDefaults with an initialized FileStream as a parameter
        /// 
        /// This method is used if the caller already has a FileStream 
        /// instance to the CONFIG file.
        /// </summary>
        /// <param name="fileStream">FileStream to the CONFIG file</param>
        private void LoadDefaults(FileStream fileStream)
        {
            // set to default values
            this.mainWindow.WinStartup.Checked = true;
            this.mainWindow.WrapKeys.Checked = false;
            this.mainWindow.MoveToCopied.Checked = true;
            this.mainWindow.ChangeTopBottom.Checked = true;
            this.mainWindow.ColorThemeBox.SelectedIndex = (int)MainWindow.ColorTheme.Light;

            // update registry for WinStartup
            this.WinStartupRegistry();

            // update ColorTheme of MultiPaste
            this.ChangeTheme();

            // write default values to CONFIG file
            this.UpdateFile(fileStream);
        }

        public void ChangeTheme()
        {
            // determine which argb collection should be used
            MainWindow.ColorTheme colorTheme = (MainWindow.ColorTheme)this.mainWindow.ColorThemeBox.SelectedIndex;
            switch (colorTheme)
            {
                case MainWindow.ColorTheme.Light:
                    this.ChangeTheme(Themes.Light);
                    break;

                case MainWindow.ColorTheme.Dark:
                    this.ChangeTheme(Themes.Dark);
                    break;
            }
        }

        /// <summary>
        /// ChangeTheme with an ArgbCollection as a parameter
        /// 
        /// This method is used if the caller already has a 
        /// specified color theme to pass.
        /// </summary>
        /// <param name="myTheme">ArgbCollection that represents the selected color theme</param>
        private void ChangeTheme(ArgbCollection myTheme)
        {
            // set main window's background color
            this.mainWindow.BackColor = myTheme.Background.GetColor();

            // set color of each control based on type
            foreach (Control control in this.mainWindow.Controls)
            {
                if (control is MenuStrip)
                {
                    control.BackColor = myTheme.MenuStrip.GetColor();

                    // update font color of each menu item
                    foreach (ToolStripItem item in ((MenuStrip)control).Items)
                    {
                        item.ForeColor = myTheme.Font.GetColor();
                    }
                }
                else if (control is Button)
                {
                    control.BackColor = myTheme.Cards.GetColor();
                }
                else if (control is ListBox)
                {
                    control.BackColor = myTheme.Cards.GetColor();
                }

                // set font color
                control.ForeColor = myTheme.Font.GetColor();
            }
        }
    }
}
