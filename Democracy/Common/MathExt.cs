using System;

namespace Democracy
{
    static class MathExt
    {
        public static double Clamp(this double number, double min, double max)
        {
            if (number < min) {
                return min;
            }

            if (number > max) {
                return max;
            }

            return number;
        }
    }
}
