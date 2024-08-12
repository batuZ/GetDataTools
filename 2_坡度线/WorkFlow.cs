using GetDataTools._2_坡度线.OutLine;
using GetDataTools._2_坡度线.坡度提取;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度线
{
    class WorkFlow
    {
        //new
        public static string poDuM(string inIMGPath, int Lev)
        {
            Gdal.AllRegister();
            Ogr.RegisterAll();

            string _outFilePath = StaticTools.tempFilePath("img", "SolpeMap");
            _outFilePath.writeInLog("SolpeMap");
            SlopeDem.Instance.Sloping(inIMGPath, _outFilePath);

            List<string> levelFiles = new List<string>();
            for (int i = 0; i < Lev; i++)
            {
                Stopwatch ssw = new Stopwatch(); ssw.Start(); Console.WriteLine("开始计算第{0}级SlopePolygon" + i + 1);
                string _outShpPath = StaticTools.tempFilePath("shp", "SolpePolyLev" + i.ToString());
                GetOutlines.Instance.MyGetOutlines(inIMGPath, _outFilePath, _outShpPath, 80 - i * 5);
                levelFiles.Add(_outShpPath);
                ssw.Stop(); Console.WriteLine("第{0}级SlopePolygon完成，用时{1}！", i + 1, ssw.Elapsed.ToString());
            }
            string res = CutImg.zhengHePdx(levelFiles);
            return res;
        }

        //add
        public static List<int> pointIds = new List<int>();
        public static string addPdx(string point, string inDSM, int lev)
        {
            //读取点ID
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource pointDs = dr.Open(point, 0);
            OSGeo.OGR.Layer pointLayer = pointDs.GetLayerByIndex(0);
            long pointCount = pointLayer.GetFeatureCount(0);
            for (int i = 0; i < pointCount; i++)
            {
                pointIds.Add(i);
            }

            //提升分辨率，计算坡度图 
            List<string> subSlop = new List<string>();

            string upDataDSM = StaticTools.tempFilePath("img", "UpDataDSM");
            CutImg.ImgReProject(inDSM, upDataDSM, 2);
            string addSolpeMap = StaticTools.tempFilePath("img", "addSolpeMap");
            SlopeDem.Instance.Sloping(upDataDSM, addSolpeMap);

            //循环坡度下限值
            for (int i = 0; i < lev; i++)
            {
                //没点了就退出
                if (pointIds.Count == 0)
                {
                    Console.WriteLine("所有目标都已找到匹配元素！");
                    break;
                }
                Console.WriteLine("【开始第{0}次循环，还有{1}个点待提取！】", i, pointIds.Count);
                string _outShpPath = StaticTools.tempFilePath("shp", "SlopAdd" + i.ToString() + "Times");
                GetOutlines.Instance.MyGetOutlines(upDataDSM, addSolpeMap, _outShpPath, 80 - i * 5);
                补充要素.AddFeatuer.selectFromPoint(point, _outShpPath);
                subSlop.Add(_outShpPath);
            }
            Console.WriteLine("还有{0}个目标没有找到对应要素!", pointIds.Count.ToString());

            return OutLine.CutImg.zhengHePdx(subSlop, 10);
        }
    }
}
