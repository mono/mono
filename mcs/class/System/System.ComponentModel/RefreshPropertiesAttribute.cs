//
// System.ComponentModel.RefreshPropertiesAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public override bool IsDefaultAttribute ()
		{
			return (this == RefreshPropertiesAttribute.Default);
		}

		#endregion // Methods
	}
}
