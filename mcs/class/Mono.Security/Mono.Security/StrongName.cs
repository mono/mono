//
// StrongName.cs - Strong Name Implementation
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

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

using System;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Security {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class StrongName {

		internal class StrongNameSignature {
			private byte[] hash;
			private byte[] signature;
			private UInt32 signaturePosition;
			private UInt32 signatureLength;
			private UInt32 metadataPosition;
			private UInt32 metadataLength;
			private byte cliFlag;
			private UInt32 cliFlagPosition;

			public byte[] Hash {
				get { return hash; }
				set { hash = value; }
			}

			public byte[] Signature {
				get { return signature; }
				set { signature = value; }
			}

			public UInt32 MetadataPosition {
				get { return metadataPosition; }
				set { metadataPosition = value; }
			}

			public UInt32 MetadataLength {
				get { return metadataLength; }
				set { metadataLength = value; }
			}

			public UInt32 SignaturePosition {
				get { return signaturePosition; }
				set { signaturePosition = value; }
			}

			public UInt32 SignatureLength {
				get { return signatureLength; }
				set { signatureLength = value; }
			}

			// delay signed -> flag = 0x01
			// strongsigned -> flag = 0x09
			public byte CliFlag {
				get { return cliFlag; }
				set { cliFlag = value; }
			}

			public UInt32 CliFlagPosition {
				get { return cliFlagPosition; }
				set { cliFlagPosition = value; }
			}
		}

		internal enum StrongNameOptions {
			Metadata,
			Signature
		}

		private RSA rsa;
		private byte[] publicKey;
		private byte[] keyToken;
		private string tokenAlgorithm;

		public StrongName ()
		{
		}

		public StrongName (int keySize)
		{
			rsa = new RSAManaged (keySize);
		}

		public StrongName (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			// check for ECMA key
			if (data.Length == 16) {
				int i = 0;
				int sum = 0;
				while (i < data.Length)
					sum += data [i++];
				if (sum == 4) {
					// it is the ECMA key
					publicKey = (byte[]) data.Clone ();
				}
			}
			else {
				RSA = CryptoConvert.FromCapiKeyBlob (data);
				if (rsa == null)
					throw new ArgumentException ("data isn't a correctly encoded RSA public key");
			}
		}

		public StrongName (RSA rsa)
		{
			if (rsa == null)
				throw new ArgumentNullException ("rsa");

			RSA = rsa;
		}

		private void InvalidateCache () 
		{
			publicKey = null;
			keyToken = null;
		}

		public bool CanSign {
			get {
				if (rsa == null)
					return false;
#if INSIDE_CORLIB
				// the easy way
				if (RSA is RSACryptoServiceProvider) {
					// available as internal for corlib
					return !(rsa as RSACryptoServiceProvider).PublicOnly;
				}
				else 
#endif
				if (RSA is RSAManaged) {
					return !(rsa as RSAManaged).PublicOnly;
				}
				else {
					// the hard way
					try {
						RSAParameters p = rsa.ExportParameters (true);
						return ((p.D != null) && (p.P != null) && (p.Q != null));
					}
					catch (CryptographicException) {
						return false;
					}
				}
			}
		}

		public RSA RSA {
			get {
				// if none then we create a new keypair
				if (rsa == null)
					rsa = (RSA) RSA.Create ();
				return rsa; 
			}
			set { 
				rsa = value;
				InvalidateCache ();
			}
		}

		public byte[] PublicKey {
			get { 
				if (publicKey == null) {
					byte[] keyPair = CryptoConvert.ToCapiKeyBlob (rsa, false);
					// since 2.0 public keys can vary from 384 to 16384 bits
					publicKey = new byte [32 + (rsa.KeySize >> 3)];

					// The first 12 bytes are documented at:
					// http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
					// ALG_ID - Signature
					publicKey [0] = keyPair [4];
					publicKey [1] = keyPair [5];	
					publicKey [2] = keyPair [6];	
					publicKey [3] = keyPair [7];	
					// ALG_ID - Hash (SHA1 == 0x8004)
					publicKey [4] = 0x04;
					publicKey [5] = 0x80;
					publicKey [6] = 0x00;
					publicKey [7] = 0x00;
					// Length of Public Key (in bytes)
					byte[] lastPart = BitConverterLE.GetBytes (publicKey.Length - 12);
					publicKey [8] = lastPart [0];
					publicKey [9] = lastPart [1];
					publicKey [10] = lastPart [2];
					publicKey [11] = lastPart [3];
					// Ok from here - Same structure as keypair - expect for public key
					publicKey [12] = 0x06;		// PUBLICKEYBLOB
					// we can copy this part
					Buffer.BlockCopy (keyPair, 1, publicKey, 13, publicKey.Length - 13);
					// and make a small adjustment 
					publicKey [23] = 0x31;		// (RSA1 not RSA2)
				}
				return (byte[]) publicKey.Clone ();
			}
		}

		public byte[] PublicKeyToken {
			get {
				if (keyToken == null) {
					byte[] publicKey = PublicKey;
					if (publicKey == null)
						return null;
					HashAlgorithm ha = HashAlgorithm.Create (TokenAlgorithm);
					byte[] hash = ha.ComputeHash (publicKey);
					// we need the last 8 bytes in reverse order
					keyToken = new byte [8];
					Buffer.BlockCopy (hash, (hash.Length - 8), keyToken, 0, 8);
					Array.Reverse (keyToken, 0, 8);
				}
				return (byte[]) keyToken.Clone ();
			}
		}

		public string TokenAlgorithm {
			get { 
				if (tokenAlgorithm == null)
					tokenAlgorithm = "SHA1";
				return tokenAlgorithm; 
			}
			set {
				string algo = value.ToUpper (CultureInfo.InvariantCulture);
				if ((algo == "SHA1") || (algo == "MD5")) {
					tokenAlgorithm = value;
					InvalidateCache ();
				}
				else
					throw new ArgumentException ("Unsupported hash algorithm for token");
			}
		}

		public byte[] GetBytes () 
		{
			return CryptoConvert.ToCapiPrivateKeyBlob (RSA);
		}

		private UInt32 RVAtoPosition (UInt32 r, int sections, byte[] headers) 
		{
			for (int i=0; i < sections; i++) {
				UInt32 p = BitConverterLE.ToUInt32 (headers, i * 40 + 20);
				UInt32 s = BitConverterLE.ToUInt32 (headers, i * 40 + 12);
				int l = (int) BitConverterLE.ToUInt32 (headers, i * 40 + 8);
				if ((s <= r) && (r < s + l)) {
					return p + r - s;
				}
			}
			return 0;
		}

		internal StrongNameSignature StrongHash (Stream stream, StrongNameOptions options)
		{
			StrongNameSignature info = new StrongNameSignature ();

			HashAlgorithm hash = HashAlgorithm.Create (TokenAlgorithm);
			CryptoStream cs = new CryptoStream (Stream.Null, hash, CryptoStreamMode.Write);

			// MS-DOS Header - always 128 bytes
			// ref: Section 24.2.1, Partition II Metadata
			byte[] mz = new byte [128];
			stream.Read (mz, 0, 128);
			if (BitConverterLE.ToUInt16 (mz, 0) != 0x5a4d)
				return null;
			UInt32 peHeader = BitConverterLE.ToUInt32 (mz, 60);
			cs.Write (mz, 0, 128);
			if (peHeader != 128) {
				byte[] mzextra = new byte [peHeader - 128];
				stream.Read (mzextra, 0, mzextra.Length);
				cs.Write (mzextra, 0, mzextra.Length);
			}

			// PE File Header - always 248 bytes
			// ref: Section 24.2.2, Partition II Metadata
			byte[] pe = new byte [248];
			stream.Read (pe, 0, 248);
			if (BitConverterLE.ToUInt32 (pe, 0) != 0x4550)
				return null;
			if (BitConverterLE.ToUInt16 (pe, 4) != 0x14c)
				return null;
			// MUST zeroize both CheckSum and Security Directory
			byte[] v = new byte [8];
			Buffer.BlockCopy (v, 0, pe, 88, 4);
			Buffer.BlockCopy (v, 0, pe, 152, 8);
			cs.Write (pe, 0, 248);

			UInt16 numSection = BitConverterLE.ToUInt16 (pe, 6);
			int sectionLength = (numSection * 40);
			byte[] sectionHeaders = new byte [sectionLength];
			stream.Read (sectionHeaders, 0, sectionLength);
			cs.Write (sectionHeaders, 0, sectionLength);

			UInt32 cliHeaderRVA = BitConverterLE.ToUInt32 (pe, 232);
			UInt32 cliHeaderPos = RVAtoPosition (cliHeaderRVA, numSection, sectionHeaders);
			int cliHeaderSiz = (int) BitConverterLE.ToUInt32 (pe, 236);

			// CLI Header
			// ref: Section 24.3.3, Partition II Metadata
			byte[] cli = new byte [cliHeaderSiz];
			stream.Position = cliHeaderPos;
			stream.Read (cli, 0, cliHeaderSiz);

			UInt32 strongNameSignatureRVA = BitConverterLE.ToUInt32 (cli, 32);
			info.SignaturePosition = RVAtoPosition (strongNameSignatureRVA, numSection, sectionHeaders);
			info.SignatureLength = BitConverterLE.ToUInt32 (cli, 36);

			UInt32 metadataRVA = BitConverterLE.ToUInt32 (cli, 8);
			info.MetadataPosition = RVAtoPosition (metadataRVA, numSection, sectionHeaders);
			info.MetadataLength = BitConverterLE.ToUInt32 (cli, 12);

			if (options == StrongNameOptions.Metadata) {
				cs.Close ();
				hash.Initialize ();
				byte[] metadata = new byte [info.MetadataLength];
				stream.Position = info.MetadataPosition;
				stream.Read (metadata, 0, metadata.Length);
				info.Hash = hash.ComputeHash (metadata);
				return info;
			}

			// now we hash every section EXCEPT the signature block
			for (int i=0; i < numSection; i++) {
				UInt32 start = BitConverterLE.ToUInt32 (sectionHeaders, i * 40 + 20);
				int length = (int) BitConverterLE.ToUInt32 (sectionHeaders, i * 40 + 16);
				byte[] section = new byte [length];
				stream.Position = start;
				stream.Read (section, 0, length);
				if ((start <= info.SignaturePosition) && (info.SignaturePosition < start + length)) {
					// hash before the signature
					int before = (int)(info.SignaturePosition - start);
					if (before > 0) {
						cs.Write (section, 0, before);
					}
					// copy signature
					info.Signature = new byte [info.SignatureLength];
					Buffer.BlockCopy (section, before, info.Signature, 0, (int)info.SignatureLength);
					Array.Reverse (info.Signature);
					// hash after the signature
					int s = (int)(before + info.SignatureLength);
					int after = (int)(length - s);
					if (after > 0) {
						cs.Write (section, s, after);
					}
				}
				else
					cs.Write (section, 0, length);
			}

			cs.Close ();
			info.Hash = hash.Hash;
			return info;
		}

		// return the same result as the undocumented and unmanaged GetHashFromAssemblyFile
		public byte[] Hash (string fileName) 
		{
			FileStream fs = File.OpenRead (fileName);
			StrongNameSignature sn = StrongHash (fs, StrongNameOptions.Metadata);
			fs.Close ();

			return sn.Hash;
		}

		public bool Sign (string fileName) 
		{
			bool result = false;
			StrongNameSignature sn;
			using (FileStream fs = File.OpenRead (fileName)) {
				sn = StrongHash (fs, StrongNameOptions.Signature);
				fs.Close ();
			}
			if (sn.Hash == null)
				return false;

			byte[] signature = null;
			try {
				RSAPKCS1SignatureFormatter sign = new RSAPKCS1SignatureFormatter (rsa);
				sign.SetHashAlgorithm (TokenAlgorithm);
				signature = sign.CreateSignature (sn.Hash);
				Array.Reverse (signature);
			}
			catch (CryptographicException) {
				return false;
			}

			using (FileStream fs = File.OpenWrite (fileName)) {
				fs.Position = sn.SignaturePosition;
				fs.Write (signature, 0, signature.Length);
				fs.Close ();
				result = true;
			}
			return result;
		}

		public bool Verify (string fileName) 
		{
			bool result = false;
			using (FileStream fs = File.OpenRead (fileName)) {
				result = Verify (fs);
				fs.Close ();
			}
			return result;
		}

		public bool Verify (Stream stream)
		{
			StrongNameSignature sn = StrongHash (stream, StrongNameOptions.Signature);
			if (sn.Hash == null) {
				return false;
			}

			try {
				AssemblyHashAlgorithm algorithm = AssemblyHashAlgorithm.SHA1;
				if (tokenAlgorithm == "MD5")
					algorithm = AssemblyHashAlgorithm.MD5;
				return Verify (rsa, algorithm, sn.Hash, sn.Signature);
			}
			catch (CryptographicException) {
				// no exception allowed
				return false;
			}
		}

#if INSIDE_CORLIB
		static object lockObject = new object ();
		static bool initialized;

		// We don't want a dependency on StrongNameManager in Mono.Security.dll
		static public bool IsAssemblyStrongnamed (string assemblyName) 
		{
			if (!initialized) {
				lock (lockObject) {
					if (!initialized) {
						string config = Environment.GetMachineConfigPath ();
						StrongNameManager.LoadConfig (config);
						initialized = true;
					}
				}
			}

			try {
				// this doesn't load the assembly (well it unloads it ;)
				// http://weblogs.asp.net/nunitaddin/posts/9991.aspx
				AssemblyName an = AssemblyName.GetAssemblyName (assemblyName);
				if (an == null)
					return false;

				byte[] publicKey = StrongNameManager.GetMappedPublicKey (an.GetPublicKeyToken ());
				if ((publicKey == null) || (publicKey.Length < 12)) {
					// no mapping
					publicKey = an.GetPublicKey ();
					if ((publicKey == null) || (publicKey.Length < 12))
						return false;
				}

				// Note: MustVerify is based on the original token (by design). Public key
				// remapping won't affect if the assembly is verified or not.
				if (!StrongNameManager.MustVerify (an)) {
					return true;
				}

				RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (publicKey, 12);
				StrongName sn = new StrongName (rsa);
				bool result = sn.Verify (assemblyName);
				return result;
			}
			catch {
				// no exception allowed
				return false;
			}
		}

		// TODO
		// we would get better performance if the runtime hashed the
		// assembly - as we wouldn't have to load it from disk a 
		// second time. The runtime already have implementations of
		// SHA1 (and even MD5 if required someday).
		static public bool VerifySignature (byte[] publicKey, int algorithm, byte[] hash, byte[] signature) 
		{
			try {
				RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (publicKey);
				return Verify (rsa, (AssemblyHashAlgorithm) algorithm, hash, signature);
			}
			catch {
				// no exception allowed
				return false;
			}
		}
#endif
		static private bool Verify (RSA rsa, AssemblyHashAlgorithm algorithm, byte[] hash, byte[] signature) 
		{
			RSAPKCS1SignatureDeformatter vrfy = new RSAPKCS1SignatureDeformatter (rsa);
			switch (algorithm) {
			case AssemblyHashAlgorithm.MD5:
				vrfy.SetHashAlgorithm ("MD5");
				break;
			case AssemblyHashAlgorithm.SHA1:
			case AssemblyHashAlgorithm.None:
			default:
				vrfy.SetHashAlgorithm ("SHA1");
				break;
			}
			return vrfy.VerifySignature (hash, signature);
		}
	}
}
