using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetDataTools._2_坡度线.Core;
using System.Diagnostics;

namespace GetDataTools._2_坡度线.坡度提取
{
    partial class SlopeDem
    {
        private static SlopeDem _instance;
        public static SlopeDem Instance
        {
            get{
                if (_instance == null)
                    _instance = new SlopeDem();
                return _instance;
            }
        }

        private Dataset slopeDs;
        double slpNodata;
        public void Sloping(string DemPath, string OutSlpPath)
        {
            Gdal.AllRegister();
            Stopwatch sw = new Stopwatch(); sw.Start();
            Console.WriteLine("【开始创建SlopeMap！】");
            int hasVal;
            double demNodata;
            double[] geoTransform = new double[6];
            //打开
            Dataset InDataset = Gdal.Open(DemPath, Access.GA_Update);
            InDataset.GetRasterBand(1).GetNoDataValue(out demNodata, out hasVal);
            InDataset.GetGeoTransform(geoTransform);

            //调用GDal创建影像，声明影像格式
            OSGeo.GDAL.Driver gdalDriver = Gdal.GetDriverByName("HFA");
            slopeDs = gdalDriver.Create(OutSlpPath, InDataset.RasterXSize, InDataset.RasterYSize, 1, DataType.GDT_Float32, null);
            slopeDs.SetProjection(InDataset.GetProjection());
            slopeDs.SetGeoTransform(geoTransform);
            if (hasVal == 0)
            {
                InDataset.GetRasterBand(1).SetNoDataValue(-100000);
                slopeDs.GetRasterBand(1).SetNoDataValue(-100000);
                slpNodata = -100000;
            }
            else
            {
                slopeDs.GetRasterBand(1).SetNoDataValue(demNodata);
                slpNodata = demNodata;
            }
            InDataset.Dispose();

            asyRE = new AutoResetEvent(false);
            Thread th = new Thread(() =>
            {
                MySloping(DemPath);
            });
            th.Start();
            asyRE.WaitOne();
            InDataset = Gdal.Open(DemPath, Access.GA_ReadOnly);
            //FixOutLineRaster(InDataset.GetRasterBand(1), demNodata);
            //处理外围的Raster
            BufferOnePixel(slopeDs.GetRasterBand(1));
            slopeDs.Dispose();
            sw.Stop(); Console.WriteLine("【SlopeMap完成，用时：" + sw.Elapsed.ToString()+"】");
        }
        AutoResetEvent asyRE = new AutoResetEvent(false);
        private async void MySloping(string DemPath)
        {
            Dataset _inDataset = Gdal.Open(DemPath, Access.GA_ReadOnly);

            int iCount = 0, Maxcnt = GetCutNumberOfImg(DemPath, 300, 4);
            for (int i = 0; i < Maxcnt; i++)
            {

                DemCutData data = new DemCutData(DemPath, i, 300, 300, 4);
                await Task.Run(() =>
                {
                    TKData(data);
                    iCount++;
                    if (iCount == Maxcnt)
                        asyRE.Set();
                });
            }
        }

        private int GetCutNumberOfImg(string imgPath, int CutWeight, int OverlapWeight)
        {
            Dataset _inDataset = Gdal.Open(imgPath, Access.GA_ReadOnly);

            int xTimes = (_inDataset.RasterXSize - OverlapWeight) / CutWeight;
            int xRemainder = (_inDataset.RasterXSize - OverlapWeight) % CutWeight;
            int yTimes = (_inDataset.RasterYSize - OverlapWeight) / CutWeight;
            int yRemainder = (_inDataset.RasterYSize - OverlapWeight) % CutWeight;
            int ax = xTimes, by = yTimes;
            if (xTimes == 0)
            { xRemainder = _inDataset.RasterXSize; }
            if (yTimes == 0)
            { yRemainder = _inDataset.RasterYSize; }
            if (xRemainder > OverlapWeight)
                ax++;
            if (yRemainder > OverlapWeight)
                by++;
            int Maxcnt = ax * by;

            return Maxcnt;
        }

