//
// System.ComponentModel.DescriptionAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Event)]
	public class DescriptionAttribute : Attribute {
		string desc;
			
		public DescriptionAttribute (string name)
		{
			desc = name;
		}

		public DescriptionAttribute ()
		{
			desc = "";
		}

		public virtual string Description {
			get {
				return DescriptionValue;
			}
		}

		//
		// Notice that the default Description implementation uses this by default
		//
		protected string DescriptionValue {
			get {
				return desc;
			}

			set {
				desc = value;
			}
		}
	}
}
