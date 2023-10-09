using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class Features
    {
        #region 字段

        private List<Feature> _Features;  //要素集合

        #endregion
        #region 构造函数

        public Features()
        {
            _Features = new List<Feature>();
        }

        #endregion
        #region 属性

        public Int32 Count
        {
            get { return _Features.Count; }
        }

        #endregion
        #region 方法

        public Feature GetItem(Int32 index)
        {
            return _Features[index];
        }
        public void Remove(Feature feature)
        {
            _Features.Remove(feature);
        }

        public void SetItem(Int32 index, Feature feature)
        {
            _Features[index] = feature;
        }

        public void Add(Feature feature)
        {
            _Features.Add(feature);
        }

        public void RemoveAt(Int32 index)
        {
            _Features.RemoveAt(index);
        }

        public void Clear()
        {
            _Features.Clear();
        }
        #endregion
    }
}
