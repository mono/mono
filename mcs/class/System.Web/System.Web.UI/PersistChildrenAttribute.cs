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

		public static readonly PersistChildrenAttribute Default;
		public static readonly PersistChildrenAttribute Yes;
		public static readonly PersistChildrenAttribute No;

		public bool Persist {
			get { return persist; }
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return false;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		[MonoTODO]
		public override bool IsDefaultAttribute ()
		{
			return false;
		}
	}
}
	
