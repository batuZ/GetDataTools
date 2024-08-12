using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo;
using OSGeo.GDAL;
using OSGeo.OSR;
using OSGeo.OGR;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;


namespace GetDataTools._3_筛选轮廓
{
    class GetHight
    {
        public static void getH(string DSMfile, string oriShp)
        {
            Console.WriteLine("开始提取建筑高度！");
            Gdal.AllRegister();
            Ogr.RegisterAll();
            Stopwatch aTime = new Stopwatch(); aTime.Start();
            //创建一个BUFFER，BUFFER距离为1米
            string bufShp = bufferFile(oriShp, 1);

            //打开原文件和Buffer文件
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource oriDs = dr.Open(oriShp, 1);
            DataSource bufDs = dr.Open(bufShp, 0);
            OSGeo.OGR.Layer bufLayer = bufDs.GetLayerByIndex(0);
            OSGeo.OGR.Layer oriLayer = oriDs.GetLayerByIndex(0);

            //判断原文件中是否有以下字段，没有就创建
            if (oriLayer.FindFieldIndex("MIN", 1) == -1)
            {
                FieldDefn min = new FieldDefn("MIN", FieldType.OFTReal);
                oriLayer.CreateField(min, 1);
            }

            if (oriLayer.FindFieldIndex("MAX", 1) == -1)
            {
                FieldDefn max = new FieldDefn("MAX", FieldType.OFTReal);
                oriLayer.CreateField(max, 1);
            }

            if (oriLayer.FindFieldIndex("HIGHT", 1) == -1)
            {
                FieldDefn hight = new FieldDefn("HIGHT", FieldType.OFTReal);
                oriLayer.CreateField(hight, 1);
            }

            //打开栅格
            Dataset dsmDs = Gdal.Open(DSMfile, Access.GA_ReadOnly);

            double[] transfrom = new double[6];
            dsmDs.GetGeoTransform(transfrom);
            int allX = dsmDs.RasterXSize;
            int allY = dsmDs.RasterYSize;
            //开始计算每个Feature需要读取的栅格参数
            long featCount = oriLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                int[] subRasterOff_Size = subRasterInfo(transfrom, allX, allY, bufLayer.GetFeature(i));

                Feature oriFeat = oriLayer.GetFeature(i);
                Feature bufFeat = bufLayer.GetFeature(i);
                getMaxMinValue(dsmDs, oriFeat, bufFeat, subRasterOff_Size);
                oriLayer.SetFeature(oriFeat);
                oriFeat.Dispose();
                Console.WriteLine("当前处理：{0}/{1}", i, featCount);
            }
            oriLayer.Dispose();
            bufLayer.Dispose();
            oriDs.Dispose();
            bufDs.Dispose();
            dsmDs.Dispose();
            aTime.Stop();
            Console.WriteLine("建筑高度值提取完成！用时：{0}", aTime.Elapsed.ToString());
        }

