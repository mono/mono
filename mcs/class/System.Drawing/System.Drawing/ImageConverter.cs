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

namespace System.Drawing
{
	/// <summary>
	/// Summary description for ImageConverter.
	/// </summary>
	public class ImageConverter : TypeConverter
	{
		
		[MonoTODO ("Implement")]
		public ImageConverter ()
		{
		}

		[MonoTODO ("Implement")]
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type srcType)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destType)
		{
			throw new NotImplementedException (); 
		}
		
		[MonoTODO ("Implement")]
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object val)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object val, Type destType )
		{
			throw new NotImplementedException (); 
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
