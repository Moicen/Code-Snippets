using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/**
 * @author <a href="mailto:moicen1988@gmail.com">Moicen</a>
 */

namespace Icen.Utils.Helper
{
    public static class ExpressionHelper<T>
    {
        #region OrderBy

        /// <summary>
        /// 缓存已用过的表达式委托
        /// </summary>
        private static readonly Dictionary<string, Func<T, object>> CacheOrderFuncs = new Dictionary<string, Func<T, object>>();
        /// <summary>
        /// 自定义排序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy(IEnumerable<T> source, string propertyName, bool desc)
        {
            var func = OrderByFunc(propertyName);
            return desc ? source.OrderByDescending(func) : source.OrderBy(func);
        }
        /// <summary>
        /// 根据属性名获取可供排序的委托
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static Func<T, object> OrderByFunc(string propertyName)
        {
            var type = typeof(T);
            string key = string.Format("{0}_{1}", type.Name, propertyName);
            if (CacheOrderFuncs.ContainsKey(key))
            {
                return CacheOrderFuncs[key];
            }
            var inst = Expression.Parameter(type);
            var prop = Expression.Convert(Expression.Property(inst, type.GetProperty(propertyName)), typeof(IComparable));
            var func = Expression.Lambda<Func<T, object>>(prop, inst).Compile();
            return CacheOrderFuncs[key] = func;
        }

        #endregion

        #region Where
        /// <summary>
        /// 使用指定的参数字典构造谓词表达式，对序列进行筛选
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IEnumerable<T> Where(IEnumerable<T> source, Dictionary<string, object> parameter)
        {
            return source.Where(WhereFunc(parameter));
        }
        /// <summary>
        /// 获取可供筛选的条件委托
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static Func<T, bool> WhereFunc(Dictionary<string, object> parameter)
        {
            var type = typeof(T);
            var inst = Expression.Parameter(type);
            Expression exp = Expression.Constant(true);
            foreach (var o in parameter)
            {
                if (o.Value == null || string.IsNullOrEmpty(o.Value.ToString())) continue;
                ComparisonModes mode;
                PropertyInfo property = ReadProperty(type, o.Key, parameter, out mode);
                var left = Expression.Property(inst, property);
                var rights = BuildConstantExpression(property, o.Value);
                Expression innerExp = Expression.Equal(left, rights[0]);
                switch (mode)
                {
                    case ComparisonModes.GreaterThen:
                        innerExp = Expression.GreaterThan(left, rights[0]);
                        break;
                    case ComparisonModes.GreaterThenOrEqual:
                        innerExp = Expression.GreaterThanOrEqual(left, rights[0]);
                        break;
                    case ComparisonModes.LessThen:
                        innerExp = Expression.LessThan(left, rights[0]);
                        break;
                    case ComparisonModes.LessThenOrEqual:
                        innerExp = Expression.LessThanOrEqual(left, rights[0]);
                        break;
                    case ComparisonModes.Equal:
                    default:
                        break;
                }
                for (int i = 1; i < rights.Length; i++)
                {
                    innerExp = Expression.Or(innerExp, Expression.Equal(left, rights[i]));
                }
                exp = Expression.AndAlso(exp, innerExp);
            }
            return Expression.Lambda<Func<T, bool>>(exp, inst).Compile();
        }
        /// <summary>
        /// 读取指定参数键对应的属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private static PropertyInfo ReadProperty(Type type, string key, Dictionary<string, object> parameter, out ComparisonModes mode)
        {
            mode = ComparisonModes.Equal;
            var property = type.GetProperty(key);
            if (property == null)
            {
                string prop = key;
                DateTime value;
                bool isDateTime = DateTime.TryParse(parameter[key].ToString(), out value);

                if (key.EndsWith("Start"))
                {
                    prop = key.Remove(key.Length - 5);
                    mode = ComparisonModes.GreaterThenOrEqual;
                    if (isDateTime) parameter[key] = value.ToString("yyyy/MM/dd") + " 00:00:00.000";
                }
                if (key.EndsWith("End"))
                {
                    prop = key.Remove(key.Length - 5);
                    mode = ComparisonModes.LessThenOrEqual;
                    if (isDateTime) parameter[key] = value.ToString("yyyy/MM/dd") + " 23:59:59.999";
                }
                property = type.GetProperty(prop);
                if (property == null) throw new Exception(string.Format("Property \"{0}\" Not Exist in {1}.", key, type.Name));
            }
            return property;
        }

        /// <summary>
        /// 使用属性构建常量表达式
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static ConstantExpression[] BuildConstantExpression(PropertyInfo property, object value)
        {
            var type = property.PropertyType;
            if (type == typeof(string))
                return new[] { Expression.Constant(value) };

            var arr = value.ToString().Split(',');
            var rights = new ConstantExpression[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                rights[i] = Expression.Constant(ChangeType(arr[i], type));
            }
            return rights;
        }
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object ChangeType(object value, Type type)
        {
            if (type.IsEnum)
                return EnumHelper.ParseEnum(value, type);
            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 比较方式
        /// </summary>
        public enum ComparisonModes
        {
            Equal = 0,
            LessThen = 1,
            LessThenOrEqual = 2,
            GreaterThen = 3,
            GreaterThenOrEqual = 4
        }

        #endregion

        #region FirstOrDefault

        private static readonly Dictionary<string, IEnumerable<PropertyInfo>> CacheProperties = new Dictionary<string, IEnumerable<PropertyInfo>>();
        /// <summary>
        /// 获取列表中符合条件的第一项的指定字段值，并将其转换为String，如果为空，返回空字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string PropertyOfFirst(IEnumerable<T> source, Func<T, bool> predicate, string propertyName)
        {
            var sourceProperties = ReadProperties();
            var first = source.FirstOrDefault(predicate);
            if (first == null) return null;
            var value = sourceProperties.First(p => p.Name == propertyName).GetValue(first, null);
            return value == null ? null : value.ToString();
        }

        private static IEnumerable<PropertyInfo> ReadProperties()
        {
            return ReadProperties<T>();
        }

        private static IEnumerable<PropertyInfo> ReadProperties<TTarget>()
        {
            var type = typeof(TTarget);
            string key = type.FullName;
            if (!CacheProperties.ContainsKey(key))
            {
                CacheProperties.Add(key, type.GetProperties());
            }
            return CacheProperties[key];
        }

        #endregion


        /// <summary>
        /// 泛型通用比较器
        /// </summary>
        public class EqualityComparer : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _compare = null;
            public EqualityComparer(Func<T, T, bool> compare)
            {
                if (compare == null) { throw new ArgumentNullException("compare"); }
                _compare = compare;
            }
            public bool Equals(T x, T y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return _compare(x, y);
            }
            public int GetHashCode(T obj)
            {
                return 0;
            }
        }
    }



}
