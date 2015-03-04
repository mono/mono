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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//   Jonathan Pobst  monkey@jpobst.com
//

using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	public class LinkConverter : TypeConverter
	{
		#region Constructors
		public LinkConverter () { }
		#endregion Constructors

		#region Public Methods
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return false;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if ((value == null) || !(value is String))
				return base.ConvertFrom (context, culture, value);

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			if (((string)value).Length == 0)
				return null;
				
			string[] parts = ((string)value).Split (culture.TextInfo.ListSeparator.ToCharArray ());

			return new LinkLabel.Link (int.Parse (parts[0].Trim ()), int.Parse (parts[1].Trim ()));
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if ((value == null) || !(value is LinkLabel.Link) || (destinationType != typeof (string)))
				return base.ConvertTo (context, culture, value, destinationType);

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			LinkLabel.Link l = (LinkLabel.Link)value;

			return string.Format ("{0}{2} {1}", l.Start, l.Length, culture.TextInfo.ListSeparator);
		}
		#endregion Public Methods
	}
}
