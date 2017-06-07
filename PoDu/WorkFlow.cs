using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools.PoDu
{
    class WorkFlow
    {
        public static void poDu(string inDsmPath, string outPoDuImgPath, string outShpPath)
        {
            SlopeDem.SlopeDemObj.Slope(inDsmPath, outPoDuImgPath);
            SlopeDem.SlopeDemObj.CreatePolygon(inDsmPath, outShpPath);
        }
    }
}
