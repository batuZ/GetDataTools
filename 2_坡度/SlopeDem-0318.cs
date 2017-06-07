using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度
{
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
    public class SlopeDem:IDisposable
    {
        //private static SlopeDem _slopeDemObj;
        private Dataset _inDataset;
        private Dataset _outDataset;
        private string _outDSpath;
        private double[] _geoTransform;
        private double _noDataValue;
        private int _hasVal = 0;
        private bool[,] _beVisited;
        private bool[,] _BeVisited;//记录当前已被访问的范围（即是面图层的范围）
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


        private Dictionary<int, OrderPoint> _myRelaDic;

        public SlopeDem()
        {
            //注册Gdal
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();
            //初始化dictionary
        }

        public void Slope(string InFilePath, string OutFilePath)
        {
            this.OpenDataSet(InFilePath);
            _geoTransform = new double[6];

            if (System.IO.File.Exists(OutFilePath))
                System.IO.File.Delete(OutFilePath);
            //调用GDal创建影像，声明影像格式
            OSGeo.GDAL.Driver gdalDriver = Gdal.GetDriverByName("HFA");
            _outDataset = gdalDriver.Create(OutFilePath, _inDataset.RasterXSize, _inDataset.RasterYSize, 1, DataType.GDT_Float32, null);
            _outDataset.SetProjection(_inDataset.GetProjection());
            //设置NodataValue
            double nodataVal;
            int hasVal;
            _inDataset.GetRasterBand(1).GetNoDataValue(out nodataVal, out hasVal);
            if (hasVal == 0)
            {
                _inDataset.GetRasterBand(1).SetNoDataValue(-100000);
                _inDataset.GetRasterBand(1).FlushCache();
                _inDataset.FlushCache();
                _inDataset.Dispose();
                this.OpenDataSet(InFilePath);
            }
            //设置仿射参数
            _inDataset.GetGeoTransform(_geoTransform);
            _outDataset.SetGeoTransform(_geoTransform);

            Sloping(_inDataset, _outDataset);

            _outDataset.FlushCache();
            _outDataset.Dispose();
            _inDataset.Dispose();
            _outDSpath = OutFilePath;

        }

        private Dataset OpenDataSet(string FilePath)
        {
            if (FilePath == null)
            {
                throw new ArgumentNullException("FilePath");
            }
            _inDataset = Gdal.Open(FilePath, Access.GA_Update);
            _inDataset.GetRasterBand(1).GetNoDataValue(out _noDataValue, out _hasVal);
            return _inDataset;
        }

        private double GetSlopeVal(double[] ReadBuffer, BlockType MyBlkType)
        {
            double[] tempArray = new double[9];
            switch (MyBlkType)
            {
                #region
                case BlockType.Angular_LeftTop:
                    {
                        if (_hasVal != 0 && ReadBuffer[0] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[0];
                        tempArray[1] = ReadBuffer[0];
                        tempArray[2] = ReadBuffer[0];
                        tempArray[3] = ReadBuffer[0];
                        tempArray[4] = ReadBuffer[0];
                        tempArray[5] = ReadBuffer[1];
                        tempArray[6] = ReadBuffer[0];
                        tempArray[7] = ReadBuffer[2];
                        tempArray[8] = ReadBuffer[3];
                        break;
                    }
                case BlockType.Angular_LeftBot:
                    {
                        if (_hasVal != 0 && ReadBuffer[2] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[2];
                        tempArray[1] = ReadBuffer[0];
                        tempArray[2] = ReadBuffer[1];
                        tempArray[3] = ReadBuffer[2];
                        tempArray[4] = ReadBuffer[2];
                        tempArray[5] = ReadBuffer[3];
                        tempArray[6] = ReadBuffer[2];
                        tempArray[7] = ReadBuffer[2];
                        tempArray[8] = ReadBuffer[2];
                        break;
                    }
                case BlockType.Angular_RightBot:
                    {
                        if (_hasVal != 0 && ReadBuffer[3] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[0];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[3];
                        tempArray[3] = ReadBuffer[2];
                        tempArray[4] = ReadBuffer[3];
                        tempArray[5] = ReadBuffer[3];
                        tempArray[6] = ReadBuffer[3];
                        tempArray[7] = ReadBuffer[3];
                        tempArray[8] = ReadBuffer[3];
                        break;
                    }
                case BlockType.Angular_RightTop:
                    {
                        if (_hasVal != 0 && ReadBuffer[1] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[1];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[1];
                        tempArray[3] = ReadBuffer[0];
                        tempArray[4] = ReadBuffer[1];
                        tempArray[5] = ReadBuffer[1];
                        tempArray[6] = ReadBuffer[2];
                        tempArray[7] = ReadBuffer[3];
                        tempArray[8] = ReadBuffer[1];
                        break;
                    }
                case BlockType.Edge_Bot:
                    {
                        if (_hasVal != 0 && ReadBuffer[4] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[0];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[2];
                        tempArray[3] = ReadBuffer[3];
                        tempArray[4] = ReadBuffer[4];
                        tempArray[5] = ReadBuffer[5];
                        tempArray[6] = ReadBuffer[4];
                        tempArray[7] = ReadBuffer[4];
                        tempArray[8] = ReadBuffer[4];
                        break;
                    }
                case BlockType.Edge_Left:
                    {
                        if (_hasVal != 0 && ReadBuffer[2] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[2];
                        tempArray[1] = ReadBuffer[0];
                        tempArray[2] = ReadBuffer[1];
                        tempArray[3] = ReadBuffer[2];
                        tempArray[4] = ReadBuffer[2];
                        tempArray[5] = ReadBuffer[3];
                        tempArray[6] = ReadBuffer[2];
                        tempArray[7] = ReadBuffer[4];
                        tempArray[8] = ReadBuffer[5];
                        break;
                    }
                case BlockType.Edge_Right:
                    {
                        if (_hasVal != 0 && ReadBuffer[3] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[0];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[3];
                        tempArray[3] = ReadBuffer[2];
                        tempArray[4] = ReadBuffer[3];
                        tempArray[5] = ReadBuffer[3];
                        tempArray[6] = ReadBuffer[4];
                        tempArray[7] = ReadBuffer[5];
                        tempArray[8] = ReadBuffer[3];
                        break;
                    }
                case BlockType.Edge_Top:
                    {
                        if (_hasVal != 0 && ReadBuffer[1] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[1];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[1];
                        tempArray[3] = ReadBuffer[0];
                        tempArray[4] = ReadBuffer[1];
                        tempArray[5] = ReadBuffer[2];
                        tempArray[6] = ReadBuffer[3];
                        tempArray[7] = ReadBuffer[4];
                        tempArray[8] = ReadBuffer[5];
                        break;
                    }
                case BlockType.Center:
                    {
                        if (_hasVal != 0 && ReadBuffer[4] == _noDataValue)
                            return _noDataValue;

                        tempArray[0] = ReadBuffer[0];
                        tempArray[1] = ReadBuffer[1];
                        tempArray[2] = ReadBuffer[2];
                        tempArray[3] = ReadBuffer[3];
                        tempArray[4] = ReadBuffer[4];
                        tempArray[5] = ReadBuffer[5];
                        tempArray[6] = ReadBuffer[6];
                        tempArray[7] = ReadBuffer[7];
                        tempArray[8] = ReadBuffer[8];
                        break;
                    }
                #endregion
            }
            return this.CalcuSlope(tempArray);
        }
        /// <summary>
        /// 根据3X3方格计算坡度
        /// </summary>
        /// <param name="InArray"></param>
        /// <returns></returns>
        private double CalcuSlope(double[] InArray)
        {
            if (InArray.Length != 9)
                throw new RankException("InArray");
            double[] _transformArray = new double[6];
            _inDataset.GetGeoTransform(_transformArray);
            double left = InArray[0] + 2 * InArray[3] + InArray[6];
            double right = InArray[2] + 2 * InArray[5] + InArray[8];
            double bot = InArray[6] + 2 * InArray[7] + InArray[8];
            double top = InArray[0] + 2 * InArray[1] + InArray[2];

            double _weSlp = (right - left) / (8 * _transformArray[1]);
            double _snSlp = (bot - top) / (8 * _transformArray[5]);

            return 180 / Math.PI * Math.Atan(Math.Sqrt(_weSlp * _weSlp + _snSlp * _snSlp));
        }

        private void Sloping(Dataset InDataset, Dataset OutDataset)
        {
            #region
            double _tempValue = 0;
            Band _band = InDataset.GetRasterBand(1);
            for (int y = 1; y < InDataset.RasterYSize - 1; y++)
            {
                for (int x = 1; x < InDataset.RasterXSize - 1; x++)
                {
                    double[] _rBuffer = new double[9];

                    _band.ReadRaster(x - 1, y - 1, 3, 3, _rBuffer, 3, 3, 0, 0);
                    _tempValue = _rBuffer[4];
                    _rBuffer = SetBufferVal(_tempValue, _rBuffer);

                    double _val = GetSlopeVal(_rBuffer, BlockType.Center);
                    double[] _valArray = new double[] { _val };
                    OutDataset.GetRasterBand(1).WriteRaster(x, y, 1, 1, _valArray, 1, 1, 0, 0);
                }
                OutDataset.GetRasterBand(1).FlushCache();
            }

            int _y = 0;
            for (int x = 1; x < InDataset.RasterXSize - 1; x++)
            {
                double[] _rBuffer = new double[6];
                _band.ReadRaster(x - 1, _y, 3, 2, _rBuffer, 3, 2, 0, 0);
                _tempValue = _rBuffer[1];
                _rBuffer = SetBufferVal(_tempValue, _rBuffer);

                double _val = GetSlopeVal(_rBuffer, BlockType.Edge_Top);
                double[] _valArray = new double[] { _val };
                OutDataset.GetRasterBand(1).WriteRaster(x, _y, 1, 1, _valArray, 1, 1, 0, 0);
                OutDataset.GetRasterBand(1).FlushCache();
            }

            _y = InDataset.RasterYSize - 1;
            for (int x = 1; x < InDataset.RasterXSize - 1; x++)
            {
                double[] _rBuffer = new double[6];
                _band.ReadRaster(x - 1, _y - 1, 3, 2, _rBuffer, 3, 2, 0, 0);
                _tempValue = _rBuffer[4];
                _rBuffer = SetBufferVal(_tempValue, _rBuffer);

                double _val = GetSlopeVal(_rBuffer, BlockType.Edge_Bot);
                double[] _valArray = new double[] { _val };
                OutDataset.GetRasterBand(1).WriteRaster(x, _y, 1, 1, _valArray, 1, 1, 0, 0);
                OutDataset.GetRasterBand(1).FlushCache();
            }

            int _x = 0;
            for (int y = 1; y < InDataset.RasterYSize - 1; y++)
            {
                double[] _rBuffer = new double[6];
                _band.ReadRaster(_x, y - 1, 2, 3, _rBuffer, 2, 3, 0, 0);
                _tempValue = _rBuffer[2];
                _rBuffer = SetBufferVal(_tempValue, _rBuffer);

                double _val = GetSlopeVal(_rBuffer, BlockType.Edge_Left);
                double[] _valArray = new double[] { _val };
                OutDataset.GetRasterBand(1).WriteRaster(_x, y, 1, 1, _valArray, 1, 1, 0, 0);
                OutDataset.GetRasterBand(1).FlushCache();

            }

            _x = InDataset.RasterXSize - 1;
            for (int y = 1; y < InDataset.RasterYSize - 1; y++)
            {
                double[] _rBuffer = new double[6];
                _band.ReadRaster(_x - 1, y - 1, 2, 3, _rBuffer, 2, 3, 0, 0);
                _tempValue = _rBuffer[3];
                _rBuffer = SetBufferVal(_tempValue, _rBuffer);

                double _val = GetSlopeVal(_rBuffer, BlockType.Edge_Right);
                double[] _valArray = new double[] { _val };
                OutDataset.GetRasterBand(1).WriteRaster(_x, y, 1, 1, _valArray, 1, 1, 0, 0);
                OutDataset.GetRasterBand(1).FlushCache();
            }

            double[] _mRbuffer = new double[4];
            _band.ReadRaster(0, 0, 2, 2, _mRbuffer, 2, 2, 0, 0);
            _tempValue = _mRbuffer[0];
            _mRbuffer = SetBufferVal(_tempValue, _mRbuffer);

            double _mval = GetSlopeVal(_mRbuffer, BlockType.Angular_LeftTop);
            double[] _mvalArray = new double[] { _mval };
            OutDataset.GetRasterBand(1).WriteRaster(0, 0, 1, 1, _mvalArray, 1, 1, 0, 0);
            OutDataset.GetRasterBand(1).FlushCache();

            _mRbuffer = new double[4];
            _band.ReadRaster(InDataset.RasterXSize - 2, 0, 2, 2, _mRbuffer, 2, 2, 0, 0);
            _tempValue = _mRbuffer[1];
            _mRbuffer = SetBufferVal(_tempValue, _mRbuffer);

            _mval = GetSlopeVal(_mRbuffer, BlockType.Angular_RightTop);
            _mvalArray = new double[] { _mval };
            OutDataset.GetRasterBand(1).WriteRaster(InDataset.RasterXSize - 1, 0, 1, 1, _mvalArray, 1, 1, 0, 0);
            OutDataset.GetRasterBand(1).FlushCache();

            _mRbuffer = new double[4];
            _band.ReadRaster(InDataset.RasterXSize - 2, InDataset.RasterYSize - 2, 2, 2, _mRbuffer, 2, 2, 0, 0);
            _tempValue = _mRbuffer[3];
            _mRbuffer = SetBufferVal(_tempValue, _mRbuffer);

            _mval = GetSlopeVal(_mRbuffer, BlockType.Angular_RightBot);
            _mvalArray = new double[] { _mval };
            OutDataset.GetRasterBand(1).WriteRaster(InDataset.RasterXSize - 1, InDataset.RasterYSize - 1, 1, 1, _mvalArray, 1, 1, 0, 0);
            OutDataset.GetRasterBand(1).FlushCache();

            _mRbuffer = new double[4];
            _band.ReadRaster(0, InDataset.RasterYSize - 2, 2, 2, _mRbuffer, 2, 2, 0, 0);
            _tempValue = _mRbuffer[2];
            _mRbuffer = SetBufferVal(_tempValue, _mRbuffer);

            _mval = GetSlopeVal(_mRbuffer, BlockType.Angular_LeftBot);
            _mvalArray = new double[] { _mval };
            OutDataset.GetRasterBand(1).WriteRaster(0, InDataset.RasterYSize - 1, 1, 1, _mvalArray, 1, 1, 0, 0);
            OutDataset.GetRasterBand(1).FlushCache();

            OutDataset.FlushCache();
            #endregion
        }

        private double[] SetBufferVal(double Val, double[] BufferArray)
        {
            double[] _resArr = new double[BufferArray.Length];
            if (_hasVal != 0)
            {
                for (int i = 0; i < BufferArray.Length; i++)
                {
                    if (BufferArray[i] == _noDataValue)
                        _resArr[i] = Val;
                    else
                        _resArr[i] = BufferArray[i];
                }
            }
            return _resArr;
        }

        /***************************以下部分提取轮廓线**********************************/
        double _importLevel ;
        double _importLevel_limit = 90;

        private void InitDictionary()
        {
            _myRelaDic = new Dictionary<int, OrderPoint>();
            _myRelaDic.Add(0, new OrderPoint(-1, -1));
            _myRelaDic.Add(1, new OrderPoint(0, -1));
            _myRelaDic.Add(2, new OrderPoint(1, -1));
            _myRelaDic.Add(3, new OrderPoint(1, 0));
            _myRelaDic.Add(4, new OrderPoint(1, 1));
            _myRelaDic.Add(5, new OrderPoint(0, 1));
            _myRelaDic.Add(6, new OrderPoint(-1, 1));
            _myRelaDic.Add(7, new OrderPoint(-1, 0));
        }
        Queue<int> resQue = new Queue<int>();

        public void CreatePolygon(string inDataPath ,string slopeDataPath,string OutShpPath,string _linePath,double importLvl)
        {
            _outDSpath = slopeDataPath;
            _importLevel = importLvl;
            polygonShp = new GeneralShp(OutShpPath, Path.GetFileNameWithoutExtension(OutShpPath), wkbGeometryType.wkbPolygon);
            polylineShp = new GeneralShp(_linePath, Path.GetFileNameWithoutExtension(_linePath), wkbGeometryType.wkbLineString);

            resQue.Enqueue(1);
            _inDataset = Gdal.Open(inDataPath, Access.GA_ReadOnly);
            _outDataset = Gdal.Open(_outDSpath, Access.GA_ReadOnly);
            _BeVisited = new bool[_outDataset.RasterXSize, _outDataset.RasterYSize];
            _geoTransform = new double[6];
            _inDataset.GetGeoTransform(_geoTransform);

            int yleng = _outDataset.RasterYSize;
            int cntY = yleng / 300;

            if (cntY == 0)
            {
                 CP_Prc(inDataPath, OutShpPath, 0, yleng);
            }
            else{
                for (int i = 0; i <= cntY; i++){
                    if (i == cntY)
                    {
                        CP_Prc(inDataPath, OutShpPath, cntY * 300, yleng);
                    }
                    else
                    {
                        CP_Prc(inDataPath, OutShpPath, i * 300, (i + 1) * 300);
                    }
                }
            }
            Console.WriteLine("完成");
            polygonShp.Close();
            polylineShp.Close();
        }
        GeneralShp polygonShp;
        GeneralShp polylineShp;

        private void CP_Prc(string inDataPath, string OutShpPath,int ys,int ye)
        {
            
            _inDataset = Gdal.Open(inDataPath, Access.GA_ReadOnly);
            _outDataset = Gdal.Open(_outDSpath, Access.GA_ReadOnly);
            //打开一个Ds
            string _shpPath = System.IO.Path.GetDirectoryName(OutShpPath);
            string _shpName = System.IO.Path.GetFileName(OutShpPath);

            //_beVisited = new bool[_outDataset.RasterXSize, _outDataset.RasterYSize];
            
            double[] _rowBuffer = new double[_outDataset.RasterXSize];
            List<OrderPoint> _resPntList;
            List<OrderPoint> _resPntList_unClock;
            for (int _j = ys; _j<ye&&_j < _outDataset.RasterYSize; _j++)
            {
                for (int _i = 0; _i < _outDataset.RasterXSize; _i++)
                {
                    if (IsImportPnt(_outDataset, _i, _j, _importLevel) && !IsBeVisited(_BeVisited, _i, _j))
                    //if (IsImportPnt(_outDataset, _i, _j, _importLevel))
                    {
                        _resPntList = new List<OrderPoint>();
                        _resPntList_unClock = new List<OrderPoint>();

                        OrderPoint _orStartPnt = GetStartPnt(_inDataset, _outDataset, _i, _j);
                        OrderPoint _orSecondPnt = GetSecondPnt2(_inDataset, _outDataset, ref _orStartPnt);
                        if (_orStartPnt != null && _orSecondPnt != null)
                        {
                            _beVisited = new bool[_outDataset.RasterXSize, _outDataset.RasterYSize];
                            //UpdateVisitedBook(_orStartPnt.X, _orStartPnt.Y, 1, 1);
                            //UpdateVisitedBook(_orSecondPnt.X, _orSecondPnt.Y, 1, 1);
                            int _relaType = ((int)_orStartPnt.Direct + 4) & 7;

                            _resPntList = GetOnePolygon(_orStartPnt, _orSecondPnt);

                            _orSecondPnt.Direct = (Toward)_relaType;
                            _resPntList_unClock = GetOnePolygon_UnClock(_orSecondPnt, _orStartPnt);

                            double _disOf2Pnt = GetPntsDistance(_resPntList[_resPntList.Count - 1], _resPntList_unClock[_resPntList_unClock.Count - 1]);
                            if (_disOf2Pnt < Math.Sqrt(51) && (_resPntList.Count + _resPntList_unClock.Count) > 50)
                            //if ((_resPntList.Count + _resPntList_unClock.Count) > 8)
                            {
                                /****************************************/
                                //新建一个polygon对象
                                Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                                foreach (OrderPoint _myPoint in _resPntList_unClock)
                                {
                                    double _x = _geoTransform[0] + (_myPoint.X + 0.5) * _geoTransform[1] + (_myPoint.Y + 0.5) * _geoTransform[2];
                                    double _y = _geoTransform[3] + (_myPoint.X + 0.5) * _geoTransform[4] + (_myPoint.Y + 0.5) * _geoTransform[5];
                                    ring.AddPoint(_x, _y, 0);
                                }
                                for (int i = (_resPntList.Count - 1); i > 0; i--)
                                {
                                    double _x = _geoTransform[0] + (_resPntList[i].X + 0.5) * _geoTransform[1] + (_resPntList[i].Y + 0.5) * _geoTransform[2];
                                    double _y = _geoTransform[3] + (_resPntList[i].X + 0.5) * _geoTransform[4] + (_resPntList[i].Y + 0.5) * _geoTransform[5];
                                    ring.AddPoint(_x, _y, 0);
                                }
                                double xt = _geoTransform[0] + (_orSecondPnt.X + 0.5) * _geoTransform[1] + (_orSecondPnt.Y + 0.5) * _geoTransform[2];
                                double yt = _geoTransform[3] + (_orSecondPnt.X + 0.5) * _geoTransform[4] + (_orSecondPnt.Y + 0.5) * _geoTransform[5];
                                ring.AddPoint(xt, yt, 0);
                                xt = _geoTransform[0] + (_orStartPnt.X + 0.5) * _geoTransform[1] + (_orStartPnt.Y + 0.5) * _geoTransform[2];
                                yt = _geoTransform[3] + (_orStartPnt.X + 0.5) * _geoTransform[4] + (_orStartPnt.Y + 0.5) * _geoTransform[5];
                                ring.AddPoint(xt, yt, 0);

                                polygonShp.InsertFeature(ring);
                                /****************************************/
                            }
                            else if ((_resPntList.Count + _resPntList_unClock.Count) > 40)
                            {
                                /****************************************/
                                //新建一个polyline对象
                                Geometry line = new Geometry(wkbGeometryType.wkbLineString);
                                for (int i = _resPntList.Count - 1; i > 0; i--)
                                {
                                    double _x = _geoTransform[0] + (_resPntList[i].X + 0.5) * _geoTransform[1] + (_resPntList[i].Y + 0.5) * _geoTransform[2];
                                    double _y = _geoTransform[3] + (_resPntList[i].X + 0.5) * _geoTransform[4] + (_resPntList[i].Y + 0.5) * _geoTransform[5];
                                    line.AddPoint(_x, _y, 0);
                                }
                                foreach (OrderPoint _myPoint in _resPntList_unClock)
                                {
                                    double _x = _geoTransform[0] + (_myPoint.X + 0.5) * _geoTransform[1] + (_myPoint.Y + 0.5) * _geoTransform[2];
                                    double _y = _geoTransform[3] + (_myPoint.X + 0.5) * _geoTransform[4] + (_myPoint.Y + 0.5) * _geoTransform[5];
                                    line.AddPoint(_x, _y, 0);
                                }

                                polylineShp.InsertFeature(line);
                                /****************************************/
                            }
                            //将改点设置为已被访问过
                            lock(_BeVisited)
                            {
                                Copy2DArrayValue(_BeVisited, _beVisited);
                            }
                        }
                    }
                }
            }
            //polygonShp.Close();
            //polylineShp.Close();
            _inDataset.Dispose();
            _outDataset.Dispose();
        }
        private double _sumOfMolecularVal;//GetOnePolygon1112过程中用来求得轮廓线平均像素值的 分子
        private int _numOfdenominator;//GetOnePolygon1112过程中用来求得轮廓线平均像素值的 分母

        private List<OrderPoint> GetOnePolygon(OrderPoint _stPnt, OrderPoint _ndPnt)
        {
            List<OrderPoint> _resPntList = new List<OrderPoint>();
            _resPntList.Add(_ndPnt);

            if ((int)GetBlockType(_outDataset, _ndPnt.X, _ndPnt.Y) != 9)
                return _resPntList;

            OrderPoint _rdPnt = GetNextPnt(_stPnt, _ndPnt);//更新于2015-11-20 18:38:13

            if (_rdPnt == null || _beVisited[_rdPnt.X, _rdPnt.Y])//更新（||_beVisited[_rdPnt.X, _rdPnt.Y]）于2015-11-10 10:23:24
            {

                return _resPntList;
            }
            else
            {
                _resPntList.AddRange(GetOnePolygon(_ndPnt, _rdPnt));
            }
            return _resPntList;
        }

        private List<OrderPoint> GetOnePolygon1112(OrderPoint _stPnt, OrderPoint _ndPnt)//更新于2015-11-12 10:53:22
        {

            List<OrderPoint> _resPntList = new List<OrderPoint>();
            _resPntList.Add(_ndPnt);
            _sumOfMolecularVal += GetPixelValOnDataset(_inDataset, _ndPnt.X, _ndPnt.Y);
            _numOfdenominator += 1;


            if ((int)GetBlockType(_outDataset, _ndPnt.X, _ndPnt.Y) != 9)
                return _resPntList;

            OrderPoint _rdPnt = GetNextPnt1112(_stPnt, _ndPnt, _sumOfMolecularVal / _numOfdenominator);
            if (_rdPnt == null || _beVisited[_rdPnt.X, _rdPnt.Y])//更新（||_beVisited[_rdPnt.X, _rdPnt.Y]）于2015-11-10 10:23:24
            {
                return _resPntList;
            }
            else
            {
                _resPntList.AddRange(GetOnePolygon1112(_ndPnt, _rdPnt));
            }
            return _resPntList;
        }

        private List<OrderPoint> GetOnePolygon1113(OrderPoint _stPnt, OrderPoint _ndPnt)//更新于2015-11-13 9:17:08
        {
            List<OrderPoint> _resPntList = new List<OrderPoint>();
            _resPntList.Add(_ndPnt);

            if ((int)GetBlockType(_outDataset, _ndPnt.X, _ndPnt.Y) != 9)
                return _resPntList;

            OrderPoint _rdPnt = GetNextPnt(_stPnt, _ndPnt);

            if (_rdPnt == null || _beVisited[_rdPnt.X, _rdPnt.Y])//更新（||_beVisited[_rdPnt.X, _rdPnt.Y]）于2015-11-10 10:23:24
            {

                return _resPntList;
            }
            else
            {
                _resPntList.AddRange(GetOnePolygon(_ndPnt, _rdPnt));
            }
            return _resPntList;
        }

        private List<OrderPoint> GetOnePolygon_UnClock(OrderPoint _stPnt, OrderPoint _ndPnt)
        {
            List<OrderPoint> _resPntList = new List<OrderPoint>();
            _resPntList.Add(_ndPnt);

            if ((int)GetBlockType(_outDataset, _ndPnt.X, _ndPnt.Y) != 9)
                return _resPntList;

            OrderPoint _rdPnt = GetNextPnt_UnClock(_stPnt, _ndPnt);

            if (_rdPnt == null || _beVisited[_rdPnt.X, _rdPnt.Y])//更新（||_beVisited[_rdPnt.X, _rdPnt.Y]）于2015-11-10 10:23:24
            {
                return _resPntList;
            }
            else
            {
                _resPntList.AddRange(GetOnePolygon_UnClock(_ndPnt, _rdPnt));
            }
            return _resPntList;
        }

        private List<OrderPoint> GetOnePolygon_UnClock1112(OrderPoint _stPnt, OrderPoint _ndPnt)//更新于2015-11-11 11:50:48
        {
            List<OrderPoint> _resPntList = new List<OrderPoint>();
            _resPntList.Add(_ndPnt);
            _sumOfMolecularVal += GetPixelValOnDataset(_inDataset, _ndPnt.X, _ndPnt.Y);
            _numOfdenominator += 1;

            if ((int)GetBlockType(_outDataset, _ndPnt.X, _ndPnt.Y) != 9)
                return _resPntList;

            OrderPoint _rdPnt = GetNextPnt_UnClock1112(_stPnt, _ndPnt, _sumOfMolecularVal / _numOfdenominator);

            if (_rdPnt == null || _beVisited[_rdPnt.X, _rdPnt.Y])//更新（||_beVisited[_rdPnt.X, _rdPnt.Y]）于2015-11-10 10:23:24
            {
                return _resPntList;
            }
            else
            {
                _resPntList.AddRange(GetOnePolygon_UnClock1112(_ndPnt, _rdPnt));
            }
            return _resPntList;
        }

        /// <summary>
        /// 获取两个OrderPoint之间的距离
        /// </summary>
        /// <param name="Point1"></param>
        /// <param name="Point2"></param>
        /// <returns></returns>
        private double GetPntsDistance(OrderPoint Point1, OrderPoint Point2)
        {
            double _dis = 0;
            _dis = Math.Sqrt((Point1.X - Point2.X) * (Point1.X - Point2.X) + (Point1.Y - Point2.Y) * (Point1.Y - Point2.Y));
            return _dis;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="SlopeDs"></param>
        /// <param name="X">从0开始</param>
        /// <param name="Y">从0开始</param>
        /// <param name="ImportLevel"></param>
        /// <returns></returns>
        private bool IsImportPnt(Dataset SlopeDs, int X, int Y, double ImportLevel)
        {
            double[] _rVal = new double[1];
            SlopeDs.GetRasterBand(1).ReadRaster(X, Y, 1, 1, _rVal, 1, 1, 0, 0);
            if (_rVal[0] >= ImportLevel)
                return true;
            else
                return false;
        }

        private bool IsImportPnt1112(Dataset SlopeDs, int X, int Y, double ImportLevel, double LevelLimit = 90)
        {
            double[] _rVal = new double[1];
            SlopeDs.GetRasterBand(1).ReadRaster(X, Y, 1, 1, _rVal, 1, 1, 0, 0);
            if (_rVal[0] >= ImportLevel && _rVal[0] <= LevelLimit)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 获取像素块儿的BlockType
        /// </summary>
        /// <param name="SlopeDs"></param>
        /// <param name="X">列号/从0开始</param>
        /// <param name="Y">行号/从0开始</param>
        /// <returns></returns>
        private BlockType GetBlockType(Dataset SlopeDs, int X, int Y)
        {
            int _Ds_x = SlopeDs.RasterXSize;
            int _Ds_y = SlopeDs.RasterYSize;
            if (X == 0)
            {
                if (Y == 0)
                    return BlockType.Angular_LeftTop;
                else if (Y == (_Ds_y - 1))
                    return BlockType.Angular_LeftBot;
                else
                    return BlockType.Edge_Left;
            }
            else if (X == (_Ds_x - 1))
            {
                if (Y == 0)
                    return BlockType.Angular_RightTop;
                else if (Y == (_Ds_y - 1))
                    return BlockType.Angular_RightBot;
                else
                    return BlockType.Edge_Right;
            }
            else if (Y == 0)
                return BlockType.Edge_Top;
            else if (Y == (_Ds_y - 1))
                return BlockType.Edge_Bot;
            else
                return BlockType.Center;
        }
        /// <summary>
        /// 获取Dataset.Band（1）上点(X,Y)处的栅格值
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private double GetPixelValOnDataset(Dataset dataset, int X, int Y)
        {
            double[] _resArray = new double[1];
            dataset.GetRasterBand(1).ReadRaster(X, Y, 1, 1, _resArray, 1, 1, 0, 0);
            return _resArray[0];
        }

        private OrderPoint GetStartPnt(Dataset GrayDs, Dataset SlopeDs, int X, int Y)
        {
            double _tempReadVal = -10000;
            BlockType _blockType;
            double[] _nbhVal;//GrayDs邻域的值 
            double[] _nbhVal_Slp;//SlopeDs邻域的值
            OrderPoint _resPnt = new OrderPoint(X, Y);

            _blockType = GetBlockType(GrayDs, X, Y);
            _nbhVal = GetNBHval(GrayDs, X, Y, _blockType);
            _nbhVal_Slp = GetNBHval(SlopeDs, X, Y, _blockType);

            #region
            for (int i = 0; i < _nbhVal_Slp.Length; i++)
            {
                if (_blockType == BlockType.Angular_LeftTop)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X + i % 2;
                        _resPnt.Y = Y + i / 2;
                    }
                    //_beVisited[X + i % 2, Y + i / 2] = true;
                }
                else if (_blockType == BlockType.Angular_LeftBot)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X + i % 2;
                        _resPnt.Y = Y - 1 + i / 2;
                    }
                    //_beVisited[X + i % 2, Y -1 + i / 2] = true;
                }
                else if (_blockType == BlockType.Angular_RightTop)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 2;
                        _resPnt.Y = Y + i / 2;
                    }
                    //_beVisited[X -1 + i % 2, Y + i / 2] = true;
                }
                else if (_blockType == BlockType.Angular_RightBot)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 2;
                        _resPnt.Y = Y + i / 2;
                    }
                    //_beVisited[X - 1 + i % 2, Y -1 + i / 2] = true;
                }

                else if (_blockType == BlockType.Edge_Left)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X + i % 2;
                        _resPnt.Y = Y - 1 + i / 2;
                    }
                    //_beVisited[X + i % 2, Y -1 + i / 2] = true;
                }
                else if (_blockType == BlockType.Edge_Right)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 2;
                        _resPnt.Y = Y - 1 + i / 2;
                    }
                    //_beVisited[X - 1 + i % 2, Y - 1 + i / 2] = true;
                }
                else if (_blockType == BlockType.Edge_Top)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 3;
                        _resPnt.Y = Y + i / 3;
                    }
                    //_beVisited[X - 1 + i % 3, Y  + i / 3] = true;
                }
                else if (_blockType == BlockType.Edge_Bot)
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 3;
                        _resPnt.Y = Y - 1 + i / 3;
                    }
                    //_beVisited[X - 1 + i % 3, Y - 1 + i / 3] = true;
                }
                else
                {
                    if (_nbhVal_Slp[i] >= _importLevel && _nbhVal[i] > _tempReadVal)
                    {
                        _resPnt.X = X - 1 + i % 3;
                        _resPnt.Y = Y - 1 + i / 3;
                    }
                    //_beVisited[X - 1 + i % 3, Y - 1 + i / 3] = true;
                }
            }
            #endregion

            //if (_beVisited[_resPnt.X, _resPnt.Y])
            //    return null;
            //else
            //{
            //    //更新Visited表
            //    _beVisited[_resPnt.X, _resPnt.Y] = true;
            //    return _resPnt;
            //}
            return _resPnt;
        }

        private OrderPoint GetSecondPnt(Dataset GrayDs, Dataset SlopeDs, ref OrderPoint FirstPnt)
        {
            double _tempReadVal = -10000;
            BlockType _blockType;
            double[] _nbhVal;//GrayDs邻域的值 
            double[] _nbhVal_Slp;//SlopeDs邻域的值
            double[] _clkNbhVal;
            double[] _clkNbhVal_Slp;
            OrderPoint _resPnt = null;

            _blockType = GetBlockType(GrayDs, FirstPnt.X, FirstPnt.Y);
            _nbhVal = GetNBHval(GrayDs, FirstPnt.X, FirstPnt.Y, _blockType);
            _nbhVal_Slp = GetNBHval(SlopeDs, FirstPnt.X, FirstPnt.Y, _blockType);

            #region BlockType.Angular_LeftTop
            if (_blockType == BlockType.Angular_LeftTop)
            {
                if (_nbhVal_Slp[3] >= _importLevel && _nbhVal[3] <= _nbhVal[2])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X;
                    _resPnt.Y = FirstPnt.Y + 1;
                    FirstPnt.Direct = Toward.Bot;
                }
                else if (_nbhVal_Slp[1] >= _importLevel && _nbhVal[1] <= _nbhVal[2])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X + 1;
                    _resPnt.Y = FirstPnt.Y;
                    FirstPnt.Direct = Toward.Right;
                }
                else if (_nbhVal_Slp[2] >= _importLevel && _nbhVal[3] <= _nbhVal[1])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X + 1;
                    _resPnt.Y = FirstPnt.Y + 1;
                    FirstPnt.Direct = Toward.Right_bot;
                }
            }
            #endregion

            #region BlockType.Angular_LeftBot
            if (_blockType == BlockType.Angular_LeftBot)
            {
                if (_nbhVal_Slp[2] >= _importLevel && _nbhVal[2] <= _nbhVal[1])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X + 1;
                    _resPnt.Y = FirstPnt.Y;
                    FirstPnt.Direct = Toward.Right;
                }
                else if (_nbhVal_Slp[0] >= _importLevel && _nbhVal[0] <= _nbhVal[1])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X;
                    _resPnt.Y = FirstPnt.Y + 1;
                    FirstPnt.Direct = Toward.Top;
                }
                else if (_nbhVal_Slp[1] >= _importLevel && _nbhVal[2] <= _nbhVal[0])
                {
                    _resPnt = new OrderPoint();
                    _resPnt.X = FirstPnt.X + 1;
                    _resPnt.Y = FirstPnt.Y - 1;
                    FirstPnt.Direct = Toward.Right_top;
                }
            }
            #endregion

            //if (_blockType == BlockType.Angular_LeftTop)
            //{
            //    for (int i = 3; i < 3 + _nbhVal_Slp.Length - 1; i++)
            //    {
            //        OrderPoint _relativePnt = GetCR_byIndex(i, _blockType);

            //    }
            //}

            //更新Visited表
            _beVisited[_resPnt.X, _resPnt.Y] = true;
            return _resPnt;
        }

        /// <summary>
        ///  
        /// </summary>
        /// 更新于2015-11-8 16:19:57
        /// <param name="GrayDs"></param>
        /// <param name="SlopeDs"></param>
        /// <param name="FirstPnt"></param>
        /// <returns>返回的结果可能为null</returns>
        private OrderPoint GetSecondPnt2(Dataset GrayDs, Dataset SlopeDs, ref OrderPoint FirstPnt)
        {
            if (FirstPnt == null)
                return null;

            double tempValue = -100000;
            OrderPoint _resPnt = null;
            BlockType _blkType;
            double[] _nbhVal;//GrayDs邻域的值 
            double[] _nbhVal_Slp;//SlopeDs邻域的值

            _blkType = GetBlockType(SlopeDs, FirstPnt.X, FirstPnt.Y);

            if (_blkType == BlockType.Center)
            {
                _nbhVal = GetNBHval(GrayDs, FirstPnt.X, FirstPnt.Y, _blkType);
                _nbhVal_Slp = GetNBHval(SlopeDs, FirstPnt.X, FirstPnt.Y, _blkType);

                _nbhVal = Clockwise_Nbhd(_nbhVal, _blkType);
                _nbhVal_Slp = Clockwise_Nbhd(_nbhVal_Slp, _blkType);

                for (int i = 0; i < 8; i++)
                {
                    if (_nbhVal[i] > tempValue && _nbhVal_Slp[i] >= _importLevel
                        && _nbhVal[(i - 1) & 7] > _nbhVal[(i + 1) & 7])
                    {
                        _resPnt = ReflectMyPoint(i, FirstPnt);
                        FirstPnt.Direct = (Toward)i;
                    }
                }
                //UpdateVisitedBook(FirstPnt.X, FirstPnt.Y, 1, 1);
            }

            return _resPnt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt(OrderPoint AheadPnt, OrderPoint MidPnt)
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = (int)AheadPnt.Direct - 1;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) - 2) & 7;
                time = 5;
            }
            double tempSlope = -1;
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal + i) & 7;
                if (eight_Nbhd_Slp[_index] > _importLevel && eight_Nbhd_Slp[_index] > tempSlope && eight_Nbhd_Val[(_index-1 ) & 7] > eight_Nbhd_Val[(_index + 1) & 7])
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    tempSlope = eight_Nbhd_Slp[_index];
                }
            }

            if (_beVisited[nextPnt.X, nextPnt.Y]||tempSlope<0)
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt1120(OrderPoint AheadPnt, OrderPoint MidPnt)
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = (int)AheadPnt.Direct - 1;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) - 2) & 7;
                time = 5;
            }
            //double tempSlope = 0;
            double difValue = 1000;
            double midValue = GetPixelValOnDataset(_inDataset, MidPnt.X, MidPnt.Y);
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal + i) & 7;
                double _difvalue = Math.Abs(eight_Nbhd_Val[_index] - midValue);
                if (_difvalue < difValue && eight_Nbhd_Val[(_index - 1) & 7] > eight_Nbhd_Val[(_index + 1) & 7])
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    //tempSlope = eight_Nbhd_Slp[_index];
                    difValue = _difvalue;
                }
            }



            if (_beVisited[nextPnt.X, nextPnt.Y])
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt1112(OrderPoint AheadPnt, OrderPoint MidPnt, double AverageVal)//更新于2015-11-12 10:57:47
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = (int)AheadPnt.Direct - 1;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) - 2) & 7;
                time = 5;
            }

            double tempSlope = 0;
            double _tempVal = 10000;
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal + i) & 7;
                double _δVal = Math.Abs(eight_Nbhd_Val[_index] - AverageVal);
                if (_δVal < _tempVal)
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    _tempVal = _δVal;
                }
            }

            bool _existPnt = false;//判断是否存在nextPnt
            if (_tempVal > 6)
            {
                for (int i = 0; i < time; i++)
                {
                    int _index = (startVal + i) & 7;
                    if (eight_Nbhd_Slp[_index] > tempSlope && eight_Nbhd_Val[(_index - 1) & 7] > eight_Nbhd_Val[(_index + 1) & 7])
                    {
                        nextPnt = this.ReflectMyPoint(_index, MidPnt);
                        MidPnt.Direct = (Toward)_index;
                        tempSlope = eight_Nbhd_Slp[_index];
                        _existPnt = true;
                    }
                }
            }
            else
                _existPnt = true;

            if (!_existPnt && _beVisited[nextPnt.X, nextPnt.Y])
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        /// <summary>
        /// 逆时针获取NextPnt
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt_UnClock(OrderPoint AheadPnt, OrderPoint MidPnt)
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = ((int)AheadPnt.Direct + 1) & 7;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) + 2) & 7;
                time = 5;
            }
            double tempSlope = 0;
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal - i) & 7;
                if (eight_Nbhd_Slp[_index] > tempSlope && eight_Nbhd_Val[(_index - 1) & 7] < eight_Nbhd_Val[(_index + 1) & 7])
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    tempSlope = eight_Nbhd_Slp[_index];
                }
            }

            if (_beVisited[nextPnt.X, nextPnt.Y])
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        /// <summary>
        /// 逆时针获取NextPnt
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt_UnClock1120(OrderPoint AheadPnt, OrderPoint MidPnt)
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = ((int)AheadPnt.Direct + 1) & 7;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) + 2) & 7;
                time = 5;
            }
            //double tempSlope = 0;
            double difValue = 1000;
            double midValue = GetPixelValOnDataset(_inDataset, MidPnt.X, MidPnt.Y);
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal - i) & 7;
                double _difvalue = Math.Abs(eight_Nbhd_Val[_index] - midValue);
                if (_difvalue < difValue && eight_Nbhd_Val[(_index - 1) & 7] < eight_Nbhd_Val[(_index + 1) & 7])
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    difValue = _difvalue;
                }
            }

            if (_beVisited[nextPnt.X, nextPnt.Y])
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        /// <summary>
        /// 逆时针获取NextPnt
        /// </summary>
        /// 同步更新Visited表
        /// <param name="AheadPnt"></param>
        /// <param name="MidPnt"></param>
        /// <returns></returns>
        private OrderPoint GetNextPnt_UnClock1112(OrderPoint AheadPnt, OrderPoint MidPnt, double AverageVal)//更新于2015-11-12 12:14:33
        {
            double[] eight_Nbhd_Val = new double[8];
            double[] eight_Nbhd_Slp = new double[8];
            double[] readValBuffer = new double[9];//以MidPnt为中心点读取（栅格像素图）九邻域
            double[] readSlpBuffer = new double[9];//以MidPnt为中心点读取（slope图）九邻域
            OrderPoint nextPnt = new OrderPoint();
            this._outDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readSlpBuffer, 3, 3, 0, 0);
            this._inDataset.GetRasterBand(1).ReadRaster(MidPnt.X - 1, MidPnt.Y - 1, 3, 3, readValBuffer, 3, 3, 0, 0);

            eight_Nbhd_Slp = this.Clockwise_Nbhd(readSlpBuffer, (BlockType)9);
            eight_Nbhd_Val = this.Clockwise_Nbhd(readValBuffer, (BlockType)9);

            int startVal;
            int time;
            if ((int)(AheadPnt.Direct) % 2 == 1)
            {
                startVal = ((int)AheadPnt.Direct + 1) & 7;
                time = 3;
            }
            else
            {
                startVal = ((int)(AheadPnt.Direct) + 2) & 7;
                time = 5;
            }

            double tempSlope = 0;
            double _tempVal = 10000;
            for (int i = 0; i < time; i++)
            {
                int _index = (startVal - i) & 7;
                double _δVal = Math.Abs(eight_Nbhd_Val[_index] - AverageVal);
                if (_δVal < _tempVal)
                {
                    nextPnt = this.ReflectMyPoint(_index, MidPnt);
                    MidPnt.Direct = (Toward)_index;
                    _tempVal = _δVal;
                }
            }


            bool _existPnt = false;//判断是否存在nextPnt
            if (_tempVal > 6)
            {
                for (int i = 0; i < time; i++)
                {
                    int _index = (startVal - i) & 7;
                    if (eight_Nbhd_Slp[_index] > tempSlope && eight_Nbhd_Val[(_index - 1) & 7] < eight_Nbhd_Val[(_index + 1) & 7])
                    {
                        nextPnt = this.ReflectMyPoint(_index, MidPnt);
                        MidPnt.Direct = (Toward)_index;
                        tempSlope = eight_Nbhd_Slp[_index];
                        _existPnt = true;
                    }
                }
            }
            else
                _existPnt = true;

            if (!_existPnt || _beVisited[nextPnt.X, nextPnt.Y])
                return null;
            //更新Visited表
            UpdateVisitedBook(MidPnt.X, MidPnt.Y, 1, 1);
            return nextPnt;
        }

        private double[] Clockwise_Nbhd(double[] NbhdArray, BlockType Type)
        {
            double[] _clockWiseArray;
            if (Type == BlockType.Angular_LeftTop || Type == BlockType.Angular_LeftBot ||
                Type == BlockType.Angular_LeftBot || Type == BlockType.Angular_LeftBot)
            {
                _clockWiseArray = new double[4];
                _clockWiseArray[0] = NbhdArray[0];
                _clockWiseArray[1] = NbhdArray[1];
                _clockWiseArray[2] = NbhdArray[3];
                _clockWiseArray[3] = NbhdArray[2];
            }

            else if (Type == BlockType.Edge_Left || Type == BlockType.Edge_Right)
            {
                _clockWiseArray = new double[6];
                _clockWiseArray[0] = NbhdArray[0];
                _clockWiseArray[1] = NbhdArray[1];
                _clockWiseArray[2] = NbhdArray[3];
                _clockWiseArray[3] = NbhdArray[5];
                _clockWiseArray[4] = NbhdArray[4];
                _clockWiseArray[5] = NbhdArray[2];
            }
            else if (Type == BlockType.Edge_Left || Type == BlockType.Edge_Right)
            {
                _clockWiseArray = new double[6];
                _clockWiseArray[0] = NbhdArray[0];
                _clockWiseArray[1] = NbhdArray[1];
                _clockWiseArray[2] = NbhdArray[2];
                _clockWiseArray[3] = NbhdArray[5];
                _clockWiseArray[4] = NbhdArray[4];
                _clockWiseArray[5] = NbhdArray[3];
            }
            else
            {
                _clockWiseArray = new double[8];
                _clockWiseArray[0] = NbhdArray[0];
                _clockWiseArray[1] = NbhdArray[1];
                _clockWiseArray[2] = NbhdArray[2];
                _clockWiseArray[3] = NbhdArray[5];
                _clockWiseArray[4] = NbhdArray[8];
                _clockWiseArray[5] = NbhdArray[7];
                _clockWiseArray[6] = NbhdArray[6];
                _clockWiseArray[7] = NbhdArray[3];
            }

            return _clockWiseArray;
        }

        private OrderPoint ReflectMyPoint(int value, OrderPoint centerPnt)
        {
            OrderPoint myPnt = new OrderPoint();
            switch (value)
            {
                case 0:
                    myPnt.X = centerPnt.X - 1;
                    myPnt.Y = centerPnt.Y - 1;
                    return myPnt;
                case 1:
                    myPnt.X = centerPnt.X;
                    myPnt.Y = centerPnt.Y - 1;
                    return myPnt;
                case 2:
                    myPnt.X = centerPnt.X + 1;
                    myPnt.Y = centerPnt.Y - 1;
                    return myPnt;
                case 3:
                    myPnt.X = centerPnt.X + 1;
                    myPnt.Y = centerPnt.Y;
                    return myPnt;
                case 4:
                    myPnt.X = centerPnt.X + 1;
                    myPnt.Y = centerPnt.Y + 1;
                    return myPnt;
                case 5:
                    myPnt.X = centerPnt.X;
                    myPnt.Y = centerPnt.Y + 1;
                    return myPnt;
                case 6:
                    myPnt.X = centerPnt.X - 1;
                    myPnt.Y = centerPnt.Y + 1;
                    return myPnt;
                case 7:
                    myPnt.X = centerPnt.X - 1;
                    myPnt.Y = centerPnt.Y;
                    return myPnt;
                default:
                    return myPnt;
            }
        }
        /// <summary>
        /// 更新Visited表
        /// </summary>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        private void UpdateVisitedBook(int StartX, int StartY, int X, int Y)
        {
            for (int j = StartY; j < StartY + Y; j++)
            {
                for (int i = StartX; i < StartX + X; i++)
                {
                    _beVisited[i, j] = true;
                }
            }
        }
        /// <summary>
        /// 判断点（X，Y）是否在CmpDataset数据范围内
        /// </summary>
        /// <param name="CmpDataset"></param>
        /// <param name="X">列数X从0开始</param>
        /// <param name="Y">行数Y从0开始</param>
        /// <returns></returns>
        private bool PntInDsFrame(Dataset CmpDataset, int X, int Y)
        {
            if (X < CmpDataset.RasterXSize && X >= 0 && Y < CmpDataset.RasterYSize && Y >= 0)
                return true;
            else
                return false;
        }

        private List<OrderPoint> GetSelfCircle(List<OrderPoint> Inlist)
        {
            List<OrderPoint> _resList;
            OrderPoint _pnt1;
            OrderPoint _pnt2;
            if (Inlist.Count < 10)
                return null;
            for (int i = 0; i < Inlist.Count; i++)
            {
                _pnt1 = Inlist[i];
                for (int j = i + 5; j < Inlist.Count; j++)
                {
                    _pnt2 = Inlist[j];
                    if (DistanceOfPnts(_pnt1, _pnt2) < Math.Sqrt(2.005))
                    {
                        _resList = Inlist.GetRange(i, j - i + 1);
                        return _resList;
                    }
                }
            }
            return null;
        }

        private double DistanceOfPnts(OrderPoint Pnt1, OrderPoint Pnt2)
        {
            int _X = Pnt1.X - Pnt2.X;
            int _Y = Pnt1.Y - Pnt2.Y;
            double _dis = Math.Sqrt(_X * _X + _Y * _Y);
            return _dis;
        }

        private OrderPoint GetCR_byIndex(int Index, BlockType BlkType)
        {
            OrderPoint _relativeCood = new OrderPoint();
            if (BlkType == BlockType.Angular_LeftBot || BlkType == BlockType.Angular_LeftTop || BlkType == BlockType.Angular_RightBot
                || BlkType == BlockType.Angular_RightTop || BlkType == BlockType.Edge_Left || BlkType == BlockType.Edge_Right)
            {
                _relativeCood.X = Index % 2;
                _relativeCood.Y = Index / 2;
            }
            else
            {
                _relativeCood.X = Index % 3;
                _relativeCood.Y = Index / 3;
            }
            return _relativeCood;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OpDataset"></param>
        /// <param name="X">列号/从0开始</param>
        /// <param name="Y">行号/从0开始</param>
        /// <param name="MyBlockType"></param>
        /// <returns></returns>
        private double[] GetNBHval(Dataset OpDataset, int X, int Y, BlockType MyBlockType)
        {
            double[] _resArray;
            if (MyBlockType == BlockType.Angular_LeftTop)
            {
                _resArray = new double[4];
                OpDataset.GetRasterBand(1).ReadRaster(X, Y, 2, 2, _resArray, 2, 2, 0, 0);
            }
            else if (MyBlockType == BlockType.Angular_LeftBot)
            {
                _resArray = new double[4];
                OpDataset.GetRasterBand(1).ReadRaster(X, Y - 1, 2, 2, _resArray, 2, 2, 0, 0);
            }
            else if (MyBlockType == BlockType.Angular_RightTop)
            {
                _resArray = new double[4];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y, 2, 2, _resArray, 2, 2, 0, 0);
            }
            else if (MyBlockType == BlockType.Angular_RightBot)
            {
                _resArray = new double[4];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y - 1, 2, 2, _resArray, 2, 2, 0, 0);
            }
            else if (MyBlockType == BlockType.Edge_Left)
            {
                _resArray = new double[6];
                OpDataset.GetRasterBand(1).ReadRaster(X, Y - 1, 2, 3, _resArray, 2, 3, 0, 0);
            }
            else if (MyBlockType == BlockType.Edge_Right)
            {
                _resArray = new double[6];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y - 1, 2, 3, _resArray, 2, 3, 0, 0);
            }
            else if (MyBlockType == BlockType.Edge_Top)
            {
                _resArray = new double[6];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y, 3, 2, _resArray, 3, 2, 0, 0);
            }
            else if (MyBlockType == BlockType.Edge_Bot)
            {
                _resArray = new double[6];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y - 1, 3, 2, _resArray, 3, 2, 0, 0);
            }
            else
            {
                _resArray = new double[9];
                OpDataset.GetRasterBand(1).ReadRaster(X - 1, Y - 1, 3, 3, _resArray, 3, 3, 0, 0);
            }

            return _resArray;
        }
        /// <summary>
        /// 更新二维数组a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Copy2DArrayValue(bool[,] a, bool[,] b)
        {
            int r = a.GetLength(0);
            int c = a.GetLength(1);
            for (int j = 0; j < c; j++)
            {
                for (int i = 0; i < r; i++)
                {
                    a[i, j] = a[i, j] || b[i, j];
                }
            }
        }

        private bool IsBeVisited(bool[,] a, int x, int y)
        {
            int x_s=x-1,
                x_e=x+1,
                y_s=y-1, 
                y_e=y+1;

            if (x - 1 < 0)
                x_s = 0;
            if (x + 1 > a.GetLength(0)-1)
                x_e = a.GetLength(0)-1;
            if (y - 1 < 0)
                y_s = 0;
            if (y + 1 > a.GetLength(1)-1)
                y_e = a.GetLength(1)-1;
            for (int _j = y_s; _j <= y_e; _j++)
            {
                for (int _i = x_s; _i <= x_e; _i++)
                {
                    if (a[_i, _j])
                        return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
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
        public OrderPoint(int _X,int _Y)
        {
            X = _X;
            Y = _Y;
        }
    }
}
