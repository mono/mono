//
// System.Web.UI.MinimizableAttributeTypeConvert.cs
//
// Authors:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.Globalization;
using System.ComponentModel;

namespace System.Web.UI 
{
	internal class MinimizableAttributeTypeConverter : TypeConverter 
	{
		public MinimizableAttributeTypeConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is string && destinationType == typeof (bool)) {
				return value != null;
			}
			if (value is bool && destinationType == typeof (string))
				return ((Boolean) value).ToString (culture);

			return base.ConvertTo (context, culture, value, destinationType);
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string typeName;
			
			if (value != null) {
				Type t = value.GetType ();
				if (t == typeof (string)) {
					string s = value as string;
					if (String.IsNullOrEmpty (s) || String.Compare (s, "false", StringComparison.OrdinalIgnoreCase) == 0)
						return false;
					else
						return true;
				}
				typeName = t.FullName;
			} else
				typeName = "null";

			throw new NotSupportedException (String.Format ("MinimizableAttributeTypeConverter cannot convert from {0}", typeName));
		}

	}
}
