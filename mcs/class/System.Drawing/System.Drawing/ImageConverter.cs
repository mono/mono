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
			if ((val is System.Drawing.Image) && (destType == typeof (System.String)))
				return val.ToString ();
			else if (CanConvertTo (null, destType)){
				//came here means destType is byte array ;
				MemoryStream ms = new MemoryStream ();
				((Image)val).Save (ms, ((Image)val).RawFormat);
				return ms.GetBuffer ();
			}else
				return new NotSupportedException ("ImageConverter can not convert from " + val.GetType ());				
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object val, Attribute[] attribs )
		{
			Type type = typeof (System.Drawing.Image);
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties (type); 
			PropertyDescriptorCollection returnPDC = pdc;
			if (attribs !=null) {
				int length = attribs.Length;
				foreach (PropertyDescriptor pd in pdc) {
					bool found = false;
					AttributeCollection attributes = pd.Attributes;
					//Apply the filter on the PropertyDescriptor
					//If any attribute is found from attribs, in 
					//PropertyDescriptor then retain that particular
					//PropertyDescriptor else discard it.
					for (int i=0; i<length; i++) {
						if (attributes.Contains (attribs [i])) {
							found = true;
							break;
						}
					}
					if (!found)
						returnPDC.Remove (pd);
				}
			}
			return returnPDC;	
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context )
		{
			return true; 
		}
	}
}
