
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     FontUnitConverter
 *
 * Author:  Gaurav Vaish, Gonzalo Paniagua Javier
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>, <gonzalo@ximian.com>
 * Implementation: yes
 * Status: 95%
 *
 * (C) Gaurav Vaish (2002)
 * (c) 2002 Ximian, Inc. (http://www.ximian.com)
 */

using System;
using System.Globalization;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

namespace System.Web.UI.WebControls
{
	public class FontUnitConverter : TypeConverter
	{
		static StandardValuesCollection valuesCollection;
		static string creatingValues = "creating value collection";

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

#if NET_2_0
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			if (destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}
#endif

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value == null)
				return null;
			if(value is string)
			{
				string val = ((string)value).Trim();
				if(val.Length == 0)
				{
					return FontUnit.Empty;
				}
				return FontUnit.Parse(val, CultureInfo.InvariantCulture);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof (string) && value is FontUnit)
			{
				FontUnit val = (FontUnit)value;
				if(val.Type == FontSize.NotSet)
				{
					return String.Empty;
				}
				return val.ToString (culture);
			}
#if NET_2_0
			if (destinationType == typeof (InstanceDescriptor)) {
				if (value is string)
					value = FontUnit.Parse ((string)value, CultureInfo.InvariantCulture);

				if (value is FontUnit) {
					FontUnit s = (FontUnit) value;
					if (s.Type == FontSize.AsUnit) {
						ConstructorInfo ci = typeof(FontUnit).GetConstructor (new Type[] { typeof (Unit) });
						return new InstanceDescriptor (ci, new object[] {s.Unit});
					} else {
						ConstructorInfo ci = typeof(FontUnit).GetConstructor (new Type[] { typeof (FontSize) });
						return new InstanceDescriptor (ci, new object[] {s.Type});
					}
				}
			}
#endif
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (valuesCollection != null)
				return valuesCollection;

			lock (creatingValues) {
				if (valuesCollection != null)
					return valuesCollection;

				Array values = Enum.GetValues (typeof (FontUnit));
				Array.Sort (values);
				valuesCollection = new StandardValuesCollection (values);
			}

			return valuesCollection;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
