//
// AuthenticodeBase.cs: Authenticode signature base class
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
	enum AuthenticodeAuthority {
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

		protected byte[] rawData;

		public AuthenticodeBase ()
		{
		}

		protected byte[] HashFile (string fileName, string hashName) 
		{
			FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] file = new byte [fs.Length];
			fs.Read (file, 0, file.Length);
			fs.Close ();

			// MZ - DOS header
			if (BitConverter.ToUInt16 (file, 0) != 0x5A4D)
				return null;

			// find offset of PE header
			int peOffset = BitConverter.ToInt32 (file, 60);
			if (peOffset > file.Length)
				return null;

			// PE - NT header
			if (BitConverter.ToUInt16 (file, peOffset) != 0x4550)
				return null;

			// IMAGE_DIRECTORY_ENTRY_SECURITY
			int dirSecurityOffset = BitConverter.ToInt32 (file, peOffset + 152);
			int dirSecuritySize = BitConverter.ToInt32 (file, peOffset + 156);

			if (dirSecuritySize > 8) {
				rawData = new byte [dirSecuritySize - 8];
				Array.Copy (file, dirSecurityOffset + 8, rawData, 0, rawData.Length);
/* DEBUG 
			FileStream debug = new FileStream (fileName + ".sig", FileMode.Create, FileAccess.Write);
			debug.Write (rawData, 0, rawData.Length);
			debug.Close ();*/
			}
			else
				rawData = null;

			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			// 0 to 215 (216) then skip 4 (checksum)
			int pe = peOffset + 88;
			hash.TransformBlock (file, 0, pe, file, 0);
			pe += 4;
			// 220 to 279 (60) then skip 8 (IMAGE_DIRECTORY_ENTRY_SECURITY)
			hash.TransformBlock (file, pe, 60, file, pe);
			pe += 68;
			// 288 to end of file
			int n = file.Length - pe;
			// minus any authenticode signature (with 8 bytes header)
			if (dirSecurityOffset != 0)
				n -= (dirSecuritySize);
			hash.TransformFinalBlock (file, pe, n);

			return hash.Hash;
		}
	}
}
