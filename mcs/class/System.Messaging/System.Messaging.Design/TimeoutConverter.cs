//
// System.Messaging.Design.TimeoutConverter
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Messaging
{
	internal class TimeoutConverter : TypeConverter
	{
		public TimeoutConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		[MonoTODO]
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			throw new NotImplementedException ();
		}
	}
}
