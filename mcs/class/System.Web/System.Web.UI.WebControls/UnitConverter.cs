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
			ConvertTo(context, culture, value, destinationType);
		}
	}
}
