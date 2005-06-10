//
// StrongNamePublicKeyBlob.cs: Strong Name Public Key Blob
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Permissions {

#if NET_2_0
[ComVisible (true)]
#endif
[Serializable]
public sealed class StrongNamePublicKeyBlob {

	internal byte[] pubkey;

	public StrongNamePublicKeyBlob (byte[] publicKey) 
	{
		if (publicKey == null)
			throw new ArgumentNullException ("publicKey");
		// Note: No sanity check ?
		pubkey = publicKey;
	}

	internal static StrongNamePublicKeyBlob FromString (string s)
	{
		if ((s == null) || (s.Length == 0))
			return null;

		int length = s.Length / 2;
	
		byte [] array = new byte [length];

		for (int i = 0, j = 0; i < s.Length; i += 2, j ++) {
			byte left = CharToByte (s [i]);
			byte right = CharToByte (s [i+1]);
			array [j] = Convert.ToByte (left * 16 + right);
		}
		
		return new StrongNamePublicKeyBlob (array);
	}

	static byte CharToByte (char c)
	{
		char ch = Char.ToLowerInvariant (c);
		
		if (Char.IsDigit (ch))
			return (byte) (ch - '0');
		else 
			return (byte) (ch - 'a' + 10);
	}
	
	public override bool Equals (object obj) 
	{
		StrongNamePublicKeyBlob snpkb = (obj as StrongNamePublicKeyBlob);
		if (snpkb == null)
			return false;

		bool result = (pubkey.Length == snpkb.pubkey.Length);
		if (result) {
			for (int i = 0; i < pubkey.Length; i++) {
				if (pubkey[i] != snpkb.pubkey[i])
					return false;
			}
		}
		return result;
	}

	// LAMESPEC: non standard get hash code - (a) Why ??? (b) How ???
	// It seems to be the first four bytes of the public key data
	// which seems like non sense as all valid public key will have the same header ?
	public override int GetHashCode () 
	{
		int hash = 0;
		int i = 0;
		// a BAD public key can be less than 4 bytes
		int n = Math.Min (pubkey.Length, 4);
		while (i < n)
			hash = (hash << 8) + pubkey [i++];
		return hash;
	}

	public override string ToString () 
	{
		StringBuilder sb = new StringBuilder ();
		for (int i=0; i < pubkey.Length; i++)
			sb.Append (pubkey[i].ToString ("X2"));
		return sb.ToString ();
	}
}

}
