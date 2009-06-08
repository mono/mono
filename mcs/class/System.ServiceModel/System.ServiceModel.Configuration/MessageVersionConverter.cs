//
// MessageVersionConverter.cs
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
using System.ServiceModel.Channels;

namespace System.ServiceModel.Configuration
{
	class MessageVersionConverter
	 : TypeConverter
	{
		static readonly Dictionary<string, MessageVersion> _lookup;

		static MessageVersionConverter () {
			_lookup = new Dictionary<string, MessageVersion> (StringComparer.OrdinalIgnoreCase);
			_lookup.Add ("Default", MessageVersion.Default);
			_lookup.Add ("None", MessageVersion.None);
			_lookup.Add ("Soap11", MessageVersion.Soap11);
			_lookup.Add ("Soap11WSAddressing10", MessageVersion.Soap11WSAddressing10);
			_lookup.Add ("Soap11WSAddressingAugust2004", MessageVersion.Soap11WSAddressingAugust2004);
			_lookup.Add ("Soap12", MessageVersion.Soap12);
			_lookup.Add ("Soap12WSAddressing10", MessageVersion.Soap12WSAddressing10);
			_lookup.Add ("Soap12WSAddressingAugust2004", MessageVersion.Soap12WSAddressingAugust2004);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string stringValue = (string) value;
			if (_lookup.ContainsKey (stringValue))
				return _lookup [stringValue];
			throw new ArgumentOutOfRangeException ();
		}
	}
}
