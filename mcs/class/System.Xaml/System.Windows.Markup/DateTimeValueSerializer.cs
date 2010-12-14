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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup
{
#if !NET_2_1
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyWindowsBase)]
#endif
	public class DateTimeValueSerializer : ValueSerializer
	{
		const DateTimeStyles styles = DateTimeStyles.RoundtripKind | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return true; // documented
		}

		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return value is DateTime;
		}

		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			if (value == null)
				throw new NotSupportedException ();
			if (value.Length == 0)
				return DateTime.MinValue;
			return DateTime.Parse (value, CultureInfo.InvariantCulture, styles);
		}

		public override string ConvertToString (object value,     IValueSerializerContext context)
		{
			if (!(value is DateTime))
				throw new NotSupportedException ();
			DateTime dt = (DateTime) value;
			if (dt.Millisecond != 0)
				return dt.ToString ("yyyy-MM-dd'T'HH:mm:ss.F");
			if (dt.Second != 0)
				return dt.ToString ("yyyy-MM-dd'T'HH:mm:ss");
			if (dt.Minute != 0)
				return dt.ToString ("yyyy-MM-dd'T'HH:mm");
			else
				return dt.ToString ("yyyy-MM-dd");
		}
	}
}
