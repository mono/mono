/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebColorConverter
 *
 * Author:  Gaurav Vaish, Gonzalo Paniagua Javier
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>, <gonzalo@ximian.com>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 * (c) 2002 Ximian, Inc. (http://www.ximian.com)
 */

using System;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class WebColorConverter : ColorConverter
	{
		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			if (value is string) {
				string val = ((string) value).Trim ();
				if(val == String.Empty || val.Length == 0)
					return Color.Empty;

				NumberStyles style = (val [0] == '#') ? NumberStyles.HexNumber :
								       NumberStyles.None;

				try {
					int n = Int32.Parse (val.Substring (1), style);
					return Color.FromArgb (n);
				} catch {
					Color c = Color.FromName (val);
					if (c.A != 0 || c.R != 0 || c.B != 0 || c.G != 0)
						return c;
				}
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (destinationType == typeof (String) && value != null) {
				Color c = (Color) value;
				if (c == Color.Empty)
					return String.Empty;

				if (c.IsNamedColor || c.IsSystemColor)
					return c.Name;

				return String.Format ("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}

