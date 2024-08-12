using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools
{

    static class StaticTools
    {
        /// <summary>
        /// 求一个数组的标准差
        /// </summary>
        /// <param name="suzu"></param>
        /// <returns></returns>
        public static double myBiaoZhunC(double[] suzu)
        {
            double pingjunzhi = suzu.Average();
            double temp = 0;
            for (int i = 0; i < suzu.Length; i++)
            {
                temp += (suzu[i] - pingjunzhi) * (suzu[i] - pingjunzhi);
            }
            return Math.Sqrt(temp / suzu.Length);
        }

        /************************ for log ***********************************/

        public static void writeInLog(this List<string> filePath, string what = "")
        {
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }

            StreamWriter sw = new StreamWriter(@"D:\TEMPFORGETDATATOOLS\log", true);

            foreach (string aPath in filePath)
            {
                string whatIsay = "【" + what + "】：" + aPath;
                sw.WriteLine(whatIsay);
            }
            sw.Close();
        }
        public static void writeInLog(this string filePath, string what = "")
        {
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }
            StreamWriter sw = new StreamWriter(@"D:\TEMPFORGETDATATOOLS\log", true);
            string whatIsay = "【" + what + "】：" + filePath;
            sw.WriteLine(whatIsay);
            sw.Close();
        }

        /*******************************  拷贝、删除shp文件组 *******************************************************/
        /// <summary>
        /// 删除shp文件组
        /// </summary>
        /// <param name="filePath"></param>
        public static void deleteFiles(this string filePath)
        {
            string files = filePath.Substring(0, filePath.LastIndexOf("."));
            System.IO.File.Delete(files + ".shp");
            System.IO.File.Delete(files + ".dbf");
            System.IO.File.Delete(files + ".shx");
            System.IO.File.Delete(files + ".qix");
            System.IO.File.Delete(files + ".prj");
            System.IO.File.Delete(files + ".img");
            System.IO.File.Delete(files + ".rrd");
            System.IO.File.Delete(files + ".cpg");
        }
        public static void copyFiles(this string filePath, string tagPath)
        {
            string files = filePath.Substring(0, filePath.LastIndexOf("."));
            string tag = tagPath.Substring(0, tagPath.LastIndexOf("."));
            if (System.IO.File.Exists(files + ".shp"))
                System.IO.File.Copy(files + ".shp", tag + ".shp");
            if (System.IO.File.Exists(files + ".dbf"))
                System.IO.File.Copy(files + ".dbf", tag + ".dbf");
            if (System.IO.File.Exists(files + ".shx"))
                System.IO.File.Copy(files + ".shx", tag + ".shx");
            if (System.IO.File.Exists(files + ".qix"))
                System.IO.File.Copy(files + ".qix", tag + ".qix");
            if (System.IO.File.Exists(files + ".prj"))
                System.IO.File.Copy(files + ".prj", tag + ".prj");
            if (System.IO.File.Exists(files + ".img"))
                System.IO.File.Copy(files + ".img", tag + ".img");
            if (System.IO.File.Exists(files + ".rrd"))
                System.IO.File.Copy(files + ".rrd", tag + ".rrd");
            if (System.IO.File.Exists(files + ".cpg"))
                System.IO.File.Copy(files + ".cpg", tag + ".cpg");
        }

        /****************************** Delete Feature Update ********************************************/

        public static void deleteFeatUpdate(this OSGeo.OGR.DataSource myDS)
        {
            string a = "REPACK " + myDS.GetLayerByIndex(0).GetName();
            myDS.ExecuteSQL(a, null, "");
            myDS.Dispose();
        }

        /************************* SHP *** Point[] *** X[] Y[] *********************************/


        /// <summary>
        /// Point[] ---> X[]\Y[]\Z[]
        /// </summary>
        /// <param name="PointGroup"></param>
        /// <param name="XorYorZ"></param>
        /// <returns></returns>
        public static double[] pointGroupToArrayGroup(this Point[] PointGroup, string XorYorZ)
        {
            int groupCount = PointGroup.Length;
            double[] singleG = new double[groupCount];
            if (XorYorZ == "X")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].X;
                }
            }
            else if (XorYorZ == "Y")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].Y;
                }
            }
            else if (XorYorZ == "Z")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].Z;
                }
            }
            else
                return null;
            return singleG;
        }
        /// <summary>
        /// Polygon.Feat(no Dispose) -----> Point[]
        /// </summary>
        /// <param name="aPolyFeat"></param>
        /// <returns></returns>
        public static Point[] aPolyFeatToPoingGroup(this Feature aPolyFeat)
        {
            Ogr.RegisterAll();
            Geometry geom = aPolyFeat.GetGeometryRef();
            Geometry subGeom = geom.GetGeometryRef(0);
            int pointCount = subGeom.GetPointCount();
            Point[] toPG = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                toPG[i].X = subGeom.GetX(i);
                toPG[i].Y = subGeom.GetY(i);
                toPG[i].Z = subGeom.GetZ(i);
            }
            subGeom.Dispose();
            geom.Dispose();
            return toPG;
        }
        /// <summary>
        /// shpPoint.Layer(no Dispose) -----> Point[]
        /// </summary>
        /// <param name="pointLayer"></param>
        /// <returns></returns>
        public static Point[] shpPointToPointGroup(this Layer pointLayer)
        {
            long pointCount = pointLayer.GetFeatureCount(0);
            Point[] toPG = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                Feature apoint = pointLayer.GetFeature(i);
                Geometry subGeom = apoint.GetGeometryRef();
                toPG[i].X = subGeom.GetX(0);
                toPG[i].Y = subGeom.GetY(0);
                toPG[i].Z = subGeom.GetZ(0);
                apoint.Dispose();
            }
            return toPG;
        }
        /// <summary>
        /// Line.Feat ----> Point[]
        /// </summary>
        /// <param name="aLineFeat"></param>
        /// <returns></returns>
        public static Point[] aLineFeatToPointGroup(this Feature aLineFeat)
        {
            Ogr.RegisterAll();
            Geometry subGeom = aLineFeat.GetGeometryRef();
            int pointCount = subGeom.GetPointCount();
            Point[] toPG = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                toPG[i].X = subGeom.GetX(i);
                toPG[i].Y = subGeom.GetY(i);
                toPG[i].Z = subGeom.GetZ(i);
            }
            subGeom.Dispose();
            return toPG;
        }

        /**************************************求交***************************************************/
        /// <summary>
        /// 两直线求交,返回交点 线公式 AX+BY+C1=0;
        /// </summary>
        /// <param name="lineA_z"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        public static Point jiaoD(this LineD lineA_z, LineD lineB)
        {
            //定义交点
            Point res = new Point();
            double X1 = 0, X2 = 0, X3 = 0, X4 = 0, Y1 = 0, Y2 = 0, Y3 = 0, Y4 = 0;
            if (Math.Abs(lineA_z.Start.X - lineA_z.End.X) > 0.01)
            {
                //lineA的两点坐标
                X1 = lineA_z.Start.X;
                Y1 = lineA_z.Start.Y;
                X2 = lineA_z.End.X;
                Y2 = lineA_z.End.Y;
                //lineB的两点坐标
                X3 = lineB.Start.X;
                Y3 = lineB.Start.Y;
                X4 = lineB.End.X;
                Y4 = lineB.End.Y;
            }
            else if (Math.Abs(lineB.Start.X - lineB.End.X) > 0.01)
            {
                //lineB的两点坐标
                X1 = lineB.Start.X;
                Y1 = lineB.Start.Y;
                X2 = lineB.End.X;
                Y2 = lineB.End.Y;
                //lineA的两点坐标
                X3 = lineA_z.Start.X;
                Y3 = lineA_z.Start.Y;
                X4 = lineA_z.End.X;
                Y4 = lineA_z.End.Y;
            }
            else
            {

                MessageBox.Show("求交过程中被输入两条平行线,无法获得交点");
            }
            //lineA套入直线公式
            double A1 = Y2 - Y1;
            double B1 = X1 - X2;
            double C1 = X2 * Y1 - X1 * Y2;
            //lineB套入直线公式
            double A2 = Y4 - Y3;
            double B2 = X3 - X4;
            double C2 = X4 * Y3 - X3 * Y4;
            //A1*X+B1*Y+C1=0;
            //A2*X+B2*Y+C2=0;

            ///y=a0+a1*x
            ///a1*x-y+a0=0
            ///A=a1
            ///B=-1
            ///
            //解出交点

            // 1 提出lineA中的未知数Y
            //      Y*B1 = -A1*X - C1;
            //      Y = -X*A1/B1 - C1/B1;
            // 2 把Y 套入 lineB的公式中
            //      A2*X+B2*(-A1/B1 * X - C1/B1)+C2=0;
            // 3 提出lineB的公式中的未知数X
            //      A2*X + B2*(-X*A1/B1 - C1/B1) + C2=0;
            //      double g =B2*(A1/B1);
            //      double h =B2*(C1/B1);
            //      A2*X + (-B2*(A1/B1) * X - B2*(C1/B1)) + C2=0;
            //      A2*X + (-g * X - h) + C2=0;
            //      A2*X - g * X =h-C2;
            //      (A2-g)*X=h-C2;
            //      X=(h-C2)/(A2-g);
            res.X = (B2 * (C1 / B1) - C2) / (A2 - B2 * (A1 / B1));
            //4 把X套入步骤 1的公式
            res.Y = -res.X * A1 / B1 - C1 / B1;
            res.Z = lineA_z.Start.Z;
            return res;
        }
        /// <summary>
        /// 判断两条线段是否相交。
        /// </summary>
        /// <param name="line1">线段1</param>
        /// <param name="line2">线段2</param>
        /// <returns>相交返回真，否则返回假。</returns>
        public static bool CheckTwoLineCrose(LineD line1, LineD line2)
        {
            return CheckCrose(line1, line2) && CheckCrose(line2, line1);
        }
        /// <summary>
        /// 判断直线2的两点是否在直线1的两边。
        /// </summary>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <returns></returns>
        private static bool CheckCrose(LineD line1, LineD line2)
        {
            Point v1 = new Point();
            Point v2 = new Point();
            Point v3 = new Point();

            v1.X = line2.Start.X - line1.End.X;
            v1.Y = line2.Start.Y - line1.End.Y;

            v2.X = line2.End.X - line1.End.X;
            v2.Y = line2.End.Y - line1.End.Y;

            v3.X = line1.Start.X - line1.End.X;
            v3.Y = line1.Start.Y - line1.End.Y;

            return (CrossMul(v1, v3) * CrossMul(v2, v3) <= 0);

        }
        /// <summary>
        /// 计算两个向量的叉乘。
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        private static double CrossMul(Point pt1, Point pt2)
        {
            return pt1.X * pt2.Y - pt1.Y * pt2.X;
        }
        /*************************************************************************************************/

        /****************************************  拟合函数  ************************************************/

        #region 拟合函数

        //多项式拟合
        static void a(double[] fitting)
        {
            double X = 0.000;
            double Y = fitting[0];
            for (int i = 1; i < fitting.Length; i++)
            {
                Y += fitting[i] * Math.Pow(X, i);
            }
        }

        //返回值是函数的系数
        //例如：1,y=a0+a1*x 返回值则为a0 a1   --------------------------->  a1*X +(-1)* Y +a0 = 0
        //例如：2,y=a0+a1*x+a2*x*x 返回值则为a0 a1 a2       ------------->  A *X + B  * Y + C = 0 
        //例如：3,y=a0+a1*x+a2*x*x+a3*x*x*x 返回值则为a0 a1 a2 a3    ---->  A = a1 ; B = -1 ; C = a0;

        ///用最小二乘法拟合二元多次曲线
        ///</summary>
        ///<param name="arrX">已知点的x坐标集合</param>
        ///<param name="arrY">已知点的y坐标集合</param>
        ///<param name="length">已知点的个数</param>
        ///<param name="dimension">方程的最高次数</param>
        public static double[] MultiLine(double[] arrX, double[] arrY, int length, int dimension)//二元多次线性方程拟合曲线
        {
            int n = dimension + 1;                  //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1];      //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++)
            {
                int j;
                for (j = 0; j < n; j++)
                {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);
        }
        private static double SumArr(double[] arr, int n, int length) //求数组的元素的n次方的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }
        private static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length)
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;

        }
        private static double[] ComputGauss(double[,] Guass, int n)
        {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];
            for (i = 0; i < n; i++) x[i] = 0.0;//初始化

            for (j = 0; j < n; j++)
            {
                max = 0;
                k = j;
                for (i = j; i < n; i++)
                {
                    if (Math.Abs(Guass[i, j]) > max)
                    {
                        max = Guass[i, j];
                        k = i;
                    }
                }


                if (k != j)
                {
                    for (m = j; m < n + 1; m++)
                    {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;
                    }
                }
                if (0 == max)
                {
                    // "此线性方程为奇异线性方程" 
                    return x;
                }

                for (i = j + 1; i < n; i++)
                {
                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++)
                    {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);
                    }
                }

            }//结束for (j=0;j<n;j++)

            for (i = n - 1; i >= 0; i--)
            {
                s = 0;
                for (j = i + 1; j < n; j++)
                {
                    s = s + Guass[i, j] * x[j];
                }
                x[i] = (Guass[i, n] - s) / Guass[i, i];
            }
            return x;
        }
        /// <summary>
        /// 拟合直线求交
        ///  A = a1 ; B = -1 ; C = a0;
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        public static Tuple<double, double> jiaodian(double[] a, double[] b)
        {
            double X, Y;
            if (a[1] != b[1])
            {
                X = (b[0] - a[0]) / (a[1] - b[1]);
                Y = a[0] + a[1] * X;
            }
            else
            { X = 0; Y = 0; }
            Tuple<double, double> Coord = Tuple.Create<double, double>(X, Y);
            return Coord;
        }
      
        #endregion

        /*************************************************************************************************/

        /************************************** TempFile ****************************************************/

        public static string tempFilePath(string format, string whatAmI = "")
        {
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }
            int i = 0;
            while (true)
            {
                string filePath = @"D:\TEMPFORGETDATATOOLS\Temp"
                    + format.ToUpper()
                    + "File"
                    + i.ToString()
                    + "_"
                    + whatAmI
                    + "."
                    + format;
                if (File.Exists(filePath))
                { i++; }
                else
                { return filePath; }
            }
        }


        /*******************************TransFrom 与坐标转换**************************************************/

        /// <summary>
        /// 从值数组的索引转成图像坐标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="xSize"></param>
        /// <param name="pixel"></param>
        /// <param name="line"></param>
        public static void indexToImgspace(int index, int xSize, out int pixel, out int line)
        {
            pixel = (index + 1) % xSize;
            line = index / xSize;
        }
        /// <summary>
        /// 从图像坐标转成值数组的索引
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="line"></param>
        /// <param name="zSize"></param>
        /// <returns></returns>
        public static int imgSpaceToIndex(int pixel, int line, int zSize)
        {
            return line * zSize + pixel;
        }
        /// <summary>
        /// 从像素空间转换到地理空间
        /// </summary>
        /// <param name="adfGeoTransform">影像坐标变换参数</param>
        /// <param name="pixel">像素所在行</param>
        /// <param name="line">像素所在列</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public static void imageToGeoSpace(double[] m_GeoTransform, int pixel, int line, out double X, out double Y)
        {
            X = m_GeoTransform[0] + pixel * m_GeoTransform[1] + line * m_GeoTransform[2];
            Y = m_GeoTransform[3] + pixel * m_GeoTransform[4] + line * m_GeoTransform[5];
        }

        /// <summary>
        /// 从地理空间转换到像素空间
        /// </summary>
        /// <param name="adfGeoTransform">影像坐标变化参数</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="pixel">像素所在行</param>
        /// <param name="line">像素所在列</param>
        public static void geoToImageSpace(double[] m_GeoTransform, double x, double y, out int pixel, out int line)
        {
            line = (int)((y * m_GeoTransform[1] - x * m_GeoTransform[4] + m_GeoTransform[0] * m_GeoTransform[4] - m_GeoTransform[3] * m_GeoTransform[1]) / (m_GeoTransform[5] * m_GeoTransform[1] - m_GeoTransform[2] * m_GeoTransform[4]));
            pixel = (int)((x - m_GeoTransform[0] - line * m_GeoTransform[2]) / m_GeoTransform[1]);
        }


        /************************************* 判断两个Featuer是否重复 *************************************************/
        /// <summary>
        /// 判断两个Featuer是否重复，ori 当前Feat，next 目标Feat
        /// </summary>
        /// <param name="ori"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static bool isSame(OSGeo.OGR.Feature ori, OSGeo.OGR.Feature next, double fanWei = 0.1)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Geometry oriGeom = ori.GetGeometryRef();
            OSGeo.OGR.Envelope oriEnve = new OSGeo.OGR.Envelope();
            oriGeom.GetEnvelope(oriEnve);
            OSGeo.OGR.Geometry nextGeom = next.GetGeometryRef();
            OSGeo.OGR.Envelope nextEnve = new OSGeo.OGR.Envelope();
            nextGeom.GetEnvelope(nextEnve);
            double oriArea = oriGeom.GetArea();
            double nextArea = nextGeom.GetArea();
            bool res =
                Math.Abs(oriEnve.MaxX - nextEnve.MaxX) < fanWei && //外接矩形差
               Math.Abs(oriEnve.MaxY - nextEnve.MaxY) < fanWei &&
               Math.Abs(oriEnve.MinX - nextEnve.MinX) < fanWei &&
               Math.Abs(oriEnve.MinY - nextEnve.MinY) < fanWei;
            //面积？    && Math.Abs(oriArea - nextArea) < 0.1;
            oriGeom.Dispose();
            oriEnve.Dispose();
            nextGeom.Dispose();
            nextEnve.Dispose();
            return res;
        }
        public static void contion()
        {
            System.Windows.Forms.MessageBox.Show("抱歉！您未得到授权！");
        }
    }




    /************************************************  通过Framwork类库中的Regex类实现了一些特殊功能数据检查     *****************************************************************************/
    /// <summary>  
    /// 通过Framwork类库中的Regex类实现了一些特殊功能数据检查  
    /// </summary>  
    static class MetarnetRegex
    {

        /// <summary>  
        /// 判断输入的字符串只包含汉字  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsChineseCh(string input)
        {
            //Regex regex = new Regex("^[u4e00-u9fa5]+$");  

            //改了一下  

            Regex regex = new Regex(@"^[u4e00-u9fa5]+$");
            return regex.IsMatch(input);
        }

        /// <summary>  
        /// 匹配3位或4位区号的电话号码，其中区号可以用小括号括起来，  
        /// 也可以不用，区号与本地号间可以用连字号或空格间隔，  
        /// 也可以没有间隔  
        /// 0\d2[- ]?\d{8}|0\d{2}[- ]?\d{8}|0\d3[- ]?\d{7}|0\d{3}[- ]?\d{7}  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsPhone(string input)
        {
            string pattern = "^\\(0\\d{2}\\)[- ]?\\d{8}$|^0\\d{2}[- ]?\\d{8}$|^\\(0\\d{3}\\)[- ]?\\d{7}$|^0\\d{3}[- ]?\\d{7}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        /// <summary>  
        /// 判断输入的字符串是否是一个合法的手机号  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsMobilePhone(string input)
        {
            Regex regex = new Regex("^13\\d{9}$");
            return regex.IsMatch(input);

        }


        /// <summary>  
        /// 判断输入的字符串只包含数字  
        /// 可以匹配整数和浮点数  
        /// ^-?\d+$|^(-?\d+)(\.\d+)?$  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsNumber(string input)
        {
            string pattern = "^-?\\d+$|^(-?\\d+)(\\.\\d+)?$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        /// <summary>  
        /// 匹配非负整数  
        ///  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsNotNagtive(string input)
        {
            Regex regex = new Regex(@"^\d+$");
            return regex.IsMatch(input);
        }
        /// <summary>  
        /// 匹配正整数  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsUint(string input)
        {
            Regex regex = new Regex("^[0-9]*[1-9][0-9]*$");
            return regex.IsMatch(input);
        }
        /// <summary>  
        /// 判断输入的字符串字包含英文字母  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsEnglisCh(string input)
        {
            Regex regex = new Regex("^[A-Za-z]+$");
            return regex.IsMatch(input);
        }


        /// <summary>  
        /// 判断输入的字符串是否是一个合法的Email地址  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsEmail(string input)
        {
            string pattern = @"^([\w-\.]+)@((
[0−9]1,3\.[0−9]1,3\.[0−9]1,3\.)|(([\w−]+\.)+))([a−zA−Z]2,4|[0−9]1,3)(
?)$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }


        /// <summary>  
        /// 判断输入的字符串是否只包含数字和英文字母  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsNumAndEnCh(string input)
        {
            string pattern = @"^[A-Za-z0-9]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }


        /// <summary>  
        /// 判断输入的字符串是否是一个超链接  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsURL(string input)
        {
            //string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";  
            string pattern = @"^[a-zA-Z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }


        /// <summary>  
        /// 判断输入的字符串是否是表示一个IP地址  
        /// </summary>  
        /// <param name="input">被比较的字符串</param>  
        /// <returns>是IP地址则为True</returns>  
        public static bool IsIPv4(string input)
        {

            string[] IPs = input.Split('.');
            Regex regex = new Regex(@"^\d+$");
            for (int i = 0; i < IPs.Length; i++)
            {
                if (!regex.IsMatch(IPs[i]))
                {
                    return false;
                }
                if (Convert.ToUInt16(IPs[i]) > 255)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>  
        /// 计算字符串的字符长度，一个汉字字符将被计算为两个字符  
        /// </summary>  
        /// <param name="input">需要计算的字符串</param>  
        /// <returns>返回字符串的长度</returns>  
        public static int GetCount(string input)
        {
            return Regex.Replace(input, @"[\一-\龥/g]", "aa").Length;
        }

        /// <summary>  
        /// 调用Regex中IsMatch函数实现一般的正则表达式匹配  
        /// </summary>  
        /// <param name="pattern">要匹配的正则表达式模式。</param>  
        /// <param name="input">要搜索匹配项的字符串</param>  
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>  
        public static bool IsMatch(string pattern, string input)
        {
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        /// <summary>  
        /// 从输入字符串中的第一个字符开始，用替换字符串替换指定的正则表达式模式的所有匹配项。  
        /// </summary>  
        /// <param name="pattern">模式字符串</param>  
        /// <param name="input">输入字符串</param>  
        /// <param name="replacement">用于替换的字符串</param>  
        /// <returns>返回被替换后的结果</returns>  
        public static string Replace(string pattern, string input, string replacement)
        {
            Regex regex = new Regex(pattern);
            return regex.Replace(input, replacement);
        }

        /// <summary>  
        /// 在由正则表达式模式定义的位置拆分输入字符串。  
        /// </summary>  
        /// <param name="pattern">模式字符串</param>  
        /// <param name="input">输入字符串</param>  
        /// <returns></returns>  
        public static string[] Split(string pattern, string input)
        {
            Regex regex = new Regex(pattern);
            return regex.Split(input);
        }

        /// <summary>  
        /// 判断输入的字符串是否是合法的IPV6 地址  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsIPV6(string input)
        {
            string pattern = "";
            string temp = input;
            string[] strs = temp.Split(':');
            if (strs.Length > 8)
            {
                return false;
            }
            int count = MetarnetRegex.GetStringCount(input, "::");
            if (count > 1)
            {
                return false;
            }
            else if (count == 0)
            {
                pattern = @"^([\da-f]{1,4}:){7}[\da-f]{1,4}$";

                Regex regex = new Regex(pattern);
                return regex.IsMatch(input);
            }
            else
            {
                pattern = @"^([\da-f]{1,4}:){0,5}::([\da-f]{1,4}:){0,5}[\da-f]{1,4}$";
                Regex regex1 = new Regex(pattern);
                return regex1.IsMatch(input);
            }

        }
        /* ******************************************************************* 
        * 1、通过“:”来分割字符串看得到的字符串数组长度是否小于等于8 
        * 2、判断输入的IPV6字符串中是否有“::”。 
        * 3、如果没有“::”采用 ^([\da-f]{1,4}:){7}[\da-f]{1,4}$ 来判断 
        * 4、如果有“::” ，判断"::"是否止出现一次 
        * 5、如果出现一次以上 返回false 
        * 6、^([\da-f]{1,4}:){0,5}::([\da-f]{1,4}:){0,5}[\da-f]{1,4}$ 
        * ******************************************************************/
        /// <summary>  
        /// 判断字符串compare 在 input字符串中出现的次数  
        /// </summary>  
        /// <param name="input">源字符串</param>  
        /// <param name="compare">用于比较的字符串</param>  
        /// <returns>字符串compare 在 input字符串中出现的次数</returns>  
        private static int GetStringCount(string input, string compare)
        {
            int index = input.IndexOf(compare);
            if (index != -1)
            {
                return 1 + GetStringCount(input.Substring(index + compare.Length), compare);
            }
            else
            {
                return 0;
            }

        }
    }
}
