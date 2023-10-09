using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class SimpleRenderer:Renderer
    {
        #region 字段

        private Symbol _Symbol;

        #endregion
        #region 构造函数

        public SimpleRenderer()
        { }

        #endregion
        #region 属性

        public override Type RendererType
        {
            get { return typeof(SimpleRenderer); }
        }

        public Symbol Symbol
        {
            get { return _Symbol; }
            set { _Symbol = value; }
        }

        #endregion
        #region 方法

        public override Renderer Clone()
        {
            SimpleRenderer sRenderer = new SimpleRenderer();
            sRenderer._Symbol = _Symbol.Clone();
            return sRenderer;
        }

        #endregion
    }
}
