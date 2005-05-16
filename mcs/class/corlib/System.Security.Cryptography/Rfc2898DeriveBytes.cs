//
// Rfc2898DeriveBytes.cs: RFC2898 (PKCS#5 v2) Key derivation for Password Based Encryption 
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Text;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class Rfc2898DeriveBytes : DeriveBytes {

		private const int defaultIterations = 1000;

		private int _iteration;
		private byte[] _salt;
		private HMACSHA1 _hmac;
		private byte[] _buffer;
		private int _pos;
		private int _f;

		// constructors

		public Rfc2898DeriveBytes (string password, byte[] salt) 
			: this (password, salt, defaultIterations)
		{
		}
		
		public Rfc2898DeriveBytes (string password, byte[] salt, int iterations) 
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			Salt = salt;
			IterationCount = iterations;
			_hmac = new HMACSHA1 (Encoding.UTF8.GetBytes (password));
		}

		public Rfc2898DeriveBytes (byte[] password, byte[] salt, int iterations) 
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			Salt = salt;
			IterationCount = iterations;
			_hmac = new HMACSHA1 (password);
		}
		
		public Rfc2898DeriveBytes (string password, int saltSize)
			: this (password, saltSize, defaultIterations)
		{
		}
		
		public Rfc2898DeriveBytes (string password, int saltSize, int iterations)
		{
			if (password == null)
				throw new ArgumentNullException ("password");
			if (saltSize < 0)
				throw new ArgumentOutOfRangeException ("invalid salt length");

			Salt = KeyBuilder.Key (saltSize);
			IterationCount = iterations;
			_hmac = new HMACSHA1 (Encoding.UTF8.GetBytes (password));
		}

		// properties

		public int IterationCount { 
			get { return _iteration; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("IterationCount < 1");

				_iteration = value; 
			}
		}

		public byte[] Salt { 
			get { return (byte[]) _salt.Clone (); }
			set {
				if (value == null)
					throw new ArgumentNullException ("Salt");
				if (value.Length < 8)
					throw new ArgumentException ("Salt < 8 bytes");

				_salt = (byte[])value.Clone (); 
			}
		}

		// methods

		private byte[] F (byte[] s, int c, int i) 
		{
			byte[] data = new byte [s.Length + 4];
			Buffer.BlockCopy (s, 0, data, 0, s.Length);
			byte[] int4 = BitConverter.GetBytes (i);
			Array.Reverse (int4, 0, 4);
			Buffer.BlockCopy (int4, 0, data, s.Length, 4);

			// this is like j=0
			byte[] u1 = _hmac.ComputeHash (data);
			data = u1;
			// so we start at j=1
			for (int j=1; j < c; j++) {
				byte[] un = _hmac.ComputeHash (data);
				// xor
				for (int k=0; k < 20; k++)
					u1 [k] = (byte)(u1 [k] ^ un [k]);
				data = un;
			}
			return u1;
		}

		public override byte[] GetBytes (int cb) 
		{
			if (cb < 1)
				throw new ArgumentOutOfRangeException ("cb");

			int l = cb / 20;	// HMACSHA1 == 160 bits == 20 bytes
			int r = cb % 20;	// remainder
			if (r != 0)
				l++;		// rounding up

			byte[] result = new byte [cb];
			int rpos = 0;
			if (_pos > 0) {
				int count = Math.Min (20 - _pos, cb);
				Buffer.BlockCopy (_buffer, _pos, result, 0, count);
				if (count >= cb)
					return result;
				_pos = 0;
				rpos = 20 - cb;
				r = cb - rpos;
			}

			for (int i=1; i <= l; i++) {
				_buffer = F (_salt, _iteration, ++_f);
				int count = ((i == l) ? r : 20);
				Buffer.BlockCopy (_buffer, _pos, result, rpos, count);
				rpos += _pos + count;
				_pos = ((count == 20) ? 0 : count);
			}

			return result;
		}
		
		public override void Reset () 
		{
			_buffer = null;
			_pos = 0;
			_f = 0;
		}
	} 
}

#endif
