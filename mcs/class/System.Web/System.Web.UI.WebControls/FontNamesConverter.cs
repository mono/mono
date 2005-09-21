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
//
//

using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class FontNamesConverter : System.ComponentModel.TypeConverter 
	{
		#region Public Constructors
		public FontNamesConverter() 
		{
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
		{
			if (sourceType == typeof(string)) 
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
		{
			if (value is string) 
			{
				string[]	names;
				string		namelist;
				int		count;

				namelist = (string)value;

				if (namelist == string.Empty) 
				{
					return new string[0];
				}

				names = namelist.Split(new char[] { ',' });

				count = names.Length;
				for (int i = 0; i < count; i++) 
				{
					names[i] = names[i].Trim();
				}

				return names;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
		{
			if ((destinationType == typeof(string)) && (value is string[])) 
			{
				return String.Join(",", (string[])value);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion	// Public Instance Methods
	}
}
