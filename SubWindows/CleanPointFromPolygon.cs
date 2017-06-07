using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools.SubWindows
{
    public partial class CleanPointFromPolygon : Form
    {
        public CleanPointFromPolygon()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            label1.Text = openFileDialog1.FileName;
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            label2.Text = saveFileDialog1.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (MetarnetRegex.IsNumber(textBox1.Text))
            {
                if (Convert.ToDouble(textBox1.Text) > 0 && Convert.ToDouble(textBox1.Text) < 180)
                {
                    textBox1.Text = textBox1.Text;
                }
                else
                {
                    textBox1.Text = "";
                    MessageBox.Show("请输入0到180之前的数字");
                }
            }
            else
            {
                textBox1.Text = "";
                MessageBox.Show("请输入数字");
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (MetarnetRegex.IsUint(textBox2.Text))
            {
                if (Convert.ToInt32(textBox2.Text) > 1 && Convert.ToDouble(textBox2.Text) <= 20)
                {
                    textBox2.Text = textBox2.Text;
                }
                else
                {
                    textBox2.Text = "";
                    MessageBox.Show("请输入2到20之前的整数");
                }
            }
            else
            {
                textBox2.Text = "";
                MessageBox.Show("请输入整数");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NativeMethods.AllocConsole();
            double jiaodu = Convert.ToDouble(textBox1.Text);
            int times = Convert.ToInt32(textBox2.Text);
            string inFile = openFileDialog1.FileName;
            string outFile = saveFileDialog1.FileName;

            for (int i = times; i > 0; i--)
            {
                _5_简化.CleanPoint.claenPoint(inFile, outFile, jiaodu, times);
                if (i == 1)
                {
                    inFile = outFile;
                    outFile = saveFileDialog1.FileName;
                }
                inFile = outFile;
                outFile = StaticTools.tempFilePath("shp", "cleanTemp");
            }
            NativeMethods.FreeConsole();
            Close();
            MessageBox.Show("OK!");
        }
    }
}
