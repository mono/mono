//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if NET_4_0
using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace System.ServiceModel.Discovery.Configuration
{
	public class DiscoveryVersionConverter : TypeConverter
	{
		public DiscoveryVersionConverter ()
		{
		}
		
		private bool CanConvert (Type type)
		{
			if (type == typeof (string))
				return true;
			if (type == typeof (DiscoveryVersion))
				return true;
			return false;
		}
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException ("sourceType");

			return CanConvert (sourceType);
		}
		
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == null)
				return false;

			return CanConvert (destinationType);
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (!CanConvertFrom (context, value.GetType ()))
				throw new NotSupportedException ("Cannot convert from value.");

			if (value is DiscoveryVersion)
				return value;

			string s = (value as string);
			if (s != null) {
				switch (s) {
				case "WSDiscovery11":
					return DiscoveryVersion.WSDiscovery11;
				case "WSDiscoveryApril2005":
					return DiscoveryVersion.WSDiscoveryApril2005;
				case "WSDiscoveryCD1":
					return DiscoveryVersion.WSDiscoveryCD1;
				}
				throw new NotSupportedException ("Cannot convert from value.");
			}

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!CanConvertTo (context, destinationType))
				throw new NotSupportedException (Locale.GetText ("Cannot convert to destination type."));

			var ver = (value as DiscoveryVersion);
			if (ver != null) {
				if (destinationType == typeof (DiscoveryVersion))
					return ver;

				if (destinationType == typeof (string)) {
					if (ver.Equals (DiscoveryVersion.WSDiscovery11))
						return "WSDiscovery11";
					if (ver.Equals (DiscoveryVersion.WSDiscoveryApril2005))
						return "WSDiscoveryApril2005";
					if (ver.Equals (DiscoveryVersion.WSDiscoveryCD1))
						return "WSDiscoveryCD1";
				}
				throw new NotSupportedException ("Cannot convert to destination type.");
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

#endif
