using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class Fields
    {
        #region 字段

        private List<Field> _Fields;  //字段集合
        private string _PrimaryField = "";  //主字段名称
        private bool _ShowAlias = false;    //是否显示别名

        #endregion

        #region 构造函数

        public Fields()
        {
            _Fields = new List<Field>();
        }

        #endregion

        #region 属性

        public Int32 Count
        {
            get { return _Fields.Count; }
        }

        public string PrimaryField
        {
            get { return _PrimaryField; }
            set { _PrimaryField = value; }
        }

        public bool ShowAlias
        {
            get { return _ShowAlias; }
            set { _ShowAlias = value; }
        }

        #endregion

        #region 方法

        public Int32 FindField(string name)
        {
            Int32 sFieldCount = _Fields.Count;
            for (Int32 i = 0; i <= sFieldCount - 1; i++)
            {
                if (_Fields[i].Name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }
            return -1;
        }

        public Field GetItem(Int32 index)
        {
            return _Fields[index];
        }

        public Field GetItem(string name)
        {
            Int32 sIndex = FindField(name);
            if (sIndex >= 0)
                return _Fields[sIndex];
            else
                return null;
        }

        public void Append(Field field)
        {
            if (FindField(field.Name) >= 0)
            {
                throw new Exception("Fields对象中不允许存在重名的字段");
            }
            _Fields.Add(field);
            //触发事件
            if (FieldAppended != null)
                FieldAppended(this, field);
        }

        public void RemoveAt(Int32 index)
        {
            Field sField = _Fields[index];
            _Fields.RemoveAt(index);
            //触发事件
            if (FieldRemoved != null)
                FieldRemoved(this, index, sField);
        }

        #endregion

        #region 事件

        internal delegate void FieldRemovedHandle(object sender, Int32 fieldIndex, moField fieldRemoved);
        /// <summary>
        /// 有字段被删除
        /// </summary>
        internal event FieldRemovedHandle FieldRemoved;

        internal delegate void FieldAppendedHandle(object sender, moField fieldAppended);
        /// <summary>
        /// 有字段被加入
        /// </summary>
        internal event FieldAppendedHandle FieldAppended;

        #endregion

    }
}
