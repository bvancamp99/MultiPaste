using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MultiPaste
{
    abstract class ClipboardItem
    {
        protected readonly MainWindow mainWindow; // store MainWindow instance to access its variables

        public ClipboardItem(MainWindow mainWindow, TypeEnum type)
        {
            this.mainWindow = mainWindow;
            this.Type = type;
        }

        public ClipboardItem(MainWindow mainWindow, TypeEnum type, ushort keyDiff)
        {
            this.mainWindow = mainWindow;
            this.Type = type;
            this.KeyDiff = keyDiff;
        }

        /// <summary>
        /// defines the possible clipboard data types
        /// </summary>
        public enum TypeEnum : byte
        {
            Text,
            FileDropList,
            Image,
            Audio,
            Custom
        }

        /// <summary>
        /// clipboard data type of the item
        /// </summary>
        public TypeEnum Type { get; }

        /// <summary>
        /// store the key text that identifies the item
        /// </summary>
        public string KeyText { get; protected set; }

        /// <summary>
        /// in case there would be other items with the same KeyText, this ushort differentiates the items for the user
        /// </summary>
        public ushort KeyDiff { get; protected set; }

        /// <summary>
        /// store the chars that represent this item in the CLIPBOARD file
        /// </summary>
        public string FileChars { get; protected set; }

        protected bool SetKeyDiff()
        {
            // calculate KeyDiff so that we can differentiate the items with duplicate key text
            for (string key = this.KeyText; LocalClipboard.Dict.ContainsKey(key); key = this.KeyText + this.KeyDiff)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = LocalClipboard.Dict[key];

                // if keys are equivalent but the items are not, increment KeyDiff and continue loop
                if (!this.IsEquivalent(duplicateKeyItem))
                {
                    this.KeyDiff++;
                    continue;
                }

                // if old item is at the target index, then there is no need to add this item
                if (LocalClipboard.Keys.IndexOf(key) == 0)
                    return false;

                // if this point is reached, remove the old item and break from the loop
                LocalClipboard.Remove(duplicateKeyItem.KeyText, duplicateKeyItem);
                break;
            }

            return true;
        }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>LocalClipboard.AddWithFile
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected abstract bool IsEquivalent(ClipboardItem duplicateKeyItem);
    }

    class TextItem : ClipboardItem
    {
        public TextItem(MainWindow mainWindow, string text) : base(mainWindow, TypeEnum.Text)
        {
            // ensure param str is valid before continuing
            if (text == null || text.Length == 0)
                return;

            // store param str
            this.Text = text;

            // set beginning part of KeyText
            base.KeyText = "Text: ";

            // append to KeyText given Text
            if (this.Text.Length > LocalClipboard.CHAR_LIMIT - base.KeyText.Length)
            {
                base.KeyText += Text.Substring(0, LocalClipboard.CHAR_LIMIT - base.KeyText.Length) + "...";
            }
            else
            {
                base.KeyText += Text;
            }

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!base.SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0)
            {
                base.KeyText += base.KeyDiff;
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 
            string keyDiffStr = base.KeyDiff.ToString();
            string textLenStr = this.Text.Length.ToString();
            base.FileChars = (char)base.Type + keyDiffStr + Environment.NewLine +
                textLenStr + Environment.NewLine + this.Text;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(base.KeyText, this);

            // move selected index to that of this instance if box is checked
            if (base.mainWindow.MoveToCopied.Checked)
            {
                base.mainWindow.ListBox.SelectedIndex = LocalClipboard.Keys.IndexOf(base.KeyText);
            }

            MsgLabel.Normal("Text item added!");
        }

        public TextItem(MainWindow mainWindow, ushort keyDiff, StringReader strRead)
            : base(mainWindow, TypeEnum.Text, keyDiff)
        {
            // retrieve number of chars in Text
            int textSize = int.Parse(strRead.ReadLine());

            // read textSize num chars from the file to a char array
            char[] textArr = new char[textSize];
            strRead.Read(textArr, 0, textSize);

            // store char array as Text
            this.Text = new string(textArr);

            // set beginning part of KeyText
            base.KeyText = "Text: ";

            // append to KeyText given Text
            if (this.Text.Length > LocalClipboard.CHAR_LIMIT - base.KeyText.Length)
                base.KeyText += this.Text.Substring(0, LocalClipboard.CHAR_LIMIT - base.KeyText.Length) + "...";
            else
                base.KeyText += this.Text;

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 
            string keyDiffStr = base.KeyDiff.ToString();
            string textLenStr = this.Text.Length.ToString();
            base.FileChars = (char)base.Type + keyDiffStr + Environment.NewLine +
                textLenStr + Environment.NewLine + this.Text;

            // add to local clipboard
            LocalClipboard.Add(base.KeyText, this);
        }

        /// <summary>
        /// store the text of the item
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Text
                && this.Text == (duplicateKeyItem as TextItem).Text;
        }
    }

    class FileItem : ClipboardItem
    {
        public FileItem(MainWindow mainWindow, StringCollection fileDropList) : base(mainWindow, TypeEnum.FileDropList)
        {
            // ensure param is valid before continuing
            if (fileDropList == null || fileDropList.Count == 0)
                return;

            // get file drop list from param
            this.FileDropList = fileDropList;

            // if 1 file was copied, KeyText will store FileDropList[0]'s filename
            if (this.FileDropList.Count == 1)
                base.KeyText = "File: " + Path.GetFileName(this.FileDropList[0]);
            // else KeyText will store FileDropList[0]'s filename + how many more files there are
            else
                base.KeyText = "Files: " + Path.GetFileName(this.FileDropList[0]) + " + " + (this.FileDropList.Count - 1) + " more";

            // shorten KeyText to fit the character limit if needed
            if (base.KeyText.Length > LocalClipboard.CHAR_LIMIT)
                base.KeyText = base.KeyText.Substring(0, LocalClipboard.CHAR_LIMIT) + "...";

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!base.SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. # strings in FileDropList
            /// 4. FileDropList
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine +
                this.FileDropList.Count.ToString() + Environment.NewLine;

            foreach (string fileDir in this.FileDropList)
            {
                base.FileChars += fileDir + Environment.NewLine;
            }

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(base.KeyText, this);

            // move selected index to that of this instance if box is checked
            if (base.mainWindow.MoveToCopied.Checked)
            {
                base.mainWindow.ListBox.SelectedIndex = LocalClipboard.Keys.IndexOf(base.KeyText);
            }

            MsgLabel.Normal("File item added!");
        }

        public FileItem(MainWindow mainWindow, ushort keyDiff, StringReader strRead)
            : base(mainWindow, TypeEnum.FileDropList, keyDiff)
        {
            // retrieve number of strings in FileDropList
            int listCount = int.Parse(strRead.ReadLine());

            // read each string into FileDropList
            this.FileDropList = new StringCollection();
            for (int i = 0; i < listCount; i++)
                this.FileDropList.Add(strRead.ReadLine());

            // if 1 file was copied, KeyText will store FileDropList[0]'s filename
            if (this.FileDropList.Count == 1)
                base.KeyText = "File: " + Path.GetFileName(this.FileDropList[0]);
            // else KeyText will store FileDropList[0]'s filename + how many more files there are
            else
                base.KeyText = "Files: " + Path.GetFileName(this.FileDropList[0]) + " + " + (this.FileDropList.Count - 1) + " more";

            // shorten KeyText to fit the character limit if needed
            if (base.KeyText.Length > LocalClipboard.CHAR_LIMIT)
                base.KeyText = base.KeyText.Substring(0, LocalClipboard.CHAR_LIMIT) + "...";

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. # strings in FileDropList
            /// 4. FileDropList
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine +
                this.FileDropList.Count.ToString() + Environment.NewLine;

            foreach (string fileDir in this.FileDropList)
            {
                base.FileChars += fileDir + Environment.NewLine;
            }

            // add to local clipboard
            LocalClipboard.Add(base.KeyText, this);
        }

        /// <summary>
        /// store the file drop list originally received from the Clipboard
        /// </summary>
        public StringCollection FileDropList { get; }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // check for valid type
            if (duplicateKeyItem.Type != TypeEnum.FileDropList)
                return false;

            // check for equivalent item count
            if (this.FileDropList.Count != (duplicateKeyItem as FileItem).FileDropList.Count)
                return false;

            // check equivalency of each filename
            for (int i = 0; i < this.FileDropList.Count; i++)
            {
                if (this.FileDropList[i] != (duplicateKeyItem as FileItem).FileDropList[i])
                    return false;
            }

            // test passed if this point is reached
            return true;
        }
    }

    class ImageItem : ClipboardItem
    {
        public ImageItem(MainWindow mainWindow, Image image) : base(mainWindow, TypeEnum.Image)
        {
            using (image)
            {
                // ensure image isn't null before continuing
                if (image == null) return;

                // copy size struct of the image
                this.Size = image.Size;

                // set KeyText using image dimensions
                base.KeyText = "Image (" + this.Size.Width + " x " + this.Size.Height + ")";

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!base.SetKeyDiff()) return;

                // add KeyDiff to KeyText if needed
                if (base.KeyDiff != 0) 
                    base.KeyText += base.KeyDiff;

                // create Images folder if it's missing
                if (!LocalClipboard.ImageFolder.Exists)
                {
                    LocalClipboard.ImageFolder.Create();
                }

                // create image file in the Images folder
                image.Save(Path.Combine(LocalClipboard.ImageFolder.FullName, base.KeyText));
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine +
                this.Size.Width.ToString() + Environment.NewLine + this.Size.Height.ToString() + 
                Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(base.KeyText, this);

            // move selected index to that of this instance if box is checked
            if (base.mainWindow.MoveToCopied.Checked)
            {
                base.mainWindow.ListBox.SelectedIndex = LocalClipboard.Keys.IndexOf(base.KeyText);
            }

            MsgLabel.Normal("Image item added!");
        }

        public ImageItem(MainWindow mainWindow, ushort keyDiff, StringReader strRead)
            : base(mainWindow, TypeEnum.Image, keyDiff)
        {
            // retrieve width int
            int width = int.Parse(strRead.ReadLine());

            // retrieve height int
            int height = int.Parse(strRead.ReadLine());

            // init Size with width and height args
            this.Size = new Size(width, height);

            // set KeyText using image dimensions
            base.KeyText = "Image (" + this.Size.Width + " x " + this.Size.Height + ")";

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine + 
                this.Size.Width.ToString() + Environment.NewLine + this.Size.Height.ToString() + 
                Environment.NewLine;

            // if image file is missing, remove item data from the CLIPBOARD file and return
            FileInfo imageFile = new FileInfo(Path.Combine(LocalClipboard.ImageFolder.FullName, base.KeyText));
            if (!imageFile.Exists)
            {
                LocalClipboard.RemoveFromFile(base.FileChars);
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(base.KeyText, this);
        }

        /// <summary>
        /// store pixel dimensions of the image linked to this item
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // check for valid type
            if (duplicateKeyItem.Type != TypeEnum.Image)
                return false;

            // check for equivalent image dimensions
            if (!this.Size.Equals((duplicateKeyItem as ImageItem).Size))
                return false;

            // TODO: finish algorithm to accurately compare images
            return true;
        }
    }

    class AudioItem : ClipboardItem
    {
        public AudioItem(MainWindow mainWindow, Stream audioStream) : base(mainWindow, TypeEnum.Audio)
        {
            using (audioStream)
            {
                // ensure the stream is not null before continuing
                if (audioStream == null) return;

                // store length in bytes of the stream
                this.ByteLength = audioStream.Length;

                // byteRate is stored at byte offset 28 of the WAVE file
                byte[] byteRateBuffer = new byte[4];
                audioStream.Read(byteRateBuffer, 28, 4);
                int byteRate = BitConverter.ToInt32(byteRateBuffer, 0);

                // subchunk2Size, i.e. byte length of the audio data, is stored at byte offset 40 (28 + 4 + 8) of the WAVE file
                byte[] subchunk2SizeBuffer = new byte[4];
                audioStream.Read(subchunk2SizeBuffer, 8, 4);
                int subchunk2Size = BitConverter.ToInt32(subchunk2SizeBuffer, 0);

                // compute the total length of the audio file in hours
                int fileLengthSeconds = subchunk2Size / byteRate;
                int fileLengthMinutes = fileLengthSeconds / 60;
                int fileLengthHours = fileLengthMinutes / 60;

                // modify to split correct time in hours, minutes, and seconds
                fileLengthMinutes %= 60;
                fileLengthSeconds %= 60;

                // calculate KeyText given the data
                base.KeyText = "Audio (" + (fileLengthHours == 0 ? "" : fileLengthHours + "h:") + (fileLengthMinutes
                    == 0 ? "" : fileLengthMinutes + "m:") + fileLengthSeconds + "s)";

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!base.SetKeyDiff()) return;

                // add KeyDiff to KeyText if needed
                if (base.KeyDiff != 0) 
                    base.KeyText += base.KeyDiff;

                // create Audio folder if it's missing
                if (!LocalClipboard.AudioFolder.Exists)
                {
                    LocalClipboard.AudioFolder.Create();
                }

                // create audio file in the Audio folder
                FileInfo audioFile = new FileInfo(Path.Combine(LocalClipboard.AudioFolder.FullName, base.KeyText));
                audioStream.Seek(0, SeekOrigin.Begin);
                using (FileStream fileStream = audioFile.Create())
                {
                    audioStream.CopyTo(fileStream);
                }
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. ByteLength
            /// 4. KeyText
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine + 
                this.ByteLength.ToString() + Environment.NewLine + base.KeyText + Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(base.KeyText, this);

            // move selected index to that of this instance if box is checked
            if (base.mainWindow.MoveToCopied.Checked)
            {
                base.mainWindow.ListBox.SelectedIndex = LocalClipboard.Keys.IndexOf(base.KeyText);
            }

            MsgLabel.Normal("Audio item added!");
        }

        public AudioItem(MainWindow mainWindow, ushort keyDiff, StringReader strRead)
            : base(mainWindow, TypeEnum.Audio, keyDiff)
        {
            // retrieving ByteLength and KeyText from the stream
            this.ByteLength = long.Parse(strRead.ReadLine());
            base.KeyText = strRead.ReadLine();

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. ByteLength
            /// 4. KeyText
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine + 
                this.ByteLength.ToString() + Environment.NewLine + base.KeyText + Environment.NewLine;

            // if audio file is missing, remove item data from the CLIPBOARD file and return
            FileInfo audioFile = new FileInfo(Path.Combine(LocalClipboard.AudioFolder.FullName, base.KeyText));
            if (!audioFile.Exists)
            {
                LocalClipboard.RemoveFromFile(base.FileChars);
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(base.KeyText, this);
        }

        /// <summary>
        /// store length in bytes of the audio stream
        /// </summary>
        public long ByteLength { get; }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Audio && 
                this.ByteLength == (duplicateKeyItem as AudioItem).ByteLength;
        }
    }

    class CustomItem : ClipboardItem
    {
        public CustomItem(MainWindow mainWindow, IDataObject dataObject) : base(mainWindow, TypeEnum.Custom)
        {
            // ensure dataObject is valid before continuing
            if (dataObject == null) return;

            // set to null before doing checks
            this.WritableFormat = null;

            // store supported formats of dataObject
            string[] formats = dataObject.GetFormats();

            // ensure formats is a valid array before continuing
            if (formats == null || formats.Length == 0) return;

            // check for a format with writable data and return true if there is one
            foreach (string format in formats)
            {
                // store the first format whose data is serializable, then break
                if (dataObject.GetData(format).GetType().IsSerializable)
                {
                    this.WritableFormat = format;
                    break;
                }
            }

            // if no formats are writable, then we can't add this item
            if (this.WritableFormat == null) return;

            // set KeyText using WritableFormat
            base.KeyText = "Custom (" + this.WritableFormat + ")";

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!base.SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            // create Custom folder if it's missing
            if (!LocalClipboard.CustomFolder.Exists)
            {
                LocalClipboard.CustomFolder.Create();
            }

            // create file in Custom folder with dataObject
            FileInfo customFile = new FileInfo(Path.Combine(LocalClipboard.CustomFolder.FullName, base.KeyText));
            object data = dataObject.GetData(this.WritableFormat);
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fileStream = customFile.Create())
            {
                formatter.Serialize(fileStream, data);
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine + 
                this.WritableFormat + Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(base.KeyText, this);

            // move selected index to that of this instance if box is checked
            if (base.mainWindow.MoveToCopied.Checked)
            {
                base.mainWindow.ListBox.SelectedIndex = LocalClipboard.Keys.IndexOf(base.KeyText);
            }

            MsgLabel.Normal("Custom item added!");
        }

        public CustomItem(MainWindow mainWindow, ushort keyDiff, StringReader strRead)
            : base(mainWindow, TypeEnum.Custom, keyDiff)
        {
            // retrieving WritableFormat from the stream
            this.WritableFormat = strRead.ReadLine();

            // set KeyText using WritableFormat
            base.KeyText = "Custom (" + this.WritableFormat + ")";

            // add KeyDiff to KeyText if needed
            if (base.KeyDiff != 0) 
                base.KeyText += base.KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            base.FileChars = (char)base.Type + base.KeyDiff.ToString() + Environment.NewLine + this.WritableFormat + Environment.NewLine;

            // if custom file is missing, remove item data from the CLIPBOARD file and return
            FileInfo customFile = new FileInfo(Path.Combine(LocalClipboard.CustomFolder.FullName, base.KeyText));
            if (!customFile.Exists)
            {
                LocalClipboard.RemoveFromFile(base.FileChars);
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(base.KeyText, this);
        }

        /// <summary>
        /// store format whose data is serializable, i.e. can be written to the CLIPBOARD file
        /// </summary>
        public string WritableFormat { get; private set; }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// 
        /// This method determines if the param is functionally equivalent to
        /// the current ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem">ClipboardItem with the same key as this instance</param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // TODO: implement method
            return false;
        }
    }
}
