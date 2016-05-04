//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;
    using System.Xaml;

    public abstract class TypeConverterBase : TypeConverter
    {
        // Give the Lazy<T> a Func<T> to create the ConcurrentDictionary<Type, TypeConverterHelper> because TypeConverterHelper is
        // internal and we want to avoid the demand for ReflectionPermission(MemberAccess).
        Lazy<ConcurrentDictionary<Type, TypeConverterHelper>> helpers = new Lazy<ConcurrentDictionary<Type, TypeConverterHelper>>( delegate()
                        {
                            return new ConcurrentDictionary<Type, TypeConverterHelper>();
                        }
                    );

        TypeConverterHelper helper;
        Type baseType;
        Type helperType;

        internal TypeConverterBase(Type baseType, Type helperType)
        {
            this.baseType = baseType;
            this.helperType = helperType;
        }

        internal TypeConverterBase(Type targetType, Type baseType, Type helperType)
        {
            this.helper = GetTypeConverterHelper(targetType, baseType, helperType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == TypeHelper.StringType)
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == TypeHelper.StringType)
            {
                return false;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                TypeConverterHelper currentHelper = helper;
                if (currentHelper == null)
                {
                    IDestinationTypeProvider targetService = context.GetService(typeof(IDestinationTypeProvider)) as IDestinationTypeProvider;
                    Type targetType = targetService.GetDestinationType();

                    if (!this.helpers.Value.TryGetValue(targetType, out currentHelper))
                    {
                        currentHelper = GetTypeConverterHelper(targetType, this.baseType, this.helperType);
                        if (!this.helpers.Value.TryAdd(targetType, currentHelper))
                        {
                            if (!this.helpers.Value.TryGetValue(targetType, out currentHelper))
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TypeConverterHelperCacheAddFailed(targetType)));
                            }
                        }
                    }
                }
                object result = currentHelper.UntypedConvertFromString(stringValue, context);
                return result;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }

        TypeConverterHelper GetTypeConverterHelper(Type targetType, Type baseType, Type helperType)
        {
            Type[] genericTypeArguments;
            if (baseType.BaseType == targetType)
            {
                // support non-generic ActivityWithResult, In/Out/InOutArgument 
                genericTypeArguments = new Type[] { TypeHelper.ObjectType };
            }
            else
            {
                // Find baseType in the base class list of targetType
                while (!targetType.IsGenericType ||
                    !(targetType.GetGenericTypeDefinition() == baseType))
                {
                    if (targetType == TypeHelper.ObjectType)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidTypeConverterUsage));
                    }

                    targetType = targetType.BaseType;
                }
                genericTypeArguments = targetType.GetGenericArguments();
            }

            Type concreteHelperType = helperType.MakeGenericType(genericTypeArguments);
            return (TypeConverterHelper)Activator.CreateInstance(concreteHelperType);
        }

        internal abstract class TypeConverterHelper
        {
            public abstract object UntypedConvertFromString(string text, ITypeDescriptorContext context);

            public static T GetService<T>(ITypeDescriptorContext context) where T : class
            {
                T service = (T)context.GetService(typeof(T));
                if (service == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidTypeConverterUsage));
                }

                return service;
            }
        }

        internal abstract class TypeConverterHelper<T> : TypeConverterHelper
        {
            public abstract T ConvertFromString(string text, ITypeDescriptorContext context);

            public sealed override object UntypedConvertFromString(string text, ITypeDescriptorContext context)
            {
                return ConvertFromString(text, context);
            }
        }
    }
}
