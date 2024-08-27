using System.ComponentModel;
using System.Globalization;

namespace System.Web
{
	public enum SameSiteMode
	{
		None,
		Lax,
		Strict
	}

	public class SameSiteModeConverter : EnumConverter
	{
		public SameSiteModeConverter()
			: base(typeof(SameSiteMode))
		{
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string text && text.Equals("Unspecified", StringComparison.InvariantCultureIgnoreCase)) {
				return (SameSiteMode)(-1);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is SameSiteMode && destinationType == typeof(string)) {
				int num = (int)value;
				if (num < 0)  {
					return "Unspecified";
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
