//
// System.Security.Cryptography.Rijndael.cs
//
// Authors: Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
//

using System;

namespace System.Security.Cryptography {
	
	[MonoTODO]
	public abstract class Rijndael : SymmetricAlgorithm {
		public static new Rijndael Create () { return null; }
		public static new Rijndael Create (string alg) { return null; }
		
		public Rijndael () {
			KeySizeValue = 256;
			BlockSizeValue = 128;
			FeedbackSizeValue = 128;
		}
	}
}
