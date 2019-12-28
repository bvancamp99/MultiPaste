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
        private readonly MainWindow mainWindow; // store MainWindow instance to access its variables
        private readonly Timer timer; // used to determine how long to display a message

        public MsgLabel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // initialize the timer with its interval at 3 seconds
            this.timer = new Timer
            {
                Interval = 3000
            };

            // clear notifLabel when the timer goes off, then stop the timer
            this.timer.Tick += (sender, e) =>
            {
                this.mainWindow.Label.Text = string.Empty;
                this.timer.Stop();
            };
        }

        public void Normal(string msg)
        {
            // stop the timer if it's currently running
            if (this.timer.Enabled)
                this.timer.Stop();

            // set the label's text to the param string
            this.mainWindow.Label.Text = msg;

            // begin the timer, which will clear the label after 3 seconds
            this.timer.Start();
        }

        public void Warn(string msg)
        {
            // stop the timer if it's currently running
            if (this.timer.Enabled)
                this.timer.Stop();

            // set the label's text to the param string
            this.mainWindow.Label.Text = "***WARNING*** " + msg;

            // begin the timer, which will clear the label after 3 seconds
            this.timer.Start();
        }

        // TODO: write to error log, maybe include line number of problem if in CLIPBOARD file
        public void Fatal(string msg)
        {
            // stop the timer if it's currently running
            if (this.timer.Enabled)
                this.timer.Stop();

            // set the label's text to the param string
            this.mainWindow.Label.Text = "***ERROR*** " + msg;

            // begin the timer, which will clear the label after 3 seconds
            this.timer.Start();
        }
    }
}
