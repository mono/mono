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
		private byte[] block;
		private int blockSize;
		private int blockCount;

		public MACAlgorithm (SymmetricAlgorithm algorithm) 
		{
			algo = (SymmetricAlgorithm) algorithm;
			algo.Mode = CipherMode.CBC;
			algo.Padding = PaddingMode.Zeros;
			blockSize = (algo.BlockSize >> 3); // in bytes
			algo.IV = new byte [blockSize];
			block = new byte [blockSize];
		}
	
		public void Initialize (byte[] key) 
		{
			algo.Key = key;
			// note: the encryptor transform may be reusable - see Final
			if (enc == null) {
				enc = algo.CreateEncryptor ();
			}
			Array.Clear (block, 0, blockSize);
			blockCount = 0;
		}
		
		public void Core (byte[] rgb, int ib, int cb) 
		{
			// 1. fill the rest of the "block"
			int n = System.Math.Min (blockSize - blockCount, cb);
			Array.Copy (rgb, ib, block, blockCount, n); 
			blockCount += n;
	
			// 2. if block is full then transform it
			if (blockCount == blockSize) {
				enc.TransformBlock (block, 0, blockSize, block, 0);
	
				// 3. transform any other full block in specified buffer
				int b = (int) ((cb - n) / blockSize);
				for (int i=0; i < b; i++) {
					enc.TransformBlock (rgb, n, blockSize, block, 0);
					n += blockSize;
				}
	
				// 4. if data is still present fill the "block" with the remainder
				blockCount = cb - n;
				if (blockCount > 0)
					Array.Copy (rgb, n, block, 0, blockCount);
			}
		}
	
		public byte[] Final () 
		{
			byte[] result;
			if (blockCount > 0)
				result = enc.TransformFinalBlock (block, 0, blockCount);
			else {
#if NET_1_0
				// add an empty (zeros) block for MAC padding
				byte[] emptyBlock = new byte [blockSize];
				result = enc.TransformFinalBlock (emptyBlock, 0, blockSize);
#else
				result = (byte[]) block.Clone ();
#endif
			}
			if (!enc.CanReuseTransform) {
				enc.Dispose ();
				enc = null;
			}
			return result;
		}
	}
}