//
// System.Web.Configuration.LowerCaseStringConverter
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

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

using System.ComponentModel;
using System.Globalization;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class LowerCaseStringConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext ctx, Type type)
		{
			return (type == typeof (string));
		}

		public override bool CanConvertTo (ITypeDescriptorContext ctx, Type type)
		{
			return (type == typeof (string));
		}

		public override object ConvertFrom (ITypeDescriptorContext ctx, CultureInfo ci, object data)
		{
			return ((string)data).ToLowerInvariant ();
		}

		public override object ConvertTo (ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
		{
			if (value == null)
				return "";

			if (! (value is string))
				throw new ArgumentException ("value");

			return ((string)value).ToLowerInvariant ();
		}
	}
}

#endif
