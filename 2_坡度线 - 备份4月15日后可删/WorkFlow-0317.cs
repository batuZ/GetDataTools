using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度线
{
    class WorkFlow
    {
        /// <summary>
        /// 新建坡度-------单线CUT------OK
        /// </summary>
        public static string poDu(string inIMGPath, int jiao)
        {
            //用来放sub坡度线的
            List<string> subPdPoly = new List<string>();
            List<string> subPdLine = new List<string>();


            Console.WriteLine("开始切图！");
            //提高分辨率
            //string reProjectIMG = StaticTools.tempFilePath("img", "reProjectIMG");
            //CutImg.ImgReProject(inIMGPath, reProjectIMG);
            List<string> cutFiles = CutImg.getSubImg(inIMGPath);
            cutFiles.writeInLog("subDSM");

            Console.WriteLine("切图完毕，开始单线程计算坡度线");
            for (int i = 0; i < cutFiles.Count; i++)
            {
                Console.WriteLine("开始处理第{0}个图，共{1}个", i + 1, cutFiles.Count);

                string inSubfile = cutFiles[i];
                string slopeMap = StaticTools.tempFilePath("img", "slop");
                string slopePoly = StaticTools.tempFilePath("shp", "slopPoly");
                string slopLine = StaticTools.tempFilePath("shp", "shpLine");

                slopeMap.writeInLog("slopMap");
                (new SlopeDem()).Slope(inSubfile, slopeMap);
                (new SlopeDem()).CreatePolygon(inSubfile, slopeMap, slopePoly, slopLine, 80);
                subPdPoly.Add(slopePoly);
                subPdLine.Add(slopLine);

                Console.WriteLine("第{0}个图处理完成，共{1}个", i + 1, cutFiles.Count);
            }

            Console.WriteLine("开始整合坡度线");

            string res = CutImg.zhengHePdx(subPdPoly);
            CutImg.zhengHePdLine(subPdLine);
            Console.WriteLine("坡度线整合完成！");


            return res;
        }

        //用来放sub坡度线的
        static List<string> subPdPoly = new List<string>();
        static List<string> subPdLine = new List<string>();
        static int aRound = 0;
        /// <summary>
        ///  新建坡度-------多线CUT------OK
        /// </summary>
        /// <param name="inIMGPath"></param>
        /// <param name="jiao"></param>
        /// <returns></returns>
        public static string poDuM(string inIMGPath, int Lev)
        {

            Console.WriteLine("开始切图！");
            //提高分辨率
            //string reProjectIMG = StaticTools.tempFilePath("img", "reProjectIMG");
            //CutImg.ImgReProject(inIMGPath, reProjectIMG);

            List<string> cutFiles = CutImg.getSubImg(inIMGPath);

            lock (@"D:\TEMPFORGETDATATOOLS\log")
            { cutFiles.writeInLog("subDSM"); }

            Console.WriteLine("切图完毕，开始多线程计算坡度线");

            subPdPoly.Clear();
            subPdLine.Clear();
            //分发任务
            for (int t = 0; t < Lev; t++)
            {
                for (int i = 0; i < cutFiles.Count; i++)
                {
                    GetSlopeFile(
                        cutFiles[i], //subDSM
                        (85 - t * 5), // 一级减5度
                        i + 1, //序列
                        cutFiles.Count);//总数量
                }
                while (aRound != cutFiles.Count)
                {
                    Console.Write("*");
                    Thread.Sleep(2000);
                }
                aRound = 0;
            }
            while (subPdPoly.Count != cutFiles.Count * Lev)
            {
                Console.Write(".");
                Thread.Sleep(2000);
            }

            Console.WriteLine("开始整合坡度线");

            string res = CutImg.zhengHePdx(subPdPoly);

            CutImg.zhengHePdLine(subPdLine);

            Console.WriteLine("坡度线整合完成！");

            return res;
        }
        async static void GetSlopeFile(string filePath, int jiao, int s, int ss)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("开始处理{0}号图Lev{1}，共{2}", s + 1,18 - jiao / 5, ss);
                string inSubfile = filePath;
                string slopeMap = StaticTools.tempFilePath("img", jiao.ToString() + "_slop_" + s.ToString());
                string slopePoly = StaticTools.tempFilePath("shp", jiao.ToString() + "_slopPoly_" + s.ToString());
                string slopLine = StaticTools.tempFilePath("shp", jiao.ToString() + "_shpLine_" + s.ToString());
                lock (@"D:\TEMPFORGETDATATOOLS\log")
                {
                    slopeMap.writeInLog("slopMap");
                    slopePoly.writeInLog("slopePoly");
                    slopLine.writeInLog("slopLine");
                }

                (new SlopeDem()).Slope(inSubfile, slopeMap);
                Console.WriteLine("{0}号图Lev{1},slopMap=>slopPolygon", s + 1, 18 - jiao / 5);
                (new SlopeDem()).CreatePolygon(inSubfile, slopeMap, slopePoly, slopLine, jiao);
                subPdPoly.Add(slopePoly);
                subPdLine.Add(slopLine);
                aRound++;
                Console.WriteLine("{0}号图Lev{3}处理完成，{1}/{2}完成", s + 1, subPdPoly.Count, ss, 18 - jiao / 5);
            });
        }
    }
}
