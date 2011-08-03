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
// Copyright (c) 2007 Novell, Inc.
//

using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.ComponentModel.Design.Serialization; // e.g. InstanceDescriptor

namespace System.Windows.Forms
{
	public class PaddingConverter : TypeConverter
	{
		public PaddingConverter ()
		{
		}

		#region Public Instance Methods
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
				
			return false;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
			else if (destinationType == typeof(InstanceDescriptor))
				return true;

			return false;
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if ((value == null) || !(value is String))
				return base.ConvertFrom (context, culture, value);

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			string[] parts = ((string)value).Split (culture.TextInfo.ListSeparator.ToCharArray ());

			return new Padding (int.Parse (parts[0].Trim ()), int.Parse (parts[1].Trim ()), int.Parse (parts[2].Trim ()), int.Parse (parts[3].Trim ()));
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (value is Padding) {
				Padding p = (Padding)value;
				if (destinationType == typeof (string)) {
					if (culture == null)
						culture = CultureInfo.CurrentCulture;
					return string.Format ("{0}{4} {1}{4} {2}{4} {3}", p.Left, p.Top, p.Right, p.Bottom, culture.TextInfo.ListSeparator);
				} else if (destinationType == typeof (InstanceDescriptor)) {
					Type[] types;
					Object[] values;
					if (p.All != -1) {
						types = new Type[] { typeof(int) };
						values = new Object[] { p.All };
					} else {
						types = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };
						values = new Object[] { p.Left, p.Top, p.Right, p.Bottom };
					}
					ConstructorInfo ci = typeof(Padding).GetConstructor (types);
					return new InstanceDescriptor (ci, values);
				}
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context, IDictionary propertyValues)
		{
			if (propertyValues == null)
				throw new ArgumentNullException ("propertyValues");
			if (context == null)
				throw new ArgumentNullException ("context");

			Padding old = (Padding)context.PropertyDescriptor.GetValue (context.Instance);
			if (old.All == (int)propertyValues["All"])
				return new Padding ((int)propertyValues["Left"], (int)propertyValues["Top"], (int)propertyValues["Right"], (int)propertyValues["Bottom"]);
			else
				return new Padding ((int)propertyValues["All"]);
		}
		
		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties (typeof (Padding), attributes);
		}
		
		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
	}
}
