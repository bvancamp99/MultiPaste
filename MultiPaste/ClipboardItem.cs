using System;
using System.IO;

namespace MultiPaste
{
    abstract class ClipboardItem
    {
        protected readonly MainWindow mainWindow; // store instance of main window

        public ClipboardItem(MainWindow mainWindow, TypeEnum type)
        {
            this.mainWindow = mainWindow;
            Type = type;
        }

        public ClipboardItem(MainWindow mainWindow, TypeEnum type, ushort keyDiff)
        {
            this.mainWindow = mainWindow;
            Type = type;
            KeyDiff = keyDiff;
        }

        /// this enumerator defines the possible clipboard data types
        public enum TypeEnum : byte
        {
            Text,
            FileDropList,
            Image,
            Audio,
            Custom
        }

        /// static property that returns the CLIPBOARD file's directory
        public static string ClipboardDir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD");

        /// static property that returns the temp CLIPBOARD file's directory
        public static string TempClipDir { get; } = ClipboardDir + " temp";

        /// clipboard data type of the item
        public TypeEnum Type { get; }

        /// store the key text that identifies the item
        public string KeyText { get; protected set; }

        /// in case there would be other items with the same KeyText, this ushort differentiates the items for the user
        public ushort KeyDiff { get; protected set; }

        /// store the chars that represent this item in the CLIPBOARD file
        public string FileChars { get; protected set; }

        public virtual void Remove()
        {
            // if CLIPBOARD file exists and isn't empty, CLIPBOARD file -= this item's FileChars
            if (File.Exists(ClipboardDir) && new FileInfo(ClipboardDir).Length > 0)
                File.WriteAllText(ClipboardDir, File.ReadAllText(ClipboardDir).Replace(FileChars, ""));

            // remove from ListBox, KeyTextCollection, and ClipboardDict
            mainWindow.ListBox.Items.Remove(KeyText);
            mainWindow.KeyTextCollection.Remove(KeyText);
            mainWindow.ClipboardDict.Remove(KeyText);
        }

        public void Move(int newIndex)
        {
            // remove from the data structures before calling Insert method
            mainWindow.ListBox.Items.Remove(KeyText);
            mainWindow.KeyTextCollection.Remove(KeyText);
            mainWindow.ClipboardDict.Remove(KeyText);

            // insert this item at the appropriate index, recovering lost file data if CLIPBOARD file is missing
            Insert(newIndex);
        }

        protected void Insert(int index)
        {
            // insert the item into ListBox, KeyTextCollection, and ClipboardDict
            mainWindow.ListBox.Items.Insert(index, KeyText);
            mainWindow.KeyTextCollection.Insert(index, KeyText);
            mainWindow.ClipboardDict.Add(KeyText, this);

            // create or overwrite CLIPBOARD file, then write all items' FileChars in the appropriate order
            using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, false))
            {
                // write items prior to index to CLIPBOARD file
                for (int i = mainWindow.KeyTextCollection.Count - 1; i > index; i--)
                    streamWriter.Write(mainWindow.ClipboardDict[mainWindow.KeyTextCollection[i]].FileChars);

                // write current item's FileChars to CLIPBOARD file
                streamWriter.Write(mainWindow.ClipboardDict[mainWindow.KeyTextCollection[index]].FileChars);

                // write items following index to CLIPBOARD file
                for (int i = index - 1; i >= 0; i--)
                    streamWriter.Write(mainWindow.ClipboardDict[mainWindow.KeyTextCollection[i]].FileChars);
            }
        }

        protected void Add()
        {
            // if CLIPBOARD file is missing or empty
            if (!File.Exists(ClipboardDir) || new FileInfo(ClipboardDir).Length <= 0)
            {
                // create new file and prepare to write
                using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, false))
                {
                    // write each item's FileChars to the CLIPBOARD file
                    for (int i = mainWindow.KeyTextCollection.Count - 1; i >= 0; i--)
                        streamWriter.Write(mainWindow.ClipboardDict[mainWindow.KeyTextCollection[i]].FileChars);
                }
            }

            // add to ListBox, KeyTextCollection, and ClipboardDict
            mainWindow.ListBox.Items.Insert(0, KeyText);
            mainWindow.KeyTextCollection.Insert(0, KeyText);
            mainWindow.ClipboardDict.Add(KeyText, this);

            // append this item's FileChars to the CLIPBOARD file
            using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, true))
                streamWriter.Write(FileChars);
        }

        protected bool SetKeyDiff()
        {
            // calculate KeyDiff so that we can differentiate the items with duplicate key text
            for (string key = KeyText; mainWindow.ClipboardDict.ContainsKey(key); key = KeyText + KeyDiff)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = mainWindow.ClipboardDict[key];

                // if this item and duplicateKeyItem hold functionally equivalent data
                if (IsEquivalent(duplicateKeyItem))
                {
                    // if old item is at the target index, then there is no need to add this item
                    if (mainWindow.KeyTextCollection.IndexOf(key) == 0)
                        return false;

                    // remove the old item and break from the loop
                    duplicateKeyItem.Remove();
                    break;
                }
                // else update KeyDiff and continue loop
                else
                    KeyDiff++;
            }

            return true;
        }

        protected abstract bool IsEquivalent(ClipboardItem duplicateKeyItem);
    }
}
