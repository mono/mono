using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions
{
    static class ExpressionUtil
    {
        private static TypeCode GetTypeCode(Type type)
        {
            if (Expression.IsNullableType(type))
                type = Expression.GetNonNullableType(type);

            return Type.GetTypeCode(type);
        }

        private static bool IsNumber(TypeCode typeCode)
        {
            if (IsInteger(typeCode) ||
                typeCode == TypeCode.Single ||
                typeCode == TypeCode.Double ||
                typeCode == TypeCode.Decimal)
                return true;

                return false;
        }

        private static bool IsInteger(TypeCode typeCode)
        {
            if (typeCode == TypeCode.Byte ||
                typeCode == TypeCode.SByte ||
                typeCode == TypeCode.Int16 ||
                typeCode == TypeCode.UInt16 ||
                typeCode == TypeCode.Int32 ||
                typeCode == TypeCode.UInt32 ||
                typeCode == TypeCode.Int64 ||
                typeCode == TypeCode.UInt64)
                return true;

            return false;
        }

        public static bool IsNumber(Type type)
        {
            // fast exit: enum is not numeric
            if (type.IsEnum)
                return false;

            return IsNumber(GetTypeCode(type));
        }

        public static bool IsInteger(Type type)
        {
            // fast exit: enum is not numeric
            if (type.IsEnum)
                return false;

            return IsInteger(GetTypeCode(type));
        }

        public static bool IsNumber(object value)
        {
            if (value == null)
                return false;

            return IsNumber(value.GetType());
        }

        public static bool IsInteger(object value)
        {
            if (value == null)
                return false;

            return IsInteger(value.GetType());
        }

        /// <summary>
        /// tries to find the method with the given name in one of the
        /// given types in the array.
        /// </summary>
        /// <param name="methodName">The name of the method to find.</param>
        /// <param name="types">An array of <see cref="Type"/> (typically 2) defining
        /// the types we're going to check for the method.</param>
        /// <returns></returns>
        public static MethodInfo GetMethod(string methodName, Type[] types)
        {
            if (types.Length > 2)
                throw new ArgumentException();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            // the method should have the two types defined as argument...
            MethodInfo method = types[0].GetMethod(methodName, flags, null, types, null);
            if (method == null && types.Length > 1)
                method = types[1].GetMethod(methodName, flags, null, types, null);

            return method;
        }

        public static MethodInfo GetOperatorMethod(string methodName, Type left, Type right)
        {
            MethodInfo opMethod = GetMethod(methodName, new Type[] { left, right });
            if (opMethod == null)
            {
                if (Expression.IsNullableType(left) &&
                    Expression.IsNullableType(right))
                {
                    // they're nullable types: try to refine them and get the method again...
                    left = Expression.GetNonNullableType(left);
                    right = Expression.GetNonNullableType(right);
                    opMethod = GetMethod(methodName, new Type[] { left, right });
                }

                // hope at this point we've found it...
                return opMethod;
            }

            if (!opMethod.IsStatic)
                throw new ArgumentException();
            if (opMethod.ReturnType == typeof(void))
                throw new ArgumentException();

            ParameterInfo[] pars = opMethod.GetParameters();
            if (pars.Length != 2)
                throw new ArgumentException();

            ParameterInfo p1 = pars[0];
            ParameterInfo p2 = pars[1];
            if (p1.ParameterType.IsAssignableFrom(left))
                throw new ArgumentException();
            if (p2.ParameterType.IsAssignableFrom (right))
                throw new ArgumentException();

            // check for the nullability of the types...
            if (Expression.IsNullableType(p1.ParameterType) &&
                !Expression.IsNullableType(left))
                throw new InvalidOperationException();

            if (Expression.IsNullableType(p2.ParameterType) &&
                !Expression.IsNullableType(right))
                throw new InvalidOperationException();

            return opMethod;
        }

        public static Type GetNullable(Type type)
        {
            if (Expression.IsNullableType(type))
                return type;
            return typeof(Nullable<>).MakeGenericType(type);
        }
    }
}
