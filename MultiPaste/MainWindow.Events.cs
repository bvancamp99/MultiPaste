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
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            // accept text and files
            if (e.Data.GetDataPresent(DataFormats.UnicodeText) || e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            // anything that can't be converted will not be accepted
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                _ = new TextItem(this, e.Data.GetData(DataFormats.UnicodeText) as string);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                StringCollection fileDropList = new StringCollection();
                fileDropList.AddRange(e.Data.GetData(DataFormats.FileDrop) as string[]);
                _ = new FileItem(this, fileDropList);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    this.Clipboard.OnKeyUp(e);
                    break;

                case Keys.Down:
                    this.Clipboard.OnKeyDown(e);
                    break;

                case Keys.Enter:
                    // copy selected item to the Windows clipboard
                    this.Clipboard.Copy();
                    break;

                case Keys.Delete:
                    // programmatically click the Remove button
                    this.removeBtn.PerformClick();
                    break;

                case Keys.Escape:
                    // minimize to the taskbar
                    this.WindowState = FormWindowState.Minimized;
                    break;

                case Keys.F4:
                    // minimize to system tray on Alt + F4
                    if (e.Alt)
                        this.Visible = false;
                    break;
            }

            e.Handled = true; // stop event handling chain
        }

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            // copy selected item to the Windows clipboard
            this.Clipboard.Copy();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            // set focus back to the local clipboard
            this.Clipboard.Focus();

            // remove the current index of the local clipboard
            this.Clipboard.Remove();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // display MainWindow when system tray icon is double-clicked
            this.Visible = true;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // minimize the form if user clicked the X button
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // cancel exiting prog
                this.Visible = false; // hide the form
            }
        }

        private void ClearItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.Clear();
        }

        private void ShowTextItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.RestrictTypes();

            // notify the user of the change
            if (showTextItem.Checked)
                this.MsgLabel.Normal("Text items displayed!");
            else
                this.MsgLabel.Normal("Text items hidden!");
        }

        private void ShowFilesItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.RestrictTypes();

            // notify the user of the change
            if (showFilesItem.Checked)
                this.MsgLabel.Normal("File items displayed!");
            else
                this.MsgLabel.Normal("File items hidden!");
        }

        private void ShowImagesItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.RestrictTypes();

            // notify the user of the change
            if (showImagesItem.Checked)
                this.MsgLabel.Normal("Image items displayed!");
            else
                this.MsgLabel.Normal("Image items hidden!");
        }

        private void ShowAudioItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.RestrictTypes();

            // notify the user of the change
            if (showAudioItem.Checked)
                this.MsgLabel.Normal("Audio items displayed!");
            else
                this.MsgLabel.Normal("Audio items hidden!");
        }

        private void ShowCustomItem_Click(object sender, EventArgs e)
        {
            this.Clipboard.RestrictTypes();

            // notify the user of the change
            if (showCustomItem.Checked)
                this.MsgLabel.Normal("Custom items displayed!");
            else
                this.MsgLabel.Normal("Custom items hidden!");
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void WinStartupItem_Click(object sender, EventArgs e)
        {
            // update Windows startup process
            this.Config.WinStartupRegistry();

            // update CONFIG file
            this.Config.UpdateFile();
        }

        private void HelpItem_Click(object sender, EventArgs e)
        {
            // notify the user that we're attempting to open the help file
            this.MsgLabel.Normal("Opening help.txt...");

            // create or open help.txt and overwrite its contents
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.txt"),
                "QUICK HELP:\n" +
                "Q: How do I add items to MultiPaste?\n" +
                "A: Copy like you normally would (e.g. Ctrl + C).  MultiPaste also supports drag-and-drop on most items!\n\n" +
                "Q: How do I exit the program?\n" +
                "A: Click on File->Exit, or right click MultiPaste from the system tray and click Exit.\n\n" +
                "Q: How do I minimize MultiPaste to the taskbar?\n" +
                "A: Press the Esc key or the minimize button at the top right.\n\n" +
                "Q: How do I minimize to the system tray so that it doesn't show up in the taskbar?\n" +
                "A: Click on the X button, or press Alt + F4.\n\n" +
                "Q: Is there a global hotkey I can use to show/hide MultiPaste?\n" +
                "A: Yes there is!  Ctrl + Alt + V is registered as a global show/hide hotkey for MultiPaste.\n\n" +
                "Q: How do I paste the thing that I copied?\n" +
                "A: Double-click the item to copy it to the Windows clipboard.  You can also press the Enter key.\n\n" +
                "Q: How do I delete one of the things that I've copied?\n" +
                "A: Click on the Remove button.  You can also press the Delete key.\n\n" +
                "Q: What if I want to clear all the items that I've copied without individually deleting all of them?\n" +
                "A: Click on File->Clear All Copied Items.  You can also press Ctrl + K.\n\n" +
                "Q: How do I relocate a copied item to a different index?\n" +
                "A: When the item is selected, hold the Shift key while pressing the up or down arrow key.\n\n" +
                "Q: Is there a way to quickly relocate an item to the top/bottom?\n" +
                "A: Yes there is!  Hold the Ctrl key along with the above relocating instructions.");

            // open the file
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.txt"));

            // notify the user of the successful operation for 3 seconds
            this.MsgLabel.Normal("help.txt is open!");
        }
    }
}
