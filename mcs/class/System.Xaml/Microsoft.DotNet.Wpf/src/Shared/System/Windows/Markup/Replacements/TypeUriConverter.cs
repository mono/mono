// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//
//  Contents:  Limited converter for string <--> System.Uri
//
//

namespace System.Xaml.Replacements
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal class TypeUriConverter : TypeConverter
    {
        public TypeUriConverter()
        { 
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            return sourceType == typeof(string) || sourceType == typeof(Uri);
        }

        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return 
                destinationType == typeof(InstanceDescriptor) ||
                destinationType == typeof(string) ||
                destinationType == typeof(Uri);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var uri = value as Uri;
            if (uri != null)
            {
                var uriKind = UriKind.RelativeOrAbsolute;
                if (uri.IsWellFormedOriginalString())
                {
                    uriKind = uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    var ci = 
                        typeof(Uri)
                        .GetConstructor(
                            BindingFlags.Public | BindingFlags.Instance, 
                            null, 
                            new Type[] { typeof(string), typeof(UriKind) }, 
                            null);
                    return 
                        new InstanceDescriptor(
                            ci, 
                            new object[] { uri.OriginalString, uriKind});
                }

                if (destinationType == typeof(string))
                {
                    return uri.OriginalString;
                }

                if (destinationType == typeof(Uri))
                {
                    return new Uri(uri.OriginalString, uriKind);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var uriString = value as string;
            if (uriString != null)
            {
                if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
                {
                    return new Uri(uriString, UriKind.Absolute);
                }

                if (Uri.IsWellFormedUriString(uriString, UriKind.Relative))
                {
                    return new Uri(uriString, UriKind.Relative);
                }

                return new Uri(uriString, UriKind.RelativeOrAbsolute);
            }

            var uri = value as Uri;
            if (uri != null)
            {
                if (uri.IsWellFormedOriginalString())
                {
                    return new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
                }

                return new Uri(uri.OriginalString, UriKind.RelativeOrAbsolute);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            var uriString = value as string;
            if (uriString != null)
            {
                Uri result = null;
                return Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out result);
            }

            return value is Uri;
        }
    }
}
