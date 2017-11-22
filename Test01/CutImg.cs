﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test01
{
    class CutImg
    {
        #region 用户定义
        static int userSetX = 1500;
        static int userSetY = 1500;
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
        /// 改变栅格值
        /// </summary>
        /// <param name="dsm"></param>
        /// <param name="savePath"></param>
        public static void editRasterValue(string dsm, string savePath, double doubles = 2)
        {
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(dsm, OSGeo.GDAL.Access.GA_ReadOnly);
            int xSize = inData.RasterXSize;
            int ySize = inData.RasterYSize;

            OSGeo.GDAL.Driver dr = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            OSGeo.GDAL.Dataset newDs = dr.Create(savePath, xSize, ySize, 1, inData.GetRasterBand(1).DataType, null);
            double[] GeoTrans = new double[6];
            inData.GetGeoTransform(GeoTrans);
            newDs.SetGeoTransform(GeoTrans);

            int userSetX = 1000;
            int userSetY = 1000;
            int pixelCount = xSize / userSetX + 1;
            int lineCount = ySize / userSetY + 1;
            Console.WriteLine("起动单线编辑栅格值，Size为{0}*{1}", xSize, ySize);
            Console.WriteLine("分块为为{0}*{1}，改变值为原来的{2}倍", userSetX, userSetY, doubles);

            for (int iline = 0; iline < lineCount; iline++)
            {
                for (int ipixel = 0; ipixel < pixelCount; ipixel++)
                {
                    int subXsize = userSetX;
                    int subYsize = userSetY;

                    int offx = subXsize * ipixel;
                    int offy = subYsize * iline;

                    if (ipixel == pixelCount - 1)
                    { subXsize = xSize - subXsize * ipixel - 1; }
                    if (iline == lineCount - 1)
                    { subYsize = ySize - subYsize * iline - 1; }

                    double[] rasterValue = new double[subXsize * subYsize];
                    inData.GetRasterBand(1).ReadRaster(offx, offy, subXsize, subYsize, rasterValue, subXsize, subYsize, 0, 0);

                    for (int v = 0; v < rasterValue.Length; v++)
                    {
                        rasterValue[v] = rasterValue[v] * doubles;
                    }
                    newDs.GetRasterBand(1).WriteRaster(offx, offy, subXsize, subYsize, rasterValue, subXsize, subYsize, 0, 0);
                    Console.WriteLine("编辑栅格值，已完成：{0}/{1}", iline * ipixel, lineCount * pixelCount);
                }
            }
            newDs.Dispose();
            inData.Dispose();
        }




        /// <summary>
        /// 栅格重建，srPath输入，dsPath输出，1为1=>1, 2为1=>4, 3为1=>9,
        /// </summary>
        /// <param name="srPath"></param>
        /// <param name="dsPath"></param>
        /// <param name="level"></param>
        public static void ImgReProject(string srPath, string dsPath, int level)
        {
            Console.WriteLine("【ReprojectImage开始，lever {0}】", level.ToString());
            double[] geoTran = new double[6];
            double[] geoTran2 = new double[6];
            //读
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.GDAL.Dataset ds = OSGeo.GDAL.Gdal.Open(srPath, OSGeo.GDAL.Access.GA_ReadOnly);
            ds.GetGeoTransform(geoTran);
            geoTran2 = geoTran;
            geoTran2[1] = geoTran[1] / level;
            geoTran2[5] = geoTran[5] / level;
            //建
            OSGeo.GDAL.Driver gdalDriver = OSGeo.GDAL.Gdal.GetDriverByName("HFA");
            OSGeo.GDAL.Dataset ods = gdalDriver.Create(dsPath, ds.RasterXSize * level, ds.RasterYSize * level, 1, OSGeo.GDAL.DataType.GDT_Float32, null);
            ods.SetProjection(ds.GetProjection());
            ods.SetGeoTransform(geoTran2);
            ods.GetRasterBand(1).SetNoDataValue(-999999);
            //写
            OSGeo.GDAL.Gdal.ReprojectImage(ds, ods, null, null, OSGeo.GDAL.ResampleAlg.GRA_NearestNeighbour, 0, 0.02, null, null);
            ds.Dispose();
            ods.Dispose();
            Console.WriteLine("【ReprojectImage完成！】");
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
        public static string zhengHePdx(List<string> subPdx, double mianJI = 200)
        {
            Console.WriteLine("开始整合所有的坡度线！");
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            string res = StaticTools.tempFilePath("shp", "整合");
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(res, null);
            OSGeo.OGR.Layer newLayer = newDS.CreateLayer("podu", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);

            for (int i = 0; i < subPdx.Count; i++)
            {
                OSGeo.OGR.DataSource ds = dr.Open(subPdx[i], 0);
                OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
                for (int j = 0; j < layer.GetFeatureCount(0); j++)
                {
                    OSGeo.OGR.Feature subFeat = layer.GetFeature(j);
                    OSGeo.OGR.Geometry subGeom = subFeat.GetGeometryRef();
                    double thisArea = subGeom.GetArea();
                    // 清理过小Featuer
                    if (thisArea > mianJI)
                    {
                        newLayer.CreateFeature(layer.GetFeature(j));
                    }
                }
                ds.Dispose();
                Console.WriteLine("完成整合{0}/{1}", i + 1, subPdx.Count);
            }
            newDS.Dispose();
            //清理重复的Featuer
            cleanPdx(res);
            return res;
        }
        /// <summary>
        /// 清理重复的Featuer----------单线程
        /// </summary>
        /// <param name="pdx"></param>
        public static void cleanPdx(string pdx)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(pdx, 1);
            OSGeo.OGR.Layer newLayer = ds.GetLayerByIndex(0);
            int 重复的 = 0;
            for (int i = 0; i < newLayer.GetFeatureCount(0) - 1; i++)
            {
                OSGeo.OGR.Feature ori = newLayer.GetFeature(i);
                for (int j = i + 1; j < newLayer.GetFeatureCount(0); j++)
                {
                    OSGeo.OGR.Feature next = newLayer.GetFeature(j);
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
            Console.WriteLine("开始多线程删除重复内容！");
            var sw = new System.Diagnostics.Stopwatch(); sw.Start();
            //初始化
            tickTime = 0;
            ids.Clear();
            for (int i = 0; i < featCount - 1; i++)
            {
                getDoubFeat(filePath, i, featCount);
            }
            while (tickTime < featCount - 1)
            {
                Thread.Sleep(2000);
                Console.WriteLine("当前执行：" + tickTime.ToString() + "/" + featCount.ToString());
            }
            sw.Stop(); Console.WriteLine("多线程清理重复Featuer用时：" + sw.Elapsed.ToString());

            for (int i = 0; i < ids.Count; i++)
            {
                layer.DeleteFeature(ids[i]);
            }
            //string a = "REPACK " + layer.GetName();
            //ds.ExecuteSQL(a, null, "");
            ds.deleteFeatUpdate();
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
}