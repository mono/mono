//
// System.Security.Cryptography DSASignatureFormatter.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;

namespace System.Security.Cryptography {

	public class DSASignatureFormatter : AsymmetricSignatureFormatter {
	
		private DSA dsa;

		public DSASignatureFormatter () : base ()
		{
		}

		public DSASignatureFormatter (AsymmetricAlgorithm key) : base ()
		{
			SetKey (key);
		}

		public override byte[] CreateSignature (byte[] rgbHash)
		{
			if (dsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("missing key"));
			}

			return dsa.CreateSignature (rgbHash);
		}

		public override void SetHashAlgorithm (string strName)
		{
			if (strName == null)
				throw new ArgumentNullException ("strName");

			try {
				// just to test, we don't need the object
				SHA1 hash = SHA1.Create (strName);
			}
			catch (InvalidCastException) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("DSA requires SHA1"));
			}
		}

		public override void SetKey (AsymmetricAlgorithm key)
		{
			if (key != null) {
				// this will throw a InvalidCastException if this isn't
				// a DSA keypair
				dsa = (DSA) key;
			}
			// here null is accepted!
		}
	}
}
