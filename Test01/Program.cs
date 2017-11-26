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
        static string dsmPath = @"C:\temp\outlinetest01.img";               //  open
        static string shpSaveFile = @"C:\temp\asf\fffff.shp";             //  save
        static string shpSavePath = Path.GetDirectoryName(shpSaveFile);     //  C:\temp\asf\
        static string slopePath = shpSavePath + "\\a.img";                  //  C:\temp\asf\a.img
        static string str_dzx = "b";                                      //  C:\temp\asf\dzx.shp
        static string str_clear = "c";                                  //  C:\temp\asf\clear.shp
        static string str_dzPoly = "d";                                //  C:\temp\asf\dzPoly.shp
        static string str_slopeLine = "e";                          //  C:\temp\asf\slopeLine.shp
        static string resFile = Path.GetFileNameWithoutExtension(shpSaveFile);//    "resFile"
        static OSGeo.OGR.DataSource shpDataSet;             //shp文件集合DIR
        static OSGeo.OGR.Layer slopePolyLayer;
        static OSGeo.OGR.Layer dzxPolyLayer;
        static OSGeo.OGR.Layer resLayer;
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
        static double ImprotLevel;                          //slopeLineAnge
        static double minArea = 200;                        //过滤最小面积
        static bool IsDelete = true;
        static double[] dsm_Transform = new double[6];
        static int dsm_Xsize;
        static int dsm_Ysize;

        static void Main(string[] args)
        {
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();
            //shp驱动
            OSGeo.OGR.Driver shpDataDriver = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            //shp数据源
            shpDataSet = shpDataDriver.CreateDataSource(shpSavePath, null);
            //img驱动
            gdalDriver = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            //dsm数据源
            dsmDataset = OSGeo.GDAL.Gdal.Open(dsmPath, OSGeo.GDAL.Access.GA_Update);
            //dsm数据信息
            dsmDataset.GetGeoTransform(dsm_Transform);
            dsm_Xsize = dsmDataset.RasterXSize;
            dsm_Ysize = dsmDataset.RasterYSize;

            //1 坡度图
            buildSlope();

            //2 坡度线,这里会被释放，过程解锁
            getPDX(2);

            //3 等高线
            dzPoly_(cleanDS_(getDZX_()));

            //4 筛选
            selectFeat();

            //5 高度值
            //  GetHight.getH(dsmPath, shpSavePath);

            //6 简化
            // jianhua(175, 5);

            shpDataSet.Dispose();
            shpDataDriver.Dispose();
            gdalDriver.Dispose();
        }

        #region 等高线
        /// <summary>
        /// 获取等高线
        /// </summary>
        /// <returns></returns>
        static OSGeo.OGR.Layer getDZX_()
        {
            //分析数据源
            double noDataValue;                 //无效值
            int hasDataValue;                   //0不使用无效值,1使用无效值
            dsmDataset.GetRasterBand(1).GetNoDataValue(out noDataValue, out hasDataValue);
            //WKT
            string wkt = dsmDataset.GetProjectionRef();
            OSGeo.OSR.SpatialReference srs = wkt == "" ? null : new OSGeo.OSR.SpatialReference(wkt);
            //图像值属性，最大值、最小值、平均值
            double min, max, mean, std;
            dsmDataset.GetRasterBand(1).GetStatistics(0, 1, out min, out max, out mean, out std);
            //计算每级线的值，加0.5是确保取到最接近的整数，不被四舍五入
            int count = Convert.ToInt32((max - min) / jianG + 0.5);
            double[] shu = new double[count];
            for (int i = 0; i < count; i++)
                shu[i] = min + jianG * i;

            //创建空的SHP，准备塞入数据
            shpDataSet.deleteLayerByName(str_dzx);
            OSGeo.OGR.Layer dzxLayer = shpDataSet.CreateLayer(str_dzx, srs, OSGeo.OGR.wkbGeometryType.wkbMultiLineString, null);
            //必须有这两个字段，函数要往里塞值，其实没毛用
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            dzxLayer.CreateField(fieldDf0, 1);//ID
            dzxLayer.CreateField(fieldDf1, 1);//Value

            //Band(1), 间隔, 起始高度, 分段数量, 分段值数组, 是否有无效值, 无效值, 预置图层. ID字段, 高度值字段, null , null
            OSGeo.GDAL.Gdal.ContourGenerate(dsmDataset.GetRasterBand(1), jianG, min, count, shu, hasDataValue, noDataValue, dzxLayer, 0, 1, null, null);
            dsmDataset.Dispose();
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
            shpDataSet.deleteLayerByName(str_clear);
            OSGeo.OGR.Layer newdzxLayer = shpDataSet.CreateLayer(str_clear, dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);

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

            if (IsDelete)
                //删掉未清理的等值线 dzxLayer
                shpDataSet.deleteLayerByName(str_dzx);

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
            shpDataSet.deleteLayerByName(str_dzPoly);
            dzxPolyLayer = shpDataSet.CreateLayer(str_dzPoly, cleanLayer.GetSpatialRef(), OSGeo.OGR.wkbGeometryType.wkbPolygon, null);

            int ii = cleanLayer.GetFeatureCount(0);
            for (int i = 0; i < ii; i++)
            {
                OSGeo.OGR.Feature lineFeat = cleanLayer.GetFeature(i);
                OSGeo.OGR.Geometry lineGeom = lineFeat.GetGeometryRef();

                OSGeo.OGR.Feature polyFeat = new OSGeo.OGR.Feature(new OSGeo.OGR.FeatureDefn(""));
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
                dzxPolyLayer.CreateFeature(polyFeat);
                lineGeom.Dispose();
                polyGeom.Dispose();
                subGeom.Dispose();
                lineFeat.Dispose();
                polyFeat.Dispose();
            }
            cleanLayer.Dispose();

            if (IsDelete)
                //删掉清理后的等值线 cleanLayer
                shpDataSet.deleteLayerByName(str_clear);

            return dzxPolyLayer;
        }

        #endregion

        #region 坡度图

        static void buildSlope()
        {
            // 创建slopeMap,设置无效值
            CreateSlopeMap();

            //Sloping
            int Maxcnt = GetCutNumberOfImg();
            for (int i = 0; i < Maxcnt; i++)
                TKData_(i);
        }
        /// <summary>
        /// 创建slopeMap,设置无效值
        /// </summary>
        /// <param name="DemPath"></param>
        /// <param name="OutSlpPath"></param>
        static void CreateSlopeMap()
        {
            double dsm_NoDataValue; int dsm_HasNoDataValue;
            OSGeo.GDAL.Band dsmBand = dsmDataset.GetRasterBand(1);
            dsmBand.GetNoDataValue(out dsm_NoDataValue, out dsm_HasNoDataValue);

            //调用GDal创建slope影像
            if (File.Exists(slopePath))
                gdalDriver.Delete(slopePath);
            slopeDataSet = gdalDriver.Create(slopePath, dsm_Xsize, dsm_Ysize, 1, OSGeo.GDAL.DataType.GDT_Float32, null);
            slopeDataSet.SetProjection(dsmDataset.GetProjection());
            slopeDataSet.SetGeoTransform(dsm_Transform);
            OSGeo.GDAL.Band slopeBand = slopeDataSet.GetRasterBand(1);

            //设置无效值
            if (dsm_HasNoDataValue == 0)            //dsm没有无效值
            {
                //把DSM和SLOPE全设置成用户指定的值
                dsmBand.SetNoDataValue(imgNodata);
                slopeBand.SetNoDataValue(imgNodata);
            }
            else                        //DSM有无效值时
            {
                //把SLOPE和全局变量设置成DSM的无效值
                imgNodata = dsm_NoDataValue;
                slopeBand.SetNoDataValue(dsm_NoDataValue);
            }
            // BufferOnePixel(slopeBand);
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
            int xTimes = (dsm_Xsize - OverlapWeight) / CutWeight;
            int xRemainder = (dsm_Xsize - OverlapWeight) % CutWeight;
            int yTimes = (dsm_Ysize - OverlapWeight) / CutWeight;
            int yRemainder = (dsm_Ysize - OverlapWeight) % CutWeight;

            int ax = xTimes, by = yTimes;
            if (xTimes == 0)
                xRemainder = dsm_Xsize;
            if (yTimes == 0)
                yRemainder = dsm_Ysize;
            if (xRemainder > OverlapWeight)
                ax++;
            if (yRemainder > OverlapWeight)
                by++;

            return ax * by;
        }
        private static void TKData_(int index)
        {
            double pixel_x = Math.Abs(dsm_Transform[1]);
            double pixel_y = Math.Abs(dsm_Transform[5]);

            int xTimes = (dsm_Xsize - OverlapWeight) / CutWeight;
            int xRemainder = (dsm_Xsize - OverlapWeight) % CutWeight;
            int yTimes = (dsm_Ysize - OverlapWeight) / CutWeight;
            int yRemainder = (dsm_Ysize - OverlapWeight) % CutWeight;
            int ax = xTimes, by = yTimes;
            if (xRemainder > OverlapWeight)
                ax++;
            int x = index % ax;
            int y = index / ax;
            int xsize = CutWeight + OverlapWeight;
            int ysize = CutWeight + OverlapWeight;
            if (x * CutWeight + xsize > dsm_Xsize)
                xsize = dsm_Xsize - x * CutWeight;
            if (y * CutWeight + ysize > dsm_Ysize)
                ysize = dsm_Ysize - y * CutWeight;

            double[] buffer = new double[xsize * ysize];
            dsmDataset.GetRasterBand(1).ReadRaster(x * CutWeight, y * CutWeight, xsize, ysize, buffer, xsize, ysize, 0, 0);

            BasicUnitSlp bunitSlp = new BasicUnitSlp(x * CutWeight, y * CutWeight, xsize, ysize, imgNodata, imgNodata, buffer, pixel_x, pixel_y);

            double[] resArr = bunitSlp.Calculate();
            lock (slopeDataSet)
            {
                slopeDataSet.GetRasterBand(1).WriteRaster(x * CutWeight + 1, y * CutWeight + 1, xsize - 2, ysize - 2, resArr, xsize - 2, ysize - 2, 0, 0);
            }
        }
        #endregion

        #region 坡度线
        static void getPDX(int Lev)
        {
            shpDataSet.deleteLayerByName(str_slopeLine);
            slopePolyLayer = shpDataSet.CreateLayer(str_slopeLine, null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            int Maxcnt = GetCutNumberOfImg("");
            for (int i = 0; i < Lev; i++)
            {
                ImprotLevel = 85 - i * 5;
                for (int ii = 0; ii < Maxcnt; ii++)
                    TKDataF(ii);
            }
            slopeDataSet.Dispose();
            if (IsDelete)
                gdalDriver.Delete(slopePath);
            cleanPdx();
        }

        /// <summary>
        /// 获取图像分块儿数
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        private static int GetCutNumberOfImg(string img)
        {
            int xTimes = (dsm_Xsize - 300) / 500;
            int xRemainder = (dsm_Xsize - 300) % 500;
            int yTimes = (dsm_Ysize - 300) / 500;
            int yRemainder = (dsm_Ysize - 300) % 500;

            int ax = xTimes, by = yTimes;
            if (xTimes == 0)
                xRemainder = dsm_Xsize;
            if (yTimes == 0)
                yRemainder = dsm_Ysize;

            if (xRemainder > 10)
                ax++;
            if (yRemainder > 10)
                by++;

            return ax * by;
        }
        private static void TKDataF(int index)
        {
            double[] geoTansform = new double[6];
            dsmDataset.GetGeoTransform(geoTansform);

            int xTimes = (dsm_Xsize - 300) / 500;
            int xRemainder = (dsm_Xsize - 300) % 500;
            int yTimes = (dsm_Ysize - 300) / 500;
            int yRemainder = (dsm_Ysize - 300) % 500;
            int ax = xTimes, by = yTimes;
            if (xRemainder > 10)
                ax++;
            int x = index % ax;
            int y = index / ax;
            int xsize = 800, ysize = 800;
            if (x * 500 + 800 > dsm_Xsize)
                xsize = dsm_Xsize - x * 500;
            if (y * 500 + 800 > dsm_Ysize)
                ysize = dsm_Ysize - y * 500;

            double[] buffer = new double[xsize * ysize];
            double[] slopebuffer = new double[xsize * ysize];
            dsmDataset.GetRasterBand(1).ReadRaster(x * 500, y * 500, xsize, ysize, buffer, xsize, ysize, 0, 0);
            slopeDataSet.GetRasterBand(1).ReadRaster(x * 500, y * 500, xsize, ysize, slopebuffer, xsize, ysize, 0, 0);

            BasicUnit bUnit = new BasicUnit(x * 500, y * 500, xsize, ysize, ImprotLevel, buffer, slopebuffer, geoTansform, imgNodata);

            List<OSGeo.OGR.Geometry> geolist = bUnit.Identify2();

            Console.WriteLine("一个图块儿计算完毕");
            lock (slopePolyLayer)
            {
                foreach (OSGeo.OGR.Geometry item in geolist)
                {
                    if (item.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon25D ||
                        item.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon)
                    {
                        OSGeo.OGR.Feature oFea = new OSGeo.OGR.Feature(slopePolyLayer.GetLayerDefn());
                        oFea.SetGeometry(item);
                        slopePolyLayer.CreateFeature(oFea);
                    }
                }
            }
        }
        /// <summary>
        /// 清理重复的Featuer----------单线程
        /// </summary>
        public static void cleanPdx()
        {
            int ffff = 0;
            int featCount = slopePolyLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount - 1; i++)
            {
                OSGeo.OGR.Feature ori = slopePolyLayer.GetFeature(i);
                for (int j = i + 1; j < featCount; j++)
                {
                    OSGeo.OGR.Feature next = slopePolyLayer.GetFeature(j);
                    bool a = StaticTools.isSame(ori, next, 1);
                    if (ori.GetGeometryRef().GetArea() < minArea || a)
                    {
                        slopePolyLayer.DeleteFeature(i);
                        ffff++;
                        Console.WriteLine("已删除{0}个重复Featuer,allFeat is {1}/{2}", ffff, i + 1, featCount);
                        break;
                    }
                    next.Dispose();
                }
                ori.Dispose();
            }
            string layerName = slopePolyLayer.GetName();
            shpDataSet.ExecuteSQL("REPACK " + layerName, null, "");
        }
        #endregion

        #region 筛选
        static void selectFeat()
        {
            shpDataSet.deleteLayerByName(resFile);
            resLayer = shpDataSet.CreateLayer(resFile, null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);

            int pdCount = slopePolyLayer.GetFeatureCount(0);
            int dzCount = dzxPolyLayer.GetFeatureCount(0);
            // 通过是否相交和面积差,获得最小面积差相交要素的ID数组
            for (int pdi = 0; pdi < pdCount; pdi++)
            {
                OSGeo.OGR.Feature pdFeat = slopePolyLayer.GetFeature(pdi);
                OSGeo.OGR.Geometry pdGeom = pdFeat.GetGeometryRef();

                double afterCha = -1;
                int yesID = -1;

                for (int dzi = 0; dzi < dzCount; dzi++)
                {
                    OSGeo.OGR.Feature dzFeat = dzxPolyLayer.GetFeature(dzi);
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
                pdGeom.Dispose();
                pdFeat.Dispose();

                if (yesID != -1)
                    resLayer.CreateFeature(dzxPolyLayer.GetFeature(yesID));
            }
            if (IsDelete)
            {
                shpDataSet.deleteLayerByName(str_slopeLine);
                shpDataSet.deleteLayerByName(str_dzPoly);
            }
            cleanPdxM();
        }
        /// <summary>
        /// 清理重复的Featuer----------多线程----与单线程效率差别很小，不推荐
        /// </summary>
        /// <param name="filePath"></param>
        private static void cleanPdxM()
        {
            int featCount = resLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount - 1; i++)
            {
                OSGeo.OGR.Feature oriFeat = resLayer.GetFeature(i);
                for (int iv = i + 1; iv < featCount; iv++)
                {
                    OSGeo.OGR.Feature nextFeat = resLayer.GetFeature(iv);
                    if (StaticTools.isSame(oriFeat, nextFeat, 1))
                    {
                        resLayer.DeleteFeature(i);
                        break;
                    }
                    nextFeat.Dispose();
                }
            }
            //string a = "REPACK " + layer.GetName();
            //ds.ExecuteSQL(a, null, "");
            shpDataSet.deleteFeatUpdate();
        }
        #endregion

        #region 简化
        /// <summary>
        /// 清理原文件
        /// </summary>
        /// <param name="jiaodu"></param>
        /// <param name="cishu"></param>
        private static void jianhua(double jiaodu, int cishu)
        {
            for (int i = cishu; i > 0; i--)
                claenPoint(shpSavePath, jiaodu, i);
        }
        private static void claenPoint(string filePath, double jiaodu, int cishu)
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
