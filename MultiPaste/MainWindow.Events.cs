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
    /// <summary>
    /// This partial class of MainWindow acts as a handler for all events that
    /// occur on the custom form.
    /// </summary>
    partial class MainWindow : Form
    {
        public void OnClipboardChange()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                _ = new TextItem(this, text);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection files = Clipboard.GetFileDropList();
                _ = new FileItem(this, files);
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                _ = new ImageItem(this, image);
            }
            else if (Clipboard.ContainsAudio())
            {
                Stream audio = Clipboard.GetAudioStream();
                _ = new AudioItem(this, audio);
            }
            else
            {
                IDataObject data = Clipboard.GetDataObject();
                _ = new CustomItem(this, data);
            }
        }

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
                    this.OnArrowKeyUp(e);
                    break;

                case Keys.Down:
                    this.OnArrowKeyDown(e);
                    break;

                case Keys.Enter:
                    // copy selected item to the Windows clipboard
                    LocalClipboard.Copy();
                    break;

                case Keys.C:
                    // copy selected item to clipboard on ctrl + c
                    if (e.Control) LocalClipboard.Copy();
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
                    if (e.Alt) this.Visible = false;
                    break;
            }

            // stop event handling chain
            e.Handled = true;
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

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            // copy selected item to the Windows clipboard
            LocalClipboard.Copy();
        }

        private void SearchTextBox_Enter(object sender, EventArgs e)
        {
            // if searchTextBox's current text is the default
            if (this.searchTextBox.Text == MainWindow.SEARCH_DEFAULT)
            {
                // clear searchTextBox's text
                this.searchTextBox.Text = string.Empty;
            }
        }

        private void SearchTextBox_Leave(object sender, EventArgs e)
        {
            // if searchTextBox's current text is empty
            if (this.searchTextBox.Text == string.Empty)
            {
                // set to default text
                this.searchTextBox.Text = MainWindow.SEARCH_DEFAULT;
            }
        }

        /// <summary>
        /// This method restricts the items that are displayed on
        /// the visual clipboard by comparing them to a keyword.
        /// </summary>
        /// <param name="keyword">current string in this.searchTextBox</param>
        private void RestrictKeys(string keyword)
        {
            // first, clear the visual part of the local clipboard
            ListBox.ObjectCollection myItems = this.listBox.Items;
            myItems.Clear();

            // traverse the keys in the local clipboard
            foreach (string key in LocalClipboard.Keys)
            {
                // if the item's key contains the keyword
                if (key.Contains(keyword))
                {
                    // add item back to the visual clipboard
                    myItems.Add(key);
                }
            }
        }

        /// <summary>
        /// This method allows all items to be displayed on the visual
        /// clipboard.
        /// </summary>
        private void AllowKeys()
        {
            // nothing to do if all items are already being displayed
            if (this.listBox.Items.Count == LocalClipboard.Keys.Count)
                return;
            
            // first, clear the visual part of the local clipboard
            ListBox.ObjectCollection myItems = this.listBox.Items;
            myItems.Clear();

            // traverse the keys in the local clipboard
            foreach (string key in LocalClipboard.Keys)
            {
                // add each item back to the visual clipboard
                myItems.Add(key);
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            // show all items if text is empty or the default
            if (this.searchTextBox.Text == MainWindow.SEARCH_DEFAULT ||
                this.searchTextBox.Text == string.Empty)
            {
                this.AllowKeys();
            }
            // else restrict keys displayed based on search query
            else
            {
                this.RestrictKeys(this.searchTextBox.Text);
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            // set focus back to the local clipboard
            LocalClipboard.Focus();

            // remove the current index of the local clipboard
            LocalClipboard.Remove();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // display MainWindow when system tray icon is double-clicked
            this.Visible = true;
        }

        private void ClearItem_Click(object sender, EventArgs e)
        {
            LocalClipboard.Clear();
        }

        /// <summary>
        /// This method restricts the type of items that are displayed on
        /// the visual clipboard.
        /// </summary>
        private void RestrictTypes()
        {
            // first, clear the visual part of the local clipboard
            ListBox.ObjectCollection myItems = this.listBox.Items;
            myItems.Clear();

            // add each item back to the listbox if its type is allowed
            bool allowType;
            foreach (string key in LocalClipboard.Keys)
            {
                allowType = (this.showTextItem.Checked && LocalClipboard.Dict[key].Type == ClipboardItem.TypeEnum.Text)
                    || (this.showFilesItem.Checked && LocalClipboard.Dict[key].Type == ClipboardItem.TypeEnum.FileDropList)
                    || (this.showImagesItem.Checked && LocalClipboard.Dict[key].Type == ClipboardItem.TypeEnum.Image)
                    || (this.showAudioItem.Checked && LocalClipboard.Dict[key].Type == ClipboardItem.TypeEnum.Audio)
                    || (this.showCustomItem.Checked && LocalClipboard.Dict[key].Type == ClipboardItem.TypeEnum.Custom);

                // if type is allowed, add to the listbox
                if (allowType) myItems.Add(key);
            }
        }

        private void ShowTextItem_Click(object sender, EventArgs e)
        {
            this.RestrictTypes();

            // notify the user of the change
            if (showTextItem.Checked)
                MsgLabel.Normal("Text items displayed!");
            else
                MsgLabel.Normal("Text items hidden!");
        }

        private void ShowFilesItem_Click(object sender, EventArgs e)
        {
            this.RestrictTypes();

            // notify the user of the change
            if (showFilesItem.Checked)
                MsgLabel.Normal("File items displayed!");
            else
                MsgLabel.Normal("File items hidden!");
        }

        private void ShowImagesItem_Click(object sender, EventArgs e)
        {
            this.RestrictTypes();

            // notify the user of the change
            if (showImagesItem.Checked)
                MsgLabel.Normal("Image items displayed!");
            else
                MsgLabel.Normal("Image items hidden!");
        }

        private void ShowAudioItem_Click(object sender, EventArgs e)
        {
            this.RestrictTypes();

            // notify the user of the change
            if (showAudioItem.Checked)
                MsgLabel.Normal("Audio items displayed!");
            else
                MsgLabel.Normal("Audio items hidden!");
        }

        private void ShowCustomItem_Click(object sender, EventArgs e)
        {
            this.RestrictTypes();

            // notify the user of the change
            if (showCustomItem.Checked)
                MsgLabel.Normal("Custom items displayed!");
            else
                MsgLabel.Normal("Custom items hidden!");
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RootToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // store selected color theme
            MainWindow.ColorTheme colorTheme = (MainWindow.ColorTheme)this.colorThemeBox.SelectedIndex;

            // if color theme is dark
            if (colorTheme == MainWindow.ColorTheme.Dark)
            {
                // change font color temporarily (for visibility)
                ToolStripMenuItem myItem = (ToolStripMenuItem)sender;
                if (myItem != null)
                {
                    myItem.ForeColor = Themes.Light.Font.GetColor();
                }
            }
        }

        private void RootToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            // store selected color theme
            MainWindow.ColorTheme colorTheme = (MainWindow.ColorTheme)this.colorThemeBox.SelectedIndex;

            // if color theme is dark
            if (colorTheme == MainWindow.ColorTheme.Dark)
            {
                // change font color back to the dark theme's default
                ToolStripMenuItem myItem = (ToolStripMenuItem)sender;
                if (myItem != null)
                {
                    myItem.ForeColor = Themes.Dark.Font.GetColor();
                }
            }
        }

        private void WinStartupItem_Click(object sender, EventArgs e)
        {
            // update Windows startup process
            Config.WinStartupRegistry();

            // update CONFIG file
            Config.UpdateFile();
        }

        private void WrapKeysItem_Click(object sender, EventArgs e)
        {
            // update CONFIG file
            Config.UpdateFile();
        }

        private void MoveToCopiedItem_Click(object sender, EventArgs e)
        {
            // update CONFIG file
            Config.UpdateFile();
        }

        private void ChangeTopBottomItem_Click(object sender, EventArgs e)
        {
            // update CONFIG file
            Config.UpdateFile();
        }

        private void ColorThemeBox_DropDownClosed(object sender, EventArgs e)
        {
            // update color theme of MultiPaste
            Config.ChangeTheme();

            // update CONFIG file
            Config.UpdateFile();

            // change font color of the config drop down if needed
            this.RootToolStripMenuItem_DropDownOpening(this.configDropDown, null);
        }

        private void DefaultConfigItem_Click(object sender, EventArgs e)
        {
            // load default config
            Config.LoadDefaults();

            MsgLabel.Normal("Default config loaded!");
        }

        private void HelpItem_Click(object sender, EventArgs e)
        {
            // notify the user that we're attempting to open the help file
            MsgLabel.Normal("Opening help.txt...");

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
            MsgLabel.Normal("help.txt is open!");
        }

        private void OnArrowKeyUp(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = this.listBox.Items.Count;
            int current = this.listBox.SelectedIndex;

            // base case 1: nothing in the clipboard
            if (total == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                this.listBox.SelectedIndex = 0;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = LocalClipboard.Dict[LocalClipboard.Keys[current]];

            // store new index for the item and/or index to be moved
            int newIndex;

            // base case 3: current index is 0
            if (current == 0)
            {
                // if box isn't checked, no wrapping or moving is needed
                if (!this.wrapKeysItem.Checked)
                    return;

                // we will be wrapping to the bottom, so newIndex is set to the bottom index
                newIndex = total - 1;

                // move current item to the bottom index if shift is pressed
                if (e.Shift)
                {
                    LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, newIndex);

                    // wrap selected index to the bottom index only if the box is checked
                    if (this.ChangeTopBottom.Checked)
                        this.listBox.SelectedIndex = newIndex;
                    else
                        this.listBox.SelectedIndex = current;
                }
                // else don't move the current item, and wrap the selected index unconditionally
                else
                    this.listBox.SelectedIndex = newIndex;

                return;
            }

            // store whether the selected index should be changed
            bool changeIndex = true;

            // if ctrl is being pressed, set newIndex to the top index, i.e. 0
            if (e.Control)
            {
                newIndex = 0;

                // selected index should be unchanged if we're moving an item 
                // to the top/bottom, but the box is unchecked
                if (e.Shift && !this.ChangeTopBottom.Checked)
                    changeIndex = false;
            }
            // else set newIndex to current - 1
            else
            {
                newIndex = current - 1;
                // changeIndex is unconditionally true if ctrl isn't being pressed
            }

            // move item to new index if shift is pressed
            if (e.Shift)
                LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, newIndex);

            // move selected index to new index if bool is satisfied
            if (changeIndex)
                this.listBox.SelectedIndex = newIndex;
            else
                this.listBox.SelectedIndex = current;
        }

        private void OnArrowKeyDown(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = this.listBox.Items.Count;
            int current = this.listBox.SelectedIndex;

            // base case 1: nothing in the clipboard
            if (total == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                this.listBox.SelectedIndex = 0;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = LocalClipboard.Dict[LocalClipboard.Keys[current]];

            // store new index for the item and/or index to be moved
            int newIndex;

            // base case 3: current is at the last index
            if (current == total - 1)
            {
                // if box isn't checked, no wrapping or moving is needed
                if (!this.wrapKeysItem.Checked)
                    return;

                // we will be wrapping to the top, so newIndex is set to the top index, i.e. 0
                newIndex = 0;

                // move current item to the top index if shift is pressed
                if (e.Shift)
                {
                    LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, newIndex);

                    // wrap selected index to the top index only if the box is checked
                    if (this.ChangeTopBottom.Checked)
                        this.listBox.SelectedIndex = newIndex;
                    else
                        this.listBox.SelectedIndex = current;
                }
                // else don't move the current item, and wrap the selected index unconditionally
                else
                    this.listBox.SelectedIndex = newIndex;

                return;
            }

            // store whether the selected index should be changed
            bool changeIndex = true;

            // if ctrl is being pressed, set newIndex to the bottom index
            if (e.Control)
            {
                newIndex = total - 1;

                // selected index should be unchanged if we're moving an item 
                // to the top/bottom, but the box is unchecked
                if (e.Shift && !this.ChangeTopBottom.Checked)
                    changeIndex = false;
            }
            // else set newIndex to current + 1
            else
            {
                newIndex = current + 1;
                // changeIndex is unconditionally true if ctrl isn't being pressed
            }

            // move item to new index if shift is pressed
            if (e.Shift)
                LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, newIndex);

            // move selected index to new index if bool is satisfied
            if (changeIndex)
                this.listBox.SelectedIndex = newIndex;
            else
                this.listBox.SelectedIndex = current;
        }
    }
}
