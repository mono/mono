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
//	Ravindra	rkumar@novell.com
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ImageIndexConverter.cs,v $
// Revision 1.1  2004/08/27 22:07:37  ravindra
// Implemented.
//

// COMPLETE

using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	public class ImageIndexConverter : Int32Converter
	{
		#region Constructors

		public ImageIndexConverter () { }

		#endregion Constructors

		#region Protected Properties

		protected virtual bool IncludeNoneAsStandardValue {
			get { return true; }
		}

		#endregion Protected Properties

		#region Public Methods

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string indexStr;
			if (value != null && value is string) {
				indexStr = (string) value;
				return Int32.Parse (indexStr);
			}
			else
				return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture,
						  object value, Type destinationType)
		{
			if (value != null && destinationType == typeof (string)) {
				if (value is int && (int) value == -1)
					return "(none)";
				else
					return value.ToString ();
			}
			else
				return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			int [] stdVal = new int [] {-1};
			return new TypeConverter.StandardValuesCollection (stdVal);
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		#endregion Public Methods
	}
}
