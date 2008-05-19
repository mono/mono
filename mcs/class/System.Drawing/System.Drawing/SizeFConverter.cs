//
// Copyright (C) 2005, 2008 Novell, Inc (http://www.novell.com)
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
// Authors:
//
//	Dennis Hayes (dennish@Raytek.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ravindra (rkumar@novell.com)
//	Jordi Mas i Hernandez <jordimash@gmail.com>
//
//

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.Drawing
{
	public class SizeFConverter : TypeConverter
	{
		public SizeFConverter ()
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
			if (destinationType == typeof (string))
				return true;

			if (destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string s = value as string;
			if (s == null)
				return base.ConvertFrom (context, culture, value);

			string[] subs = s.Split (culture.TextInfo.ListSeparator.ToCharArray ());

			SingleConverter converter = new SingleConverter ();
			float[] numSubs = new float[subs.Length];
			for (int i = 0; i < numSubs.Length; i++) {
				numSubs[i] = (float) converter.ConvertFromString (context, culture, subs[i]);
			}

			if (subs.Length != 2)
				throw new ArgumentException ("Failed to parse Text(" + s + ") expected text in the format \"Width,Height.\"");

			return new SizeF (numSubs[0], numSubs[1]);

		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is SizeF) {
				SizeF size = (SizeF) value;
				if (destinationType == typeof (string)) {
					return size.Width.ToString (culture) + culture.TextInfo.ListSeparator
						+ " " + size.Height.ToString (culture);
				} else if (destinationType == typeof (InstanceDescriptor)) {
					ConstructorInfo ctor = typeof(SizeF).GetConstructor (new Type[] {typeof(float), typeof(float)});
					return new InstanceDescriptor (ctor, new object[] { size.Width, size.Height});
				}
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context, IDictionary propertyValues)
		{
			float w = (float) propertyValues ["Width"];
			float h = (float) propertyValues ["Height"];
			return new SizeF (w, h);
		}

		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			if (value is SizeF)
				return TypeDescriptor.GetProperties (value, attributes);

			return base.GetProperties (context, value, attributes);
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}

#endif

