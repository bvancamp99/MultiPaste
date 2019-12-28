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

        //private static ListBox myListBox; // store listbox that contains all copied items

        private readonly MainWindow mainWindow; // store MainWindow instance to access its variables

        //private static Dictionary<string, ClipboardItem> myDict; // dict used for fast access of clipboard data
        //private static StringCollection myKeys; // collection used to store keys to clipboard items
        //private static bool handleClipboard; // bool to determine if we should handle clipboard changes

        //private static string clipboardFile; // clipboard file directory
        //private static string tempFile; // temp clipboard file, used when adding items from file on startup
        //private static string imageFolder; // image folder directory
        //private static string audioFolder; // audio folder directory
        //private static string customFolder; // custom folder directory

        //public MyClipboard(ListBox myListBox)
        //{
        //    MyClipboard.myListBox = myListBox;

        //    MyClipboard.myDict = new Dictionary<string, ClipboardItem>();
        //    MyClipboard.myKeys = new StringCollection();
        //    MyClipboard.handleClipboard = true;

        //    MyClipboard.clipboardFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD");
        //    MyClipboard.tempFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        //    MyClipboard.imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        //    MyClipboard.audioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
        //    MyClipboard.customFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom");

        //    // read from CLIPBOARD file and write to local clipboard
        //    this.FromFile();
        //}

        public LocalClipboard(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            this.Dict = new Dictionary<string, ClipboardItem>();
            this.Keys = new StringCollection();
            this.HandleClipboard = true;

            this.ClipboardFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD");
            this.TempFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
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
        /// temp clipboard file, used when adding items from file on startup
        /// </summary>
        public string TempFile { get; }

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

        //public ListBox GetListBox()
        //{
        //    return MyClipboard.myListBox;
        //}

        //// TODO: remove after figuring out Config.cs
        //public static Dictionary<string, ClipboardItem> GetDict()
        //{
        //    return this.Dict;
        //}

        //// TODO: remove after figuring out Config.cs
        //public static StringCollection GetKeys()
        //{
        //    return this.Keys;
        //}

        //public static bool GetHandleClipboard()
        //{
        //    return LocalClipboard.handleClipboard;
        //}

        //public static string GetClipboardFile()
        //{
        //    return this.ClipboardFile;
        //}

        //public static string GetTempFile()
        //{
        //    return this.TempFile;
        //}

        //public static string GetImageFolder()
        //{
        //    return this.ImageFolder;
        //}

        //public static string GetAudioFolder()
        //{
        //    return this.AudioFolder;
        //}

        //public static string GetCustomFolder()
        //{
        //    return this.CustomFolder;
        //}

        public void OnClipboardChange()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                TextItem myItem = new TextItem(this.mainWindow, text);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection files = Clipboard.GetFileDropList();
                FileItem myItem = new FileItem(this.mainWindow, files);
            }
            else if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                ImageItem myItem = new ImageItem(this.mainWindow, image);
            }
            else if (Clipboard.ContainsAudio())
            {
                Stream audio = Clipboard.GetAudioStream();
                AudioItem myItem = new AudioItem(this.mainWindow, audio);
            }
            else
            {
                IDataObject data = Clipboard.GetDataObject();
                CustomItem myItem = new CustomItem(this.mainWindow, data);
            }
        }

        public void RestrictTypes()
        {
            // first, clear the visual part of the local clipboard
            ListBox myListBox = this.mainWindow.ListBox;
            myListBox.Items.Clear();

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
                    myListBox.Items.Add(key);
            }
        }

        public void Focus()
        {
            this.mainWindow.ListBox.Focus();
        }

        public void Add(string key, ClipboardItem value)
        {
            mainWindow.ListBox.Items.Insert(0, key);
            this.Keys.Insert(0, key);
            this.Dict.Add(key, value);
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

            // add to data structures in the local clipboard
            this.Add(key, value);

            // append the item's FileChars to the CLIPBOARD file
            using (StreamWriter streamWriter = new StreamWriter(this.ClipboardFile, true))
                streamWriter.Write(value.FileChars);
        }

        public void Insert(string key, ClipboardItem value, int index)
        {
            mainWindow.ListBox.Items.Insert(index, key);
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
            // if CLIPBOARD file exists and isn't empty, CLIPBOARD file -= this item's FileChars
            if (File.Exists(this.ClipboardFile) && new FileInfo(this.ClipboardFile).Length > 0)
            {
                string newText = File.ReadAllText(this.ClipboardFile).Replace(value.FileChars, "");
                File.WriteAllText(this.ClipboardFile, newText);
            }

            // remove item by its key
            mainWindow.ListBox.Items.Remove(key);
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
            if (mainWindow.ListBox.Items.Count > index)
                mainWindow.ListBox.SelectedIndex = index;
            // else select the item located before the removed item
            else
                mainWindow.ListBox.SelectedIndex = index - 1;

            // notify the user of the successful operation for 3 seconds
            MsgLabel.Normal("Item removed!");
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
            mainWindow.ListBox.Items.Clear();
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

            MsgLabel.Normal("All items cleared!");
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = mainWindow.ListBox.Items.Count;
            int current = mainWindow.ListBox.SelectedIndex;

            // base case 1: nothing in the clipboard, or current index is 0
            if (total == 0 || current == 0) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                mainWindow.ListBox.SelectedIndex = 0;
                return;
            }

            // base case 3: shift isn't being pressed; move cursor up one index
            if (!e.Shift)
            {
                mainWindow.ListBox.SelectedIndex--;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

            // set new index based on ctrl being pressed
            if (e.Control)
                current = 0;
            else
                current--;

            // move item to new index
            this.Move(clipboardItem.KeyText, clipboardItem, current);
            mainWindow.ListBox.SelectedIndex = current;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            // vars regarding listbox data
            int total = mainWindow.ListBox.Items.Count;
            int current = mainWindow.ListBox.SelectedIndex;

            // base case 1: nothing in the clipboard, or current is at the last index
            if (total == 0 || current == total - 1) return;

            // base case 2: no item is selected, or only one item exists
            if (current < 0 || total == 1)
            {
                mainWindow.ListBox.SelectedIndex = 0;
                return;
            }

            // base case 3: shift isn't being pressed; move cursor down one index
            if (!e.Shift)
            {
                mainWindow.ListBox.SelectedIndex++;
                return;
            }

            // store the item at current index
            ClipboardItem clipboardItem = this.Dict[this.Keys[current]];

            // set new index based on ctrl being pressed
            if (e.Control)
                current = total - 1;
            else
                current++;

            // move item to new index
            this.Move(clipboardItem.KeyText, clipboardItem, current);
            mainWindow.ListBox.SelectedIndex = current;
        }

        public void Copy()
        {
            // check for valid SelectedIndex val before continuing
            if (mainWindow.ListBox.SelectedIndex < 0)
                return;

            // Windows clipboard will be changed in this method; we don't want it to be handled
            this.HandleClipboard = false;

            // store the selected item
            string key = this.Keys[mainWindow.ListBox.SelectedIndex];
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
            this.HandleClipboard = true;

            // notify to the user the results of the operation attempt
            if (errMsg == null)
                MsgLabel.Normal("Copied to clipboard!");
            else
                MsgLabel.Warn(errMsg);
        }

        public void FromFile()
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(this.ClipboardFile) || new FileInfo(this.ClipboardFile).Length <= 0)
                return;

            // if any data files are missing, this temp CLIPBOARD file will be updated
            File.WriteAllText(this.TempFile, File.ReadAllText(this.ClipboardFile));

            // traverse and store the contents of the file into new instances of ClipboardItem's derived classes
            using (StreamReader streamReader = new StreamReader(this.ClipboardFile))
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
                            TextItem myTextItem = new TextItem(this.mainWindow, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.FileDropList:
                            FileItem myFileItem = new FileItem(this.mainWindow, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Image:
                            ImageItem myImageItem = new ImageItem(this.mainWindow, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Audio:
                            AudioItem myAudioItem = new AudioItem(this.mainWindow, keyDiff, streamReader);
                            break;

                        case ClipboardItem.TypeEnum.Custom:
                            CustomItem myCustomItem = new CustomItem(this.mainWindow, keyDiff, streamReader);
                            break;
                    }
                }
            }

            // all items have been read from the file and added; replace CLIPBOARD file with the temp file
            File.Delete(this.ClipboardFile);
            File.Move(this.TempFile, this.ClipboardFile);
        }
    }
}
