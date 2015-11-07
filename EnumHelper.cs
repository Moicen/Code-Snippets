using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

/**
 * @author <a href="mailto:moicen1988@gmail.com">Moicen</a>
 */

namespace Icen.Utils.Helper
{
    /// <summary>
    /// 枚举解析帮助类
    /// </summary>
    public static class EnumHelper
    {

        #region Read Description

        /// <summary>
        /// 读取枚举类型的描述
        /// </summary>
        /// <param name="name">要读取的枚举项的名称</param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(string name)
        {
            FieldInfo item = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (item == null) return null;
            var attribute = Attribute.GetCustomAttribute(item, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? null : attribute.Description;
        }
        /// <summary>
        /// 读取枚举类型的描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">枚举值/名</param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(int value)
        {
            var name = ((T)Enum.ToObject(typeof(T), value)).ToString();
            FieldInfo item = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (item == null) return null;
            var attribute = Attribute.GetCustomAttribute(item, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? null : attribute.Description;
        }
        /// <summary>
        /// 读取枚举值的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            Type type = value.GetType();
            FieldInfo item = type.GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
            if (item == null) return null;
            var attribute = Attribute.GetCustomAttribute(item, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? null : attribute.Description;
        }
        /// <summary>
        /// 读取枚举值的描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return
                GetEnumDescription(
                    (Enum)
                        ParseEnum(value, typeof (T), value is char || (value is string && value.ToString().Length == 1)));
        }

        #endregion

        #region Parse

        /// <summary>
        /// 枚举解析(可以解析枚举值、枚举名和枚举描述)
        /// </summary>
        /// <typeparam name="T">要解析的枚举类型</typeparam>
        /// <param name="value">要解析的值</param>
        /// <returns></returns>
        public static T ParseEnum<T>(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            Type type = typeof(T);
            if (Enum.IsDefined(type, value))
            {
                int val;
                if (int.TryParse(value.ToString(), out val))
                {
                    return (T)Enum.ToObject(type, val);
                }
                return (T)Enum.Parse(type, value.ToString(), true);
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null && attr.Description == value.ToString())
                {
                    return (T)field.GetRawConstantValue();
                }
            }
            throw new ArgumentOutOfRangeException("value");
        }
        /// <summary>
        /// 枚举解析(可以解析枚举值、枚举名和枚举描述)
        /// </summary>
        /// <typeparam name="T">要解析的枚举类型</typeparam>
        /// <param name="value">要解析的值</param>
        /// <param name="isChar">标明枚举值是char型且传入的参数为枚举值</param>
        /// <returns></returns>
        public static T ParseEnum<T>(object value, bool isChar)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (isChar)
                return ParseEnum<T>((int) Convert.ToChar(value));
            return ParseEnum<T>(value);
        }
        public static object ParseEnum(object value, Type type, bool isChar)
        {
            return isChar ? ParseEnum((int)Convert.ToChar(value), type) : ParseEnum(value, type);
        }
        /// <summary>
        /// 使用指定的枚举类型对输入值进行解析
        /// </summary>
        /// <param name="value">要解析的值</param>
        /// <param name="type">要解析的枚举类型</param>
        /// <returns></returns>
        public static object ParseEnum(object value, Type type)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (!type.IsEnum) throw new InvalidEnumArgumentException("type");
            if (Enum.IsDefined(type, value))
            {
                int val;
                if (int.TryParse(value.ToString(), out val))
                {
                    return Enum.ToObject(type, val);
                }
                return Enum.Parse(type, value.ToString(), true);
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null && attr.Description == value.ToString())
                {
                    return field.GetRawConstantValue();
                }
            }
            throw new ArgumentOutOfRangeException("value");
        }

        #endregion

        #region Count

        /// <summary>
        /// 获取枚举类型的个数，按枚举名计算
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Count<T>()
        {
            return Count(typeof (T));
        }
        /// <summary>
        /// 获取枚举类型的个数，按枚举名计算
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int Count(Type type)
        {
            return Enum.GetNames(type).Length;
        }

        #endregion

        #region To Select List

        public static IEnumerable<SelectListItem> ToSelectList<T>()
        {
            return Enum.GetNames(typeof(T)).Select(n => new SelectListItem()
            {
                Value = n,
                Text = GetEnumDescription<T>(n)
            });
        }
        /// <summary>
        /// 枚举生成下拉列表(以枚举名为下拉值)
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="excluded">需要排除的枚举值列表</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> ToSelectList<T>(params T[] excluded)
        {
            return
                Enum.GetNames(typeof(T)).Where(n => !excluded.Any(i => i.ToString().Equals(n))).Select(n => new SelectListItem()
                {
                    Value = n,
                    Text = GetEnumDescription<T>(n)
                });
        }

        public static IEnumerable<SelectListItem> ToSelectList<T>(T selected)
        {
            return
               Enum.GetNames(typeof(T)).Select(n => new SelectListItem()
               {
                   Value = n,
                   Text = GetEnumDescription<T>(n),
                   Selected = selected.ToString() == n
               });
        }
        public static IEnumerable<SelectListItem> ToSelectedList<T>(T selected, params T[] excluded)
        {
            return
                Enum.GetNames(typeof(T)).Where(n => !excluded.Any(i => i.ToString().Equals(n))).Select(n => new SelectListItem()
                {
                    Value = n,
                    Text = GetEnumDescription<T>(n),
                    Selected = selected.ToString() == n
                });
        }

        /// <summary>
        /// 枚举生成下拉列表
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="useValue">是否读取枚举值作为下拉选项的值</param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectList<T>(bool useValue)
        {
            if (!useValue) return ToSelectList<T>().ToList();
            var enums = new List<SelectListItem>();
            foreach (int value in Enum.GetValues(typeof(T)))
            {
                enums.Add(new SelectListItem()
                {
                    Text = GetEnumDescription<T>(value),
                    Value = Convert.ToInt32(value).ToString()
                });
            }
            return enums;
        }
        public static List<SelectListItem> ToSelectList<T>(bool useValue, T selected)
        {
            if (!useValue) return ToSelectList(selected).ToList();
            var enums = new List<SelectListItem>();
            foreach (int value in Enum.GetValues(typeof(T)))
            {
                var t = (T)(value as object);
                enums.Add(new SelectListItem()
                {
                    Text = GetEnumDescription(t as Enum),
                    Value = Convert.ToInt32(value).ToString(),
                    Selected = Convert.ToInt32(selected) == value
                });
            }
            return enums;
        }
        /// <summary>
        /// 枚举生成下拉列表(以枚举值为下拉值)
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="useValue">值定义是否Char类型</param>
        /// <param name="excluded">需要排除的枚举值列表</param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectList<T>(bool useValue, params T[] excluded)
        {
            if (!useValue) return ToSelectList(excluded).ToList();

            var enums = new List<SelectListItem>();
            foreach (int value in Enum.GetValues(typeof(T)))
            {
                if (excluded.Any(i => Convert.ToInt32(i) == value)) continue;
                enums.Add(new SelectListItem()
                {
                    Text = GetEnumDescription<T>(value),
                    Value = Convert.ToInt32(value).ToString()
                });
            }
            return enums;
        }
        public static List<SelectListItem> ToSelectedList<T>(bool useValue, T selected, params T[] excluded)
        {
            if (!useValue) return ToSelectedList(selected, excluded).ToList();
            var enums = new List<SelectListItem>();
            foreach (int value in Enum.GetValues(typeof(T)))
            {
                if (excluded.Any(i => Convert.ToInt32(i) == value)) continue;
                enums.Add(new SelectListItem()
                {
                    Text = GetEnumDescription<T>(value),
                    Value = Convert.ToInt32(value).ToString(),
                    Selected = Convert.ToInt32(selected) == value
                });
            }
            return enums;
        }


        #endregion


    }
}
