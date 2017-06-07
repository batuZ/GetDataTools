using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetDataTools._2_坡度线.Core;

namespace GetDataTools._2_坡度线.OutLine
{
    partial class GetOutlines
    {
        QThread qthread;
        private static GetOutlines _instance;
        public static GetOutlines Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GetOutlines();
                return _instance;
            }
        }

        public GetOutlines() { }

        async void TK(object n)
        {
            AutoResetEvent autoRE = new AutoResetEvent(false);
            int cnt = 0, Maxcnt = 3;
            for (int i = 0; i < Maxcnt; i++)
            {
                await Task.Run(() =>
                {
                    Console.WriteLine("TK输出：{0}； TK输入为{1}", i,n);
                    lock (qthread)
                    {
                        qthread.Write("TK输出：" + i + "； TK输入为" + n);
                    }
                    cnt++;
                    if (cnt == Maxcnt)
                        autoRE.Set();
                });
                if (cnt == Maxcnt)
                    autoRE.Set();
            }
            autoRE.WaitOne();
        }




        public void GetOlines(int m,double ImprotLevel = 80)
        {
            AutoResetEvent autoRE = new AutoResetEvent(false);
            int iCount=0, Maxcnt = 4;
            List<Thread> li = new List<Thread>();
            List<int> list = new List<int>();
            for (int i = 0; i < Maxcnt; i++)
            {
                Interlocked.Increment(ref iCount);
                Thread th = new Thread((tt) =>
               {
                   TK(tt);
                   Random rand = new Random();
                   int rd = rand.Next(1435);
                   Thread.Sleep(1550 + rd);
                   if (iCount == Maxcnt)
                       autoRE.Set();
               });
                th.Name = "thNM_" + i.ToString();
                th.Start(iCount);
                li.Add(th);
            }
            autoRE.WaitOne();
        }

        public void GetOlines(string inDataPath, string _outDSpath, string OutShpPath,double ImprotLevel = 80)
        {
            Gdal.AllRegister();

            qthread = new QThread(OutShpPath);
            Dataset _inDataset = Gdal.Open(inDataPath, Access.GA_ReadOnly);
            Dataset _outDataset = Gdal.Open(_outDSpath, Access.GA_ReadOnly);

            AutoResetEvent autoRE = new AutoResetEvent(false);
            int iCount = 0, Maxcnt = GetCutNumberOfImg(inDataPath);
            for (int i = 0; i < Maxcnt; i++)
            {
                CutData data = new CutData(inDataPath, _outDSpath, OutShpPath, i,ImprotLevel);
                Thread th = new Thread((tt) =>
                {
                    TKData(tt);
                    iCount++;
                    if (iCount == Maxcnt)
                        autoRE.Set();
                });
                th.Name = "thNM_" + i.ToString();
                th.Start(data);
            }
            autoRE.WaitOne();
            qthread.Close();
        }
        AutoResetEvent asyRE = new AutoResetEvent(false);
        private async void GetOlines00(string inDataPath, string _outDSpath, string OutShpPath, double ImprotLevel = 80)
        {
            Gdal.AllRegister();

            
            Dataset _inDataset = Gdal.Open(inDataPath, Access.GA_ReadOnly);
            Dataset _outDataset = Gdal.Open(_outDSpath, Access.GA_ReadOnly);

            
            int iCount = 0, Maxcnt = GetCutNumberOfImg(inDataPath);
            for (int i = 0; i < Maxcnt; i++)
            {
                CutData data = new CutData(inDataPath, _outDSpath, OutShpPath, i, ImprotLevel);
                await Task.Run(() =>
                    {
                        TKData(data);
                        iCount++;
                        if (iCount == Maxcnt)
                            asyRE.Set();
                    });
            }
        }

        public void MyGetOutlines(string inDataPath, string _outDSpath, string OutShpPath, double ImprotLevel)
        {
            qthread = new QThread(OutShpPath);
            asyRE = new AutoResetEvent(false);
            Thread th = new Thread(() => {
                GetOlines00(inDataPath, _outDSpath, OutShpPath, ImprotLevel);
            });
            th.Start();
            asyRE.WaitOne();
            qthread.Close();
        }
        /// <summary>
        /// 获取图像分块儿数
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        private int GetCutNumberOfImg(string imgPath)
        {
            Dataset _inDataset = Gdal.Open(imgPath, Access.GA_ReadOnly);

            int xTimes = (_inDataset.RasterXSize - 300) / 500;
            int xRemainder = (_inDataset.RasterXSize - 300) % 500;
            int yTimes = (_inDataset.RasterYSize - 300) / 500;
            int yRemainder = (_inDataset.RasterYSize - 300) % 500;
            int ax = xTimes, by = yTimes;
            if (xTimes == 0)
            { xRemainder = _inDataset.RasterXSize; }
            if (yTimes == 0)
            { yRemainder = _inDataset.RasterYSize; }
            if (xRemainder > 10)
                ax++;
            if (yRemainder > 10)
                by++;
            int Maxcnt = ax * by;

            return Maxcnt;
        }
    }
}
