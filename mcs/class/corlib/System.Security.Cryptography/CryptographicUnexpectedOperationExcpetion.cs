//
// System.Security.Cryptography.CryptographicUnexpectedOperationException.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Runtime.Serialization;

namespace System.Security.Cryptography {

[Serializable]
public class CryptographicUnexpectedOperationException : CryptographicException {
	// Constructors
	public CryptographicUnexpectedOperationException ()
		: base ("Error occured during a cryptographic operation.")
	{
		// Default to CORSEC_E_CRYPTO_UNEX_OPER (CorError.h)
		HResult = unchecked ((int)0x80131431);
	}

	public CryptographicUnexpectedOperationException (string message)
		: base (message)
	{
		HResult = unchecked ((int)0x80131431);
	}

	public CryptographicUnexpectedOperationException (string message, Exception inner)
		: base (message, inner)
	{
		HResult = unchecked ((int)0x80131431);
	}

	public CryptographicUnexpectedOperationException (string format, string insert)
		: base (String.Format(format, insert))
	{
		HResult = unchecked ((int)0x80131431);
	}

	protected CryptographicUnexpectedOperationException (SerializationInfo info, StreamingContext context)
		: base (info, context) 
	{
	}
}

}
