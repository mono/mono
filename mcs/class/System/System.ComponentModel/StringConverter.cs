//
// System.ComponentModel.StringConverter.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Globalization;

namespace System.ComponentModel {

	public class StringConverter : TypeConverter
	{
		public StringConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(String))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				return String.Empty;

			if (value is string)
				return (string) value;

			return base.ConvertFrom (context, culture, value);
		}
	}
}

