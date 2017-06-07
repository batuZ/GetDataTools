using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._5_简化
{
    class _最小外接矩形
    {
        public static void 小外接矩形(string strVectorFile, string shpPath)
        {
            Ogr.RegisterAll();

            DataSource ds = Ogr.Open(strVectorFile, 0);
            if (ds == null)
            {
                Console.WriteLine("打开文件【{0}】失败！", strVectorFile);
                return;
            }
            Layer oLayer = ds.GetLayerByIndex(0);
            Feature oFeature = null;

            DataSource dst_DS = CreateSMBR.Instance.Gen_Datasource(shpPath);
            Layer dst_Lyr =CreateSMBR.Instance. Gen_SimplePolygonLyrByDS(dst_DS);


            while ((oFeature = oLayer.GetNextFeature()) != null)
            {
                Geometry oGeometry = oFeature.GetGeometryRef();
                Geometry ogeo = oGeometry.GetGeometryRef(0);
                QTGeometry qtGeo = new QTGeometry(ogeo);
                List<Point_qt> list = qtGeo.GetSMBR();

                Geometry dst_geo = CreateSMBR.Instance.GetSMBR_Geometry(list);
                CreateSMBR.Instance.AddGeometry2Layer(dst_Lyr, dst_geo);
            }
            dst_Lyr.Dispose();
            dst_DS.Dispose();
        }
    }

    class CreateSMBR
    {
        private static CreateSMBR _instance = null;
        private CreateSMBR() { }
        public static CreateSMBR Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CreateSMBR();
                }
                return _instance;
            }
        }

       
        public Geometry GetSMBR_Geometry(List<Point_qt> list)
        {
            Geometry geomRectangle = new Geometry(wkbGeometryType.wkbLinearRing);
            for (int i = 0; i < list.Count; i++)
            {
                geomRectangle.AddPoint_2D(list[i].X, list[i].Y);
            }
            geomRectangle.AddPoint_2D(list[0].X, list[0].Y);

            Geometry geo = new Geometry(wkbGeometryType.wkbPolygon);
            geo.AddGeometryDirectly(geomRectangle);
            return geo;
        }

        public void AddGeometry2Layer(Layer oLayer, Geometry geo)
        {
            if (oLayer == null || oLayer.GetGeomType() != wkbGeometryType.wkbPolygon)
            {
                Console.WriteLine("图层读取错误！\n");
                return;
            }
            FeatureDefn oDefn = oLayer.GetLayerDefn();

            Feature oFeatureRect = new Feature(oDefn);
            oFeatureRect.SetGeometry(geo);

            oLayer.CreateFeature(oFeatureRect);
        }

        public DataSource Gen_Datasource(string shpfilePath)
        {
            Ogr.RegisterAll();
            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

            if (File.Exists(shpfilePath))
                File.Delete(shpfilePath);
            string strDriverName = "ESRI Shapefile";
            Driver oDriver = Ogr.GetDriverByName(strDriverName);
            if (oDriver == null)
            {
                Console.WriteLine("%s 驱动不可用！\n", shpfilePath);
                return null;
            }

            // 创建数据源
            DataSource oDS = oDriver.CreateDataSource(shpfilePath, null);
            if (oDS == null)
            {
                Console.WriteLine("创建矢量文件【%s】失败！\n", shpfilePath);
                return null;
            }

            return oDS;
        }

        public Layer Gen_SimplePolygonLyrByDS(DataSource oDS)
        {
            Layer oLayer = oDS.CreateLayer("TestP", null, wkbGeometryType.wkbPolygon, null);
            if (oLayer == null)
            {
                Console.WriteLine("图层创建失败！\n");
            }
            return oLayer;
        }
    }
    class QTGeometry
    {
        Point_qt orPnt = new Point_qt();
        Geometry qtGeometry;
        public QTGeometry(Geometry geo)
        {
            qtGeometry = geo;
        }

        private Stack<Point_qt> sta_convexPlgon;//说明是否进行了GetConvexPolygon
        /// <summary>
        /// 获取凸包多边形的点集
        /// </summary>
        /// <returns>构建凸包所需点集</returns>
        public Stack<Point_qt> GetConvexPolygon()
        {
            List<Point_qt> sortList = GetSortedList();
            if (sortList.Count < 2)
                return null;
            Stack<Point_qt> staPnts = new Stack<Point_qt>(sortList.Count + 2);
            staPnts.Push(orPnt);
            staPnts.Push(sortList[0]);
            staPnts.Push(sortList[1]);

            for (int i = 2; i < sortList.Count; i++)
            {
                Foo(sortList[i], staPnts);
            }
            staPnts.Push(orPnt);

            sta_convexPlgon = staPnts;
            return staPnts;
        }

        /// <summary>
        /// 根据QTGeometry里的点集构建顺序list
        /// </summary>
        /// <returns></returns>
        private List<Point_qt> GetSortedList()
        {
            List<Point_qt> list = PutPoints2List();

            if ((list[0].X - list[list.Count - 1].X) < 0.000001 && (list[0].Y - list[list.Count - 1].Y) < 0.000001)
                list.RemoveAt(0);
            SortByX(ref list);
            orPnt = list[0];//选出X值最小的点
            list.Remove(orPnt);//移除orPnt;

            SetListPntAng(orPnt, ref list);//给list里边的Point赋角度值
            SortByAngl(ref list);//list按角度排序
            DeleSameAng(ref list);//筛选list中角度相同的点

            return list;
        }

        /// <summary>
        /// 将Geometry里的Point压入List
        /// </summary>
        /// <returns></returns>
        private List<Point_qt> PutPoints2List()
        {
            Point_qt pnt_qt;
            int pCnt = qtGeometry.GetPointCount();
            List<Point_qt> list_pnts = new List<Point_qt>(pCnt);
            for (int i = 0; i < pCnt; i++)
            {
                pnt_qt = new Point_qt(qtGeometry.GetX(i), qtGeometry.GetY(i), qtGeometry.GetZ(i));
                pnt_qt.PID = i;
                list_pnts.Add(pnt_qt);
            }
            return list_pnts;
        }

        /// <summary>
        /// 根据角度排序
        /// </summary>
        /// <param name="list"></param>
        private void SortByAngl(ref List<Point_qt> list)
        {
            list.Sort((p1, p2) =>
            {
                return p1.Rela_Ang.CompareTo(p2.Rela_Ang);
            });
        }

        /// <summary>
        /// 根据X坐标排序
        /// </summary>
        /// <param name="list"></param>
        private void SortByX(ref List<Point_qt> list)
        {
            list.Sort((p1, p2) =>
            {
                return p1.X.CompareTo(p2.X);
            });
        }

        /// <summary>
        /// 给每个点设置相对角度（角度为向量（or_Pnt,rela_Pnt）和（0，1）的夹角）
        /// </summary>
        /// <param name="or_Pnt"></param>
        /// <param name="rela_Pnt"></param>
        private void SetRelaAng(Point_qt or_Pnt, ref Point_qt rela_Pnt)
        {
            double x = rela_Pnt.X - or_Pnt.X;
            double y = rela_Pnt.Y - or_Pnt.Y;

            rela_Pnt.Rela_Ang = y / Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// 给list里边的Point设置相对角度
        /// </summary>
        /// <param name="or_Pnt"></param>
        /// <param name="list"></param>
        private void SetListPntAng(Point_qt or_Pnt, ref List<Point_qt> list)
        {
            Point_qt[] ar = list.ToArray();
            for (int i = 0; i < list.Count; i++)
            {
                SetRelaAng(or_Pnt, ref ar[i]);
            }
            list = ar.ToList();
        }

        /// <summary>
        /// 相同角度的Point，只保留相对距离远的
        /// </summary>
        /// <param name="list"></param>
        private void DeleSameAng(ref List<Point_qt> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                if ((list[i].Rela_Ang - list[i - 1].Rela_Ang) < 0.000001)
                {
                    if (Math.Abs(list[i].Y) > Math.Abs(list[i - 1].Y))
                        list.RemoveAt(i - 1);
                    else
                        list.RemoveAt(i);

                    i--;
                }
            }
        }

        /// <summary>
        /// 判断点p3是否在线段p1p2左边
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private bool IsLeft(Point_qt p1, Point_qt p2, Point_qt p3)
        {
            double x_12 = p2.X - p1.X;
            double y_12 = p2.Y - p1.Y;

            double x_13 = p3.X - p1.X;
            double y_13 = p3.Y - p1.Y;
            if ((x_12 * y_13 - y_12 * x_13) < 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 构建凸包的递归算法
        /// </summary>
        /// <param name="RelaPnt"></param>
        /// <param name="stack"></param>
        private void Foo(Point_qt RelaPnt, Stack<Point_qt> stack)
        {
            Point_qt p2 = stack.Pop();
            Point_qt p1 = stack.Pop();

            if (IsLeft(p1, p2, RelaPnt))
            {
                stack.Push(p1);
                stack.Push(p2);
                stack.Push(RelaPnt);
            }
            else
            {
                stack.Push(p1);
                Foo(RelaPnt, stack);
            }
        }



        public List<Point_qt> GetSMBR()
        {
            Point_qt p1 = new Point_qt();
            Point_qt p2 = new Point_qt();
            Point_qt p3 = new Point_qt();
            Point_qt p4 = new Point_qt();
            List<Point_qt> list = new List<Point_qt>();

            Edge_qt edge = GetBestEdge();
            p1 = CalcPnt_Coor(edge.minX, edge.FromNode, edge.ToNode);
            p2 = CalcPnt_Coor(edge.MaxX, edge.FromNode, edge.ToNode);
            p3 = ReferencePoint_Clock(edge.MaxY, p2, p1);
            p4 = ReferencePoint_unClock(edge.MaxY, p1, p2);

            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Edge_qt GetBestEdge()
        {
            List<Edge_qt> listEdges;
            if (sta_convexPlgon == null)
                GetConvexPolygon();

            listEdges = GetListEdge(sta_convexPlgon);
            Edge_qt[] arr = listEdges.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                Calc_Args(ref arr[i], sta_convexPlgon);
            }
            for (int i = 0; i < arr.Length; i++)
            {
                Calc_Area(ref arr[i]);
            }
            listEdges = arr.ToList();
            listEdges.Sort((e1, e2) =>
            {
                return e1.Area_Enve.CompareTo(e2.Area_Enve);
            });


            return listEdges[0];
        }

        /// <summary>
        /// 获得边Edge
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        private List<Edge_qt> GetListEdge(Stack<Point_qt> stack)
        {
            Edge_qt edge;
            List<Edge_qt> list = new List<Edge_qt>();
            for (int i = 1; i < stack.Count; i++)
            {
                edge = new Edge_qt();
                edge.FromNode = stack.ElementAt(i);
                edge.ToNode = stack.ElementAt(i - 1);
                list.Add(edge);
            }

            return list;
        }

        private void Calc_Args(ref Edge_qt edge, Stack<Point_qt> stack)
        {
            for (int i = 0; i < stack.Count; i++)
            {
                double dis = GetD_PL(edge, stack.ElementAt(i));
                if (dis > edge.MaxY)
                {
                    edge.MaxY = dis;
                }
                double mx = GetVecProj(edge, stack.ElementAt(i));
                if (mx < 0 && mx < edge.minX)
                {
                    edge.minX = mx;
                    edge.minXNode = stack.ElementAt(i);
                }
                else if (mx > 0 && mx > edge.MaxX)
                {
                    edge.MaxX = mx;
                    edge.MaxXNode = stack.ElementAt(i);
                }
            }
        }

        private void Calc_Area(ref Edge_qt edge)
        {
            double x = Math.Abs(edge.MaxX - edge.minX);
            double y = edge.MaxY;
            edge.Area_Enve = x * y;
        }

        /// <summary>
        /// 得到点线距
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="pnt"></param>
        /// <returns></returns>
        private double GetD_PL(Edge_qt edge, Point_qt pnt)
        {
            double x = pnt.X;
            double y = pnt.Y;

            Point_qt p1 = edge.FromNode;
            Point_qt p2 = edge.ToNode;

            double m = p2.X - p1.X;
            double n = p2.Y - p1.Y;

            double fenzi = Math.Abs(n * x - n * p1.X - m * y + m * p1.Y);
            double fenmu = Math.Sqrt(n * n + m * m);

            return fenzi / fenmu;
        }

        /// <summary>
        /// 获得向量在线段上的投影
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="pnt"></param>
        /// <returns>返回值正负皆可能</returns>
        private double GetVecProj(Edge_qt edge, Point_qt pnt)
        {
            Point_qt p1 = edge.FromNode;
            Point_qt p2 = edge.ToNode;

            double e1_x = p2.X - p1.X;
            double e1_y = p2.Y - p1.Y;
            double e2_x = pnt.X - p1.X;
            double e2_y = pnt.Y - p1.Y;
            double temp = (e1_x * e2_x + e1_y * e2_y) / Math.Sqrt(e1_x * e1_x + e1_y * e1_y);

            return (e1_x * e2_x + e1_y * e2_y) / Math.Sqrt(e1_x * e1_x + e1_y * e1_y);
        }

        /// <summary>
        /// 获得向量spnt_epnt在线段上的投影
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="spnt"></param>
        /// <param name="epnt"></param>
        /// <returns></returns>
        private double GetVecProj(Edge_qt edge, Point_qt spnt, Point_qt epnt)
        {
            Point_qt p1 = edge.FromNode;
            Point_qt p2 = edge.ToNode;

            double e1_x = p2.X - p1.X;
            double e1_y = p2.Y - p1.Y;
            double e2_x = spnt.X - epnt.X;
            double e2_y = spnt.Y - epnt.Y;

            return (e1_x * e2_x + e1_y * e2_y) / Math.Sqrt(e1_x * e1_x + e1_y * e1_y);
        }

        /// <summary>
        /// 返回向量P1P2上距离点P1 dis处的坐标
        /// </summary>
        /// <param name="dis"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Point_qt CalcPnt_Coor(double dis, Point_qt p1, Point_qt p2)
        {
            Point_qt p = new Point_qt();

            double X = p2.X - p1.X;
            double Y = p2.Y - p1.Y;
            double length = Math.Sqrt(X * X + Y * Y);
            p.X = p1.X + (p2.X - p1.X) * dis / length;
            p.Y = p1.Y + (p2.Y - p1.Y) * dis / length;

            return p;
        }
        /// <summary>
        /// 返回点p使得向量p1p和向量p1p2二维平面垂直(p1p2逆时针旋转90度得到p1p)
        /// p1p的模长为M
        /// </summary>
        /// <param name="M"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Point_qt ReferencePoint_unClock(double M, Point_qt p1, Point_qt p2)
        {
            Point_qt p = new Point_qt();
            Vec2 p1p2 = new Vec2(p2.X - p1.X, p2.Y - p1.Y);
            Vec2 p1p = p1p2.GetCrossVec2();
            if (p1p2 * p1p < 0)
            {
                p1p.X = -p1p.X;
                p1p.Y = -p1p.Y;
            }
            p1p = p1p.GetUnitVec2() * M;
            p.X = p1.X + p1p.X;
            p.Y = p1.Y + p1p.Y;

            return p;
        }
        /// <summary>
        /// 返回点p使得向量p1p和向量p1p2二维平面垂直(p1p2顺时针旋转90度得到p1p)
        /// p1p的模长为M
        /// </summary>
        /// <param name="M"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Point_qt ReferencePoint_Clock(double M, Point_qt p1, Point_qt p2)
        {
            Point_qt p = new Point_qt();
            Vec2 p1p2 = new Vec2(p2.X - p1.X, p2.Y - p1.Y);
            Vec2 p1p = p1p2.GetCrossVec2();
            if (p1p2 * p1p > 0)
            {
                p1p.X = -p1p.X;
                p1p.Y = -p1p.Y;
            }
            p1p = p1p.GetUnitVec2() * M;
            p.X = p1.X + p1p.X;
            p.Y = p1.Y + p1p.Y;

            return p;
        }
    }
    struct Point_qt
    {
        public double X;
        public double Y;
        public double Z;
        public double Rela_Ang;
        public int PID;

        public Point_qt(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Rela_Ang = 2;
            this.PID = -1;
        }
        public Point_qt(double x, double y, double z, double Rela_Ang)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Rela_Ang = Rela_Ang;
            this.PID = -1;
        }

        public Point_qt(double Rela_Ang)
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.Rela_Ang = Rela_Ang;
            this.PID = -1;
        }
    }
    struct Edge_qt
    {
        public Point_qt FromNode;
        public Point_qt ToNode;
        public Point_qt minXNode;
        public Point_qt MaxXNode;
        public double MaxY;
        public double minX;
        public double MaxX;
        public double Area_Enve;
    }
    class Vec2
    {
        public double X;
        public double Y;
        public double Length { get { return Math.Sqrt(X * X + Y * Y); } }

        public Vec2() { }
        public Vec2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vec2 GetCrossVec2()
        {
            Vec2 vec = new Vec2(Y, -X);
            return vec;
        }
        public Vec2 GetUnitVec2()
        {
            Vec2 vec = new Vec2();
            vec.X = this.X / this.Length;
            vec.Y = this.Y / this.Length;
            return vec;
        }

        public static Vec2 operator *(Vec2 lhs, double rhs)
        {
            Vec2 vec = new Vec2();
            vec.X = lhs.X * rhs;
            vec.Y = lhs.Y * rhs;
            return vec;
        }

        public static double operator *(Vec2 lhs, Vec2 rhs)
        {
            double result;
            result = lhs.X * rhs.Y - lhs.Y * rhs.X;
            return result;
        }
    }
}
