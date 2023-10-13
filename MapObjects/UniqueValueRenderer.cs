using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class UniqueValueRenderer:Renderer
    {
        #region 字段

        private string _Field = ""; //绑定字段
        private string _HeadTitle = ""; //在图层显示控件中的标题
        private bool _ShowHead = true; //在图层显示控件中是否显示标题
        private List<string> _Values = new List<string>();
        private List<Symbol> _Symbols = new List<Symbol>();
        private Symbol _DefaultSymbol;    //默认符号
        private bool _ShowDefaultSymbol = true; //在图层显示控件中是否显示默认符号

        #endregion
        #region 构造函数

        public UniqueValueRenderer()
        { }

        #endregion
        #region 属性

        public override Type RendererType
        {
            get
            {
                return typeof(UniqueValueRenderer);
            }
        }

        public string Field
        {
            get { return _Field; }
            set { _Field = value; }
        }

        public Int32 ValueCount
        {
            get { return _Values.Count; }
        }

        public Symbol DefaultSymbol
        {
            get { return _DefaultSymbol; }
            set { _DefaultSymbol = value; }
        }
        //其他属性不再编写，自行添加
        #endregion
        #region 方法

        public string GetValue(Int32 index)
        {
            return _Values[index];
        }

        public void SetValue(Int32 index, string value)
        {
            _Values[index] = value;
        }

        public Symbol GetSymbol(Int32 index)
        {
            return _Symbols[index];
        }

        public void SetSymbol(Int32 index, Symbol symbol)
        {
            _Symbols[index] = symbol;
        }

        public void AddUniqueValue(string value, Symbol symbol)
        {
            _Values.Add(value);
            _Symbols.Add(symbol);
        }

        public void AddUniqueValues(string[] values, Symbol[] symbols)
        {
            if (values.Length != symbols.Length)
            {
                throw new Exception("the length of the two arrays is not equal!");
            }
            _Values.AddRange(values);
            _Symbols.AddRange(symbols);
        }

        /// <summary>
        /// 根据指定唯一值获取对应符号，如果该值不存在则返回默认符号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Symbol FindSymbol(string value)
        {
            Int32 sValueCount = _Values.Count;
            for (Int32 i = 0; i <= sValueCount; i++)
            {
                if (_Values[i] == value)
                    return _Symbols[i];
            }
            return _DefaultSymbol;
        }

        public override Renderer Clone()
        {
            UniqueValueRenderer sRenderer = new UniqueValueRenderer();
            sRenderer._Field = _Field;
            sRenderer._HeadTitle = _HeadTitle;
            sRenderer._ShowHead = _ShowHead;
            Int32 sValueCount = _Values.Count;
            for (Int32 i = 0; i <= sValueCount - 1; i++)
            {
                string sValue = _Values[i];
                Symbol sSymbol = null;
                if (_Symbols[i] != null)
                    sSymbol = _Symbols[i].Clone();
                sRenderer.AddUniqueValue(sValue, sSymbol);
            }
            if (_DefaultSymbol != null)
                sRenderer.DefaultSymbol = _DefaultSymbol.Clone();
            sRenderer._ShowDefaultSymbol = _ShowDefaultSymbol;
            return sRenderer;
        }

        #endregion
    }
}
