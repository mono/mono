/**
 * Namespace: System.Web.UI.WebControls
 * Class:     FontNamesConverter
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
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class FontNamesConverter : TypeConverter
	{
		public FontNamesConverter(): base()
		{
		}
		
		 public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		 {
		 	return (sourceType == typeof(string));
		 }
		 
		 public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		 {
		 	if(value is string)
		 	{
		 		string fontNames = (string)value;
		 		if(fontNames.Length == 0)
		 		{
		 			return (new string[0]);
		 		}
		 		string[] names = fontNames.Split(new char[] { ','});
		 		foreach(string current in names)
		 		{
		 			current = current.Trim();
		 		}
		 		return names;
		 	}
		 	throw GetConvertFromException(value);
		 }
		 
		 public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		 {
		 	if(destinationType is typeof(string))
		 	{
		 		if(value == null || ((string[])value) == null)
		 			return String.Empty;
		 		return String.Join(",", (string[])value);
		 	}
		 	throw GetConvertToException(value, destinationType);
		 }
	}
}
