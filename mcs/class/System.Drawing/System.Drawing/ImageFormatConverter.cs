//
// System.Drawing.ImageFormatConverter.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Imaging;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for ImageFormatConverter.
	/// </summary>
	public class ImageFormatConverter : TypeConverter
	{
		public ImageFormatConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type srcType)
		{
			if (srcType == typeof (string))
				return true;
			else
				return false;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destType)
		{
			if (destType == typeof (string))
				return true;
			else
				return false; 
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object val)
		{			
			string strFormat = val as string;
			if (strFormat == null)
				return base.ConvertFrom (context, culture, val);
			
			if (strFormat.Equals ("Bmp"))
				return ImageFormat.Bmp;
			else if (strFormat.Equals ("Emf"))
				return ImageFormat.Emf;
			else if (strFormat.Equals ("Exif"))
				return ImageFormat.Exif;
			else if (strFormat.Equals ("Gif"))
				return ImageFormat.Gif;
			else if (strFormat.Equals ("Icon"))
				return ImageFormat.Icon;
			else if (strFormat.Equals ("Jpeg"))
				return ImageFormat.Jpeg;
			else if (strFormat.Equals ("MemoryBMP"))
				return ImageFormat.MemoryBmp;
			else if (strFormat.Equals ("Png"))
				return ImageFormat.Png;
			else if (strFormat.Equals ("Tiff"))
				return ImageFormat.Tiff;
			else if (strFormat.Equals ("Wmf"))
				return ImageFormat.Wmf;
			else
				return new NotSupportedException ("ImageFormatConverter cannot convert from " + val.GetType ());
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object val, Type destType )
		{
			if ((val is ImageFormat) && (destType == typeof (string)))
				return val.ToString ();
			
			return new NotSupportedException ("ImageFormatConverter can not convert from " + val.GetType ());
		}

		[MonoTODO ("Implement")]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context )
		{
			throw new NotImplementedException (); 
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context )
		{
			return false;
		}
	}
}
