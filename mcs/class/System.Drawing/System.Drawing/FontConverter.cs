//
// System.Drawing.FontConverter.cs
//
// Authors:
// 	Dennis Hayes (dennish@Raytek.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Drawing
{
	public class FontConverter : TypeConverter
	{
		public FontConverter()
		{
		}

		[MonoTODO]
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object CreateInstance (ITypeDescriptorContext context,
						       IDictionary propertyValues)
		{
			throw new NotImplementedException ();
		}

		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		[MonoTODO]
		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value,
									    Attribute [] attributes)
		{
			throw new NotImplementedException ();
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public sealed class FontNameConverter : TypeConverter
		{
			public FontNameConverter ()
			{
			}

			[MonoTODO]
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override object ConvertFrom (ITypeDescriptorContext context,
							    CultureInfo culture,
							    object value)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
			{
				throw new NotImplementedException ();
			}

			public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
			{
				return true;
			}
		}
	}
}

