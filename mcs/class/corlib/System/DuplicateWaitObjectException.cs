//
// System.DuplicateWaitObjectException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class DuplicateWaitObjectException : ArgumentException {
		// Constructors
		public DuplicateWaitObjectException ()
			: base (Locale.GetText ("Duplicate objects in argument"))
		{
		}

		public DuplicateWaitObjectException (string param_name)
			: base (Locale.GetText ("Duplicate objects in argument"), param_name)
		{
		}

		public DuplicateWaitObjectException (string param_name, string message)
			: base (message, param_name)
		{
		}

		protected DuplicateWaitObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
