//
// SecurityAlgorithmSuiteConverter.cs
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
using System.ServiceModel.Security;

namespace System.ServiceModel.Configuration
{
	sealed class SecurityAlgorithmSuiteConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string strValue = (string) value;
			switch (strValue.ToLowerInvariant ()) {
			case "default":
				return SecurityAlgorithmSuite.Default;
			case "basic128":
				return SecurityAlgorithmSuite.Basic128;
			case "basic128rsa15":
				return SecurityAlgorithmSuite.Basic128Rsa15;
			case "basic128sha256":
				return SecurityAlgorithmSuite.Basic128Sha256;
			case "basic128sha256rsa15":
				return SecurityAlgorithmSuite.Basic128Sha256Rsa15;
			case "basic192":
				return SecurityAlgorithmSuite.Basic192;
			case "basic192rsa15":
				return SecurityAlgorithmSuite.Basic192Rsa15;
			case "basic192sha256":
				return SecurityAlgorithmSuite.Basic192Sha256;
			case "basic192sha256rsa15":
				return SecurityAlgorithmSuite.Basic192Sha256Rsa15;
			case "basic256":
				return SecurityAlgorithmSuite.Basic256;
			case "basic256rsa15":
				return SecurityAlgorithmSuite.Basic256Rsa15;
			case "basic256sha256":
				return SecurityAlgorithmSuite.Basic256Sha256;
			case "basic256sha256rsa15":
				return SecurityAlgorithmSuite.Basic256Sha256Rsa15;
			}
			throw new ArgumentException ();
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			return base.ConvertTo (context, culture, value, destinationType);
		}

	}
}
