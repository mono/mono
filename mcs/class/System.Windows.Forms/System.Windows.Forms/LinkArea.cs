//
// System.Drawing.LinkArea.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
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

using System.ComponentModel;

namespace System.Windows.Forms
{
	[Serializable]
	[TypeConverter (typeof (LinkArea.LinkAreaConverter))]
	public struct LinkArea
	{
		private int start;
		private int length;

		// -----------------------
		// Public Constructor
		// -----------------------

		public LinkArea (int start, int length)
		{
			this.start = start;
			this.length = length;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool IsEmpty {
			get {
				if (start != 0)
					return false;
				return (length == 0);
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is LinkArea)) {
				return false;
			}

			LinkArea comp = (LinkArea) o;
			return (comp.Start == start && comp.Length == length);
		}

		public override int GetHashCode ()
		{
			return start << 4 | length;
		}

		public int Start {
			get { return start; }
			set { start = value; }
		}

		public int Length {
			get { return length; }
			set { length = value; }
		}

		[MonoTODO ("Implement")]
		public class LinkAreaConverter : TypeConverter
		{
			//Implement
		}
	}
}
