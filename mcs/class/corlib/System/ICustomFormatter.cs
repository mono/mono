//
// System.ICustomFormatter.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	interface ICustomFormatter {
		string Format (string format, object arg, IFormatProvider formatProvider);
	}
}
