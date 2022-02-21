using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderCoachoutlet
{
    internal static class Extensions
    {
        private static Random random = new Random();

        public static string RandomString(int from, int to)
        {
            return RandomString(from + random.Next(to + 1 - from));
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static string RandomStringAndNum(int from, int to)
        {
            return RandomStringAndNum(from + random.Next(to + 1 - from));
        }
        public static string RandomStringAndNum(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static string RandomNumber(int from, int to)
        {
            return RandomNumber(from + random.Next(to + 1 - from));
        }
        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
