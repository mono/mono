//
// System.Web.UI.Design.DataSourceConverter
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.Design
{
	public class DataSourceConverter : TypeConverter
	{
		public DataSourceConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string));
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				return string.Empty;

			if (value.GetType () == typeof(string))
				return ((string) value);

			throw base.GetConvertFromException (value);
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return ((context.Instance as IComponent) != null);
		}
	}
}
