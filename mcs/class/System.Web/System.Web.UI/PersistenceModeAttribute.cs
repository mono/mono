//
// System.Web.UI.PersistenceModeAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.All)]
	public sealed class PersistenceModeAttribute : Attribute
	{
		PersistenceMode mode;
		
		public PersistenceModeAttribute (PersistenceMode mode)
		{
			this.mode = mode;
		}

		public static readonly PersistenceModeAttribute Attribute;
		public static readonly PersistenceModeAttribute Default;
		public static readonly PersistenceModeAttribute EncodedInnerDefaultProperty;
		public static readonly PersistenceModeAttribute InnerDefaultProperty;
		public static readonly PersistenceModeAttribute InnerProperty;
		
		public PersistenceMode Mode {
			get { return mode; }
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
	
