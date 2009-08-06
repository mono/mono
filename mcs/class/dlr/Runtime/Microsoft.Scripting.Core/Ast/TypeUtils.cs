/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Reflection;

#if SILVERLIGHT
using System.Core;
#endif

#if CODEPLEX_40
namespace System.Dynamic.Utils {
#else
namespace Microsoft.Scripting.Utils {
#endif

    internal static class TypeUtils {
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        internal const MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;

        internal static Type GetNonNullableType(this Type type) {
            if (IsNullableType(type)) {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static Type GetNullableType(Type type) {
            Debug.Assert(type != null, "type cannot be null");
            if (type.IsValueType && !IsNullableType(type)) {
                return typeof(Nullable<>).MakeGenericType(type);
            }
            return type;
        }

        internal static bool IsNullableType(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool IsBool(Type type) {
            return GetNonNullableType(type) == typeof(bool);
        }

        internal static bool IsNumeric(Type type) {
            type = GetNonNullableType(type);
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsInteger(Type type) {
            type = GetNonNullableType(type);
            if (type.IsEnum) {
                return false;
            }
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }


        internal static bool IsArithmetic(Type type) {
            type = GetNonNullableType(type);
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsUnsignedInt(Type type) {
            type = GetNonNullableType(type);
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsIntegerOrBool(Type type) {
            type = GetNonNullableType(type);
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Int64:
                    case TypeCode.Int32:
                    case TypeCode.Int16:
                    case TypeCode.UInt64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt16:
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        return true;
                }
            }
            return false;
        }

        internal static bool AreEquivalent(Type t1, Type t2)
        {
#if MICROSOFT_SCRIPTING_CORE || SILVERLIGHT
            return t1 == t2;
#else
            return t1 == t2 || t1.IsEquivalentTo(t2);
#endif
        }

        internal static bool AreReferenceAssignable(Type dest, Type src) {
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            if (AreEquivalent(dest, src)) {
                return true;
            }
            if (!dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src)) {
                return true;
            }
            return false;
        }

        // Checks if the type is a valid target for an instance call
        internal static bool IsValidInstanceType(MemberInfo member, Type instanceType) {
            Type targetType = member.DeclaringType;
            if (AreReferenceAssignable(targetType, instanceType)) {
                return true;
            }
            if (instanceType.IsValueType) {
                if (AreReferenceAssignable(targetType, typeof(System.Object))) {
                    return true;
                }
                if (AreReferenceAssignable(targetType, typeof(System.ValueType))) {
                    return true;
                }
                if (instanceType.IsEnum && AreReferenceAssignable(targetType, typeof(System.Enum))) {
                    return true;
                }
                // A call to an interface implemented by a struct is legal whether the struct has
                // been boxed or not.
                if (targetType.IsInterface) {
                    foreach (Type interfaceType in instanceType.GetInterfaces()) {
                        if (AreReferenceAssignable(targetType, interfaceType)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static bool HasIdentityPrimitiveOrNullableConversion(Type source, Type dest) {
            Debug.Assert(source != null && dest != null);

            // Identity conversion
            if (AreEquivalent(source, dest)) {
                return true;
            }

            // Nullable conversions
            if (IsNullableType(source) && AreEquivalent(dest, GetNonNullableType(source))) {
                return true;
            }
            if (IsNullableType(dest) && AreEquivalent(source, GetNonNullableType(dest))) {
                return true;
            }
            // Primitive runtime conversions
            // All conversions amongst enum, bool, char, integer and float types
            // (and their corresponding nullable types) are legal except for
            // nonbool==>bool and nonbool==>bool?
            // Since we have already covered bool==>bool, bool==>bool?, etc, above,
            // we can just disallow having a bool or bool? destination type here.
            if (IsConvertible(source) && IsConvertible(dest) && GetNonNullableType(dest) != typeof(bool)) {
                return true;
            }
            return false;
        }

        internal static bool HasReferenceConversion(Type source, Type dest) {
            Debug.Assert(source != null && dest != null);

            // void -> void conversion is handled elsewhere
            // (it's an identity conversion)
            // All other void conversions are disallowed.
            if (source == typeof(void) || dest == typeof(void)) {
                return false;
            }

            Type nnSourceType = TypeUtils.GetNonNullableType(source);
            Type nnDestType = TypeUtils.GetNonNullableType(dest);

            // Down conversion
            if (nnSourceType.IsAssignableFrom(nnDestType)) {
                return true;
            }
            // Up conversion
            if (nnDestType.IsAssignableFrom(nnSourceType)) {
                return true;
            }
            // Interface conversion
            if (source.IsInterface || dest.IsInterface) {
                return true;
            }
            // Object conversion
            if (source == typeof(object) || dest == typeof(object)) {
                return true;
            }
            return false;
        }

        internal static bool IsConvertible(Type type) {
            type = GetNonNullableType(type);
            if (type.IsEnum) {
                return true;
            }
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
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
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool HasReferenceEquality(Type left, Type right) {
            if (left.IsValueType || right.IsValueType) {
                return false;
            }

            // If we have an interface and a reference type then we can do 
            // reference equality.

            // If we have two reference types and one is assignable to the
            // other then we can do reference equality.

            return left.IsInterface || right.IsInterface ||
                AreReferenceAssignable(left, right) ||
                AreReferenceAssignable(right, left);
        }

        internal static bool HasBuiltInEqualityOperator(Type left, Type right) {
            // If we have an interface and a reference type then we can do 
            // reference equality.
            if (left.IsInterface && !right.IsValueType) {
                return true;
            }
            if (right.IsInterface && !left.IsValueType) {
                return true;
            }
            // If we have two reference types and one is assignable to the
            // other then we can do reference equality.
            if (!left.IsValueType && !right.IsValueType) {
                if (AreReferenceAssignable(left, right) || AreReferenceAssignable(right, left)) {
                    return true;
                }
            }
            // Otherwise, if the types are not the same then we definitely 
            // do not have a built-in equality operator.
            if (!AreEquivalent(left, right)) {
                return false;
            }
            // We have two identical value types, modulo nullability.  (If they were both the 
            // same reference type then we would have returned true earlier.)
            Debug.Assert(left.IsValueType);
            // Equality between struct types is only defined for numerics, bools, enums,
            // and their nullable equivalents.
            Type nnType = GetNonNullableType(left);
            if (nnType == typeof(bool) || IsNumeric(nnType) || nnType.IsEnum) {
                return true;
            }
            return false;
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination) {
            return AreEquivalent(source, destination) ||                // identity conversion
                IsImplicitNumericConversion(source, destination) ||
                IsImplicitReferenceConversion(source, destination) ||
                IsImplicitBoxingConversion(source, destination) ||
                IsImplicitNullableConversion(source, destination);
        }


        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly) {
            // check for implicit coercions first
            Type nnExprType = TypeUtils.GetNonNullableType(convertFrom);
            Type nnConvType = TypeUtils.GetNonNullableType(convertToType);
            // try exact match on types
            MethodInfo[] eMethods = nnExprType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo method = FindConversionOperator(eMethods, convertFrom, convertToType, implicitOnly);
            if (method != null) {
                return method;
            }
            MethodInfo[] cMethods = nnConvType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method = FindConversionOperator(cMethods, convertFrom, convertToType, implicitOnly);
            if (method != null) {
                return method;
            }
            // try lifted conversion
            if (!TypeUtils.AreEquivalent(nnExprType, convertFrom) ||
                !TypeUtils.AreEquivalent(nnConvType, convertToType)) {
                method = FindConversionOperator(eMethods, nnExprType, nnConvType, implicitOnly);
                if (method == null) {
                    method = FindConversionOperator(cMethods, nnExprType, nnConvType, implicitOnly);
                }
                if (method != null) {
                    return method;
                }
            }
            return null;
        }

        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly) {
            foreach (MethodInfo mi in methods) {
                if (mi.Name != "op_Implicit" && (implicitOnly || mi.Name != "op_Explicit")) {
                    continue;
                }
                if (!TypeUtils.AreEquivalent(mi.ReturnType, typeTo)) {
                    continue;
                }
                ParameterInfo[] pis = mi.GetParametersCached();
                if (!TypeUtils.AreEquivalent(pis[0].ParameterType, typeFrom)) {
                    continue;
                }
                return mi;
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool IsImplicitNumericConversion(Type source, Type destination) {
            TypeCode tcSource = Type.GetTypeCode(source);
            TypeCode tcDest = Type.GetTypeCode(destination);

            switch (tcSource) {
                case TypeCode.SByte:
                    switch (tcDest) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch (tcDest) {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int16:
                    switch (tcDest) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (tcDest) {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int32:
                    switch (tcDest) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (tcDest) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (tcDest) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Char:
                    switch (tcDest) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Single:
                    return (tcDest == TypeCode.Double);
            }
            return false;
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination) {
            return destination.IsAssignableFrom(source);
        }

        private static bool IsImplicitBoxingConversion(Type source, Type destination) {
            if (source.IsValueType && (destination == typeof(object) || destination == typeof(System.ValueType)))
                return true;
            if (source.IsEnum && destination == typeof(System.Enum))
                return true;
            return false;
        }

        private static bool IsImplicitNullableConversion(Type source, Type destination) {
            if (IsNullableType(destination))
                return IsImplicitlyConvertible(GetNonNullableType(source), GetNonNullableType(destination));
            return false;
        }

        internal static bool IsSameOrSubclass(Type type, Type subType) {
            return AreEquivalent(type, subType) || subType.IsSubclassOf(type);
        }

        internal static void ValidateType(Type type) {
            if (type.IsGenericTypeDefinition) {
                throw Error.TypeIsGeneric(type);
            }
            if (type.ContainsGenericParameters) {
                throw Error.TypeContainsGenericParameters(type);
            }
        }

        //from TypeHelper
        internal static Type FindGenericType(Type definition, Type type) {
            while (type != null && type != typeof(object)) {
                if (type.IsGenericType && AreEquivalent(type.GetGenericTypeDefinition(), definition)) {
                    return type;
                }
                if (definition.IsInterface) {
                    foreach (Type itype in type.GetInterfaces()) {
                        Type found = FindGenericType(definition, itype);
                        if (found != null)
                            return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static bool IsUnsigned(Type type) {
            type = GetNonNullableType(type);
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Char:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsFloatingPoint(Type type) {
            type = GetNonNullableType(type);
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Searches for an operator method on the type. The method must have
        /// the specified signature, no generic arguments, and have the
        /// SpecialName bit set. Also searches inherited operator methods.
        /// 
        /// NOTE: This was designed to satisfy the needs of op_True and
        /// op_False, because we have to do runtime lookup for those. It may
        /// not work right for unary operators in general.
        /// </summary>
        internal static MethodInfo GetBooleanOperator(Type type, string name) {
            do {
                MethodInfo result = type.GetMethodValidated(name, AnyStatic, null, new Type[] { type }, null);
                if (result != null && result.IsSpecialName && !result.ContainsGenericParameters) {
                    return result;
                }
                type = type.BaseType;
            } while (type != null);
            return null;
        }

        internal static Type GetNonRefType(this Type type) {
            return type.IsByRef ? type.GetElementType() : type;
        }

        private static readonly Assembly _mscorlib = typeof(object).Assembly;
        private static readonly Assembly _systemCore = typeof(Expression).Assembly;

        /// <summary>
        /// We can cache references to types, as long as they aren't in
        /// collectable assemblies. Unfortunately, we can't really distinguish
        /// between different flavors of assemblies. But, we can at least
        /// create a whitelist for types in mscorlib (so we get the primitives)
        /// and System.Core (so we find Func/Action overloads, etc).
        /// </summary>
        internal static bool CanCache(this Type t) {
            // Note: we don't have to scan base or declaring types here.
            // There's no way for a type in mscorlib to derive from or be
            // contained in a type from another assembly. The only thing we
            // need to look at is the generic arguments, which are the thing
            // that allows mscorlib types to be specialized by types in other
            // assemblies.

            var asm = t.Assembly;
            if (asm != _mscorlib && asm != _systemCore) {
                // Not in mscorlib or our assembly
                return false;
            }

            if (t.IsGenericType) {
                foreach (Type g in t.GetGenericArguments()) {
                    if (!CanCache(g)) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
