//
// System.Security.Cryptography.RSA.cs
//
// Authors:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;

namespace System.Security.Cryptography {
	
	[MonoTODO]
	public abstract class RSA : AsymmetricAlgorithm {
		public static new RSA Create () { return null; }
		public static new RSA Create (string alg) { return null; }
	
		public RSA () { }

		public abstract byte[] EncryptValue (byte[] rgb);
		public abstract byte[] DecryptValue (byte[] rgb);

		public abstract RSAParameters ExportParameters (bool include);
		public abstract void ImportParameters (RSAParameters parameters);

		public override void FromXmlString (string xml) {
		}

		public override string ToXmlString (bool include) {
			return null;
		}
	}
}
