//
// System.ComponentModel.BrowsableAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.All)]
	public sealed class BrowsableAttribute : Attribute {
		bool browsable;
		
		public static readonly BrowsableAttribute No;
		public static readonly BrowsableAttribute Yes;

		static BrowsableAttribute ()
		{
			No = new BrowsableAttribute (false);
			Yes = new BrowsableAttribute (false);
		}
		
		public BrowsableAttribute (bool browsable)
		{
			this.browsable = browsable;
		}

		public bool Browsable {
			get {
				return browsable;
			}
		}
		
	}
}
