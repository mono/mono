/*++
Copyright (c) 2003 Microsoft Corporation

Module Name:

    _UriTypeConverter.cs

Abstract:

    A default TypeConverter implementation for the System.Uri type

Revision History:

--*/
namespace System {
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

    //
    // A limited conversion is implemented such as to and from string
    // A conversion to InstanceDescriptor is also provided for design time support.
    //
    public class UriTypeConverter: TypeConverter
    {
        private UriKind m_UriKind;


        public UriTypeConverter() : this(UriKind.RelativeOrAbsolute) { }

        internal UriTypeConverter(UriKind uriKind)
        {
            m_UriKind = uriKind;
        }


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            if (sourceType == typeof(string))
                return true;

            if (typeof(Uri).IsAssignableFrom(sourceType))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        //
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;

            if (destinationType == typeof(string))
                return true;

            if (destinationType == typeof(Uri))
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        //
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,  object value)
        {
            string uriString = value as string;
            if (uriString != null)
                return new Uri(uriString, m_UriKind);

            Uri uri = value as Uri;
            if (uri != null)
                return new Uri(uri.OriginalString,
                    m_UriKind == UriKind.RelativeOrAbsolute ? uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative : m_UriKind);

            return base.ConvertFrom(context, culture, value);
        }
        //
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Uri uri = value as Uri;

            if (uri != null && destinationType == typeof(InstanceDescriptor))
            {
                ConstructorInfo ci = typeof(Uri).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[]{typeof(string), typeof(UriKind)}, null);
                return new InstanceDescriptor(ci, new object[] { uri.OriginalString,
                    m_UriKind == UriKind.RelativeOrAbsolute ? uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative : m_UriKind });
            }

            if (uri != null && destinationType == typeof(string))
                return uri.OriginalString;

            if (uri != null && destinationType == typeof(Uri))
                return new Uri(uri.OriginalString,
                    m_UriKind == UriKind.RelativeOrAbsolute ? uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative : m_UriKind);

            return base.ConvertTo(context, culture, value, destinationType);
        }
        //
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            string str = value as string;
            Uri temp;
            if (str != null)
                return Uri.TryCreate(str, m_UriKind, out temp);

            return value is Uri;
        }
    }

}


