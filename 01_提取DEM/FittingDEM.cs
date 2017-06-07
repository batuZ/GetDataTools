using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools._01_提取DEM
{
    /// <summary>
    /// 拟合曲面算法~---未完成
    /// </summary>
    class FittingDEM
    {

        /// <summary>
        /// 通过Feature设置栅格无效值
        /// </summary>
        /// <param name="polyFeat"></param>
        /// <param name="inDs"></param>
        public static void setNoData(Feature polyFeat, Dataset inDs)
        {
            //注册库
            Gdal.AllRegister();
            Ogr.RegisterAll();

            //Get Geometry & Envelope
            Geometry Geom = polyFeat.GetGeometryRef();
            Envelope enve = new Envelope();
            Geom.GetEnvelope(enve);

            //Get all Transform
            double[] GeoTransform = new double[6];
            inDs.GetGeoTransform(GeoTransform);

            // Envelope Conver to ImgSpace -- off to end 
            int xoff, yoff, xend, yend;
            StaticTools.geoToImageSpace(GeoTransform, enve.MinX, enve.MaxY, out xoff, out yoff);
            StaticTools.geoToImageSpace(GeoTransform, enve.MaxX, enve.MinY, out xend, out yend);
            int xsize = xend - xoff;
            int ysize = yend - yoff;

            //Get The Values 
            double[] rasterValues = new double[xsize * ysize];
            inDs.GetRasterBand(1).ReadRaster(xoff, yoff, xsize, ysize, rasterValues, xsize, ysize, 0, 0);

            //Get new Transform
            double[] NewTransform = GeoTransform;
            NewTransform[0] = enve.MinX;
            NewTransform[3] = enve.MaxY;

            //无效值，特定值
            double noData = 9999;

            for (int iline = 0; iline < ysize; iline++)
            {
                for (int iPixel = 0; iPixel < xsize; iPixel++)
                {
                    Geometry poiGeom = new Geometry(wkbGeometryType.wkbPoint);
                    double x, y;
                    StaticTools.imageToGeoSpace(NewTransform, iPixel, iline, out x, out y);
                    poiGeom.AddPoint_2D(x, y);
                    if (poiGeom.Within(Geom))
                    {
                        rasterValues[iline * xsize + iPixel] = noData;
                    }
                    poiGeom.Dispose();
                }
            }
            inDs.GetRasterBand(1).WriteRaster(xoff, yoff, xsize, ysize, rasterValues, xsize, ysize, 0, 0);
            inDs.Dispose();
            polyFeat.Dispose();
        }

        public static void dd(string filepath)
        {
            Gdal.AllRegister();
            Dataset indata = Gdal.Open(filepath, Access.GA_Update);
           int ySize =  indata.RasterYSize;
           for (int i = 0; i < ySize; i++)
           {
               读取当前行及值fitting(indata, i);
               Console.WriteLine("{0}/{1}", i, ySize);
           }
           indata.Dispose();
        }

       static  void 读取当前行及值fitting(Dataset inds, int line)
        {
            double[] values = new double[inds.RasterXSize];
            inds.GetRasterBand(1).ReadRaster(0, line, inds.RasterXSize, 1, values, inds.RasterXSize, 1, 0, 0);
            if (Array.IndexOf(values, 9999) < 0)
            { 
                return; 
            }
            int a = Array.IndexOf(values, 9999) - 5 < 0 ? 0 : Array.IndexOf(values, 9999) - 5;
            int b = Array.LastIndexOf(values, 9999) + 5 > inds.RasterXSize ? inds.RasterXSize : Array.LastIndexOf(values, 9999) + 5;
            List<double> xArray = new List<double>();
            List<double> valArry = new List<double>();
            for (int i = a; i < b ; i++)
            {
                if (values[i] != 9999)
                {
                    xArray.Add(i);
                    valArry.Add(values[i]);
                }
            }
            double[] fitting = StaticTools.MultiLine(xArray.ToArray(), valArry.ToArray(), xArray.Count, 3);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == 9999)
                {
                    values[i] = fitting[0] + fitting[1] * i + fitting[2] * i * i + fitting[3] * i * i * i;
                }
            }
            inds.GetRasterBand(1).WriteRaster(0, line, inds.RasterXSize, 1, values, inds.RasterXSize, 1, 0, 0);
        }
       static void 读取当前列及值fitting(Dataset inds, int pixel)
       {
           double[] values = new double[inds.RasterXSize];
           inds.GetRasterBand(1).ReadRaster(pixel, 0, 1, inds.RasterYSize, values, 1, inds.RasterYSize, 0, 0);
           if (Array.IndexOf(values, 9999) < 0)
           {
               return;
           }
           int a = Array.IndexOf(values, 9999) - 5 < 0 ? 0 : Array.IndexOf(values, 9999) - 5;
           int b = Array.LastIndexOf(values, 9999) + 5 > inds.RasterXSize ? inds.RasterXSize : Array.LastIndexOf(values, 9999) + 5;
           List<double> xArray = new List<double>();
           List<double> valArry = new List<double>();
           for (int i = a; i < b; i++)
           {
               if (values[i] != 9999)
               {
                   xArray.Add(i);
                   valArry.Add(values[i]);
               }
           }
           double[] fitting = StaticTools.MultiLine(xArray.ToArray(), valArry.ToArray(), xArray.Count, 3);
           for (int i = 0; i < values.Length; i++)
           {
               if (values[i] == 9999)
               {
                   values[i] = fitting[0] + fitting[1] * i + fitting[2] * i * i + fitting[3] * i * i * i;
               }
           }
           inds.GetRasterBand(1).WriteRaster(pixel, 0, 1, inds.RasterYSize, values, 1, inds.RasterYSize, 0, 0);
       }
    }
}
