//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    // Thread Safety: This class is thread safe
    public class QueryStringConverter
    {
        Hashtable defaultSupportedQueryStringTypes;
        // the cache does not have a quota since it is per endpoint and is
        // bounded by the number of types in the contract at the endpoint
        Hashtable typeConverterCache;

        public QueryStringConverter()
        {
            this.defaultSupportedQueryStringTypes = new Hashtable();
            this.defaultSupportedQueryStringTypes.Add(typeof(Byte), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(SByte), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Int16), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Int32), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Int64), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(UInt16), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(UInt32), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(UInt64), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Single), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Double), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Boolean), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Char), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Decimal), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(String), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Object), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(DateTime), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(TimeSpan), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(byte[]), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Guid), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(Uri), null);
            this.defaultSupportedQueryStringTypes.Add(typeof(DateTimeOffset), null);
            this.typeConverterCache = new Hashtable();
        }

        public virtual bool CanConvert(Type type)
        {
            if (this.defaultSupportedQueryStringTypes.ContainsKey(type))
            {
                return true;
            }
            // otherwise check if its an enum
            if (typeof(Enum).IsAssignableFrom(type))
            {
                return true;
            }
            // check if there's a typeconverter defined on the type
            return (GetStringConverter(type) != null);
        }

        public virtual object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameterType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterType");
            }
            switch (Type.GetTypeCode(parameterType))
            {
                case TypeCode.Byte:
                    return parameter == null ? default(Byte) : XmlConvert.ToByte(parameter);
                case TypeCode.SByte:
                    return parameter == null ? default(SByte) : XmlConvert.ToSByte(parameter);
                case TypeCode.Int16:
                    return parameter == null ? default(Int16) : XmlConvert.ToInt16(parameter);
                case TypeCode.Int32:
                    {
                        if (typeof(Enum).IsAssignableFrom(parameterType))
                        {
                            return Enum.Parse(parameterType, parameter, true);
                        }
                        else
                        {
                            return parameter == null ? default(Int32) : XmlConvert.ToInt32(parameter);
                        }
                    }
                case TypeCode.Int64:
                    return parameter == null ? default(Int64) : XmlConvert.ToInt64(parameter);
                case TypeCode.UInt16:
                    return parameter == null ? default(UInt16) : XmlConvert.ToUInt16(parameter);
                case TypeCode.UInt32:
                    return parameter == null ? default(UInt32) : XmlConvert.ToUInt32(parameter);
                case TypeCode.UInt64:
                    return parameter == null ? default(UInt64) : XmlConvert.ToUInt64(parameter);
                case TypeCode.Single:
                    return parameter == null ? default(Single) : XmlConvert.ToSingle(parameter);
                case TypeCode.Double:
                    return parameter == null ? default(Double) : XmlConvert.ToDouble(parameter);
                case TypeCode.Char:
                    return parameter == null ? default(Char) : XmlConvert.ToChar(parameter);
                case TypeCode.Decimal:
                    return parameter == null ? default(Decimal) : XmlConvert.ToDecimal(parameter);
                case TypeCode.Boolean:
                    return parameter == null ? default(Boolean) : Convert.ToBoolean(parameter, CultureInfo.InvariantCulture);
                case TypeCode.String:
                    return parameter;
                case TypeCode.DateTime:
                    return parameter == null ? default(DateTime) : DateTime.Parse(parameter, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                default:
                    {
                        if (parameterType == typeof(TimeSpan))
                        {
                            // support the XML as well as default way of representing timespans
                            TimeSpan result;
                            if (!TimeSpan.TryParse(parameter, out result))
                            {
                                result = parameter == null ? default(TimeSpan) : XmlConvert.ToTimeSpan(parameter);
                            }
                            return result;
                        }
                        else if (parameterType == typeof(Guid))
                        {
                            return parameter == null ? default(Guid) : XmlConvert.ToGuid(parameter);
                        }
                        else if (parameterType == typeof(DateTimeOffset))
                        {
                            return (parameter == null) ? default(DateTimeOffset) : DateTimeOffset.Parse(parameter, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces);
                        }
                        else if (parameterType == typeof(byte[]))
                        {
                            return (!string.IsNullOrEmpty(parameter)) ? Convert.FromBase64String(parameter) : new byte[] { };
                        }
                        else if (parameterType == typeof(Uri))
                        {
                            return (!string.IsNullOrEmpty(parameter)) ? new Uri(parameter, UriKind.RelativeOrAbsolute) : null;
                        }
                        else if (parameterType == typeof(object))
                        {
                            return parameter;
                        }
                        else
                        {
                            TypeConverter stringConverter = GetStringConverter(parameterType);
                            if (stringConverter == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                                    SR2.GetString(
                                    SR2.TypeNotSupportedByQueryStringConverter,
                                    parameterType.ToString(), this.GetType().Name)));
                            }
                            return stringConverter.ConvertFromInvariantString(parameter);
                        }
                    }
            }
        }

        public virtual string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameterType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterType");
            }
            if (parameterType.IsValueType && parameter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameter");
            }
            switch (Type.GetTypeCode(parameterType))
            {
                case TypeCode.Byte:
                    return XmlConvert.ToString((Byte) parameter);
                case TypeCode.SByte:
                    return XmlConvert.ToString((SByte) parameter);
                case TypeCode.Int16:
                    return XmlConvert.ToString((Int16) parameter);
                case TypeCode.Int32:
                    {
                        if (typeof(Enum).IsAssignableFrom(parameterType))
                        {
                            return Enum.Format(parameterType, parameter, "G");
                        }
                        else
                        {
                            return XmlConvert.ToString((int) parameter);
                        }
                    }
                case TypeCode.Int64:
                    return XmlConvert.ToString((Int64) parameter);
                case TypeCode.UInt16:
                    return XmlConvert.ToString((UInt16) parameter);
                case TypeCode.UInt32:
                    return XmlConvert.ToString((uint) parameter);
                case TypeCode.UInt64:
                    return XmlConvert.ToString((UInt64) parameter);
                case TypeCode.Single:
                    return XmlConvert.ToString((Single) parameter);
                case TypeCode.Double:
                    return XmlConvert.ToString((double) parameter);
                case TypeCode.Char:
                    return XmlConvert.ToString((char) parameter);
                case TypeCode.Decimal:
                    return XmlConvert.ToString((decimal) parameter);
                case TypeCode.Boolean:
                    return XmlConvert.ToString((bool) parameter);
                case TypeCode.String:
                    return (string) parameter;
                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime) parameter, XmlDateTimeSerializationMode.RoundtripKind);
                default:
                    {
                        if (parameterType == typeof(TimeSpan))
                        {
                            return XmlConvert.ToString((TimeSpan) parameter);
                        }
                        else if (parameterType == typeof(Guid))
                        {
                            return XmlConvert.ToString((Guid) parameter);
                        }
                        else if (parameterType == typeof(DateTimeOffset))
                        {
                            return XmlConvert.ToString((DateTimeOffset) parameter);
                        }
                        else if (parameterType == typeof(byte[]))
                        {
                            return (parameter != null) ? Convert.ToBase64String((byte[]) parameter, Base64FormattingOptions.None) : null;
                        }
                        else if (parameterType == typeof(Uri) || parameterType == typeof(object))
                        {
                            // URI or object
                            return (parameter != null) ? Convert.ToString(parameter, CultureInfo.InvariantCulture) : null;
                        }
                        else
                        {
                            TypeConverter stringConverter = GetStringConverter(parameterType);
                            if (stringConverter == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                                    SR2.GetString(
                                    SR2.TypeNotSupportedByQueryStringConverter,
                                    parameterType.ToString(), this.GetType().Name)));
                            }
                            else
                            {
                                return stringConverter.ConvertToInvariantString(parameter);
                            }
                        }
                    }
            }
        }

        // hash table is safe for multiple readers single writer
        [SuppressMessage("Reliability", "Reliability104:CaughtAndHandledExceptionsRule", Justification = "The exception is traced in the finally clause")]
        TypeConverter GetStringConverter(Type parameterType)
        {
            if (this.typeConverterCache.ContainsKey(parameterType))
            {
                return (TypeConverter) this.typeConverterCache[parameterType];
            }
            TypeConverterAttribute[] typeConverterAttrs = parameterType.GetCustomAttributes(typeof(TypeConverterAttribute), true) as TypeConverterAttribute[];
            if (typeConverterAttrs != null)
            {
                foreach (TypeConverterAttribute converterAttr in typeConverterAttrs)
                {
                    Type converterType = Type.GetType(converterAttr.ConverterTypeName, false, true);
                    if (converterType != null)
                    {
                        TypeConverter converter = null;
                        Exception handledException = null;
                        try
                        {
                            converter = (TypeConverter) Activator.CreateInstance(converterType);
                        }
                        catch (TargetInvocationException e)
                        {
                            handledException = e;
                        }
                        catch (MemberAccessException e)
                        {
                            handledException = e;
                        }
                        catch (TypeLoadException e)
                        {
                            handledException = e;
                        }
                        catch (COMException e)
                        {
                            handledException = e;
                        }
                        catch (InvalidComObjectException e)
                        {
                            handledException = e;
                        }
                        finally
                        {
                            if (handledException != null)
                            {
                                if (Fx.IsFatal(handledException))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(handledException);
                                }
                                DiagnosticUtility.TraceHandledException(handledException, TraceEventType.Warning);
                            }
                        }
                        if (converter == null)
                        {
                            continue;
                        }
                        if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
                        {
                            this.typeConverterCache.Add(parameterType, converter);
                            return converter;
                        }
                    }
                }
            }
            return null;
        }
    }
}
