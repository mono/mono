//
// System.AttributeUsage.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	[AttributeUsage(AttributeTargets.All)]
	[Serializable]
	public sealed class AttributeUsageAttribute : Attribute {
		AttributeTargets valid_on;
		bool allow_multiple, inherited;
		
		public AttributeUsageAttribute (AttributeTargets validOn)
		{
			valid_on = validOn;
		}

		public bool AllowMultiple {
			get {
				return allow_multiple;
			}

			set {
				allow_multiple = value;
			}
		}

		public bool Inherited {
			get {
				return inherited;
			}

			set {
				inherited = value;
			}
		}

		public AttributeTargets ValidOn {
			get {
				return valid_on;
			}
		}
	}
}

