
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
using System.Globalization;
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
				for(int i=0; i < names.Length; i++)
				{
					names[i] = names[i].Trim();
				}
		 		return names;
			}
			throw GetConvertFromException(value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof(string))
			{
				if(value == null || ((string[])value) == null)
					return String.Empty;
				return String.Join(",", (string[])value);
			}
			throw GetConvertToException(value, destinationType);
		}
	}
}
