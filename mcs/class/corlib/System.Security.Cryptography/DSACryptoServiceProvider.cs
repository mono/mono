//
// System.Security.Cryptography.DSACryptoServiceProvider.cs
//
// Authors:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;
using System.IO;

namespace System.Security.Cryptography {
	
	[MonoTODO]
	public class DSACryptoServiceProvider : DSA {
		public DSACryptoServiceProvider () { }
		public DSACryptoServiceProvider (CspParameters parameters) { }
		public DSACryptoServiceProvider (int key_size) { }
		public DSACryptoServiceProvider (int key_size, CspParameters parameters) { }

		public override string KeyExchangeAlgorithm {
			get { return null; }
		}

		public override int KeySize {
			get { return 0; }
		}

		public override KeySizes[] LegalKeySizes {
			get { return null; }
		}

		public override string SignatureAlgorithm {
			get { return null; }
		}

		public bool PersistKeyInCsp {
			get { return false; }
			set { }
		}
		
		protected override void Dispose (bool disposing) {}

		public override byte[] CreateSignature (byte[] rgb) {
			return null;
		}
		
		public override bool VerifySignature(byte[] hash, byte[] sig) {
			return false;
		}

		public byte[] SignData (byte[] data) {
			return SignData (data, 0, data.Length);
		}

		public byte[] SignData (byte[] data, int offset, int count) {
			return null;
		}

		public byte[] SignData (Stream data) {
			return null;
		}

		public byte[] SignHash (byte[] hash, string str) {
			return null;
		}

		public bool VerifyData (byte[] data, byte[] sig) {
			return false;
		}

		public override DSAParameters ExportParameters (bool include) {
			return new DSAParameters ();
		}

		public override void ImportParameters (DSAParameters parameters) {
		}

		public override void FromXmlString(string xmlString) {
		}

		public override string ToXmlString(bool includePrivateParameters) {
			return null;
		}
	}
}
