//
// System.Web.UI.Design.DataBindingCollectionConverter
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
	public class DataBindingCollectionConverter : TypeConverter
	{
		public DataBindingCollectionConverter ()
		{
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
				return string.Empty;

			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
