//
// System.ComponentModel.CultureInfoConverter
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
	public class CultureInfoConverter : TypeConverter
	{

		public CultureInfoConverter()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context,
			Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
			Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
			if (destinationType == typeof (InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
			CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string CultureString = (String) value;
				try {
					// try to create a new CultureInfo if form is RFC 1766
					return new CultureInfo (CultureString);
				} catch {
					// try to create a new CultureInfo if form is verbose name
					foreach (CultureInfo CI in CultureInfo.GetCultures (CultureTypes.AllCultures))
					// LAMESPEC MS seems to use EnglishName (culture invariant) - check this
						if (CI.EnglishName.ToString().IndexOf (CultureString) > 0)
							return CI;
				}
				throw new ArgumentException ("Culture incorrect or not available in this environment.", "value");
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string) && value != null && (value is CultureInfo))
				// LAMESPEC MS seems to use EnglishName (culture invariant) - check this
				return ((CultureInfo) value).EnglishName;
			if (destinationType == typeof (InstanceDescriptor) && value is CultureInfo) {
				CultureInfo cval = (CultureInfo) value;
				ConstructorInfo ctor = typeof(CultureInfo).GetConstructor (new Type[] {typeof(int)});
				return new InstanceDescriptor (ctor, new object[] {cval.LCID});
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (CultureInfo.GetCultures (CultureTypes.AllCultures));
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

	}
}
