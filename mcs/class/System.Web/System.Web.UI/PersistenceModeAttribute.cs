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

		public static readonly PersistenceModeAttribute Attribute =
						new PersistenceModeAttribute (PersistenceMode.Attribute);

		public static readonly PersistenceModeAttribute Default =
						new PersistenceModeAttribute (PersistenceMode.Attribute);

		public static readonly PersistenceModeAttribute EncodedInnerDefaultProperty =
						new PersistenceModeAttribute (PersistenceMode.EncodedInnerDefaultProperty);

		public static readonly PersistenceModeAttribute InnerDefaultProperty =
						new PersistenceModeAttribute (PersistenceMode.InnerDefaultProperty);

		public static readonly PersistenceModeAttribute InnerProperty =
						new PersistenceModeAttribute (PersistenceMode.InnerProperty);
		
		public PersistenceMode Mode {
			get { return mode; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is PersistenceModeAttribute))
				return false;

			return ((PersistenceModeAttribute) obj).mode == mode;
		}

		public override int GetHashCode ()
		{
			return (int) mode;
		}

		public override bool IsDefaultAttribute ()
		{
			return (mode == PersistenceMode.Attribute);
		}
	}
}
	
