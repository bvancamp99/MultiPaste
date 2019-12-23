﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace MultiPaste
{
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

                #region set KeyText using the stream

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

                #endregion

                #region setting KeyDiff using KeyText

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!SetKeyDiff()) return;

                #endregion

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

            #region setting FileChars using Type, KeyDiff, ByteLength, and KeyText

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. ByteLength
            /// 4. KeyText
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + ByteLength.ToString() + Environment.NewLine
                + KeyText + Environment.NewLine;

            #endregion

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

            #region setting FileChars using Type, KeyDiff, ByteLength, and KeyText

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. ByteLength
            /// 4. KeyText
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + ByteLength.ToString() + Environment.NewLine
                + KeyText + Environment.NewLine;

            #endregion

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

        ///// static property that returns the directory of the Audio folder
        //public static string FolderDir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");

        /// store length in bytes of the audio stream
        public long ByteLength { get; }

        //public override void Remove()
        //{
        //    base.Remove(); // remove from ListBox, KeyTextCollection, ClipboardDict, and the CLIPBOARD file

        //    // if directory is missing, there is no image to remove
        //    if (!Directory.Exists(FolderDir))
        //        return;

        //    // delete audio file if it exists
        //    if (File.Exists(Path.Combine(FolderDir, KeyText)))
        //        File.Delete(Path.Combine(FolderDir, KeyText));

        //    // delete the audio directory if it's empty
        //    if (Directory.GetFiles(FolderDir).Length <= 0)
        //        Directory.Delete(FolderDir);
        //}

        protected override bool IsEquivalent(ClipboardItem duplicateKeyItem)
        {
            return duplicateKeyItem.Type == TypeEnum.Audio
                && ByteLength == (duplicateKeyItem as AudioItem).ByteLength;
        }
    }
}
