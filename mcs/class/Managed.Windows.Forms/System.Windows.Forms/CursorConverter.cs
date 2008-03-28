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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Windows.Forms
{
	public class CursorConverter : TypeConverter
	{
		public CursorConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (byte []))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (byte []) || destinationType == typeof (InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
				CultureInfo culture, object value)
		{
			byte [] val = value as byte [];
			if (val == null)
				return base.ConvertFrom (context, culture, value);

			using (MemoryStream s = new MemoryStream (val)) {
				return new Cursor (s);
			}
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture,
				object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (value == null && destinationType == typeof (string))
				return "(none)";

			if ( !(value is Cursor))
				throw new ArgumentException("object must be of class Cursor", "value");

			if (destinationType == typeof (byte [])) {
				Cursor			c;
				SerializationInfo	si;

				if (value == null) {
					return new byte [0];
				}

				c = (Cursor)value;

				si = new SerializationInfo(typeof(Cursor), new FormatterConverter());
				((ISerializable)c).GetObjectData(si, new StreamingContext(StreamingContextStates.Remoting));

				return (byte[])si.GetValue("CursorData", typeof(byte[]));
			} else if (destinationType == typeof (InstanceDescriptor)) {
				PropertyInfo[] properties = typeof (Cursors).GetProperties ();
				foreach (PropertyInfo propInfo in properties) {
					if (propInfo.GetValue (null, null) == value) {
						return new InstanceDescriptor (propInfo, null);
					}
				}
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			PropertyInfo[] props = typeof (Cursors).GetProperties();
			
			ArrayList vals = new ArrayList ();

			for (int i = 0; i < props.Length; i++) {
				vals.Add (props [i].GetValue (null, null));
			}
			return new StandardValuesCollection (vals);
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
