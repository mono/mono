//
// System.Attribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public abstract class Attribute {

		protected Attribute ()
		{
		}

		public virtual object TypeId {
			get {
				// TODO: Implement me
				return null;
			}
		}
	}
}
