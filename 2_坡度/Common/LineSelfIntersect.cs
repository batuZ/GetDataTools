using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetDataTools._2_坡度.Core;

namespace GetDataTools._2_坡度.Common
{
    public class LineSelfSect
    {
        private static LineSelfSect _instance;
        public static LineSelfSect Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LineSelfSect();
                return _instance;
            }
        }
        /// <summary>
        /// 判断自相交图形
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<OrderPoint> RingedList(List<OrderPoint> list)
        {
            foreach (OrderPoint item in list)
            {
                int _itemIndex=list.IndexOf(item);
                List<OrderPoint> findlist = list.FindAll((a) =>
                {
                    if (Math.Abs(a.X - item.X) < 3 &&
                        Math.Abs(a.Y - item.Y) < 3 &&
                        Math.Abs(list.IndexOf(a) - list.IndexOf(item)) > 25)
                        return true;
                    else
                        return false;
                });
                if (findlist.Count > 0)
                {
                    int min = list.IndexOf(findlist[0]);
                    if (min < _itemIndex)
                        return list.GetRange(min, _itemIndex-min);
                    else
                        return list.GetRange(_itemIndex, min-_itemIndex);
                }
            }
            return null;
        }
    }
}
