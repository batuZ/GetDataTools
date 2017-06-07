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
    public partial class GetHightOpenFileDialog1 : Form
    {
        public GetHightOpenFileDialog1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetHightOpen.ShowDialog();
        }
        private void GetHightOpen_FileOk(object sender, CancelEventArgs e)
        {
            GetHightlabel1.Text = GetHightOpen.FileName;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            GetHopenSHPFile.ShowDialog();
        }
        private void GetHopenSHPFile_FileOk(object sender, CancelEventArgs e)
        {
            GetHightlabel2.Text = GetHopenSHPFile.FileName;
            NativeMethods.AllocConsole();
            _3_筛选轮廓.GetHight.getHM(GetHightOpen.FileName, GetHopenSHPFile.FileName);
            NativeMethods.FreeConsole();
            Close();
        }
    }
}
