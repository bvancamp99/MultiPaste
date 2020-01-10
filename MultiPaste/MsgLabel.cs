using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    /// <summary>
    /// This static class is used to communicate program actions with the user.
    /// 
    /// It also notifies the user of errors that occurred, as well as keeping
    /// an error log file to log fatal error information.
    /// </summary>
    static class MsgLabel
    {
        private static readonly System.Windows.Forms.Timer timer; // used to determine how long to display a message
        private static readonly FileInfo errLogFile; // err.log file used to communicate fatal errors

        static MsgLabel()
        {
            // initialize the timer with its interval at 3 seconds
            MsgLabel.timer = new System.Windows.Forms.Timer
            {
                Interval = 3000
            };

            // clear notifLabel when the timer goes off, then stop the timer
            MsgLabel.timer.Tick += (sender, e) =>
            {
                MsgLabel.MainWindow.Label.Text = string.Empty;
                MsgLabel.timer.Stop();
            };

            // initialize FileInfo representing the err.log file
            errLogFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "err.log"));
        }

        /// <summary>
        /// store MainWindow to access its variables
        /// </summary>
        public static MainWindow MainWindow { private get; set; }

        private static void SendMsg(string msg)
        {
            // stop the timer if it's currently running
            if (MsgLabel.timer.Enabled)
                MsgLabel.timer.Stop();

            // set the label's text to the param string
            MsgLabel.MainWindow.Label.Text = msg;

            // begin the timer, which will clear the label after 3 seconds
            MsgLabel.timer.Start();
        }

        public static void Normal(string msg)
        {
            // send normal message to user
            MsgLabel.SendMsg(msg);
        }

        public static void Warn(string msg)
        {
            // send warning message to user
            MsgLabel.SendMsg("***WARNING*** " + msg);
        }

        /// <summary>
        /// This method communicates to the user that a fatal error occurred.
        /// 
        /// First, it displays msg param to the user via the msg label, 
        /// followed by directing the user to err.log to view the details 
        /// of the error.
        /// 
        /// This method also writes to the err.log file in the program's
        /// directory, and then terminates the process after sleeping for
        /// the duration of the message being displayed to the user.
        /// </summary>
        /// <param name="msg">brief explanation of the fatal error</param>
        /// <param name="msgExtended">error msg explained in more detail</param>
        /// <param name="solutions">possible solution(s) that the user could implement</param>
        public static void Fatal(string msg, string msgExtended, string solutions)
        {
            // set string that will be written to err.log
            string log = "Date: " + DateTime.Now.ToString() + Environment.NewLine +
                "Abstract: " + msg + Environment.NewLine +
                "Log: " + msgExtended + Environment.NewLine +
                "Possible solution(s): " + solutions + Environment.NewLine;

            // append log string to err.log, creating the file if it doesn't exist
            using (StreamWriter sw = MsgLabel.errLogFile.AppendText())
            {
                sw.WriteLine(log);
            }

            // send fatal error message to user
            MsgLabel.SendMsg("***FATAL*** " + msg + "; see err.log for details.");

            // sleep for the duration of msg being displayed
            Thread.Sleep(MsgLabel.timer.Interval);

            // terminate the process
            Application.Exit();
        }
    }
}
