//
// System.Drawing.RectangleConverter.cs
//
// Authors:
//   	Dennis Hayes (dennish@Raytek.com)
//	Jordi Mas (jordi@ximian.com)
//	Ravindra (rkumar@novell.com)
//	
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using System;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Text;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for RectangleConverter.
	/// </summary>
	public class RectangleConverter : TypeConverter
	{
		public RectangleConverter ()
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
			if (subs.Length != 4)
				throw new ArgumentException ("Failed to parse Text(" + s + ") expected text in the format \"x,y,Width,Height.\"");

			int x = Int32.Parse (subs [0]);
			int y = Int32.Parse (subs [1]);
			int width = Int32.Parse (subs [2]);
			int height = Int32.Parse (subs [3]);

			return new Rectangle (x, y, width, height);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if ((destinationType == typeof (string)) && (value is Rectangle)) {
				Rectangle rect = (Rectangle) value;
				StringBuilder sb = new StringBuilder ();
				sb.Append (rect.X); sb.Append (", ");
				sb.Append (rect.Y); sb.Append (", ");
				sb.Append (rect.Width); sb.Append (", ");
				sb.Append (rect.Height);
				return sb.ToString ();
			}
			
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context,
						       IDictionary propertyValues)
		{
			int x = (int) propertyValues ["X"];
			int y = (int) propertyValues ["Y"];
			int width = (int) propertyValues ["Width"];
			int height = (int) propertyValues ["Height"];

			return new Rectangle (x, y, width, height);
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
