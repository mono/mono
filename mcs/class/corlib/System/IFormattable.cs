//
// System.IFormattable.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	interface IFormattable {
		public string ToString (string format, IFormatProvider format_provider);
	}
}
