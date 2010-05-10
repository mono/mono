//
// Mono.Security.Protocol.Ntlm.ChallengeResponse
//	Implements Challenge Response for NTLM v1
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
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
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm {

	public class ChallengeResponse : IDisposable {

		static private byte[] magic = { 0x4B, 0x47, 0x53, 0x21, 0x40, 0x23, 0x24, 0x25 };

		// This is the pre-encrypted magic value with a null DES key (0xAAD3B435B51404EE)
		// Ref: http://packetstormsecurity.nl/Crackers/NT/l0phtcrack/l0phtcrack2.5-readme.html
		static private byte[] nullEncMagic = { 0xAA, 0xD3, 0xB4, 0x35, 0xB5, 0x14, 0x04, 0xEE };

		private bool _disposed;
		private byte[] _challenge;
		private byte[] _lmpwd;
		private byte[] _ntpwd;

		// constructors

		public ChallengeResponse () 
		{
			_disposed = false;
			_lmpwd = new byte [21];
			_ntpwd = new byte [21];
		}
		
		public ChallengeResponse (string password, byte[] challenge) : this ()
		{
			Password = password;
			Challenge = challenge;
		}

		~ChallengeResponse () 
		{
			if (!_disposed)
				Dispose ();
		}

		// properties

		public string Password {
			get { return null; }
			set { 
				if (_disposed)
					throw new ObjectDisposedException ("too late");

				// create Lan Manager password
#if MOONLIGHT
				DESCryptoServiceProvider des = new DESCryptoServiceProvider ();
#else
				DES des = DES.Create ();
#endif
				des.Mode = CipherMode.ECB;
				ICryptoTransform ct = null;
				
				// Note: In .NET DES cannot accept a weak key
				// this can happen for a null password
				if ((value == null) || (value.Length < 1)) {
					Buffer.BlockCopy (nullEncMagic, 0, _lmpwd, 0, 8);
				}
				else {
					des.Key = PasswordToKey (value, 0);
					ct = des.CreateEncryptor ();
					ct.TransformBlock (magic, 0, 8, _lmpwd, 0);
				}

				// and if a password has less than 8 characters
				if ((value == null) || (value.Length < 8)) {
					Buffer.BlockCopy (nullEncMagic, 0, _lmpwd, 8, 8);
				}
				else {
					des.Key = PasswordToKey (value, 7);
					ct = des.CreateEncryptor ();
					ct.TransformBlock (magic, 0, 8, _lmpwd, 8);
				}

				// create NT password
#if MOONLIGHT
				MD4Managed md4 = new MD4Managed ();
#else
				MD4 md4 = MD4.Create ();
#endif
				byte[] data = ((value == null) ? (new byte [0]) : (Encoding.Unicode.GetBytes (value)));
				byte[] hash = md4.ComputeHash (data);
				Buffer.BlockCopy (hash, 0, _ntpwd, 0, 16);

				// clean up
				Array.Clear (data, 0, data.Length);
				Array.Clear (hash, 0, hash.Length);
				des.Clear ();
			}
		}

		public byte[] Challenge {
			get { return null; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Challenge");
				if (_disposed)
					throw new ObjectDisposedException ("too late");
				// we don't want the caller to modify the value afterward
				_challenge = (byte[]) value.Clone ();
			}
		}

		public byte[] LM {
			get { 
				if (_disposed)
					throw new ObjectDisposedException ("too late");

				return GetResponse (_lmpwd);
			}
		}

		public byte[] NT {
			get { 
				if (_disposed)
					throw new ObjectDisposedException ("too late");

				return GetResponse (_ntpwd);
			}
		}

		// IDisposable method

		public void Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing) 
		{
			if (!_disposed) {
				// cleanup our stuff
				Array.Clear (_lmpwd, 0, _lmpwd.Length);
				Array.Clear (_ntpwd, 0, _ntpwd.Length);
				if (_challenge != null)
					Array.Clear (_challenge, 0, _challenge.Length);
				_disposed = true;
			}
		}

		// private methods

		private byte[] GetResponse (byte[] pwd) 
		{
			byte[] response = new byte [24];
#if MOONLIGHT
			DESCryptoServiceProvider des = new DESCryptoServiceProvider ();
#else
			DES des = DES.Create ();
#endif
			des.Mode = CipherMode.ECB;
			des.Key = PrepareDESKey (pwd, 0);
			ICryptoTransform ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 0);
			des.Key = PrepareDESKey (pwd, 7);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 8);
			des.Key = PrepareDESKey (pwd, 14);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 16);
			return response;
		}

		private byte[] PrepareDESKey (byte[] key56bits, int position) 
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

		private byte[] PasswordToKey (string password, int position) 
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
