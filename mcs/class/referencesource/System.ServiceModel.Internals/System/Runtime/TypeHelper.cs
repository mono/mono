//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    static class TypeHelper
    {
        public static readonly Type ArrayType = typeof(Array);
        public static readonly Type BoolType = typeof(bool);
        public static readonly Type GenericCollectionType = typeof(ICollection<>);
        public static readonly Type ByteType = typeof(byte);
        public static readonly Type SByteType = typeof(sbyte);
        public static readonly Type CharType = typeof(char);
        public static readonly Type ShortType = typeof(short);
        public static readonly Type UShortType = typeof(ushort);
        public static readonly Type IntType = typeof(int);
        public static readonly Type UIntType = typeof(uint);
        public static readonly Type LongType = typeof(long);
        public static readonly Type ULongType = typeof(ulong);
        public static readonly Type FloatType = typeof(float);
        public static readonly Type DoubleType = typeof(double);
        public static readonly Type DecimalType = typeof(decimal);
        public static readonly Type ExceptionType = typeof(Exception);
        public static readonly Type NullableType = typeof(Nullable<>);
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type StringType = typeof(string);
        public static readonly Type TypeType = typeof(Type);
        public static readonly Type VoidType = typeof(void);

        public static bool AreTypesCompatible(object source, Type destinationType)
        {
            if (source == null)
            {
                return !destinationType.IsValueType || IsNullableType(destinationType);
            }

            return AreTypesCompatible(source.GetType(), destinationType);
        }

        // return true if the sourceType is implicitly convertible to the destinationType
        public static bool AreTypesCompatible(Type sourceType, Type destinationType)
        {
            if (object.ReferenceEquals(sourceType, destinationType))
            {
                return true;
            }

            return IsImplicitNumericConversion(sourceType, destinationType) ||
                IsImplicitReferenceConversion(sourceType, destinationType) ||
                IsImplicitBoxingConversion(sourceType, destinationType) ||
                IsImplicitNullableConversion(sourceType, destinationType);
        }

        // simpler, more performant version of AreTypesCompatible when
        // we know both sides are reference types
        public static bool AreReferenceTypesCompatible(Type sourceType, Type destinationType)
        {
            Fx.Assert(!sourceType.IsValueType && !destinationType.IsValueType, "AreReferenceTypesCompatible can only be used for reference types");
            if (object.ReferenceEquals(sourceType, destinationType))
            {
                return true;
            }

            return IsImplicitReferenceConversion(sourceType, destinationType);
        }

        // variation to OfType<T> that uses AreTypesCompatible instead of Type equality
        public static IEnumerable<Type> GetCompatibleTypes(IEnumerable<Type> enumerable, Type targetType)
        {
            foreach (Type sourceType in enumerable)
            {
                if (TypeHelper.AreTypesCompatible(sourceType, targetType))
                {
                    yield return sourceType;
                }
            }
        }

        public static bool ContainsCompatibleType(IEnumerable<Type> enumerable, Type targetType)
        {
            foreach (Type sourceType in enumerable)
            {
                if (TypeHelper.AreTypesCompatible(sourceType, targetType))
                {
                    return true;
                }
            }

            return false;
        }

        // handles not only the simple cast, but also value type widening, etc.
        public static T Convert<T>(object source)
        {
            // first check the common cases
            if (source is T)
            {
                return (T)source;
            }

            if (source == null)
            {
                if (typeof(T).IsValueType && !IsNullableType(typeof(T)))
                {
                    throw Fx.Exception.AsError(new InvalidCastException(InternalSR.CannotConvertObject(source, typeof(T))));
                }

                return default(T);
            }

            T result;
            if (TryNumericConversion<T>(source, out result))
            {
                return result;
            }

            throw Fx.Exception.AsError(new InvalidCastException(InternalSR.CannotConvertObject(source, typeof(T))));
        }

        // get all of the types that this Type implements (based classes, interfaces, etc)
        public static IEnumerable<Type> GetImplementedTypes(Type type)
        {
            Dictionary<Type, object> typesEncountered = new Dictionary<Type, object>();

            GetImplementedTypesHelper(type, typesEncountered);
            return typesEncountered.Keys;
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "typesEncountered", Justification = "No need to support type equivalence here.")]
        static void GetImplementedTypesHelper(Type type, Dictionary<Type, object> typesEncountered)
        {
            if (typesEncountered.ContainsKey(type))
            {
                return;
            }

            typesEncountered.Add(type, type);

            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; ++i)
            {
                GetImplementedTypesHelper(interfaces[i], typesEncountered);
            }

            Type baseType = type.BaseType;
            while ((baseType != null) && (baseType != TypeHelper.ObjectType))
            {
                GetImplementedTypesHelper(baseType, typesEncountered);
                baseType = baseType.BaseType;
            }
        }

        [SuppressMessage(FxCop.Category.Maintainability, FxCop.Rule.AvoidExcessiveComplexity,
            Justification = "Need to check all possible numeric conversions")]
        static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode sourceTypeCode = Type.GetTypeCode(source);
            TypeCode destinationTypeCode = Type.GetTypeCode(destination);

            switch (sourceTypeCode)
            {
                case TypeCode.SByte:
                    switch (destinationTypeCode)
                    {
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
                    switch (destinationTypeCode)
                    {
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
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (destinationTypeCode)
                    {
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
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Char:
                    switch (destinationTypeCode)
                    {
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
                    return (destinationTypeCode == TypeCode.Double);
            }

            return false;
        }

        static bool IsImplicitReferenceConversion(Type sourceType, Type destinationType)
        {
            return destinationType.IsAssignableFrom(sourceType);
        }

        static bool IsImplicitBoxingConversion(Type sourceType, Type destinationType)
        {
            if (sourceType.IsValueType && (destinationType == ObjectType || destinationType == typeof(ValueType)))
            {
                return true;
            }
            if (sourceType.IsEnum && destinationType == typeof(Enum))
            {
                return true;
            }
            return false;
        }

        static bool IsImplicitNullableConversion(Type sourceType, Type destinationType)
        {
            if (!IsNullableType(destinationType))
            {
                return false;
            }

            destinationType = destinationType.GetGenericArguments()[0];
            if (IsNullableType(sourceType))
            {
                sourceType = sourceType.GetGenericArguments()[0];
            }
            return AreTypesCompatible(sourceType, destinationType);
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == NullableType;
        }

        static bool TryNumericConversion<T>(object source, out T result)
        {
            Fx.Assert(source != null, "caller must verify");
            TypeCode sourceTypeCode = Type.GetTypeCode(source.GetType());
            TypeCode destinationTypeCode = Type.GetTypeCode(typeof(T));

            switch (sourceTypeCode)
            {
                case TypeCode.SByte:
                    {
                        SByte sbyteSource = (SByte)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Int16:
                                result = (T)(object)(Int16)sbyteSource;
                                return true;
                            case TypeCode.Int32:
                                result = (T)(object)(Int32)sbyteSource;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)sbyteSource;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)sbyteSource;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)sbyteSource;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)sbyteSource;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Byte:
                    {
                        Byte byteSource = (Byte)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Int16:
                                result = (T)(object)(Int16)byteSource;
                                return true;
                            case TypeCode.UInt16:
                                result = (T)(object)(UInt16)byteSource;
                                return true;
                            case TypeCode.Int32:
                                result = (T)(object)(Int32)byteSource;
                                return true;
                            case TypeCode.UInt32:
                                result = (T)(object)(UInt32)byteSource;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)byteSource;
                                return true;
                            case TypeCode.UInt64:
                                result = (T)(object)(UInt64)byteSource;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)byteSource;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)byteSource;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)byteSource;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Int16:
                    {
                        Int16 int16Source = (Int16)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Int32:
                                result = (T)(object)(Int32)int16Source;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)int16Source;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)int16Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)int16Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)int16Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        UInt16 uint16Source = (UInt16)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Int32:
                                result = (T)(object)(Int32)uint16Source;
                                return true;
                            case TypeCode.UInt32:
                                result = (T)(object)(UInt32)uint16Source;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)uint16Source;
                                return true;
                            case TypeCode.UInt64:
                                result = (T)(object)(UInt64)uint16Source;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)uint16Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)uint16Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)uint16Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Int32:
                    {
                        Int32 int32Source = (Int32)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)int32Source;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)int32Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)int32Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)int32Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        UInt32 uint32Source = (UInt32)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.UInt32:
                                result = (T)(object)(UInt32)uint32Source;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)uint32Source;
                                return true;
                            case TypeCode.UInt64:
                                result = (T)(object)(UInt64)uint32Source;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)uint32Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)uint32Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)uint32Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Int64:
                    {
                        Int64 int64Source = (Int64)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Single:
                                result = (T)(object)(Single)int64Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)int64Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)int64Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        UInt64 uint64Source = (UInt64)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.Single:
                                result = (T)(object)(Single)uint64Source;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)uint64Source;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)uint64Source;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Char:
                    {
                        Char charSource = (Char)source;
                        switch (destinationTypeCode)
                        {
                            case TypeCode.UInt16:
                                result = (T)(object)(UInt16)charSource;
                                return true;
                            case TypeCode.Int32:
                                result = (T)(object)(Int32)charSource;
                                return true;
                            case TypeCode.UInt32:
                                result = (T)(object)(UInt32)charSource;
                                return true;
                            case TypeCode.Int64:
                                result = (T)(object)(Int64)charSource;
                                return true;
                            case TypeCode.UInt64:
                                result = (T)(object)(UInt64)charSource;
                                return true;
                            case TypeCode.Single:
                                result = (T)(object)(Single)charSource;
                                return true;
                            case TypeCode.Double:
                                result = (T)(object)(Double)charSource;
                                return true;
                            case TypeCode.Decimal:
                                result = (T)(object)(Decimal)charSource;
                                return true;
                        }
                        break;
                    }
                case TypeCode.Single:
                    {
                        if (destinationTypeCode == TypeCode.Double)
                        {
                            result = (T)(object)(Double)(Single)source;
                            return true;
                        }
                        break;
                    }
            }

            result = default(T);
            return false;
        }

        public static object GetDefaultValueForType(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            if (type.IsEnum)
            {
                Array enumValues = Enum.GetValues(type);
                if (enumValues.Length > 0)
                {
                    return enumValues.GetValue(0);
                }
            }

            return Activator.CreateInstance(type);
        }

        public static bool IsNullableValueType(Type type)
        {
            return type.IsValueType && IsNullableType(type);
        }

        public static bool IsNonNullableValueType(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            if (type.IsGenericType)
            {
                return false;
            }

            return type != StringType;
        }

        public static bool ShouldFilterProperty(PropertyDescriptor property, Attribute[] attributes)
        {
            if (attributes == null || attributes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < attributes.Length; i++)
            {
                Attribute filterAttribute = attributes[i];
                Attribute propertyAttribute = property.Attributes[filterAttribute.GetType()];
                if (propertyAttribute == null)
                {
                    if (!filterAttribute.IsDefaultAttribute())
                    {
                        return true;
                    }
                }
                else
                {
                    if (!filterAttribute.Match(propertyAttribute))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
