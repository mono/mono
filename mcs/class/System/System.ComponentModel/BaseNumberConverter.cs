//
// System.ComponentModel.BaseNumberConverter.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
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

using System;
using System.Globalization;

namespace System.ComponentModel
{
	public abstract class BaseNumberConverter : TypeConverter
	{
		internal Type InnerType;

		protected BaseNumberConverter()
		{
		}

		internal abstract bool SupportHex {
			get;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof (string) || base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
		{
			return t.IsPrimitive || base.CanConvertTo (context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			string text = value as string;
			if (text != null) {
				try {
					if (SupportHex) {
						if (text.Length >= 1 && text[0] == '#') {
							return ConvertFromString (text.Substring (1), 16);
						}

						if (text.StartsWith ("0x") || text.StartsWith ("0X")) {
							return ConvertFromString (text, 16);
						}
					}

 					NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat(typeof(NumberFormatInfo));
					return ConvertFromString (text, numberFormatInfo);
				} catch (Exception e) {
					// LAMESPEC MS wraps the actual exception in an Exception
					throw new Exception (value.ToString() + " is not a valid "
						+ "value for " + InnerType.Name + ".", e);
				}
			}

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
						 object value, Type destinationType)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

#if NET_2_0
			if (destinationType == typeof (string) && value is IConvertible)
				return ((IConvertible) value).ToType (destinationType, culture);
#else
			if (destinationType == typeof (string) && value.GetType () == InnerType) {
				NumberFormatInfo numberFormatInfo = (NumberFormatInfo) culture.GetFormat (typeof (NumberFormatInfo));
				return ConvertToString (value, numberFormatInfo);
			}
#endif

			if (destinationType.IsPrimitive)
				return Convert.ChangeType (value, destinationType, culture);

			return base.ConvertTo (context, culture, value, destinationType);
		}

		internal abstract string ConvertToString (object value, NumberFormatInfo format);

		internal abstract object ConvertFromString (string value, NumberFormatInfo format);

		internal virtual object ConvertFromString (string value, int fromBase)
		{
			if (SupportHex) {
				throw new NotImplementedException ();
			} else {
				throw new InvalidOperationException ();
			}
		}
	}
}
