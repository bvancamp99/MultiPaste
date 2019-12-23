using System;
using System.IO;

namespace MultiPaste
{
    abstract class ClipboardItem
    {
        public ClipboardItem(TypeEnum type)
        {
            this.Type = type;
        }

        public ClipboardItem(TypeEnum type, ushort keyDiff)
        {
            this.Type = type;
            this.KeyDiff = keyDiff;
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

        ///// static property that returns the CLIPBOARD file's directory
        //public static string ClipboardDir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLIPBOARD");

        ///// static property that returns the temp CLIPBOARD file's directory
        //public static string TempClipDir { get; } = ClipboardDir + " temp";

        /// clipboard data type of the item
        public TypeEnum Type { get; }

        /// store the key text that identifies the item
        public string KeyText { get; protected set; }

        /// in case there would be other items with the same KeyText, this ushort differentiates the items for the user
        public ushort KeyDiff { get; protected set; }

        /// store the chars that represent this item in the CLIPBOARD file
        public string FileChars { get; protected set; }

        //public virtual void Remove()
        //{
        //    // if CLIPBOARD file exists and isn't empty, CLIPBOARD file -= this item's FileChars
        //    if (File.Exists(ClipboardDir) && new FileInfo(ClipboardDir).Length > 0)
        //        File.WriteAllText(ClipboardDir, File.ReadAllText(ClipboardDir).Replace(FileChars, ""));

        //    // remove this item from the local clipboard
        //    LocalClipboard.Remove(this.KeyText);
        //}

        //public void Move(int newIndex)
        //{
        //    // first, remove this item from the local clipboard
        //    LocalClipboard.Remove(this.KeyText);

        //    // insert this item at the appropriate index, recovering lost file data if CLIPBOARD file is missing
        //    this.Insert(newIndex);
        //}

        //protected void Insert(int index)
        //{
        //    // insert the item into ListBox, KeyTextCollection, and ClipboardDict
        //    LocalClipboard.GetListBox().Items.Insert(index, KeyText);
        //    LocalClipboard.GetKeys().Insert(index, KeyText);
        //    LocalClipboard.GetDict().Add(KeyText, this);

        //    // create or overwrite CLIPBOARD file, then write all items' FileChars in the appropriate order
        //    using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, false))
        //    {
        //        // write items prior to index to CLIPBOARD file
        //        for (int i = LocalClipboard.GetKeys().Count - 1; i > index; i--)
        //            streamWriter.Write(LocalClipboard.GetDict()[LocalClipboard.GetKeys()[i]].FileChars);

        //        // write current item's FileChars to CLIPBOARD file
        //        streamWriter.Write(LocalClipboard.GetDict()[LocalClipboard.GetKeys()[index]].FileChars);

        //        // write items following index to CLIPBOARD file
        //        for (int i = index - 1; i >= 0; i--)
        //            streamWriter.Write(LocalClipboard.GetDict()[LocalClipboard.GetKeys()[i]].FileChars);
        //    }
        //}

        //protected void Add()
        //{
        //    // if CLIPBOARD file is missing or empty
        //    if (!File.Exists(ClipboardDir) || new FileInfo(ClipboardDir).Length <= 0)
        //    {
        //        // create new file and prepare to write
        //        using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, false))
        //        {
        //            // write each item's FileChars to the CLIPBOARD file
        //            for (int i = LocalClipboard.GetKeys().Count - 1; i >= 0; i--)
        //                streamWriter.Write(LocalClipboard.GetDict()[LocalClipboard.GetKeys()[i]].FileChars);
        //        }
        //    }

        //    // add to ListBox, KeyTextCollection, and ClipboardDict
        //    LocalClipboard.GetListBox().Items.Insert(0, KeyText);
        //    LocalClipboard.GetKeys().Insert(0, KeyText);
        //    LocalClipboard.GetDict().Add(KeyText, this);

        //    // append this item's FileChars to the CLIPBOARD file
        //    using (StreamWriter streamWriter = new StreamWriter(ClipboardDir, true))
        //        streamWriter.Write(FileChars);
        //}

        protected bool SetKeyDiff()
        {
            // calculate KeyDiff so that we can differentiate the items with duplicate key text
            for (string key = KeyText; LocalClipboard.GetDict().ContainsKey(key); key = KeyText + KeyDiff)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = LocalClipboard.GetDict()[key];

                // if this item and duplicateKeyItem hold functionally equivalent data
                if (IsEquivalent(duplicateKeyItem))
                {
                    // if old item is at the target index, then there is no need to add this item
                    if (LocalClipboard.GetKeys().IndexOf(key) == 0)
                        return false;

                    // remove the old item and break from the loop
                    LocalClipboard.Remove(duplicateKeyItem.KeyText, duplicateKeyItem);
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
