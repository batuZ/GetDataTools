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
    public partial class ChangeValue : Form
    {
        public ChangeValue()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            labOpen.Text = openFileDialog1.FileName;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            labSave.Text = saveFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName != null && saveFileDialog1.FileName != null)//路径完整
            {
                NativeMethods.AllocConsole();
                if (radioButton1.Checked)//Value
                {
                    _2_坡度线.OutLine.CutImg.editRasterValue(openFileDialog1.FileName, saveFileDialog1.FileName, comboBox1.SelectedIndex + 1);
                }
                if (radioButton2.Checked)//Resolution
                {
                    _2_坡度线.OutLine.CutImg.ImgReProject(openFileDialog1.FileName, saveFileDialog1.FileName, comboBox1.SelectedIndex + 1);
                }
                MessageBox.Show("操作完成！");
                NativeMethods.FreeConsole();
                Close();
            }
            else
            {
                MessageBox.Show("请指定路径！");
            }
        }

        private void ChangeValue_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 1;
        }
    }
}
