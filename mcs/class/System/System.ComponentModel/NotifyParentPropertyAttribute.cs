//
// System.ComponentModel.NotifyParentPropertyAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//
//

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class NotifyParentPropertyAttribute : Attribute {

		#region Fields

		bool notifyParent;

		#endregion // Fields
		
		public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute (false);
		public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute (true);
		public static readonly NotifyParentPropertyAttribute Default = new NotifyParentPropertyAttribute (false);

		#region Constructors

		public NotifyParentPropertyAttribute (bool notifyParent)
		{
			this.notifyParent = notifyParent;
		}

		#endregion // Constructors

		#region Properties

		public bool NotifyParent {
			get { return notifyParent; }
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

		[MonoTODO]
		public override bool IsDefaultAttribute ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
