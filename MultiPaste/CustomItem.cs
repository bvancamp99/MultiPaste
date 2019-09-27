using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MultiPaste
{
    class CustomItem : ClipboardItem
    {
        public CustomItem(MainWindow mainWindow, IDataObject dataObject) : base(mainWindow, TypeEnum.Custom)
        {
            // ensure dataObject is valid before continuing
            if (dataObject == null) return;

            #region setting WritableFormat with dataObject

            // set to null before doing checks
            WritableFormat = null;

            // store supported formats of dataObject
            string[] formats = dataObject.GetFormats();

            // ensure formats is a valid array before continuing
            if (formats.Length <= 0) return;

            // check for a format with writable data and return true if there is one
            foreach (string format in formats)
            {
                // store the first format whose data is serializable, then break
                if (dataObject.GetData(format).GetType().IsSerializable)
                {
                    WritableFormat = format;
                    break;
                }
            }

            // if no formats are writable, then we can't add this item
            if (WritableFormat == null) return;

            #endregion

            #region setting KeyText using WritableFormat

            // set KeyText using WritableFormat
            KeyText = "Custom (" + WritableFormat + ")";

            #endregion

            #region setting KeyDiff using KeyText

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            #endregion

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            // create folder if it's missing
            Directory.CreateDirectory(FolderDir);

            // create file in Custom folder with dataObject
            using (var fileStream = File.Create(Path.Combine(FolderDir, KeyText)))
            {
                new BinaryFormatter().Serialize(fileStream, dataObject.GetData(WritableFormat));
            }

            #region setting FileChars using Type, KeyDiff, and WritableFormat

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + WritableFormat + Environment.NewLine;

            #endregion

            // add to ListBox, KeyTextCollection, and ClipboardDict, then append to the CLIPBOARD file
            Add();

            mainWindow.NotifyUser("Custom item added!");
        }

        public CustomItem(MainWindow mainWindow, ushort keyDiff, StreamReader streamReader)
            : base(mainWindow, TypeEnum.Custom, keyDiff)
        {
            // retrieving WritableFormat from the stream
            WritableFormat = streamReader.ReadLine();

            #region setting KeyText using WritableFormat and KeyDiff

            // set KeyText using WritableFormat
            KeyText = "Custom (" + WritableFormat + ")";

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #endregion

            #region setting FileChars using Type, KeyDiff, and WritableFormat

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + WritableFormat + Environment.NewLine;

            #endregion

            // if custom file is missing, remove item data from the temp CLIPBOARD file and return
            if (!File.Exists(Path.Combine(FolderDir, KeyText)))
            {
                File.WriteAllText(TempClipDir, File.ReadAllText(TempClipDir).Replace(FileChars, ""));
                return;
            }

            // add to ListBox, KeyTextCollection, and ClipboardDict
            mainWindow.ListBox.Items.Insert(0, KeyText);
            mainWindow.KeyTextCollection.Insert(0, KeyText);
            mainWindow.ClipboardDict.Add(KeyText, this);
        }

        /// static property that returns the directory of the Images folder
        public static string FolderDir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom");

        /// store format whose data is serializable, i.e. can be written to the CLIPBOARD file
        public string WritableFormat { get; private set; }

        public override void Remove()
        {
            base.Remove(); // remove from ListBox, KeyTextCollection, ClipboardDict, and the CLIPBOARD file

            // if directory is missing, there is no file to remove
            if (!Directory.Exists(FolderDir))
                return;

            // delete custom file if it exists
            if (File.Exists(Path.Combine(FolderDir, KeyText)))
                File.Delete(Path.Combine(FolderDir, KeyText));

            // delete the custom directory if it's empty
            if (Directory.GetFiles(FolderDir).Length <= 0)
                Directory.Delete(FolderDir);
        }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // TODO: implement method
            return false;
        }
    }
}