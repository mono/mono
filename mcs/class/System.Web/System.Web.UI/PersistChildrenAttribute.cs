//
// System.Web.UI.PersistChildrenAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class PersistChildrenAttribute : Attribute
	{
		bool persist;
		
		public PersistChildrenAttribute (bool persist)
		{
			this.persist = persist;
		}

		public static readonly PersistChildrenAttribute Default = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute Yes = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute No = new PersistChildrenAttribute (false);

		public bool Persist {
			get { return persist; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is PersistChildrenAttribute))
				return false;

			return (((PersistChildrenAttribute) obj).persist == persist);
		}

		public override int GetHashCode ()
		{
			return persist ? 1 : 0;
		}

		public override bool IsDefaultAttribute ()
		{
			return (persist == true);
		}
	}
}

