//
// System.Drawing.ImageConverter.cs
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
using System.IO;
using System.Drawing.Imaging;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for ImageConverter.
	/// </summary>
	public class ImageConverter : TypeConverter
	{		
		public ImageConverter ()
		{
		}
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type srcType)
		{
			if (srcType == typeof (System.Byte[]))
				return true;
			else
				return false; 
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destType)
		{
			if ((destType == typeof (System.Byte[])) || (destType == typeof (System.String)))
				return true;
			else
				return false;
		}
		
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object val)
		{
			byte [] bytes = val as byte [];
			if (bytes == null)
				return base.ConvertFrom (context, culture, val);
			
			MemoryStream ms = new MemoryStream (bytes);
			
			return Image.FromStream (ms);	
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object val, Type destType )
		{
			if ((val is Image) && (destType == typeof (string)))
				return val.ToString ();
			else if (CanConvertTo (null, destType)){
				//came here means destType is byte array ;
				MemoryStream ms = new MemoryStream ();
				((Image)val).Save (ms, ((Image)val).RawFormat);
				return ms.GetBuffer ();
			}else
				return new NotSupportedException ("ImageConverter can not convert from " + val.GetType ());				
		}

		[MonoTODO ("Implement")]
		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object val, Attribute[] attribs )
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public override bool GetPropertiesSupported (ITypeDescriptorContext context )
		{
			throw new NotImplementedException (); 
		}
	}
}
