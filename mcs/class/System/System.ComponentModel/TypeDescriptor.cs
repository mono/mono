//
// System.ComponentModel.TypeDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.ComponentModel
{

[MonoTODO("Only implemented the minimal features needed to use ColorConverter")]
public sealed class TypeDescriptor
{
	public static TypeConverter GetConverter (Type type)
	{
		object [] attrs = type.GetCustomAttributes (false);
		string converter_name = null;
		foreach (object o in attrs){
			if (o is TypeConverterAttribute){
				TypeConverterAttribute tc = (TypeConverterAttribute) o;
				converter_name = tc.ConverterTypeName;
				break;
			}
		}

		if (converter_name == null)
			return null;

		object converter = null;
		try {
			converter = Activator.CreateInstance (Type.GetType (converter_name));
		} catch (Exception){
		}
	
		return converter as TypeConverter;
	}
}
}

