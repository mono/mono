//
// System.ComponentModel.StringConverter
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

	[MonoTODO]
	public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		throw new NotImplementedException ();
	}
}
}

