using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test01
{
    //点\单多边形\单线
    struct Point
    {
        public double X;
        public double Y;
        public double Z;
    }
    struct DemCutData
    {
        public string DemPath;
        public int cutIndex;
        public int Weight;
        public int High;
        public int OverLapPixel;

        public DemCutData(string demPath, int cut_index, int Weight, int High, int OverlapPixel)
        {
            this.DemPath = demPath;
            this.cutIndex = cut_index;
            this.Weight = Weight;
            this.High = High;
            this.OverLapPixel = OverlapPixel;
        }
    }

    /// <summary>
    /// 截取一个像素宽度800X800的区域
    /// </summary>
    struct CutData
    {
        public string DemPath;
        public string SlopePath;
        public string PolygonShpPath;
        public int cutIndex;
        public int Weight;
        public int High;
        public double ImportLevel;
        public CutData(string dempath, string slpPath, string shpPath, int cut_index, double importLevel)
        {
            this.DemPath = dempath;
            this.SlopePath = slpPath;
            this.PolygonShpPath = shpPath;
            this.cutIndex = cut_index;
            this.ImportLevel = importLevel;

            this.Weight = 800;
            this.High = 800;
        }
    }
    public class OrderPoint
    {
        public int X;
        public int Y;
        public Toward Direct;//定义指向下一点的朝向
        public int pID;//简化节点的时候需要用到

        public OrderPoint()
        { }
        public OrderPoint(int _X, int _Y)
        {
            X = _X;
            Y = _Y;
        }
    }
    /// <summary>
    /// 八邻域朝向（从左上角1开始）
    /// </summary>
    public enum Toward
    {
        Left_top = 0,
        Top = 1,
        Right_top = 2,
        Right = 3,
        Right_bot = 4,
        Bot = 5,
        Left_bot = 6,
        Left = 7
    }
    /// <summary>
    /// Angular = 4表示与之相邻的像素块有4块（含本身）
    /// </summary>
    public enum BlockType : int
    {
        Angular_LeftTop = 1,
        Angular_RightTop = 3,
        Angular_LeftBot = 5,
        Angular_RightBot = 7,
        Edge_Top = 2,
        Edge_Right = 4,
        Edge_Bot = 6,
        Edge_Left = 8,
        Center = 9
    }
    public class LineSelfSect
    {
        private static LineSelfSect _instance;
        public static LineSelfSect Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LineSelfSect();
                return _instance;
            }
        }
        /// <summary>
        /// 判断自相交图形
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<OrderPoint> RingedList(List<OrderPoint> list)
        {
            foreach (OrderPoint item in list)
            {
                int _itemIndex = list.IndexOf(item);
                List<OrderPoint> findlist = list.FindAll((a) =>
                {
                    if (Math.Abs(a.X - item.X) < 3 &&
                        Math.Abs(a.Y - item.Y) < 3 &&
                        Math.Abs(list.IndexOf(a) - list.IndexOf(item)) > 25)
                        return true;
                    else
                        return false;
                });
                if (findlist.Count > 0)
                {
                    int min = list.IndexOf(findlist[0]);
                    if (min < _itemIndex)
                        return list.GetRange(min, _itemIndex - min);
                    else
                        return list.GetRange(_itemIndex, min - _itemIndex);
                }
            }
            return null;
        }
    }
}
