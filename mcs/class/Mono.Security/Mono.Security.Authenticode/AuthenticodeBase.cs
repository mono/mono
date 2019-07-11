//
// AuthenticodeBase.cs: Authenticode signature base class
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Cryptography;

/*
FIXME There are a number of problems and deficiencies in this code.

- It requires the PE header to fit in 4K. This is not guaranteed
by the file format and it is easy to construct valid files that violate it.
i.e. with a large MS-DOS header. The code should just read the entire
file into memory.

- It has a number of missing or incorrect range checks.
  Incorrect, as in, checking that record or field starts within
  range, but does not end within range.

- It removes/ignores COFF symbols. These rarely/never occur, but removing
them is not likely correct. It is not mentioned in either of the two specifications.
This seems to be extra unnecessary incorrect code.

- There are two specifications, Authenticode and PE:
https://download.microsoft.com/download/9/c/5/9c5b2167-8017-4bae-9fde-d599bac8184a/Authenticode_PE.docx
https://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
https://msdn.microsoft.com/library/windows/desktop/ms680547(v=vs.85).aspx

These are in contradiction regarding hashing of data after the sections.
Such data is usually absent. More comparison is need between
Mono runtime and desktop runtime/tools.
The most common such data is an older signature, which is supposed to be ignored.
The next most common is appended setup data, which isn't likely with managed code.
  However this code has nothing to do with signing managed code specifically, just PEs.
There is a slight inconsistency in the Authenticode_PE.doc around the location
of the signature vs. other data past sections.
The picture has the signature first, the text puts last.

- A buffer size of 4K is small and probably not performant.
  Buffering makes the code harder to update and correct, vs.
  reading the entire file into memory.

- It does not validate NumberOfRvasAndSizes field.
  Usually it is valid.

- It is missing a number of other validations.
  For example, the optional header magic was ignored, so in the
  interest of small change, we treat all non-0x20B values the same.

Mail with Microsoft confirms the documents do not agree, and that the PE document
is outdated and/or incorrect and/or referring to no longer supported v1 Authenticode,
and that the intent is for the signature to be at the end, per the text and not the picture.
And that data past the sections is to be hashed -- there rarely is any.

The plan is therefore:
 read the entire file into memory
 add missing validation
 hash, excluding checksum, security directory, and security content
 place security content at the end, replacing what is there if anything
 remove the symbol code (here and in formatter)
 expose more offsets from here to cleanup the formatter code

There is also no unit tests for this code it seems.
*/

namespace Mono.Security.Authenticode {

