using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度线
{

    class CutImg
    {
        #region 用户定义
        public static int userSetX = 1500;
        public static int userSetY = 1500;
        static int xBuf;
        static int yBuf;
        #endregion
        /// <summary>
        /// 切分图像||
        /// 解决图像过大导致的计算缓慢||
        /// 输入图像路径，输出分幅路径||
        /// 重叠区根据图像分辨率确定，约为实地100~200米
        /// </summary>
        /// <param name="inPaht"></param>
        /// <returns></returns>
        public static List<string> getSubImg(string inPaht)
        {
            //注册插件
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.GDAL.Driver gdalDriver = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            //读进数据
            OSGeo.GDAL.Dataset inDS = OSGeo.GDAL.Gdal.Open(inPaht, OSGeo.GDAL.Access.GA_ReadOnly);

            //根据栅格分辨率确定重叠区大小
            double[] dsTraansform = new double[6];
            inDS.GetGeoTransform(dsTraansform);
            if (dsTraansform[1] < 0.16)
            { xBuf = 1000; yBuf = 1000; }//1000*0.1=>100M
            if (dsTraansform[1] < 0.3)
            { xBuf = 500; yBuf = 500; }//500*0.25=>125M
            else if (dsTraansform[1] < 0.6)
            { xBuf = 300; yBuf = 300; }//300*0.5=>150M
            else if (dsTraansform[1] < 1.1)
            { xBuf = 200; yBuf = 150; }//150*1=>200M
            else if (dsTraansform[1] < 2.1)
            { xBuf = 100; yBuf = 100; }//100*2=>200M
            else
            { xBuf = 50; yBuf = 50; }//50*5=>250M


            //获取数据XY相元数量
            int oriXcount = inDS.RasterXSize;
            int oriYcount = inDS.RasterYSize;

            //用来返回的文件路径列表
            List<string> imgFilePaths = new List<string>();

            if (oriXcount > userSetX || oriYcount > userSetY)
            {
                //确定文件行列数
                int u, v;
                u = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(oriXcount) / userSetX));//行文件数
                v = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(oriYcount) / userSetY));//列文件数

                //循环列
                for (int i = 0; i < v; i++)
                {
                    //循环行
                    for (int j = 0; j < u; j++)
                    {
                        ////////////  定义起点  /////////////
                        int offX = j * userSetX;
                        int offY = i * userSetY;

                        //////////////定义边缘栅格大小/////////////////
                        int thinkJ = userSetX * (j + 1) + xBuf;
                        int thinkI = userSetY * (i + 1) + yBuf;
                        int subXcount = oriXcount - thinkJ > 0 ? userSetX + xBuf : oriXcount - userSetX * j;
                        int subYcount = oriYcount - thinkI > 0 ? userSetY + yBuf : oriYcount - userSetY * i;

                        ////////////  把原栅格读进内存，内容为oriValue /////////////
                        OSGeo.GDAL.Band oriBand = inDS.GetRasterBand(1);
                        double[] oriValue = new double[subXcount * subYcount];

                        oriBand.ReadRaster
                            (
                            offX,//起点X索引
                            offY,//起点Y索引
                            subXcount,//X方向相元数量
                            subYcount,//Y方向相元数量
                            oriValue,//各相元值
                            subXcount,//执行读入的X方向数量
                            subYcount,//执行读入的Y方向数量
                            0,//执行读入的起点X索引
                            0//执行读入的起点Y索引
                            );

                        ////////////  创建子栅格 /////////////
                        string imgFilePath = StaticTools.tempFilePath("img", i.ToString() + "_" + j.ToString());
                        OSGeo.GDAL.Dataset subDs = gdalDriver.Create
                            (
                            imgFilePath,
                            subXcount,
                            subYcount,
                            1,
                            OSGeo.GDAL.DataType.GDT_Float32,
                            null
                            );
                        subDs.SetProjection(inDS.GetProjectionRef());

                        //获取数据Transfrom
                        double[] oriTransFrom = new double[6];
                        inDS.GetGeoTransform(oriTransFrom);
                        oriTransFrom[0] = oriTransFrom[0] + offX * oriTransFrom[1] + offY * oriTransFrom[2];
                        oriTransFrom[3] = oriTransFrom[3] + offX * oriTransFrom[4] + offY * oriTransFrom[5];
                        subDs.SetGeoTransform(oriTransFrom);
                        ////////////  把值写入子栅格 /////////////
                        subDs.GetRasterBand(1).WriteRaster
                            (
                            0,
                            0,
                            subXcount,
                            subYcount,
                            oriValue,
                            subXcount,
                            subYcount,
                            0,
                            0
                            );

                        ///////////////// 返回子栅格路径 ////////////////////
                        imgFilePaths.Add(imgFilePath);
                        subDs.Dispose();

                    }
                }
            }
            else
            {
                imgFilePaths.Add(inPaht);
            }
            inDS.Dispose();
            return imgFilePaths;
        }




        /// <summary>
        /// 栅格重建，srPath输入，dsPath输出，1为1=>1, 2为1=>4, 3为1=>9,
        /// </summary>
        /// <param name="srPath"></param>
        /// <param name="dsPath"></param>
        /// <param name="level"></param>
        public static void ImgReProject(string srPath, string dsPath, int level = 2)
        {
            Console.WriteLine("ReprojectImage开始，lever 2");
            double[] geoTran = new double[6];
            double[] geoTran2 = new double[6];
            //读
            OSGeo.GDAL.Gdal.AllRegister();
            Dataset ds = Gdal.Open(srPath, Access.GA_ReadOnly);
            ds.GetGeoTransform(geoTran);
            geoTran2 = geoTran;
            geoTran2[1] = geoTran[1] / level;
            geoTran2[5] = geoTran[5] / level;
            //建
            OSGeo.GDAL.Driver gdalDriver = Gdal.GetDriverByName("HFA");
            Dataset ods = gdalDriver.Create(dsPath, ds.RasterXSize * level, ds.RasterYSize * level, 1, DataType.GDT_Float32, null);
            ods.SetProjection(ds.GetProjection());
            ods.SetGeoTransform(geoTran2);
            ods.GetRasterBand(1).SetNoDataValue(-999999);
            //写
            Gdal.ReprojectImage(ds, ods, null, null, ResampleAlg.GRA_NearestNeighbour, 0, 0.02, null, null);
            ds.Dispose();
            ods.Dispose();
            Console.WriteLine("ReprojectImage完成");
        }
        /// <summary>
        /// 整合坡度线（Line）
        /// </summary>
        /// <param name="subPdx"></param>
        /// <returns></returns>
        public static string zhengHePdLine(List<string> subPdx)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");

            string newPath = StaticTools.tempFilePath("shp", "整合线");
            OSGeo.OGR.DataSource newDs = dr.CreateDataSource(newPath, null);/////////////////
            OSGeo.OGR.Layer newLayer = newDs.CreateLayer("Lines",
                dr.Open(subPdx[0], 0).GetLayerByIndex(0).GetSpatialRef(),
                dr.Open(subPdx[0], 0).GetLayerByIndex(0).GetGeomType(), null);

            for (int i = 0; i < subPdx.Count; i++)
            {
                OSGeo.OGR.DataSource dsLine = dr.Open(subPdx[i], 0);///////////////////////
                OSGeo.OGR.Layer lineLayer = dsLine.GetLayerByIndex(0);
                for (int j = 0; j < lineLayer.GetFeatureCount(0); j++)
                {
                    newLayer.CreateFeature(lineLayer.GetFeature(j));
                }
                dsLine.Dispose();
            }
            newDs.Dispose();
            return newPath;
        }
        /// <summary>
        /// 整合坡度多边形(Polygon),并清除重叠部份
        /// </summary>
        /// <param name="subPdx"></param>
        /// <returns></returns>
        public static string zhengHePdx(List<string> subPdx)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            string res = StaticTools.tempFilePath("shp", "整合");
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(res, null);//////////////待关闭1  has
            Layer newLayer = newDS.CreateLayer("podu", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);//////////////待关闭2

            for (int i = 0; i < subPdx.Count; i++)
            {
                OSGeo.OGR.DataSource ds = dr.Open(subPdx[i], 0);//////////////待关闭3   has
                Layer layer = ds.GetLayerByIndex(0);
                for (int j = 0; j < layer.GetFeatureCount(0); j++)
                {
                    OSGeo.OGR.Feature subFeat = layer.GetFeature(j);
                    OSGeo.OGR.Geometry subGeom = subFeat.GetGeometryRef();
                    double thisArea = subGeom.GetArea();
                    // 清理过小Featuer
                    if (thisArea > 200)
                    {
                        newLayer.CreateFeature(layer.GetFeature(j));
                    }
                }
                ds.Dispose();
                Console.WriteLine("完成整合{0}/{1}", i + 1, subPdx.Count);
            }
            newDS.Dispose();
            cleanPdx(res);
            return res;
        }
        /// <summary>
        /// 清理重复的Featuer----------单线程
        /// </summary>
        /// <param name="pdx"></param>
        public static void cleanPdx(string pdx)
        {
            Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            DataSource ds = dr.Open(pdx, 1);
            Layer newLayer = ds.GetLayerByIndex(0);
            int 重复的 = 0;
            for (int i = 0; i < newLayer.GetFeatureCount(0) - 1; i++)
            {
                Feature ori = newLayer.GetFeature(i);
                for (int j = i + 1; j < newLayer.GetFeatureCount(0); j++)
                {
                    Feature next = newLayer.GetFeature(j);
                    bool a = StaticTools.isSame(ori, next, 1);
                    if (a)
                    {
                        newLayer.DeleteFeature(i);
                        重复的++;
                        Console.WriteLine("已删除{0}个重复Featuer,allFeat is {1}/{2}", 重复的, i + 1, newLayer.GetFeatureCount(0));
                        break;
                    }
                    next.Dispose();
                }
                ori.Dispose();
            }
            string layerName = newLayer.GetName();
            ds.ExecuteSQL("REPACK " + layerName, null, "");
            ds.Dispose();
        }



        static List<int> ids = new List<int>();
        static int tickTime;
        /// <summary>
        /// 清理重复的Featuer----------多线程----与单线程效率差别很小，不推荐
        /// </summary>
        /// <param name="filePath"></param>
        public static void cleanPdxM(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(filePath, 1);
            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
            int featCount = layer.GetFeatureCount(0);

            var sw = new System.Diagnostics.Stopwatch(); sw.Start();
            for (int i = 0; i < featCount - 1; i++)
            {
                getDoubFeat(filePath, i, featCount);
            }
            while (tickTime != featCount - 1)
            {
                Thread.Sleep(2000);
                Console.WriteLine(tickTime.ToString() + " /" + featCount.ToString()+"/ deletePoint "+ids.Count().ToString());
            }
            sw.Stop(); Console.WriteLine("多线程清理重复Featuer用时：" + sw.Elapsed.ToString());

            for (int i = 0; i < ids.Count; i++)
            {
                layer.DeleteFeature(ids[i]);
            }
            string a = "REPACK " + layer.GetName();
            ds.ExecuteSQL(a, null, "");
            ds.Dispose();
        }

        async static void getDoubFeat(string filePath, int s, int ss)
        {
            await Task.Run(() =>
            {
                OSGeo.OGR.Ogr.RegisterAll();
                OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
                OSGeo.OGR.DataSource ds = dr.Open(filePath, 0);
                OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
                OSGeo.OGR.Feature oriFeat = layer.GetFeature(s);
                for (int i = s + 1; i < ss; i++)
                {
                    OSGeo.OGR.Feature nextFeat = layer.GetFeature(i);
                    if (StaticTools.isSame(oriFeat, nextFeat, 1))
                    {
                        ids.Add(s);
                        break;
                    }
                    nextFeat.Dispose();
                }
                oriFeat.Dispose();
                layer.Dispose();
                ds.Dispose();
                tickTime++;
            }
            );
        }

    }

    //Xp = GeoTransform[0] + P * GeoTransform[1] + L * GeoTransform[2];
    //Yp = GeoTransform[3] + P * GeoTransform[4] + L * GeoTransform[5];	
    /*
    class FloodSimulation
    {
        #region 类成员变量

        public OSGeo.GDAL.Dataset m_DEMDataSet;	    //DEM数据集
        public OSGeo.GDAL.Dataset m_FloodSimulatedDataSet; //洪涝淹没范围数据集
        public int m_XSize;		     //数据X方向栅格个数
        public int m_YSize;		     //数据Y方向栅格个数
        //public double m_AreaFlooded;	    //水面面积
        //public double m_WaterVolume;	    //淹没水体体积
        public double[] m_FloodBuffer;	  //填充缓冲区（洪涝淹没范围）
        public double[] m_DEMdataBuffer;	   //DEM数据缓冲

         //这里的GeoTransform(影像坐标变换参数)的定义是：通过像素所在的行列值得到其左上角点空间坐标的运算参数
         //   例如:某图像上(P,L)点左上角的实际空间坐标为：
         //   Xp = GeoTransform[0] + P * GeoTransform[1] + L * GeoTransform[2];
         //   Yp = GeoTransform[3] + P * GeoTransform[4] + L * GeoTransform[5];								     
        public double[] m_adfGeoTransform;

        #endregion

        //构造函数
        public FloodSimulation()
        {
            m_adfGeoTransform = new double[6];
        }

        /// <summary>
        /// 加载淹没区DEM,并创建淹没范围影像
        /// </summary>
        /// <param name="m_DEMFilePath">DEM文件路径</param>
        /// <returns></returns>
        public void loadDataSet(string m_DEMFilePath)
        {
            m_DEMDataSet = OSGeo.GDAL.Gdal.Open(m_DEMFilePath, OSGeo.GDAL.Access.GA_ReadOnly);
            //获取X、Y方向栅格数
            m_XSize = m_DEMDataSet.RasterXSize;
            m_YSize = m_DEMDataSet.RasterYSize;

            //读取DEM数据到内存中
            OSGeo.GDAL.Band m_DEMBand = m_DEMDataSet.GetRasterBand(1); //获取第一个波段
            m_DEMdataBuffer = new double[m_XSize * m_YSize];
            m_DEMBand.ReadRaster
                (
                0,
                0,
                m_XSize,  //数据X方向栅格个数
                m_YSize, //数据Y方向栅格个数
                m_DEMdataBuffer, //DEM数据缓冲
                m_XSize, //数据X方向栅格个数
                m_YSize, //数据X方向栅格个数
                0,
                0
                );

            //淹没范围填充缓冲区
            m_FloodBuffer = new double[m_XSize * m_YSize];
            //获取影像坐标转换参数
            m_DEMDataSet.GetGeoTransform(m_adfGeoTransform);

            //创建洪涝淹没范围影像
            string m_FloodImagePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\FloodSimulation\\FloodedRegion.tif";
            if (System.IO.File.Exists(m_FloodImagePath))
            {
                System.IO.File.Delete(m_FloodImagePath);
            }
            //在GDAL中创建影像,先需要明确待创建影像的格式,并获取到该影像格式的驱动
            OSGeo.GDAL.Driver driver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff");
            //调用Creat函数创建影像
            m_FloodSimulatedDataSet = driver.Create
                (
                m_FloodImagePath, //存储路径
                m_XSize, //数据X方向栅格个数
                m_YSize, //数据Y方向栅格个数
                1, //Band 数
                OSGeo.GDAL.DataType.GDT_CFloat32, //数据类型
                null
                );
            //设置影像属性
            m_FloodSimulatedDataSet.SetGeoTransform(m_adfGeoTransform); //影像转换参数
            m_FloodSimulatedDataSet.SetProjection(m_DEMDataSet.GetProjection()); //投影
            //将影像数据写入内存
            m_FloodSimulatedDataSet.GetRasterBand(1).WriteRaster
                (
                0,//左上X
                0, //左上Y
                m_XSize,//数据X方向栅格个数
                m_YSize,//数据Y方向栅格个数
                m_FloodBuffer, //填充缓冲区
                m_XSize,
                m_YSize,
                0,
                0
                );
            m_FloodSimulatedDataSet.GetRasterBand(1).FlushCache();
            m_FloodSimulatedDataSet.FlushCache();

        }

        /// <summary>
        /// 从像素空间转换到地理空间
        /// </summary>
        /// <param name="adfGeoTransform">影像坐标变换参数</param>
        /// <param name="pixel">像素所在行</param>
        /// <param name="line">像素所在列</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void imageToGeoSpace(double[] m_GeoTransform, int pixel, int line, out double X, out double Y)
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
        public void geoToImageSpace(double[] m_GeoTransform, double x, double y, out int pixel, out int line)
        {
            line = (int)((y * m_GeoTransform[1] - x * m_GeoTransform[4] + m_GeoTransform[0] * m_GeoTransform[4] - m_GeoTransform[3] * m_GeoTransform[1]) / (m_GeoTransform[5] * m_GeoTransform[1] - m_GeoTransform[2] * m_GeoTransform[4]));
            pixel = (int)((x - m_GeoTransform[0] - line * m_GeoTransform[2]) / m_GeoTransform[1]);
        }
    }*/
}
