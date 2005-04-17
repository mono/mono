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
using System.Globalization;


namespace System.Windows.Forms {

	public class CursorConverter : TypeConverter {

		public CursorConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type source_type)
		{
			if (source_type == typeof (byte []))
				return true;
			return base.CanConvertFrom (context, source_type);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type dest_type)
		{
			if (dest_type == typeof (byte []))
				return true;
			return base.CanConvertTo (context, dest_type);
		}

		[MonoTODO ("Waiting on Cursor::.ctor (Stream)")]
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

		[MonoTODO ("Waiting on Cursor::Draw")]
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture,
				object value, Type dest_type)
		{
			if (dest_type == null)
				throw new ArgumentNullException ("destinationType");

			if (dest_type == typeof (byte [])) {
				
				if (value == null)
					return new byte [0];

				using (MemoryStream s = new MemoryStream ()) {
					Cursor val = value as Cursor;
					
					Bitmap b = new Bitmap (val.Size.Width, val.Size.Height);
					using (Graphics g  = Graphics.FromImage (b)) {
						// This isn't implemented in Cursor yet

						// Rectangle r = new Rectangle (0, 0,
						// val.Size.Width, val.Size.Height);
						//	  val.Draw (g, r);
					}
					b.Save (s, ImageFormat.Bmp);
					return s.ToArray ();
				}
			}
			return base.ConvertTo (context, culture, value, dest_type);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties (typeof (Cursors));
			ArrayList vals = new ArrayList ();

			for (int i = 0; i < props.Count; i++)
				vals.Add (props [i].GetValue (null));
			return new StandardValuesCollection (vals);
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}

}

