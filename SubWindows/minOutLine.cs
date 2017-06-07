using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools.SubWindows
{
    public partial class minOutLine : Form
    {
        public minOutLine()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            button1.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            button2.Text = saveFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NativeMethods.AllocConsole();
            string open = openFileDialog1.FileName;
            string save = saveFileDialog1.FileName;
            _5_简化._最小外接矩形.小外接矩形(open, save);
            NativeMethods.FreeConsole();
            MessageBox.Show("OK!");
            this.Close(); 
        }
    }
}
