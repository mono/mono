//
// ColumnStyle.cs
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
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2004 Novell, Inc.
//
namespace System.Windows.Forms 
{
	public class ColumnStyle : TableLayoutStyle 
	{
		float width;
		
		public ColumnStyle ()
		{
			this.width = 0;
		}

		public ColumnStyle (SizeType sizeType) 
		{
			this.width = 0;
			base.SizeType = sizeType;
		}

		public ColumnStyle (SizeType sizeType, float width)
		{
			if (width < 0)
				throw new ArgumentOutOfRangeException ("height");

			base.SizeType = sizeType;
			this.width = width;
		}

		public float Width {
			get { return this.width; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				
				if (width != value) {
					width = value; 
					if (Owner != null)
						Owner.PerformLayout (Owner, "Style");
				}
			}
		}
	}
}
