//
// System.ComponentModel.DesignTimeVisibleAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class DesignTimeVisibleAttribute : Attribute 
	{
		#region Fields

		bool visible;
		
		public static readonly DesignTimeVisibleAttribute Default = new DesignTimeVisibleAttribute (true);
		public static readonly DesignTimeVisibleAttribute No = new DesignTimeVisibleAttribute (false);
		public static readonly DesignTimeVisibleAttribute Yes = new DesignTimeVisibleAttribute (true);

		#endregion // Fields

		#region Constructors

		public DesignTimeVisibleAttribute ()
			: this (true)
		{
		}

		public DesignTimeVisibleAttribute (bool visible)
		{
			this.visible = visible; 
		}

		#endregion // Constructors

		#region Properties

		public bool Visible {
			get { return visible; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsDefaultAttribute ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
