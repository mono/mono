using System.Collections;
using System.ComponentModel;
using System.Globalization;

#if NET_2_0
namespace System.Web.UI
{
	public class DataSourceCacheDurationConverter : Int32Converter
	{
		public DataSourceCacheDurationConverter () {
			throw new NotImplementedException ();
		}
		public bool CanConvertFrom (Type sourceType) {
			throw new NotImplementedException ();
		}
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
			throw new NotImplementedException ();
		}
		public bool CanConvertTo (Type destinationType) {
			throw new NotImplementedException ();
		}
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) {
			throw new NotImplementedException ();
		}
		public Object ConvertFrom (Object value) {
			throw new NotImplementedException ();
		}
		public override Object ConvertFrom (ITypeDescriptorContext context,
											CultureInfo culture,
											Object value) {
			throw new NotImplementedException ();
		}
		public Object ConvertTo (Object value, Type destinationType) {
			throw new NotImplementedException ();
		}
		public override Object ConvertTo (ITypeDescriptorContext context,
										CultureInfo culture,
										Object value,
										Type destinationType) {
			throw new NotImplementedException ();
		}
		public ICollection GetStandardValues () {
			throw new NotImplementedException ();
		}
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
		public bool GetStandardValuesExclusive () {
			throw new NotImplementedException ();
		}
		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
		public bool GetStandardValuesSupported () {
			throw new NotImplementedException ();
		}
		public override bool GetStandardValuesSupported (ITypeDescriptorContext context) {
			throw new NotImplementedException ();
		}
	}
}
#endif
