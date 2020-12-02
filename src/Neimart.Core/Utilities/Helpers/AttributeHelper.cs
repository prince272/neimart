using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neimart.Core.Utilities.Helpers
{
    // C# Reflection AttributeHelper
    // source: https://lotsacode.wordpress.com/2011/04/27/c-reflectionhelper/
    public static class AttributeHelper
    {
        public static Dictionary<object, List<Attribute>> AttributeCache { get; } = new Dictionary<object, List<Attribute>>();

        // Types
        public static List<Attribute> GetTypeAttributes<TType>()
        {
            return GetTypeAttributes(typeof(TType));
        }

        public static List<Attribute> GetTypeAttributes(Type type)
        {
            return LockAndGetAttributes(type, tp => ((Type)tp).GetCustomAttributes(true));
        }

        public static List<TAttributeType> GetTypeAttributes<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
        {
            return
                GetTypeAttributes(type)
                    .Where<Attribute, TAttributeType>()
                    .Where(attr => predicate == null || predicate(attr))
                    .ToList();
        }

        public static List<TAttributeType> GetTypeAttributes<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
        {
            return GetTypeAttributes(typeof(TType), predicate);
        }

        public static TAttributeType GetTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
        {
            return
                GetTypeAttribute(typeof(TType), predicate);
        }

        public static TAttributeType GetTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
        {
            return
                GetTypeAttributes<TAttributeType>(type, predicate)
                    .FirstOrDefault();
        }

        public static bool HasTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
        {
            return HasTypeAttribute<TAttributeType>(typeof(TType), predicate);
        }

        public static bool HasTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
        {
            return GetTypeAttribute<TAttributeType>(type, predicate) != null;
        }

        // Members and properties
        public static List<Attribute> GetMemberAttributes<TType>(Expression<Func<TType, object>> action)
        {
            return GetMemberAttributes(GetMember(action));
        }

        public static List<TAttributeType> GetMemberAttributes<TType, TAttributeType>(
            Expression<Func<TType, object>> action,
            Func<TAttributeType, bool> predicate = null)
            where TAttributeType : Attribute
        {
            return GetMemberAttributes<TAttributeType>(GetMember(action), predicate);
        }

        public static TAttributeType GetMemberAttribute<TType, TAttributeType>(
            Expression<Func<TType, object>> action,
            Func<TAttributeType, bool> predicate = null)
            where TAttributeType : Attribute
        {
            return GetMemberAttribute<TAttributeType>(GetMember(action), predicate);
        }

        public static TAttributeType GetMemberAttribute<TAttributeType>(
            Expression<Func<object, object>> action,
            Func<TAttributeType, bool> predicate = null)
            where TAttributeType : Attribute
        {
            return GetMemberAttribute<TAttributeType>(GetMember(action), predicate);
        }

        public static bool HasMemberAttribute<TType, TAttributeType>(Expression<Func<TType, object>> action, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
        {
            return GetMemberAttribute(GetMember(action), predicate) != null;
        }

        // MemberInfo (and PropertyInfo since PropertyInfo inherits from MemberInfo)
        public static List<Attribute> GetMemberAttributes(this MemberInfo memberInfo)
        {
            return
                LockAndGetAttributes(memberInfo, mi => ((MemberInfo)mi).GetCustomAttributes(true));
        }

        public static List<TAttributeType> GetMemberAttributes<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
        {
            return
                GetMemberAttributes(memberInfo)
                    .Where<Attribute, TAttributeType>()
                    .Where(attr => predicate == null || predicate(attr))
                    .ToList();
        }

        public static MemberInfo GetEnumMemberInfo<TType>(TType value)
            where TType : struct, IConvertible
        {
            return value.GetType().GetMember(value.ToString())[0];
        }

        public static TAttributeType GetMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
        {
            return
                GetMemberAttributes<TAttributeType>(memberInfo, predicate)
                    .FirstOrDefault();
        }

        public static bool HasMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
        {
            return
                memberInfo.GetMemberAttribute<TAttributeType>(predicate) != null;
        }

        // Internal stuff
        private static IEnumerable<TType> Where<X, TType>(this IEnumerable<X> list)
        {
            return
                list
                    .Where(item => item is TType)
                    .Cast<TType>();
        }

        private static TType FirstOrDefault<X, TType>(this IEnumerable<X> list)
        {
            return
                list
                    .Where<X, TType>()
                    .FirstOrDefault();
        }

        private static List<Attribute> LockAndGetAttributes(object key, Func<object, object[]> retrieveValue)
        {
            return
                LockAndGet<object, List<Attribute>>(AttributeCache, key, mi => retrieveValue(mi).Cast<Attribute>().ToList());
        }

        // Method for thread safely executing slow method and storing the result in a dictionary
        private static TValue LockAndGet<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> retrieveValue)
        {
            TValue value = default(TValue);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }
            }

            value = retrieveValue(key);

            lock (dictionary)
            {
                if (dictionary.ContainsKey(key) == false)
                {
                    dictionary.Add(key, value);
                }

                return value;
            }
        }

        private static MemberInfo GetMember<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;

            if (memberExpression != null)
            {
                return memberExpression.Member;
            }

            UnaryExpression unaryExpression = expression.Body as UnaryExpression;

            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;

                if (memberExpression != null)
                {
                    return memberExpression.Member;
                }

                MethodCallExpression methodCall = unaryExpression.Operand as MethodCallExpression;
                if (methodCall != null)
                {
                    return methodCall.Method;
                }
            }

            return null;
        }
    }
}