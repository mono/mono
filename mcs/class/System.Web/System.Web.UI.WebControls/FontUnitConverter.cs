/**
 * Namespace: System.Web.UI.WebControls
 * Class:     FontUnitConverter
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status: 95%
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
	public class FontUnitConverter : TypeConverter
	{
		public FontUnitConverter(): base()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
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
					return FontUnit.Empty;
				}
				return FontUnit.Parse(val, culture);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(value != null && value is FontUnit)
			{
				FontUnit val = (FontUnit)value;
				if(val.Type == FontSize.NotSet)
				{
					return String.Empty;
				}
				return val.ToString(culture);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		[MonoTODO("GetStandardValues")]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
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
