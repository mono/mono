//
// System.Drawing.SizeConverter.cs
//
// Authors:
// 	Dennis Hayes (dennish@Raytek.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc
// (C) 2003 Novell, Inc. (http://www.novell.com)
//
using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for SizeConverter.
	/// </summary>
	public class SizeConverter : TypeConverter
	{
		public SizeConverter()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
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

			string [] subs = s.Split (',');
			if (subs.Length != 2)
				throw new ArgumentException ("Error parsing " + s + " as Size", "value");

			try {
				int width = Int32.Parse (subs [0]);
				int height = Int32.Parse (subs [0]);
				return new Size (width, height);
			} catch {
				throw new ArgumentException ("Error parsing " + s + " as Size", "value");
			}
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			throw new NotImplementedException ();
		}
	}
}

