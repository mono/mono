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

using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	public class ImageKeyConverter : StringConverter
	{
		#region Constructors
		public ImageKeyConverter () { }
		#endregion Constructors

		#region Protected Properties
		protected virtual bool IncludeNoneAsStandardValue {
			get { return true; }
		}
		#endregion Protected Properties

		#region Public Methods
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return false;
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null && value is string)
				return (string) value;
			else
				return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture,
						  object value, Type destinationType)
		{
			if (value == null)
				return "(none)";
			else if (destinationType == typeof (string)) {
				if (value is string && (string) value == string.Empty)
					return "(none)";
				else
					return value.ToString ();
			}
			else
				return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			string[] stdVal = new string[] { string.Empty };
			return new TypeConverter.StandardValuesCollection (stdVal);
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion Public Methods
	}
}
