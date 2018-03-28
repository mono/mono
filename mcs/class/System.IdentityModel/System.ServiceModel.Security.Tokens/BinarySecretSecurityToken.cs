//
// BinarySecretSecurityToken.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security.Tokens
{
	public class BinarySecretSecurityToken : SecurityToken
	{
		ReadOnlyCollection<SecurityKey> keys;

		string id;
		byte [] key;
		bool allow_crypto;
		DateTime valid_from = DateTime.Now.ToUniversalTime ();

		BinarySecretSecurityToken (string id, bool allowCrypto)
		{
			this.id = id;
			allow_crypto = allowCrypto;
		}

		public BinarySecretSecurityToken (byte [] key)
			: this ("uuid:" + Guid.NewGuid ().ToString (), key)
		{
		}

		public BinarySecretSecurityToken (string id, byte [] key)
			: this (id, key, false)
		{
		}

		protected BinarySecretSecurityToken (string id, byte [] key, bool allowCrypto)
			: this (id, allowCrypto)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			this.key = key;

			SecurityKey [] arr = new SecurityKey [] {new InMemorySymmetricSecurityKey (key)};
			keys = new ReadOnlyCollection<SecurityKey> (arr);
		}

		public BinarySecretSecurityToken (int keySizeInBits)
			: this ("uuid:" + Guid.NewGuid ().ToString (), keySizeInBits)
		{
		}

		public BinarySecretSecurityToken (string id, int keySizeInBits)
			: this (id, keySizeInBits, false)
		{
		}

		protected BinarySecretSecurityToken (string id, int keySizeInBits, bool allowCrypto)
			: this (id, allowCrypto)
		{
			if (keySizeInBits < 0)
				throw new ArgumentOutOfRangeException ("keySizeInBits");

			this.key = new byte [keySizeInBits >> 3 + (keySizeInBits % 8 == 0 ? 0 : 1)];

			SecurityKey [] arr = new SecurityKey [] {new InMemorySymmetricSecurityKey (key)};
			keys = new ReadOnlyCollection<SecurityKey> (arr);
		}

		public override DateTime ValidFrom {
			get { return valid_from; }
		}

		public override DateTime ValidTo {
			get { return DateTime.MaxValue.AddDays (-1); }
		}

		public override string Id {
			get { return id; }
		}

		public int KeySize {
			get { return key.Length; }
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { return keys; }
		}

		public byte [] GetKeyBytes ()
		{
			return (byte []) key.Clone ();
		}
	}
}
