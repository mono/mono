//
// System.Security.Cryptography.CryptographicException.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Runtime.Serialization;

namespace System.Security.Cryptography {

[Serializable]
public class CryptographicException : SystemException {

	// Constructors
	public CryptographicException ()
		: base ("Error occured during a cryptographic operation.")
	{
	}

	public CryptographicException (int hr)
	{
		HResult = hr;
	}

	public CryptographicException (string message)
		: base (message)
	{
	}

	public CryptographicException (string message, Exception inner)
		: base (message, inner)
	{
	}

	public CryptographicException (string format, string insert)
		: base (String.Format(format, insert))
	{
	}

	protected CryptographicException (SerializationInfo info, StreamingContext context)
		: base (info, context) 
	{
	}
}

}
