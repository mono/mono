/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebColorConverter
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class WebColorConverter : ColorConverter
	{
		public WebColorConverter(): base()
		{
		}
		
		[MonoTODO("Implement_If_Color_Is_#xxxxxx_OR_A_KnownColor")]
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is string)
			{
				string val = ((string)value).Trim();
				if(val == String.Empty || val.Lenth == 0)
				{
					return Color.Empty;
				}
				if(val[0] == '#')
				{
					throw new NotImplementedException();
				}
			}
			return ConvertFrom(context, culture, value);
		}
		
		[MonoTODO("Convert_To_For_KnownColor_And_For_#xxxxxx")]
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == null)
			{
				throw new ArgumentNullException("destinationType");
			}
			throw new NotImplementedException();
		}
	}
}
