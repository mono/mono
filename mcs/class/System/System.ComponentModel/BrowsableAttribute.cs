//
// System.ComponentModel.BrowsableAttribute.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.All)]
	public sealed class BrowsableAttribute : Attribute
	{
		private bool browsable;

		public static readonly BrowsableAttribute Default = new BrowsableAttribute (true);
		public static readonly BrowsableAttribute No = new BrowsableAttribute (false);
		public static readonly BrowsableAttribute Yes = new BrowsableAttribute (true);

		public BrowsableAttribute ()
		{
		}

		public BrowsableAttribute (bool browsable)
		{
			this.browsable = browsable;
		}

		public bool Browsable {
			get { return browsable; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is BrowsableAttribute))
				return false;
			if (obj == this)
				return true;
			return ((BrowsableAttribute) obj).Browsable == browsable;
		}

		public override int GetHashCode ()
		{
			return browsable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return browsable == BrowsableAttribute.Default.Browsable;
		}

	}
}

