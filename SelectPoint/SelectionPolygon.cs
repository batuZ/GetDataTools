using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools.SelectPoint
{
    class SelectionPolygon
    {
        ///通过坡度线筛选等值线
        /// 1 清理等值线,长度\ 值域
        /// 2 等值线转面
        /// 3 根据坡度线求交等值线,并通过面积筛选
        /// 输入文件路径,输出临时shp
        /// 

        /**************************************   清理等值线   *****************************************
        /// <summary>
        /// 通过线长,值 清理等值线
        /// </summary>
        /// <param name="filePath"></param>
        public static void cleanPolyline(string filePath)
        {
            //获取数据
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(filePath, 1);
            OSGeo.OGR.Layer fileLayer = ds.GetLayerByIndex(0);

            //获取Featuer数
            int featCount = fileLayer.GetFeatureCount(0);

            //求标准差
            // 1 拿到每个Featuer的Value
            double[] values = new double[featCount];
            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = fileLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                values[i] = fileFeat.GetFieldAsDouble("EVE");
            }

            // 2 求Values的平均值
            double aue = UsefullTools.myAue(values);

            // 3 求values与平均值差的平方和
            double pingFangHe = 0;
            for (int i = 0; i < featCount; i++)
            {
                pingFangHe += (values[i] - aue) * (values[i] - aue);
            }

            // 4 每个值与平均值的差相加,除Featuer数.再开方,得到标准差
            double bzc = Math.Sqrt(pingFangHe / featCount);

            double minLength = 80;
            double maxLength = 600;
            double minValue = aue - bzc;
            double maxValue = aue + bzc;

            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = fileLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                double featValue = fileFeat.GetFieldAsDouble("EVE");
                double FeatLength = fileGeom.Length();
                bool s1 = FeatLength < minLength || FeatLength > maxLength;
                bool s2 = featValue < minValue || featValue > maxValue;
                if (s1 || s2 || !fileGeom.IsRing())
                {
                    fileLayer.DeleteFeature(i);
                }
            }
            string layerName = fileLayer.GetName();
            ds.ExecuteSQL("REPACK " + layerName, null, "");
            ds.Dispose();
        }
        /// <summary>
        /// 用来清理坡度线
        /// </summary>
        /// <param name="filePath"></param>
        public static void cleanPDPoly(string filePath)
        {
            //获取数据
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(filePath, 1);
            OSGeo.OGR.Layer fileLayer = ds.GetLayerByIndex(0);

            //获取Featuer数
            int featCount = fileLayer.GetFeatureCount(0);

            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = fileLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                OSGeo.OGR.Geometry subGeom = fileGeom.GetGeometryRef(0);
                double FeatLength = subGeom.Length();
                bool s1 = FeatLength < 80 || FeatLength > 800;
              
                if (s1)
                {
                    fileLayer.DeleteFeature(i);
                }
            }
            string layerName = fileLayer.GetName();
            ds.ExecuteSQL("REPACK " + layerName, null, "");
            ds.Dispose();
        }
       
        /********************************   等值线转换为多边形   ***********************************************

   
        /// <summary>
        /// 等值线转为POLYGON
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string lineToPoly(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource lineDS = dr.Open(filePath,0);
            OSGeo.OGR.Layer lineLayer = lineDS.GetLayerByIndex(0);
            string savePath = filePath.Substring(0, filePath.LastIndexOf(".")) + "_ToPolygong.shp";
            //存在即删除
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            //创建 一个新的数据源 for Polygon  
            OSGeo.OGR.DataSource polyDS = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer polyLayer = polyDS.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            polyLayer.CreateField(fieldDf0, 1);//ID
            polyLayer.CreateField(fieldDf1, 1);//Value
            for (int i = 0; i < lineLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature lineFeat = lineLayer.GetFeature(i);
                OSGeo.OGR.Geometry lineGeom = lineFeat.GetGeometryRef();
                OSGeo.OGR.FeatureDefn featDF = new OSGeo.OGR.FeatureDefn("");
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
                subGeom.CloseRings();
                polyGeom.AddGeometry(subGeom);
                polyFeat.SetGeometry(polyGeom);
                polyLayer.CreateFeature(polyFeat);
            }
            polyLayer.Dispose();
            polyDS.Dispose();
            lineDS.Dispose();
            return savePath;
        }

        /************************************  获取坡度线  ***************************************************

        private void getPDlines()
        {
            string _inFilepath = @"D:\code\testDATA\test2.img";
            string _outFilePath = @"D:\code\outputfile\test2.img";
            string _outShpPath = @"D:\code\outputfile\test2";
            SlopeDem.SlopeDemObj.Slope(_inFilepath, _outFilePath);
            SlopeDem.SlopeDemObj.CreatePolygon(_inFilepath, _outShpPath);

        }

        /************************************  对比并筛选 ***************************************************/


        /// <summary>
        /// 通过是否相交和面积差,获得最小面积差相交要素的ID数组
        /// </summary>
        /// <returns></returns>
        private static int[] getMinIdGroup(string dzLine, string pdLing)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzDS = dr.Open(dzLine, 0);
            OSGeo.OGR.DataSource pdDS = dr.Open(pdLing, 0);
            OSGeo.OGR.Layer dzLayer = dzDS.GetLayerByIndex(0);
            OSGeo.OGR.Layer pdLayer = pdDS.GetLayerByIndex(0);
            int dzCount = dzLayer.GetFeatureCount(0);
            int pdCount = pdLayer.GetFeatureCount(0);

            //面积差值最小要素的ID数组
            List<int> minIdG = new List<int>();

            //坡度Layer中的每个要素,成为当前坡度参考线
            for (int pdi = 0; pdi < pdCount; pdi++)
            {
                //Get 坡度要素
                OSGeo.OGR.Feature pdFeat = pdLayer.GetFeature(pdi);
                OSGeo.OGR.Geometry pdGeom = pdFeat.GetGeometryRef();

                //前个面积差,循环结束将此值为最小面积差
                double afterCha = -1;

                //前个ID,循环结束将此值添加到minID中
                int yesID = -1;

                //查找与当前坡度要素有交集的等值线
                for (int dzi = 0; dzi < dzCount; dzi++)
                {
                    //get 等值线要素
                    OSGeo.OGR.Feature dzFeat = dzLayer.GetFeature(dzi);
                    OSGeo.OGR.Geometry dzGeom = dzFeat.GetGeometryRef();
                    if (dzGeom != null)
                    {
                        //判断是否相交
                        if (pdGeom.Intersect(dzGeom))
                        {
                            //求当前等值线要素与坡度线要素的面积差
                            double cha = Math.Abs(dzGeom.GetArea() - pdGeom.GetArea());

                            //如果前个面积差未被赋值,则把当前差赋值给前差,并记录ID
                            if (afterCha == -1)
                            {
                                afterCha = Math.Abs(dzGeom.GetArea() - pdGeom.GetArea());
                                yesID = dzi;
                            }
                            //如果前差已赋值,且当前差小于前差,则把当前差赋值给前差,并记录ID
                            else if (cha < afterCha)
                            {
                                afterCha = cha;
                                yesID = dzi;
                            }
                        }
                    }
                }
                //如果yesID被赋值,则把这个结果添加到ID数组中
                if (yesID != -1)
                {
                    minIdG.Add(yesID);
                }
            }
            dzDS.Dispose();
            pdDS.Dispose();
            return minIdG.ToArray();
        }


        /// <summary>
        /// 通过ID数组提取出所有要素
        /// </summary>
        /// <param name="dzLine"></param>
        /// <param name="pdLing"></param>
        /// <param name="savePath"></param>
        public static void selectFeat(string dzLine, string pdLing, string savePath)
        {
            //获得数组
            int[] a = getMinIdGroup(dzLine, pdLing);

            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            //读取原Layer
            OSGeo.OGR.DataSource dzDS = dr.Open(dzLine, 0);
            OSGeo.OGR.Layer dzLayer = dzDS.GetLayerByIndex(0);
            //新建Layer
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            //get featuer by ID
            for (int i = 0; i < a.Length; i++)
            {
                OSGeo.OGR.Feature dzFeat = dzLayer.GetFeature(a[i]);
                OSGeo.OGR.Geometry dzGeom = dzFeat.GetGeometryRef();
                OSGeo.OGR.Geometry fileGeom = dzGeom;
                OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(dzFeat.GetDefnRef());
                fileFeat.SetGeometry(fileGeom);
                fileLayer.CreateFeature(dzFeat);
            }
            fileLayer.Dispose();
            ds.Dispose();
            dzDS.Dispose();
        }

    }
}
