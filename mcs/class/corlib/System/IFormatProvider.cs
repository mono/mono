//
// System.IFormatProvider.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public interface IFormatProvider {
		object GetFormat (Type format_type);
	}
}
