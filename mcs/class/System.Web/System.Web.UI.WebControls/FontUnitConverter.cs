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
