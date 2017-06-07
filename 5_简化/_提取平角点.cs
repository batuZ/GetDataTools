using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools._5_简化
{
    class _提取平角点
    {
        public static List<int> _平角点ID集(Feature aFeatuer, double userSet, int seleTime)
        {
            OSGeo.OGR.Geometry oriGeom = aFeatuer.GetGeometryRef();
            OSGeo.OGR.Geometry subGeom = oriGeom.GetGeometryRef(0);
            int pointCount = subGeom.GetPointCount();
            Point[] pointList = new Point[pointCount];
            for (int c = 0; c < pointCount; c++)
            {
                pointList[c].X = subGeom.GetX(c);
                pointList[c].Y = subGeom.GetY(c);
                pointList[c].Z = subGeom.GetZ(c);
            }
            if (pointList.Length < seleTime * 3)
            {
                MessageBox.Show("这个图形点数小于重复次数~试试降低重复次数~或删除");
                return null;
            }
            List<int> ids = new List<int>();
            for (int i = 0; i < pointList.Length; i++)
            {
                int frontId, thisId, backId;
                bool[] yon = new bool[seleTime];
                for (int t = 1; t <= seleTime; t++)
                {
                    frontId = i < t ? pointList.Length - 1 + i - t : i - t;
                    thisId = i;
                    backId = i > pointList.Length - 1 - t ? i - (pointList.Length - 1) + t : i + t;
                    double jiaodu = cosCalculator(pointList[frontId], pointList[thisId], pointList[backId]);
                    yon[t - 1] = jiaodu > userSet;
                }
                if (yon.Contains(true))
                {
                    ids.Add(i);
                }
            }
            return ids;
        }
        private static double cosCalculator(Point p1, Point p, Point p2)
        {
            double fenzi = (p1.X - p.X) * (p2.X - p.X) + (p1.Y - p.Y) * (p2.Y - p.Y);
            double fenmu = Math.Sqrt((p1.X - p.X) * (p1.X - p.X) + (p1.Y - p.Y) * (p1.Y - p.Y)) * Math.Sqrt((p2.X - p.X) * (p2.X - p.X) + (p2.Y - p.Y) * (p2.Y - p.Y));
            double cosValue = fenzi / fenmu;
            double acosV = Math.Acos(cosValue) * 180 / Math.PI;
            return acosV;
        }
        /// <summary>
        /// 提取平角点到文件
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public static void newGeom(string inFile, string outFile, double jiao = 175, int times = 3)
        {
            Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource ds = dr.Open(inFile, 0);
            Layer layer = ds.GetLayerByIndex(0);

            var pointDs = dr.CreateDataSource(outFile, null);
            var pointLayer = pointDs.CreateLayer("PointLayer", null, wkbGeometryType.wkbPoint, null);

            int featCount = layer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                Feature afeat = layer.GetFeature(i);
                List<int> a = _平角点ID集(afeat, jiao, times);
                int straightCount = a.Count;
                for (int j = 0; j < straightCount; j++)
                {
                    Geometry geom = afeat.GetGeometryRef();
                    Geometry subGeom = geom.GetGeometryRef(0);
                    Feature pointFeat = new Feature(new FeatureDefn(""));
                    Geometry pointGeom = new Geometry(wkbGeometryType.wkbPoint);
                    pointGeom.AddPoint_2D(subGeom.GetX(j), subGeom.GetY(j));
                    pointFeat.SetGeometry(pointGeom);
                    pointLayer.CreateFeature(pointFeat);
                    pointFeat.Dispose();
                }
                afeat.Dispose();
            }
            ds.Dispose();
            pointDs.Dispose();
        }
    }
}
