//
// System.ArgumentNullException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System {

	[Serializable]
	public class ArgumentNullException : ArgumentException {
		// Constructors
		public ArgumentNullException ()
			: base (Locale.GetText ("Argument cannot be null"))
		{
		}

		public ArgumentNullException (string param_name)
			: base (Locale.GetText ("Argument cannot be null"), param_name)
		{
		}

		public ArgumentNullException (string param_name, string message)
			: base (message, param_name)
		{
		}

		protected ArgumentNullException (SerializationInfo info, StreamingContext sc)
			: base (info, sc)
		{
		}
	}
}