	// References:
	// a.	http://www.cs.auckland.ac.nz/~pgut001/pubs/authenticode.txt

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	enum Authority {
		Individual,
		Commercial,
		Maximum
	}

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class AuthenticodeBase {

		public const string spcIndirectDataContext = "1.3.6.1.4.1.311.2.1.4";

		private byte[] fileblock;
		private Stream fs;
		private int blockNo;
		private int blockLength;
		private int peOffset;
		private int dirSecurityOffset;
		private int dirSecuritySize;
		private int coffSymbolTableOffset;
		private bool pe64;

		internal bool PE64 {
			get {
				if (blockNo < 1)
					ReadFirstBlock ();
				return pe64;
			}
		}

		public AuthenticodeBase ()
		{
			// FIXME Read the entire file into memory.
			// See earlier comments.
			fileblock = new byte [4096];
		}

		internal int PEOffset {
			get {
				if (blockNo < 1)
					ReadFirstBlock ();
				return peOffset;
			}
		}

		internal int CoffSymbolTableOffset {
			get {
				if (blockNo < 1)
					ReadFirstBlock ();
				return coffSymbolTableOffset;
			}
		}

		internal int SecurityOffset {
			get {
				if (blockNo < 1)
					ReadFirstBlock ();
				return dirSecurityOffset;
			}
		}

		internal void Open (string filename)
		{
			if (fs != null)
				Close ();
			fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			blockNo = 0;
		}

		internal void Open (byte[] rawdata)
		{
			if (fs != null)
				Close ();
			fs = new MemoryStream (rawdata, false);
			blockNo = 0;
		}

		internal void Close ()
		{
			if (fs != null) {
				fs.Close ();
				fs = null;
			}
		}

		internal void ReadFirstBlock ()
		{
			int error = ProcessFirstBlock ();
			if (error != 0) {
				string msg = Locale.GetText ("Cannot sign non PE files, e.g. .CAB or .MSI files (error {0}).", 
					error);
				throw new NotSupportedException (msg);
			}
		}

		internal int ProcessFirstBlock ()
		{
			if (fs == null)
				return 1;

			fs.Position = 0;
			// read first block - it will include (100% sure) 
			// the MZ header and (99.9% sure) the PE header
			blockLength = fs.Read (fileblock, 0, fileblock.Length);
			blockNo = 1;
			if (blockLength < 64)
				return 2;	// invalid PE file

			// 1. Validate the MZ header informations
			// 1.1. Check for magic MZ at start of header
			if (BitConverterLE.ToUInt16 (fileblock, 0) != 0x5A4D)
				return 3;

			// 1.2. Find the offset of the PE header
			peOffset = BitConverterLE.ToInt32 (fileblock, 60);
			if (peOffset > fileblock.Length) {
				// just in case (0.1%) this can actually happen
				// FIXME This does not mean the file is invalid,
				// just that this code cannot handle it.
				// FIXME Read the entire file into memory.
				// See earlier comments.
				string msg = String.Format (Locale.GetText (
					"Header size too big (> {0} bytes)."),
					fileblock.Length);
				throw new NotSupportedException (msg);
			}
			// FIXME This verifies that PE starts within the file,
			// but not that it fits.
			if (peOffset > fs.Length)
				return 4;

			// 2. Read between DOS header and first part of PE header
			// 2.1. Check for magic PE at start of header
			//	PE - NT header ('P' 'E' 0x00 0x00)
			if (BitConverterLE.ToUInt32 (fileblock, peOffset) != 0x4550)
				return 5;

			// PE signature is followed by 20 byte file header, and
			// then 2 byte magic 0x10B for PE32 or 0x20B for PE32+,
			// or 0x107 for the obscure ROM case.
			// FIXME The code historically ignored this magic value
			// entirely, so we only treat 0x20B differently to maintain
			// this dubious behavior.
			// FIXME The code also lacks range checks in a number of places,
			// and will access arrays out of bounds for valid files.

			ushort magic = BitConverterLE.ToUInt16 (fileblock, peOffset + 24);
			const int IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B;
			pe64 = magic == IMAGE_NT_OPTIONAL_HDR64_MAGIC;

			// FIXME This fails to validate NumberOfRvasAndSizes.
			// 2.2. Locate IMAGE_DIRECTORY_ENTRY_SECURITY (offset and size)
			// These offsets are from the documentation, but add 24 for
			// PE signature and file header.
			if (pe64) {
				dirSecurityOffset = BitConverterLE.ToInt32 (fileblock, peOffset + 168);
				dirSecuritySize = BitConverterLE.ToInt32 (fileblock, peOffset + 168 + 4);
			}
			else {
				dirSecurityOffset = BitConverterLE.ToInt32 (fileblock, peOffset + 152);
				dirSecuritySize = BitConverterLE.ToInt32 (fileblock, peOffset + 156);
			}

			// FIXME Remove this code and the dependency on it.
			coffSymbolTableOffset = BitConverterLE.ToInt32 (fileblock, peOffset + 12);

			return 0;
		}

		internal byte[] GetSecurityEntry () 
		{
			if (blockNo < 1)
				ReadFirstBlock ();

			if (dirSecuritySize > 8) {
				// remove header from size (not ASN.1 based)
				byte[] secEntry = new byte [dirSecuritySize - 8];
				// position after header and read entry
				fs.Position = dirSecurityOffset + 8;
				fs.Read (secEntry, 0, secEntry.Length);
				return secEntry;
			}
			return null;
		}

		internal byte[] GetHash (HashAlgorithm hash)
		{
			if (blockNo < 1)
				ReadFirstBlock ();
			fs.Position = blockLength;

			// hash the rest of the file
			long n;
			int addsize = 0;
			// minus any authenticode signature (with 8 bytes header)
			if (dirSecurityOffset > 0) {
				// it is also possible that the signature block 
				// starts within the block in memory (small EXE)
				if (dirSecurityOffset < blockLength) {
					blockLength = dirSecurityOffset;
					n = 0;
				} else {
					n = dirSecurityOffset - blockLength;
				}
			} else if (coffSymbolTableOffset > 0) {
				fileblock[PEOffset + 12] = 0;
				fileblock[PEOffset + 13] = 0;
				fileblock[PEOffset + 14] = 0;
				fileblock[PEOffset + 15] = 0;
				fileblock[PEOffset + 16] = 0;
				fileblock[PEOffset + 17] = 0;
				fileblock[PEOffset + 18] = 0;
				fileblock[PEOffset + 19] = 0;
				// it is also possible that the signature block 
				// starts within the block in memory (small EXE)
				if (coffSymbolTableOffset < blockLength) {
					blockLength = coffSymbolTableOffset;
					n = 0;
				} else {
					n = coffSymbolTableOffset - blockLength;
				}
			} else {
				addsize = (int) (fs.Length & 7);
				if (addsize > 0)
					addsize = 8 - addsize;
				
				n = fs.Length - blockLength;
			}

			// Authenticode(r) gymnastics
			// Hash from (generally) 0 to 215 (216 bytes)
			// 88 = 64 + 24
			// 64 is the offset of Checksum within OptionalHeader.
			// 24 is offset of OptionalHeader within PEHeader.
			int pe = peOffset + 88;
			hash.TransformBlock (fileblock, 0, pe, fileblock, 0);
			// then skip 4 for checksum
			pe += 4;

			if (pe64) {
				// security_directory, if present, is at offset 144 within OptionalHeader64
				// FIXME This code fails to check if the security_directory is present.
				// If it is absent, it may or may not be difficult to add, and reject
				// the file as valid but unsignable.
				// Checksum is at [64, 68].
				// 144 - 68 = 76
				// Hash from checksum to security_directory.
				hash.TransformBlock (fileblock, pe, 76, fileblock, pe);
				// then skip 8 bytes for IMAGE_DIRECTORY_ENTRY_SECURITY
				pe += 76 + 8;
			}
			else {
				// security_directory, if present, is at offset 128 within OptionalHeader32
				// FIXME This code fails to check if the security_directory is present.
				// If it is absent, it may or may not be difficult to add, and reject
				// the file as valid but unsignable.
				// Checksum is at [64, 68].
				// 128 - 68 = 60
				// Continue hashing from (generally) 220 to 279 (60 bytes)
				hash.TransformBlock (fileblock, pe, 60, fileblock, pe);
				// then skip 8 bytes for IMAGE_DIRECTORY_ENTRY_SECURITY
				pe += 68;
			}

			// everything is present so start the hashing
			if (n == 0) {
				// hash the (only) block
				hash.TransformFinalBlock (fileblock, pe, blockLength - pe);
			}
			else {
				// hash the last part of the first (already in memory) block
				hash.TransformBlock (fileblock, pe, blockLength - pe, fileblock, pe);

				// hash by blocks of 4096 bytes
				long blocks = (n >> 12);
				int remainder = (int)(n - (blocks << 12));
				if (remainder == 0) {
					blocks--;
					remainder = 4096;
				}
				// blocks
				while (blocks-- > 0) {
					fs.Read (fileblock, 0, fileblock.Length);
					hash.TransformBlock (fileblock, 0, fileblock.Length, fileblock, 0);
				}
				// remainder
				if (fs.Read (fileblock, 0, remainder) != remainder)
					return null;

				if (addsize > 0) {
					hash.TransformBlock (fileblock, 0, remainder, fileblock, 0);
					hash.TransformFinalBlock (new byte [addsize], 0, addsize);
				} else {
					hash.TransformFinalBlock (fileblock, 0, remainder);
				}
			}
			return hash.Hash;
		}

		// for compatibility only
		protected byte[] HashFile (string fileName, string hashName) 
		{
			try {
				Open (fileName);
				HashAlgorithm hash = HashAlgorithm.Create (hashName);
				byte[] result = GetHash (hash);
				Close ();
				return result;
			}
			catch {
				return null;
			}
		}
	}
}
