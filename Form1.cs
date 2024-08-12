//using GetDataTools;
using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Media;
using OSGeo.GDAL;
namespace GetDataTools
{
    public partial class MasterWin : Form
    {
        public MasterWin()
        {
            InitializeComponent();
        }

        /*****************************************************/
        #region 提取轮廓线
        private void butOpen_Click(object sender, EventArgs e)
        {
            openFileDSM.ShowDialog();
        }
        private void butSaveSHP_Click(object sender, EventArgs e)
        {
            saveFileSHP.ShowDialog();
        }
        private void openFileDSM_FileOk(object sender, CancelEventArgs e)
        {
            labDSMin.Text = openFileDSM.FileName;
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.GDAL.Dataset ds = OSGeo.GDAL.Gdal.Open(openFileDSM.FileName, OSGeo.GDAL.Access.GA_ReadOnly);
            double[] a = new double[6];
            ds.GetGeoTransform(a);
            double xx = a[0];
            double yy = a[3];
            double size = a[1];
            double xSixe = ds.RasterXSize;
            double ySixe = ds.RasterYSize;
            labInFileInfo.Text += openFileDSM.SafeFileName + "：\r" + "\r\n"
                + "左上角X坐标：" + xx.ToString() + "\r"
                + "左上角Y坐标：" + yy.ToString() + "\r"
                + "像元尺寸：" + size.ToString() + "\r"
                + "图像大小：" + xSixe.ToString() + "*" + ySixe.ToString();
            ds.Dispose();
        }
        private void saveFileSHP_FileOk(object sender, CancelEventArgs e)
        {
            labSHPout.Text = saveFileSHP.FileName;
        }

        int lev = 3;
        string dzxPath, InDSM, OutShp;
        private void butRunOutLine_Click(object sender, EventArgs e)
        { 
            NativeMethods.AllocConsole();
            if (!checkAdd.Checked)
            {
                if (labDSMin.Text != "" && labSHPout.Text != "")
                {
                    var sw = new System.Diagnostics.Stopwatch(); sw.Start();
                    if (File.Exists(@"D:\TEMPFORGETDATATOOLS\log"))
                        File.Delete(@"D:\TEMPFORGETDATATOOLS\log");
                    openFileDSM.FileName.writeInLog("InDSM");
                    saveFileSHP.FileName.writeInLog("outSHP");

                    dzxPath = _1_等值线.WorkFlow.dzx(openFileDSM.FileName);
                    dzxPath.writeInLog("等值线Polygong");

                    Stopwatch sww = new Stopwatch(); sww.Start();
                    string pdxPath = _2_坡度线.WorkFlow.poDuM(openFileDSM.FileName, lev);

                    sww.Stop(); Console.WriteLine("生成等值线用时：" + sww.Elapsed.ToString());

                    _3_筛选轮廓.筛选轮廓线.selectFeat(dzxPath, pdxPath, saveFileSHP.FileName);

                    _3_筛选轮廓.GetHight.getH(openFileDSM.FileName, saveFileSHP.FileName);
                    for (int i = 5; i > 0; i--)
                    {
                      
                        _5_简化.CleanPoint.claenPoint(saveFileSHP.FileName, 175, i);
                    }
                    sw.Stop();
                    if (File.Exists(@"C:\Windows\Media\Ring03.wav"))
                    {
                        var simpleSound = new SoundPlayer(@"C:\Windows\Media\Ring03.wav");
                        simpleSound.Play();
                    }
                    MessageBox.Show("创建完成!用时：" + sw.Elapsed.ToString());
                }
                else
                {
                  
                    if (labDSMin.Text == "")
                    { MessageBox.Show("请指定要处理的数据！"); }
                    else if (labSHPout.Text == "")
                    { MessageBox.Show("请指定保存路径！"); }
                    return;
                }
            }

            else//ADD 选择补充后
            {
                /*************************e******   直接提取等值线内容       **************************************/
                if (labDSMin.Text == "")
                { MessageBox.Show("请指定要处理的数据！"); return; }
                else if (labeSaveAdd.Text == "")
                { MessageBox.Show("请指定保存路径！"); return; }
                else if (labelPOINT.Text == "")
                { MessageBox.Show("请指定要提取的目标！"); return; }
      
                //计时开始
                var sw = new System.Diagnostics.Stopwatch(); sw.Start();

                //如果没有等值线，就算一个
                if (dzxPath == null)
                {
                    dzxPath = _1_等值线.WorkFlow.dzx(InDSM);
                }

                _2_坡度线.补充要素.AddFeatuer.selectDZXFromPointM(openFilePOINT.FileName, dzxPath, SaveAdd.FileName);

                //计时结束，提示用户
                sw.Stop();
                if (File.Exists(@"C:\Windows\Media\Ring03.wav"))
                {
                    var simpleSound = new SoundPlayer(@"C:\Windows\Media\Ring03.wav");
                    simpleSound.Play();
                }
                MessageBox.Show("补充完成!用时：" + sw.Elapsed.ToString());
            }
            NativeMethods.FreeConsole();
        }

