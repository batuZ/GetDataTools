using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._3_筛选轮廓
{
    class 筛选轮廓线
    {
        /************************************  对比并筛选 ***************************************************/

        /// <summary>
        /// 通过ID数组提取出所有要素,创建动作
        /// </summary>
        /// <param name="dzLine"></param>
        /// <param name="pdLing"></param>
        /// <param name="savePath"></param>
        public static void selectFeat(string dzx, string pdx, string savePath)
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
            _2_坡度线.OutLine.CutImg.cleanPdxM(savePath);
            ssww.Stop();
            Console.WriteLine("提取轮廓线完成！用时：{0}",ssww.Elapsed.ToString());
        }

        /// <summary>
        /// 通过是否相交和面积差,获得最小面积差相交要素的ID数组--------单线OK
        /// </summary>
        /// <returns></returns>
        //private static int[] getMinIdGroup(string dzx, string pdx)
        //{
        //    List<int> minIdG = new List<int>();
        //    OSGeo.OGR.Ogr.RegisterAll();
        //    OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
        //    OSGeo.OGR.DataSource dzDS = dr.Open(dzx, 0);
        //    OSGeo.OGR.DataSource pdDS = dr.Open(pdx, 0);
        //    OSGeo.OGR.Layer dzLayer = dzDS.GetLayerByIndex(0);
        //    OSGeo.OGR.Layer pdLayer = pdDS.GetLayerByIndex(0);

        //    int dzCount = dzLayer.GetFeatureCount(0);
        //    int pdCount = pdLayer.GetFeatureCount(0);
        //    Console.WriteLine("起用单线程提取轮廓线ID....");

        //    for (int pdi = 0; pdi < pdCount; pdi++)
        //    {
        //        OSGeo.OGR.Feature pdFeat = pdLayer.GetFeature(pdi);
        //        OSGeo.OGR.Geometry pdGeom = pdFeat.GetGeometryRef();

        //        //前个面积差,循环结束将此值为最小面积差
        //        double afterCha = -1;

        //        //前个ID,循环结束将此值添加到minID中
        //        int yesID = -1;

        //        //查找与当前坡度要素有交集的等值线
        //        for (int dzi = 0; dzi < dzCount; dzi++)
        //        {
        //            //get 等值线要素
        //            OSGeo.OGR.Feature dzFeat = dzLayer.GetFeature(dzi);
        //            OSGeo.OGR.Geometry dzGeom = dzFeat.GetGeometryRef();

        //            if (dzGeom != null)
        //            {
        //                //判断是否相交
        //                if (pdGeom.Intersect(dzGeom))
        //                {
        //                    //求当前等值线要素与坡度线要素的面积差
        //                    double cha = Math.Abs(dzGeom.GetArea() - pdGeom.GetArea());

        //                    //如果前个面积差未被赋值,则把当前差赋值给前差,并记录ID
        //                    if (afterCha == -1)
        //                    {
        //                        afterCha = Math.Abs(dzGeom.GetArea() - pdGeom.GetArea());
        //                        yesID = dzi;
        //                    }
        //                    //如果前差已赋值,且当前差小于前差,则把当前差赋值给前差,并记录ID
        //                    else if (cha < afterCha)
        //                    {
        //                        afterCha = cha;
        //                        yesID = dzi;
        //                    }
        //                }
        //            }
        //            dzGeom.Dispose();
        //            dzFeat.Dispose();
        //        }
        //        //如果yesID被赋值,则把这个结果添加到ID数组中
        //        if (yesID != -1)
        //        {
        //            minIdG.Add(yesID);
        //        }
        //        pdGeom.Dispose();
        //        pdFeat.Dispose();
        //        Console.WriteLine("第{0}个完成，共{1}个", pdi, pdCount);
        //    }
        //    pdLayer.Dispose();
        //    pdDS.Dispose();
        //    dzLayer.Dispose();
        //    dzDS.Dispose();
        //    Console.WriteLine("单线程提取ID完成.....");
        //    return minIdG.ToArray();
        //}


        static List<int> minIdG = new List<int>();
        static int tickTime;
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
        /// <summary>
        /// 子线程提取轮廓线ID
        /// </summary>
        /// <param name="pdFeat"></param>
        /// <param name="dzx"></param>
        async static void select(Feature pdFeat, string dzx)
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

    }
}