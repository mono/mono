//
// Mono.Security.Protocol.Ntlm.ChallengeResponse
//	Implements Challenge Response for NTLM v1 and NTLM v2 Session
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Martin Baulig <martin.baulig@xamarin.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
// (C) 2012 Xamarin, Inc. (http://www.xamarin.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
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
using System.IO;
using System.Net;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm {

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	static class ChallengeResponse2 {

		static private byte[] magic = { 0x4B, 0x47, 0x53, 0x21, 0x40, 0x23, 0x24, 0x25 };

		// This is the pre-encrypted magic value with a null DES key (0xAAD3B435B51404EE)
		// Ref: http://packetstormsecurity.nl/Crackers/NT/l0phtcrack/l0phtcrack2.5-readme.html
		static private byte[] nullEncMagic = { 0xAA, 0xD3, 0xB4, 0x35, 0xB5, 0x14, 0x04, 0xEE };

		static byte[] Compute_LM (string password, byte[] challenge)
		{
			var buffer = new byte [21];

			// create Lan Manager password
			DES des = DES.Create ();
			des.Mode = CipherMode.ECB;
			ICryptoTransform ct = null;
				
			// Note: In .NET DES cannot accept a weak key
			// this can happen for a null password
			if ((password == null) || (password.Length < 1)) {
				Buffer.BlockCopy (nullEncMagic, 0, buffer, 0, 8);
			} else {
				des.Key = PasswordToKey (password, 0);
				ct = des.CreateEncryptor ();
				ct.TransformBlock (magic, 0, 8, buffer, 0);
			}
				
			// and if a password has less than 8 characters
			if ((password == null) || (password.Length < 8)) {
				Buffer.BlockCopy (nullEncMagic, 0, buffer, 8, 8);
			} else {
				des.Key = PasswordToKey (password, 7);
				ct = des.CreateEncryptor ();
				ct.TransformBlock (magic, 0, 8, buffer, 8);
			}
				
			des.Clear ();

			return GetResponse (challenge, buffer);
		}

		static byte[] Compute_NTLM_Password (string password)
		{
			var buffer = new byte [21];

			// create NT password
			MD4 md4 = MD4.Create ();
			byte[] data = ((password == null) ? (new byte [0]) : (Encoding.Unicode.GetBytes (password)));
			byte[] hash = md4.ComputeHash (data);
			Buffer.BlockCopy (hash, 0, buffer, 0, 16);
			
			// clean up
			Array.Clear (data, 0, data.Length);
			Array.Clear (hash, 0, hash.Length);

			return buffer;
		}

		static byte[] Compute_NTLM (string password, byte[] challenge)
		{
			var buffer = Compute_NTLM_Password (password);
			return GetResponse (challenge, buffer);
		}

		static void Compute_NTLMv2_Session (string password, byte[] challenge,
		                                    out byte[] lm, out byte[] ntlm)
		{
			var nonce = new byte [8];
			var rng = RandomNumberGenerator.Create ();
			rng.GetBytes (nonce);

			var sessionNonce = new byte [challenge.Length + 8];
			challenge.CopyTo (sessionNonce, 0);
			nonce.CopyTo (sessionNonce, challenge.Length);

			lm = new byte [24];
			nonce.CopyTo (lm, 0);

			MD5 md5 = MD5.Create ();
			
			var hash = md5.ComputeHash (sessionNonce);
			var newChallenge = new byte [8];
			Array.Copy (hash, newChallenge, 8);

			ntlm = Compute_NTLM (password, newChallenge);

			// clean up
			Array.Clear (nonce, 0, nonce.Length);
			Array.Clear (sessionNonce, 0, sessionNonce.Length);
			Array.Clear (newChallenge, 0, newChallenge.Length);
			Array.Clear (hash, 0, hash.Length);
		}

		static byte[] Compute_NTLMv2 (Type2Message type2, string username, string password, string domain)
		{
			var ntlm_hash = Compute_NTLM_Password (password);

			var ubytes = Encoding.Unicode.GetBytes (username.ToUpperInvariant ());
			var tbytes = Encoding.Unicode.GetBytes (domain);

			var bytes = new byte [ubytes.Length + tbytes.Length];
			ubytes.CopyTo (bytes, 0);
			Array.Copy (tbytes, 0, bytes, ubytes.Length, tbytes.Length);

			var md5 = new HMACMD5 (ntlm_hash);
			var ntlm_v2_hash = md5.ComputeHash (bytes);

			Array.Clear (ntlm_hash, 0, ntlm_hash.Length);
			md5.Clear ();

			var ntlm_v2_md5 = new HMACMD5 (ntlm_v2_hash);

			var now = DateTime.Now;
			var timestamp = now.Ticks - 504911232000000000;
			
			var nonce = new byte [8];
			var rng = RandomNumberGenerator.Create ();
			rng.GetBytes (nonce);
			
			byte[] blob = new byte [28 + type2.TargetInfo.Length];
			blob[0] = 0x01;
			blob[1] = 0x01;

			Buffer.BlockCopy (BitConverterLE.GetBytes (timestamp), 0, blob, 8, 8);

			Buffer.BlockCopy (nonce, 0, blob, 16, 8);
			Buffer.BlockCopy (type2.TargetInfo, 0, blob, 28, type2.TargetInfo.Length);

			var challenge = type2.Nonce;

			var hashInput = new byte [challenge.Length + blob.Length];
			challenge.CopyTo (hashInput, 0);
			blob.CopyTo (hashInput, challenge.Length);

			var blobHash = ntlm_v2_md5.ComputeHash (hashInput);

			var response = new byte [blob.Length + blobHash.Length];
			blobHash.CopyTo (response, 0);
			blob.CopyTo (response, blobHash.Length);

			Array.Clear (ntlm_v2_hash, 0, ntlm_v2_hash.Length);
			ntlm_v2_md5.Clear ();
			Array.Clear (nonce, 0, nonce.Length);
			Array.Clear (blob, 0, blob.Length);
			Array.Clear (hashInput, 0, hashInput.Length);
			Array.Clear (blobHash, 0, blobHash.Length);

			return response;
		}

		public static void Compute (Type2Message type2, NtlmAuthLevel level,
		                            string username, string password, string domain,
		                            out byte[] lm, out byte[] ntlm)
		{
			lm = null;

			switch (level) {
			case NtlmAuthLevel.LM_and_NTLM:
				lm = Compute_LM (password, type2.Nonce);
				ntlm = Compute_NTLM (password, type2.Nonce);
				break;

			case NtlmAuthLevel.LM_and_NTLM_and_try_NTLMv2_Session:
				if ((type2.Flags & NtlmFlags.NegotiateNtlm2Key) == 0)
					goto case NtlmAuthLevel.LM_and_NTLM;
				Compute_NTLMv2_Session (password, type2.Nonce, out lm, out ntlm);
				break;

			case NtlmAuthLevel.NTLM_only:
				if ((type2.Flags & NtlmFlags.NegotiateNtlm2Key) != 0)
					Compute_NTLMv2_Session (password, type2.Nonce, out lm, out ntlm);
				else
					ntlm = Compute_NTLM (password, type2.Nonce);
				break;

			case NtlmAuthLevel.NTLMv2_only:
				ntlm = Compute_NTLMv2 (type2, username, password, domain);
				break;

			default:
				throw new InvalidOperationException ();
			}
		}

		static byte[] GetResponse (byte[] challenge, byte[] pwd) 
		{
			byte[] response = new byte [24];
			DES des = DES.Create ();
			des.Mode = CipherMode.ECB;
			des.Key = PrepareDESKey (pwd, 0);
			ICryptoTransform ct = des.CreateEncryptor ();
			ct.TransformBlock (challenge, 0, 8, response, 0);
			des.Key = PrepareDESKey (pwd, 7);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (challenge, 0, 8, response, 8);
			des.Key = PrepareDESKey (pwd, 14);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (challenge, 0, 8, response, 16);
			return response;
		}

		static byte[] PrepareDESKey (byte[] key56bits, int position) 
		{
			// convert to 8 bytes
			byte[] key = new byte [8];
			key [0] = key56bits [position];
			key [1] = (byte) ((key56bits [position] << 7)     | (key56bits [position + 1] >> 1));
			key [2] = (byte) ((key56bits [position + 1] << 6) | (key56bits [position + 2] >> 2));
			key [3] = (byte) ((key56bits [position + 2] << 5) | (key56bits [position + 3] >> 3));
			key [4] = (byte) ((key56bits [position + 3] << 4) | (key56bits [position + 4] >> 4));
			key [5] = (byte) ((key56bits [position + 4] << 3) | (key56bits [position + 5] >> 5));
			key [6] = (byte) ((key56bits [position + 5] << 2) | (key56bits [position + 6] >> 6));
			key [7] = (byte)  (key56bits [position + 6] << 1);
			return key;
		}

		static byte[] PasswordToKey (string password, int position) 
		{
			byte[] key7 = new byte [7];
			int len = System.Math.Min (password.Length - position, 7);
			Encoding.ASCII.GetBytes (password.ToUpper (CultureInfo.CurrentCulture), position, len, key7, 0);
			byte[] key8 = PrepareDESKey (key7, 0);
			// cleanup intermediate key material
			Array.Clear (key7, 0, key7.Length);
			return key8;
		}
	}
}
