//
// System.ComponentModel.RefreshPropertiesAttribute.cs
//
// Author:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
//
//

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.All)]
	public sealed class RefreshPropertiesAttribute : Attribute {

		#region Fields

		RefreshProperties refresh;

		#endregion // Fields
		
		public static readonly RefreshPropertiesAttribute All = new RefreshPropertiesAttribute (RefreshProperties.All);
		public static readonly RefreshPropertiesAttribute Default = new RefreshPropertiesAttribute (RefreshProperties.None);
		public static readonly RefreshPropertiesAttribute Repaint = new RefreshPropertiesAttribute (RefreshProperties.Repaint);

		#region Constructors

		public RefreshPropertiesAttribute (RefreshProperties refresh)
		{
			this.refresh = refresh;
		}

		#endregion // Constructors

		#region Properties

		public RefreshProperties RefreshProperties {
			get { return refresh; }
		}

		#endregion // Properties

		#region Methods

		public override bool Equals (object obj)
		{
			if (!(obj is RefreshPropertiesAttribute))
				return false;
			if (obj == this)
				return true;
			return ((RefreshPropertiesAttribute) obj).RefreshProperties == refresh;
		}

		public override int GetHashCode ()
		{
			return refresh.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return (this == RefreshPropertiesAttribute.Default);
		}

		#endregion // Methods
	}
}
