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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//	Marek Safar     (marek.safar@gmail.com) 
//
//

using System.Collections;
using System.Drawing;
using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class WebColorConverter : ColorConverter
	{
		// Converts from string to Color
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) 
		{
			if (value is string) {
				string	s = ((string)value).Trim();
					return ColorTranslator.FromHtml (s);
			}
			
			return base.ConvertFrom (context, culture, value);
		}

		// Converts from Color to string
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
		{
			if (!(value is Color) || destinationType != typeof (string))
				return base.ConvertTo (context, culture, value, destinationType);

			Color c = (Color) value;

			if (culture == null)
				culture = Helpers.InvariantCulture;

			string s = c.ToKnownColor ().ToString ();
			if (s != "0")
				return s;

			return String.Concat ("#", c.R.ToString ("X2"), c.G.ToString ("X2"), c.B.ToString ("X2"));
		}
	}
}
