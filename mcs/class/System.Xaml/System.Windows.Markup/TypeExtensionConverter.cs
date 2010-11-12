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
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Markup
{
	internal class TypeExtensionConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			// unlike implied at http://msdn.microsoft.com/en-us/library/ee621338.aspx , it does not support IXamlTypeResolver.
			//if (sourceType == typeof (string) && context != null) {
			//	var xtr = context.GetService (typeof (IXamlTypeResolver)) as IXamlTypeResolver;
			//	return xtr != null;
			//}
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var vctx = (IValueSerializerContext) context;
			var tx = value as TypeExtension;
			var lookup = vctx != null ? (INamespacePrefixLookup) vctx.GetService (typeof (INamespacePrefixLookup)) : null;
			if (tx != null && destinationType == typeof (string))
				return tx.TypeName ?? new XamlTypeName ((XamlType) tx.ProvideValue (vctx)).ToString (lookup);
			else
				return base.ConvertTo (context, culture, value, destinationType); // it seems it still handles not-supported types (such as int).
		}
	}
}
