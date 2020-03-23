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
        public string key{ get; set; }

        public PopupWindow(string key)
        {
            InitializeComponent();

            this.key = key;
        }

        public new string ShowDialog()
        {
            if (base.ShowDialog() != DialogResult.OK || key == string.Empty)
                return null;
            else
                return key;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            key = tb1.Text;
        }
    }
}
