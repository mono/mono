//
// System.Drawing.LinkArea.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
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