        private void FixOutLineRaster(Band band,double demNodata)
        {
            int xOff, yOff, xSize, ySize;
            double[] slpBuffer,getBuffer;
            xOff = 0; yOff = 0; xSize = slopeDs.RasterXSize; ySize = 1;
            slpBuffer = new double[xSize * ySize];
            slopeDs.GetRasterBand(1).ReadRaster(xOff, yOff + 1, xSize, ySize, slpBuffer, xSize, ySize, 0, 0);
            getBuffer = GetRasterOutline(band, demNodata, xOff, yOff, xSize, ySize, slpBuffer);
            slopeDs.GetRasterBand(1).WriteRaster(xOff, yOff, xSize, ySize, getBuffer, xSize, ySize, 0, 0);

            xOff = 0; yOff = slopeDs.RasterYSize-1; xSize = slopeDs.RasterXSize; ySize = 1;
            slpBuffer = new double[xSize * ySize];
            slopeDs.GetRasterBand(1).ReadRaster(xOff, yOff - 1, xSize, ySize, slpBuffer, xSize, ySize, 0, 0);
            getBuffer = GetRasterOutline(band, demNodata, xOff, yOff, xSize, ySize, slpBuffer);
            slopeDs.GetRasterBand(1).WriteRaster(xOff, yOff, xSize, ySize, getBuffer, xSize, ySize, 0, 0);

            xOff = 0; yOff = 0; xSize = 1; ySize = slopeDs.RasterYSize;
            slpBuffer = new double[xSize * ySize];
            slopeDs.GetRasterBand(1).ReadRaster(xOff, yOff , xSize, ySize, slpBuffer, xSize, ySize, 0, 0);
            getBuffer = GetRasterOutline(band, demNodata, xOff, yOff, xSize, ySize, slpBuffer);
            slopeDs.GetRasterBand(1).WriteRaster(xOff, yOff, xSize, ySize, getBuffer, xSize, ySize, 0, 0);

            xOff = slopeDs.RasterXSize-1; yOff = 0; xSize = 1; ySize = slopeDs.RasterYSize;
            slpBuffer = new double[xSize * ySize];
            slopeDs.GetRasterBand(1).ReadRaster(xOff-1, yOff , xSize, ySize, slpBuffer, xSize, ySize, 0, 0);
            getBuffer = GetRasterOutline(band, demNodata, xOff, yOff, xSize, ySize, slpBuffer);
            slopeDs.GetRasterBand(1).WriteRaster(xOff, yOff, xSize, ySize, getBuffer, xSize, ySize, 0, 0);
        }

        private double[] GetRasterOutline(Band demBand, double demNodata, int xoff, int yoff, int xsize, int ysize, double[] slpLineBuffer)
        {
            double[] r = slpLineBuffer;

            double[] demLineBuffer = new double[xsize * ysize];
            demBand.ReadRaster(xoff, yoff, xsize, ysize, demLineBuffer, xsize, ysize, 0, 0);
            for (int i = 0; i < demLineBuffer.Length; i++)
            {
                if (Math.Abs(demLineBuffer[i] - demNodata) < 0.00001)
                    r[i] = slpNodata;
            }
            return r;
        }

        private void BufferOnePixel(Band TargetBand)
        {
            double[] readArr = new double[TargetBand.XSize];
            TargetBand.ReadRaster(0, 1, TargetBand.XSize, 1, readArr, TargetBand.XSize, 1, 0, 0);
            TargetBand.WriteRaster(0, 0, TargetBand.XSize, 1, readArr, TargetBand.XSize, 1, 0, 0);

            TargetBand.ReadRaster(0, TargetBand.YSize - 2, TargetBand.XSize, 1, readArr, TargetBand.XSize, 1, 0, 0);
            TargetBand.WriteRaster(0, TargetBand.YSize - 1, TargetBand.XSize, 1, readArr, TargetBand.XSize, 1, 0, 0);

            readArr = new double[TargetBand.YSize];
            TargetBand.ReadRaster(1, 0, 1, TargetBand.YSize, readArr, 1, TargetBand.YSize, 0, 0);
            TargetBand.WriteRaster(0, 0, 1, TargetBand.YSize, readArr, 1, TargetBand.YSize, 0, 0);

            TargetBand.ReadRaster(TargetBand.XSize - 2, 0, 1, TargetBand.YSize, readArr, 1, TargetBand.YSize, 0, 0);
            TargetBand.WriteRaster(TargetBand.XSize - 1, 0, 1, TargetBand.YSize, readArr, 1, TargetBand.YSize, 0, 0);
        }
    }

}
