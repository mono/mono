//
// System.ComponentModel.TypeConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Globalization;

namespace System.ComponentModel {

[MonoTODO("Only has the minimal implementation needed to use ColorConverter")]
public class TypeConverter
{
	public object ConvertFrom (object o)
	{
		return ConvertFrom (null, null, o);
	}

	public virtual object ConvertFrom (ITypeDescriptorContext context,
					   CultureInfo culture,
					   object value)
	{
		throw new NotImplementedException ();
	}

	public object ConvertFromString (string s)
	{
		return ConvertFrom (s);
	}
}
}

