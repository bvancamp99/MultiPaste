using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    class LocalClipboard
    {
        public const int CHAR_LIMIT = 100; // store max number of characters that can be displayed as a key in the dictionary

        private readonly MainWindow mainWindow; // store MainWindow instance to access its variables

        public LocalClipboard(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            this.Dict = new Dictionary<string, ClipboardItem>();
            this.Keys = new StringCollection();
            this.HandleClipboard = true;

            // init readonly properties denoting directories of files/folders
            this.ClipboardFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD.mp");
            this.BackupFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD.bak");
            //this.TempFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            this.ImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            this.AudioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
            this.CustomFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom");
        }

        /// <summary>
        /// dict used for fast access of clipboard data
        /// </summary>
        public Dictionary<string, ClipboardItem> Dict { get; }

        /// <summary>
        /// collection used to store keys to clipboard items
        /// </summary>
        public StringCollection Keys { get; }

        /// <summary>
        /// bool to determine if we should handle clipboard changes
        /// </summary>
        public bool HandleClipboard { get; private set; }

        /// <summary>
        /// clipboard file directory
        /// </summary>
        public string ClipboardFile { get; }

        /// <summary>
        /// directory for clipboard file backup
        /// </summary>
        public string BackupFile { get; }

        ///// <summary>
        ///// temp clipboard file, used when adding items from file on startup
        ///// </summary>
        //public string TempFile { get; }

        /// <summary>
        /// image folder directory
        /// </summary>
        public string ImageFolder { get; }

        /// <summary>
        /// audio folder directory
        /// </summary>
        public string AudioFolder { get; }

        /// <summary>
        /// custom folder directory
        /// </summary>
        public string CustomFolder { get; }

        public void OnClipboardChange()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                _ = new TextItem(this.mainWindow, text);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection files = Clipboard.GetFileDropList();
                _ = new FileItem(this.mainWindow, files);
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                _ = new ImageItem(this.mainWindow, image);
            }
            else if (Clipboard.ContainsAudio())
            {
                Stream audio = Clipboard.GetAudioStream();
                _ = new AudioItem(this.mainWindow, audio);
            }
            else
            {
                IDataObject data = Clipboard.GetDataObject();
                _ = new CustomItem(this.mainWindow, data);
            }
        }

        public void RestrictTypes()
        {
            // first, clear the visual part of the local clipboard
            ListBox.ObjectCollection myItems = this.mainWindow.ListBox.Items;
            myItems.Clear();

            // add each item back to the listbox if its type is allowed
            bool allowType;
            foreach (string key in this.Keys)
            {
                allowType = (this.mainWindow.ShowText.Checked && this.Dict[key].Type == ClipboardItem.TypeEnum.Text)
                    || (this.mainWindow.ShowFiles.Checked && this.Dict[key].Type == ClipboardItem.TypeEnum.FileDropList)
                    || (this.mainWindow.ShowImages.Checked && this.Dict[key].Type == ClipboardItem.TypeEnum.Image)
                    || (this.mainWindow.ShowAudio.Checked && this.Dict[key].Type == ClipboardItem.TypeEnum.Audio)
                    || (this.mainWindow.ShowCustom.Checked && this.Dict[key].Type == ClipboardItem.TypeEnum.Custom);

                // if type is allowed, add to the listbox
                if (allowType)
                    myItems.Add(key);
            }
        }

        public void Focus()
        {
            this.mainWindow.ListBox.Focus();
        }

        public void Add(string key, ClipboardItem value)
        {
            // add to back-end dict
            try
            {
                this.Dict.Add(key, value);
            }
            // exception thrown on attempt to add a duplicate key to the dict
            catch (ArgumentException)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = this.Dict[key];

                // remove old item and prepare to replace with new item
                this.Remove(duplicateKeyItem.KeyText, duplicateKeyItem);

                // add new item to dict
                this.Dict.Add(key, value);
            }

            // add to back-end string collection of keys
            this.Keys.Insert(0, key);

            // add to visual clipboard last
            this.mainWindow.ListBox.Items.Insert(0, key);
        }

        public void AddWithFile(string key, ClipboardItem value)
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(this.ClipboardFile) || new FileInfo(this.ClipboardFile).Length <= 0)
            {
                // create new file and prepare to write
                using (StreamWriter streamWriter = new StreamWriter(this.ClipboardFile, false))
                {
                    // write each item's FileChars to the CLIPBOARD file
                    for (int i = this.Keys.Count - 1; i >= 0; i--)
                        streamWriter.Write(this.Dict[this.Keys[i]].FileChars);
                }
            }

            // append the item's FileChars to the CLIPBOARD file
            using (StreamWriter streamWriter = new StreamWriter(this.ClipboardFile, true))
                streamWriter.Write(value.FileChars);

            // add to data structures in the local clipboard
            this.Add(key, value);
        }

        public void Insert(string key, ClipboardItem value, int index)
        {
            this.mainWindow.ListBox.Items.Insert(index, key);
            this.Keys.Insert(index, key);
            this.Dict.Add(key, value);

            // create or overwrite CLIPBOARD file, then write all items' FileChars in the appropriate order
            using (StreamWriter streamWriter = new StreamWriter(this.ClipboardFile, false))
            {
                // write items prior to index to CLIPBOARD file
                for (int i = this.Keys.Count - 1; i > index; i--)
                    streamWriter.Write(this.Dict[this.Keys[i]].FileChars);

                // write current item's FileChars to CLIPBOARD file
                streamWriter.Write(this.Dict[this.Keys[index]].FileChars);

                // write items following index to CLIPBOARD file
                for (int i = index - 1; i >= 0; i--)
                    streamWriter.Write(this.Dict[this.Keys[i]].FileChars);
            }
        }

        public void Remove(string key, ClipboardItem value)
        {
            // String.Replace replaces all instances of a string.  This local
            // function instead replaces only the first instance of the string.
            string ReplaceFirst(string oldText, string strToReplace, string replacement)
            {
                // find index of the first occurrence of strToReplace in oldText
                int pos = oldText.IndexOf(strToReplace);

                // IndexOf returns -1 if string not found
                if (pos == -1)
                {
                    // nothing to replace; return oldText
                    return oldText;
                }

                // replace strToReplace with the replacement string, and store into the return string
                string newText = oldText.Substring(0, pos) + replacement + oldText.Substring(pos + strToReplace.Length);

                return newText;
            }

            // make sure the CLIPBOARD file exists and is non-empty
            FileInfo fileInfo = new FileInfo(this.ClipboardFile);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                // store oldText, the old contents of the CLIPBOARD file
                string oldText = File.ReadAllText(this.ClipboardFile);

                // newText = oldText - this item's FileChars
                string newText = ReplaceFirst(oldText, value.FileChars, "");

                // replace old contents of the CLIPBOARD file with newText
                File.WriteAllText(this.ClipboardFile, newText);
            }

            // remove item by its key
            this.mainWindow.ListBox.Items.Remove(key);
            this.Keys.Remove(key);
            this.Dict.Remove(key);

            // determine if we need to delete any files along with the item
            string folder = null;
            if (value is ImageItem)
                folder = this.ImageFolder;
            else if (value is AudioItem)
                folder = this.AudioFolder;
            else if (value is CustomItem)
                folder = this.CustomFolder;

            // delete the specified file if applicable
            if (folder != null)
            {
                // if folder is missing, there is no file to remove
                if (!Directory.Exists(folder))
                    return;

                // delete file if it exists
                if (File.Exists(Path.Combine(folder, key)))
                    File.Delete(Path.Combine(folder, key));

                // delete the folder if it's empty
                if (Directory.GetFiles(folder).Length == 0)
                    Directory.Delete(folder);
            }
        }

        public void Remove(int index)
        {
            // return if index is invalid
            if (index < 0)
                return;

            // remove the item at the index
            ClipboardItem clipboardItem = this.Dict[this.Keys[index]];
            this.Remove(clipboardItem.KeyText, clipboardItem);

            // if there was an item located after the removed item, select that item
            if (this.mainWindow.ListBox.Items.Count > index)
                this.mainWindow.ListBox.SelectedIndex = index;
            // else select the item located before the removed item
            else
                this.mainWindow.ListBox.SelectedIndex = index - 1;

            // notify the user of the successful operation for 3 seconds
            this.mainWindow.MsgLabel.Normal("Item removed!");
        }

        public void Remove()
        {
            this.Remove(mainWindow.ListBox.SelectedIndex);
        }

        public void Move(string key, ClipboardItem value, int index)
        {
            this.Remove(key, value);
            this.Insert(key, value, index);
        }

        public void Clear()
        {
            // clear each data structure associated with the local clipboard
            this.mainWindow.ListBox.Items.Clear();
            this.Keys.Clear();
            this.Dict.Clear();

            // delete the CLIPBOARD file
            File.Delete(this.ClipboardFile);

            // recursively delete each item folder if it exists
            if (Directory.Exists(this.ImageFolder))
                Directory.Delete(this.ImageFolder, true);
            if (Directory.Exists(this.AudioFolder))
                Directory.Delete(this.AudioFolder, true);
            if (Directory.Exists(this.CustomFolder))
                Directory.Delete(this.CustomFolder, true);

            this.mainWindow.MsgLabel.Normal("All items cleared!");
        }

        //public void OnKeyUp(KeyEventArgs e)
        //{
        //    // vars regarding listbox data
        //    int total = this.mainWindow.ListBox.Items.Count;
        //    int current = this.mainWindow.ListBox.SelectedIndex;

        //    // base case 1: nothing in the clipboard
        //    if (total == 0) return;

        //    // base case 2: no item is selected, or only one item exists
        //    if (current < 0 || total == 1)
        //    {
        //        this.mainWindow.ListBox.SelectedIndex = 0;
        //        return;
        //    }

        //    // base case 3: current index is 0
        //    if (current == 0)
        //    {
        //        // wrap to bottom index if box is checked
        //        if (this.mainWindow.WrapKeys.Checked && !e.Shift)
        //            this.mainWindow.ListBox.SelectedIndex = total - 1;
        //        // else do nothing

        //        return;
        //    }

        //    // base case 4: shift isn't being pressed; move cursor up one index
        //    if (!e.Shift)
        //    {
        //        this.mainWindow.ListBox.SelectedIndex--;
        //        return;
        //    }

        //    // store the item at current index
        //    ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

        //    // set new index based on ctrl being pressed
        //    if (e.Control)
        //        current = 0;
        //    else
        //        current--;

        //    // move item to new index
        //    this.Move(clipboardItem.KeyText, clipboardItem, current);
        //    this.mainWindow.ListBox.SelectedIndex = current;
        //}

        public void OnKeyUp(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = this.mainWindow.ListBox.Items.Count;
            int current = this.mainWindow.ListBox.SelectedIndex;

            // base case 1: nothing in the clipboard
            if (total == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                this.mainWindow.ListBox.SelectedIndex = 0;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

            // store new index for the item and/or index to be moved
            int newIndex;

            // base case 3: current index is 0
            if (current == 0)
            {
                // if box isn't checked, no wrapping or moving is needed
                if (!this.mainWindow.WrapKeys.Checked)
                    return;

                // we will be wrapping to the bottom, so newIndex is set to the bottom index
                newIndex = total - 1;

                // move current item to the bottom index if shift is pressed
                if (e.Shift)
                {
                    this.Move(clipboardItem.KeyText, clipboardItem, newIndex);

                    // wrap selected index to the bottom index only if the box is checked
                    if (this.mainWindow.ChangeTopBottom.Checked)
                        this.mainWindow.ListBox.SelectedIndex = newIndex;
                    else
                        this.mainWindow.ListBox.SelectedIndex = current;
                }
                // else don't move the current item, and wrap the selected index unconditionally
                else
                    this.mainWindow.ListBox.SelectedIndex = newIndex;

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
                if (e.Shift && !this.mainWindow.ChangeTopBottom.Checked)
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
                this.Move(clipboardItem.KeyText, clipboardItem, newIndex);

            // move selected index to new index if bool is satisfied
            if (changeIndex)
                this.mainWindow.ListBox.SelectedIndex = newIndex;
            else
                this.mainWindow.ListBox.SelectedIndex = current;
        }

        //public void OnKeyDown(KeyEventArgs e)
        //{
        //    // vars regarding listbox data
        //    int total = this.mainWindow.ListBox.Items.Count;
        //    int current = this.mainWindow.ListBox.SelectedIndex;

        //    // base case 1: nothing in the clipboard
        //    if (total == 0) return;

        //    // base case 2: no item is selected, or only one item exists
        //    if (current < 0 || total == 1)
        //    {
        //        this.mainWindow.ListBox.SelectedIndex = 0;
        //        return;
        //    }

        //    // base case 3: current is at the last index
        //    if (current == total - 1)
        //    {
        //        // wrap to top index if box is checked
        //        if (this.mainWindow.WrapKeys.Checked && !e.Shift)
        //            this.mainWindow.ListBox.SelectedIndex = 0;
        //        // else do nothing

        //        return;
        //    }

        //    // base case 4: shift isn't being pressed; move cursor down one index
        //    if (!e.Shift)
        //    {
        //        this.mainWindow.ListBox.SelectedIndex++;
        //        return;
        //    }

        //    // store the item at current index
        //    ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

        //    // set new index based on ctrl being pressed
        //    if (e.Control)
        //        current = total - 1;
        //    else
        //        current++;

        //    // move item to new index
        //    this.Move(clipboardItem.KeyText, clipboardItem, current);
        //    this.mainWindow.ListBox.SelectedIndex = current;
        //}

        public void OnKeyDown(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = this.mainWindow.ListBox.Items.Count;
            int current = this.mainWindow.ListBox.SelectedIndex;

            // base case 1: nothing in the clipboard
            if (total == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                this.mainWindow.ListBox.SelectedIndex = 0;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

            // store new index for the item and/or index to be moved
            int newIndex;

            // base case 3: current is at the last index
            if (current == total - 1)
            {
                // if box isn't checked, no wrapping or moving is needed
                if (!this.mainWindow.WrapKeys.Checked)
                    return;

                // we will be wrapping to the top, so newIndex is set to the top index, i.e. 0
                newIndex = 0;

                // move current item to the top index if shift is pressed
                if (e.Shift)
                {
                    this.Move(clipboardItem.KeyText, clipboardItem, newIndex);

                    // wrap selected index to the top index only if the box is checked
                    if (this.mainWindow.ChangeTopBottom.Checked)
                        this.mainWindow.ListBox.SelectedIndex = newIndex;
                    else
                        this.mainWindow.ListBox.SelectedIndex = current;
                }
                // else don't move the current item, and wrap the selected index unconditionally
                else
                    this.mainWindow.ListBox.SelectedIndex = newIndex;

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
                if (e.Shift && !this.mainWindow.ChangeTopBottom.Checked)
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
                this.Move(clipboardItem.KeyText, clipboardItem, newIndex);

            // move selected index to new index if bool is satisfied
            if (changeIndex)
                this.mainWindow.ListBox.SelectedIndex = newIndex;
            else
                this.mainWindow.ListBox.SelectedIndex = current;
        }

        public void Copy()
        {
            // check for valid SelectedIndex val before continuing
            if (this.mainWindow.ListBox.SelectedIndex < 0)
                return;

            // Windows clipboard will be changed in this method; we don't want it to be handled
            this.HandleClipboard = false;

            // store the selected item
            string key = this.Keys[this.mainWindow.ListBox.SelectedIndex];
            ClipboardItem clipboardItem = this.Dict[key];

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
                    if (File.Exists(Path.Combine(this.ImageFolder, clipboardItem.KeyText)))
                    {
                        // store the image from the file
                        Image image = Image.FromFile(Path.Combine(this.ImageFolder, clipboardItem.KeyText));

                        // set image to the Windows clipboard
                        Clipboard.SetImage(image);

                        // dispose the image to free up the file
                        image.Dispose();
                    }
                    else
                        errMsg = "Image file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Audio:
                    if (File.Exists(Path.Combine(this.AudioFolder, clipboardItem.KeyText)))
                    {
                        // store audio stream from the file
                        Stream audio = new MemoryStream(File.ReadAllBytes(Path.Combine
                            (this.AudioFolder, clipboardItem.KeyText)));

                        // set audio to the Windows clipboard
                        Clipboard.SetAudio(audio);

                        // dispose the stream to free up the file
                        audio.Dispose();
                    }
                    else
                        errMsg = "Audio file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Custom:
                    if (File.Exists(Path.Combine(this.CustomFolder, clipboardItem.KeyText)))
                    {
                        // store custom data from the file
                        object data;
                        using (var stream = new FileStream(Path.Combine(this.CustomFolder,
                            clipboardItem.KeyText), FileMode.Open))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            data = formatter.Deserialize(stream);
                        }

                        // set custom data to the Windows clipboard
                        Clipboard.SetData((clipboardItem as CustomItem).WritableFormat, data);
                    }
                    else
                        errMsg = "Custom file is missing!";
                    break;
            }

            // flip to true after algorithm is finished
            this.HandleClipboard = true;

            // notify to the user the results of the operation attempt
            if (errMsg == null)
                this.mainWindow.MsgLabel.Normal("Copied to clipboard!");
            else
                this.mainWindow.MsgLabel.Warn(errMsg);
        }

        public void FromFile()
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(this.ClipboardFile) || new FileInfo(this.ClipboardFile).Length <= 0)
                return;

            // clipboardString = old contents of the CLIPBOARD file
            string clipboardString = File.ReadAllText(this.ClipboardFile);

            //// if any data files are missing, this temp CLIPBOARD file will be updated
            //File.WriteAllText(this.TempFile, clipboardString);

            // backup file contents = clipboardString
            File.WriteAllText(this.BackupFile, clipboardString);

            // traverse clipboardString to make new instances of 
            // ClipboardItem's derived classes
            StringReader stringReader = new StringReader(clipboardString);
            using (stringReader)
            {
                // peek returns -1 if no more characters are available
                while (stringReader.Peek() != -1)
                {
                    // Type
                    ClipboardItem.TypeEnum type = (ClipboardItem.TypeEnum)stringReader.Read();

                    // KeyDiff
                    ushort keyDiff = ushort.Parse(stringReader.ReadLine());

                    // remaining operations depend on the type of item
                    switch (type)
                    {
                        case ClipboardItem.TypeEnum.Text:
                            _ = new TextItem(this.mainWindow, keyDiff, stringReader);
                            break;

                        case ClipboardItem.TypeEnum.FileDropList:
                            _ = new FileItem(this.mainWindow, keyDiff, stringReader);
                            break;

                        case ClipboardItem.TypeEnum.Image:
                            _ = new ImageItem(this.mainWindow, keyDiff, stringReader);
                            break;

                        case ClipboardItem.TypeEnum.Audio:
                            _ = new AudioItem(this.mainWindow, keyDiff, stringReader);
                            break;

                        case ClipboardItem.TypeEnum.Custom:
                            _ = new CustomItem(this.mainWindow, keyDiff, stringReader);
                            break;
                    }
                }
            }

            //// store CLIPBOARD file contents into backup file, then replace with temp file
            ////File.Delete(this.ClipboardFile);
            ////File.Move(this.TempFile, this.ClipboardFile);
            //File.Replace(this.TempFile, this.ClipboardFile, this.BackupFile);
        }
    }
}
