//
// System.ComponentModel.NotifyParentPropertyAttribute.cs
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
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class NotifyParentPropertyAttribute : Attribute {

		#region Fields

		private bool notifyParent;

		#endregion // Fields
		
		public static readonly NotifyParentPropertyAttribute Default = new NotifyParentPropertyAttribute (false);
		public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute (false);
		public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute (true);

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

		public override bool Equals (object obj)
		{
			if (!(obj is NotifyParentPropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((NotifyParentPropertyAttribute) obj).NotifyParent == notifyParent;
		}

		public override int GetHashCode ()
		{
			return notifyParent.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return notifyParent == NotifyParentPropertyAttribute.Default.NotifyParent;
		}

		#endregion // Methods
	}
}
