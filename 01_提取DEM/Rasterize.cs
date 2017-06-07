using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._01_提取DEM
{
    static class RasterizeCls
    {
        static int Tcnt = 0;
        static int IMaxCnt = 0;

        public static void Rasterize(string inputFeature, string outRaster, string fieldName, int cellSize)
        {
            int rasterCellSize = cellSize;

            const double noDataValue = -9999;

            string outputRasterFile = outRaster;

            Ogr.RegisterAll();

            DataSource dataSource = Ogr.Open(inputFeature, 0);
            Layer layer = dataSource.GetLayerByIndex(0);
            Envelope envelope = new Envelope();
            layer.GetExtent(envelope, 0);

            int x_res = Convert.ToInt32((envelope.MaxX - envelope.MinX) / rasterCellSize);
            int y_res = Convert.ToInt32((envelope.MaxY - envelope.MinY) / rasterCellSize);

            Console.WriteLine("Extent: " + envelope.MaxX + " " + envelope.MinX + " " + envelope.MaxY + " " + envelope.MinY);
            Console.WriteLine("X resolution: " + x_res);
            Console.WriteLine("X resolution: " + y_res);

         

            Gdal.AllRegister();
    
            if (File.Exists(outputRasterFile))
            {
                File.Delete(outputRasterFile);
            }
            //Create new tiff   
            OSGeo.GDAL.Driver outputDriver = Gdal.GetDriverByName("GTiff");
            //OSGeo.GDAL.Driver outputDriver = Gdal.GetDriverByName("HFA");
            Dataset outputDataset = outputDriver.Create(outputRasterFile, x_res, y_res, 1, DataType.GDT_Float64, null);
            //Extrac srs from input feature   
            string inputShapeSrs;

            OSGeo.OSR.SpatialReference spatialRefrence = layer.GetSpatialRef();
            spatialRefrence.ExportToWkt(out inputShapeSrs);
            //Assign input feature srs to outpur raster  
            outputDataset.SetProjection(inputShapeSrs);
            //Geotransform  
            double[] argin = new double[] { envelope.MinX, rasterCellSize, 0, envelope.MaxY, 0, -rasterCellSize };
            outputDataset.SetGeoTransform(argin);
            //Set no data  
            Band band = outputDataset.GetRasterBand(1);
            band.SetNoDataValue(noDataValue);
            //close tiff  
            outputDataset.FlushCache();
            outputDataset.Dispose();
            //Feature to raster rasterize layer options  
            //No of bands (1)  
            int[] bandlist = new int[] { 1 };
            //Values to be burn on raster (10.0)  
            double[] burnValues = new double[] { 10.0 };
            Dataset myDataset = Gdal.Open(outputRasterFile, Access.GA_Update);
            //additional options  
            string[] rasterizeOptions;
            //rasterizeOptions = new string[] { "ALL_TOUCHED=TRUE", "ATTRIBUTE=" + fieldName }; //To set all touched pixels into raster pixel  
            //rasterizeOptions = new string[] { "ATTRIBUTE=" + fieldName };
            rasterizeOptions = new string[] { "ATTRIBUTE=" + fieldName };
            //Rasterize layer  
            //Gdal.RasterizeLayer(myDataset, 1, bandlist, layer, IntPtr.Zero, IntPtr.Zero, 1, burnValues, null, null, null); // To burn the given burn values instead of feature attributes  
            //Gdal.RasterizeLayer(myDataset, 1, bandlist, layer, IntPtr.Zero, IntPtr.Zero, 1, burnValues, rasterizeOptions, new Gdal.GDALProgressFuncDelegate(ProgressFunc), "Raster conversion");
            Gdal.RasterizeLayer(myDataset, 1, bandlist, layer, IntPtr.Zero, IntPtr.Zero, 1, burnValues, rasterizeOptions, new Gdal.GDALProgressFuncDelegate(ProgressFunc), "Raster conversion");
        }
        private static int ProgressFunc(double complete, IntPtr message, IntPtr data)
        {
            Console.Write("Processing ... " + complete * 100 + "% Completed.");
            if (message != IntPtr.Zero)
            {
                Console.Write(" Message:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message));
            }
            if (data != IntPtr.Zero)
            {
                Console.Write(" Data:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(data));
            }
            Console.WriteLine("");
            return 1;
        }
        public static void Rasterize(string inputFeature,string referRaster, string outRaster, string fieldName="")
        {
            //Register the raster drivers  
            Gdal.AllRegister();
            //Register the vector drivers  
            Ogr.RegisterAll();
            //Open referRaster
            Dataset _referRaster = Gdal.Open(referRaster,Access.GA_ReadOnly);
            //Get geoTransform Args
            double[] _geoTransform = new double[6];
            _referRaster.GetGeoTransform(_geoTransform);
            // Define pixel_size and NoData value of new raster  
            //int rasterCellSize = cellSize;
            double _xCellSize = _geoTransform[1];
            double _yCellSize = -_geoTransform[5];

            const double noDataValue = -10000;
            string outputRasterFile = outRaster;
            
            //Reading the vector data  
            DataSource dataSource = Ogr.Open(inputFeature, 0);
            Layer layer = dataSource.GetLayerByIndex(0);
            Envelope envelope = new Envelope();
            layer.GetExtent(envelope, 0);
            //Compute the out raster cell resolutions  
            int x_res = _referRaster.RasterXSize;
            int y_res = _referRaster.RasterYSize;
            //Console.WriteLine("Extent: " + envelope.MaxX + " " + envelope.MinX + " " + envelope.MaxY + " " + envelope.MinY);
            //Console.WriteLine("X resolution: " + x_res);
            //Console.WriteLine("X resolution: " + y_res);
            
            //Check if output raster exists & delete (optional)  
            if (File.Exists(outputRasterFile))
            {
                File.Delete(outputRasterFile);
            }
            //Create new tiff   
            DataType dType = _referRaster.GetRasterBand(1).DataType;
            //OSGeo.GDAL.Driver outputDriver = Gdal.GetDriverByName("GTiff");
            OSGeo.GDAL.Driver outputDriver = Gdal.GetDriverByName("HFA");
            
            Dataset outputDataset = outputDriver.Create(outputRasterFile, x_res, y_res, 1, dType, null);
            //Extrac srs from input feature   
            string inputShapeSrs;

            OSGeo.OSR.SpatialReference spatialRefrence = layer.GetSpatialRef();
            if (spatialRefrence != null)
            {
                spatialRefrence.ExportToWkt(out inputShapeSrs);
                //Assign input feature srs to outpur raster  
                outputDataset.SetProjection(inputShapeSrs);
            }
            //Geotransform  
            outputDataset.SetGeoTransform(_geoTransform);
            //Set no data  
            Band band = outputDataset.GetRasterBand(1);
            band.SetNoDataValue(noDataValue);
            //close tiff  
            outputDataset.FlushCache();
            outputDataset.Dispose();
            //Feature to raster rasterize layer options  
            //No of bands (1)  
            int[] bandlist = new int[] { 1 };
            //Values to be burn on raster (10.0)  
            double[] burnValues = new double[] { 10.0 };
            Dataset myDataset = Gdal.Open(outputRasterFile, Access.GA_Update);
            //additional options  
            string[] rasterizeOptions;
            //rasterizeOptions = new string[] { "ALL_TOUCHED=TRUE", "ATTRIBUTE=" + fieldName }; //To set all touched pixels into raster pixel  
            //rasterizeOptions = new string[] { "ATTRIBUTE=" + fieldName };
            rasterizeOptions = new string[] { };
            //Rasterize layer  
            //Gdal.RasterizeLayer(myDataset, 1, bandlist, layer, IntPtr.Zero, IntPtr.Zero, 1, burnValues, null, null, null); // To burn the given burn values instead of feature attributes  
            //Gdal.RasterizeLayer(myDataset, 1, bandlist, layer, IntPtr.Zero, IntPtr.Zero, 1, burnValues, rasterizeOptions, new Gdal.GDALProgressFuncDelegate(ProgressFunc), "Raster conversion");
            Gdal.RasterizeLayer(
                myDataset,//NEW inDS
                1,    //BAND
                bandlist, //int[] bandlist = new int[] { 1 };Band数量
                layer,  // 待转
                IntPtr.Zero,
                IntPtr.Zero,
                1,
                burnValues, //Values to be burn on raster (10.0)  
                rasterizeOptions,
                new Gdal.GDALProgressFuncDelegate(ProgressFunc),
                "Raster conversion"
                );
            myDataset.FlushCache();
            myDataset.Dispose();
        }
        //检查raster的Nodata值，如无设置其NodataValue
        private static void SetNodataValue(string rasterPath, double nodataValue)
        {
            double nodataV;
            int hasVal;
            Dataset rasterDS = Gdal.Open(rasterPath, Access.GA_Update);
            rasterDS.GetRasterBand(1).GetNoDataValue(out nodataV, out hasVal);
            if (hasVal == 0)
                rasterDS.GetRasterBand(1).SetNoDataValue(nodataValue);
            rasterDS.GetRasterBand(1).FlushCache();
            rasterDS.FlushCache();
            rasterDS.Dispose();
        }


        public static async void RasterExtract(string RasterPath, string MaskPath, string shpPath,Action callBack = null)
        {
            SetNodataValue(RasterPath, -99998.0);
            double rasterNodata,maskNodata;
            int rasHasVal,hasVal;

            
            Dataset _rasterDataset = Gdal.Open(RasterPath, Access.GA_Update);
            Dataset _maskDataset = Gdal.Open(MaskPath, Access.GA_Update);
            _rasterDataset.GetRasterBand(1).GetNoDataValue(out rasterNodata,out rasHasVal);
            _maskDataset.GetRasterBand(1).GetNoDataValue(out maskNodata,out hasVal);
            int xsize = _rasterDataset.RasterXSize;
            int ysize = _rasterDataset.RasterYSize;
            IMaxCnt = ysize;
            for (int i = 0; i < ysize; i++)
            {
                await Task.Run(() =>
                {
                    double[] rArray = new double[_rasterDataset.RasterXSize];
                    double[] mArray = new double[_maskDataset.RasterXSize];
                    _rasterDataset.GetRasterBand(1).ReadRaster(0, i, _rasterDataset.RasterXSize, 1, rArray, _rasterDataset.RasterXSize, 1, 0, 0);
                    _maskDataset.GetRasterBand(1).ReadRaster(0, i, _maskDataset.RasterXSize, 1, mArray, _maskDataset.RasterXSize, 1, 0, 0);
                    for (int m = 0; m < mArray.Length; m++)
                    {
                        if (Math.Abs(mArray[m] - maskNodata) > 0.000001)
                            rArray[m] = rasterNodata;
                    }
                    lock (_rasterDataset.GetRasterBand(1))
                    {
                        _rasterDataset.GetRasterBand(1).WriteRaster(0, i, _rasterDataset.RasterXSize, 1, rArray, _rasterDataset.RasterXSize, 1, 0, 0);
                        _rasterDataset.GetRasterBand(1).FlushCache();
                    }
                    Tcnt++;
                }
                      );
                Console.WriteLine("For循环进度： "+" {0}/{1}",i,ysize);
                if (Tcnt == ysize)
                {
                    Console.WriteLine("For is over!");
                    //调用其他方法
                    RasterRepair rr = new RasterRepair(RasterPath, shpPath, callBack);
                }
            }
        }

        public static void PreInit(ref string imgPath, ref string shpPath, out string tempOutRaster, string tempDirPath = @"E:\temp_qt")
        {
            if (!Directory.Exists(tempDirPath))
                Directory.CreateDirectory(tempDirPath);

            string tempImgPath = tempDirPath + "\\" + Path.GetFileName(imgPath);
            if (imgPath != tempImgPath)
            {
                File.Copy(imgPath, tempImgPath, true);
                imgPath = tempImgPath;
            }

            string destshpPath = tempDirPath + "\\" + Path.GetFileNameWithoutExtension(shpPath) + "_buf.shp";
            if (File.Exists(destshpPath))
            {
                string fileName = Path.GetFileName(destshpPath);
                string[] strArr = fileName.Split('.');
                string[] Files = Directory.GetFiles(Path.GetDirectoryName(destshpPath), strArr[0] + ".*");
                foreach (string item in Files)
                {
                    File.Delete(item);
                }
            }
            BufferLayer(shpPath, destshpPath, 0.5);
            shpPath = destshpPath;

            tempOutRaster = tempDirPath + "\\" + "tts.img";//临时数据路径，可任意指定位置
        }
        private static void BufferLayer(string orShpPath, string destShpPath, double BufferDistance)
        {
            //destShpPath = Path.GetDirectoryName(orShpPath)+"\\"+Path.GetFileNameWithoutExtension(orShpPath) + "_buf.shp";
            DataSource dataSource = Ogr.Open(orShpPath, 1);
            Layer layer = dataSource.GetLayerByIndex(0);

            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI Shapefile");
            DataSource destDs = dr.CreateDataSource(Path.GetDirectoryName(destShpPath), null);
            Layer destLayer = destDs.CreateLayer(Path.GetFileNameWithoutExtension(destShpPath), layer.GetSpatialRef(), layer.GetGeomType(), null);
            Feature fea;
            while ((fea = layer.GetNextFeature()) != null)
            {
                Geometry geo = fea.GetGeometryRef();
                geo = geo.Buffer(BufferDistance, 5);
                fea.SetGeometry(geo);
                destLayer.CreateFeature(fea);
            }
            destLayer.Dispose();
            destDs.Dispose();
            layer.Dispose();
            dataSource.Dispose();
        }

  
    }
}
