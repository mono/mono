//
// System.IFormattable.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public interface IFormattable {
		string ToString (string format, IFormatProvider format_provider);
	}
}
