using System;
using System.Drawing;
using System.IO;

namespace MultiPaste
{
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

                #region setting KeyText using Size

                // set KeyText using image dimensions
                KeyText = "Image (" + Size.Width + " x " + Size.Height + ")";

                #endregion

                #region setting KeyDiff using KeyText

                // if setting KeyDiff returns false, then there is an equivalent item at index 0
                if (!SetKeyDiff()) return;

                #endregion

                // add KeyDiff to KeyText if needed
                if (KeyDiff != 0) KeyText += KeyDiff;

                // create Images folder if it's missing
                string imageFolder = LocalClipboard.GetImageFolder();
                Directory.CreateDirectory(imageFolder);

                // create image file in the Images folder
                image.Save(Path.Combine(imageFolder, KeyText));
            }

            #region setting FileChars using Type, KeyDiff, Size.Width, and Size.Height

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + Size.Width.ToString() + Environment.NewLine + 
                Size.Height.ToString() + Environment.NewLine;

            #endregion

            // add to local clipboard with CLIPBOARD file
            LocalClipboard.AddWithFile(this.KeyText, this);

            MsgLabel.Normal("Image item added!");
        }

        public ImageItem(ushort keyDiff, StreamReader streamReader) 
            : base(TypeEnum.Image, keyDiff)
        {
            #region retrieving Size from the stream

            // retrieve width int
            int width = int.Parse(streamReader.ReadLine());

            // retrieve height int
            int height = int.Parse(streamReader.ReadLine());

            // init Size with width and height args
            Size = new Size(width, height);

            #endregion

            #region setting KeyText using Size and KeyDiff

            // set KeyText using image dimensions
            KeyText = "Image (" + Size.Width + " x " + Size.Height + ")";

            // add KeyDiff to KeyText if needed
            if (KeyDiff != 0) KeyText += KeyDiff;

            #endregion

            #region setting FileChars using Type, KeyDiff, Size.Width, and Size.Height

            /// Char order:
            /// 
            /// 1. Type
            /// 2. KeyDiff
            /// 3. Size.Width
            /// 4. Size.Height
            /// 

            FileChars = (char)Type + KeyDiff.ToString() + Environment.NewLine + Size.Width.ToString() + Environment.NewLine +
                Size.Height.ToString() + Environment.NewLine;

            #endregion

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

        ///// static property that returns the directory of the Images folder
        //public static string FolderDir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        /// store pixel dimensions of the image linked to this item
        public Size Size { get; }

        //public override void Remove()
        //{
        //    base.Remove(); // remove from ListBox, KeyTextCollection, ClipboardDict, and the CLIPBOARD file

        //    // if directory is missing, there is no file to remove
        //    if (!Directory.Exists(FolderDir))
        //        return;

        //    // delete image file if it exists
        //    if (File.Exists(Path.Combine(FolderDir, KeyText)))
        //        File.Delete(Path.Combine(FolderDir, KeyText));

        //    // delete the images directory if it's empty
        //    if (Directory.GetFiles(FolderDir).Length <= 0)
        //        Directory.Delete(FolderDir);
        //}

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
}
