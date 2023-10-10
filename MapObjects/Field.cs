using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class Field
    {
        #region 字段

        private string _Name = "";  //字段名称
        private string _AliasName = ""; //字段别名
        private Type _ValueType = typeof(Int32);
        private Int32 _Length;  //字段长度

        #endregion

        #region 构造函数

        public Field(string name)
        {
            _Name = name;
            _AliasName = name;
        }

        public Field(string name, Type valueType)
        {
            _Name = name;
            _AliasName = name;
            _ValueType = valueType;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取字段名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
        }

        public string AliasName
        {
            get { return _AliasName; }
            set { _AliasName = value; }
        }

        public Type ValueType
        {
            get { return _ValueType; }
        }

        public Int32 Length
        {
            get { return _Length; }
        }

        #endregion

        #region 方法

        public Field Clone()
        {
            Field sField = new Field(_Name, _ValueType);
            sField._AliasName = _AliasName;
            sField._Length = _Length;
            return sField;
        }

        #endregion
    }
}
