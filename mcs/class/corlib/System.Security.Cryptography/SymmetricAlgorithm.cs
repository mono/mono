//
// System.Security.Cryptography SymmetricAlgorithm Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

	// This class implement most of the common code required for symmetric
	// algorithm transforms, like:
	// - CipherMode: Builds CBC and CFB on top of (descendant supplied) ECB
	// - PaddingMode, transform properties, multiple blocks, reuse...
	//
	// Descendants MUST:
	// - intialize themselves (like key expansion, ...)
	// - override the ECB (Electronic Code Book) method which will only be
	//   called using BlockSize byte[] array.
	internal abstract class SymmetricTransform : ICryptoTransform {
		protected SymmetricAlgorithm algo;
		protected bool encrypt;
		private int BlockSizeByte;
		private byte[] temp;
		private byte[] temp2;
		private int FeedBackByte;
		private int FeedBackIter;
		private bool m_disposed = false;

		public SymmetricTransform (SymmetricAlgorithm symmAlgo, bool encryption, byte[] rgbIV) 
		{
			algo = symmAlgo;
			encrypt = encryption;
			BlockSizeByte = (algo.BlockSize >> 3);
			temp = new byte [BlockSizeByte];
			Array.Copy (rgbIV, 0, temp, 0, BlockSizeByte);
			temp2 = new byte [BlockSizeByte];
			FeedBackByte = (algo.FeedbackSize >> 3);
			FeedBackIter = (int) BlockSizeByte / FeedBackByte;
		}

		~SymmetricTransform () 
		{
			Dispose (false);
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		// MUST be overriden by classes using unmanaged ressources
		// the override method must call the base class
		protected void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				if (disposing) {
					// dispose managed object: zeroize and free
					Array.Clear (temp, 0, BlockSizeByte);
					temp = null;
					Array.Clear (temp2, 0, BlockSizeByte);
					temp2 = null;
				}
				m_disposed = true;
			}
		}

		public virtual bool CanTransformMultipleBlocks {
			get { return false; }
		}

		public bool CanReuseTransform {
			get { return false; }
		}

		public virtual int InputBlockSize {
			get { return BlockSizeByte; }
		}

		public virtual int OutputBlockSize {
			get { return BlockSizeByte; }
		}

		// note: Each block MUST be BlockSizeValue in size!!!
		// i.e. Any padding must be done before calling this method
		protected void Transform (byte[] input, byte[] output) 
		{
			switch (algo.Mode) {
			case CipherMode.ECB:
				ECB (input, output);
				break;
			case CipherMode.CBC:
				CBC (input, output);
				break;
			case CipherMode.CFB:
				CFB (input, output);
				break;
			case CipherMode.OFB:
				OFB (input, output);
				break;
			case CipherMode.CTS:
				CTS (input, output);
				break;
			default:
				throw new NotImplementedException ("Unkown CipherMode" + algo.Mode.ToString ());
			}
		}

		// Electronic Code Book (ECB)
		protected abstract void ECB (byte[] input, byte[] output); 

		// Cipher-Block-Chaining (CBC)
		protected virtual void CBC (byte[] input, byte[] output) 
		{
			if (encrypt) {
				for (int i = 0; i < BlockSizeByte; i++)
					temp[i] ^= input[i];
				ECB (temp, output);
				Array.Copy (output, 0, temp, 0, BlockSizeByte);
			}
			else {
				Array.Copy (input, 0, temp2, 0, BlockSizeByte);
				ECB (input, output);
				for (int i = 0; i < BlockSizeByte; i++)
					output[i] ^= temp[i];
				Array.Copy (temp2, 0, temp, 0, BlockSizeByte);
			}
		}

		// Cipher-FeedBack (CFB)
		protected virtual void CFB (byte[] input, byte[] output) 
		{
			if (encrypt) {
				for (int x = 0; x < FeedBackIter; x++) {
					// temp is first initialized with the IV
					ECB (temp, temp2);

					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
					Array.Copy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Array.Copy (output, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
				}
			}
			else {
				for (int x = 0; x < FeedBackIter; x++) {
					// we do not really decrypt this data!
					encrypt = true;
					// temp is first initialized with the IV
					ECB (temp, temp2);
					encrypt = false;

					Array.Copy (temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
					Array.Copy (input, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
					for (int i = 0; i < FeedBackByte; i++)
						output[i + x] = (byte)(temp2[i] ^ input[i + x]);
				}
			}
		}

		// Output-FeedBack (OFB)
		protected virtual void OFB (byte[] input, byte[] output) 
		{
			throw new NotImplementedException ("OFB not yet supported");
		}

		// Cipher Text Stealing (CTS)
		protected virtual void CTS (byte[] input, byte[] output) 
		{
			throw new NotImplementedException ("CTS not yet supported");
		}

		public virtual int TransformBlock (byte [] inputBuffer, int inputOffset, int inputCount, byte [] outputBuffer, int outputOffset) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");

			// if ((inputCount & (BlockSizeByte-1)) != 0) didn't work for Rijndael block = 192
			if ((inputCount % BlockSizeByte) != 0)
				throw new CryptographicException ("Invalid input block size.");

			if (outputOffset + inputCount > outputBuffer.Length)
				throw new CryptographicException ("Insufficient output buffer size.");

			int step = InputBlockSize;
			int offs = inputOffset;
			int full = inputCount / step;

			byte [] workBuff = new byte [step];
			byte[] workout =  new byte [step];

			for (int i = 0; i < full; i++) {
				Array.Copy (inputBuffer, offs, workBuff, 0, step);
				Transform (workBuff, workout);
				Array.Copy (workout, 0, outputBuffer, outputOffset, step);
				offs += step;
				outputOffset += step;
			}

			return (full * step);
		}

		public virtual byte [] TransformFinalBlock (byte [] inputBuffer, int inputOffset, int inputCount) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("Object is disposed");

			int num = (inputCount + BlockSizeByte) & (~(BlockSizeByte-1));
			byte [] res = new byte [num];
			//int full = (num - BlockSizeByte); // didn't work for bs 192
			int full = (inputCount / BlockSizeByte) * BlockSizeByte;

			// are there still full block to process ?
			if (full > 0)
				TransformBlock (inputBuffer, inputOffset, full, res, 0);

			//int rem = inputCount & (BlockSizeByte-1);
			int rem = inputCount - full; 

			// is there a partial block to pad ?
			if (rem > 0) {
				if (encrypt) {
					int padding = BlockSizeByte - (inputCount % BlockSizeByte);
					switch (algo.Padding) {
					case PaddingMode.None:
						break;
					case PaddingMode.PKCS7:
						for (int i = BlockSizeByte; --i >= (BlockSizeByte - padding);) 
							res [i] = (byte) padding;
						break;
					case PaddingMode.Zeros:
						for (int i = BlockSizeByte; --i >= (BlockSizeByte - padding);)
							res [i] = 0;
						break;
					}

					Array.Copy (inputBuffer, inputOffset + full, res, full, rem);

					// the last padded block will be transformed in-place
					TransformBlock (res, full, BlockSizeByte, res, full);
				}
				else {
					switch (algo.Padding) {
					case PaddingMode.None:
						break;
					case PaddingMode.PKCS7:
						byte padding = res [inputCount - 1];
						for (int i = 0; i < padding; i++) {
							if (res [inputCount - 1 - i] == padding)
								res[inputCount - 1 - i] = 0x00;
						}
						break;
					case PaddingMode.Zeros:
						break;
					}
				}
			}
			return res;
		}

	}

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
				if (IsLegalKeySize(this.LegalBlockSizesValue, value))
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
		[MonoTODO]
		public virtual byte[] IV {
			get {
				if (this.IVValue == null)
					GenerateIV();

				return this.IVValue;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("tried setting initial vector to null");
					
				if (value.Length * 8 != this.BlockSizeValue)
					throw new CryptographicException ("IV length must match block size");
				
				this.IVValue = new byte [value.Length];
				System.Array.Copy (value, 0, this.IVValue, 0, value.Length);
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
					throw new ArgumentNullException ("tried setting key to null");

				if (!IsLegalKeySize (this.LegalKeySizesValue, value.Length * 8))
					throw new CryptographicException ("key size not supported by algorithm");

				this.KeySizeValue = value.Length * 8;
				this.KeyValue = new byte [value.Length];
				System.Array.Copy (value, 0, this.KeyValue, 0, value.Length);
			}
		}
		
		/// <summary>
		/// Gets or sets the actual key size in bits
		/// </summary>
		public virtual int KeySize {
			get { return this.KeySizeValue; }
			set {
				if (!IsLegalKeySize (this.LegalKeySizesValue, value))
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

		internal bool IsLegalKeySize (KeySizes[] LegalKeys, int Size) 
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

