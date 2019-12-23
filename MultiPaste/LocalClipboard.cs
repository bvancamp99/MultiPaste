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

        private static ListBox myListBox; // store listbox that contains all copied items

        private static Dictionary<string, ClipboardItem> myDict; // dict used for fast access of clipboard data
        private static StringCollection myKeys; // collection used to store keys to clipboard items
        private static bool handleClipboard; // bool to determine if we should handle clipboard changes

        private static string clipboardFile; // clipboard file directory
        private static string tempFile; // temp clipboard file, used when adding items from file on startup
        private static string imageFolder; // image folder directory
        private static string audioFolder; // audio folder directory
        private static string customFolder; // custom folder directory

        public LocalClipboard(ListBox myListBox)
        {
            LocalClipboard.myListBox = myListBox;

            LocalClipboard.myDict = new Dictionary<string, ClipboardItem>();
            LocalClipboard.myKeys = new StringCollection();
            LocalClipboard.handleClipboard = true;

            LocalClipboard.clipboardFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD");
            LocalClipboard.tempFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            LocalClipboard.imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            LocalClipboard.audioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
            LocalClipboard.customFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom");

            // read from CLIPBOARD file and write to local clipboard
            this.FromFile();
        }

        public static ListBox GetListBox()
        {
            return LocalClipboard.myListBox;
        }

        public static Dictionary<string, ClipboardItem> GetDict()
        {
            return LocalClipboard.myDict;
        }

        public static StringCollection GetKeys()
        {
            return LocalClipboard.myKeys;
        }

        public static bool GetHandleClipboard()
        {
            return LocalClipboard.handleClipboard;
        }

        public static string GetClipboardFile()
        {
            return LocalClipboard.clipboardFile;
        }

        public static string GetTempFile()
        {
            return LocalClipboard.tempFile;
        }

        public static string GetImageFolder()
        {
            return LocalClipboard.imageFolder;
        }

        public static string GetAudioFolder()
        {
            return LocalClipboard.audioFolder;
        }

        public static string GetCustomFolder()
        {
            return LocalClipboard.customFolder;
        }

        public static void OnClipboardChange()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                TextItem myItem = new TextItem(text);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection files = Clipboard.GetFileDropList();
                FileItem myItem = new FileItem(files);
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                ImageItem myItem = new ImageItem(image);
            }
            else if (Clipboard.ContainsAudio())
            {
                Stream audio = Clipboard.GetAudioStream();
                AudioItem myItem = new AudioItem(audio);
            }
            else
            {
                IDataObject data = Clipboard.GetDataObject();
                CustomItem myItem = new CustomItem(data);
            }
        }

        public static void Focus()
        {
            LocalClipboard.myListBox.Focus();
        }

        public static void Add(string key, ClipboardItem value)
        {
            LocalClipboard.myListBox.Items.Insert(0, key);
            LocalClipboard.myKeys.Insert(0, key);
            LocalClipboard.myDict.Add(key, value);
        }

        public static void AddWithFile(string key, ClipboardItem value)
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(LocalClipboard.clipboardFile) || new FileInfo(LocalClipboard.clipboardFile).Length <= 0)
            {
                // create new file and prepare to write
                using (StreamWriter streamWriter = new StreamWriter(LocalClipboard.clipboardFile, false))
                {
                    // write each item's FileChars to the CLIPBOARD file
                    for (int i = LocalClipboard.myKeys.Count - 1; i >= 0; i--)
                        streamWriter.Write(LocalClipboard.myDict[LocalClipboard.myKeys[i]].FileChars);
                }
            }

            // add to data structures in the local clipboard
            LocalClipboard.Add(key, value);

            // append the item's FileChars to the CLIPBOARD file
            using (StreamWriter streamWriter = new StreamWriter(LocalClipboard.clipboardFile, true))
                streamWriter.Write(value.FileChars);
        }

        public static void Insert(string key, ClipboardItem value, int index)
        {
            LocalClipboard.myListBox.Items.Insert(index, key);
            LocalClipboard.myKeys.Insert(index, key);
            LocalClipboard.myDict.Add(key, value);

            // create or overwrite CLIPBOARD file, then write all items' FileChars in the appropriate order
            using (StreamWriter streamWriter = new StreamWriter(LocalClipboard.clipboardFile, false))
            {
                // write items prior to index to CLIPBOARD file
                for (int i = LocalClipboard.myKeys.Count - 1; i > index; i--)
                    streamWriter.Write(LocalClipboard.myDict[LocalClipboard.myKeys[i]].FileChars);

                // write current item's FileChars to CLIPBOARD file
                streamWriter.Write(LocalClipboard.myDict[LocalClipboard.myKeys[index]].FileChars);

                // write items following index to CLIPBOARD file
                for (int i = index - 1; i >= 0; i--)
                    streamWriter.Write(LocalClipboard.myDict[LocalClipboard.myKeys[i]].FileChars);
            }
        }

        public static void Remove(string key, ClipboardItem value)
        {
            // if CLIPBOARD file exists and isn't empty, CLIPBOARD file -= this item's FileChars
            if (File.Exists(LocalClipboard.clipboardFile) && new FileInfo(LocalClipboard.clipboardFile).Length > 0)
            {
                string newText = File.ReadAllText(LocalClipboard.clipboardFile).Replace(value.FileChars, "");
                File.WriteAllText(LocalClipboard.clipboardFile, newText);
            }

            // remove item by its key
            LocalClipboard.myListBox.Items.Remove(key);
            LocalClipboard.myKeys.Remove(key);
            LocalClipboard.myDict.Remove(key);

            // determine if we need to delete any files along with the item
            string folder = null;
            if (value is ImageItem)
                folder = LocalClipboard.imageFolder;
            else if (value is AudioItem)
                folder = LocalClipboard.audioFolder;
            else if (value is CustomItem)
                folder = LocalClipboard.customFolder;

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

        public static void Remove(int index)
        {
            // return if index is invalid
            if (index < 0)
                return;

            // remove the item at the index
            ClipboardItem clipboardItem = LocalClipboard.myDict[LocalClipboard.myKeys[index]];
            LocalClipboard.Remove(clipboardItem.KeyText, clipboardItem);

            // if there was an item located after the removed item, select that item
            if (LocalClipboard.myListBox.Items.Count > index)
                LocalClipboard.myListBox.SelectedIndex = index;
            // else select the item located before the removed item
            else
                LocalClipboard.myListBox.SelectedIndex = index - 1;

            // notify the user of the successful operation for 3 seconds
            MsgLabel.Normal("Item removed!");
        }

        public static void Remove()
        {
            LocalClipboard.Remove(LocalClipboard.myListBox.SelectedIndex);
        }

        public static void Move(string key, ClipboardItem value, int index)
        {
            LocalClipboard.Remove(key, value);
            LocalClipboard.Insert(key, value, index);
        }

        public static void Clear()
        {
            // clear each data structure associated with the local clipboard
            LocalClipboard.myListBox.Items.Clear();
            LocalClipboard.myKeys.Clear();
            LocalClipboard.myDict.Clear();

            // delete the CLIPBOARD file
            File.Delete(LocalClipboard.clipboardFile);

            // recursively delete each item folder if it exists
            if (Directory.Exists(LocalClipboard.imageFolder))
                Directory.Delete(LocalClipboard.imageFolder, true);
            if (Directory.Exists(LocalClipboard.audioFolder))
                Directory.Delete(LocalClipboard.audioFolder, true);
            if (Directory.Exists(LocalClipboard.customFolder))
                Directory.Delete(LocalClipboard.customFolder, true);

            MsgLabel.Normal("All items cleared!");
        }

        public static void OnKeyUp(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = LocalClipboard.myListBox.Items.Count;
            int current = LocalClipboard.myListBox.SelectedIndex;

            // base case 1: nothing in the clipboard, or current index is 0
            if (total == 0 || current == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                LocalClipboard.myListBox.SelectedIndex = 0;
                return;
            }

            // base case 3: shift isn't being pressed; move cursor up one index
            if (!e.Shift)
            {
                LocalClipboard.myListBox.SelectedIndex--;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = LocalClipboard.myDict[LocalClipboard.myKeys[current]];

            // set new index based on ctrl being pressed
            if (e.Control)
                current = 0;
            else
                current--;

            // move item to new index
            LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, current);
            LocalClipboard.myListBox.SelectedIndex = current;
        }

        public static void OnKeyDown(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = LocalClipboard.myListBox.Items.Count;
            int current = LocalClipboard.myListBox.SelectedIndex;

            // base case 1: nothing in the clipboard, or current is at the last index
            if (total == 0 || current == total - 1) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                LocalClipboard.myListBox.SelectedIndex = 0;
                return;
            }

            // base case 3: shift isn't being pressed; move cursor down one index
            if (!e.Shift)
            {
                LocalClipboard.myListBox.SelectedIndex++;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = LocalClipboard.myDict[LocalClipboard.myKeys[current]];

            // set new index based on ctrl being pressed
            if (e.Control)
                current = total - 1;
            else
                current++;

            // move item to new index
            LocalClipboard.Move(clipboardItem.KeyText, clipboardItem, current);
            LocalClipboard.myListBox.SelectedIndex = current;
        }

        public static void OnTypeRestrictionClick(ToolStripMenuItem text, ToolStripMenuItem files, 
            ToolStripMenuItem images, ToolStripMenuItem audio, ToolStripMenuItem custom)
        {
            // first, clear the listbox
            LocalClipboard.myListBox.Items.Clear();

            // add each item back to the listbox if its type is allowed
            bool allow;
            foreach (string key in LocalClipboard.myKeys)
            {
                allow = (text.Checked && LocalClipboard.myDict[key].Type == ClipboardItem.TypeEnum.Text)
                    || (files.Checked && LocalClipboard.myDict[key].Type == ClipboardItem.TypeEnum.FileDropList)
                    || (images.Checked && LocalClipboard.myDict[key].Type == ClipboardItem.TypeEnum.Image)
                    || (audio.Checked && LocalClipboard.myDict[key].Type == ClipboardItem.TypeEnum.Audio)
                    || (custom.Checked && LocalClipboard.myDict[key].Type == ClipboardItem.TypeEnum.Custom);

                // if type is allowed, add to the listbox
                if (allow)
                    LocalClipboard.myListBox.Items.Add(key);
            }
        }

        public static void Copy()
        {
            // check for valid SelectedIndex val before continuing
            if (LocalClipboard.myListBox.SelectedIndex < 0)
                return;

            // clipboard will be changed in this method; we don't want it to be handled
            LocalClipboard.handleClipboard = false;

            // store the selected item
            string key = LocalClipboard.myKeys[LocalClipboard.myListBox.SelectedIndex];
            ClipboardItem clipboardItem = LocalClipboard.myDict[key];

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
                    if (File.Exists(Path.Combine(LocalClipboard.imageFolder, clipboardItem.KeyText)))
                    {
                        // store the image from the file
                        Image image = Image.FromFile(Path.Combine(LocalClipboard.imageFolder, clipboardItem.KeyText));

                        // set image to the Windows clipboard
                        Clipboard.SetImage(image);

                        // dispose the image to free up the file
                        image.Dispose();
                    }
                    else
                        errMsg = "Image file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Audio:
                    if (File.Exists(Path.Combine(LocalClipboard.audioFolder, clipboardItem.KeyText)))
                    {
                        // store audio stream from the file
                        Stream audio = new MemoryStream(File.ReadAllBytes(Path.Combine
                            (LocalClipboard.audioFolder, clipboardItem.KeyText)));

                        // set audio to the Windows clipboard
                        Clipboard.SetAudio(audio);

                        // dispose the stream to free up the file
                        audio.Dispose();
                    }
                    else
                        errMsg = "Audio file is missing!";
                    break;

                case ClipboardItem.TypeEnum.Custom:
                    if (File.Exists(Path.Combine(LocalClipboard.customFolder, clipboardItem.KeyText)))
                    {
                        // store custom data from the file
                        object data;
                        using (var stream = new FileStream(Path.Combine(LocalClipboard.customFolder,
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
            LocalClipboard.handleClipboard = true;

            // notify to the user the results of the operation attempt
            if (errMsg == null)
                MsgLabel.Normal("Copied to clipboard!");
            else
                MsgLabel.Warn(errMsg);
        }

        private void FromFile()
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(LocalClipboard.clipboardFile) || new FileInfo(LocalClipboard.clipboardFile).Length <= 0)
                return;

            // if any data files are missing, this temp CLIPBOARD file will be updated
            File.WriteAllText(LocalClipboard.tempFile, File.ReadAllText(LocalClipboard.clipboardFile));

            // traverse and store the contents of the file into new instances of ClipboardItem's derived classes
            using (StreamReader streamReader = new StreamReader(LocalClipboard.clipboardFile))
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
                            TextItem myTextItem = new TextItem(keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.FileDropList:
                            FileItem myFileItem = new FileItem(keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Image:
                            ImageItem myImageItem = new ImageItem(keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Audio:
                            AudioItem myAudioItem = new AudioItem(keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Custom:
                            CustomItem myCustomItem = new CustomItem(keyDiff, streamReader);
                            break;
                    }
                }
            }

            // all items have been read from the file and added; replace CLIPBOARD file with the temp file
            File.Delete(LocalClipboard.clipboardFile);
            File.Move(LocalClipboard.tempFile, LocalClipboard.clipboardFile);
        }
    }
}
