//
// MACTripleDES.cs: Handles MAC with TripleDES
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Security.Cryptography;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 81: DES MODES OF OPERATION 
//	MAC: Appendix F (MACDES not MACTripleDES but close enough ;-)
//	http://www.itl.nist.gov/fipspubs/fip81.htm

// Generic MAC mechanims - most of the work is done in here
// It should work with any symmetric algorithm function e.g. DES for MACDES (fips81)
internal class MACAlgorithm {
	protected SymmetricAlgorithm algo;
	private ICryptoTransform enc;
	private CryptoStream stream;
	private MemoryStream ms;
	private int BlockSize;	// in bytes (not in bits)

	public MACAlgorithm (string algoName, CipherMode algoMode) 
	{
		if ((algoMode != CipherMode.CBC) && (algoMode != CipherMode.CFB))
			throw new CryptographicException();

		algo = (SymmetricAlgorithm) CryptoConfig.CreateFromName (algoName);
		algo.Mode = algoMode;
		algo.Padding = PaddingMode.Zeros;
		BlockSize = (algo.BlockSize >> 3);
	}

	~MACAlgorithm () 
	{
		Dispose ();
	}

	public void Dispose () 
	{
		ZeroizeKey ();
		// algo.Clear (); not yet present in SymmetricAlgorithm
	}

	public SymmetricAlgorithm Algo {
		get { return algo; }
	}

	public byte[] Key {
		get { return algo.Key; }
		set { algo.Key = value; }
	}

	public byte[] IV {
		get { return algo.IV; }
		set { algo.IV = value; }
	}

	[MonoTODO()]
	public void Initialize () 
	{
		if (algo.Mode == CipherMode.CBC)
			algo.IV = new Byte [BlockSize];
		enc = algo.CreateEncryptor();
		// TODO Change MemoryStream (unrealistic for big continuous streams)
		ms = new MemoryStream ();
		stream = new CryptoStream (ms, enc, CryptoStreamMode.Write);
	}
	
	[MonoTODO("")]
	public void Core (byte[] rgb, int ib, int cb) 
	{
		if (enc == null)
			Initialize ();

		stream.Write (rgb, ib, cb);
	}

	[MonoTODO("How should it finish? encrypting the last block?")]
	public byte[] Final () 
	{
		stream.FlushFinalBlock ();
		byte[] mac = new byte [BlockSize];
		ms.Position -= BlockSize;
		ms.Read (mac, 0, BlockSize);
		return mac;
	}

	public void ZeroizeKey () 
	{
		// well maybe the algo did it - but better twice than none
		if (algo.Key != null)
			Array.Clear (algo.Key, 0, algo.Key.Length);
	}
}

// LAMESPEC: MACTripleDES == MAC-CBC using TripleDES (not MAC-CFB).
// LAMESPEC: Unlike FIPS81 or FIPS113 the result is encrypted twice (ANSI like?)
public class MACTripleDES: KeyedHashAlgorithm {
	private MACAlgorithm mac;

	public MACTripleDES () 
	{
		mac = new MACAlgorithm ("TripleDES", CipherMode.CBC);
		HashSizeValue = mac.Algo.BlockSize;
	}

	public MACTripleDES (byte[] rgbKey)
	{
		MACAlgorithm mac = new MACAlgorithm ("TripleDES", CipherMode.CBC);
		HashSizeValue = mac.Algo.BlockSize;
		mac.Key = rgbKey;
	}

	public MACTripleDES (string strTripleDES, byte[] rgbKey)
	{
		MACAlgorithm mac = new MACAlgorithm (strTripleDES, CipherMode.CBC);
		HashSizeValue = mac.Algo.BlockSize;
		mac.Key = rgbKey;
	}

	~MACTripleDES () 
	{
		Dispose (false);
	}

	protected override void Dispose (bool disposing) 
	{
		if (mac != null)
			mac.Dispose();
		base.Dispose (disposing);
	}

	public override void Initialize () 
	{
		State = 0;
		mac.Initialize ();
	}

	protected override void HashCore (byte[] rgb, int ib, int cb) 
	{
		State = 1;
		mac.Core (rgb, ib, cb);
	}

	protected override byte[] HashFinal () 
	{
		State = 0;
		return mac.Final ();
	}
}

}