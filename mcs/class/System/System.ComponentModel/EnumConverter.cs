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

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (destinationType == typeof (string))
				if (value != null)
					return Enum.Format (type, value, "G");
					
			if (destinationType == typeof (InstanceDescriptor) && type.IsInstanceOfType (value)) {
				FieldInfo f = type.GetField (value.ToString ());
				if (f == null) throw new ArgumentException (string.Format ("The value '{0}' is not a valid value for the enum '{1}'", value, type));
				return new InstanceDescriptor (f, null);
			}
			
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

			return Enum.Parse (type, val, true);
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

