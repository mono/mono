//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Core
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;

    using System.Activities.Presentation;

    // This class is used for generating collections of passed type argument, based on the 
    // fully qualified type name passed as containerParameter. i.e. one may convert
    // Foo class into List<Foo>
    internal sealed class GenericTypeParameterConverter : IValueConverter
    {

        bool displayFullName;

        public GenericTypeParameterConverter() : this(false)
        {
        }

        public GenericTypeParameterConverter(bool displayFullName)
        {
            this.displayFullName = displayFullName;
        }

        object IValueConverter.Convert(object value, Type convertToType, object containerParameter, CultureInfo culture)
        {
            Type[] argumentTypes = GetGenericTypeArguments(value);
            Type containerType = GetContainerType(containerParameter);

            if (null != argumentTypes && null != containerType)
            {
                Type resultType = BuildTargetType(argumentTypes, containerType);
                return HandleConversion(resultType, convertToType);
            }

            containerType = GetContainerType(value);
            if (null != containerType)
            {
                return HandleConversion(containerType, convertToType);
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        Type GetContainerType(object value)
        {
            if (null == value)
            {
                return null;
            }
            if (value is ModelItem)
            {
                value = ((ModelItem)value).GetCurrentValue();
            }
            if (value is Type)
            {
                return (Type)value;
            }
            else if (value is string)
            {
                return Type.GetType((string)value, false, true);
            }
            return null;
        }

        Type[] GetGenericTypeArguments(object value)
        {
            Type[] result = null;
            if (value is ModelItem)
            {
                value = ((ModelItem)value).GetCurrentValue();
            }
            if (null != value && (value is Type))
            {
                Type type = (Type)value;
                if (type.IsGenericType)
                {
                    result = type.GetGenericArguments();
                }
                else
                {
                    result = new Type[] { type };
                }
            }
            return result;
        }

        Type BuildTargetType(Type[] argumentTypes, Type containerType)
        {
            if (containerType.IsGenericType)
            {
                return containerType.MakeGenericType(argumentTypes);
            }
            return containerType;
        }

        object HandleConversion(Type resultType, Type convertToType)
        {
            if (typeof(string) == convertToType)
            {
                if (resultType.IsGenericType)
                {
                    StringBuilder strBldr = new StringBuilder();
                    if (this.displayFullName)
                    {
                        strBldr.Append(resultType.FullName.Substring(0, resultType.FullName.IndexOf('`')));
                    }
                    else
                    {
                        strBldr.Append(resultType.Name.Substring(0, resultType.Name.IndexOf('`')));
                    }
                    
                    strBldr.Append("<");
                    bool addComma = false;
                    foreach (Type arg in resultType.GetGenericArguments())
                    {
                        strBldr.Append(addComma ? "," : string.Empty);
                        if (arg.IsGenericType)
                        {
                            strBldr.Append(HandleConversion(arg, convertToType));
                        }
                        else
                        {
                            strBldr.Append(this.displayFullName ? arg.FullName : arg.Name);
                        }
                        addComma = true;
                    }
                    strBldr.Append(">");
                    return strBldr.ToString();
                }
                return this.displayFullName ? resultType.FullName : resultType.Name;
            }
            else if (typeof(Type) == convertToType)
            {
                return resultType;
            }
            return null;
        }
    }
}
