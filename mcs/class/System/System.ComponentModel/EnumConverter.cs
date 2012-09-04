//
// System.ComponentModel.EnumConverter.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ivan N. Zlatev (contact@i-nz.net)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
// (C) 2007 Ivan N. Zlatev
//

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
using System.Globalization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

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

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor))
				return true;

			if (destinationType == typeof (Enum[]))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (destinationType == typeof (string) && value != null) {
				// we need to be able to convert enum names,
				// integral values and other enum fields
				if (value is IConvertible) {
					Type underlyingType = Enum.GetUnderlyingType (type);
					if (underlyingType != value.GetType ()) {
						value = ((IConvertible) value).ToType (
							underlyingType, culture);
					}
				}

				if (!IsFlags && !IsValid (context, value))
					throw CreateValueNotValidException (value);

				return Enum.Format (type, value, "G");
			} else if (destinationType == typeof (InstanceDescriptor) && value != null) {
				string enumString = ConvertToString (context, culture, value);
				if (IsFlags && enumString.IndexOf (",") != -1) {
					if (value is IConvertible) {
						Type underlyingType = Enum.GetUnderlyingType (type);
						object enumValue = ((IConvertible)value).ToType (underlyingType, culture);
						MethodInfo toObjectMethod = typeof (Enum).GetMethod ("ToObject", new Type[] { typeof (Type), 
																			  underlyingType });
						return new InstanceDescriptor (toObjectMethod, new object[] { type, enumValue });
					}
				} else {
					FieldInfo f = type.GetField (enumString);
					if (f != null)
						return new InstanceDescriptor (f, null);
				}
			} else if (destinationType == typeof (Enum[]) && value != null) {
				if (!IsFlags) {
					return new Enum[] { (Enum) Enum.ToObject (type, value) };
				} else {
					long valueLong = Convert.ToInt64 (
						(Enum) value, culture);
					Array enumValArray = Enum.GetValues (type);
					long[] enumValues = new long[enumValArray.Length];
					for (int i=0; i < enumValArray.Length; i++)
						enumValues[i] = Convert.ToInt64 (enumValArray.GetValue (i));

					ArrayList enums = new ArrayList ();
					bool interrupt = false;
					while (!interrupt) {
						// interrupt the loop unless we find a match to avoid
						// looping indefinitely
						interrupt = true;
						foreach (long val in enumValues) {
							if ((val != 0 && ((val & valueLong) == val)) || val == valueLong) {
								enums.Add (Enum.ToObject (type, val));
								valueLong &= (~val);
								interrupt = false;
							}
						}
						if (valueLong == 0) // nothing left to do
							interrupt = true;
					}

					// add item for remainder
					if (valueLong != 0)
						enums.Add (Enum.ToObject (type, valueLong));

					return enums.ToArray (typeof(Enum));
				}
			}
			
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			if (sourceType == typeof (Enum[]))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			if (value is string) {
				string name = value as string;
				try {
					if (name.IndexOf (',') == -1)
						return Enum.Parse (type, name, true);

					long val = 0;
					string [] names = name.Split (',');
					foreach (string n in names) {
						Enum e = (Enum) Enum.Parse (type, n, true);
						val |= Convert.ToInt64 (e, culture);
					}
					return Enum.ToObject (type, val);
				} catch (Exception ex) {
					throw new FormatException (name + " is " +
						"not a valid value for " +
						type.Name, ex);
				}
			} else if (value is Enum[]) {
				long val = 0;
				foreach (Enum e in (Enum[])value)
					val |= Convert.ToInt64 (e, culture);
				return Enum.ToObject (type, val);
			}

			return base.ConvertFrom (context, culture, value);
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
			return !IsFlags;
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

		ArgumentException CreateValueNotValidException (object value)
		{
			string msg = string.Format (CultureInfo.InvariantCulture,
				"The value '{0}' is not a valid value for the " +
				"enum '{1}'", value, type.Name);
			return new ArgumentException (msg);
		}

		bool IsFlags {
			get {
				return (type.IsDefined (typeof (FlagsAttribute),
					false));
			}
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
