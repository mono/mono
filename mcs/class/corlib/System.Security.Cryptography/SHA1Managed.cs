//
// System.Security.Cryptography SHA1Managed Class implementation
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

// Note:
// The MS Framework includes two (almost) identical class for SHA1.
//	SHA1Managed (this file) is a 100% managed implementation.
//	SHA1CryptoServiceProvider is a wrapper on CryptoAPI.
// Mono must provide those two class for binary compatibility.
// In our case both class are wrappers around a managed internal class SHA1Internal.

public class SHA1Managed : SHA1 {

	private SHA1Internal sha;

	public SHA1Managed () 
	{
		sha = new SHA1Internal ();
	}

	protected override void HashCore (byte[] rgb, int start, int size) 
	{
		State = 1;
		sha.HashCore (rgb, start, size);
	}

	protected override byte[] HashFinal () 
	{
		State = 0;
		return sha.HashFinal ();
	}

	public override void Initialize () 
	{
		sha.Initialize ();
	}
}

}

