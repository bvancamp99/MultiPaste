using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiPaste
{
    public partial class PopupWindow : Form
    {
        public string key{ get; private set; }

        public PopupWindow()
        {
            InitializeComponent();
        }

        public string ShowDialog(string key)
        {
            this.key = key;
            tb1.Text = key;

            if (ShowDialog() != DialogResult.OK || this.key == string.Empty || this.key == key)
                return null;
            else
                return this.key;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            key = tb1.Text;
        }
    }
}
