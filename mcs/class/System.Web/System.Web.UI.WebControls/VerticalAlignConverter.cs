//
// System.Web.UI.WebControls.VerticalAlignConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.WebControls
{
	class VerticalAlignConverter : EnumConverter
	{
		public VerticalAlignConverter () : base (typeof(VerticalAlign))
		{
		}

		// The base class is enough to handle everything.
		// The methods are here just to make the class status page happy.
		// Add some optimizations?

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			return base.ConvertFrom (context, culture, value);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

