//
// System.Diagnostics.Design.LogConverter
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel;
using System.Globalization;

namespace System.Diagnostics.Design
{
	public class LogConverter : TypeConverter
	{
		public LogConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if ((value as string) != null)
				return ((string) value).Trim ();

			return base.ConvertFrom (context, culture, value);
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
