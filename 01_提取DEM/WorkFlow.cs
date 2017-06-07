using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools._01_提取DEM
{
    class WorkFlow
    {
        public static void getDEM(string dsmIn, string shpIn, string demSave)
        {
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();

            string tempOutRaster = StaticTools.tempFilePath("img", "DEMtemp");

            Action callBack = (() => { MessageBox.Show("程序运行结束！"); });//程序结束后的回调函数，如无，可设为null

            //路径,Buffer           
   //         RasterizeCls.PreInit(ref imgPath, ref shpPath, out tempOutRaster, tempDirPath);
            //把目标转为NoData,tempOutRaster 是通过Poly转栅格
            RasterizeCls.Rasterize(shpIn, dsmIn, tempOutRaster);
            //
            RasterizeCls.RasterExtract(dsmIn, tempOutRaster, shpIn, callBack);
        }
    }
}
