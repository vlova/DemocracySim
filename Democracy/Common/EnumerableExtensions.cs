using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Democracy.Common
{
    public static class EnumerableExtensions
    {
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private static int GetRandomNumber(int size)
        {
            byte[] data = new byte[4];

            rng.GetBytes(data);

            // well, ok, using of LossyAbs will make bias on random, but it's small
            var number = LossyAbs(BitConverter.ToInt32(data, 0));
            return number % size;
        }
        private static int LossyAbs(int value)
        {
            if (value >= 0) return value;
            if (value == int.MinValue) return int.MaxValue;
            return -value;
        }

        public static IEnumerable<T> GetShuffled<T>(this IEnumerable<T> collection)
        {
            var list = new List<T>(collection);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = GetRandomNumber(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
