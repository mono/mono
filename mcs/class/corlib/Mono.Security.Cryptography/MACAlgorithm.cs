//
// MACAlgorithm.cs: Handles MAC with any symmetric algorithm
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

	// References:
	// a.	FIPS PUB 81: DES MODES OF OPERATION 
	//	MAC: Appendix F (MACDES not MACTripleDES but close enough ;-)
	//	http://www.itl.nist.gov/fipspubs/fip81.htm
	
	// Generic MAC mechanims - most of the work is done in here
	// It should work with any symmetric algorithm function e.g. DES for MACDES (fips81)
	internal class MACAlgorithm {

		private SymmetricAlgorithm algo;
		private ICryptoTransform enc;
		private BlockProcessor block;
	
		public MACAlgorithm (SymmetricAlgorithm algorithm)
		{
			algo = (SymmetricAlgorithm) algorithm;
			algo.Mode = CipherMode.CBC;
			algo.Padding = PaddingMode.Zeros;
			algo.IV = new byte [(algo.BlockSize >> 3)];
		}
	
		public void Initialize (byte[] key) 
		{
			algo.Key = key;
			// note: the encryptor transform amy be reused - see Final
			if (enc == null) {
				enc = algo.CreateEncryptor ();
				block = new BlockProcessor (enc);
			}
			block.Initialize ();
		}
		
		public void Core (byte[] rgb, int ib, int cb) 
		{
			block.Core (rgb, ib, cb);
		}
	
		public byte[] Final () 
		{
			byte[] mac = block.Final ();
			if (!enc.CanReuseTransform) {
				enc.Dispose();
				enc = null;
			}
			return mac;
		}
	}
}