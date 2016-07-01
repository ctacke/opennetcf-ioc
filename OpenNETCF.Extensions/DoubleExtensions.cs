using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DoubleExtensions
    {
        public static bool InRange(this double value, double goal, double range)
        {
            if (value <= goal + range &&
                value >= goal - range)
            {
                return true;
            }
            return false;
        }
    }
}
