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
    /// <summary>
    /// This static class implements the handling of user config information.
    /// </summary>
    static class Config
    {
        private const int CONFIG_SIZE = 5; // num bytes that a valid CONFIG file would be; depends on number of config settings

        private static readonly FileInfo configFile; // config file directory and other info

        static Config()
        {
            Config.configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG.mp"));
        }

        /// <summary>
        /// store MainWindow to access its variables
        /// </summary>
        public static MainWindow MainWindow { private get; set; }

        public static void WinStartupRegistry()
        {
            // establish the registry key for Windows startup; bool set to true to allow write access
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // add to or delete from Windows startup processes depending on bool
            if (Config.MainWindow.WinStartup.Checked)
            {
                rk.SetValue("MultiPaste", System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            else
            {
                rk.DeleteValue("MultiPaste", false);
            }
        }

        public static void FromFile()
        {
            // read from config file to get user config information
            using (FileStream fs = Config.configFile.Open(FileMode.OpenOrCreate))
            {
                // check for valid CONFIG file size
                if (fs.Length == Config.CONFIG_SIZE)
                {
                    // read bytes and assign to the appropriate properties
                    Config.MainWindow.WinStartup.Checked = Convert.ToBoolean(fs.ReadByte());
                    Config.MainWindow.WrapKeys.Checked = Convert.ToBoolean(fs.ReadByte());
                    Config.MainWindow.MoveToCopied.Checked = Convert.ToBoolean(fs.ReadByte());
                    Config.MainWindow.ChangeTopBottom.Checked = Convert.ToBoolean(fs.ReadByte());

                    // read ColorTheme from file
                    Config.MainWindow.ColorThemeBox.SelectedIndex = fs.ReadByte();
                }
                // else CONFIG file is invalid
                else
                {
                    // load default config
                    Config.LoadDefaults(fs);
                }
            }

            // update registry for WinStartup
            Config.WinStartupRegistry();

            // update ColorTheme of MultiPaste
            Config.ChangeTheme();
        }

        /// <summary>
        /// UpdateFile with an initialized FileStream as a parameter
        /// 
        /// This method is used if the caller already has a FileStream 
        /// instance to the CONFIG file.
        /// </summary>
        /// <param name="fs">FileStream to the CONFIG file</param>
        private static void UpdateFile(FileStream fs)
        {
            // write applicable bools to file
            fs.WriteByte(Convert.ToByte(Config.MainWindow.WinStartup.Checked));
            fs.WriteByte(Convert.ToByte(Config.MainWindow.WrapKeys.Checked));
            fs.WriteByte(Convert.ToByte(Config.MainWindow.MoveToCopied.Checked));
            fs.WriteByte(Convert.ToByte(Config.MainWindow.ChangeTopBottom.Checked));

            // write selected ColorTheme to file
            fs.WriteByte((byte)Config.MainWindow.ColorThemeBox.SelectedIndex);
        }

        public static void UpdateFile()
        {
            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (FileStream fs = Config.configFile.Open(FileMode.Create))
            {
                // call helper with FileStream as arg
                Config.UpdateFile(fs);
            }
        }

        /// <summary>
        /// LoadDefaults with an initialized FileStream as a parameter
        /// 
        /// This method is used if the caller already has a FileStream 
        /// instance to the CONFIG file.
        /// </summary>
        /// <param name="fs">FileStream to the CONFIG file</param>
        private static void LoadDefaults(FileStream fs)
        {
            // set to default values
            Config.MainWindow.WinStartup.Checked = true;
            Config.MainWindow.WrapKeys.Checked = false;
            Config.MainWindow.MoveToCopied.Checked = true;
            Config.MainWindow.ChangeTopBottom.Checked = true;
            Config.MainWindow.ColorThemeBox.SelectedIndex = (int)MainWindow.ColorTheme.Light;

            // update registry for WinStartup
            Config.WinStartupRegistry();

            // update ColorTheme of MultiPaste
            Config.ChangeTheme();

            // write default values to CONFIG file
            Config.UpdateFile(fs);
        }

        public static void LoadDefaults()
        {
            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using (FileStream fs = Config.configFile.Open(FileMode.Create))
            {
                // call helper with FileStream as arg
                Config.LoadDefaults(fs);
            }
        }

        /// <summary>
        /// ChangeTheme with an ArgbCollection as a parameter
        /// 
        /// This method is used if the caller already has a 
        /// specified color theme to pass.
        /// </summary>
        /// <param name="myTheme">ArgbCollection that represents the selected color theme</param>
        private static void ChangeTheme(ArgbCollection myTheme)
        {
            // set main window's background color
            Config.MainWindow.BackColor = myTheme.Background.GetColor();

            // set color of each control based on type
            foreach (Control control in Config.MainWindow.Controls)
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

        public static void ChangeTheme()
        {
            // determine which argb collection should be used
            MainWindow.ColorTheme colorTheme = (MainWindow.ColorTheme)Config.MainWindow.ColorThemeBox.SelectedIndex;
            switch (colorTheme)
            {
                case MultiPaste.MainWindow.ColorTheme.Light:
                    Config.ChangeTheme(Themes.Light);
                    break;

                case MultiPaste.MainWindow.ColorTheme.Dark:
                    Config.ChangeTheme(Themes.Dark);
                    break;
            }
        }
    }
}
