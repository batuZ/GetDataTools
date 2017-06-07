using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetDataTools._2_坡度线.Core;

namespace GetDataTools._2_坡度线.坡度提取
{
    partial class SlopeDem
    {
        private void TKData(object data)
        {
            DemCutData dcData = (DemCutData)data;
            Dataset demDs = Gdal.Open(dcData.DemPath, Access.GA_ReadOnly);
            int hasval;
            double demNodata, pixel_x, pixel_y;
            double[] geoTransform = new double[6];
            demDs.GetRasterBand(1).GetNoDataValue(out demNodata, out hasval);
            demDs.GetGeoTransform(geoTransform);
            pixel_x = Math.Abs(geoTransform[1]);
            pixel_y = Math.Abs(geoTransform[5]);

            int xTimes = (demDs.RasterXSize - dcData.OverLapPixel) / dcData.Weight;
            int xRemainder = (demDs.RasterXSize - dcData.OverLapPixel) % dcData.Weight;
            int yTimes = (demDs.RasterYSize - dcData.OverLapPixel) / dcData.High;
            int yRemainder = (demDs.RasterYSize - dcData.OverLapPixel) % dcData.High;
            int ax = xTimes, by = yTimes;
            if (xRemainder > dcData.OverLapPixel)
                ax++;
            int x = dcData.cutIndex % ax;
            int y = dcData.cutIndex / ax;
            int xsize = dcData.Weight + dcData.OverLapPixel,
                ysize = dcData.High + dcData.OverLapPixel;
            if (x * dcData.Weight + xsize > demDs.RasterXSize)
                xsize = demDs.RasterXSize - x * dcData.Weight;
            if (y * dcData.High + ysize > demDs.RasterYSize)
                ysize = demDs.RasterYSize - y * dcData.High;

            double[] buffer = new double[xsize * ysize];
            demDs.GetRasterBand(1).ReadRaster(x * dcData.Weight, y * dcData.High, xsize, ysize, buffer, xsize, ysize, 0, 0);

            BasicUnitSlp bunitSlp = new BasicUnitSlp(x * dcData.Weight, y * dcData.High, xsize, ysize, demNodata, slpNodata, buffer, pixel_x, pixel_y);

            double[] resArr = bunitSlp.Calculate();
            lock(slopeDs)
            {
                slopeDs.GetRasterBand(1).WriteRaster(x * dcData.Weight + 1, y * dcData.High + 1, xsize - 2, ysize - 2, resArr, xsize - 2, ysize - 2, 0, 0);

            }
        }
    }
}
