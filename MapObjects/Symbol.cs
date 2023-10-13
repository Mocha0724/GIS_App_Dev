using MyMapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public abstract class Symbol
    {
        public abstract SymbolTypeConstant SymbolType { get; }
        public abstract Symbol Clone();
    }
}
