//
// System.ComponentModel.BindableAttribute.cs
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
	public sealed class BindableAttribute : Attribute {

		#region Fields

		//BindableSupport flags;
		private bool bindable;

		#endregion // Fields
		
		public static readonly BindableAttribute No = new BindableAttribute (BindableSupport.No);
		public static readonly BindableAttribute Yes = new BindableAttribute (BindableSupport.Yes);
		public static readonly BindableAttribute Default = new BindableAttribute (BindableSupport.Default);

		#region Constructors

		public BindableAttribute (BindableSupport flags)
		{
			//this.flags = flags;
			if (flags == BindableSupport.No)
				this.bindable = false;
				
			if (flags == BindableSupport.Yes || flags == BindableSupport.Default)
				this.bindable = true;
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

		public override bool Equals (object obj)
		{
			if (!(obj is BindableAttribute))
				return false;

			if (obj == this)
				return true;

			return ((BindableAttribute) obj).Bindable == bindable;
		}

		public override int GetHashCode ()
		{
			return bindable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return bindable == BindableAttribute.Default.Bindable;
		}

		#endregion // Methods
	}
}

