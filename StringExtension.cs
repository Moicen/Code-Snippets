using System;

/**
 * @author <a href="mailto:moicen1988@gmail.com">Moicen</a>
 */

namespace Icen.Utils.Extension
{
    public static class StringExtension
    {
        /// <summary>
        /// Contains扩展重载
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="ignoreCase">是否区分大小写</param>
        /// <returns></returns>
        public static bool Contains(this string str, string value, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (string.IsNullOrEmpty(str)) return false;
            return ignoreCase ? str.ToLower().Contains(value.ToLower()) : str.Contains(value);
        }

        public static int[] ToInts(this string[] strs)
        {
            var len = strs.Length;
            var ints = new int[len];
            for (var i = 0; i < len; i++)
            {
                ints[i] = Convert.ToInt32(strs[i]);
            }
            return ints;
        }
        public static int[] ToInts(this string str, char separator = ',')
        {
            if (string.IsNullOrEmpty(str)) return new int[] { };
            return str.Split(separator).ToInts();
        }
    }
}
