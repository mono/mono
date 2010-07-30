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
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			throw new NotImplementedException ();
		}
		
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			throw new NotImplementedException ();
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			throw new NotImplementedException ();
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			throw new NotImplementedException ();
		}
	}
}

