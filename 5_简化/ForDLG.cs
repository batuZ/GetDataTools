using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._5_简化
{
    class ForDLG
    {
      
        /// 1、设置分区方式

        public static void a(string asd)
        {
            Ogr.RegisterAll();
            Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource ds = dr.Open(asd, 0);
            Layer layer = ds.GetLayerByIndex(0);
            Feature feat = layer.GetFeature(0);
            Geometry geom = feat.GetGeometryRef();
            Geometry n = geom.GetGeometryRef(0);

            double s1 = 0, s2 = 0, s3 = 0, s4 = 0, s5 = 0, s6 = 0;
            int j1 = 0, j2 = 0, j3 = 0, j4 = 0, j5 = 0, j6 = 0;

            for (int i = 0; i < n.GetPointCount() - 1; i++)
            {
                double z = (n.GetY(i) - n.GetY(i + 1)) / (n.GetX(i) - n.GetX(i + 1));
                double f = Math.Atan(z) * 180 / Math.PI;
                if (f >= 75 || f <= -75)
                {
                    double f1 = f > 0 ? f : 180 - f;
                    s1 += f1;
                    j1++;
                }
                else if (f < 75 && f >= 45)
                { s2 += f; j2++; }
                else if (f < 45 && f > 15)
                { s3 += f; j3++; }
                else if (f <= 15 && f >= -15)
                { s4 += f; j4++; }
                else if (f < -15 && f > -45)
                { s5 += f; j5++; }
                else if (f <= -45 && f > -75)
                { s6 += f; j6++; }
            }

            double okValue;
        }







        public static Geometry _提取断平拟合直线(Feature aFeature, List<int> cutIDS, List<int> straightIDS)
        {
            //临时坐标集
            List<double> xArray = new List<double>();
            List<double> yArray = new List<double>();

            //Feature起点两则点的坐标集
            List<double> xArray1 = new List<double>();
            List<double> yArray1 = new List<double>();

            //原数据
            Geometry subGeom = aFeature.GetGeometryRef().GetGeometryRef(0);

            //通过线段拟合出的直线参数，返回用
            List<double[]> _直线集 = new List<double[]>();

            //平角点的个数
            int pjPoint = straightIDS.Count;

            //断点计数器
            int k = 1;

            for (int i = 0; i < pjPoint; i++)
            {
                //当平角点在第一个断点之前或在最后一个断点之后时，把点坐标值添加到指定数组中
                if (straightIDS[i] <= cutIDS[0] || straightIDS[i] >= cutIDS[cutIDS.Count - 1])//85  82 会直接跳到else 使增加空值！！！！！
                {
                    xArray1.Add(subGeom.GetX(straightIDS[i]));
                    yArray1.Add(subGeom.GetY(straightIDS[i]));
                }

                //当平角点小于当前（不是第一个）断点时，添加到数组中
                else if (straightIDS[i] < cutIDS[k])
                {
                    xArray.Add(subGeom.GetX(straightIDS[i]));
                    yArray.Add(subGeom.GetY(straightIDS[i]));
                }

                //当平角点ID超过当前断点ID时，把点集中的点拟合直线。清空数据。断点ID+1
                else
                {
                    if (xArray.Count > 5)
                    {
                        _直线集.Add(StaticTools.MultiLine(xArray.ToArray(), yArray.ToArray(), xArray.Count, 1));
                    }
                    xArray.Clear();
                    yArray.Clear();
                    xArray.Add(subGeom.GetX(straightIDS[i]));
                    yArray.Add(subGeom.GetY(straightIDS[i]));
                    k++;
                }
            }
            if (xArray.Count > 5)
                _直线集.Add(StaticTools.MultiLine(xArray.ToArray(), yArray.ToArray(), xArray.Count, 1));
            if (xArray1.Count > 5)
                _直线集.Add(StaticTools.MultiLine(xArray1.ToArray(), yArray1.ToArray(), xArray1.Count, 1));


            /******     完成拆分、拟合  ↓↓↓求交↓↓↓            ******/

            Geometry res = new Geometry(wkbGeometryType.wkbLinearRing);
            int LC = _直线集.Count;
            for (int i = 0; i < LC; i++)
            {
                int b = i == (LC - 1) ? 0 : (i + 1);
                double X = StaticTools.jiaodian(_直线集[i], _直线集[b]).Item1;
                double Y = StaticTools.jiaodian(_直线集[i], _直线集[b]).Item2;
                res.AddPoint(X, Y, 0);
            }
            return res;
        }

        public static void _Polygong节点转为ShpPoint(Feature aPoly, string outFile)
        {
            OSGeo.OGR.Geometry geom = aPoly.GetGeometryRef();
            OSGeo.OGR.Geometry subGeom = geom.GetGeometryRef(0);

            Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource outDS = dr.CreateDataSource(outFile, null);
            Layer outLayer = outDS.CreateLayer("toPoint", null, wkbGeometryType.wkbPoint25D, null);
            FieldDefn pointID = new FieldDefn("FeatID", FieldType.OFTReal);
            FieldDefn cutField = new FieldDefn("CUT", FieldType.OFTReal);
            FieldDefn straightField = new FieldDefn("STRAIGHT", FieldType.OFTInteger);
            outLayer.CreateField(pointID, 1);
            outLayer.CreateField(cutField, 1);
            outLayer.CreateField(straightField, 1);

            int pointCount = subGeom.GetPointCount();

            for (int i = 0; i < pointCount; i++)
            {
                Feature pFeat = new Feature(new FeatureDefn(""));
                Geometry pGeom = new Geometry(wkbGeometryType.wkbPoint);
                pGeom.AddPoint_2D(subGeom.GetX(i), subGeom.GetY(i));
                pFeat.SetGeometry(pGeom);
                outLayer.CreateFeature(pFeat);
                pGeom.Dispose();
                pFeat.Dispose();
            }
            outLayer.Dispose();
            outDS.Dispose();
        }
    }
}
/*↖↗↘↙↓↑←→*/



