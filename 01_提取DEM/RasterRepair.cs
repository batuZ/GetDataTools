using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools._01_提取DEM
{
    struct RasBlock
    {
        public int minX;//行列号minX；
        public int minY;//行列号minY；
        public int maxX;//行列号maxX；
        public int maxY;//行列号maxY；
        public int xSize
        {
            get
            {
                return Math.Abs(maxX - minX) + 1;
            }
        }
        public int ySize
        {
            get
            {
                return Math.Abs(maxY - minY) + 1;
            }
        }

        public int Fid;
        public string imgPath;

        public double NodataVal;
        public double[] rasterArr;//如果rasterArr（实则看做一个二维数组）边界某值为Nodata，其对应的orderArr值为0
        //public double[] orderArr;
        public List<int> orderList;

        public void Init()
        {
            rasterArr = new double[xSize * ySize];
            //orderArr = new double[xSize * ySize];
            orderList = new List<int>();
            for (int i = 0; i < xSize * ySize; i++)
            {
                orderList.Add(i);
            }
        }
        //public Geometry polygon;
    }

    public class RasterRepair
    {
        string _imgPath ;
        string _shpPath ;

        public RasterRepair(string imgPath,string shpPath,Action callback = null)
        {
            _imgPath = imgPath;
            _shpPath = shpPath;
            double[] geoTransform = new double[6];

            Dataset imgDs = Gdal.Open(_imgPath, Access.GA_Update);
            imgDs.GetGeoTransform(geoTransform);
            Band _band = imgDs.GetRasterBand(1);
            OSGeo.GDAL.Driver d = Gdal.GetDriverByName("HFA");
            //Dataset dsout = d.Create(_orderPath, _band.XSize, _band.YSize, 1, DataType.GDT_Float32, null);

            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            DataSource ds = dr.Open(_shpPath, 0);
            Layer lyr = ds.GetLayerByIndex(0);

            thr tr = new thr(imgDs, imgPath, geoTransform, lyr);
            tr.Repair(callback);
        }
    }

    //demo Func/callBack  注：在桌面有该段代码修改前的备份 2016-2-1 11:06:32
    class thr
    {
        private string _myImgPath;
        private Band _band;//要保证该band有nodata值
        private Dataset _imgDs;
        private double _nodataVal;
        private int _hasVal;
        private double[] _geoTransform;
        private int iCount;
        private long iMaxCount;
        private Layer _layer;
        private Queue<RasBlock> rasBlock_res_Queue;
        private int Cnt_Thr = 0;

        public thr(Dataset imgDs,string ImgPath, double[] geoTransform, Layer layer)
        {
            _myImgPath = ImgPath;
            _imgDs = imgDs;
            _band = imgDs.GetRasterBand(1);
            _band.GetNoDataValue(out _nodataVal, out _hasVal);
            _layer = layer;
            //rasterEvents = rasterWrite;

            if (_hasVal == 0)
            {
                _band.SetNoDataValue(0);
            }
            _geoTransform = geoTransform;
            iMaxCount = layer.GetFeatureCount(1);
            iCount = 0;
        }
        //使用异步 

        public void Repair(Action Callback = null)
        {
            rasBlock_res_Queue = new Queue<RasBlock>();
            for (int i = 0; i < _layer.GetFeatureCount(1); i++)
            {
                RasBlock _ms = new RasBlock();
                _ms.Fid = i;
                Envelope myEnve = GetEnve2(_ms.Fid, 10, _geoTransform);
                _ms.minX = Convert.ToInt32(myEnve.MinX);
                _ms.minY = Convert.ToInt32(myEnve.MinY);
                _ms.maxX = Convert.ToInt32(myEnve.MaxX);
                _ms.maxY = Convert.ToInt32(myEnve.MaxY);
                _ms.NodataVal = _nodataVal;
                _ms.imgPath = _myImgPath;
                _ms.Init();
                lock (_band)
                {
                    _band.ReadRaster(_ms.minX, _ms.minY, _ms.xSize, _ms.ySize, _ms.rasterArr, _ms.xSize, _ms.ySize, 0, 0);
                }
                ThreadProc3(_ms);//进行异步计算
            }

            //等待异步结果

            long time = _layer.GetFeatureCount(1), cur = 0;
            while (cur < time)
            {
                if (rasBlock_res_Queue.Count > 0)
                {
                    RasBlock res = rasBlock_res_Queue.Dequeue();
                    double[] subBuff = GetSubBuffer(res.rasterArr, res.xSize, res.ySize);
                    Console.WriteLine("主程序执行写入操作......");
                    _band.WriteRaster(res.minX + 1, res.minY + 1, res.xSize - 2, res.ySize - 2, subBuff, res.xSize - 2, res.ySize - 2, 0, 0);
                    _band.FlushCache();

                    cur++;
                }
                Thread.Sleep(2000);
            }

            _band.Dispose();
            _imgDs.Dispose();
            if (Callback != null)
            {
                Callback();
            }
        }

        private Envelope GetEnve2(int Fid, int BlkBuf, double[] geoTransform)
        {
            Geometry geo = _layer.GetFeature(Fid).GetGeometryRef();

            Envelope res = new Envelope();
            Envelope enve = new Envelope();
            double minX, maxX, minY, maxY;

            geo.GetEnvelope(enve);
            double[] a = geoTransform;
            if (a[1] < 0)
            {
                minX = enve.MaxX;
                maxX = enve.MinX;
            }
            else
            {
                minX = enve.MinX;
                maxX = enve.MaxX;
            }
            if (a[5] < 0)
            {
                minY = enve.MaxY;
                maxY = enve.MinY;
            }
            else
            {
                minY = enve.MinY;
                maxY = enve.MaxY;
            }
            double dTemp = a[1] * a[5] - a[2] * a[4];

            res.MinX = (a[5] * (minX - a[0]) - a[2] * (minY - a[3])) / dTemp - 0.5 - BlkBuf;
            if (res.MinX < 0)
                res.MinX = 0;
            res.MinY = (a[1] * (minY - a[3]) - a[4] * (minX - a[0])) / dTemp - 0.5 - BlkBuf;
            if (res.MinY < 0)
                res.MinY = 0;
            res.MaxX = (a[5] * (maxX - a[0]) - a[2] * (maxY - a[3])) / dTemp - 0.5 + BlkBuf;
            if (res.MaxX > _band.XSize - 1)
                res.MaxX = _band.XSize - 1;
            res.MaxY = (a[1] * (maxY - a[3]) - a[4] * (maxX - a[0])) / dTemp - 0.5 + BlkBuf;
            if (res.MaxY > _band.YSize - 1)
                res.MaxY = _band.YSize - 1;
            return res;
        }

        private async void ThreadProc3(RasBlock rb)
        {
            await Task.Run(() =>
            {
                CriByFor2(rb);
                //Console.WriteLine("Start rasBlock_res_Queue.Enqueue:" + rasBlock_res_Queue.Count);
                rasBlock_res_Queue.Enqueue(rb);
                //Console.WriteLine("End rasBlock_res_Queue.Enqueue:" + rasBlock_res_Queue.Count);
            });
           
        }

        private void ThreadProc4(RasBlock rb)
        {
            CriByFor2(rb);
            rasBlock_res_Queue.Enqueue(rb);

        }
        private double[] GetSubBuffer(double[] Arr, int xSize,int ySize)
        {
            double[] res = new double[(xSize - 2) * (ySize - 2)];
            for (int j = 0; j < ySize - 2; j++)
            {
                for (int i = 0; i < xSize - 2; i++)
                {
                    res[(j * (xSize - 2)) + i] = Arr[(j + 1) * xSize + (i + 1)];
                }
            }

            return res;
        }

        #region Criminisi算法
        private void CriByFor(RasBlock item)
        {
            for (int i = 0; i < item.xSize * item.ySize; i++)
            {
                int maxC_idx = GetMaxC_Index(item);
                if (maxC_idx > -1)
                {
                    GetFixedBlockAndSetValue(item, maxC_idx);
                }
                else
                    break;
            }
            for (int m = 0; m < item.xSize * item.ySize; m++)
            {
                if (Math.Abs(item.rasterArr[m] - item.NodataVal) < 0.00001)
                {
                    Console.WriteLine("第{0}个Feature含有Nodata！", item.Fid);
                    break;
                }
            }
                Console.WriteLine("Calcu  Compelete");
        }

        private void CriByFor2(RasBlock item)
        {
            for (int i = 0; i < item.xSize * item.ySize; i++)
            {

                int maxC_idx = GetMaxC_Index2(item);
                if (maxC_idx > -1)
                {
                    GetFixedBlockAndSetValue(item, maxC_idx);
                }
                else
                    break;
            }
            Console.WriteLine("Calcu  Compelete");
        }

        //private int GetMaxC_Index_beifen(RasBlock rasB)//备份算法GetMaxC_Index 2016-3-1 17:43:24
        //{
        //    int Index_orderVal_Max = -1;
        //    double orderVal = 0;
        //    for (int i = 0; i < rasB.rasterArr.Length; i++)
        //    {
        //        if (Math.Abs(rasB.rasterArr[i] - rasB.NodataVal) < 0.00001)//说明该点为Nodata
        //        {
        //            int x = i % rasB.xSize;
        //            int y = i / rasB.xSize;
        //            if (x != 0 && x != (rasB.xSize - 1) && y != 0 && y != (rasB.ySize - 1))
        //            {
        //                double[] aBuffer = new double[8];
        //                int num = ValidBlocks(i, rasB);
        //                if (num > 0)
        //                {
        //                    rasB.orderArr[i] = Convert.ToDouble(num) / 8;
        //                }
        //                else
        //                    rasB.orderArr[i] = 0;
        //            }
        //        }
        //        else
        //            rasB.orderArr[i] = 0;
        //        if (rasB.orderArr[i] > orderVal)
        //        {
        //            orderVal = rasB.orderArr[i];
        //            Index_orderVal_Max = i;
        //        }
        //    }
        //    if (orderVal < 0.00001)
        //        Index_orderVal_Max = -1;
        //    return Index_orderVal_Max;
        //}
        private int GetMaxC_Index(RasBlock rasB)
        {
            int Index_orderVal_Max = -1;
            int orderVal = 0;
            for (int i = 0; i < rasB.rasterArr.Length; i++)
            {
                if (Math.Abs(rasB.rasterArr[i] - rasB.NodataVal) < 0.00001)//说明该点为Nodata
                {
                    int x = i % rasB.xSize;
                    int y = i / rasB.xSize;
                    if (x != 0 && x != (rasB.xSize - 1) && y != 0 && y != (rasB.ySize - 1))
                    {
                        double[] aBuffer = new double[8];
                        int num = ValidBlocks(i, rasB);
                        //rasB.orderArr[i] = num;
                        if (num > orderVal)
                        {
                            orderVal = num;
                            Index_orderVal_Max = i;
                        }
                    }
                }
            }

            return Index_orderVal_Max;
        }
        private int GetMaxC_Index2(RasBlock rasB)
        {
            int Index_orderVal_Max = -1;
            int orderVal = 0;
            for (int i = 0; i < rasB.orderList.Count; i++)
            {
                int listVal = rasB.orderList[i];
                int x = listVal % rasB.xSize;
                int y = listVal / rasB.xSize;
                if (i == 110)
                { 
                }
                double tt = rasB.rasterArr[listVal];
                if (Math.Abs(rasB.rasterArr[listVal] - rasB.NodataVal) > 0.00001)//说明该点不为Nodata
                {
                    rasB.orderList.RemoveAt(i);
                    i--;
                }
                else if (x == 0 || x == (rasB.xSize - 1) || y == 0 || y == (rasB.ySize - 1))
                {
                    rasB.orderList.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < rasB.orderList.Count; i++)
            {
                int list_Val = rasB.orderList[i];
                double[] aBuffer = new double[8];
                int num = ValidBlocks(list_Val, rasB);
                //rasB.orderArr[i] = num;
                if (num > orderVal)
                {
                    orderVal = num;
                    Index_orderVal_Max = list_Val;
                }
            }
            return Index_orderVal_Max;
        }

        private int ValidBlocks(int i, RasBlock RB)
        {
            int num = 0;
            double[] aBuffer = new double[8];
            aBuffer[0] = RB.rasterArr[i - RB.xSize-1];
            aBuffer[1] = RB.rasterArr[i - RB.xSize];
            aBuffer[2] = RB.rasterArr[i - RB.xSize+1];
            aBuffer[3] = RB.rasterArr[i - 1];
            aBuffer[4] = RB.rasterArr[i + 1];
            aBuffer[5] = RB.rasterArr[i + RB.xSize-1];
            aBuffer[6] = RB.rasterArr[i + RB.xSize];
            aBuffer[7] = RB.rasterArr[i + RB.xSize+1];

            foreach (double item in aBuffer)
            {
                if (Math.Abs(item - RB.NodataVal) > 0.000001)
                {
                    num++;
                }
            }
            return num;
        }

        private void GetFixedBlockAndSetValue(RasBlock rB, int Index_MaxCon)
        {
            double minDif = Double.MaxValue;
            double[] betterArr ;
            int beCenVal_Index = -1;
            double[] arr1 = GetSuitArray(rB.rasterArr, rB.xSize, Index_MaxCon);
            for (int i = rB.xSize + 1; i < (rB.rasterArr.Length - rB.xSize - 1) && (i / rB.xSize) != 0 && ((i + 1) / rB.xSize) != 0; i++)
            {
                double[] arr2 = GetSuitArray(rB.rasterArr, rB.xSize, i);
                double difVal = DifVal(arr1, arr2, rB.NodataVal);
                if (difVal >= 0 && difVal < minDif&&Math.Abs(rB.rasterArr[i]-rB.NodataVal)>0.00001)
                {
                    minDif = difVal;
                    betterArr = arr2;
                    beCenVal_Index = i;
                }
            }
            if (beCenVal_Index == -1)
                throw new Exception("匹配不到合适的栅格块儿！");
            //CopyValue
            if (Math.Abs(rB.rasterArr[Index_MaxCon - rB.xSize - 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon - rB.xSize-1] = rB.rasterArr[beCenVal_Index - rB.xSize-1];
            if (Math.Abs(rB.rasterArr[Index_MaxCon - rB.xSize] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon - rB.xSize] = rB.rasterArr[beCenVal_Index - rB.xSize];
            if (Math.Abs(rB.rasterArr[Index_MaxCon - rB.xSize + 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon - rB.xSize+1] = rB.rasterArr[beCenVal_Index - rB.xSize+1];
            if (Math.Abs(rB.rasterArr[Index_MaxCon - 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon - 1] = rB.rasterArr[beCenVal_Index - 1];
            if (Math.Abs(rB.rasterArr[Index_MaxCon] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon ] = rB.rasterArr[beCenVal_Index];
            if (Math.Abs(rB.rasterArr[Index_MaxCon + 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon + 1] = rB.rasterArr[beCenVal_Index + 1];
            if (Math.Abs(rB.rasterArr[Index_MaxCon + rB.xSize - 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon + rB.xSize - 1] = rB.rasterArr[beCenVal_Index + rB.xSize - 1];
            if (Math.Abs(rB.rasterArr[Index_MaxCon + rB.xSize] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon + rB.xSize] = rB.rasterArr[beCenVal_Index + rB.xSize];
            if (Math.Abs(rB.rasterArr[Index_MaxCon + rB.xSize + 1] - rB.NodataVal) < 0.000001)
                rB.rasterArr[Index_MaxCon + rB.xSize + 1] = rB.rasterArr[beCenVal_Index + rB.xSize + 1];
        }

        private double[] GetSuitArray(double[] SourceArr,int xSize, int Index_MaxCon)//获取Index_MaxCon的八邻域数组
        {
            double[] ar = new double[8];
            ar[0] = SourceArr[Index_MaxCon - xSize-1];
            ar[1] = SourceArr[Index_MaxCon - xSize];
            ar[2] = SourceArr[Index_MaxCon - xSize+1];
            ar[3] = SourceArr[Index_MaxCon - 1];
            ar[4] = SourceArr[Index_MaxCon + 1];
            ar[5] = SourceArr[Index_MaxCon + xSize-1];
            ar[6] = SourceArr[Index_MaxCon + xSize];
            ar[7] = SourceArr[Index_MaxCon + xSize+1];

            return ar;
        }
        private double  DifVal(double[] a, double[] b, double noDataVal)
        {
            if (a.Length == b.Length)
            {
                int cnt = 0;
                double _res = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    if (Math.Abs(b[i] - noDataVal) < 0.00001)
                        return -1;
                    if (Math.Abs(a[i] - noDataVal) > 0.00001)
                    {
                        _res += (a[i] - b[i]) * (a[i] - b[i]);
                        cnt++;
                    }
                }

                if (cnt > 0)
                    return _res / cnt;
                else
                    return -1;//表明块儿b不能参考
            }
            else
                throw new Exception("数据不匹配！");
        }

        #endregion
    }
}
