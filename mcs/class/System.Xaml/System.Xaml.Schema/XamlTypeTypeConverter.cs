//
// Copyright (C) 2010 Novell Inc. http://novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xaml;

namespace System.Xaml.Schema
{
	public class XamlTypeTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof (string);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof (string);
		}

		public override Object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			throw new NotSupportedException (String.Format ("Conversion from type {0} is not supported", value != null ? value.GetType () : null));
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var vctx = (IValueSerializerContext) context;
			var lookup = vctx != null ? (INamespacePrefixLookup) vctx.GetService (typeof (INamespacePrefixLookup)) : null;
			var xt = value as XamlType;
			if (xt != null && destinationType == typeof (string)) {
				if (lookup != null)
					return new XamlTypeName (xt).ToString (lookup);
				else
					return xt.UnderlyingType != null ? xt.UnderlyingType.ToString () : xt.ToString ();
			}
			else
				return base.ConvertTo (context, culture, value, destinationType); // it seems it still handles not-supported types (such as int).
		}
	}
}
