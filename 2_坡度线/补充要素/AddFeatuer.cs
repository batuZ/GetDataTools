using GetDataTools._2_坡度线.OutLine;
using GetDataTools._2_坡度线.坡度提取;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度线.补充要素
{
    class AddFeatuer
    {
        public static void selectFromPoint(string point, string slopPoly)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource poiDS = dr.Open(point, 0);
            OSGeo.OGR.DataSource slopDS = dr.Open(slopPoly, 1);
            OSGeo.OGR.Layer poiLayer = poiDS.GetLayerByIndex(0);
            OSGeo.OGR.Layer slopLayer = slopDS.GetLayerByIndex(0);

            List<int> staIds = new List<int>();
            int poiCount = WorkFlow.pointIds.Count;
            long slopCount = slopLayer.GetFeatureCount(0);

            for (int i = 0; i < poiCount; i++)
            {
                OSGeo.OGR.Feature poiFeat = poiLayer.GetFeature(WorkFlow.pointIds[i]);
                OSGeo.OGR.Geometry poiGeom = poiFeat.GetGeometryRef();
                for (int t = 0; t < slopCount; t++)
                {
                    OSGeo.OGR.Feature slopFeat = slopLayer.GetFeature(t);
                    OSGeo.OGR.Geometry slopGeom = slopFeat.GetGeometryRef();
                    if (poiGeom.Within(slopGeom))
                    {
                        staIds.Add(t);
                        WorkFlow.pointIds[i] = -1;
                    }
                    slopFeat.Dispose();
                }
                poiFeat.Dispose();
            }
            Console.WriteLine("【本次提取到{0}个要素】", staIds.Count);
            while (WorkFlow.pointIds.IndexOf(-1) > -1)
            { WorkFlow.pointIds.Remove(-1); }

            for (int i = 0; i < slopCount; i++)
            {
                if (staIds.IndexOf(i) == -1)
                {
                    slopLayer.DeleteFeature(i);
                }
            }
            slopDS.deleteFeatUpdate();
            slopDS.Dispose();
            poiDS.Dispose();
        }

        public static void selectDZXFromPoint(string point, string dzx,string savePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource poiDS = dr.Open(point, 0);
            OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
            if (File.Exists(savePath))
                File.Delete(savePath);
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer poiLayer = poiDS.GetLayerByIndex(0);
            OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);
            OSGeo.OGR.Layer newLayer = newDS.CreateLayer("", dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);

            long poiCount = poiLayer.GetFeatureCount(0);
            long dzxCount = dzxLayer.GetFeatureCount(0);

            for (int i = 0; i < poiCount; i++)
            {
                OSGeo.OGR.Feature poiFeat = poiLayer.GetFeature(i);
                OSGeo.OGR.Geometry poiGeom = poiFeat.GetGeometryRef();
                for (int t = 0; t < dzxCount; t++)
                {
                    OSGeo.OGR.Feature dzxFeat = dzxLayer.GetFeature(t);
                    OSGeo.OGR.Geometry dzxGeom = dzxFeat.GetGeometryRef();
               
                    if (poiGeom.Within(dzxGeom))
                    {
                        newLayer.CreateFeature(dzxFeat);
                    }
                    dzxFeat.Dispose();
                    Console.WriteLine("getFeatureByPoint:{0}/{1}", i, poiCount);
                }
                poiFeat.Dispose();
            }
            Console.WriteLine("【本次提取到{0}个要素】", newLayer.GetFeatureCount(0));
            newDS.Dispose();
            dzxDS.Dispose();
            poiDS.Dispose();
        }
        static int tickTime;
        public static void selectDZXFromPointM(string point, string dzx, string savePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");

            OSGeo.OGR.DataSource poiDS = dr.Open(point, 0);
            OSGeo.OGR.Layer poiLayer = poiDS.GetLayerByIndex(0);

            if (File.Exists(savePath))
                File.Delete(savePath);
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer newLayer = newDS.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
    
            tickTime = 0;
            long poiCount = poiLayer.GetFeatureCount(0);
            
            for (int i = 0; i < poiCount; i++)
            {
                 subAdd(poiLayer.GetFeature(i), dzx, newLayer);
            }
            while (tickTime < poiCount)
            {
                Thread.Sleep(2222);
                Console.WriteLine("getFeatureByPoint:{0}/{1}", tickTime, poiCount);
            }
           
            Console.WriteLine("【本次提取到{0}个要素】", newLayer.GetFeatureCount(0));
            newDS.Dispose();
            poiDS.Dispose();
        }
        async static void subAdd(Feature poiFeat, string dzx, Layer newLayer)
        {
            await Task.Run(() =>
            {
                OSGeo.OGR.Ogr.RegisterAll();
                OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
                OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
                OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);
                long dzxCount = dzxLayer.GetFeatureCount(0);
                OSGeo.OGR.Geometry poiGeom = poiFeat.GetGeometryRef();
                for (int i = 0; i < dzxLayer.GetFeatureCount(0); i++)
                {
                    OSGeo.OGR.Feature dzxFeat = dzxLayer.GetFeature(i);
                    OSGeo.OGR.Geometry dzxGeom = dzxFeat.GetGeometryRef();

                    if (poiGeom.Within(dzxGeom))
                    {
                        newLayer.CreateFeature(dzxFeat);
                    }
                    dzxGeom.Dispose();
                    dzxFeat.Dispose();
                }
                poiFeat.Dispose();
                dzxDS.Dispose();
                tickTime++;
            });
        }
    }
}
