//
// EncodingConverter.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace System.ServiceModel.Configuration
{
	sealed class EncodingConverter : TypeConverter
	{
		static EncodingConverter _instance = new EncodingConverter ();

		public static EncodingConverter Instance {
			get { return _instance; }
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) {
			string encString = (string) value;
			Encoding encoding;

			switch (encString.ToLower (CultureInfo.InvariantCulture)) {
			case "utf-16le":
			case "utf-16":
			case "ucs-2":
			case "unicode":
			case "iso-10646-ucs-2":
				encoding = new UnicodeEncoding (false, true);
				break;
			case "utf-16be":
			case "unicodefffe":
				encoding = new UnicodeEncoding (true, true);
				break;
			case "utf-8":
			case "unicode-1-1-utf-8":
			case "unicode-2-0-utf-8":
			case "x-unicode-1-1-utf-8":
			case "x-unicode-2-0-utf-8":
				encoding = Encoding.UTF8;
				break;
			default:
				encoding = Encoding.GetEncoding (encString);
				break;
			}

			return encoding;
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			Encoding encoding = (Encoding) value;
			return encoding.WebName;
		}
	}
}
