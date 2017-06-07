using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetDataTools._2_坡度线.Common;
using GetDataTools._2_坡度线.Core;

namespace GetDataTools._2_坡度线.OutLine
{
    partial class GetOutlines
    {
        void TKData(object n)
        {
            int hasVal;
            double nodataV;
            double importLevel;
            double[] geoTansform = new double[6];

            CutData cutData = (CutData)n;
            importLevel = cutData.ImportLevel;
            Dataset demDs = Gdal.Open(cutData.DemPath,Access.GA_ReadOnly);
            Dataset slpDs = Gdal.Open(cutData.SlopePath, Access.GA_ReadOnly);
            demDs.GetRasterBand(1).GetNoDataValue(out nodataV, out hasVal);
            if (hasVal == 0)
                throw new Exception("该栅格未设置Nodata！");
            demDs.GetGeoTransform(geoTansform);

            int xTimes = (demDs.RasterXSize - 300) / 500;
            int xRemainder = (demDs.RasterXSize - 300) % 500;
            int yTimes = (demDs.RasterYSize - 300) / 500;
            int yRemainder = (demDs.RasterYSize - 300) % 500;
            int ax = xTimes, by = yTimes;
            if (xRemainder > 10)
                ax++;
            int x = cutData.cutIndex % ax;
            int y = cutData.cutIndex / ax;
            int xsize = 800, ysize = 800;
            if (x * 500 + 800 > demDs.RasterXSize)
                xsize = demDs.RasterXSize - x * 500;
            if (y * 500 + 800 > demDs.RasterYSize)
                ysize = demDs.RasterYSize - y * 500;

            double[] buffer = new double[xsize * ysize];
            double[] slopebuffer = new double[xsize * ysize];
            demDs.GetRasterBand(1).ReadRaster(x * 500, y * 500, xsize, ysize, buffer, xsize, ysize, 0, 0);
            slpDs.GetRasterBand(1).ReadRaster(x * 500, y * 500, xsize, ysize, slopebuffer, xsize, ysize, 0, 0);

            BasicUnit bUnit = new BasicUnit(x * 500, y * 500, xsize, ysize,
                                    importLevel, buffer, slopebuffer, geoTansform, nodataV);

            List<Geometry> geolist = bUnit.Identify2();
            Console.WriteLine("一个图块儿计算完毕");
            lock (qthread)
            {
                foreach (Geometry item in geolist)
                {
                    qthread.WritePolygonShp(item);
                }
            }
            demDs.Dispose();
            slpDs.Dispose();
        }
    }
}
