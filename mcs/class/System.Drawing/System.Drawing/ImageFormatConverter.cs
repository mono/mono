//
// System.Drawing.ImageFormatConverter.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Imaging;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

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
				
			return base.CanConvertFrom (context, srcType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destType)
		{
			if (destType == typeof (string))
				return true;
				
			if (destType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destType);
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object val)
		{			
			string strFormat = val as string;
			if (strFormat == null)
				return base.ConvertFrom (context, culture, val);
			
			if (strFormat.Equals (ImageFormat.Bmp.ToString ()))
				return ImageFormat.Bmp;
			else if (strFormat.Equals (ImageFormat.Emf.ToString ()))
				return ImageFormat.Emf;
			else if (strFormat.Equals (ImageFormat.Exif.ToString ()))
				return ImageFormat.Exif;
			else if (strFormat.Equals (ImageFormat.Gif.ToString ()))
				return ImageFormat.Gif;
			else if (strFormat.Equals (ImageFormat.Icon.ToString ()))
				return ImageFormat.Icon;
			else if (strFormat.Equals (ImageFormat.Jpeg.ToString ()))
				return ImageFormat.Jpeg;
			else if (strFormat.Equals (ImageFormat.MemoryBmp.ToString ()))
				return ImageFormat.MemoryBmp;
			else if (strFormat.Equals (ImageFormat.Png.ToString ()))
				return ImageFormat.Png;
			else if (strFormat.Equals (ImageFormat.Tiff.ToString ()))
				return ImageFormat.Tiff;
			else if (strFormat.Equals (ImageFormat.Wmf.ToString ()))
				return ImageFormat.Wmf;
				
			return base.ConvertFrom (context, culture, val);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object val, Type destType )
		{
			if ((val is ImageFormat) && (destType == typeof (string)))
				return val.ToString ();
			
			if (destType == typeof (InstanceDescriptor) && val is ImageFormat) {
				ImageFormat c = (ImageFormat) val;

				string prop = null;
				if (c.Guid.Equals (ImageFormat.Bmp.Guid))
					prop = "Bmp";
				else if (c.Guid.Equals (ImageFormat.Emf.Guid))
					prop = "Emf";
				else if (c.Guid.Equals (ImageFormat.Exif.Guid))
					prop = "Exif";
				else if (c.Guid.Equals (ImageFormat.Gif.Guid))
					prop = "Gif";
				else if (c.Guid.Equals (ImageFormat.Icon.Guid))
					prop = "Icon";
				else if (c.Guid.Equals (ImageFormat.Jpeg.Guid))
					prop = "Jpeg";
				else if (c.Guid.Equals (ImageFormat.MemoryBmp.Guid))
					prop = "MemoryBmp";
				else if (c.Guid.Equals (ImageFormat.Png.Guid))
					prop = "Png";
				else if (c.Guid.Equals (ImageFormat.Tiff.Guid))
					prop = "Tiff";
				else if (c.Guid.Equals (ImageFormat.Wmf.Guid))
					prop = "Wmf";
				
				if (prop != null){
					return new InstanceDescriptor (typeof (ImageFormat).GetProperty (prop), null);
				} else {
					ConstructorInfo ctor = typeof(ImageFormat).GetConstructor (new Type[] {typeof(Guid)} );
					return new InstanceDescriptor (ctor, new object[] {c.Guid});
				}
			}
			
			return base.ConvertTo (context, culture, val, destType);
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
