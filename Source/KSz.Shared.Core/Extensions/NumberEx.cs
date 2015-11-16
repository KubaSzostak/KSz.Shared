using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class NumberEx
    {
        public static bool SameValue(this double v0, double val, double precision)
        {
            return Math.Abs(val - v0) < precision;
        }
    }
}
