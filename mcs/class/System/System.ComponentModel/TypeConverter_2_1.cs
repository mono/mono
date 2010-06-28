//
// TypeConverter for SL 2
//
// Copyright (C) 2008 Novell, Inc.
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

#if MOONLIGHT

using System;
using System.Globalization;

namespace System.ComponentModel {

	public class TypeConverter {

		public bool CanConvertFrom (Type sourceType)
		{
			return CanConvertFrom (null, sourceType);
		}

		public virtual bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return false;
		}

		public object ConvertFrom (object value)
		{
			return ConvertFrom (null, CultureInfo.CurrentCulture, value);
		}

		public virtual object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			throw new NotImplementedException ();
		}

		public object ConvertFromString (string text)
		{
			return ConvertFrom (null, null, text);
		}

		public bool CanConvertTo (Type destinationType)
		{
			return CanConvertTo (null, destinationType);
		}

		public virtual bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return false;
		}

		public object ConvertTo (object value, Type destinationType)
		{
			return ConvertTo (null, CultureInfo.CurrentCulture, value, destinationType);
		}

		public virtual object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			throw new NotImplementedException ();
		}

		public string ConvertToString (object value)
		{
			return (string) ConvertTo (null, CultureInfo.CurrentCulture, value, typeof(string));
		}
	}
}

#endif
