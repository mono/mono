//
// StrongNamePublicKeyBlob.cs: Strong Name Public Key Blob
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

namespace System.Security.Permissions {

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
		char ch = Char.ToLower (c);
		
		if (Char.IsDigit (ch))
			return (byte) (ch - '0');
		else 
			return (byte) (ch - 'a' + 10);
	}
	
	public override bool Equals (object obj) 
	{
		bool result = (obj is StrongNamePublicKeyBlob);
		if (result) {
			StrongNamePublicKeyBlob snpkb = (obj as StrongNamePublicKeyBlob);
			result = (pubkey.Length == snpkb.pubkey.Length);
			if (result) {
				for (int i = 0; i < pubkey.Length; i++) {
					if (pubkey[i] != snpkb.pubkey[i])
						return false;
				}
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
