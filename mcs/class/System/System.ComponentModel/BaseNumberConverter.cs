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

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
		{
			if (t == typeof (string))
				return true;

			return base.CanConvertTo (context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			if (value is string) {
				try {
					return Convert.ChangeType (value, InnerType, culture.NumberFormat);
				} catch (Exception e) {
					// LAMESPEC MS just seems to pass the internal Exception on to the user
					// so it throws a pure Exception here. We should probably throw a 
					// ArgumentException or something like that
					throw e;
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

			if (destinationType == typeof (string) && value.GetType() == InnerType)
				return Convert.ChangeType (value, typeof (string), culture.NumberFormat);

			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

