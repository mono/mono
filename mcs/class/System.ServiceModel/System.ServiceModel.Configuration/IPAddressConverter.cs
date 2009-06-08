using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Net;

namespace System.ServiceModel.Configuration
{
	class IPAddressConverter
	 : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			return IPAddress.Parse ((string) value);
		}
	}
}
