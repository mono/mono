//
// System.Drawing.SizeConverter.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2003 Novell, Inc. http://www.novell.com
//

using System;
using System.Collections;
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

		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
						   Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			return base.CanConvertTo (context, destinationType);
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
				throw new ArgumentException ("Failed to parse Text(" + s + ") expected text in the format \"Width,Height.\"");

			int width = Int32.Parse (subs [0]);
			int height = Int32.Parse (subs [1]);

			return new Size (width, height);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if ((destinationType == typeof (string)) && (value is Size))
				return value.ToString ();
			
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context,
						       IDictionary propertyValues)
		{
			int width = (int) propertyValues ["Width"];
			int height = (int) propertyValues ["Height"];

			return new Size (width, height);
		}

		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		[MonoTODO]
		public override PropertyDescriptorCollection GetProperties (
							ITypeDescriptorContext context,
							object value, Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}
		
		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
