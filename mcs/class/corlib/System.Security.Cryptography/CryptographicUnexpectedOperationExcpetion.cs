//
// System.Security.Cryptography.CryptographicUnexpectedOperationException.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;

namespace System.Security.Cryptography {

	public class CryptographicUnexpectedOperationException : CryptographicException {
		// Constructors
		public CryptographicUnexpectedOperationException ()
			: base ("Error occured during a cryptographic operation.")
		{
		}

		public CryptographicUnexpectedOperationException (string message)
			: base (message)
		{
		}

		public CryptographicUnexpectedOperationException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public CryptographicUnexpectedOperationException (string format, string insert)
			: base (String.Format(format, insert))
		{
		}
	}
}
