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
    public partial class GetSopeMap : Form
    {
        public GetSopeMap()
        {
            InitializeComponent();
        }

        private void butOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void butRun_Click(object sender, EventArgs e)
        {
            NativeMethods.AllocConsole();
           _2_坡度线.坡度提取.SlopeDem.Instance.Sloping(openFileDialog1.FileName, saveFileDialog1.FileName);
           _2_坡度线.OutLine.GetOutlines.Instance.MyGetOutlines(openFileDialog1.FileName, saveFileDialog1.FileName, saveFileDialog2.FileName, 80);
           MessageBox.Show("完事");
           NativeMethods.FreeConsole();
           Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog2.ShowDialog();
        }
    }
}
