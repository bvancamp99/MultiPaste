using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPaste
{
    static class Themes
    {
        static Themes()
        {
            // create light theme color scheme
            Argb lightBackground = new Argb(255, 240, 240, 240);
            Argb lightMenuStrip = new Argb(255, 230, 230, 230);
            Argb lightCards = new Argb(255, 225, 225, 225);
            Argb lightFont = Argb.FromColor(Color.Black); // black font color
            Themes.Light = new ArgbCollection(lightBackground, lightMenuStrip, lightCards, lightFont);

            // create dark theme color scheme
            Argb darkBackground = new Argb(255, 45, 45, 45);
            Argb darkMenuStrip = new Argb(255, 65, 65, 65);
            Argb darkCards = new Argb(255, 80, 80, 80);
            Argb darkFont = Argb.FromColor(Color.White); // white font color
            Themes.Dark = new ArgbCollection(darkBackground, darkMenuStrip, darkCards, darkFont);
        }

        /// <summary>
        /// ArgbCollection for the Light color theme
        /// </summary>
        public static ArgbCollection Light { get; }

        /// <summary>
        /// ArgbCollection for the Dark color theme
        /// </summary>
        public static ArgbCollection Dark { get; }
    }

    struct ArgbCollection
    {
        public ArgbCollection(Argb background, Argb menuStrip, Argb cards, Argb font)
        {
            this.Background = background;
            this.MenuStrip = menuStrip;
            this.Cards = cards;
            this.Font = font;
        }

        /// <summary>
        /// argb values for the background of MainWindow
        /// </summary>
        public Argb Background { get; }

        /// <summary>
        /// argb values for the main menustrip
        /// </summary>
        public Argb MenuStrip { get; }

        /// <summary>
        /// argb values for "cards", i.e. the listbox, buttons, etc.
        /// </summary>
        public Argb Cards { get; }

        /// <summary>
        /// argb values for the color of the control's foreground; includes font color
        /// </summary>
        public Argb Font { get; }
    }

    struct Argb
    {
        private readonly byte alpha; // value denoting transparency (opacity) of the color
        private readonly byte red;
        private readonly byte green;
        private readonly byte blue;

        /// <summary>
        /// Create instance of the Argb struct with passed argb values.
        /// </summary>
        /// <param name="alpha">color opacity</param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public Argb(byte alpha, byte red, byte green, byte blue)
        {
            this.alpha = alpha;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public static Argb FromColor(Color myColor)
        {
            // create Argb instance from color's argb values
            Argb myArgb = new Argb(myColor.A, myColor.R, myColor.G, myColor.B);

            return myArgb;
        }

        public Color GetColor()
        {
            // create color from argb values
            Color myColor = Color.FromArgb(this.alpha, this.red, this.green, this.blue);

            return myColor;
        }
    }
}
