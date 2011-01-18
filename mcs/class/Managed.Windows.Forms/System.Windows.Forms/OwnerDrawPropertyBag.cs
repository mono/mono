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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	[Serializable]
	public class OwnerDrawPropertyBag : MarshalByRefObject, ISerializable {

		private Color fore_color;
		private Color back_color;
		private Font font;

		internal OwnerDrawPropertyBag ()
		{
			fore_color = back_color = Color.Empty;
		}

		private OwnerDrawPropertyBag (Color fore_color, Color back_color, Font font)
		{
			this.fore_color = fore_color;
			this.back_color = back_color;
			this.font = font;
		}

		protected OwnerDrawPropertyBag(SerializationInfo info, StreamingContext context) {
			SerializationInfoEnumerator	en;
			SerializationEntry		e;

			en = info.GetEnumerator();

			while (en.MoveNext()) {
				e = en.Current;
				switch(e.Name) {
					case "Font": font = (Font)e.Value; break;
					case "ForeColor": fore_color = (Color)e.Value; break;
					case "BackColor": back_color = (Color)e.Value; break;
				}
			}
		}


		public Color ForeColor {
			get { return fore_color; }
			set { fore_color = value; }
		}

		public Color BackColor {
			get { return back_color; }
			set { back_color = value; }
		}

		public Font Font {
			get { return font; }
			set { font = value; }
		}

		public virtual bool IsEmpty ()
		{
			return (font == null && fore_color.IsEmpty && back_color.IsEmpty);
		}

		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context)
		{
			si.AddValue ("BackColor", BackColor);
			si.AddValue ("ForeColor", ForeColor);
			si.AddValue ("Font", Font);
		}

		public static OwnerDrawPropertyBag Copy (OwnerDrawPropertyBag value)
		{
			return new OwnerDrawPropertyBag (value.ForeColor, value.BackColor, value.Font);
		}
	}
}


