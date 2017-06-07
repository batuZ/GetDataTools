using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._1_等值线
{
    class 生成等值线
    {
        /************************************   生成等值线  **********************************************/
      
        public static string dzx(string filePath)
        {
            Console.WriteLine("开始创建等值线！");
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.GDAL.Gdal.AllRegister();
            //无效值
            double noDataValue;
            
            //0不使用无效值,1使用无效值
            int hasDataValue;

            //读入数据源
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(filePath, OSGeo.GDAL.Access.GA_ReadOnly);

            //分析数据源
            inData.GetRasterBand(1).GetNoDataValue(out noDataValue, out hasDataValue);

            double min, max, mean, std;
            inData.GetRasterBand(1).GetStatistics(0, 1, out min, out max, out mean, out std);

            int jianG = 2;

            int count = Convert.ToInt32((max - min) / jianG + 0.5);

            double[] shu = new double[count];

            for (int i = 0; i < count; i++)
            {
                shu[i] = min + jianG * i;
            }

            //创建空的SHP
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            string a = StaticTools.tempFilePath("shp","原始等值线");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(a, null);
            OSGeo.OGR.Layer dzxLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbMultiLineString, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            dzxLayer.CreateField(fieldDf0, 1);//ID
            dzxLayer.CreateField(fieldDf1, 1);//Value
            //Band(1), 间隔, 起始高度, 分段数量, 分段值数组, 是否有无效值, 无效值, 预置图层. ID字段, 高度值字段, null , null
            OSGeo.GDAL.Gdal.ContourGenerate(inData.GetRasterBand(1), jianG, min, count, shu, hasDataValue, noDataValue, dzxLayer, 0, 1, null, null);
            if (dzxLayer.GetFeatureCount(0) > 0)
            { Console.WriteLine("等值线创建完成！"); }
            else { Console.WriteLine("等值线创建失败！"); }
            ds.Dispose();
            inData.Dispose();
            return a;
        }
    }
}
