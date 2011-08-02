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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;


namespace System.Windows.Forms {

	public class TreeViewImageIndexConverter : ImageIndexConverter {

		public TreeViewImageIndexConverter ()
		{
		}

		#region Protected Properties
		protected override bool IncludeNoneAsStandardValue {
			get { return false; }
		}
		#endregion

		#region Public Methods
		public override object ConvertFrom (System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string indexStr;
			
			if (value != null && value is string) {
				indexStr = (string)value;

				if (indexStr.Equals ("(default)", StringComparison.InvariantCultureIgnoreCase))
					return -1;
				else if (indexStr.Equals ("(none)", StringComparison.InvariantCultureIgnoreCase))
					return -2;
					
				return Int32.Parse (indexStr);
			} else
				return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string)) {
				if (value == null)
					return string.Empty;
				else if (value is int && (int)value == -1)
					return "(default)";
				else if (value is int && (int)value == -2)
					return "(none)";
				else if (value is string && ((string)value).Length == 0)
					return string.Empty;
				else
					return value.ToString ();
			} else
				return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (System.ComponentModel.ITypeDescriptorContext context)
		{
			int[] stdVal = new int[] { -1, -2 };
			return new TypeConverter.StandardValuesCollection (stdVal);
		}
		#endregion
	}
}

