//
// System.ComponentModel.EnumConverter.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//
using System;
using System.Collections;
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
				if (value != null)
					return Enum.Format (type, value, "G");
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

		protected virtual IComparer Comparer {
			get { return new EnumConverter.EnumComparer (); }
		}

		protected Type EnumType {
			get { return type; }
		}

		protected TypeConverter.StandardValuesCollection Values {
			get { return stdValues;  }
			set { stdValues = value; }
		}

		private class EnumComparer : IComparer
		{
			int IComparer.Compare (object compareObject1, object compareObject2) 
			{
				string CompareString1 = (compareObject1 as string);
				string CompareString2 = (compareObject2 as string);
				if ((CompareString1 == null) || (CompareString2 == null))
					return Collections.Comparer.Default.Compare (compareObject1, compareObject2);
				return CultureInfo.InvariantCulture.CompareInfo.Compare (CompareString1, CompareString2);
			}
		}
	}

}

