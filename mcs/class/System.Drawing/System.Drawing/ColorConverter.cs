//
// System.Drawing.ColorConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Drawing {

public class ColorConverter : TypeConverter
{
	public ColorConverter ()
	{
	}

	public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof (string))
			return true;

		return base.CanConvertFrom(context, sourceType);
	}

	//[MonoTODO]
	public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
	{
		throw new NotImplementedException ();
	}

	public override object ConvertFrom (ITypeDescriptorContext context,
					    CultureInfo culture,
					    object value)
	{
		string s = value as string;
		if (s == null)
			return base.ConvertFrom (context, culture, value);

		object named = Color.NamedColors [s];
		if (named != null)
			return (Color) named;

		named = Color.SystemColors [s];
		if (named != null)
			return (Color) named;
			
		int i;
		if (s [0] == '#')
			i = Int32.Parse (s.Substring (1), NumberStyles.HexNumber);
		else
			i = Int32.Parse (s, NumberStyles.Integer);

		int A = (int) (i & 0xFF000000) >> 24;
		if (A == 0)
			A = 255;
		return Color.FromArgb (A, (i & 0x00FF0000) >> 16, (i & 0x00FF00) >> 8, (i & 0x0FF));
	}

	//[MonoTODO]
	public override object ConvertTo (ITypeDescriptorContext context,
					  CultureInfo culture,
					  object value,
					  Type destinationType)
	{
		throw new NotImplementedException ();
	}
/*
 *  StandardValuesCollection is TypeDescriptor.StandardValuesCollection
 *  TODO: check if the compiler already supports that.
	public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
	{
	}
*/

	//[MonoTODO]
	public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
	{
		// This should return true once GetStandardValues is implemented
		throw new NotImplementedException ();
	}
}
}

