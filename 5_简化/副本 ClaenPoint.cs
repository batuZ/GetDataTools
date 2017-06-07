using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetDataTools;

namespace GetDataTools.SelectPoint
{
    /// <summary>
    /// 用来清理等值线上接近平角的点
    /// 
    /// </summary>
    class CleanPoint
    {

        /// <summary>
        /// 清理等值线上的点
        /// </summary>
        /// <param name="filePath"></param>
        public static void claenPoint(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            //进来的Layer
            OSGeo.OGR.DataSource oriDs = dr.Open(filePath, 0);
            OSGeo.OGR.Layer oriLayer = oriDs.GetLayerByIndex(0);
            //出去的Layer
            if (System.IO.File.Exists(filePath.Substring(0, filePath.LastIndexOf(".")) + "_targ.shp"))
            {
               // UsefullTools.deleteFiles(filePath.Substring(0, filePath.LastIndexOf(".")) + "_targ.shp");
            }
            OSGeo.OGR.DataSource targDs = dr.CreateDataSource(filePath.Substring(0, filePath.LastIndexOf(".")) + "_targ.shp", null);
            OSGeo.OGR.Layer targLayer = targDs.CreateLayer("targ", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);

            int featCount = oriLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                ///进来的Featuer
                OSGeo.OGR.Feature oriFeat = oriLayer.GetFeature(i);

                ///把一个Featuer转为点数组
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

                ///调选点方法,得到一个新的Featuer
                OSGeo.OGR.Feature newFeat = JID(aFeat);

                if (newFeat != null)
                {
                    targLayer.CreateFeature(newFeat);
                }

            }
            oriDs.Dispose();
            targDs.Dispose();
        }



        /// <summary>
        /// 三点夹角的判定条件,输出为满足条件的成员的ID所组成的ID数组
        /// </summary>
        /// <param name="aFeat"></param>
        /// <returns></returns>
        private static OSGeo.OGR.Feature JID(Point[] aFeat)
        {
            int userSet = 165;
            int seleTime = 10;
            List<Point[]> pjGroupL = new List<Point[]>();
            List<Point[]> zjGroupL = new List<Point[]>();

            List<Point> pjGroup = new List<Point>();//平角
            List<Point> zjGroup = new List<Point>();//平角

            for (int i = 0; i < aFeat.Length; i++)
            {
                int frontId, thisId, backId;
                bool[] yon = new bool[seleTime];
                for (int t = 1; t <= seleTime; t++)
                {
                    if (i < t)
                    {
                        frontId = aFeat.Length - 1 + i - t;
                    }
                    else
                    {
                        frontId = i - t;
                    }

                    thisId = i;

                    if (i > aFeat.Length - 1 - t)
                    {
                        backId = i - (aFeat.Length - 1) + t;
                    }
                    else
                    {
                        backId = i + t;
                    }
                    double jiaodu = cosCalculator(aFeat[frontId], aFeat[thisId], aFeat[backId]);//求角度
                    yon[t - 1] = jiaodu > userSet;
                }

                //判定条件为:以上两条件满足其一则被选为平角,并输出为结果

                //当两有任意一个角度大于 160度. 则认为是平角
                if (yon.Contains(true))
                {
                    //if (zjGroup.Count != null)
                    //{
                    //    zjGroupL.Add(zjGroup.ToArray());
                    //    zjGroup.Clear();
                    //}
                    pjGroup.Add(aFeat[i]);
                }
                //否则认为是直角
                else
                {
                    //if (pjGroup.Count >3 )
                    //{
                    //    pjGroupL.Add(pjGroup.ToArray());
                    //    pjGroup.Clear();
                    //}
                    zjGroup.Add(aFeat[i]);
                }

            }

            ///输出Featuer

            OSGeo.OGR.FeatureDefn featDf = new OSGeo.OGR.FeatureDefn("");
            OSGeo.OGR.Feature outFeat = new OSGeo.OGR.Feature(featDf);
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
            outFeat.SetGeometry(outGeom);
            return outFeat;
        }

    
        // public static Polyline a (Polyline b)
        /// <summary>
        /// 求夹角
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static double cosCalculator(Point p1, Point p, Point p2)
        {
            double fenzi = (p1.X - p.X) * (p2.X - p.X) + (p1.Y - p.Y) * (p2.Y - p.Y);
            double fenmu = Math.Sqrt((p1.X - p.X) * (p1.X - p.X) + (p1.Y - p.Y) * (p1.Y - p.Y)) * Math.Sqrt((p2.X - p.X) * (p2.X - p.X) + (p2.Y - p.Y) * (p2.Y - p.Y));
            double cosValue = fenzi / fenmu;
            double acosV = Math.Acos(cosValue) * 180 / Math.PI;
            return acosV;
        }
    }
}
