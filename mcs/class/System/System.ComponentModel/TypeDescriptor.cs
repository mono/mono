//
// System.ComponentModel.TypeDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Drawing;

namespace System.ComponentModel
{

[MonoTODO("Only implemented the minimal features needed to use ColorConverter")]
public sealed class TypeDescriptor
{
	public static TypeConverter GetConverter (Type type)
	{
		if (type != typeof (System.Drawing.Color))
			throw new NotImplementedException ("Only System.Drawing.Color is supported by now.");

		return new ColorConverter ();
	}
}
}

