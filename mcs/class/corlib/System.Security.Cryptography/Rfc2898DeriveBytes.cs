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

using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class Rfc2898DeriveBytes : DeriveBytes {

		private const int defaultIterations = 1000;
		private int _iteration;
		private byte[] _salt;

		// constructors

		public Rfc2898DeriveBytes (string password, byte[] salt) 
			: this (password, salt, defaultIterations) {}
		
		public Rfc2898DeriveBytes (string password, byte[] salt, int iterations) 
		{
			if (password == null)
				throw new ArgumentNullException ("password");
			if (salt == null)
				throw new ArgumentNullException ("salt");
			if (salt.Length < 8)
				throw new ArgumentException ("salt < 8 bytes");
			if (iterations < 1)
				throw new ArgumentException ("iterations < 1");

			_iteration = iterations;
			_salt = (byte[]) salt.Clone ();
		}
		
		public Rfc2898DeriveBytes (string password, int saltSize)
			: this (password, KeyBuilder.Key(saltSize), defaultIterations) {}
		
		public Rfc2898DeriveBytes (string password, int saltSize, int iterations)
			: this (password, KeyBuilder.Key(saltSize), iterations) {}

		// properties

		public int IterationCount { 
			get { return _iteration; }
			set { _iteration = value; }
		}

		public byte[] Salt { 
			get { return (byte[]) _salt.Clone (); }
			set { _salt = (byte[])value.Clone (); }
		}

		// methods

		[MonoTODO]
		public override byte[] GetBytes (int cb) 
		{
			return null;
		}
		
		[MonoTODO]
		public override void Reset () 
		{
		}
	} 
}

#endif