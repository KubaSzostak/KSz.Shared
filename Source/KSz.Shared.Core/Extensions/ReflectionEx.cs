using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;

namespace System.Reflection
{
    public static class ReflectionEx
    {

        public static bool IsEditableValue(this PropertyInfo prop)
        {
            var isUpdatable = prop.PropertyType.IsValueType || (prop.PropertyType == typeof(string));
            //var notEditableAttr = prop.GetCustomAttributes(typeof(NotEditablePropertyAttribute), true);
            var notEditableAttr = prop.IsDefined(typeof(NotEditablePropertyAttribute), true);

            return isUpdatable && prop.CanWrite && !notEditableAttr;
        }
        private static PropertyInfo GetLambdaPropertyInfo<T>(this LambdaExpression propertyExpresssion)
        {
            if (propertyExpresssion == null)
                throw new ArgumentNullException("propertyExpresssion");

            var lambda = (LambdaExpression)propertyExpresssion;
            var unaryExpr = lambda.Body as UnaryExpression;
            MemberExpression mbrExpr = null;

            if (unaryExpr != null)
                mbrExpr = unaryExpr.Operand as MemberExpression;
            else
                mbrExpr = lambda.Body as MemberExpression;
            if (mbrExpr == null)
                throw new ArgumentException("The expression is not a member access expression.", "propertyExpresssion");

            var propInfo = mbrExpr.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("The member access expression does not access a property.", "propertyExpresssion");

            var getMethod = propInfo.GetGetMethod(true);
            if (getMethod.IsStatic)
                throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");

            return propInfo;
        }

        /// <summary>
        /// You can use:
        /// GetPropertyName(() => MyProperty);
        /// </summary>
        /// <typeparam name="T">Leave Empty</typeparam>
        /// <param name="property">Use () => MyProperty</param>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T>> propertyExpresssion)
        {
            return GetLambdaPropertyInfo<T>(propertyExpresssion as LambdaExpression);
        }

        public static PropertyInfo GetPropertyInfo<T, TRes>(this Expression<Func<T, TRes>> propertyExpresssion)
        {
            return GetLambdaPropertyInfo<T>(propertyExpresssion as LambdaExpression);
        }

        /// <summary>
        /// You can use:
        /// GetPropertyName(() => MyProperty);
        /// </summary>
        /// <typeparam name="T">Leave Empty</typeparam>
        /// <param name="property">Use () => MyProperty</param>
        public static string GetPropertyName<T>(this Expression<Func<T>> propertyExpresssion)
        {
            return propertyExpresssion.GetPropertyInfo().Name;
        }

        public static string GetPropertyName<T>(this object obj, Expression<Func<T>> propertyExpresssion)
        {
            return propertyExpresssion.GetPropertyName();
        }

        public static PropertyChangedEventArgs GetPropertyArgs<T>(this object obj, Expression<Func<T>> propertyExpresssion)
        {
            return new PropertyChangedEventArgs(GetPropertyName(propertyExpresssion));
        }

        public static void SetStringValue(this PropertyInfo propInfo, object propObj, string strValue)
        {
            object val = strValue;
            if (propInfo.PropertyType != typeof(string))
            {
                if (string.IsNullOrEmpty(strValue))
                    val = null;
                else if (propInfo.PropertyType.IsEnum)
                    val = Enum.Parse(propInfo.PropertyType, strValue, true);
                else
                    val = Convert.ChangeType(strValue, propInfo.PropertyType, CultureInfo.InvariantCulture);
            }

            propInfo.SetValue(propObj, val, null);
        }

        public static string GetStringValue(this PropertyInfo propInfo, object propObj)
        {
            object val = propInfo.GetValue(propObj, null);
            if (val.IsEmpty())
                return null;

            return Convert.ChangeType(val, typeof(string), CultureInfo.InvariantCulture).ToText();
        }


        public static Dictionary<string, object> GetProperties(this object obj, bool onlyEditableProperties)
        {
            var res = new Dictionary<string, object>();
            var props = obj.GetType().GetProperties();
            if (onlyEditableProperties)
                props = props.Where(p => p.IsEditableValue()).ToArray();

            foreach (var p in props)
            {
                try
                {
                    res[p.Name] = p.GetValue(obj, null);
                }
                catch (Exception ex)
                {
                    res[p.Name] = "ERROR: " + ex.Message;
                }
            }
            return res;
        }
    }
}
