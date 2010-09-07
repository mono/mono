//
// System.ComponentModel.CultureInfoConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Ivan N. Zlatev <contact@i-nz.net>
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2008 Novell, Inc. (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

namespace System.ComponentModel
{
	public class CultureInfoConverter : TypeConverter
	{
		private class CultureInfoComparer : IComparer
		{
			public int Compare (object first,  object second)
			{
				if (first == null) {
					if (second == null)
						return 0;
					else
						return -1;
				}
				if (second == null)
					return 1;

				return String.Compare (((CultureInfo)first).DisplayName, ((CultureInfo)second).DisplayName, 
						       false, CultureInfo.CurrentCulture);
			}
		}

		private StandardValuesCollection _standardValues;

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
			string culture_string = value as string;
			if (culture_string != null) {
				if (String.Compare (culture_string, "(Default)", false) == 0)
					return CultureInfo.InvariantCulture;

				try {
					// try to create a new CultureInfo if form is RFC 1766
					return new CultureInfo (culture_string);
				} catch {
					// try to create a new CultureInfo if form is verbose name
					foreach (CultureInfo CI in CultureInfo.GetCultures (CultureTypes.AllCultures))
						if (string.Compare (CI.DisplayName, 0, culture_string, 0, culture_string.Length, true) == 0)
							return CI;
				}
				throw new ArgumentException (string.Format (
					"Culture {0} cannot be converted to a " +
					"CultureInfo or is not available in " +
					"this environment.", value));
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string)) {
				if (value != null && (value is CultureInfo)) {
					if (value == CultureInfo.InvariantCulture)
						return "(Default)";
					else
						return ((CultureInfo) value).DisplayName; 
				} else {
					return "(Default)";
				}
			}
			if (destinationType == typeof (InstanceDescriptor) && value is CultureInfo) {
				CultureInfo cval = (CultureInfo) value;
				ConstructorInfo ctor = typeof(CultureInfo).GetConstructor (new Type[] {typeof(int)});
				return new InstanceDescriptor (ctor, new object[] {cval.LCID});
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			if (_standardValues == null) {
				CultureInfo[] cultures = CultureInfo.GetCultures (CultureTypes.AllCultures);
				Array.Sort (cultures, new CultureInfoComparer ());
				CultureInfo[] stdValues = new CultureInfo[cultures.Length + 1];
				stdValues[0] = CultureInfo.InvariantCulture;
				Array.Copy (cultures, 0, stdValues, 1, cultures.Length);
				_standardValues = new StandardValuesCollection (stdValues);
			}
			return _standardValues;
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

#if NET_4_0
		protected virtual string GetCultureName (CultureInfo culture)
		{
			// .Net doesn't throw ArgumentNullException here, ugh.
			return culture.Name;
		}
#endif

	}
}
