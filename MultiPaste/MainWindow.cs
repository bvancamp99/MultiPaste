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
    public partial class MainWindow : Form
    {
        private const string CONFIG_FILENAME = "CONFIG"; // store name of config file
        private const byte CHAR_LIMIT = 100; // store max number of characters that can be displayed as a key in the dictionary
        private readonly Timer timer; // create a winforms timer that will be used for notifLabel
        private readonly GlobalEventHook eventHook; // store custom multi-purpose event hook

        public MainWindow()
        {
            // required method for Designer support
            InitializeComponent();

            // read from config file to get user config information
            using (var fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.
                BaseDirectory, CONFIG_FILENAME), FileMode.OpenOrCreate))
            {
                // if length is 0, the file was probably removed or misplaced; set to default values
                if (fileStream.Length == 0)
                    winStartupItem.Checked = true;
                // else read bytes and assign to the appropriate properties
                else
                    winStartupItem.Checked = Convert.ToBoolean((byte)fileStream.ReadByte());
            }

            // change properties depending on user config settings
            UpdateWindowsStartup();

            // init clipboard dictionary
            ClipboardDict = new Dictionary<string, ClipboardItem>();

            // read from clipboard file and add each saved clipboard item
            AddClipboardItemsFromFile();

            // initialize the timer with its interval at 3 seconds
            timer = new Timer
            {
                Interval = 3000
            };

            // clear notifLabel when the timer goes off, then stop the timer
            timer.Tick += (sender, e) =>
            {
                notifLabel.Text = string.Empty;
                timer.Stop();
            };

            // initialize custom event hook that will handle clipboard changes and keyboard input
            eventHook = new GlobalEventHook(this);
        }

        /// if true, clipboard change will be handled when detected
        public bool HandleClipboard { get; private set; } = true;

        /// safely returns private const int CHAR_LIMIT
        public byte CharacterLimit
        {
            get { return CHAR_LIMIT; }
        }

        /// store all clipboard items' KeyTexts in a StringCollection
        public StringCollection KeyTextCollection { get; } = new StringCollection();

        /// safely returns private ListBox listBox
        public ListBox ListBox
        {
            get { return listBox; }
        }

        /// store/retrieve clipboard data
        internal Dictionary<string, ClipboardItem> ClipboardDict { get; }

        public void NotifyUser(string notifText)
        {
            // stop the timer if it's currently running
            if (timer.Enabled)
                timer.Stop();

            // set notifLabel's text to the param string
            notifLabel.Text = notifText;

            // begin the timer, which will clear notifLabel after 3 seconds
            timer.Start();
        }

        public void HandleClipboardChange()
        {
            if (Clipboard.ContainsText())
                new TextItem(this, Clipboard.GetText());
            else if (Clipboard.ContainsFileDropList())
                new FileItem(this, Clipboard.GetFileDropList());
            else if (Clipboard.ContainsImage())
                new ImageItem(this, Clipboard.GetImage());
            else if (Clipboard.ContainsAudio())
                new AudioItem(this, Clipboard.GetAudioStream());
            else
                new CustomItem(this, Clipboard.GetDataObject());
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
                new TextItem(this, e.Data.GetData(DataFormats.UnicodeText) as string);
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                StringCollection fileDropList = new StringCollection();
                fileDropList.AddRange(e.Data.GetData(DataFormats.FileDrop) as string[]);
                new FileItem(this, fileDropList);
            }
        }

        private void CopyToClipboard()
        {
            // check for valid SelectedIndex val before continuing
            if (ListBox.SelectedIndex < 0)
                return;

            // clipboard will be changed in this method; we don't want it to be handled
            HandleClipboard = false;

            // store the selected item
            ClipboardItem clipboardItem = ClipboardDict[KeyTextCollection[ListBox.SelectedIndex]];

            // store an error msg string that will notify the user if an error occurred
            string errMsg = null;

            switch (clipboardItem.Type)
            {
                case ClipboardItem.TypeEnum.Text:
                    Clipboard.SetText((clipboardItem as TextItem).Text);
                    break;

                case ClipboardItem.TypeEnum.FileDropList:
                    Clipboard.SetFileDropList((clipboardItem as FileItem).FileDropList);
                    break;

                case ClipboardItem.TypeEnum.Image:
                    if (File.Exists(Path.Combine(ImageItem.FolderDir, clipboardItem.KeyText)))
                    {
                        // store the image from the file
                        Image image = Image.FromFile(Path.Combine(ImageItem.FolderDir, clipboardItem.KeyText));

                        // set image to the Windows clipboard
                        Clipboard.SetImage(image);

                        // dispose the image to free up the file
                        image.Dispose();
                    }
                    else
                        errMsg = "Image file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Audio:
                    if (File.Exists(Path.Combine(AudioItem.FolderDir, clipboardItem.KeyText)))
                    {
                        // store audio stream from the file
                        Stream audio = new MemoryStream(File.ReadAllBytes(Path.Combine
                            (AudioItem.FolderDir, clipboardItem.KeyText)));

                        // set audio to the Windows clipboard
                        Clipboard.SetAudio(audio);

                        // dispose the stream to free up the file
                        audio.Dispose();
                    }
                    else
                        errMsg = "Audio file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Custom:
                    if (File.Exists(Path.Combine(CustomItem.FolderDir, clipboardItem.KeyText)))
                    {
                        // store custom data from the file
                        object data;
                        using (var stream = new FileStream(Path.Combine(CustomItem.FolderDir,
                            clipboardItem.KeyText), FileMode.Open))
                        {
                            data = new BinaryFormatter().Deserialize(stream);
                        }

                        // set custom data to the Windows clipboard
                        Clipboard.SetData((clipboardItem as CustomItem).WritableFormat, data);
                    }
                    else
                        errMsg = "Custom file is missing!";
                    break;
            }

            // flip to true after algorithm is finished
            HandleClipboard = true;

            // notify to the user the results of the operation attempt
            if (errMsg == null)
                NotifyUser("Copied to clipboard!");
            else
                NotifyUser(errMsg);
        }

        private void AddClipboardItemsFromFile()
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(ClipboardItem.ClipboardDir) || new FileInfo(ClipboardItem.ClipboardDir).Length <= 0)
                return;

            // if any data files are missing, this temp CLIPBOARD file will be updated
            File.WriteAllText(ClipboardItem.TempClipDir, File.ReadAllText(ClipboardItem.ClipboardDir));

            // traverse and store the contents of the file into new instances of ClipboardItem's derived classes
            using (StreamReader streamReader = new StreamReader(ClipboardItem.ClipboardDir))
            {
                while (!streamReader.EndOfStream)
                {
                    // Type
                    ClipboardItem.TypeEnum type = (ClipboardItem.TypeEnum)streamReader.Read();

                    // KeyDiff
                    ushort keyDiff = ushort.Parse(streamReader.ReadLine());

                    // remaining operations depend on the type of item
                    switch (type)
                    {
                        case ClipboardItem.TypeEnum.Text:
                            new TextItem(this, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.FileDropList:
                            new FileItem(this, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Image:
                            new ImageItem(this, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Audio:
                            new AudioItem(this, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Custom:
                            new CustomItem(this, keyDiff, streamReader);
                            break;
                    }
                }
            }

            // all items have been read from the file and added; replace CLIPBOARD file with the temp file
            File.Delete(ClipboardItem.ClipboardDir);
            File.Move(ClipboardItem.TempClipDir, ClipboardItem.ClipboardDir);
        }

        private void UpdateWindowsStartup()
        {
            // establish the registry key for Windows startup; bool set to true to allow write access
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // add to or delete from Windows startup processes depending on bool
            if (winStartupItem.Checked)
                registryKey.SetValue("MultiPaste", System.Reflection.Assembly.GetEntryAssembly().Location);
            else
                registryKey.DeleteValue("MultiPaste", false);
        }

        private void WriteConfigFile()
        {
            // before writing, clear config or create new empty file if it was unexpectedly deleted
            using FileStream fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.
                BaseDirectory, CONFIG_FILENAME), FileMode.Create);

            // write applicable bools to file
            fileStream.WriteByte(Convert.ToByte(winStartupItem.Checked));
        }

        private void RemoveItem(int index)
        {
            // return if index is invalid
            if (index < 0)
                return;

            // remove the ListBox item located at the index
            ClipboardDict[KeyTextCollection[index]].Remove();

            // if there was an item located after the removed item, select that item
            if (ListBox.Items.Count > index)
                ListBox.SelectedIndex = index;
            // else select the item located before the removed item
            else
                ListBox.SelectedIndex = index - 1;

            // notify the user of the successful operation for 3 seconds
            NotifyUser("Item removed!");
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    // if there is one item in ListBox, select that item
                    if (ListBox.Items.Count == 1)
                        ListBox.SelectedIndex = 0;
                    // else determine actions on the selected index
                    else if (ListBox.Items.Count > 1)
                    {
                        // store current index of the selected item
                        int selectedIndex = ListBox.SelectedIndex;

                        // if no item is selected, set selected index to 0 and leave
                        if (selectedIndex < 0)
                        {
                            ListBox.SelectedIndex = 0;
                            break;
                        }

                        // if shift key is also held, then we want to move the selected item
                        if (e.Shift)
                        {
                            // move item to the top if ctrl key is being pressed
                            if (e.Control)
                            {
                                if (selectedIndex > 0)
                                {
                                    ClipboardDict[KeyTextCollection[selectedIndex]].Move(0);
                                    ListBox.SelectedIndex = 0;
                                }
                            }
                            // else if selected index is 0, move selected item to last index
                            else if (selectedIndex == 0)
                            {
                                ClipboardDict[KeyTextCollection[0]].Move(ListBox.Items.Count - 1);
                                ListBox.SelectedIndex = ListBox.Items.Count - 1;
                            }
                            // else move item up one index
                            else
                            {
                                ClipboardDict[KeyTextCollection[selectedIndex]].Move(selectedIndex - 1);
                                ListBox.SelectedIndex = selectedIndex - 1;
                            }
                        }
                        // else only change selected index
                        else
                        {
                            // if selected item is at index 0, select bottom item
                            if (ListBox.SelectedIndex <= 0)
                                ListBox.SelectedIndex = ListBox.Items.Count - 1;
                            // else select previous item
                            else
                                ListBox.SelectedIndex--;
                        }
                    }
                    break;

                case Keys.Down:
                    // if there is one item in ListBox, select that item
                    if (ListBox.Items.Count == 1)
                        ListBox.SelectedIndex = 0;
                    // else determine actions on the selected index
                    else if (ListBox.Items.Count > 1)
                    {
                        // store current index of the selected item
                        int selectedIndex = ListBox.SelectedIndex;

                        // if no item is selected, set selected index to 0 and leave
                        if (selectedIndex < 0)
                        {
                            ListBox.SelectedIndex = 0;
                            break;
                        }

                        // if shift key is also held, then we want to move the selected item
                        if (e.Shift)
                        {
                            // move item to the bottom if ctrl key is being pressed
                            if (e.Control)
                            {
                                if (selectedIndex < ListBox.Items.Count - 1)
                                {
                                    ClipboardDict[KeyTextCollection[selectedIndex]].Move(ListBox.Items.Count - 1);
                                    ListBox.SelectedIndex = ListBox.Items.Count - 1;
                                }
                            }
                            // else if index selected isn't the final index, move item down one index
                            else if (ListBox.SelectedIndex < ListBox.Items.Count - 1)
                            {
                                ClipboardDict[KeyTextCollection[selectedIndex]].Move(selectedIndex + 1);
                                ListBox.SelectedIndex = selectedIndex + 1;
                            }
                            // else move selected item to index 0
                            else
                            {
                                ClipboardDict[KeyTextCollection[ListBox.Items.Count - 1]].Move(0);
                                ListBox.SelectedIndex = 0;
                            }
                        }
                        // else only change selected index
                        else
                        {
                            // if selected item isn't the final index, select the next item
                            if (ListBox.SelectedIndex < ListBox.Items.Count - 1)
                                ListBox.SelectedIndex++;
                            // else select item at index 0
                            else
                                ListBox.SelectedIndex = 0;
                        }
                    }
                    break;

                case Keys.Enter:
                    // copy selected item to the Windows clipboard
                    CopyToClipboard();
                    break;

                case Keys.Delete:
                    // programmatically click the Remove button
                    removeBtn.PerformClick();
                    break;

                case Keys.Escape:
                    // minimize to the taskbar
                    WindowState = FormWindowState.Minimized;
                    break;

                case Keys.F4:
                    // minimize to system tray on Alt + F4
                    if (e.Alt)
                        Visible = false;
                    break;
            }

            e.Handled = true; // stop event handling chain
        }

        private void MoveTopBtn_Click(object sender, EventArgs e)
        {
            // set focus away from the button
            ListBox.Focus();

            // return if SelectedIndex is invalid or if the item is already at the top
            if (ListBox.SelectedIndex <= 0)
                return;

            // relocate the selected item to the top of ListBox
            ClipboardDict[KeyTextCollection[ListBox.SelectedIndex]].Move(0);

            // select the item that was moved to the top
            ListBox.SelectedIndex = 0;
        }

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            // copy selected item to the Windows clipboard
            CopyToClipboard();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            // set focus away from the button
            ListBox.Focus();

            // remove ListBox selected item
            RemoveItem(ListBox.SelectedIndex);
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true; // show the form
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // minimize the form if user clicked the X button
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // cancel exiting prog
                Visible = false; // hide the form
            }
        }

        private void ClearItem_Click(object sender, EventArgs e)
        {
            // clear ListBox, KeyTextCollection, and ClipboardDict
            ListBox.Items.Clear();
            KeyTextCollection.Clear();
            ClipboardDict.Clear();

            // delete the CLIPBOARD file
            File.Delete(ClipboardItem.ClipboardDir);

            // recursively delete each item folder if it exists
            if (Directory.Exists(ImageItem.FolderDir))
                Directory.Delete(ImageItem.FolderDir, true);
            if (Directory.Exists(AudioItem.FolderDir))
                Directory.Delete(AudioItem.FolderDir, true);
            if (Directory.Exists(CustomItem.FolderDir))
                Directory.Delete(CustomItem.FolderDir, true);

            NotifyUser("All items cleared!");
        }

        private void UpdateDisplayedItems()
        {
            ListBox.Items.Clear(); // clear ListBox

            foreach (string key in KeyTextCollection)
            {
                // add to ListBox if the item's data type is allowed
                if ((dispTextItem.Checked && ClipboardDict[key].Type == ClipboardItem.TypeEnum.Text)
                    || (dispFilesItem.Checked && ClipboardDict[key].Type == ClipboardItem.TypeEnum.FileDropList)
                    || (dispImagesItem.Checked && ClipboardDict[key].Type == ClipboardItem.TypeEnum.Image)
                    || (dispAudioItem.Checked && ClipboardDict[key].Type == ClipboardItem.TypeEnum.Audio)
                    || (dispCustomItem.Checked && ClipboardDict[key].Type == ClipboardItem.TypeEnum.Custom))
                    ListBox.Items.Add(key);
            }
        }

        private void DispTextItem_Click(object sender, EventArgs e)
        {
            UpdateDisplayedItems();

            // notify the user of the change
            if (dispTextItem.Checked)
                NotifyUser("Text items displayed!");
            else
                NotifyUser("Text items hidden!");
        }

        private void DispFilesItem_Click(object sender, EventArgs e)
        {
            UpdateDisplayedItems();

            // notify the user of the change
            if (dispFilesItem.Checked)
                NotifyUser("File items displayed!");
            else
                NotifyUser("File items hidden!");
        }

        private void DispImagesItem_Click(object sender, EventArgs e)
        {
            UpdateDisplayedItems();

            // notify the user of the change
            if (dispImagesItem.Checked)
                NotifyUser("Image items displayed!");
            else
                NotifyUser("Image items hidden!");
        }

        private void DispAudioItem_Click(object sender, EventArgs e)
        {
            UpdateDisplayedItems();

            // notify the user of the change
            if (dispAudioItem.Checked)
                NotifyUser("Audio items displayed!");
            else
                NotifyUser("Audio items hidden!");
        }

        private void DispCustomItem_Click(object sender, EventArgs e)
        {
            UpdateDisplayedItems();

            // notify the user of the change
            if (dispCustomItem.Checked)
                NotifyUser("Custom items displayed!");
            else
                NotifyUser("Custom items hidden!");
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void WinStartupItem_Click(object sender, EventArgs e)
        {
            // update Windows startup process
            UpdateWindowsStartup();

            // update file
            WriteConfigFile();
        }

        private void HelpItem_Click(object sender, EventArgs e)
        {
            // notify the user that we're attempting to open the help file
            NotifyUser("Opening help.txt...");

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
            NotifyUser("help.txt is open!");
        }
    }
}
