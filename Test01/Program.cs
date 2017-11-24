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
        static string dsmPath = @"E:\test\imgData\outlinetest02.img";
        static string shpSavePath = @"C:\temp\asf";
        static string slopeImgSavePath = @"C:\temp\asf\uu.img";
        static OSGeo.OGR.Driver shpDataDriver;              //shp文件驱动
        static OSGeo.OGR.DataSource shpDataSet;             //shp文件集合DIR
        static OSGeo.GDAL.Driver gdalDriver;                //IMG文件驱动
        static OSGeo.GDAL.Dataset dsmDataset;               //dsmDataSet;
        static OSGeo.GDAL.Dataset slopeDataSet;             //slopeDataSet

        //用户定义参数

        static int jianG = 2;                               //生成等值线的间隔
        static double minLength = 50;                       //清理等值线的最小长度
        static double maxLength = 2600;                     //清理等值线的最大长度
        static double sec = 2;                              //2倍的标准差;1倍为68.2%;2倍为95.4%;3倍为99.8%;正态分布
        static double imgNodata = -100000;                  //dsm和slope的无效值
        static int CutWeight = 300;                         //slope子图大小，影响计算效率
        static int OverlapWeight = 4;                       //slope子图重叠区
        static void Main(string[] args)
        {
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();
            shpDataDriver = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            gdalDriver = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            shpDataSet = shpDataDriver.CreateDataSource(shpSavePath, null);

            //1 等高线
            //OSGeo.OGR.Layer dzxLayer = getDZX_();
            //OSGeo.OGR.Layer clearLines = cleanDS_(dzxLayer);
            //OSGeo.OGR.Layer dzPoly = dzPoly_(clearLines);

            //2 坡度图
            buildSlope();

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
        static OSGeo.OGR.Layer getDZX_()
        {
            //读入数据源
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(dsmPath, OSGeo.GDAL.Access.GA_ReadOnly);

            //分析数据源
            double noDataValue;                 //无效值
            int hasDataValue;                   //0不使用无效值,1使用无效值
            inData.GetRasterBand(1).GetNoDataValue(out noDataValue, out hasDataValue);
            //WKT
            string wkt = inData.GetProjectionRef();
            OSGeo.OSR.SpatialReference srs = wkt == "" ? null : new OSGeo.OSR.SpatialReference(wkt);
            //图像值属性，最大值、最小值、平均值
            double min, max, mean, std;
            inData.GetRasterBand(1).GetStatistics(0, 1, out min, out max, out mean, out std);
            //计算每级线的值，加0.5是确保取到最接近的整数，不被四舍五入
            int count = Convert.ToInt32((max - min) / jianG + 0.5);
            double[] shu = new double[count];
            for (int i = 0; i < count; i++)
                shu[i] = min + jianG * i;

            //创建空的SHP，准备塞入数据
            OSGeo.OGR.Layer dzxLayer = shpDataSet.CreateLayer("dzx", srs, OSGeo.OGR.wkbGeometryType.wkbMultiLineString, null);
            //必须有这两个字段，函数要往里塞值，其实没毛用
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            dzxLayer.CreateField(fieldDf0, 1);//ID
            dzxLayer.CreateField(fieldDf1, 1);//Value

            //Band(1), 间隔, 起始高度, 分段数量, 分段值数组, 是否有无效值, 无效值, 预置图层. ID字段, 高度值字段, null , null
            OSGeo.GDAL.Gdal.ContourGenerate(inData.GetRasterBand(1), jianG, min, count, shu, hasDataValue, noDataValue, dzxLayer, 0, 1, null, null);
            inData.Dispose();
            if (dzxLayer.GetFeatureCount(0) > 0)
            {
                return dzxLayer;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 通过线长,值 清理等值线
        /// </summary>
        /// <param name="dzx"></param>
        /// <returns></returns>
        static OSGeo.OGR.Layer cleanDS_(OSGeo.OGR.Layer dzxLayer)
        {
            //new a shp
            OSGeo.OGR.Layer newdzxLayer = shpDataSet.CreateLayer("clear", dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);

            //aue平均值，bzc标准差
            double aue, bzc;
            StaticTools.getBZC(dzxLayer, out aue, out bzc);
            //清理过大和过小的高度值，取值范围为平均值两则，2倍的标准差，约为95.4%
            double minValue = aue - bzc * sec;
            double maxValue = aue + bzc * sec;
            int FeatureCount = dzxLayer.GetFeatureCount(0);
            for (int i = 0; i < FeatureCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                //判断长度
                double FeatLength = fileGeom.Length();
                bool s1 = FeatLength > minLength && FeatLength < maxLength;
                //判断值
                double featValue = fileFeat.GetFieldAsDouble("EVE");
                bool s2 = featValue > minValue && featValue < maxValue;
                //判断闭合
                bool isR = fileGeom.IsRing();
                if (s1 && s2 && isR)
                {
                    newdzxLayer.CreateFeature(fileFeat);
                }
                fileFeat.Dispose();
            }
            dzxLayer.Dispose();
#if BT_Release
            //删掉未清理的等值线 dzxLayer
            shpDataSet.DeleteLayer(0);
#endif
            if (newdzxLayer.GetFeatureCount(0) > 0)
                return newdzxLayer;
            else
                return null;
        }
        /// <summary>
        /// 等值线转为POLYGON
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static OSGeo.OGR.Layer dzPoly_(OSGeo.OGR.Layer cleanLayer)
        {
            //创建poly层
            OSGeo.OGR.Layer polyLayer = shpDataSet.CreateLayer("dzPoly", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            polyLayer.CreateField(fieldDf0, 1);//ID
            polyLayer.CreateField(fieldDf1, 1);//Value

            OSGeo.OGR.FeatureDefn featDF = new OSGeo.OGR.FeatureDefn("");
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
#if BT_Release
            //删掉清理后的等值线 cleanLayer
            shpDataSet.DeleteLayer(0);
#endif
            return polyLayer;
        }

        #endregion

        #region 坡度图

        static void buildSlope()
        {
            // 创建slopeMap,设置无效值
            CreateSlopeMap(dsmPath, slopeImgSavePath);

            //
            MySloping(dsmPath);
            //AutoResetEvent asyRE = new AutoResetEvent(false);
            //new Thread(() => { MySloping(dsmPath, asyRE); }).Start();
            //asyRE.WaitOne();
        }
        /// <summary>
        /// 创建slopeMap,设置无效值
        /// </summary>
        /// <param name="DemPath"></param>
        /// <param name="OutSlpPath"></param>
        static void CreateSlopeMap(string DemPath, string OutSlpPath)
        {
            //打开DSM,获取属性：变换参数、是否有无效值、无效值
            OSGeo.GDAL.Dataset dsmDataset = OSGeo.GDAL.Gdal.Open(DemPath, OSGeo.GDAL.Access.GA_Update);
            double[] geoTransform = new double[6];
            dsmDataset.GetGeoTransform(geoTransform);
            OSGeo.GDAL.Band dsmBand = dsmDataset.GetRasterBand(1);
            int hasVal; double demNodata;
            dsmBand.GetNoDataValue(out demNodata, out hasVal);

            //调用GDal创建slope影像
            slopeDataSet = gdalDriver.Create(OutSlpPath, dsmDataset.RasterXSize, dsmDataset.RasterYSize, 1, OSGeo.GDAL.DataType.GDT_Float32, null);
            slopeDataSet.SetProjection(dsmDataset.GetProjection());
            slopeDataSet.SetGeoTransform(geoTransform);
            OSGeo.GDAL.Band slopeBand = slopeDataSet.GetRasterBand(1);

            //设置无效值
            if (hasVal == 0)            //没有无效值
            {
                //把DSM和SLOPE全设置成用户指定的值
                dsmBand.SetNoDataValue(imgNodata);
                slopeBand.SetNoDataValue(imgNodata);
            }
            else                        //DSM有无效值时
            {
                //把SLOPE和全局变量设置成DSM的无效值
                imgNodata = demNodata;
                slopeBand.SetNoDataValue(demNodata);
            }
            dsmBand.Dispose();
            // dsmDataset.Dispose();
            // BufferOnePixel(slopeBand);
            slopeBand.Dispose();
            // slopeDataSet.Dispose();
        }
        private static async void MySloping(string DemPath, AutoResetEvent asyRE)
        {
            int iCount = 0;
            int Maxcnt = GetCutNumberOfImg();
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
        private static void MySloping(string DemPath)
        {
            int Maxcnt = GetCutNumberOfImg();
            for (int i = 0; i < Maxcnt; i++)
            {
                DemCutData data = new DemCutData(DemPath, i, 300, 300, 4);
                TKData(data);
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
        /// <summary>
        /// 分块数量
        /// </summary>
        /// <param name="imgPath">DSM路径</param>
        /// <param name="CutWeight">切块大小</param>
        /// <param name="OverlapWeight">块重叠区</param>
        /// <returns></returns>
        private static int GetCutNumberOfImg()
        {
            OSGeo.GDAL.Dataset _inDataset = OSGeo.GDAL.Gdal.Open(dsmPath, OSGeo.GDAL.Access.GA_ReadOnly);
            int xSize = _inDataset.RasterXSize;
            int ySize = _inDataset.RasterYSize;
            _inDataset.Dispose();

            int xTimes = (xSize - OverlapWeight) / CutWeight;
            int xRemainder = (xSize - OverlapWeight) % CutWeight;
            int yTimes = (ySize - OverlapWeight) / CutWeight;
            int yRemainder = (ySize - OverlapWeight) % CutWeight;

            int ax = xTimes, by = yTimes;
            if (xTimes == 0)
                xRemainder = xSize;
            if (yTimes == 0)
                yRemainder = ySize;
            if (xRemainder > OverlapWeight)
                ax++;
            if (yRemainder > OverlapWeight)
                by++;

            return ax * by;
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

            BasicUnitSlp bunitSlp = new BasicUnitSlp(x * dcData.Weight, y * dcData.High, xsize, ysize, demNodata, imgNodata, buffer, pixel_x, pixel_y);

            double[] resArr = bunitSlp.Calculate();
            lock (slopeDataSet)
            {
                slopeDataSet.GetRasterBand(1).WriteRaster(x * dcData.Weight + 1, y * dcData.High + 1, xsize - 2, ysize - 2, resArr, xsize - 2, ysize - 2, 0, 0);
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
                TKDataF(data);
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
        private static void TKDataF(object n)
        {
            int hasVal;
            double nodataV;
            double importLevel;
            double[] geoTansform = new double[6];

            CutData cutData = (CutData)n;
            importLevel = cutData.ImportLevel;
            OSGeo.GDAL.Dataset demDs = OSGeo.GDAL.Gdal.Open(cutData.DemPath, OSGeo.GDAL.Access.GA_ReadOnly);
            OSGeo.GDAL.Dataset slpDs = OSGeo.GDAL.Gdal.Open(cutData.SlopePath, OSGeo.GDAL.Access.GA_ReadOnly);
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

            BasicUnit bUnit = new BasicUnit(x * 500, y * 500, xsize, ysize, importLevel, buffer, slopebuffer, geoTansform, nodataV);

            List<OSGeo.OGR.Geometry> geolist = bUnit.Identify2();
            Console.WriteLine("一个图块儿计算完毕");
            lock (qthread)
            {
                foreach (OSGeo.OGR.Geometry item in geolist)
                {
                    qthread.WritePolygonShp(item);
                }
            }
            demDs.Dispose();
            slpDs.Dispose();
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
