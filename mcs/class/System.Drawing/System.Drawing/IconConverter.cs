                                                                                                             //
// System.Drawing.IconConverter.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing {
	/// <summary>
	/// Summary description for IconConverter.
	/// </summary>
	public class IconConverter : ExpandableObjectConverter
	{
		public IconConverter()
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
			
			return new Icon (ms);				
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object val, Type destType )
		{
			if ((val is Icon) && (destType == typeof (string)))
				return val.ToString ();
			else if (CanConvertTo (null, destType)) {
				//came here means destType is byte array ;
				MemoryStream ms = new MemoryStream ();
				((Icon)val).Save (ms);
				return ms.GetBuffer ();
			}else
				return new NotSupportedException ("IconConverter can not convert from " + val.GetType ());				
		}
	}
}
