using System;
using System.IO;

namespace MultiPaste
{
    class TextItem : ClipboardItem
    {
        public TextItem(MainWindow mainWindow, string text) : base(mainWindow, TypeEnum.Text)
        {
            #region setting Text using param string

            // ensure param str is valid before continuing
            if (text == null || text.Length == 0)
                return;

            // store param str
            Text = text;

            #endregion

            #region setting KeyText using Text

            // set beginning part of KeyText
            KeyText = "Text: ";

            // append to KeyText given Text
            if (Text.Length > mainWindow.CharacterLimit - KeyText.Length)
                KeyText += Text.Substring(0, mainWindow.CharacterLimit - KeyText.Length) + "...";
            else
                KeyText += Text;

            #endregion

            #region setting KeyDiff using KeyText

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            #endregion

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #region setting FileChars using Type, KeyDiff, and Text

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine +
                Text.Length.ToString() + Environment.NewLine + Text;

            #endregion

            Add(); // add to ListBox, KeyTextCollection, and ClipboardDict, then append to the CLIPBOARD file

            mainWindow.NotifyUser("Text item added!");
        }

        public TextItem(MainWindow mainWindow, ushort keyDiff, StreamReader streamReader) 
            : base(mainWindow, TypeEnum.Text, keyDiff)
        {
            #region retrieving Text from the stream

            // retrieve number of chars in Text
            int textSize = int.Parse(streamReader.ReadLine());

            // read textSize num chars from the file to a char array
            char[] textArr = new char[textSize];
            streamReader.Read(textArr, 0, textSize);

            // store char array as Text
            Text = new string(textArr);

            #endregion

            #region setting KeyText using Text and KeyDiff

            // set beginning part of KeyText
            KeyText = "Text: ";

            // append to KeyText given Text
            if (Text.Length > mainWindow.CharacterLimit - KeyText.Length)
                KeyText += Text.Substring(0, mainWindow.CharacterLimit - KeyText.Length) + "...";
            else
                KeyText += Text;

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #endregion

            #region setting FileChars using Type, KeyDiff, and Text

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine +
                Text.Length.ToString() + Environment.NewLine + Text;

            #endregion

            // add to data structures
            mainWindow.ListBox.Items.Insert(0, KeyText);
            mainWindow.KeyTextCollection.Insert(0, KeyText);
            mainWindow.ClipboardDict.Add(KeyText, this);
        }

        /// store the text of the item
        public string Text { get; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Text
                && Text == (duplicateKeyItem as TextItem).Text;
        }
    }
}
