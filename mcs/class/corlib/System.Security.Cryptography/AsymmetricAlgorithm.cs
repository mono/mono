//
// System.Security.Cryptography.AsymmetricAlgorithm Class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

	// to import keypairs parameters using MiniParser
	internal class AsymmetricParameters : MiniParser.IReader {
		private string xml;
		private int pos;

		public AsymmetricParameters (string xml) 
		{
			this.xml = xml;
			pos = 0;
		}

		public int Read () 
		{
			try {
				return (int) xml [pos++];
			}
			catch {
				return -1;
			}
		}
	}

	/// <summary>
	/// Abstract base class for all cryptographic asymmetric algorithms.
	/// Available algorithms include:
	/// RSA, DSA
	/// </summary>
	public abstract class AsymmetricAlgorithm : IDisposable	{

		protected int KeySizeValue; // The size of the secret key used by the symmetric algorithm in bits. 
		protected KeySizes[] LegalKeySizesValue; // Specifies the key sizes that are supported by the symmetric algorithm. 

		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		protected AsymmetricAlgorithm () {}
		
		/// <summary>
		/// Gets the key exchange algorithm
		/// </summary>
		public abstract string KeyExchangeAlgorithm {get;}
		
		/// <summary>
		/// Gets or sets the actual key size
		/// </summary>
		public virtual int KeySize {
			get { return this.KeySizeValue; }
			set {
				if (!IsLegalKeySize (this.LegalKeySizesValue, value))
					throw new CryptographicException("key size not supported by algorithm");
				
				this.KeySizeValue = value;
			}
		}

		/// <summary>
		/// Gets all legal key sizes
		/// </summary>
		public virtual KeySizes[] LegalKeySizes {
			get { return this.LegalKeySizesValue; }
		}

		/// <summary>
		/// Gets the signature algorithm
		/// </summary>
		public abstract string SignatureAlgorithm {get;}

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

		/// <summary>
		/// Reconstructs the AsymmetricAlgorithm Object from an XML-string
		/// </summary>
		public abstract void FromXmlString (string xmlString);
		
		/// <summary>
		/// Returns an XML string representation the current AsymmetricAlgorithm object
		/// </summary>
		public abstract string ToXmlString (bool includePrivateParameters);		
		
		private bool IsLegalKeySize (KeySizes[] LegalKeys, int Size) 
		{
			foreach (KeySizes LegalKeySize in LegalKeys) {
				for (int i=LegalKeySize.MinSize; i<=LegalKeySize.MaxSize; i+=LegalKeySize.SkipSize) {
					if (i == Size)
						return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Checks wether the given keyLength is valid for the current algorithm
		/// </summary>
		/// <param name="bitLength">the given keyLength</param>
		public bool ValidKeySize (int bitLength) 
		{
			return IsLegalKeySize (LegalKeySizesValue, bitLength);
		}
		
		/// <summary>
		/// Creates the default implementation of the default asymmetric algorithm (RSA).
		/// </summary>
		public static AsymmetricAlgorithm Create () 
		{
			return Create ("System.Security.Cryptography.AsymmetricAlgorithm");
		}
	
		/// <summary>
		/// Creates a specific implementation of the given asymmetric algorithm.
		/// </summary>
		/// <param name="algo">Specifies which derived class to create</param>
		public static AsymmetricAlgorithm Create (string algName) 
		{
			return (AsymmetricAlgorithm) CryptoConfig.CreateFromName (algName);
		}
	}
}

