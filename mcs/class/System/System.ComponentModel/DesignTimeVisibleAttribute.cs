//
// System.ComponentModel.DesignTimeVisibleAttribute.cs
//
// Author:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class DesignTimeVisibleAttribute : Attribute 
	{
		#region Fields

		private bool visible;
		
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


		public override bool Equals (object obj)
		{
			if (!(obj is DesignTimeVisibleAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DesignTimeVisibleAttribute) obj).Visible == visible;
		}

		public override int GetHashCode ()
		{
			return visible.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return visible == DesignTimeVisibleAttribute.Default.Visible;
		}

		#endregion // Methods
	}
}
