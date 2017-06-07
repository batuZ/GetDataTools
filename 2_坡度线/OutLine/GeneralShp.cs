using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataTools._2_坡度线.OutLine
{
    class GeneralShp
    {
        string _outShpPath = "";
        string _shpfileName;
        DataSource oDS;
        public Layer oLayer;
        public string shpPath
        {
            get { return _outShpPath; }
        }
        wkbGeometryType _geoType;
        public GeneralShp(string OutputPath, string shpfileNameWithoutExt, wkbGeometryType type)
        {
            _outShpPath = Path.GetDirectoryName(OutputPath) + "\\" + shpfileNameWithoutExt + ".shp";
            _shpfileName = shpfileNameWithoutExt;
            _geoType = type;
            Ogr.RegisterAll();

            //创建数据，这里以创建ESRI的shp文件为例
            string strDriverName = "ESRI Shapefile";
            int count = Ogr.GetDriverCount();
            Driver oDriver = Ogr.GetDriverByName(strDriverName);
            if (oDriver == null)
            {
                Console.WriteLine("%s 驱动不可用！\n", _outShpPath);
                return;
            }
            // 创建数据源
            if (File.Exists(_outShpPath))
            {
                MessageBoxButtons messBut = MessageBoxButtons.YesNo;
                if (MessageBox.Show(_shpfileName + "存在同名文件，是否覆盖？", "文件重名", messBut) == DialogResult.Yes)
                {
                    File.Delete(_outShpPath);
                }
                else
                {
                    string dir = Path.GetDirectoryName(_outShpPath);
                    string filename = Path.GetFileNameWithoutExtension(_outShpPath);
                    string ext = Path.GetExtension(_outShpPath);

                    _outShpPath = dir + "\\" + filename + "1" + ext;
                }
            }
            oDS = oDriver.CreateDataSource(_outShpPath, null);
            if (oDS == null)
            {
                Console.WriteLine("创建矢量文件【%s】失败！\n", _outShpPath);
                return;
            }
            // 创建图层，创建一个多边形图层，这里没有指定空间参考，如果需要的话，需要在这里进行指定
            oLayer = oDS.CreateLayer(_shpfileName, null, type, null);

            if (oLayer == null)
            {
                Console.WriteLine("图层创建失败！\n");
                return;
            }
        }

        //添加字段
        public void AddField(string FieldName, FieldType fieldType)
        {
            AddField(FieldName, fieldType, 0);
        }
        public void AddField(string FieldName, FieldType fieldType, int FieldLength)
        {
            if (oLayer == null)
            {
                Console.WriteLine("目标图层为null，无法添加字段！");
                return;
            }
            FieldDefn oFieldDef = new FieldDefn(FieldName, fieldType);
            if (FieldLength > 0)
                oFieldDef.SetWidth(FieldLength);
            oLayer.CreateField(oFieldDef, 1);
        }

        //创建Feature
        public void InsertFeature(Geometry RingGeo)
        {
            Feature oFea = new Feature(oLayer.GetLayerDefn());
            oFea.SetGeometry(RingGeo);
            oLayer.CreateFeature(oFea);
        }
        //Ring进行闭合检查
        private void RingCheck(Geometry geometry)
        {
            if (geometry.GetGeometryType() == wkbGeometryType.wkbLinearRing)
            {
                double x, y, z;
                int cnt = geometry.GetPointCount();
                x = geometry.GetX(0);
                y = geometry.GetY(0);
                z = geometry.GetZ(0);
                if (!DoubEqual(x, geometry.GetX(cnt - 1))
                    || !DoubEqual(y, geometry.GetY(cnt - 1))
                    || !DoubEqual(z, geometry.GetZ(cnt - 1)))
                {
                    geometry.AddPoint(x, y, z);
                }

            }
        }

        private bool DoubEqual(double a, double b)
        {
            if (Math.Abs(a - b) < 0.0000001)
                return true;
            else
                return false;
        }

        public void Close()
        {
            oLayer.Dispose();
            oDS.Dispose();
        }
    }
}