        private void openFilePOINT_FileOk(object sender, CancelEventArgs e)
        {
            if (openFilePOINT.FileName != null)
                labelPOINT.Text = openFilePOINT.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFilePOINT.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkAdd.Checked)
            {
                butSaveAdd.Enabled = true;
                buttonAdd.Enabled = true;
                butSaveSHP.Enabled = false;
                butOpen.Enabled = false;
                labSHPout.Text = "";
                if (File.Exists(@"D:\TEMPFORGETDATATOOLS\log"))
                {
                    StreamReader sr = new StreamReader(@"D:\TEMPFORGETDATATOOLS\log");
                    string afile;
                    while ((afile = sr.ReadLine()) != null)
                    {
                        if (afile.Contains("InDSM"))
                        {
                            InDSM = afile.Substring(afile.IndexOf("：") + 1, afile.Length - afile.IndexOf("：") - 1);
                        }
                        else if (afile.Contains("outSHP"))
                        {
                            OutShp = afile.Substring(afile.IndexOf("：") + 1, afile.Length - afile.IndexOf("：") - 1);
                        }
                        else if (afile.Contains("等值线Polygong"))
                        {
                            dzxPath = afile.Substring(afile.IndexOf("：") + 1, afile.Length - afile.IndexOf("：") - 1);
                        }
                    }
                    labDSMin.Text = InDSM;
                    labSHPout.Text = OutShp;
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("没有需要补充的数据！");
                    checkAdd.Checked = false;
                }
            }
            else
            {
                butOpen.Enabled = true;
                butSaveSHP.Enabled = true;
                buttonAdd.Enabled = false;
                butSaveAdd.Enabled = false;
                labelPOINT.Text = "";
                labeSaveAdd.Text = "";
            }
        }

        private void SaveAdd_FileOk(object sender, CancelEventArgs e)
        {
            labeSaveAdd.Text = SaveAdd.FileName;
        }

        private void butSaveAdd_Click(object sender, EventArgs e)
        {
            SaveAdd.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "1- 预览级")
            { lev = 1; }
            else if (comboBox1.Text == "2- 快")
            { lev = 2; }
            else if (comboBox1.Text == "3- 通用")
            { lev = 3; }
            else if (comboBox1.Text == "4- 慢")
            { lev = 4; }
            else if (comboBox1.Text == "5- 极慢")
            { lev = 5; }
        }

        private void labDSMin_TextChanged(object sender, EventArgs e)
        {
            if (labDSMin.Text != "")
            {
                Gdal.AllRegister();
                OSGeo.GDAL.Dataset ds = OSGeo.GDAL.Gdal.Open(labDSMin.Text, OSGeo.GDAL.Access.GA_ReadOnly);
                double[] a = new double[6];
                ds.GetGeoTransform(a);
                double xx = a[0];
                double yy = a[3];
                double size = a[1];
                double xSixe = ds.RasterXSize;
                double ySixe = ds.RasterYSize;
                labInFileInfo.Text += labDSMin.Text + "：\r" + "\r\n"
                    + "左上角X坐标：" + xx.ToString() + "\r"
                    + "左上角Y坐标：" + yy.ToString() + "\r"
                    + "像元尺寸：" + size.ToString() + "\r"
                    + "图像大小：" + xSixe.ToString() + "*" + ySixe.ToString();
                ds.Dispose();
            }
            else
            {
                labInFileInfo.Text = "";
            }
        }

        private void labInFileInfo_DoubleClick(object sender, EventArgs e)
        {
            labInFileInfo.Text = "";
        }

        #endregion

        /*****************************************************************/
        #region 提取DEM

        private void openDSMforDEM_FileOk(object sender, CancelEventArgs e)
        {
            labDEMin.Text = openDSMforDEM.FileName;
        }

        private void openSHPforDEM_FileOk(object sender, CancelEventArgs e)
        {
            labDEMinSHP.Text = openSHPforDEM.FileName;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            openDSMforDEM.ShowDialog();
        }

        private void butDEMshp_Click(object sender, EventArgs e)
        {
            openSHPforDEM.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            saveDEM.ShowDialog();
        }

        private void saveDEM_FileOk(object sender, CancelEventArgs e)
        {
            labDEMsave.Text = saveDEM.FileName;
            if (labDEMin.Text == "" || labDEMinSHP.Text == "")
            { return; }

            _01_提取DEM.WorkFlow.getDEM(labDEMin.Text, labDEMinSHP.Text, saveDEM.FileName);
        }

        #endregion

        /*****************************************************************/
        #region 小工具

        private void butGetHihgt_Click(object sender, EventArgs e)
        {
            //半成
            //获取建筑高度，需要用多线程，高度值不准
            SubWindows.GetHightOpenFileDialog1 a = new SubWindows.GetHightOpenFileDialog1();
            a.ShowDialog();
        }
        private void butBuffer_Click(object sender, EventArgs e)
        {
            //提高图像分辨率，改变栅格值
            SubWindows.ChangeValue a = new SubWindows.ChangeValue();
            a.ShowDialog();
        }
        private void butCleanPoly_Click(object sender, EventArgs e)
        {
            SubWindows.CleanPointFromPolygon a = new SubWindows.CleanPointFromPolygon();
            a.ShowDialog();
        }

        private void butGetSlope_Click(object sender, EventArgs e)
        {
            SubWindows.GetSopeMap a = new SubWindows.GetSopeMap();
            a.ShowDialog();
        }
        private void butMoveLayer_Click(object sender, EventArgs e)
        {
            SubWindows.minOutLine a = new SubWindows.minOutLine();
            a.ShowDialog();
        }
        #endregion

        private void butGetDoubleFeat_Click(object sender, EventArgs e)
        {
            string inFile = @"D:\code\outputfile\Polygo_clean_10.shp";
            string outFile = @"D:\code\outputfile\Polygo_clean_10_agein.shp";
            _5_简化.CleanPoint.getCleanLins(inFile, outFile);
            MessageBox.Show("!!!!!");
        }
    }
}

