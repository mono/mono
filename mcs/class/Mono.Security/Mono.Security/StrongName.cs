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
					HashAlgorithm ha = GetHashAlgorithm (TokenAlgorithm);
					byte[] hash = ha.ComputeHash (publicKey);
					// we need the last 8 bytes in reverse order
					keyToken = new byte [8];
					Buffer.BlockCopy (hash, (hash.Length - 8), keyToken, 0, 8);
					Array.Reverse (keyToken, 0, 8);
				}
				return (byte[]) keyToken.Clone ();
			}
		}

		static HashAlgorithm GetHashAlgorithm (string algorithm)
		{
#if FULL_AOT_RUNTIME
			switch (algorithm.ToUpper (CultureInfo.InvariantCulture)) {
			case "SHA1":
				return new SHA1CryptoServiceProvider ();
			case "MD5":
				return new MD5CryptoServiceProvider ();
			default:
				throw new ArgumentException ("Unsupported hash algorithm for token");
			}
#else
			return HashAlgorithm.Create (algorithm);
#endif
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
				if ((s <= r) && (r < s + l))
					return p + r - s;
			}
			return 0;
		}

		private static StrongNameSignature Error (string a)
		{
			//Console.WriteLine (a);
			return null;
		}

		private static byte[] ReadMore (Stream stream, byte[] a, int newSize)
		{
			int oldSize = a.Length;
			Array.Resize (ref a, newSize);
			if (newSize <= oldSize)
				return a;
			int diff = newSize - oldSize;
			return (stream.Read (a, oldSize, diff) == diff) ? a : null;
		}

		internal StrongNameSignature StrongHash (Stream stream, StrongNameOptions options)
		{
			// Bing "msdn pecoff".
			//   https://msdn.microsoft.com/en-us/library/windows/desktop/ms680547(v=vs.85).aspx
			//   Very many of the magic constants and names, funny or otherwise, come from this.
			// ref: Section 24.2.1, Partition II Metadata
			// ref: Section 24.2.2, Partition II Metadata
			// ref: Section 24.3.3, Partition II Metadata

			// Read MS-DOS header.

			const int mzSize = 64;
			byte[] mz = new byte [mzSize];

			int peHeader = 0;
			int mzRead = stream.Read (mz, 0, mzSize);

			if (mzRead == mzSize && mz [0] == (byte)'M' && mz [1] == (byte)'Z') { // 0x5a4d
				peHeader = BitConverterLE.ToInt32 (mz, 60);
				if (peHeader < mzSize)
					return Error ("peHeader_lt_64");

				// Read MS-DOS stub.

				mz = ReadMore (stream, mz, peHeader);
				if (mz == null)
					return Error ("read_mz2_failed");
			} else if (mzRead >= 4 && mz [0] == (byte)'P' && mz [1] == (byte)'E' && mz [2] == 0 && mz [3] == 0) { // 0x4550
				// MS-DOS header/stub can be omitted and just start with PE, though it is rare.
				stream.Position = 0;
				mz = new byte [0];
			} else
				return Error ("read_mz_or_mzsig_failed");

			// PE File Header
			// PE signature 4 bytes
			// file header 20 bytes (really, at this point)
			// optional header varies in size and its size is in the file header
			// "optional" means "not in .obj files", but always in .dll/.exes

			const int sizeOfPeSignature = 4;
			const int sizeOfFileHeader = 20;
			const int sizeOfOptionalHeaderMagic = 2;
			const int offsetOfFileHeader = sizeOfPeSignature;
			const int offsetOfOptionalHeader = sizeOfPeSignature + sizeOfFileHeader;
			int sizeOfOptionalHeader = sizeOfOptionalHeaderMagic; // initial minimum
			int minimumHeadersSize = offsetOfOptionalHeader + sizeOfOptionalHeader;
			byte[] pe = new byte [minimumHeadersSize];
			if (stream.Read (pe, 0, minimumHeadersSize) != minimumHeadersSize
				|| pe [0] != (byte)'P' || pe [1] != (byte)'E' || pe [2] != 0 || pe [3] != 0) // 0x4550
				return Error ("read_minimumHeadersSize_or_pesig_failed");

			sizeOfOptionalHeader = BitConverterLE.ToUInt16 (pe, offsetOfFileHeader + 16);
			if (sizeOfOptionalHeader < sizeOfOptionalHeaderMagic)
				return Error ($"sizeOfOptionalHeader_lt_2 ${sizeOfOptionalHeader}");

			int headersSize = offsetOfOptionalHeader + sizeOfOptionalHeader;
			if (headersSize < offsetOfOptionalHeader) // check overflow
				return Error ("headers_overflow");

			// Read the rest of the NT headers (i.e. the rest of the optional header).

			pe = ReadMore (stream, pe, headersSize);
			if (pe == null)
				return Error ("read_pe2_failed");

			uint magic = BitConverterLE.ToUInt16 (pe, offsetOfOptionalHeader);

			// Refer to PE32+ as PE64 for brevity.
			// PE64 proposal that widened more fields was rejected.
			// Between PE32 and PE32+:
			//   Some fields are the same size and offset. For example the entire
			//      MS-DOS header, FileHeader, and section headers, and some of the optional header.
			//   Some fields are PE32-only (BaseOfData).
			//   Some fields are constant size, some are pointer size.
			//   Relative virtual addresses and file offsets are always 4 bytes.
			//   Some fields offsets are offset by 4, 8, or 12, but mostly 0 or 16,
			//     and it so happens that the 4/8/12-offset fields are less interesting.
			int pe64 = 0;
			bool rom = false;
			if (magic == 0x10B) {
				// nothing
			} else if (magic == 0x20B)
				pe64 = 16;
			else if (magic == 0x107)
				rom = true;
			else
				return Error ("bad_magic_value");

			uint numberOfRvaAndSizes = 0;

			if (!rom) { // ROM images have no data directories or checksum.
				if (sizeOfOptionalHeader >= offsetOfOptionalHeader + 92 + pe64 + 4)
					numberOfRvaAndSizes = BitConverterLE.ToUInt32 (pe, offsetOfOptionalHeader + 92 + pe64);

				// Clear CheckSum and Security Directory if present.
				// CheckSum is located the same for PE32+, all data directories are not.

				for (int i = 64; i < sizeOfOptionalHeader && i < 68; ++i)
					pe [offsetOfOptionalHeader + i] = 0;

				for (int i = 128 + pe64; i < sizeOfOptionalHeader && i < 128 + 8 + pe64; ++i)
					pe [offsetOfOptionalHeader + i] = 0;
			}

			// Read the section headers if present (an image can have no sections, just headers).

			const int sizeOfSectionHeader = 40;
			int numberOfSections = BitConverterLE.ToUInt16 (pe, offsetOfFileHeader + 2);
			byte[] sectionHeaders = new byte [numberOfSections * sizeOfSectionHeader];
			if (stream.Read (sectionHeaders, 0, sectionHeaders.Length) != sectionHeaders.Length)
				return Error ("read_section_headers_failed");

			// Read the CLR header if present.

			uint SignaturePosition = 0;
			uint SignatureLength = 0;
			uint MetadataPosition = 0;
			uint MetadataLength = 0;

			if (15 < numberOfRvaAndSizes && sizeOfOptionalHeader >= 216 + pe64) {
				uint cliHeaderRVA = BitConverterLE.ToUInt32 (pe, offsetOfOptionalHeader + 208 + pe64);
				uint cliHeaderPos = RVAtoPosition (cliHeaderRVA, numberOfSections, sectionHeaders);
				int cliHeaderSiz = BitConverterLE.ToInt32 (pe, offsetOfOptionalHeader + 208 + 4 + pe64);

				// CLI Header
				// ref: Section 24.3.3, Partition II Metadata
				var cli = new byte [cliHeaderSiz];
				stream.Position = cliHeaderPos;
				if (stream.Read (cli, 0, cliHeaderSiz) != cliHeaderSiz)
					return Error ("read_cli_header_failed");

				uint strongNameSignatureRVA = BitConverterLE.ToUInt32 (cli, 32);
				SignaturePosition = RVAtoPosition (strongNameSignatureRVA, numberOfSections, sectionHeaders);
				SignatureLength = BitConverterLE.ToUInt32 (cli, 36);

				uint metadataRVA = BitConverterLE.ToUInt32 (cli, 8);
				MetadataPosition = RVAtoPosition (metadataRVA, numberOfSections, sectionHeaders);
				MetadataLength = BitConverterLE.ToUInt32 (cli, 12);
			}

			StrongNameSignature info = new StrongNameSignature ();
			info.SignaturePosition = SignaturePosition;
			info.SignatureLength = SignatureLength;
			info.MetadataPosition = MetadataPosition;
			info.MetadataLength = MetadataLength;

			using (HashAlgorithm hash = HashAlgorithm.Create (TokenAlgorithm)) {
				if (options == StrongNameOptions.Metadata) {
					hash.Initialize ();
					byte[] metadata = new byte [MetadataLength];
					stream.Position = MetadataPosition;
					if (stream.Read (metadata, 0, (int)MetadataLength) != (int)MetadataLength)
						return Error ("read_cli_metadata_failed");
					info.Hash = hash.ComputeHash (metadata);
					return info;
				}

				using (CryptoStream cs = new CryptoStream (Stream.Null, hash, CryptoStreamMode.Write)) {
					cs.Write (mz, 0, mz.Length); // Hash MS-DOS header/stub despite that stub is not run.
					cs.Write (pe, 0, pe.Length);
					cs.Write (sectionHeaders, 0, sectionHeaders.Length);

					// now we hash every section EXCEPT the signature block
					for (int i=0; i < numberOfSections; i++) {
						UInt32 start = BitConverterLE.ToUInt32 (sectionHeaders, i * sizeOfSectionHeader + 20);
						int length = BitConverterLE.ToInt32 (sectionHeaders, i * sizeOfSectionHeader + 16);
						byte[] section = new byte [length];
						stream.Position = start;
						if (stream.Read (section, 0, length) != length)
							return Error ("read_section_failed");
						// The signature is assumed not to straddle sections.
						if ((start <= SignaturePosition) && (SignaturePosition < start + (uint)length)) {
							// hash before the signature
							int before = (int)(SignaturePosition - start);
							if (before > 0)
								cs.Write (section, 0, before);

							// copy signature
							info.Signature = new byte [SignatureLength];
							Buffer.BlockCopy (section, before, info.Signature, 0, (int)SignatureLength);
							Array.Reverse (info.Signature);
							// hash after the signature
							int s = (int)(before + SignatureLength);
							int after = (int)(length - s);
							if (after > 0)
								cs.Write (section, s, after);
						}
						else
							cs.Write (section, 0, length);
					}
				}
				info.Hash = hash.Hash;
			}
			return info;
		}

		// return the same result as the undocumented and unmanaged GetHashFromAssemblyFile
		public byte[] Hash (string fileName) 
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				return StrongHash (fs, StrongNameOptions.Metadata).Hash;
			}
		}

		public bool Sign (string fileName) 
		{
			StrongNameSignature sn;
			using (FileStream fs = File.OpenRead (fileName)) {
				sn = StrongHash (fs, StrongNameOptions.Signature);
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
			}
			return true;
		}

		public bool Verify (string fileName) 
		{
			using (FileStream fs = File.OpenRead (fileName)) {
				return Verify (fs);
			}
		}

		public bool Verify (Stream stream)
		{
			StrongNameSignature sn = StrongHash (stream, StrongNameOptions.Signature);
			if (sn.Hash == null)
				return false;

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
				if (publicKey == null || publicKey.Length < 12) {
					// no mapping
					publicKey = an.GetPublicKey ();
					if (publicKey == null || publicKey.Length < 12)
						return false;
				}

				// Note: MustVerify is based on the original token (by design). Public key
				// remapping won't affect if the assembly is verified or not.
				if (!StrongNameManager.MustVerify (an))
					return true;

				RSA rsa = CryptoConvert.FromCapiPublicKeyBlob (publicKey, 12);
				return new StrongName (rsa).Verify (assemblyName);
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
