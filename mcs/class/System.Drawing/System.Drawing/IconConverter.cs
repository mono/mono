                                                                                                             //
// System.Drawing.IconConverter.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing {
	/// <summary>
	/// Summary description for IconConverter.
	/// </summary>
	public class IconConverter : ExpandableObjectConverter
	{
		public IconConverter()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (System.Byte []))
				return true;
			else
				return false;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if ((destinationType == typeof (System.Byte [])) || (destinationType == typeof (System.String)))
				return true;
			else
				return false;
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			byte [] bytes = value as byte [];
			if (bytes == null)
				return base.ConvertFrom (context, culture, value);
			
			MemoryStream ms = new MemoryStream (bytes);
			
			return new Icon (ms);				
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((value is Icon) && (destinationType == typeof (string)))
				return value.ToString ();
			else if (value == null && destinationType == typeof (string))
				return "(none)";
			else if (CanConvertTo (null, destinationType)) {
				//came here means destType is byte array ;
				using (MemoryStream ms = new MemoryStream ()) {
					((Icon) value).Save (ms);
					return ms.ToArray ();
				}
			}else
				return new NotSupportedException ("IconConverter can not convert from " + value.GetType ());				
		}
	}
}
