//
// System.TypeLoadException
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	public class TypeLoadException : SystemException {
		// Constructors
		public TypeLoadException ()
			: base (Locale.GetText ("A type load exception has occurred."))
		{
		}

		public TypeLoadException (string message)
			: base (message)
		{
		}

		public TypeLoadException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
