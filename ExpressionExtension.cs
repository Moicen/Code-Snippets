using System;
using System.Collections.Generic;
using System.Linq;
using Icen.Utils.Helper;

/**
 * @author <a href="mailto:moicen1988@gmail.com">Moicen</a>
 */

namespace Icen.Utils.Extension
{
    public static class ExpressionExtension
    {
        /// <summary>
        /// 通过对指定的属性的值进行比较，对序列进行正序排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string propertyName) where T : class, new()
        {
            return OrderBy(source, propertyName, false);
        }
        /// <summary>
        /// 通过对指定的属性的值进行比较，使用一个布尔值指定是否倒序，对序列进行排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string propertyName, bool desc) where T : class, new()
        {
            return ExpressionHelper<T>.OrderBy(source, propertyName, desc);
        }
        /// <summary>
        /// 使用指定的参数字典构造谓词表达式，对序列进行筛选
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Dictionary<string, object> param) where T : class, new()
        {
            if (param == null || param.Count == 0) return source;
            return ExpressionHelper<T>.Where(source, param);
        }
        /// <summary>
        /// 通过对谓词表达式指定的属性值进行比较，返回序列中的非重复元素
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element))) { yield return element; }
            }
        }

        /// <summary>
        /// 获取符合条件的第一个值，如果未找到，返回一个新的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T FirstOrNew<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class, new()
        {
            var first = source.FirstOrDefault(predicate);
            return first ?? new T();
        }

        /// <summary>
        /// 获取符合条件的第一个值的指定字段值，如果为空，返回空字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string PropertyOfFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, string propertyName) where T : class , new()
        {
            return ExpressionHelper<T>.PropertyOfFirst(source, predicate, propertyName);
        }

    }
}
