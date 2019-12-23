using System;
using System.Collections.Specialized;
using System.IO;

namespace MultiPaste
{
    class FileItem : ClipboardItem
    {
        public FileItem(StringCollection fileDropList) : base(TypeEnum.FileDropList)
        {
            #region setting FileDropList using param

            // ensure param is valid before continuing
            if (fileDropList == null || fileDropList.Count == 0)
                return;

            // get file drop list from param
            FileDropList = fileDropList;

            #endregion

            #region setting KeyText using FileDropList

            // if 1 file was copied, KeyText will store FileDropList[0]'s filename
            if (FileDropList.Count == 1)
                KeyText = "File: " + Path.GetFileName(FileDropList[0]);
            // else KeyText will store FileDropList[0]'s filename + how many more files there are
            else
                KeyText = "Files: " + Path.GetFileName(FileDropList[0]) + " + " + (FileDropList.Count - 1) + " more";

            // shorten KeyText to fit the character limit if needed
            if (KeyText.Length > LocalClipboard.CHAR_LIMIT)
                KeyText = KeyText.Substring(0, LocalClipboard.CHAR_LIMIT) + "...";

            #endregion

            #region setting KeyDiff using KeyText

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            #endregion

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #region setting FileChars using Type, KeyDiff, and FileDropList

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. # strings in FileDropList
            /// 4. FileDropList
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + 
                FileDropList.Count.ToString() + Environment.NewLine;

            foreach (string dir in FileDropList)
                FileChars += dir + Environment.NewLine;

            #endregion

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("File item added!");
        }

        public FileItem(ushort keyDiff, StreamReader streamReader) 
            : base(TypeEnum.FileDropList, keyDiff)
        {
            #region retrieving FileDropList from the stream

            // retrieve number of strings in FileDropList
            int listCount = int.Parse(streamReader.ReadLine());

            // read each string into FileDropList
            FileDropList = new StringCollection();
            for (int i = 0; i < listCount; i++)
                FileDropList.Add(streamReader.ReadLine());

            #endregion

            #region setting KeyText using FileDropList and KeyDiff

            // if 1 file was copied, KeyText will store FileDropList[0]'s filename
            if (FileDropList.Count == 1)
                KeyText = "File: " + Path.GetFileName(FileDropList[0]);
            // else KeyText will store FileDropList[0]'s filename + how many more files there are
            else
                KeyText = "Files: " + Path.GetFileName(FileDropList[0]) + " + " + (FileDropList.Count - 1) + " more";

            // shorten KeyText to fit the character limit if needed
            if (KeyText.Length > LocalClipboard.CHAR_LIMIT)
                KeyText = KeyText.Substring(0, LocalClipboard.CHAR_LIMIT) + "...";

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #endregion

            #region setting FileChars using Type, KeyDiff, and FileDropList

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. # strings in FileDropList
            /// 4. FileDropList
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine +
                FileDropList.Count.ToString() + Environment.NewLine;

            foreach (string dir in FileDropList)
                FileChars += dir + Environment.NewLine;

            #endregion

            // add to local clipboard
            LocalClipboard.Add(this.KeyText, this);
        }

        /// store the file drop list originally received from the Clipboard
        public StringCollection FileDropList { get; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // check for valid type
            if (duplicateKeyItem.Type != TypeEnum.FileDropList)
                return false;

            // check for equivalent item count
            if (FileDropList.Count != (duplicateKeyItem as FileItem).FileDropList.Count)
                return false;

            // check equivalency of each filename
            for (int i = 0; i < FileDropList.Count; i++)
            {
                if (FileDropList[i] != (duplicateKeyItem as FileItem).FileDropList[i])
                    return false;
            }

            // test passed if this point is reached
            return true;
        }
    }
}
