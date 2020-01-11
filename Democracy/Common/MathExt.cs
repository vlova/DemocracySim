using MathNet.Numerics.Distributions;
using System;

namespace Democracy
{
    static class MathExt
    {
        public static double Clamp(this double number, double min, double max)
        {
            if (number < min)
            {
                return min;
            }

            if (number > max)
            {
                return max;
            }

            return number;
        }

        public static int GetPercentageWithRandomRounding(this int amount, double percentage)
        {
            var integerAmount = (int)Math.Floor(amount * percentage);
            var mathExpectation = amount * percentage;
            var difference = mathExpectation - integerAmount; // 0 <= difference < 1
            var roundedValue = ContinuousUniform.Sample(0, 1) < difference ? 1 : 0;
            return integerAmount + roundedValue;
        }
    }
}
