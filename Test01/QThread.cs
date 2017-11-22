using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test01
{
    class QThread
    {
        GeneralShp polygonShp;
        GeneralShp polylineShp;
        public QThread(string shpPath)
        {
            polygonShp = new GeneralShp(shpPath, Path.GetFileNameWithoutExtension(shpPath), OSGeo.OGR.wkbGeometryType.wkbPolygon);
            //polylineShp = new GeneralShp(shpPath, Path.GetFileNameWithoutExtension(shpPath) + "_line", wkbGeometryType.wkbLineString);
        }
        public void Write(object i)
        {
            Console.WriteLine("qThread输出： " + i.ToString());
        }
        private void WriteShp(OSGeo.OGR.Geometry geo)
        {
            if (geo.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon25D ||
                geo.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon)
            {
                polygonShp.InsertFeature(geo);
            }
            else
                polylineShp.InsertFeature(geo);
        }

        public void WritePolygonShp(OSGeo.OGR.Geometry geo)
        {
            if (geo.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon25D ||
                geo.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon)
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
