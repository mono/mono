//
// System.Enum.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

namespace System {

	public abstract class Enum {

		public static bool IsDefined (Type enum_type, object value)
		{
			return false;
		}
		public override string ToString() {
			return null;
		}
		public string ToString( IFormatProvider provider) {
			return null;
		}
		public string ToString(String format) {
			return null;
		}
		public string ToString(String format, IFormatProvider provider) {
			return null;
		}

	}

}
