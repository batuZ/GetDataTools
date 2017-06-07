using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._2_坡度
{
    class QThread
    {
        GeneralShp polygonShp;
        GeneralShp polylineShp;
        public QThread(string shpPath)
        {
            polygonShp = new GeneralShp(shpPath, Path.GetFileNameWithoutExtension(shpPath), wkbGeometryType.wkbPolygon);
            //polylineShp = new GeneralShp(shpPath, Path.GetFileNameWithoutExtension(shpPath) + "_line", wkbGeometryType.wkbLineString);
        }
        public void Write(object i)
        {
            Console.WriteLine("qThread输出： "+i.ToString());
        }
        private void WriteShp(Geometry geo)
        {
            if (geo.GetGeometryType() == wkbGeometryType.wkbPolygon25D||
                geo.GetGeometryType() == wkbGeometryType.wkbPolygon)
            {
                polygonShp.InsertFeature(geo);
            }
            else
                polylineShp.InsertFeature(geo);
        }

        public void WritePolygonShp(Geometry geo)
        {
            if (geo.GetGeometryType() == wkbGeometryType.wkbPolygon25D ||
                geo.GetGeometryType() == wkbGeometryType.wkbPolygon)
            {
                polygonShp.InsertFeature(geo);
            }
        }
        public void Close()
        {
            polygonShp.Close();
            //polylineShp.Close();
        }
    }
}
