//
// System.ComponentModel.BindableAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//
//

namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.All)]
	public sealed class BindableAttribute : Attribute {

		#region Fields

		BindableSupport flags;
		bool bindable;

		#endregion // Fields
		
		public static readonly BindableAttribute No = new BindableAttribute (BindableSupport.No);
		public static readonly BindableAttribute Yes = new BindableAttribute (BindableSupport.Yes);
		public static readonly BindableAttribute Default = new BindableAttribute (BindableSupport.Default);

		#region Constructors

		public BindableAttribute (BindableSupport flags)
		{
			this.flags = flags;
			this.bindable = false;
		}

		public BindableAttribute (bool bindable)
		{
			this.bindable = bindable;
		}

		#endregion // Constructors

		#region Properties

		public bool Bindable {
			get { return bindable; }
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
