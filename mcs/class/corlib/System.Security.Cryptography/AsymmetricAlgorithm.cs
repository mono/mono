//
// System.Security.Cryptography.AsymmetricAlgorithm Class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;

namespace System.Security.Cryptography {

	public abstract class AsymmetricAlgorithm : IDisposable	{

		protected int KeySizeValue;
		protected KeySizes[] LegalKeySizesValue; 

		protected AsymmetricAlgorithm ()
		{
		}
		
		public abstract string KeyExchangeAlgorithm {
			get;
		}
		
		public virtual int KeySize {
			get { return this.KeySizeValue; }
			set {
				if (!KeySizes.IsLegalKeySize (this.LegalKeySizesValue, value))
					throw new CryptographicException (Locale.GetText ("Key size not supported by algorithm."));
				
				this.KeySizeValue = value;
			}
		}

		public virtual KeySizes[] LegalKeySizes {
			get { return this.LegalKeySizesValue; }
		}

		public abstract string SignatureAlgorithm {
			get;
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		public void Clear () 
		{
			Dispose (false);
		}

		protected abstract void Dispose (bool disposing);

		public abstract void FromXmlString (string xmlString);
		
		public abstract string ToXmlString (bool includePrivateParameters);		
		
		public static AsymmetricAlgorithm Create () 
		{
			return Create ("System.Security.Cryptography.AsymmetricAlgorithm");
		}
	
		public static AsymmetricAlgorithm Create (string algName) 
		{
			return (AsymmetricAlgorithm) CryptoConfig.CreateFromName (algName);
		}
	}
}