        public static void getHM(string DSMfile, string oriShp)
        {
            Console.WriteLine("开始提取建筑高度！");
            Stopwatch aTime = new Stopwatch(); aTime.Start();
            Ogr.RegisterAll();
            Gdal.AllRegister();
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource oriDs = dr.Open(oriShp, 0);
            Layer oriLayer = oriDs.GetLayerByIndex(0);
            //判断原文件中是否有以下字段，没有就创建
            if (oriLayer.FindFieldIndex("MIN", 1) == -1)
            {
                FieldDefn min = new FieldDefn("MIN", FieldType.OFTReal);
                oriLayer.CreateField(min, 1);
            }

            if (oriLayer.FindFieldIndex("MAX", 1) == -1)
            {
                FieldDefn max = new FieldDefn("MAX", FieldType.OFTReal);
                oriLayer.CreateField(max, 1);
            }

            if (oriLayer.FindFieldIndex("HIGHT", 1) == -1)
            {
                FieldDefn hight = new FieldDefn("HIGHT", FieldType.OFTReal);
                oriLayer.CreateField(hight, 1);
            }
            long featCount = oriLayer.GetFeatureCount(0);
            tickTime = 0;
            oriDs.Dispose();
            for (int i = 0; i < featCount; i++)
            {
                multi(DSMfile, oriShp, i);
            }
            while (tickTime < featCount)
            {
                Console.WriteLine("提取建筑高度，当前完成：{0}/{1}.", tickTime, featCount);
                Thread.Sleep(2000);
            }
            aTime.Stop();
            Console.WriteLine("建筑高度值提取完成！用时：{0}", aTime.Elapsed.ToString());
            MessageBox.Show("提取高度完成！");
        }
        static int tickTime;
        async static void multi(string dsm, string oriShp, int i)
        {
            await Task.Run(() =>
            {
                Ogr.RegisterAll();
                Gdal.AllRegister();

                Dataset rastDs = Gdal.Open(dsm, Access.GA_ReadOnly);
                var geoTransform = new double[6];
                rastDs.GetGeoTransform(geoTransform);

                OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
                DataSource featDs = dr.Open(oriShp, 1);
                Layer oriLayer = featDs.GetLayerByIndex(0);
                Feature oriFeat = oriLayer.GetFeature(i);

                Feature bufFeat = new Feature(new FeatureDefn(""));
                bufFeat.SetGeometry(oriFeat.GetGeometryRef().Buffer(1, 0));

                int[] subRasterOff_Size = subRasterInfo(geoTransform, rastDs.RasterXSize, rastDs.RasterYSize, bufFeat);

                getMaxMinValue(rastDs, oriFeat, bufFeat, subRasterOff_Size);

                oriLayer.SetFeature(oriFeat);

                bufFeat.Dispose();
                rastDs.Dispose();
                featDs.Dispose();
                tickTime++;
            });
        }
        /// <summary>
        /// 获取Feature的最大值和最小值
        /// </summary>
        /// <param name="dsmDs"></param>
        /// <param name="oriFeat"></param>
        /// <param name="bufFeat"></param>
        /// <param name="subRasterOff_Size"></param>
        private static void getMaxMinValue(Dataset dsmDs, Feature oriFeat, Feature bufFeat, int[] subRasterOff_Size)
        {
            //拿到当前Feature对应的栅格值（数组）
            double[] rastValue = new double[subRasterOff_Size[2] * subRasterOff_Size[3]];
            //拿到全局Transfrom
            double[] Transfrom = new double[6];
            dsmDs.GetGeoTransform(Transfrom);

            //把SUBimg的值读进数组
            dsmDs.GetRasterBand(1).ReadRaster(
                subRasterOff_Size[0],
                subRasterOff_Size[1],
                subRasterOff_Size[2],
                subRasterOff_Size[3],
                rastValue,
                subRasterOff_Size[2],
                subRasterOff_Size[3],
                0, 0);

            while (oriFeat.GetFieldAsString("MAX") == "")
            {
                //拿到数组中最大值的索引
                int maxId = Array.IndexOf(rastValue, rastValue.Max());

                //通过索引拿到空间坐标
                double maxX, maxY;
                indexToGeoSpace(maxId, subRasterOff_Size, Transfrom, out maxX, out maxY);

                //把空间坐标写进Geomtry
                Geometry maxGeom = new Geometry(wkbGeometryType.wkbPoint);
                maxGeom.AddPoint_2D(maxX, maxY);

                //判断坐标是否在Feature中，在则把值写入对应字段，不在则把当前值改为平均值
                Geometry oriGeom = oriFeat.GetGeometryRef();
                if (maxGeom.Within(oriGeom))
                {
                    oriFeat.SetField("MAX", rastValue.Max());
                }
                else
                {
                    rastValue[maxId] = rastValue.Average();
                }
            }

            while (oriFeat.GetFieldAsString("MIN") == "")
            {
                //拿到数组中最小值的索引
                int minId = Array.IndexOf(rastValue, rastValue.Min());

                //通过索引拿到空间坐标
                double minX, minY;
                indexToGeoSpace(minId, subRasterOff_Size, Transfrom, out minX, out minY);

                //把空间坐标写进Geomtry
                Geometry minGeom = new Geometry(wkbGeometryType.wkbPoint);
                minGeom.AddPoint_2D(minX, minY);

                //判断坐标是否在Feature中，在则把值写入对应字段，不在则把当前值改为平均值
                Geometry bufGeom = bufFeat.GetGeometryRef();
                if (minGeom.Within(bufGeom))
                {
                    oriFeat.SetField("MIN", rastValue.Min());
                }
                else
                {
                    rastValue[minId] = rastValue.Average();
                }
            }
            oriFeat.SetField("HIGHT", oriFeat.GetFieldAsDouble("MAX") - oriFeat.GetFieldAsDouble("MIN"));
        }

