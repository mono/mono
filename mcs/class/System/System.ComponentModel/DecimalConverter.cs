//
// System.ComponentModel.DecimalConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
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

using System.Globalization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

namespace System.ComponentModel
{
	public class DecimalConverter : BaseNumberConverter
	{
		public DecimalConverter()
		{
			InnerType = typeof(Decimal);
		}

		internal override bool SupportHex {
			get { return false; }
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
			Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}
		
		public override object ConvertTo (ITypeDescriptorContext context,
			CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor) && value is Decimal) {
				Decimal cval = (Decimal) value;
				ConstructorInfo ctor = typeof(Decimal).GetConstructor (new Type[] {typeof(int[])});
				return new InstanceDescriptor (ctor, new object[] {Decimal.GetBits (cval)});
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		internal override string ConvertToString (object value, NumberFormatInfo format)
		{
			return ((decimal) value).ToString ("G", format);
		}

		internal override object ConvertFromString (string value, NumberFormatInfo format)
		{
			return decimal.Parse (value, NumberStyles.Float, format);
		}
	}
}
