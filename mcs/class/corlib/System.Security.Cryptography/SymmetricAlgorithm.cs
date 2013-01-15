//
// System.Security.Cryptography SymmetricAlgorithm Class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//   Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.InteropServices;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public abstract class SymmetricAlgorithm : IDisposable {
		protected int BlockSizeValue; 
		protected byte[] IVValue; 
		protected int KeySizeValue; 
		protected byte[] KeyValue; 
		protected KeySizes[] LegalBlockSizesValue; 
		protected KeySizes[] LegalKeySizesValue; 
		protected int FeedbackSizeValue;
		protected CipherMode ModeValue;
		protected PaddingMode PaddingValue;
		private bool m_disposed;

		protected SymmetricAlgorithm ()
		{
			ModeValue = CipherMode.CBC;
			PaddingValue = PaddingMode.PKCS7;
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose () 
#endif
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		public void Clear() 
		{
			Dispose (true);
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

		public virtual int BlockSize {
			get { return this.BlockSizeValue; }
			set {
				if (!KeySizes.IsLegalKeySize (this.LegalBlockSizesValue, value)) {
					throw new CryptographicException (
						Locale.GetText ("block size not supported by algorithm"));
				}
				// re-setting the same BlockSize *doesn't* regenerate the IV
				if (BlockSizeValue != value) {
					BlockSizeValue = value;
					IVValue = null;
				}
			}
		}

		public virtual int FeedbackSize {
			get { return this.FeedbackSizeValue; }
			set {
				if ((value <= 0) || (value > this.BlockSizeValue)) {
					throw new CryptographicException (
						Locale.GetText ("feedback size larger than block size"));
				}
				if ((value & 3) != 0) {
					throw new CryptographicException (
						Locale.GetText ("feedback size must be a multiple of 8 (bits)"));
				}
				this.FeedbackSizeValue = value;
			}
		}
		
		public virtual byte[] IV {
			get {
				if (this.IVValue == null)
					GenerateIV();

				return (byte[]) this.IVValue.Clone ();
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("IV");
				// 2.0 is stricter for IV length - which is bad for IV-less stream ciphers like RC4
				if ((value.Length << 3) != this.BlockSizeValue) {
					throw new CryptographicException (
						Locale.GetText ("IV length is different than block size"));
				}
				this.IVValue = (byte[]) value.Clone ();
			}
		}

		public virtual byte[] Key {
			get {
				if (this.KeyValue == null)
					GenerateKey();

				return (byte[]) this.KeyValue.Clone ();
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Key");

				int length = (value.Length << 3);
				if (!KeySizes.IsLegalKeySize (this.LegalKeySizesValue, length)) {
					throw new CryptographicException (
						Locale.GetText ("Key size not supported by algorithm"));
				}
				this.KeySizeValue = length;
				this.KeyValue = (byte[]) value.Clone ();
			}
		}
		
		public virtual int KeySize {
			get { return this.KeySizeValue; }
			set {
				if (!KeySizes.IsLegalKeySize (this.LegalKeySizesValue, value)) {
					throw new CryptographicException (
						Locale.GetText ("Key size not supported by algorithm"));
				}
				// re-setting the same KeySize *does* regenerate the key
				KeySizeValue = value;
				KeyValue = null;
			}
		}

		public virtual KeySizes[] LegalBlockSizes {
			get { return this.LegalBlockSizesValue; }
		}

		public virtual KeySizes[] LegalKeySizes {
			get { return this.LegalKeySizesValue; }
		}

		public virtual CipherMode Mode {
			get { return this.ModeValue; }
			set {
				if (!Enum.IsDefined (ModeValue.GetType (), value)) {
					throw new CryptographicException (
						Locale.GetText ("Cipher mode not available"));
				}
				
				this.ModeValue = value;
			}
		}

		public virtual PaddingMode Padding {
			get { return this.PaddingValue; }
			set {
				if (!Enum.IsDefined (PaddingValue.GetType (), value)) {
					throw new CryptographicException (
						Locale.GetText ("Padding mode not available"));
				}
				
				this.PaddingValue = value;
			}
		}

		public virtual ICryptoTransform CreateDecryptor () 
		{
			return CreateDecryptor (Key, IV);
		}

		public abstract ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV);

		public virtual ICryptoTransform CreateEncryptor() 
		{
			return CreateEncryptor (Key, IV);
		}

		public abstract ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV);

		public abstract void GenerateIV ();

		public abstract void GenerateKey ();

		public bool ValidKeySize (int bitLength) 
		{
			return KeySizes.IsLegalKeySize (LegalKeySizesValue, bitLength);
		}
		
		// LAMESPEC: Default is Rijndael - not TripleDES
		public static SymmetricAlgorithm Create () 
		{
#if FULL_AOT_RUNTIME
			return new System.Security.Cryptography.RijndaelManaged ();
#else
			return Create ("System.Security.Cryptography.SymmetricAlgorithm");
#endif
		}

		public static SymmetricAlgorithm Create (string algName) 
		{
			return (SymmetricAlgorithm) CryptoConfig.CreateFromName (algName);
		}
	}
}
