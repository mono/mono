//
// System.Drawing.ColorConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Drawing {

public class ColorConverter : TypeConverter
{
	static StandardValuesCollection cached;
	static object creatingCached = new object ();

	public ColorConverter ()
	{
	}

	public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof (string))
			return true;

		return base.CanConvertFrom(context, sourceType);
	}

	[MonoTODO]
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

		Color result = Color.FromArgb (A, (i & 0x00FF0000) >> 16, (i & 0x00FF00) >> 8, (i & 0x0FF));
		// Look for a named or system color with those values
		foreach (Color c in Color.NamedColors.Values) {
			if (c.A == result.A && c.R == result.R && c.G == result.G && c.B == result.B)
				return c;
		}

		foreach (Color c in Color.SystemColors.Values) {
			if (c.A == result.A && c.R == result.R && c.G == result.G && c.B == result.B)
				return c;
		}

		return result;
	}

	[MonoTODO]
	public override object ConvertTo (ITypeDescriptorContext context,
					  CultureInfo culture,
					  object value,
					  Type destinationType)
	{
		throw new NotImplementedException ();
	}

	public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
	{
		if (cached != null)
			return cached;

		lock (creatingCached) {
			if (cached != null)
				return cached;
			
			ICollection named = (ICollection) Color.NamedColors;
			ICollection system = (ICollection) Color.SystemColors;
			Array colors = Array.CreateInstance (typeof (Color), named.Count + system.Count);
			named.CopyTo (colors, 0);
			system.CopyTo (colors, named.Count);
			Array.Sort (colors, 0, colors.Length, new CompareColors ());
			cached = new StandardValuesCollection (colors);
		}

		return cached;
	}

	public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
	{
		return true;
	}

	class CompareColors : IComparer
	{
		public int Compare (object x, object y)
		{
			return String.Compare (((Color) x).Name, ((Color) y).Name);
		}
	}
}
}

