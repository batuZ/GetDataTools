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
}
