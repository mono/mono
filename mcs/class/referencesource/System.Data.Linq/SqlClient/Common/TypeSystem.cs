using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Linq;

namespace System.Data.Linq.SqlClient {
    internal static class TypeSystem {

        internal static bool IsSequenceType(Type seqType) {
            return seqType != typeof(string)
                   && seqType != typeof(byte[])
                   && seqType != typeof(char[])
                   && FindIEnumerable(seqType) != null;
        }
        internal static bool HasIEnumerable(Type seqType) {
            return FindIEnumerable(seqType) != null;
        }
        private static Type FindIEnumerable(Type seqType) {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType) {
                foreach (Type arg in seqType.GetGenericArguments()) {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType)) {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0) {
                foreach (Type iface in ifaces) {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object)) {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
        internal static Type GetFlatSequenceType(Type elementType) {
            Type ienum = FindIEnumerable(elementType);
            if (ienum != null) return ienum;
            return typeof(IEnumerable<>).MakeGenericType(elementType);
        }
        internal static Type GetSequenceType(Type elementType) {
            return typeof(IEnumerable<>).MakeGenericType(elementType);
        }
        internal static Type GetElementType(Type seqType) {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }
        internal static bool IsNullableType(Type type) {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        internal static bool IsNullAssignable(Type type) {
            return !type.IsValueType || IsNullableType(type);
        }
        internal static Type GetNonNullableType(Type type) {
            if (IsNullableType(type)) {
                return type.GetGenericArguments()[0];
            }
            return type;
        }
        internal static Type GetMemberType(MemberInfo mi) {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            EventInfo ei = mi as EventInfo;
            if (ei != null) return ei.EventHandlerType;
            return null;
        }
        internal static IEnumerable<FieldInfo> GetAllFields(Type type, BindingFlags flags) {
            Dictionary<MetaPosition, FieldInfo> seen = new Dictionary<MetaPosition, FieldInfo>();
            Type currentType = type;
            do {
                foreach (FieldInfo fi in currentType.GetFields(flags)) {
                    if (fi.IsPrivate || type == currentType) {
                        MetaPosition mp = new MetaPosition(fi);
                        seen[mp] = fi;
                    }
                }
                currentType = currentType.BaseType;
            } while (currentType != null);
            return seen.Values;
        }
        internal static IEnumerable<PropertyInfo> GetAllProperties(Type type, BindingFlags flags) {
            Dictionary<MetaPosition, PropertyInfo> seen = new Dictionary<MetaPosition, PropertyInfo>();
            Type currentType = type;
            do {
                foreach (PropertyInfo pi in currentType.GetProperties(flags)) {
                    if (type == currentType || IsPrivate(pi)) {
                        MetaPosition mp = new MetaPosition(pi);
                        seen[mp] = pi;
                    }
                }
                currentType = currentType.BaseType;
            } while (currentType != null);
            return seen.Values;
        }

        private static bool IsPrivate(PropertyInfo pi) {
            MethodInfo mi = pi.GetGetMethod() ?? pi.GetSetMethod();
            if (mi != null) {
                return mi.IsPrivate;
            }
            return true;
        }

        private static ILookup<string, MethodInfo> _sequenceMethods;
        internal static MethodInfo FindSequenceMethod(string name, Type[] args, params Type[] typeArgs) {
            if (_sequenceMethods == null) {
                _sequenceMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).ToLookup(m => m.Name);
            }
            MethodInfo mi = _sequenceMethods[name].FirstOrDefault(m => ArgsMatchExact(m, args, typeArgs));
            if (mi == null)
                return null;
            if (typeArgs != null)
                return mi.MakeGenericMethod(typeArgs);
            return mi;
        }
        internal static MethodInfo FindSequenceMethod(string name, IEnumerable sequence) {
            return FindSequenceMethod(name, new Type[] {sequence.GetType()}, new Type[] {GetElementType(sequence.GetType())});
        }

        private static ILookup<string, MethodInfo> _queryMethods;
        internal static MethodInfo FindQueryableMethod(string name, Type[] args, params Type[] typeArgs) {
            if (_queryMethods == null) {
                _queryMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).ToLookup(m => m.Name);
            }
            MethodInfo mi = _queryMethods[name].FirstOrDefault(m => ArgsMatchExact(m, args, typeArgs));
            if (mi == null)
                throw Error.NoMethodInTypeMatchingArguments(typeof(Queryable));
            if (typeArgs != null)
                return mi.MakeGenericMethod(typeArgs);
            return mi;
        }

