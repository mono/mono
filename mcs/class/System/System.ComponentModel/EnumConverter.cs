//
// System.ComponentModel.EnumConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Globalization;

namespace System.ComponentModel
{
	public class EnumConverter : TypeConverter
	{
		private Type type;
		private StandardValuesCollection stdValues;

		public EnumConverter (Type type)
		{
			this.type = type;
		}

		[MonoTODO]
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return base.CanConvertTo (context, destinationType);
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (destinationType == typeof (string))
				return value.ToString ();
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			string val = value as string;
			if (val == null)
				return base.ConvertFrom(context, culture, value);

			string [] subValues = val.Split (new char [] {','});
					
			long longResult = 0;
			foreach (string s in subValues)
				longResult |= (long) Enum.Parse (type, s, true);

			return Enum.ToObject (type, longResult);
		}

		public override bool IsValid (ITypeDescriptorContext context, object value)
		{
			return Enum.IsDefined (type, value);
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return !(type.IsDefined (typeof (FlagsAttribute), false));
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			if (stdValues == null) {
				Array values = Enum.GetValues (type);
				Array.Sort (values);
				stdValues = new StandardValuesCollection (values);
			}
			return stdValues;
		}
	}

}

