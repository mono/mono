using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions
{
    static class ExpressionUtil
    {
        public static bool IsNumber(Type type)
        {
            // enum can also be in a numeric form, but we
            // want it to be considered as a number?
            if (type.IsEnum)
                return false;

            TypeCode typeCode = Type.GetTypeCode(type);
            if (typeCode == TypeCode.Byte ||
                typeCode == TypeCode.Decimal ||
                typeCode == TypeCode.Double ||
                typeCode == TypeCode.Int16 ||
                typeCode == TypeCode.Int32 ||
                typeCode == TypeCode.Int64 ||
                typeCode == TypeCode.Single)
                return true;

            return false;
        }

        public static bool IsNumber(object value)
        {
            if (value == null)
                return false;
            return IsNumber(value.GetType());
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

        public static ReadOnlyCollection<MemberBinding> GetReadOnlyCollection(IEnumerable<MemberBinding> en)
        {
            if (en == null)
                return new ReadOnlyCollection<MemberBinding>(new MemberBinding[0]);
            
            List<MemberBinding> list = new List<MemberBinding>(en);
            return new ReadOnlyCollection<MemberBinding>(list);
        }

        public static ReadOnlyCollection<Expression> GetReadOnlyCollection(IEnumerable<Expression> en)
        {
            if (en == null)
                return new ReadOnlyCollection<Expression>(new Expression[0]);
            
            List<Expression> list = new List<Expression>(en);
            return new ReadOnlyCollection<Expression>(list);
        }
    }
}
