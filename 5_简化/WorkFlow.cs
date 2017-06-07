using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._5_简化
{
    class WorkFlow
    {
        /*******************************     算法一     *************************************/

        ///     
        /// 2 调用点矩法提出断点，标记在CUT字段中 
        ///     
        /// 3 调用点角法提出平角点，标计在straight中
        /// 
        /// 4 通过cut字段提取分段点集，再通过straight提取选用点集
        /// 
        /// 5 用选出的点集拟合直线并保留参数
        /// 6 用参数集进行求交
        /// 7 主要参数：点距法中的最大点距、点角法中的最大角度和迭代次数
        /// 


        /*******************************     工作流     *************************************/


        public static void getDLG(string inFile, string outFile)
        {
            //注册
            Ogr.RegisterAll(); Gdal.AllRegister();
            //驱动
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            //原数据
            DataSource inDS = dr.Open(inFile, 0);
            Layer inLayer = inDS.GetLayerByIndex(0);

            //out数据
            if (File.Exists(outFile))
                File.Delete(outFile);
            DataSource outDS = dr.CreateDataSource(outFile, null);
            Layer outLayer = outDS.CreateLayer("outLayer", null, wkbGeometryType.wkbPolygon, null);

            int featCount = inLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                Feature aFeatuer = inLayer.GetFeature(i);
                Geometry subGeom = aFeatuer.GetGeometryRef().GetGeometryRef(0);
                //工作流 2 ， 返回所有断点的ID，有重要参数！
                List<int> cutIDS = _点距法提断点._提取断点(aFeatuer, 2);

                //工作流 3， 返回所有平角点的ID，有重要参数！
                List<int> straightIDS = _提取平角点._平角点ID集(aFeatuer, 175, 5);
             
                //输出准备工作
                Feature newFeat = new Feature(new FeatureDefn(""));
                Geometry newGeom = new Geometry(wkbGeometryType.wkbPolygon);

                //工作流 4 , 通过cut字段提取分段点集，再通过straight提取选用点集
                newGeom.AddGeometry(ForDLG._提取断平拟合直线(aFeatuer, cutIDS, straightIDS));

                //收！
                newFeat.SetGeometry(newGeom);
                outLayer.CreateFeature(newFeat);
                newFeat.Dispose();
                aFeatuer.Dispose();
            }

            inLayer.Dispose();
            outLayer.Dispose();
            inDS.Dispose();
            outDS.Dispose();
        }



    }
}
