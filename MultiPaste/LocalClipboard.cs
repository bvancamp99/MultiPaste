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

    /// <summary>
    /// This static class is the driver of the clipboard history function.
    /// </summary>
    static class LocalClipboard
    {
        public const int CHAR_LIMIT = 100; // store max number of characters that can be displayed as a key in the dictionary

        /// <summary>
        /// This static constructor initializes the static variables that 
        /// don't rely on MainWindow.
        /// 
        /// This includes LocalClipboard's data structures, HandleClipboard 
        /// bool, and file/directory information.
        /// </summary>
        static LocalClipboard()
        {
            // init data structures used to access ClipboardItem information
            LocalClipboard.Dict = new Dictionary<string, ClipboardItem>();
            LocalClipboard.Keys = new StringCollection();

            // by default, we should handle clipboard changes
            LocalClipboard.HandleClipboard = true;

            // init readonly properties denoting directories of files/folders
            LocalClipboard.ClipboardFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD.mp"));
            LocalClipboard.BackupFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD.bak"));
            LocalClipboard.ImageFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"));
            LocalClipboard.AudioFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio"));
            LocalClipboard.CustomFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom"));
        }

        /// <summary>
        /// store MainWindow to access its variables
        /// </summary>
        public static MainWindow MainWindow { private get; set; }

        /// <summary>
        /// dict used for fast access of clipboard data
        /// </summary>
        public static Dictionary<string, ClipboardItem> Dict { get; }

        /// <summary>
        /// collection used to store keys to clipboard items
        /// </summary>
        public static StringCollection Keys { get; }

        /// <summary>
        /// bool to determine if we should handle changes to the clipboard
        /// </summary>
        public static bool HandleClipboard { get; private set; }

        /// <summary>
        /// clipboard file directory
        /// </summary>
        public static FileInfo ClipboardFile { get; }

        /// <summary>
        /// directory for clipboard file backup
        /// </summary>
        public static FileInfo BackupFile { get; }

        /// <summary>
        /// image folder directory
        /// </summary>
        public static DirectoryInfo ImageFolder { get; }

        /// <summary>
        /// audio folder directory
        /// </summary>
        public static DirectoryInfo AudioFolder { get; }

        /// <summary>
        /// custom folder directory
        /// </summary>
        public static DirectoryInfo CustomFolder { get; }

        public static void Focus()
        {
            LocalClipboard.MainWindow.ListBox.Focus();
        }

        public static void AddWithFile(string key, ClipboardItem value)
        {
            // if CLIPBOARD file is missing or empty
            if (!LocalClipboard.ClipboardFile.Exists || LocalClipboard.ClipboardFile.Length == 0)
            {
                // create new file and prepare to write
                using (StreamWriter sw = LocalClipboard.ClipboardFile.CreateText())
                {
                    // write each item's FileChars to the CLIPBOARD file
                    for (int i = LocalClipboard.Keys.Count - 1; i >= 0; i--)
                        sw.Write(LocalClipboard.Dict[LocalClipboard.Keys[i]].FileChars);
                }
            }

            // append the item's FileChars to the CLIPBOARD file
            using (StreamWriter sw = LocalClipboard.ClipboardFile.AppendText())
            {
                sw.Write(value.FileChars);
            }

            // add to data structures in the local clipboard
            LocalClipboard.Add(key, value);
        }

        public static void Add(string key, ClipboardItem value)
        {
            try
            {
                // add to the back-end dictionary
                LocalClipboard.Dict.Add(key, value);
            }
            // exception thrown on attempt to add a duplicate key to the dict
            catch (ArgumentException)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = LocalClipboard.Dict[key];

                // remove old item and prepare to replace with new item
                LocalClipboard.Remove(duplicateKeyItem.KeyText, duplicateKeyItem);

                // add new item
                LocalClipboard.Dict.Add(key, value);
            }

            // add to back-end string collection of keys
            LocalClipboard.Keys.Insert(0, key);

            // add to visual clipboard last
            LocalClipboard.MainWindow.ListBox.Items.Insert(0, key);
        }

        /// <summary>
        /// String.Replace replaces all instances of a string.  This method
        /// instead replaces only the first instance of a string.
        /// 
        /// This method is used to resolve duplicate key conflicts by replacing
        /// the first occurrence of strToReplace while preserving the most 
        /// recent (2nd) occurrence of strToReplace in the CLIPBOARD file.
        /// </summary>
        /// <param name="oldText"></param>
        /// <param name="strToReplace"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private static string ReplaceFirst(string oldText, string strToReplace, string replacement)
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

        public static void RemoveFromFile(string fileChars)
        {
            // base case: CLIPBOARD file is missing or empty
            if (!LocalClipboard.ClipboardFile.Exists || LocalClipboard.ClipboardFile.Length == 0)
                return;

            // store oldText, the old contents of the CLIPBOARD file
            string oldText;
            using (StreamReader sr = LocalClipboard.ClipboardFile.OpenText())
            {
                oldText = sr.ReadToEnd();
            }

            // newText = oldText - this item's FileChars
            string newText = LocalClipboard.ReplaceFirst(oldText, fileChars, "");

            // replace old contents of the CLIPBOARD file with newText
            using (StreamWriter sw = LocalClipboard.ClipboardFile.CreateText())
            {
                sw.Write(newText);
            }
        }

        public static void Remove(string key, ClipboardItem value)
        {
            // first, remove the item's FileChars from the CLIPBOARD file
            LocalClipboard.RemoveFromFile(value.FileChars);

            // remove item by its key
            LocalClipboard.MainWindow.ListBox.Items.Remove(key);
            LocalClipboard.Keys.Remove(key);
            LocalClipboard.Dict.Remove(key);

            // determine if we need to delete any files along with the item
            DirectoryInfo folder = null;
            if (value is ImageItem)
            {
                folder = LocalClipboard.ImageFolder;
            }
            else if (value is AudioItem)
            {
                folder = LocalClipboard.AudioFolder;
            }
            else if (value is CustomItem)
            {
                folder = LocalClipboard.CustomFolder;
            }

            // nothing to do if folder is null or missing
            if (folder == null || !folder.Exists)
                return;

            // delete image/audio/custom file if it exists
            FileInfo fileToDelete = new FileInfo(Path.Combine(folder.FullName, key));
            if (fileToDelete.Exists)
            {
                fileToDelete.Delete();
            }

            // delete the folder if it's empty, i.e. contains 0 files
            FileInfo[] fileInfos = folder.GetFiles();
            if (fileInfos.Length == 0)
            {
                folder.Delete();
            }
        }

        public static void Remove(int index)
        {
            // return if index is invalid
            if (index < 0)
                return;

            // remove the item at the index
            ClipboardItem clipboardItem = LocalClipboard.Dict[LocalClipboard.Keys[index]];
            LocalClipboard.Remove(clipboardItem.KeyText, clipboardItem);

            // if there was an item located after the removed item, select that item
            if (LocalClipboard.MainWindow.ListBox.Items.Count > index)
            {
                LocalClipboard.MainWindow.ListBox.SelectedIndex = index;
            }
            // else select the item located before the removed item
            else
            {
                LocalClipboard.MainWindow.ListBox.SelectedIndex = index - 1;
            }

            // notify the user of the successful operation for 3 seconds
            MsgLabel.Normal("Item removed!");
        }

        public static void Remove()
        {
            LocalClipboard.Remove(MainWindow.ListBox.SelectedIndex);
        }

        public static void Insert(string key, ClipboardItem value, int index)
        {
            LocalClipboard.MainWindow.ListBox.Items.Insert(index, key);
            LocalClipboard.Keys.Insert(index, key);
            LocalClipboard.Dict.Add(key, value);

            // create or overwrite CLIPBOARD file, then write all items' FileChars in the appropriate order
            using (StreamWriter sw = LocalClipboard.ClipboardFile.CreateText())
            {
                // write items prior to index to CLIPBOARD file
                for (int i = LocalClipboard.Keys.Count - 1; i > index; i--)
                    sw.Write(LocalClipboard.Dict[LocalClipboard.Keys[i]].FileChars);

                // write current item's FileChars to CLIPBOARD file
                sw.Write(LocalClipboard.Dict[LocalClipboard.Keys[index]].FileChars);

                // write items following index to CLIPBOARD file
                for (int i = index - 1; i >= 0; i--)
                    sw.Write(LocalClipboard.Dict[LocalClipboard.Keys[i]].FileChars);
            }
        }

        public static void Move(string key, ClipboardItem value, int index)
        {
            LocalClipboard.Remove(key, value);
            LocalClipboard.Insert(key, value, index);
        }

        /// <summary>
        /// This method clears the structures, files, and folders involved in
        /// the local clipboard.
        /// 
        /// Note that the backup file is preserved in case the clear operation
        /// was accidental.
        /// </summary>
        public static void Clear()
        {
            // clear each data structure associated with the local clipboard
            LocalClipboard.MainWindow.ListBox.Items.Clear();
            LocalClipboard.Keys.Clear();
            LocalClipboard.Dict.Clear();

            // delete the CLIPBOARD file
            LocalClipboard.ClipboardFile.Delete();

            // recursively delete each item folder if it exists
            if (LocalClipboard.ImageFolder.Exists)
                LocalClipboard.ImageFolder.Delete(true);
            if (LocalClipboard.AudioFolder.Exists)
                LocalClipboard.AudioFolder.Delete(true);
            if (LocalClipboard.CustomFolder.Exists)
                LocalClipboard.CustomFolder.Delete(true);

            MsgLabel.Normal("All items cleared!");
        }

        public static void Copy()
        {
            // check for valid SelectedIndex val before continuing
            if (LocalClipboard.MainWindow.ListBox.SelectedIndex < 0)
                return;

            // Windows clipboard will be changed in this method; we don't want it to be handled
            LocalClipboard.HandleClipboard = false;

            // store the selected item
            string key = LocalClipboard.Keys[LocalClipboard.MainWindow.ListBox.SelectedIndex];
            ClipboardItem clipboardItem = LocalClipboard.Dict[key];

            // store an error msg string that will notify the user if an error occurred
            string errMsg = null;

            // what's copied to the clipboard depends on the item's type
            switch (clipboardItem.Type)
            {
                case ClipboardItem.TypeEnum.Text:
                    // copy the item's text to the Windows clipboard
                    Clipboard.SetText((clipboardItem as TextItem).Text);

                    break;

                case ClipboardItem.TypeEnum.FileDropList:
                    // copy the item's files to the Windows clipboard
                    Clipboard.SetFileDropList((clipboardItem as FileItem).FileDropList);

                    break;

                case ClipboardItem.TypeEnum.Image:
                    // make sure the image file exists
                    FileInfo imageFile = new FileInfo(Path.Combine(LocalClipboard.ImageFolder.FullName, clipboardItem.KeyText));
                    if (imageFile.Exists)
                    {
                        // get image from the file
                        using (Image image = Image.FromFile(imageFile.FullName))
                        {
                            // set image to the Windows clipboard
                            Clipboard.SetImage(image);
                        }
                    }
                    // else set errMsg to be reported
                    else
                    {
                        errMsg = "Image file is missing!";
                    }

                    break;

                case ClipboardItem.TypeEnum.Audio:
                    // make sure the audio file exists
                    FileInfo audioFile = new FileInfo(Path.Combine(LocalClipboard.AudioFolder.FullName, clipboardItem.KeyText));
                    if (audioFile.Exists)
                    {
                        // get audio stream from the file
                        Stream audioStream = new MemoryStream();
                        using (FileStream fs = audioFile.OpenRead())
                        {
                            fs.CopyTo(audioStream);
                        }

                        using (audioStream)
                        {
                            // set audio to the Windows clipboard
                            Clipboard.SetAudio(audioStream);
                        }
                    }
                    // else set errMsg to be reported
                    else
                    {
                        errMsg = "Audio file is missing!";
                    }

                    break;

                case ClipboardItem.TypeEnum.Custom:
                    // make sure the custom file exists
                    FileInfo customFile = new FileInfo(Path.Combine(LocalClipboard.CustomFolder.FullName, clipboardItem.KeyText));
                    if (customFile.Exists)
                    {
                        // store custom data from the serializable file
                        object data;
                        using (FileStream customStream = customFile.Open(FileMode.Open))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            data = formatter.Deserialize(customStream);
                        }

                        // set custom data to the Windows clipboard
                        Clipboard.SetData((clipboardItem as CustomItem).WritableFormat, data);
                    }
                    // else set errMsg to be reported
                    else
                    {
                        errMsg = "Custom file is missing!";
                    }

                    break;
            }

            // flip to true after algorithm is finished
            LocalClipboard.HandleClipboard = true;

            // notify to the user the results of the operation attempt
            if (errMsg == null)
            {
                MsgLabel.Normal("Copied to clipboard!");
            }
            else
            {
                MsgLabel.Warn(errMsg);
            }
        }

        public static void FromFile()
        {
            // if CLIPBOARD file is missing or empty
            if (!LocalClipboard.ClipboardFile.Exists || LocalClipboard.ClipboardFile.Length == 0)
                return;

            // clipboardString = old contents of the CLIPBOARD file
            string clipboardString;
            using (StreamReader sr = LocalClipboard.ClipboardFile.OpenText())
            {
                clipboardString = sr.ReadToEnd();
            }

            // backup file contents = clipboardString
            using (StreamWriter sw = LocalClipboard.BackupFile.CreateText())
            {
                sw.Write(clipboardString);
            }

            // traverse clipboardString via StringReader
            using (StringReader strRead = new StringReader(clipboardString))
            {
                // peek returns -1 if no more characters are available
                while (strRead.Peek() != -1)
                {
                    // Type
                    ClipboardItem.TypeEnum type = (ClipboardItem.TypeEnum)strRead.Read();

                    // KeyDiff
                    ushort keyDiff = ushort.Parse(strRead.ReadLine());

                    // remaining operations depend on the type of item
                    switch (type)
                    {
                        case ClipboardItem.TypeEnum.Text:
                            _ = new TextItem(LocalClipboard.MainWindow, keyDiff, strRead);

                            break;

                        case ClipboardItem.TypeEnum.FileDropList:
                            _ = new FileItem(LocalClipboard.MainWindow, keyDiff, strRead);

                            break;

                        case ClipboardItem.TypeEnum.Image:
                            _ = new ImageItem(LocalClipboard.MainWindow, keyDiff, strRead);

                            break;

                        case ClipboardItem.TypeEnum.Audio:
                            _ = new AudioItem(LocalClipboard.MainWindow, keyDiff, strRead);

                            break;

                        case ClipboardItem.TypeEnum.Custom:
                            _ = new CustomItem(LocalClipboard.MainWindow, keyDiff, strRead);

                            break;
                    }
                }
            }
        }
    }
}