        /// <summary>
        /// 获取与一个Feature有关的Raster参数,[0]offX,[1]offY,[2]sizeX,[3]sizeY
        /// </summary>
        /// <param name="Trans"></param>
        /// <param name="aFeat"></param>
        /// <returns></returns>
        private static int[] subRasterInfo(double[] Trans, int xSize, int ySize, Feature aFeat)
        {
            //拿到Buffer Featuer的壳
            OSGeo.OGR.Ogr.RegisterAll();
            Geometry bufGeom = aFeat.GetGeometryRef();
            Envelope bufEnve = new Envelope();
            bufGeom.GetEnvelope(bufEnve);
            //判断壳是否超出全局范围，是则赋边界值
            double maxX, minY;
            imageToGeoSpace(Trans, xSize, ySize, out maxX, out minY);
            if (bufEnve.MinX < Trans[0])
            {
                bufEnve.MinX = Trans[0];
            }
            if (bufEnve.MaxY > Trans[3])
            {
                bufEnve.MaxY = Trans[3];
            }
            if (bufEnve.MaxX > maxX)
            {
                bufEnve.MaxX = maxX;
            }
            if (bufEnve.MinY < minY)
            {
                bufEnve.MinY = minY;
            }

            //通过壳坐标拿到SubRaster的起点及行列数
            var a = new int[4];
            int leftUpX, leftUpY, rightDownX, rightDownY;
            geoToImageSpace(Trans, bufEnve.MinX, bufEnve.MaxY, out leftUpX, out leftUpY);
            geoToImageSpace(Trans, bufEnve.MaxX, bufEnve.MinY, out rightDownX, out rightDownY);

            a[0] = leftUpX;
            a[1] = leftUpY;
            a[2] = Math.Abs(rightDownX - leftUpX);
            a[3] = Math.Abs(leftUpY - rightDownY);
            bufGeom.Dispose();
            bufEnve.Dispose();
            //aFeat.Dispose();
            return a;
        }


        /// <summary>
        /// 创建一个Buffer file,用于获取最小高度,juli是buffer的距离
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="juli"></param>
        /// <returns></returns>
        private static string bufferFile(string infile, int juli)
        {
            Ogr.RegisterAll();
            string bufferFile = StaticTools.tempFilePath("shp", "Buf");
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");

            DataSource infileDs = dr.Open(infile, 0);
            Layer inLayer = infileDs.GetLayerByIndex(0);

            DataSource bufferDs = dr.CreateDataSource(bufferFile, null);
            Layer bufferLayer = bufferDs.CreateLayer(inLayer.GetName(), inLayer.GetSpatialRef(), inLayer.GetGeomType(), null);

            long featCount = inLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                Feature inFeat = inLayer.GetFeature(i);
                Geometry inGeom = inFeat.GetGeometryRef();
                Geometry outGeom = inGeom.Buffer(juli, 0);
                Feature outFeat = new Feature(new FeatureDefn(""));
                outFeat.SetGeometry(outGeom);
                bufferLayer.CreateFeature(outFeat);
                inFeat.Dispose();
                outFeat.Dispose();
            }
            infileDs.Dispose();
            bufferDs.Dispose();
            return bufferFile;
        }


        /// <summary>
        ///  Array Index to geoSpace,index(索引)，xSize(图像X轴栅格数量),不通用！
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subRasterOff_Size">
        /// [0] offx
        /// [1] offy
        /// [2] xSize
        /// [3] ySize
        /// </param>
        /// <param name="Transfrom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void indexToGeoSpace(int index, int[] subRasterOff_Size, double[] Transfrom, out double x, out double y)
        {
            //通过索引获得当前值在的sub图像中的坐标
            int subPixel = (index + 1) % subRasterOff_Size[2];
            int subLine = index / subRasterOff_Size[2];
            //sub索引加off图像坐标，获得 当前值所在的全局图像坐标
            int Pixel = subPixel + subRasterOff_Size[0];
            int Line = subLine + subRasterOff_Size[1];
            // 从像素空间转换到地理空间
            imageToGeoSpace(Transfrom, Pixel, Line, out x, out y);
        }

        /// <summary>
        /// 从像素空间转换到地理空间
        /// </summary>
        /// <param name="adfGeoTransform">影像坐标变换参数</param>
        /// <param name="pixel">像素所在行</param>
        /// <param name="line">像素所在列</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        private static void imageToGeoSpace(double[] Tran, int pixel, int line, out double X, out double Y)
        {
            X = Tran[0] + pixel * Tran[1] + line * Tran[2];
            Y = Tran[3] + pixel * Tran[4] + line * Tran[5];
        }


        /// <summary>
        /// 从地理空间转换到像素空间
        /// </summary>
        /// <param name="adfGeoTransform">影像坐标变化参数</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="pixel">像素所在行</param>
        /// <param name="line">像素所在列</param>
        private static void geoToImageSpace(double[] Tran, double x, double y, out int pixel, out int line)
        {
            line = (int)((y * Tran[1] - x * Tran[4] + Tran[0] * Tran[4] - Tran[3] * Tran[1]) / (Tran[5] * Tran[1] - Tran[2] * Tran[4]));
            pixel = (int)((x - Tran[0] - line * Tran[2]) / Tran[1]);
        }

    }
}
