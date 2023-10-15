using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class Layers
    {
        #region 字段

        private List<Layer> _Layers = new List<Layer>();

        #endregion

        #region 构造函数

        public Layers()
        { }

        #endregion

        #region 属性

        /// <summary>
        /// 获取图层数量
        /// </summary>
        public Int32 Count
        {
            get { return _Layers.Count; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取指定索引号的图层
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Layer GetItem(Int32 index)
        {
            return _Layers[index];
        }

        /// <summary>
        /// 在图层序列末尾增加一个图层
        /// </summary>
        /// <param name="mapLayer"></param>
        public void Add(Layer mapLayer)
        {
            _Layers.Add(mapLayer);
        }

        /// <summary>
        /// 移除指定图层
        /// </summary>
        /// <param name="mapLayer"></param>
        public void Remove(Layer mapLayer)
        {
            _Layers.Remove(mapLayer);
        }

        /// <summary>
        /// 移除指定索引号的图层
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(Int32 index)
        {
            _Layers.RemoveAt(index);
        }

        /// <summary>
        /// 清除所有图层
        /// </summary>
        public void Clear()
        {
            _Layers.Clear();
        }

        /// <summary>
        /// 将指定索引的图层移动到另一指定索引
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        public void MoveTo(Int32 fromIndex, Int32 toIndex)
        {
            if (fromIndex == toIndex)
                return;
            else
            {
                Layer sLayer = _Layers[fromIndex];
                _Layers.RemoveAt(fromIndex);
                _Layers.Insert(toIndex, sLayer);
            }
        }

        #endregion
    }
}
