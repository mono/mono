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

[MonoTODO]
public class ColorConverter : TypeConverter
{
	public ColorConverter ()
	{
	}

	[MonoTODO("Only some basic conversion needed by xsp")]
	public override object ConvertFrom (ITypeDescriptorContext context,
					    CultureInfo culture,
					    object value)
	{
		string s = value as string;
		if (s == null)
			throw new NotImplementedException ();

		if (s == "")
			return Color.Empty;

		Color c = Color.FromName (s);
		if (!(c.A == c.R && c.R == c.G && c.G == c.B && c.B == 0))
			return c;
		
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
}
}

