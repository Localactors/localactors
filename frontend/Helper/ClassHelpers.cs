using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;


    public static class ClassHelpers {

       

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property) {
            return ApplyOrder(source, property, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property) {
            return ApplyOrder(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property) {
            return ApplyOrder(source, property, "ThenBy");
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property) {
            return ApplyOrder(source, property, "ThenByDescending");
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName) {
            string[] props = property.Split('.');
            Type type = typeof (T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props) {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof (Func<,>).MakeGenericType(typeof (T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof (Queryable).GetMethods().Single(method => method.Name == methodName && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2).MakeGenericMethod(typeof (T), type).Invoke(null, new object[] {source, lambda});
            return (IOrderedQueryable<T>) result;
        }

        public static string Capitalize(this string str) {

            if (string.IsNullOrEmpty(str))
                return str;

            //TextInfo info = new CultureInfo("it-IT", false).TextInfo;
            //return info.ToTitleCase(str);

            StringBuilder result = new StringBuilder(str);
            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++i) {

                char control = result[i - 1];
                if (!char.IsLetterOrDigit(control))
                    result[i] = char.ToUpper(result[i]);
                else
                    result[i] = char.ToLower(result[i]);
            }
            return result.ToString();

        }
        public static string Truncate(this string str, int maxlen) {
            if (string.IsNullOrEmpty(str) || str.Length <= maxlen)
                return str;
            return str.Substring(0, maxlen);
        }
        public static string Truncate(this string str, int maxlen, string ellipsis)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxlen)
                return str;

            return str.Substring(0, maxlen - ellipsis.Length) + ellipsis;
        }
    }
