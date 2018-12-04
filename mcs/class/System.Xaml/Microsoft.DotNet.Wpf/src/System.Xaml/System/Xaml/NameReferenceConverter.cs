// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Xaml;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup
{
    public class NameReferenceConverter: TypeConverter 
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            
            var nameResolver = (IXamlNameResolver)context.GetService(typeof(IXamlNameResolver));
            if (nameResolver == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.MissingNameResolver));
            }

            string name = value as string;
            if (String.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException(SR.Get(SRID.MustHaveName));
            }
            object obj = nameResolver.Resolve(name);
            if (obj == null)
            {
                string[] names = new string[] { name };
                obj = nameResolver.GetFixupToken(names, true);
            }
            return obj;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (context == null || (context.GetService(typeof(IXamlNameProvider)) as  IXamlNameProvider) == null)
            {
                return false;
            }

            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
            
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var nameProvider = (IXamlNameProvider)context.GetService(typeof(IXamlNameProvider));
            if (nameProvider == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.MissingNameProvider));
            }

            return nameProvider.GetName(value);            
        }
    }
}
