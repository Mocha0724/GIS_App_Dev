﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public abstract class Renderer
    {
        public abstract Type RendererType { get; }
        public abstract Renderer Clone();
    }
}