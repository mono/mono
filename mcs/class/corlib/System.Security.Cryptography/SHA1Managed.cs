//
// System.Security.Cryptography SHA1Managed Class implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

namespace System.Security.Cryptography {

// Note:
// The MS Framework includes two (almost) identical class for SHA1.
//	SHA1Managed (this file) is a 100% managed implementation.
//	SHA1CryptoServiceProvider is a wrapper on CryptoAPI.
// Mono must provide those two class for binayry compatibility.
// In our case both class are wrappers around a managed internal class SHA1Internal.

public sealed class SHA1Managed : SHA1 {

	private SHA1Internal sha;

	public SHA1Managed () 
	{
		sha = new SHA1Internal ();
	}

	~SHA1Managed () 
	{
		Dispose (false);
	}

	protected override void Dispose (bool disposing) 
	{
		// nothing new to do (managed implementation)
		base.Dispose (disposing);
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

	private void ProcessBlock (byte[] inputBuffer, int inputOffset) 
	{
		sha.ProcessBlock (inputBuffer, inputOffset);
	}

	private void ProcessFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
	{
		sha.ProcessFinalBlock (inputBuffer, inputOffset, inputCount);
	}
}

}

