//
// System.ComponentModel.DesignOnlyAttribute.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class DesignOnlyAttribute : Attribute
	{

		private bool design_only;

		public static readonly DesignOnlyAttribute Default = new DesignOnlyAttribute (false);
		public static readonly DesignOnlyAttribute No = new DesignOnlyAttribute (false);
		public static readonly DesignOnlyAttribute Yes = new DesignOnlyAttribute (true);

		public DesignOnlyAttribute (bool design_only)
		{
			this.design_only = design_only;
		}

		public bool IsDesignOnly {
			get { return design_only; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DesignOnlyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DesignOnlyAttribute) obj).IsDesignOnly == design_only;
		}

		public override int GetHashCode ()
		{
			return design_only.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return design_only == DesignOnlyAttribute.Default.IsDesignOnly;
		}
	}
}

