using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    class MsgLabel
    {
        private static Label myLabel;
        private static Timer myTimer;

        public MsgLabel (Label myLabel)
        {
            MsgLabel.myLabel = myLabel;

            // initialize the timer with its interval at 3 seconds
            MsgLabel.myTimer = new Timer
            {
                Interval = 3000
            };

            // clear notifLabel when the timer goes off, then stop the timer
            MsgLabel.myTimer.Tick += (sender, e) =>
            {
                MsgLabel.myLabel.Text = string.Empty;
                MsgLabel.myTimer.Stop();
            };
        }

        public static void Normal(string msg)
        {
            // stop the timer if it's currently running
            if (myTimer.Enabled)
                myTimer.Stop();

            // set myLabel's text to the param string
            myLabel.Text = msg;

            // begin the timer, which will clear myLabel after 3 seconds
            myTimer.Start();
        }

        public static void Warn(string msg)
        {
            // stop the timer if it's currently running
            if (myTimer.Enabled)
                myTimer.Stop();

            // set myLabel's text to the param string
            myLabel.Text = "***WARNING*** " + msg;

            // begin the timer, which will clear myLabel after 3 seconds
            myTimer.Start();
        }

        // TODO: write to error log, maybe include line number of problem if in CLIPBOARD file
        public static void Fatal(string msg)
        {
            // stop the timer if it's currently running
            if (myTimer.Enabled)
                myTimer.Stop();

            // set myLabel's text to the param string
            myLabel.Text = "***ERROR*** " + msg;

            // begin the timer, which will clear myLabel after 3 seconds
            myTimer.Start();
        }
    }
}
