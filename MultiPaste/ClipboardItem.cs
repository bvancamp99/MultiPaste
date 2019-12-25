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

        /// clipboard data type of the item
        public TypeEnum Type { get; }

        /// store the key text that identifies the item
        public string KeyText { get; protected set; }

        /// in case there would be other items with the same KeyText, this ushort differentiates the items for the user
        public ushort KeyDiff { get; protected set; }

        /// store the chars that represent this item in the CLIPBOARD file
        public string FileChars { get; protected set; }

        protected bool SetKeyDiff()
        {
            // calculate KeyDiff so that we can differentiate the items with duplicate key text
            StringCollection myKeys = LocalClipboard.GetKeys();
            Dictionary<string, ClipboardItem> myDict = LocalClipboard.GetDict();
            for (string key = KeyText; myDict.ContainsKey(key); key = KeyText + KeyDiff)
            {
                // store item with the same key
                ClipboardItem duplicateKeyItem = myDict[key];

                // if keys are equivalent but the items are not, increment KeyDiff and continue loop
                if (!this.IsEquivalent(duplicateKeyItem))
                {
                    this.KeyDiff++;
                    continue;
                }

                // if old item is at the target index, then there is no need to add this item
                if (myKeys.IndexOf(key) == 0)
                    return false;

                // if this point is reached, remove the old item and break from the loop
                LocalClipboard.Remove(duplicateKeyItem.KeyText, duplicateKeyItem);
                break;
            }

            return true;
        }

        /// <summary>
        /// All child classes of ClipboardItem must implement IsEquivalent.
        /// This method determines if the param is equivalent to the current
        /// ClipboardItem instance.
        /// </summary>
        /// <param name="duplicateKeyItem"></param>
        /// <returns>whether or not the ClipboardItems are equivalent</returns>
        protected abstract bool IsEquivalent(ClipboardItem duplicateKeyItem);
    }

    class TextItem : ClipboardItem
    {
        public TextItem(string text) : base(TypeEnum.Text)
        {
            // ensure param str is valid before continuing
            if (text == null || text.Length == 0)
                return;

            // store param str
            Text = text;

            // set beginning part of KeyText
            KeyText = "Text: ";

            // append to KeyText given Text
            if (Text.Length > LocalClipboard.CHAR_LIMIT - KeyText.Length)
                KeyText += Text.Substring(0, LocalClipboard.CHAR_LIMIT - KeyText.Length) + "...";
            else
                KeyText += Text;

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine +
                Text.Length.ToString() + Environment.NewLine + Text;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("Text item added!");
        }

        public TextItem(ushort keyDiff, StreamReader streamReader)
            : base(TypeEnum.Text, keyDiff)
        {
            // retrieve number of chars in Text
            int textSize = int.Parse(streamReader.ReadLine());

            // read textSize num chars from the file to a char array
            char[] textArr = new char[textSize];
            streamReader.Read(textArr, 0, textSize);

            // store char array as Text
            Text = new string(textArr);

            // set beginning part of KeyText
            KeyText = "Text: ";

            // append to KeyText given Text
            if (Text.Length > LocalClipboard.CHAR_LIMIT - KeyText.Length)
                KeyText += Text.Substring(0, LocalClipboard.CHAR_LIMIT - KeyText.Length) + "...";
            else
                KeyText += Text;

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Text.Length
            /// 4. Text
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine +
                Text.Length.ToString() + Environment.NewLine + Text;

            // add to local clipboard
            LocalClipboard.Add(this.KeyText, this);
        }

        /// store the text of the item
        public string Text { get; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Text
                && Text == (duplicateKeyItem as TextItem).Text;
        }
    }

    class FileItem : ClipboardItem
    {
        public FileItem(StringCollection fileDropList) : base(TypeEnum.FileDropList)
        {
            // ensure param is valid before continuing
            if (fileDropList == null || fileDropList.Count == 0)
                return;

            // get file drop list from param
            FileDropList = fileDropList;

            // if 1 file was copied, KeyText will store FileDropList[0]'s filename
            if (FileDropList.Count == 1)
                KeyText = "File: " + Path.GetFileName(FileDropList[0]);
            // else KeyText will store FileDropList[0]'s filename + how many more files there are
            else
                KeyText = "Files: " + Path.GetFileName(FileDropList[0]) + " + " + (FileDropList.Count - 1) + " more";

            // shorten KeyText to fit the character limit if needed
            if (KeyText.Length > LocalClipboard.CHAR_LIMIT)
                KeyText = KeyText.Substring(0, LocalClipboard.CHAR_LIMIT) + "...";

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

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

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("File item added!");
        }

        public FileItem(ushort keyDiff, StreamReader streamReader)
            : base(TypeEnum.FileDropList, keyDiff)
        {
            // retrieve number of strings in FileDropList
            int listCount = int.Parse(streamReader.ReadLine());

            // read each string into FileDropList
            FileDropList = new StringCollection();
            for (int i = 0; i < listCount; i++)
                FileDropList.Add(streamReader.ReadLine());

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

    class ImageItem : ClipboardItem
    {
        public ImageItem(Image image) : base(TypeEnum.Image)
        {
            using (image)
            {
                // ensure image isn't null before continuing
                if (image == null) return;

                // copy size struct of the image
                Size = image.Size;

                // set KeyText using image dimensions
                KeyText = "Image (" + Size.Width + " x " + Size.Height + ")";

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!SetKeyDiff()) return;

                // add KeyDiff to KeyText if needed
                if (KeyDiff != 0) KeyText += KeyDiff;

                // create Images folder if it's missing
                string imageFolder = LocalClipboard.GetImageFolder();
                Directory.CreateDirectory(imageFolder);

                // create image file in the Images folder
                image.Save(Path.Combine(imageFolder, KeyText));
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + Size.Width.ToString() + Environment.NewLine +
                Size.Height.ToString() + Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("Image item added!");
        }

        public ImageItem(ushort keyDiff, StreamReader streamReader)
            : base(TypeEnum.Image, keyDiff)
        {
            // retrieve width int
            int width = int.Parse(streamReader.ReadLine());

            // retrieve height int
            int height = int.Parse(streamReader.ReadLine());

            // init Size with width and height args
            Size = new Size(width, height);

            // set KeyText using image dimensions
            KeyText = "Image (" + Size.Width + " x " + Size.Height + ")";

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + Size.Width.ToString() + Environment.NewLine +
                Size.Height.ToString() + Environment.NewLine;

            // if image file is missing, remove item data from the temp CLIPBOARD file and return
            string imageFolder = LocalClipboard.GetImageFolder();
            if (!File.Exists(Path.Combine(imageFolder, KeyText)))
            {
                string tempFile = LocalClipboard.GetTempFile();
                File.WriteAllText(tempFile, File.ReadAllText(tempFile).Replace(FileChars, ""));
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(this.KeyText, this);
        }

        /// store pixel dimensions of the image linked to this item
        public Size Size { get; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // check for valid type
            if (duplicateKeyItem.Type != TypeEnum.Image)
                return false;

            // check for equivalent image dimensions
            if (!Size.Equals((duplicateKeyItem as ImageItem).Size))
                return false;

            // TODO: finish algorithm to accurately compare images
            return true;
        }
    }

    class AudioItem : ClipboardItem
    {
        public AudioItem(Stream audioStream) : base(TypeEnum.Audio)
        {
            using (audioStream)
            {
                // ensure the stream is not null before continuing
                if (audioStream == null) return;

                // store length in bytes of the stream
                ByteLength = audioStream.Length;

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
                KeyText = "Audio (" + (fileLengthHours == 0 ? "" : fileLengthHours + "h:") + (fileLengthMinutes
                    == 0 ? "" : fileLengthMinutes + "m:") + fileLengthSeconds + "s)";

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!SetKeyDiff()) return;

                // add KeyDiff to KeyText if needed
                if (KeyDiff != 0) KeyText += KeyDiff;

                // create Audio folder if it's missing
                string audioFolder = LocalClipboard.GetAudioFolder();
                Directory.CreateDirectory(audioFolder);

                // create audio file in the Audio folder
                using (var fileStream = File.Create(Path.Combine(audioFolder, KeyText)))
                {
                    audioStream.Seek(0, SeekOrigin.Begin);
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

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + ByteLength.ToString() + Environment.NewLine
                + KeyText + Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("Audio item added!");
        }

        public AudioItem(ushort keyDiff, StreamReader streamReader)
            : base(TypeEnum.Audio, keyDiff)
        {
            // retrieving ByteLength and KeyText from the stream
            ByteLength = long.Parse(streamReader.ReadLine());
            KeyText = streamReader.ReadLine();

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. ByteLength
            /// 4. KeyText
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + ByteLength.ToString() + Environment.NewLine
                + KeyText + Environment.NewLine;

            // if audio file is missing, remove item data from the temp CLIPBOARD file and return
            string audioFolder = LocalClipboard.GetAudioFolder();
            if (!File.Exists(Path.Combine(audioFolder, KeyText)))
            {
                string tempFile = LocalClipboard.GetTempFile();
                File.WriteAllText(tempFile, File.ReadAllText(tempFile).Replace(FileChars, ""));
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(this.KeyText, this);
        }

        /// store length in bytes of the audio stream
        public long ByteLength { get; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Audio
                && ByteLength == (duplicateKeyItem as AudioItem).ByteLength;
        }
    }

    class CustomItem : ClipboardItem
    {
        public CustomItem(IDataObject dataObject) : base(TypeEnum.Custom)
        {
            // ensure dataObject is valid before continuing
            if (dataObject == null) return;

            // set to null before doing checks
            WritableFormat = null;

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
                    WritableFormat = format;
                    break;
                }
            }

            // if no formats are writable, then we can't add this item
            if (WritableFormat == null) return;

            // set KeyText using WritableFormat
            KeyText = "Custom (" + WritableFormat + ")";

            // if setting KeyDiff returns false, then there is an equivalent item at index 0
            if (!SetKeyDiff()) return;

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            // create folder if it's missing
            string customFolder = LocalClipboard.GetCustomFolder();
            Directory.CreateDirectory(customFolder);

            // create file in Custom folder with dataObject
            using (var fileStream = File.Create(Path.Combine(customFolder, KeyText)))
            {
                new BinaryFormatter().Serialize(fileStream, dataObject.GetData(WritableFormat));
            }

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + WritableFormat + Environment.NewLine;

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("Custom item added!");
        }

        public CustomItem(ushort keyDiff, StreamReader streamReader)
            : base(TypeEnum.Custom, keyDiff)
        {
            // retrieving WritableFormat from the stream
            WritableFormat = streamReader.ReadLine();

            // set KeyText using WritableFormat
            KeyText = "Custom (" + WritableFormat + ")";

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. WritableFormat
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + WritableFormat + Environment.NewLine;

            // if custom file is missing, remove item data from the temp CLIPBOARD file and return
            string customFolder = LocalClipboard.GetCustomFolder();
            if (!File.Exists(Path.Combine(customFolder, KeyText)))
            {
                string tempFile = LocalClipboard.GetTempFile();
                File.WriteAllText(tempFile, File.ReadAllText(tempFile).Replace(FileChars, ""));
                return;
            }

            // add to local clipboard
            LocalClipboard.Add(this.KeyText, this);
        }

        /// store format whose data is serializable, i.e. can be written to the CLIPBOARD file
        public string WritableFormat { get; private set; }

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            // TODO: implement method
            return false;
        }
    }
}
