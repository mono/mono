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

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
				
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
				
			if (destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			// we must be able to convert from short names and long names
			string strFormat = (value as string);
			if (strFormat == null) {
				// case #1, this is not a string
				return base.ConvertFrom (context, culture, value);
			} else if (strFormat [0] == '[') {
				// case #2, this is probably a long format (guid)
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
			} else {
				// case #3, this is probably a short format
				if (String.Compare (strFormat, "Bmp", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Bmp;
				else if (String.Compare (strFormat, "Emf", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Emf;
				else if (String.Compare (strFormat, "Exif", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Exif;
				else if (String.Compare (strFormat, "Gif", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Gif;
				else if (String.Compare (strFormat, "Icon", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Icon;
				else if (String.Compare (strFormat, "Jpeg", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Jpeg;
				else if (String.Compare (strFormat, "MemoryBmp", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.MemoryBmp;
				else if (String.Compare (strFormat, "Png", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Png;
				else if (String.Compare (strFormat, "Tiff", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Tiff;
				else if (String.Compare (strFormat, "Wmf", StringComparison.OrdinalIgnoreCase) == 0)
					return ImageFormat.Wmf;
			}
			// last case, this is an unknown string
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
		{
			if (value is ImageFormat) {
				ImageFormat c = (ImageFormat) value;
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

				if (destinationType == typeof (string)) {
					return prop != null ? prop : c.ToString ();
				} else if (destinationType == typeof (InstanceDescriptor)) {
					if (prop != null){
						return new InstanceDescriptor (typeof (ImageFormat).GetTypeInfo ().GetProperty (prop), null);
					} else {
						ConstructorInfo ctor = typeof(ImageFormat).GetTypeInfo ().GetConstructor (new Type[] {typeof(Guid)} );
						return new InstanceDescriptor (ctor, new object[] {c.Guid});
					}
				}
			}
			
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			ImageFormat[] list = new ImageFormat [10];
			list [0] = ImageFormat.MemoryBmp;
			list [1] = ImageFormat.Bmp;
			list [2] = ImageFormat.Emf;
			list [3] = ImageFormat.Wmf;
			list [4] = ImageFormat.Gif;
			list [5] = ImageFormat.Jpeg;
			list [6] = ImageFormat.Png;
			list [7] = ImageFormat.Tiff;
			list [8] = ImageFormat.Exif;
			list [9] = ImageFormat.Icon;
			return new TypeConverter.StandardValuesCollection (list);
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
