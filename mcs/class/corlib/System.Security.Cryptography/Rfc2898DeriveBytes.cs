//
// Rfc2898DeriveBytes.cs: RFC2898 (PKCS#5 v2) Key derivation for Password Based Encryption 
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Text;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class Rfc2898DeriveBytes : DeriveBytes {

		private const int defaultIterations = 1000;
		private int _iteration;
		private byte[] _salt;
		private HMACSHA1 _hmac;

		// constructors

		public Rfc2898DeriveBytes (string password, byte[] salt) 
			: this (password, salt, defaultIterations) {}
		
		public Rfc2898DeriveBytes (string password, byte[] salt, int iterations) 
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			Salt = salt;
			IterationCount = iterations;
			_hmac = new HMACSHA1 (Encoding.UTF8.GetBytes (password));
		}
		
		public Rfc2898DeriveBytes (string password, int saltSize)
			: this (password, saltSize, defaultIterations) {}
		
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
			int l = cb / 20;	// HMACSHA1 == 160 bits == 20 bytes
			int r = cb % 20;	// remainder
			if (r != 0)
				l++;		// rounding up

			byte[] result = new byte [cb];
			int pos = 0;

			for (int i=0; i < l; i++) {
				byte[] t =  F (_salt, _iteration, l);
				int count = ((i == l - 1) ? r : 20);
				Buffer.BlockCopy (t, 0, result, pos, count);
				pos += count;
			}

			return result;
		}
		
		[MonoTODO]
		public override void Reset () 
		{
		}
	} 
}

#endif