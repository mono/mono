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

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.WebControls 
{
	public class FontUnitConverter : TypeConverter 
	{
		#region Public Constructors
		public FontUnitConverter() 
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
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) 
		{
			if (destinationType == typeof (string)) {
				return true;
			}

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) 
		{
			string	s;

			if ((value == null) || !(value is string)) 
			{
				return base.ConvertFrom (context, culture, value);
			}


			s = (string)value;

			if (culture == null) 
			{
				culture = CultureInfo.CurrentCulture;
			}

			if (s == "") 
			{
				return FontUnit.Empty;
			}
			return FontUnit.Parse(s, culture);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) 
		{
			FontUnit	fu;

			if ((value == null) || !(value is FontUnit) || (destinationType != typeof(string))) 
			{
				return base.ConvertTo (context, culture, value, destinationType);
			}

			if (culture == null) 
			{
				culture = CultureInfo.CurrentCulture;
			}

			fu = (FontUnit)value;

			return fu.ToString(culture);
		}

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) 
		{
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties (typeof (FontUnit));
			ArrayList vals = new ArrayList ();
                
			for (int i = 0; i < props.Count; i++)
				vals.Add (props [i].GetValue (null));
			return new StandardValuesCollection (vals);
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) 
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) 
		{
			return true;
		}
		#endregion	// Public Instance Methods
	}
}
