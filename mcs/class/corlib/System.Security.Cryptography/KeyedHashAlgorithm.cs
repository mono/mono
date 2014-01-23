//
// KeyedHashAlgorithm.cs: Handles keyed hash and MAC classes.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

[ComVisible (true)]
public abstract class KeyedHashAlgorithm : HashAlgorithm {
	
	protected byte[] KeyValue;

	protected KeyedHashAlgorithm () : base () 
	{
		// create a random 64 bits key
	}

	public virtual byte[] Key {
		get { 
			return (byte[]) KeyValue.Clone (); 
		}
		set { 
			// can't change the key during a hashing ops
			if (State != 0) {
				throw new CryptographicException (
					Locale.GetText ("Key can't be changed at this state."));
			}
			// zeroize current key material for security
			ZeroizeKey ();
			// copy new key
			KeyValue = (byte[]) value.Clone (); 
		}
	}

	protected override void Dispose (bool disposing)
	{
                // zeroize key material for security
		ZeroizeKey();
		// dispose managed resources
                // none so far
		// dispose unmanaged resources 
                // none so far
		// calling base class HashAlgorithm
		base.Dispose (disposing);
	}

	private void ZeroizeKey() 
	{
		if (KeyValue != null)
			Array.Clear (KeyValue, 0, KeyValue.Length);
	}

	public static new KeyedHashAlgorithm Create ()
	{
#if FULL_AOT_RUNTIME
		return new System.Security.Cryptography.HMACSHA1 ();
#else
		return Create ("System.Security.Cryptography.KeyedHashAlgorithm");
#endif
	}

	public static new KeyedHashAlgorithm Create (string algName)
	{
		return (KeyedHashAlgorithm) CryptoConfig.CreateFromName (algName);
	}
}

}
