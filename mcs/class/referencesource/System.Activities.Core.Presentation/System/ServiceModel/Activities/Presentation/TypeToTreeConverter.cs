//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Collections;
    using System.Activities.Core.Presentation;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    sealed class TypeToTreeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = null;
            if (value is ModelItem)
            {
                value = ((ModelItem)value).GetCurrentValue();
            }
            if (value is Type)
            {
                type = (Type)value;
            }
            else if (value is PropertyInfo)
            {
                type = ((PropertyInfo)value).PropertyType;
            }
            else if (value is FieldInfo)
            {
                type = ((FieldInfo)value).FieldType;
            }

            //1) Dead-ends (not expand any more)
            //  a. CLR built-in types 
            //  b. Byte array, DateTime, TimeSpan, GUID, Uri, XmlQualifiedName, XmlElement and XmlNode array [This includes XElement and XNode array from .NET 3.5] 
            //  c. Enums 
            //  d. Arrays and Collection classes including List<T>, Dictionary<K,V> and Hashtable(Anything that implements IEnumerable or IDictionary or is an array is treated as a collection)
            //  e. Types marked with [CollectionDataContract] attribute
            //2) Show nothing (Xpath generator cannot generate XPath according to member info), but user should be able to manually input query string
            //  a. Types that implement IXmlSerializable
            //  b. Types that implement ISerializable.
            //3) Show all fields  without [NonSerializable] regardless of visibility
            //  a. Types marked with Serializable attribute
            //4) Show all [DataMembers]
            //  a. Types marked with DataContract attribute
            //5) Show all public fields and properties without [IgnoreDataMember]
            //  a. Types with none of the above attributes (POCO) but with a default constructor (can be non-public).
            //
            //
            //Priority of those interfaces or attributes:
            //IXmlSerializable -> ISerializable -> DataContract -> Serializable
            //Type cannot be Iserializable/IXmlSerializable and have DataContractAttribute attribute.
            IEnumerable result = null;
            if ((null != type) && (!ContentCorrelationTypeExpander.IsPrimitiveTypeInXPath(type)))
            {
                if (type.GetInterface("IXmlSerializable", false) != null)
                {
                    result = null;
                }
                else if (type.GetInterface("ISerializable", false) != null)
                {
                    result = null;
                }
                else if (type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0)
                {
                    result = type
                        .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(member => member.GetCustomAttributes(typeof(DataMemberAttribute), false).Length > 0)
                        .OrderBy(member => member.Name);
                }
                else if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                {
                    result = type
                        .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(member => member.MemberType == MemberTypes.Field && member.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                        .OrderBy(member => member.Name);
                }
                else if (type.GetConstructor(new Type[0] { }) != null)
                {
                    result = type
                        .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                        .Where(member => (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property) &&
                        (member.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false).Length == 0))
                        .OrderBy(member => member.Name);
                }
            }
            return result;
        }


        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
