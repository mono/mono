//
// System.Drawing.PointConverter.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
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
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for PointConverter.
	/// </summary>
	public class PointConverter : TypeConverter
	{
		public PointConverter()	{ }

		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
						   Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			if (destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			string s = value as string;
			if (s == null)
				return base.ConvertFrom (context, culture, value);

			// FIXME: use culture
			string [] subs = s.Split (',');
			if (subs.Length != 2)
				throw new ArgumentException ("Failed to parse Text(" + s + ") expected text in the format \"x, y.\"");

			int x = Int32.Parse (subs [0]);
			int y = Int32.Parse (subs [1]);

			return new Point (x, y);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			// LAMESPEC: "The default implementation calls the object's
			// ToString method if the object is valid and if the destination
			// type is string." MS does not behave as per the specs.
			// Oh well, we have to be compatible with MS.
			if ((destinationType == typeof (string)) && (value is Point))
				return ((Point) value).X + ", " + ((Point) value).Y;
			
			if (destinationType == typeof (InstanceDescriptor) && value is Point) {
				Point c = (Point)value;
				ConstructorInfo ctor = typeof(Point).GetConstructor (new Type[] {typeof(int), typeof(int)} );
				return new InstanceDescriptor (ctor, new object[] {c.X, c.Y });
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context,
						       IDictionary propertyValues)
		{
			int x = (int) propertyValues ["X"];
			int y = (int) propertyValues ["Y"];

			return new Point (x, y);
		}


		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties (
							ITypeDescriptorContext context,
							object value, Attribute[] attributes)
		{
			if (value is Point)
				return TypeDescriptor.GetProperties (value, attributes);

			return base.GetProperties (context, value, attributes);
		}
		
		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

	}
}