        internal static MethodInfo FindStaticMethod(Type type, string name, Type[] args, params Type[] typeArgs) {
            MethodInfo mi = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == name && ArgsMatchExact(m, args, typeArgs));
            if (mi == null)
                throw Error.NoMethodInTypeMatchingArguments(type);
            if (typeArgs != null)
                return mi.MakeGenericMethod(typeArgs);
            return mi;
        }

        private static bool ArgsMatchExact(MethodInfo m, Type[] argTypes, Type[] typeArgs) {
            ParameterInfo[] mParams = m.GetParameters();
            if (mParams.Length != argTypes.Length)
                return false;
            if (!m.IsGenericMethodDefinition && m.IsGenericMethod && m.ContainsGenericParameters) {
                m = m.GetGenericMethodDefinition();
            }
            if (m.IsGenericMethodDefinition) {
                if (typeArgs == null || typeArgs.Length == 0)
                    return false;
                if (m.GetGenericArguments().Length != typeArgs.Length)
                    return false;
                m = m.MakeGenericMethod(typeArgs);
                mParams = m.GetParameters();
            }
            else if (typeArgs != null && typeArgs.Length > 0) {
                return false;
            }
            for (int i = 0, n = argTypes.Length; i < n; i++) {
                Type parameterType = mParams[i].ParameterType;
                if (parameterType == null)
                    return false;
                Type argType = argTypes[i];
                if (!parameterType.IsAssignableFrom(argType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the type is one of the built in simple types.
        /// </summary>
        internal static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];

            if (type.IsEnum)
                return true;

            if (type == typeof(Guid))
                return true;

            TypeCode tc = Type.GetTypeCode(type);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                    return true;
                case TypeCode.Object:
                    return (typeof(TimeSpan) == type) || (typeof(DateTimeOffset) == type);
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Hashable MetaDataToken+Assembly. This type uniquely describes a metadata element
    /// like a MemberInfo. MetaDataToken by itself is not sufficient because its only
    /// unique within a single assembly.
    /// </summary>
    internal struct MetaPosition : IEqualityComparer<MetaPosition>, IEqualityComparer {
        private int metadataToken;
        private Assembly assembly;
        internal MetaPosition(MemberInfo mi)
            : this(mi.DeclaringType.Assembly, mi.MetadataToken) {
        }
        private MetaPosition(Assembly assembly, int metadataToken) {
            this.assembly = assembly;
            this.metadataToken = metadataToken;
        }

        // Equality is implemented here according to the advice in
        // CLR via C# 2ed, J. Richter, p 146. In particular, ValueType.Equals
        // should not be called for perf reasons.

        #region Object Members
        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            if (obj.GetType() != this.GetType())
                return false;

            return AreEqual(this, (MetaPosition)obj);
        }
        public override int GetHashCode() {
            return metadataToken;
        }
        #endregion

        #region IEqualityComparer<MetaPosition> Members
        public bool Equals(MetaPosition x, MetaPosition y) {
            return AreEqual(x, y);
        }

        public int GetHashCode(MetaPosition obj) {
            return obj.metadataToken;
        }
        #endregion

        #region IEqualityComparer Members
        bool IEqualityComparer.Equals(object x, object y) {
            return this.Equals((MetaPosition)x, (MetaPosition)y);
        }
        int IEqualityComparer.GetHashCode(object obj) {
            return this.GetHashCode((MetaPosition) obj);
        }
        #endregion

        private static bool AreEqual(MetaPosition x, MetaPosition y) {
            return (x.metadataToken == y.metadataToken)
                && (x.assembly == y.assembly);
        }

        // Since MetaPositions are immutable, we overload the equality operator
        // to test for value equality, rather than reference equality
        public static bool operator==(MetaPosition x, MetaPosition y) {
            return AreEqual(x, y);
        }
        public static bool operator !=(MetaPosition x, MetaPosition y) {
            return !AreEqual(x, y);
        }

        internal static bool AreSameMember(MemberInfo x, MemberInfo y) {
            if (x.MetadataToken != y.MetadataToken || x.DeclaringType.Assembly != y.DeclaringType.Assembly) {
                return false;
            }
            return true;
        }
    }
}
