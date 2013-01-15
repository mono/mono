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
		private FileStream fs;
		private int blockNo;
		private int blockLength;
		private int peOffset;
		private int dirSecurityOffset;
		private int dirSecuritySize;
		private int coffSymbolTableOffset;

		public AuthenticodeBase ()
		{
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
		}

		internal void Close ()
		{
			if (fs != null) {
				fs.Close ();
				fs = null;
				blockNo = 0;
			}
		}

		internal bool ReadFirstBlock ()
		{
			if (fs == null)
				return false;

			fs.Position = 0;
			// read first block - it will include (100% sure) 
			// the MZ header and (99.9% sure) the PE header
			blockLength = fs.Read (fileblock, 0, fileblock.Length);
			blockNo = 1;
			if (blockLength < 64)
				return false;	// invalid PE file

			// 1. Validate the MZ header informations
			// 1.1. Check for magic MZ at start of header
			if (BitConverterLE.ToUInt16 (fileblock, 0) != 0x5A4D)
				return false;

			// 1.2. Find the offset of the PE header
			peOffset = BitConverterLE.ToInt32 (fileblock, 60);
			if (peOffset > fileblock.Length) {
				// just in case (0.1%) this can actually happen
				string msg = String.Format (Locale.GetText (
					"Header size too big (> {0} bytes)."),
					fileblock.Length);
				throw new NotSupportedException (msg);
			}
			if (peOffset > fs.Length)
				return false;

			// 2. Read between DOS header and first part of PE header
			// 2.1. Check for magic PE at start of header
			//	PE - NT header ('P' 'E' 0x00 0x00)
			if (BitConverterLE.ToUInt32 (fileblock, peOffset) != 0x4550)
				return false;

			// 2.2. Locate IMAGE_DIRECTORY_ENTRY_SECURITY (offset and size)
			dirSecurityOffset = BitConverterLE.ToInt32 (fileblock, peOffset + 152);
			dirSecuritySize = BitConverterLE.ToInt32 (fileblock, peOffset + 156);

			// COFF symbol tables are deprecated - we'll strip them if we see them!
			// (otherwise the signature won't work on MS and we don't want to support COFF for that)
			coffSymbolTableOffset = BitConverterLE.ToInt32 (fileblock, peOffset + 12);

			return true;
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
			int pe = peOffset + 88;
			hash.TransformBlock (fileblock, 0, pe, fileblock, 0);
			// then skip 4 for checksum
			pe += 4;
			// Continue hashing from (generally) 220 to 279 (60 bytes)
			hash.TransformBlock (fileblock, pe, 60, fileblock, pe);
			// then skip 8 bytes for IMAGE_DIRECTORY_ENTRY_SECURITY
			pe += 68;

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
