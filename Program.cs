using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace GetDataTools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Tolsk.ClassTolsk.kyes().Contains(Soft.getMNum()))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MasterWin());
            }
            else
            { StaticTools.contion(); }
        }
    }

    //点\单多边形\单线
    struct Point
    {
       public double X;
       public double Y;
       public double Z;
    }
    //描述一个线段或过两点的直线

    struct LineD
    {
        public Point Start;
        public Point End;
    }
    public partial class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
       
    }
}
