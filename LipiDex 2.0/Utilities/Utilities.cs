using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipiDex_2._0.Utilities
{
    public static class Utilities
    {
        public static double PPMError(double mass1, double mass2)
        {
            return (Math.Abs(mass1 - mass2) / mass2) * 1000000.0;
        }

    }
}
