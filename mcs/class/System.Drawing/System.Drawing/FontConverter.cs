//
// System.Drawing.FontConverter.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002,2003 Ximian, Inc.  http://www.ximian.com
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
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Drawing
{
	public class FontConverter : TypeConverter
	{
		public FontConverter ()
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
			if (destinationType == typeof (String))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if ((destinationType == typeof (string)) && (value is Font))
				return value.ToString ();

			return base.ConvertTo (context, culture, value, destinationType);
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

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value,
									    Attribute [] attributes)
		{
			if (value is Font)
				return TypeDescriptor.GetProperties (value, attributes);

			return base.GetProperties (context, value, attributes);
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

			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string))
					return true;

				return base.CanConvertFrom (context, sourceType);
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

			[MonoTODO]
			~FontNameConverter ()
			{
				throw new NotImplementedException ();
			}
		}

		public class FontUnitConverter : EnumConverter
		{
			public FontUnitConverter () : base (typeof (GraphicsUnit))
			{
			}

			[MonoTODO]
			public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
			{
				throw new NotImplementedException ();
			}
		}
	}
}
