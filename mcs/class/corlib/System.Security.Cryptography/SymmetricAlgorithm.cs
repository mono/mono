//
// System.Security.Cryptography SymmetricAlgorithm Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	/// <summary>
	/// Abstract base class for all cryptographic symmetric algorithms.
	/// Available algorithms include:
	/// DES, RC2, Rijndael, TripleDES
	/// </summary>
	public abstract class SymmetricAlgorithm : IDisposable {
		protected int BlockSizeValue; // The block size of the cryptographic operation in bits. 
		protected int FeedbackSizeValue; // The feedback size of the cryptographic operation in bits. 
		protected byte[] IVValue; // The initialization vector ( IV) for the symmetric algorithm. 
		protected int KeySizeValue; // The size of the secret key used by the symmetric algorithm in bits. 
		protected byte[] KeyValue; // The secret key for the symmetric algorithm. 
		protected KeySizes[] LegalBlockSizesValue; // Specifies the block sizes that are supported by the symmetric algorithm. 
		protected KeySizes[] LegalKeySizesValue; // Specifies the key sizes that are supported by the symmetric algorithm. 
		protected CipherMode ModeValue; // Represents the cipher mode used in the symmetric algorithm. 
		protected PaddingMode PaddingValue; // Represents the padding mode used in the symmetric algorithm. 
		private bool m_disposed;

		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		public SymmetricAlgorithm () 
		{
			ModeValue = CipherMode.CBC;
			PaddingValue = PaddingMode.PKCS7;
			m_disposed = false;
		}
		
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		~SymmetricAlgorithm () 
		{
			Dispose (false);
		}

		public void Clear() 
		{
			Dispose (true);
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		protected virtual void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// always zeroize keys
				if (KeyValue != null) {
					// Zeroize the secret key and free
					Array.Clear (KeyValue, 0, KeyValue.Length);
					KeyValue = null;
				}
				// dispose unmanaged managed objects
				if (disposing) {
					// dispose managed objects
				}
				m_disposed = true;
			}
		}

		/// <summary>
		/// Gets or sets the actual BlockSize
		/// </summary>
		public virtual int BlockSize {
			get { return this.BlockSizeValue; }
			set {
				if (KeySizes.IsLegalKeySize (this.LegalBlockSizesValue, value))
					this.BlockSizeValue = value;
				else
					throw new CryptographicException("block size not supported by algorithm");
			}
		}

		/// <summary>
		/// Gets or sets the actual FeedbackSize
		/// </summary>
		public virtual int FeedbackSize {
			get { return this.FeedbackSizeValue; }
			set {
				if (value > this.BlockSizeValue)
					throw new CryptographicException("feedback size larger than block size");
				else
					this.FeedbackSizeValue = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the actual Initial Vector
		/// </summary>
		public virtual byte[] IV {
			get {
				if (this.IVValue == null)
					GenerateIV();

				return this.IVValue;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("IV");
					
				if ((value.Length << 3) > this.BlockSizeValue)
					throw new CryptographicException ("IV length cannot be larger than block size");

				this.IVValue = (byte[]) value.Clone ();
			}
		}

		/// <summary>
		/// Gets or sets the actual key
		/// </summary>
		public virtual byte[] Key {
			get {
				if (this.KeyValue == null)
					GenerateKey();

				return this.KeyValue;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Key");

				int length = (value.Length << 3);
				if (!KeySizes.IsLegalKeySize (this.LegalKeySizesValue, length))
					throw new CryptographicException ("key size not supported by algorithm");

				this.KeySizeValue = length;
				this.KeyValue = (byte[]) value.Clone ();
			}
		}
		
		/// <summary>
		/// Gets or sets the actual key size in bits
		/// </summary>
		public virtual int KeySize {
			get { return this.KeySizeValue; }
			set {
				if (!KeySizes.IsLegalKeySize (this.LegalKeySizesValue, value))
					throw new CryptographicException ("key size not supported by algorithm");
				
				this.KeyValue = null;
				this.KeySizeValue = value;
			}
		}

		/// <summary>
		/// Gets all legal block sizes
		/// </summary>
		public virtual KeySizes[] LegalBlockSizes {
			get { return this.LegalBlockSizesValue; }
		}

		/// <summary>
		/// Gets all legal key sizes
		/// </summary>
		public virtual KeySizes[] LegalKeySizes {
			get { return this.LegalKeySizesValue; }
		}

		/// <summary>
		/// Gets or sets the actual cipher mode
		/// </summary>
		public virtual CipherMode Mode {
			get { return this.ModeValue; }
			set {
				if (Enum.IsDefined( ModeValue.GetType (), value))
					this.ModeValue = value;
				else
					throw new CryptographicException ("padding mode not available");
			}
		}

		/// <summary>
		/// Gets or sets the actual padding
		/// </summary>
		public virtual PaddingMode Padding {
			get { return this.PaddingValue; }
			set {
				if (Enum.IsDefined (PaddingValue.GetType (), value))
					this.PaddingValue = value;
				else
					throw new CryptographicException ("padding mode not available");
			}
		}

		/// <summary>
		/// Gets an Decryptor transform object to work with a CryptoStream
		/// </summary>
		public virtual ICryptoTransform CreateDecryptor () 
		{
			return CreateDecryptor (Key, IV);
		}

		/// <summary>
		/// Gets an Decryptor transform object to work with a CryptoStream
		/// </summary>
		public abstract ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV);

		/// <summary>
		/// Gets an Encryptor transform object to work with a CryptoStream
		/// </summary>
		public virtual ICryptoTransform CreateEncryptor() 
		{
			return CreateEncryptor (Key, IV);
		}

		/// <summary>
		/// Gets an Encryptor transform object to work with a CryptoStream
		/// </summary>
		public abstract ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV);

		/// <summary>
		/// used to generate an inital vector if none is specified
		/// </summary>
		public abstract void GenerateIV ();

		/// </summary>
		/// used to generate a random key if none is specified
		/// </summary>
		public abstract void GenerateKey ();

		/// <summary>
		/// Checks wether the given keyLength is valid for the current algorithm
		/// </summary>
		/// <param name="bitLength">the given keyLength</param>
		public bool ValidKeySize (int bitLength) 
		{
			return KeySizes.IsLegalKeySize (LegalKeySizesValue, bitLength);
		}
		
		/// <summary>
		/// Creates the default implementation of the default symmetric algorithm (Rijndael).
		/// </summary>
		// LAMESPEC: Default is Rijndael - not TripleDES
		public static SymmetricAlgorithm Create () 
		{
			return Create ("System.Security.Cryptography.SymmetricAlgorithm");
		}

		/// <summary>
		/// Creates a specific implementation of the given symmetric algorithm.
		/// </summary>
		/// <param name="algName">Specifies which derived class to create</param>
		public static SymmetricAlgorithm Create (string algName) 
		{
			return (SymmetricAlgorithm) CryptoConfig.CreateFromName (algName);
		}
	}
}
