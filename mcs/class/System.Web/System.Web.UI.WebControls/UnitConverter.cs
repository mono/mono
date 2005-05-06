
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
 * Class:     UnitConverter
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Globalization;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.Web.UI.WebControls
{
	public class UnitConverter : TypeConverter
	{
		public UnitConverter(): base()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string))
				return true;
			return CanConvertFrom(context, sourceType);
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
					return Unit.Empty;
				}
				return (culture == null ? Unit.Parse(val) : Unit.Parse(val, culture));
			}
			return ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof(string))
			{
				Unit val = (Unit)value;
				if(val == Unit.Empty)
				{
					return String.Empty;
				}
				return val.ToString(culture);
			}

#if NET_2_0
			if (destinationType == typeof (InstanceDescriptor) && value is Unit) {
				Unit s = (Unit) value;
				ConstructorInfo ci = typeof(Unit).GetConstructor (new Type[] { typeof(double), typeof(UnitType) });
				return new InstanceDescriptor (ci, new object[] { s.Value, s.Type });
			}

			if (destinationType == typeof (InstanceDescriptor) && value is string) {
				Unit s = Unit.Parse ((string)value, CultureInfo.InvariantCulture);
				ConstructorInfo ci = typeof(Unit).GetConstructor (new Type[] { typeof(double), typeof(UnitType) });
				return new InstanceDescriptor (ci, new object[] { s.Value, s.Type });
			}
#endif
			
			return ConvertTo(context, culture, value, destinationType);
		}
	}
}
