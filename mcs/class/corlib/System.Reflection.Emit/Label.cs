//
// System.Reflection.Emit/Label.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Reflection.Emit {
	public struct Label {
		public int label;

		public override bool Equals (object obj) {
			return false;
		}

		public override int GetHashCode () {
			return label;
		}
	}
}
