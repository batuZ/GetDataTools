using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._1_等值线
{
    class WorkFlow
    {
        public static string dzx(string inIMGPath) 
        {
            //if (Tolsk.ClassTolsk.kyes().Contains(Soft.getMNum()))
            //{
                string 原始等值线 = 生成等值线.dzx(inIMGPath);
                string cleanLine = 清理等值线.cleanDS(原始等值线);
                return 等值线转POLYGON.dzPoly(cleanLine);
            //}
            //else
            //{ return ""; }
        }
      
    }
}
