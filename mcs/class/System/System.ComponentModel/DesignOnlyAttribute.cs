//
// System.ComponentModel.DesignOnlyAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property)]
	public class DesignOnlyAttribute : Attribute {
		bool design_only;
		
		public static readonly DesignOnlyAttribute No;
		public static readonly DesignOnlyAttribute Yes;

		static DesignOnlyAttribute ()
		{
			No = new DesignOnlyAttribute (false);
			Yes = new DesignOnlyAttribute (false);
		}
		
		public DesignOnlyAttribute (bool design_only)
		{
			this.design_only = design_only;
		}

		public bool IsDesignOnly {
			get {
				return design_only;
			}
		}
	}
}
