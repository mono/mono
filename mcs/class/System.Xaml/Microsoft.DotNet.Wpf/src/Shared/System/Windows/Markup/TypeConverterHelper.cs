// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Description: Specifies that the whitespace surrounding an element should be trimmed.
//

using System;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
#if SYSTEM_XAML
using System.Xaml.Replacements;
#endif

#if PBTCOMPILER
namespace MS.Internal.Markup
#elif SYSTEM_XAML
namespace System.Xaml
#else
namespace System.Windows.Markup
#endif
{

    /// <summary>
    /// Class that provides functionality to obtain a TypeConverter from a property or the
    /// type of the property, based on logic similar to TypeDescriptor.GetConverter.
    /// </summary>
    internal static class TypeConverterHelper
    {
        private static CultureInfo invariantEnglishUS = CultureInfo.InvariantCulture;

        internal static CultureInfo InvariantEnglishUS
        {
            get
            {
                return invariantEnglishUS;
            }
        }

#if !SYSTEM_XAML
        internal static MemberInfo GetMemberInfoForPropertyConverter(object dpOrPiOrMi)
        {
            MemberInfo memberInfo = dpOrPiOrMi as PropertyInfo;

            if (memberInfo == null)
            {
                MethodInfo methodInfo;
#if !PBTCOMPILER
                DependencyProperty dp = dpOrPiOrMi as DependencyProperty;

                if (dp != null)
                {
                    // While parsing styles or templates, we end up getting a DependencyProperty,
                    // even for non-attached cases. In this case, we try fetching the CLR
                    // property info and getting its attributes.
                    memberInfo = dp.OwnerType.GetProperty(
                                 dp.Name,
                                 BindingFlags.Instance | BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                    // We failed to get a CLR wrapper for the DependencyProperty, so we
                    // assume that this is an attached property and look for the MethodInfo
                    // for the static getter.
                    if (memberInfo == null)
                    {
                        // Get the method member that defines the DependencyProperty
                        memberInfo = dp.OwnerType.GetMethod(
                                     "Get" + dp.Name,
                                     BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    }
                }
                else
#endif
                if ((methodInfo = dpOrPiOrMi as MethodInfo) != null)
                {
                    // miSetter may not be a MethodInfo when we are dealing with event handlers that
                    // belong to local assemblies. One such case is encountered when building DrtCompiler.
                    if (methodInfo.GetParameters().Length == 1)
                    {
                        // Use Getter of the attached property
                        memberInfo = methodInfo;
                    }
                    else
                    {
                        // Use the Setter of the attached property (if any)
                        memberInfo = methodInfo.DeclaringType.GetMethod(
                             "Get" + methodInfo.Name.Substring("Set".Length),
                             BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    }
                }
            }

            return memberInfo;
        }

        internal static Type GetConverterType(MemberInfo memberInfo)
        {
            Debug.Assert(null != memberInfo, "Null passed for memberInfo to GetConverterType");

            Type converterType = null;

            // Try looking for the TypeConverter for the type using reflection.
            string converterName = ReflectionHelper.GetTypeConverterAttributeData(memberInfo, out converterType);

            if (converterType == null)
            {
                converterType = GetConverterTypeFromName(converterName);
            }

            return converterType;
        }
#endif
        internal static Type GetConverterType(Type type)
        {
            Debug.Assert(null != type, "Null passed for type to GetConverterType");

            Type converterType = null;

            // Try looking for the TypeConverter for the type using reflection.
            string converterName = ReflectionHelper.GetTypeConverterAttributeData(type, out converterType);

            if (converterType == null)
            {
                converterType = GetConverterTypeFromName(converterName);
            }

            return converterType;
        }

        private static Type GetConverterTypeFromName(string converterName)
        {
            Type converterType = null;

            if (!string.IsNullOrEmpty(converterName))
            {
                converterType = ReflectionHelper.GetQualifiedType(converterName);

                if (converterType != null)
                {
                    // Validate that this is an accessible type converter.
                    if (!ReflectionHelper.IsPublicType(converterType))
                    {
#if PBTCOMPILER
                        if (!ReflectionHelper.IsInternalType(converterType) ||
                            !ReflectionHelper.IsInternalAllowedOnType(converterType))
                        {
#endif
                            converterType = null;
#if PBTCOMPILER
                        }
#endif
                    }
                }
            }

            return converterType;
        }
#if !SYSTEM_XAML
        internal static Type GetCoreConverterTypeFromCustomType(Type type)
        {
            Type converterType = null;
            if (type.IsEnum)
            {
                // Need to handle Enums types specially as they require a ctor that
                // takes the underlying type, but at compile time we only need to know
                // the Type of the Converter and not an actual instance of it.
                converterType = typeof(EnumConverter);
            }
            else if (typeof(Int32).IsAssignableFrom(type))
            {
                converterType = typeof(Int32Converter);
            }
            else if (typeof(Int16).IsAssignableFrom(type))
            {
                converterType = typeof(Int16Converter);
            }
            else if (typeof(Int64).IsAssignableFrom(type))
            {
                converterType = typeof(Int64Converter);
            }
            else if (typeof(UInt32).IsAssignableFrom(type))
            {
                converterType = typeof(UInt32Converter);
            }
            else if (typeof(UInt16).IsAssignableFrom(type))
            {
                converterType = typeof(UInt16Converter);
            }
            else if (typeof(UInt64).IsAssignableFrom(type))
            {
                converterType = typeof(UInt64Converter);
            }
            else if (typeof(Boolean).IsAssignableFrom(type))
            {
                converterType = typeof(BooleanConverter);
            }
            else if (typeof(Double).IsAssignableFrom(type))
            {
                converterType = typeof(DoubleConverter);
            }
            else if (typeof(Single).IsAssignableFrom(type))
            {
                converterType = typeof(SingleConverter);
            }
            else if (typeof(Byte).IsAssignableFrom(type))
            {
                converterType = typeof(ByteConverter);
            }
            else if (typeof(SByte).IsAssignableFrom(type))
            {
                converterType = typeof(SByteConverter);
            }
            else if (typeof(Char).IsAssignableFrom(type))
            {
                converterType = typeof(CharConverter);
            }
            else if (typeof(Decimal).IsAssignableFrom(type))
            {
                converterType = typeof(DecimalConverter);
            }
            else if (typeof(TimeSpan).IsAssignableFrom(type))
            {
                converterType = typeof(TimeSpanConverter);
            }
            else if (typeof(Guid).IsAssignableFrom(type))
            {
                converterType = typeof(GuidConverter);
            }
            else if (typeof(String).IsAssignableFrom(type))
            {
                converterType = typeof(StringConverter);
            }
            else if (typeof(CultureInfo).IsAssignableFrom(type))
            {
                converterType = typeof(CultureInfoConverter);
            }
            else if (typeof(Type).IsAssignableFrom(type))
            {
                converterType = typeof(TypeTypeConverter);
            }
            else if (typeof(DateTime).IsAssignableFrom(type))
            {
                converterType = typeof(DateTimeConverter2);
            }

            return converterType;
        }
#endif
#if !PBTCOMPILER
        private static TypeConverter GetCoreConverterFromCoreType(Type type)
        {
            TypeConverter typeConverter = null;
            if (type == typeof(Int32))
            {
                typeConverter = new System.ComponentModel.Int32Converter();
            }
            else if (type == typeof(Int16))
            {
                typeConverter = new System.ComponentModel.Int16Converter();
            }
            else if (type == typeof(Int64))
            {
                typeConverter = new System.ComponentModel.Int64Converter();
            }
            else if (type == typeof(UInt32))
            {
                typeConverter = new System.ComponentModel.UInt32Converter();
            }
            else if (type == typeof(UInt16))
            {
                typeConverter = new System.ComponentModel.UInt16Converter();
            }
            else if (type == typeof(UInt64))
            {
                typeConverter = new System.ComponentModel.UInt64Converter();
            }
            else if (type == typeof(Boolean))
            {
                typeConverter = new System.ComponentModel.BooleanConverter();
            }
            else if (type == typeof(Double))
            {
                typeConverter = new System.ComponentModel.DoubleConverter();
            }
            else if (type == typeof(Single))
            {
                typeConverter = new System.ComponentModel.SingleConverter();
            }
            else if (type == typeof(Byte))
            {
                typeConverter = new System.ComponentModel.ByteConverter();
            }
            else if (type == typeof(SByte))
            {
                typeConverter = new System.ComponentModel.SByteConverter();
            }
            else if (type == typeof(Char))
            {
                typeConverter = new System.ComponentModel.CharConverter();
            }
            else if (type == typeof(Decimal))
            {
                typeConverter = new System.ComponentModel.DecimalConverter();
            }
            else if (type == typeof(TimeSpan))
            {
                typeConverter = new System.ComponentModel.TimeSpanConverter();
            }
            else if (type == typeof(Guid))
            {
                typeConverter = new System.ComponentModel.GuidConverter();
            }
            else if (type == typeof(String))
            {
                typeConverter = new System.ComponentModel.StringConverter();
            }
            else if (type == typeof(CultureInfo))
            {
                typeConverter = new System.ComponentModel.CultureInfoConverter();
            }
#if !SYSTEM_XAML
            else if (type == typeof(Type))
            {
                typeConverter = new System.Windows.Markup.TypeTypeConverter();
            }
#else
            else if (type == typeof(Type))
            {
                typeConverter = new System.Xaml.Replacements.TypeTypeConverter();
            }
#endif
            else if (type == typeof(DateTime))
            {
                typeConverter = new DateTimeConverter2();
            }
            else if (ReflectionHelper.IsNullableType(type))
            {
                typeConverter = new System.ComponentModel.NullableConverter(type);
            }

            return typeConverter;
        }

        internal static TypeConverter GetCoreConverterFromCustomType(Type type)
        {
            TypeConverter typeConverter = null;
            if (type.IsEnum)
            {
                // Need to handle Enums types specially as they require a ctor that
                // takes the underlying type.
                typeConverter = new System.ComponentModel.EnumConverter(type);
            }
            else if (typeof(Int32).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.Int32Converter();
            }
            else if (typeof(Int16).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.Int16Converter();
            }
            else if (typeof(Int64).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.Int64Converter();
            }
            else if (typeof(UInt32).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.UInt32Converter();
            }
            else if (typeof(UInt16).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.UInt16Converter();
            }
            else if (typeof(UInt64).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.UInt64Converter();
            }
            else if (typeof(Boolean).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.BooleanConverter();
            }
            else if (typeof(Double).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.DoubleConverter();
            }
            else if (typeof(Single).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.SingleConverter();
            }
            else if (typeof(Byte).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.ByteConverter();
            }
            else if (typeof(SByte).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.SByteConverter();
            }
            else if (typeof(Char).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.CharConverter();
            }
            else if (typeof(Decimal).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.DecimalConverter();
            }
            else if (typeof(TimeSpan).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.TimeSpanConverter();
            }
            else if (typeof(Guid).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.GuidConverter();
            }
            else if (typeof(String).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.StringConverter();
            }
            else if (typeof(CultureInfo).IsAssignableFrom(type))
            {
                typeConverter = new System.ComponentModel.CultureInfoConverter();
            }
#if !SYSTEM_XAML
            else if (typeof(Type).IsAssignableFrom(type))
            {
                typeConverter = new System.Windows.Markup.TypeTypeConverter();
            }
#else
            else if (type == typeof(Type))
            {
                typeConverter = new System.Xaml.Replacements.TypeTypeConverter();
            }
#endif
            else if (typeof(DateTime).IsAssignableFrom(type))
            {
                typeConverter = new DateTimeConverter2();
            }
            else if (typeof(Uri).IsAssignableFrom(type))
            {
                typeConverter = new System.Xaml.Replacements.TypeUriConverter();
            }

            return typeConverter;
        }

        /// <summary>
        /// Returns a TypeConverter for the given target Type, otherwise null if not found.
        /// First, if the type is one of the known system types, it lookups a table to determine the TypeConverter.
        /// Next, it tries to find a TypeConverterAttribute on the type using reflection.
        /// Finally, it looks up the table of known typeConverters again if the given type derives from one of the
        /// known system types.
        /// </summary>
        /// <param name="type">The target Type for which to find a TypeConverter.</param>
        /// <returns>A TypeConverter for the Type type if found. Null otherwise.</returns>
        internal static TypeConverter GetTypeConverter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            TypeConverter typeConverter = GetCoreConverterFromCoreType(type);

            if (typeConverter == null)
            {
                Type converterType = GetConverterType(type);
                if (converterType != null)
                {
                    typeConverter = Activator.CreateInstance(converterType,
                                                             BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
                                                             null,
                                                             null,
                                                             InvariantEnglishUS) as TypeConverter;
                }
                else
                {
                    typeConverter = GetCoreConverterFromCustomType(type);
                }

                if (typeConverter == null)
                {
                    typeConverter = new TypeConverter();
                }
            }

            return typeConverter;
        }
#endif
    }
}
