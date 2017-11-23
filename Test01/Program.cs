using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test01
{
    class Program
    {
        static string dsmPath = @"C:\temp\outlinetest02.img";
        static string shpSavePath = @"C:\temp\aa.shp";
        static OSGeo.OGR.Driver shpDataDriver;
        static OSGeo.OGR.DataSource shpDataSet;
        static OSGeo.GDAL.Dataset dsmDataSet;

        static void Main(string[] args)
        {
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();
            shpDataDriver = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            shpDataSet = shpDataDriver.CreateDataSource(shpSavePath, null);

            //1 等高线
            //string dzx1 = getDZX();
            //string dzx2 = cleanDS(dzx1);
            //string dzx = dzPoly(dzx2);
            getDZX_();
            cleanDS_(shpSavePath);
            dzPoly_();
            //2 坡度图
            // string pdt = getPDT();

            //3 坡度线
            // string pdx = getPDX(pdt, 3);

            //4 筛选
            // selectFeat(dzx, pdx, shpSavePath);

            //5 高度值
            //  GetHight.getH(dsmPath, shpSavePath);

            //6 简化
            // jianhua(175, 5);

            Console.ReadLine();
        }

        #region 等高线
        /// <summary>
        /// 获取等高线
        /// </summary>
        /// <returns></returns>
        static string getDZX()
        {
            //无效值
            double noDataValue;

            //0不使用无效值,1使用无效值
            int hasDataValue;

            //读入数据源
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(dsmPath, OSGeo.GDAL.Access.GA_ReadOnly);

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
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            string a = StaticTools.tempFilePath("shp", "原始等值线");
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
        static void getDZX_()
        {
            //无效值
            double noDataValue;

            //0不使用无效值,1使用无效值
            int hasDataValue;

            //读入数据源
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(dsmPath, OSGeo.GDAL.Access.GA_ReadOnly);
            //分析数据源
            inData.GetRasterBand(1).GetNoDataValue(out noDataValue, out hasDataValue);
            OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(inData.GetProjectionRef());
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
            OSGeo.OGR.Layer dzxLayer = shpDataSet.CreateLayer("dzx", srs, OSGeo.OGR.wkbGeometryType.wkbMultiLineString, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            dzxLayer.CreateField(fieldDf0, 1);//ID
            dzxLayer.CreateField(fieldDf1, 1);//Value
            //Band(1), 间隔, 起始高度, 分段数量, 分段值数组, 是否有无效值, 无效值, 预置图层. ID字段, 高度值字段, null , null
            OSGeo.GDAL.Gdal.ContourGenerate(inData.GetRasterBand(1), jianG, min, count, shu, hasDataValue, noDataValue, dzxLayer, 0, 1, null, null);
            if (dzxLayer.GetFeatureCount(0) > 0)
            { Console.WriteLine("等值线创建完成！"); }
            else { Console.WriteLine("等值线创建失败！"); }
            dzxLayer.Dispose();
            inData.Dispose();
        }
        /// <summary>
        /// 通过线长,值 清理等值线
        /// </summary>
        /// <param name="dzx"></param>
        /// <returns></returns>
        static string cleanDS(string dzx)
        {
            Console.WriteLine("开始清理等值线！");
            double aue, bzc; _标准差(dzx, out aue, out bzc);
            double minLength = 50;
            double maxLength = 2600;
            double minValue = aue - bzc * 2;
            double maxValue = aue + bzc * 2;
            //open dzx
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
            OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);
            //new a shp
            string cleanline = StaticTools.tempFilePath("shp", "清理后的等值线");
            OSGeo.OGR.DataSource newdzxDS = dr.CreateDataSource(cleanline, null);
            OSGeo.OGR.Layer newdzxLayer = newdzxDS.CreateLayer(dzxLayer.GetName(), dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);

            for (int i = 0; i < dzxLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();

                double FeatLength = fileGeom.Length();
                bool s1 = FeatLength > minLength && FeatLength < maxLength;

                double featValue = fileFeat.GetFieldAsDouble("EVE");
                bool s2 = featValue > minValue && featValue < maxValue;

                bool isR = fileGeom.IsRing();
                if (s1 && s2 && isR)
                {
                    newdzxLayer.CreateFeature(fileFeat);
                }
                fileFeat.Dispose();
            }
            newdzxDS.Dispose();
            dzxDS.Dispose();
            Console.WriteLine("清理等值线完成！");
            return cleanline;
        }
        static void cleanDS_(string dzx)
        {
            Console.WriteLine("开始清理等值线！");
            //open dzx

            OSGeo.OGR.Layer dzxLayer = shpDataSet.GetLayerByName("aa");
            dzxLayer = shpDataSet.GetLayerByIndex(0);
            //new a shp
            OSGeo.OGR.Layer newdzxLayer = shpDataSet.CreateLayer("clear", dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);
            int dd = shpDataSet.GetLayerCount();
            double aue, bzc; _标准差_(dzxLayer, out aue, out bzc);
            double minLength = 50;
            double maxLength = 2600;
            double minValue = aue - bzc * 2;
            double maxValue = aue + bzc * 2;

            for (int i = 0; i < dzxLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();

                double FeatLength = fileGeom.Length();
                bool s1 = FeatLength > minLength && FeatLength < maxLength;

                double featValue = fileFeat.GetFieldAsDouble("EVE");
                bool s2 = featValue > minValue && featValue < maxValue;

                bool isR = fileGeom.IsRing();
                if (s1 && s2 && isR)
                {
                    newdzxLayer.CreateFeature(fileFeat);
                }
                fileFeat.Dispose();
            }
            dzxLayer.Dispose();
            newdzxLayer.Dispose();
            shpDataSet.DeleteLayer(0);
            Console.WriteLine("清理等值线完成！");
        }
        /// <summary>
        /// 标准差
        /// </summary>
        /// <param name="dzx"></param>
        /// <param name="aue"></param>
        /// <param name="bzc"></param>
        static void _标准差(string dzx, out double aue, out double bzc)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
            OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);

            //获取Featuer数
            int featCount = dzxLayer.GetFeatureCount(0);

            // 1 拿到每个Featuer的Value
            double[] values = new double[featCount];
            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                values[i] = fileFeat.GetFieldAsDouble("EVE");
                fileGeom.Dispose();
                fileFeat.Dispose();
            }
            dzxDS.Dispose();
            // 2 求Values的平均值
            aue = values.Average();
            // 3 求values与平均值差的平方和
            double pingFangHe = 0;
            for (int i = 0; i < featCount; i++)
            {
                pingFangHe += (values[i] - aue) * (values[i] - aue);
            }
            // 4 每个值与平均值的差相加,除Featuer数.再开方,得到标准差
            bzc = Math.Sqrt(pingFangHe / featCount);
        }
        static void _标准差_(OSGeo.OGR.Layer dzxLayer, out double aue, out double bzc)
        {
            //获取Featuer数
            int featCount = dzxLayer.GetFeatureCount(0);

            // 1 拿到每个Featuer的Value
            double[] values = new double[featCount];
            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                values[i] = fileFeat.GetFieldAsDouble("EVE");
                fileGeom.Dispose();
                fileFeat.Dispose();
            }
            // 2 求Values的平均值
            aue = values.Average();

            // 3 求values与平均值差的平方和
            double pingFangHe = 0;
            for (int i = 0; i < featCount; i++)
            {
                pingFangHe += (values[i] - aue) * (values[i] - aue);
            }
            // 4 每个值与平均值的差相加,除Featuer数.再开方,得到标准差
            bzc = Math.Sqrt(pingFangHe / featCount);
        }

        /// <summary>
        /// 等值线转为POLYGON
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static string dzPoly(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");

            string a = StaticTools.tempFilePath("shp", "等值线POLY");
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(a, null);
            OSGeo.OGR.Layer polyLayer = newDS.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            polyLayer.CreateField(fieldDf0, 1);//ID
            polyLayer.CreateField(fieldDf1, 1);//Value
            OSGeo.OGR.FeatureDefn featDF = new OSGeo.OGR.FeatureDefn("");
            Console.WriteLine("开始等值线转POLY！");
            OSGeo.OGR.DataSource cleanDS = dr.Open(filePath, 0);
            OSGeo.OGR.Layer cleanLayer = cleanDS.GetLayerByIndex(0);
            for (int i = 0; i < cleanLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature lineFeat = cleanLayer.GetFeature(i);
                OSGeo.OGR.Geometry lineGeom = lineFeat.GetGeometryRef();

                OSGeo.OGR.Feature polyFeat = new OSGeo.OGR.Feature(featDF);
                OSGeo.OGR.Geometry polyGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                int u = lineGeom.GetPointCount();
                for (int s = 0; s < u; s++)
                {
                    double x = lineGeom.GetX(s);
                    double y = lineGeom.GetY(s);
                    double z = lineGeom.GetZ(s);
                    subGeom.AddPoint(x, y, z);
                }
                polyGeom.AddGeometry(subGeom);
                polyFeat.SetGeometry(polyGeom);
                polyLayer.CreateFeature(polyFeat);
                lineGeom.Dispose();
                polyGeom.Dispose();
                subGeom.Dispose();
                lineFeat.Dispose();
                polyFeat.Dispose();
            }
            cleanLayer.Dispose();
            polyLayer.Dispose();
            cleanDS.Dispose();
            newDS.Dispose();
            Console.WriteLine("等值线转POLY完成！");
            return a;
        }
        static void dzPoly_()
        {
            //创建poly层
            OSGeo.OGR.Layer polyLayer = shpDataSet.CreateLayer("dzPoly", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            polyLayer.CreateField(fieldDf0, 1);//ID
            polyLayer.CreateField(fieldDf1, 1);//Value

            OSGeo.OGR.FeatureDefn featDF = new OSGeo.OGR.FeatureDefn("");
            Console.WriteLine("开始等值线转POLY！");
            OSGeo.OGR.Layer cleanLayer = shpDataSet.GetLayerByName("clear");
            int ii = cleanLayer.GetFeatureCount(0);
            for (int i = 0; i < ii; i++)
            {
                OSGeo.OGR.Feature lineFeat = cleanLayer.GetFeature(i);
                OSGeo.OGR.Geometry lineGeom = lineFeat.GetGeometryRef();

                OSGeo.OGR.Feature polyFeat = new OSGeo.OGR.Feature(featDF);
                OSGeo.OGR.Geometry polyGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                int u = lineGeom.GetPointCount();
                for (int s = 0; s < u; s++)
                {
                    double x = lineGeom.GetX(s);
                    double y = lineGeom.GetY(s);
                    double z = lineGeom.GetZ(s);
                    subGeom.AddPoint(x, y, z);
                }
                polyGeom.AddGeometry(subGeom);
                polyFeat.SetGeometry(polyGeom);
                polyLayer.CreateFeature(polyFeat);
                lineGeom.Dispose();
                polyGeom.Dispose();
                subGeom.Dispose();
                lineFeat.Dispose();
                polyFeat.Dispose();
            }
            cleanLayer.Dispose();
            polyLayer.Dispose();
            Console.WriteLine("等值线转POLY完成！");
        }

        #endregion

        #region 坡度图
        //获取坡度图
        static string getPDT()
        {
            string _outFilePath = StaticTools.tempFilePath("img", "SolpeMap");
            Sloping(dsmPath, _outFilePath);
            return _outFilePath;
        }

        static AutoResetEvent asyRE = new AutoResetEvent(false);
        static OSGeo.GDAL.Dataset slopeDs;
        static double slpNodata;

        static void Sloping(string DemPath, string OutSlpPath)
        {
            Stopwatch sw = new Stopwatch(); sw.Start();
            Console.WriteLine("【开始创建SlopeMap！】");
            int hasVal;
            double demNodata;
            double[] geoTransform = new double[6];
            //打开
            OSGeo.GDAL.Dataset InDataset = OSGeo.GDAL.Gdal.Open(DemPath, OSGeo.GDAL.Access.GA_Update);
            InDataset.GetRasterBand(1).GetNoDataValue(out demNodata, out hasVal);
            InDataset.GetGeoTransform(geoTransform);

            //调用GDal创建影像，声明影像格式
            OSGeo.GDAL.Driver gdalDriver = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            slopeDs = gdalDriver.Create(OutSlpPath, InDataset.RasterXSize, InDataset.RasterYSize, 1, OSGeo.GDAL.DataType.GDT_Float32, null);
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

            //asyRE = new AutoResetEvent(false);
            //Thread th = new Thread(() =>
            //{
            MySloping(DemPath);
            //});
            //th.Start();
            //asyRE.WaitOne();
            InDataset = OSGeo.GDAL.Gdal.Open(DemPath, OSGeo.GDAL.Access.GA_ReadOnly);
            //FixOutLineRaster(InDataset.GetRasterBand(1), demNodata);
            //处理外围的Raster
            BufferOnePixel(slopeDs.GetRasterBand(1));
            slopeDs.Dispose();
            sw.Stop(); Console.WriteLine("【SlopeMap完成，用时：" + sw.Elapsed.ToString() + "】");
        }
        private static async void MySloping(string DemPath)
        {
            OSGeo.GDAL.Dataset _inDataset = OSGeo.GDAL.Gdal.Open(DemPath, OSGeo.GDAL.Access.GA_ReadOnly);

            int iCount = 0, Maxcnt = GetCutNumberOfImg(DemPath, 300, 4);
            for (int i = 0; i < Maxcnt; i++)
            {

                DemCutData data = new DemCutData(DemPath, i, 300, 300, 4);
                //await Task.Run(() =>
                //{
                TKData(data);
                //iCount++;
                //    if (iCount == Maxcnt)
                //        asyRE.Set();
                //});
            }
        }
        private static void BufferOnePixel(OSGeo.GDAL.Band TargetBand)
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
        private static int GetCutNumberOfImg(string imgPath, int CutWeight, int OverlapWeight)
        {
            OSGeo.GDAL.Dataset _inDataset = OSGeo.GDAL.Gdal.Open(imgPath, OSGeo.GDAL.Access.GA_ReadOnly);

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
        private static void TKData(object data)
        {
            DemCutData dcData = (DemCutData)data;
            OSGeo.GDAL.Dataset demDs = OSGeo.GDAL.Gdal.Open(dcData.DemPath, OSGeo.GDAL.Access.GA_ReadOnly);
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
            lock (slopeDs)
            {
                slopeDs.GetRasterBand(1).WriteRaster(x * dcData.Weight + 1, y * dcData.High + 1, xsize - 2, ysize - 2, resArr, xsize - 2, ysize - 2, 0, 0);

            }
        }
        #endregion

        #region 坡度线
        static string getPDX(string slopMap, int Lev)
        {
            List<string> levelFiles = new List<string>();
            for (int i = 0; i < Lev; i++)
            {
                Stopwatch ssw = new Stopwatch(); ssw.Start(); Console.WriteLine("开始计算第{0}级SlopePolygon" + i + 1);
                string _outShpPath = StaticTools.tempFilePath("shp", "SolpePolyLev" + i.ToString());
                MyGetOutlines(dsmPath, slopMap, _outShpPath, 80 - i * 5);
                levelFiles.Add(_outShpPath);
                ssw.Stop(); Console.WriteLine("第{0}级SlopePolygon完成，用时{1}！", i + 1, ssw.Elapsed.ToString());
            }
            string res = CutImg.zhengHePdx(levelFiles);
            return res;
        }

        static QThread qthread;
        static void MyGetOutlines(string inDataPath, string _outDSpath, string OutShpPath, double ImprotLevel)
        {
            //qthread = new QThread(OutShpPath);
            //asyRE = new AutoResetEvent(false);
            //Thread th = new Thread(() =>
            //{
            GetOlines00(inDataPath, _outDSpath, OutShpPath, ImprotLevel);
            //});
            //th.Start();
            //asyRE.WaitOne();
            //qthread.Close();
        }
        private static async void GetOlines00(string inDataPath, string _outDSpath, string OutShpPath, double ImprotLevel = 80)
        {

            // OSGeo.GDAL.Dataset _inDataset = OSGeo.GDAL.Gdal.Open(inDataPath, OSGeo.GDAL.Access.GA_ReadOnly);
            // OSGeo.GDAL.Dataset _outDataset = OSGeo.GDAL.Gdal.Open(_outDSpath, OSGeo.GDAL.Access.GA_ReadOnly);


            int iCount = 0, Maxcnt = GetCutNumberOfImg(inDataPath);
            for (int i = 0; i < Maxcnt; i++)
            {
                CutData data = new CutData(inDataPath, _outDSpath, OutShpPath, i, ImprotLevel);
                //await Task.Run(() =>
                //{
                TKData(data);
                //    iCount++;
                //    if (iCount == Maxcnt)
                //        asyRE.Set();
                //});
            }
        }
        /// <summary>
        /// 获取图像分块儿数
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        private static int GetCutNumberOfImg(string imgPath)
        {
            OSGeo.GDAL.Dataset _inDataset = OSGeo.GDAL.Gdal.Open(imgPath, OSGeo.GDAL.Access.GA_ReadOnly);

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


        #endregion

        #region 筛选
        static void selectFeat(string dzx, string pdx, string savePath)
        {
            Stopwatch ssww = new Stopwatch(); ssww.Start();
            //获得数组
            int[] a = getMinIdGroupM(dzx, pdx);

            Console.WriteLine("ID to Feater...");
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzDS = dr.Open(dzx, 0);
            OSGeo.OGR.Layer dzLayer = dzDS.GetLayerByIndex(0);
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            //get featuer by ID
            for (int i = 0; i < a.Length; i++)
            {
                fileLayer.CreateFeature(dzLayer.GetFeature(a[i]));
                dzLayer.GetFeature(a[i]).Dispose();
            }
            dzLayer.Dispose();
            dzDS.Dispose();
            fileLayer.Dispose();
            ds.Dispose();
            cleanPdxM(savePath);
            ssww.Stop();
            Console.WriteLine("提取轮廓线完成！用时：{0}", ssww.Elapsed.ToString());
        }
        /// <summary>
        /// 通过是否相交和面积差,获得最小面积差相交要素的ID数组--------多线OK
        /// </summary>
        /// <param name="dzx"></param>
        /// <param name="pdx"></param>
        /// <returns></returns>
        private static int[] getMinIdGroupM(string dzx, string pdx)
        {
            Console.WriteLine("起用多线程提取轮廓线ID....");

            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource pdDS = dr.Open(pdx, 0);
            OSGeo.OGR.Layer pdLayer = pdDS.GetLayerByIndex(0);

            int pdCount = pdLayer.GetFeatureCount(0);
            for (int pdi = 0; pdi < pdCount; pdi++)
            {
                OSGeo.OGR.Feature pdFeat = pdLayer.GetFeature(pdi);
                select(pdFeat, dzx);
            }

            while (tickTime < pdCount)
            {
                Console.WriteLine("第{0}个完成，共{1}个", tickTime, pdCount);
                Thread.Sleep(2000);
            }

            pdLayer.Dispose();
            pdDS.Dispose();
            Console.WriteLine("多线程提取ID完成.....");
            return minIdG.ToArray();
        }

        static List<int> minIdG = new List<int>();
        static int tickTime;


        /// <summary>
        /// 子线程提取轮廓线ID
        /// </summary>
        /// <param name="pdFeat"></param>
        /// <param name="dzx"></param>
        async static void select(OSGeo.OGR.Feature pdFeat, string dzx)
        {
            await Task.Run(() =>
            {
                double afterCha = -1;
                int yesID = -1;

                OSGeo.OGR.Ogr.RegisterAll();
                OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
                OSGeo.OGR.DataSource dzDS = dr.Open(dzx, 0);
                OSGeo.OGR.Layer dzxLayer = dzDS.GetLayerByIndex(0);

                OSGeo.OGR.Geometry pdGeom = pdFeat.GetGeometryRef();

                int dzCount = dzxLayer.GetFeatureCount(0);

                for (int dzi = 0; dzi < dzCount; dzi++)
                {
                    OSGeo.OGR.Feature dzFeat = dzxLayer.GetFeature(dzi);
                    OSGeo.OGR.Geometry dzGeom = dzFeat.GetGeometryRef();

                    if (pdGeom.Intersect(dzGeom))
                    {
                        double cha = Math.Abs(dzGeom.GetArea() - pdGeom.GetArea());
                        if (afterCha == -1)
                        {
                            afterCha = cha;
                            yesID = dzi;
                        }
                        else if (cha < afterCha)
                        {
                            afterCha = cha;
                            yesID = dzi;
                        }
                    }

                    dzGeom.Dispose();
                    dzFeat.Dispose();
                }

                if (yesID != -1)
                {
                    minIdG.Add(yesID);
                }
                dzDS.Dispose();
                dzxLayer.Dispose();
                pdGeom.Dispose();
                pdFeat.Dispose();
                tickTime++;
            });
        }


        static List<int> ids = new List<int>();
        static int tickTime1;
        /// <summary>
        /// 清理重复的Featuer----------多线程----与单线程效率差别很小，不推荐
        /// </summary>
        /// <param name="filePath"></param>
        public static void cleanPdxM(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(filePath, 1);
            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
            int featCount = layer.GetFeatureCount(0);
            Console.WriteLine("开始多线程删除重复内容！");
            var sw = new System.Diagnostics.Stopwatch(); sw.Start();
            //初始化
            tickTime1 = 0;
            ids.Clear();
            for (int i = 0; i < featCount - 1; i++)
            {
                getDoubFeat(filePath, i, featCount);
            }
            while (tickTime1 < featCount - 1)
            {
                Thread.Sleep(2000);
                Console.WriteLine("当前执行：" + tickTime1.ToString() + "/" + featCount.ToString());
            }
            sw.Stop(); Console.WriteLine("多线程清理重复Featuer用时：" + sw.Elapsed.ToString());

            for (int i = 0; i < ids.Count; i++)
            {
                layer.DeleteFeature(ids[i]);
            }
            //string a = "REPACK " + layer.GetName();
            //ds.ExecuteSQL(a, null, "");
            ds.deleteFeatUpdate();
            ds.Dispose();
        }
        /// <summary>
        /// 清理重复的Featuer----------单线程
        /// </summary>
        /// <param name="pdx"></param>
        public static void cleanPdx(string pdx)
        {
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(pdx, 1);
            OSGeo.OGR.Layer newLayer = ds.GetLayerByIndex(0);
            int 重复的 = 0;
            for (int i = 0; i < newLayer.GetFeatureCount(0) - 1; i++)
            {
                OSGeo.OGR.Feature ori = newLayer.GetFeature(i);
                for (int j = i + 1; j < newLayer.GetFeatureCount(0); j++)
                {
                    OSGeo.OGR.Feature next = newLayer.GetFeature(j);
                    bool a = StaticTools.isSame(ori, next, 1);
                    if (a)
                    {
                        newLayer.DeleteFeature(i);
                        重复的++;
                        Console.WriteLine("已删除{0}个重复Featuer,allFeat is {1}/{2}", 重复的, i + 1, newLayer.GetFeatureCount(0));
                        break;
                    }
                    next.Dispose();
                }
                ori.Dispose();
            }
            string layerName = newLayer.GetName();
            ds.ExecuteSQL("REPACK " + layerName, null, "");
            ds.Dispose();
        }

        async static void getDoubFeat(string filePath, int s, int ss)
        {
            await Task.Run(() =>
            {
                OSGeo.OGR.Ogr.RegisterAll();
                OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
                OSGeo.OGR.DataSource ds = dr.Open(filePath, 0);
                OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
                OSGeo.OGR.Feature oriFeat = layer.GetFeature(s);
                for (int i = s + 1; i < ss; i++)
                {
                    OSGeo.OGR.Feature nextFeat = layer.GetFeature(i);
                    if (StaticTools.isSame(oriFeat, nextFeat, 1))
                    {
                        ids.Add(s);
                        break;
                    }
                    nextFeat.Dispose();
                }
                oriFeat.Dispose();
                layer.Dispose();
                ds.Dispose();
                tickTime++;
            }
            );
        }
        #endregion

        #region 简化
        /// <summary>
        /// 清理原文件
        /// </summary>
        /// <param name="jiaodu"></param>
        /// <param name="cishu"></param>
        static void jianhua(double jiaodu, int cishu)
        {
            for (int i = cishu; i > 0; i--)
                claenPoint(shpSavePath, jiaodu, i);
        }
        public static void claenPoint(string filePath, double jiaodu, int cishu)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource oriDs = dr.Open(filePath, 1);
            OSGeo.OGR.Layer oriLayer = oriDs.GetLayerByIndex(0);

            int featCount = oriLayer.GetFeatureCount(0);

            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature oriFeat = oriLayer.GetFeature(i);
                OSGeo.OGR.Geometry oriGeom = oriFeat.GetGeometryRef();
                OSGeo.OGR.Geometry subGeom = oriGeom.GetGeometryRef(0);

                int pointCount = subGeom.GetPointCount();

                Point[] aFeat = new Point[pointCount];

                for (int c = 0; c < pointCount; c++)
                {
                    aFeat[c].X = subGeom.GetX(c);
                    aFeat[c].Y = subGeom.GetY(c);
                    aFeat[c].Z = subGeom.GetZ(c);
                }

                OSGeo.OGR.Geometry newGeom = null;
                if (aFeat.Length > cishu * 3)
                {
                    newGeom = JID(aFeat, jiaodu, cishu);
                }
                else
                {
                    oriFeat.Dispose();
                    continue;
                }
                if (newGeom != null)
                {
                    oriFeat.SetGeometry(newGeom);
                    oriLayer.SetFeature(oriFeat);
                }
                Console.WriteLine("cleanPoint已完成{0}/{1}", i, featCount);
                oriFeat.Dispose();
            }
            oriDs.Dispose();
        }
        /// <summary>
        /// 三点夹角的判定条件,输出为满足条件的成员的ID所组成的ID数组
        /// </summary>
        /// <param name="aFeat"></param>
        /// <returns></returns>
        private static OSGeo.OGR.Geometry JID(Point[] aFeat, double userSet, int seleTime)
        {
            List<Point[]> pjGroupL = new List<Point[]>();
            List<Point[]> zjGroupL = new List<Point[]>();

            List<Point> pjGroup = new List<Point>();
            List<Point> zjGroup = new List<Point>();

            for (int i = 0; i < aFeat.Length; i++)
            {
                int frontId, thisId, backId;
                bool[] yon = new bool[seleTime];
                for (int t = 1; t <= seleTime; t++)
                {
                    frontId = i < t ? aFeat.Length - 1 + i - t : i - t;

                    thisId = i;

                    backId = i > aFeat.Length - 1 - t ? i - (aFeat.Length - 1) + t : backId = i + t;

                    double jiaodu = cosCalculator(aFeat[frontId], aFeat[thisId], aFeat[backId]);

                    yon[t - 1] = jiaodu > userSet;
                }

                if (yon.Contains(true))
                {
                    pjGroup.Add(aFeat[i]);
                }
                else
                {
                    zjGroup.Add(aFeat[i]);
                }
            }

            OSGeo.OGR.Geometry outGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
            OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);

            for (int g = 0; g < zjGroup.Count(); g++)
            {
                Point a = zjGroup[g];
                subGeom.AddPoint(a.X, a.Y, a.Z);
            }
            if (subGeom.GetPointCount() < 4)
            {
                return null;
            }
            subGeom.CloseRings();
            outGeom.AddGeometry(subGeom);
            return outGeom;
        }
        private static double cosCalculator(Point p1, Point p, Point p2)   /// 求夹角
        {
            double fenzi = (p1.X - p.X) * (p2.X - p.X) + (p1.Y - p.Y) * (p2.Y - p.Y);
            double fenmu = Math.Sqrt((p1.X - p.X) * (p1.X - p.X) + (p1.Y - p.Y) * (p1.Y - p.Y)) * Math.Sqrt((p2.X - p.X) * (p2.X - p.X) + (p2.Y - p.Y) * (p2.Y - p.Y));
            double cosValue = fenzi / fenmu;
            double acosV = Math.Acos(cosValue) * 180 / Math.PI;
            return acosV;
        }

        #endregion
    }
    public partial class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

    }
}
