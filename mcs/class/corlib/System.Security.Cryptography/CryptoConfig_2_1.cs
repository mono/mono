//
// CryptoConfig.cs: Handles cryptographic implementations and OIDs mappings.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2007, 2009 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Cryptography {

	public partial class CryptoConfig {

		public static byte[] EncodeOID (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");
			char[] delim = { '.' };
			string[] parts = str.Split (delim);
			// according to X.208 n is always at least 2
			if (parts.Length < 2) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("OID must have at least two parts"));
			}

			// we're sure that the encoded OID is shorter than its string representation
			byte[] oid = new byte [str.Length];
			// now encoding value
			try {
				byte part0 = Convert.ToByte (parts [0]);
				// OID[0] > 2 is invalid but "supported" in MS BCL
				// uncomment next line to trap this error
				// if (part0 > 2) throw new CryptographicUnexpectedOperationException ();
				byte part1 = Convert.ToByte (parts [1]);
				// OID[1] >= 40 is illegal for OID[0] < 2 because of the % 40
				// however the syntax is "supported" in MS BCL
				// uncomment next 2 lines to trap this error
				//if ((part0 < 2) && (part1 >= 40))
				//	throw new CryptographicUnexpectedOperationException ();
				oid[2] = Convert.ToByte (part0 * 40 + part1);
			}
			catch {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("Invalid OID"));
			}
			int j = 3;
			for (int i = 2; i < parts.Length; i++) {
				long x = Convert.ToInt64 (parts [i]);
				if (x > 0x7F) {
					byte[] num = EncodeLongNumber (x);
					Buffer.BlockCopy (num, 0, oid, j, num.Length);
					j += num.Length;
				}
				else
					oid[j++] = Convert.ToByte (x);
			}

			int k = 2;
			// copy the exact number of byte required
			byte[] oid2 = new byte [j];
			oid2[0] = 0x06; // always - this tag means OID
			// Length (of value)
			if (j > 0x7F) {
				// for compatibility with MS BCL
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("OID > 127 bytes"));
				// comment exception and uncomment next 3 lines to remove restriction
				//byte[] num = EncodeLongNumber (j);
				//Buffer.BlockCopy (num, 0, oid, j, num.Length);
				//k = num.Length + 1;
			}
			else
				oid2 [1] = Convert.ToByte (j - 2); 

			Buffer.BlockCopy (oid, k, oid2, k, j - k);
			return oid2;
		}

		// encode (7bits array) number greater than 127
		private static byte[] EncodeLongNumber (long x)
		{
			// for MS BCL compatibility
			// comment next two lines to remove restriction
			if ((x > Int32.MaxValue) || (x < Int32.MinValue))
				throw new OverflowException (Locale.GetText ("Part of OID doesn't fit in Int32"));

			long y = x;
			// number of bytes required to encode this number
			int n = 1;
			while (y > 0x7F) {
				y = y >> 7;
				n++;
			}
			byte[] num = new byte [n];
			// encode all bytes 
			for (int i = 0; i < n; i++) {
				y = x >> (7 * i);
				y = y & 0x7F;
				if (i != 0)
					y += 0x80;
				num[n-i-1] = Convert.ToByte (y);
			}
			return num;
		}
#if MOONLIGHT
		// we need SHA1 support to verify the codecs binary integrity
		public static string MapNameToOID (string name)
		{
			if ((name != null) && name.Contains ("SHA1"))
				return "1.3.14.3.2.26";
			return String.Empty;
		}

		private const string AES = "System.Security.Cryptography.AesManaged, System.Core, Version=2.0.5.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

		// non-configurable (versus machine.config) mappings for Moonlight (to avoid loading custom code)
		public static object CreateFromName (string name)
		{
			switch (name) {
			case "System.Security.Cryptography.HashAlgorithm":
			case "System.Security.Cryptography.SHA1":
			case "SHA1":
				return new SHA1Managed ();
			case "SHA256":
				return new SHA256Managed ();
			case "System.Security.Cryptography.MD5":
			case "MD5":
				return new MD5CryptoServiceProvider ();
			case "System.Security.Cryptography.RandomNumberGenerator":
				return new RNGCryptoServiceProvider ();
			case "System.Security.Cryptography.RSA":
				return new Mono.Security.Cryptography.RSAManaged ();
			case "AES":
			case AES:
				return (Aes) Activator.CreateInstance (Type.GetType (AES), null);
			default:
				throw new NotImplementedException (name);
			}
		}
#endif
	}
}

