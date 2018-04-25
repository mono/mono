//
// EncryptedKeyIdentifierClause.cs
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
using System.Collections.Generic;
using System.Xml;
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public sealed class EncryptedKeyIdentifierClause : BinaryKeyIdentifierClause
	{
		public EncryptedKeyIdentifierClause (
			byte [] encryptedKey, string encryptionMethod)
			: this (encryptedKey, encryptionMethod, null)
		{
		}

		public EncryptedKeyIdentifierClause (
			byte [] encryptedKey, string encryptionMethod, 
			SecurityKeyIdentifier encryptingKeyIdentifier)
			: this (encryptedKey, encryptionMethod, encryptingKeyIdentifier, null)
		{
		}

		public EncryptedKeyIdentifierClause (
			byte [] encryptedKey, string encryptionMethod,
			SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName)
			: this (encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName, null, 0)
		{
		}

		public EncryptedKeyIdentifierClause (
			byte [] encryptedKey, string encryptionMethod,
			SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName,
			byte [] derivationNonce, int derivationLength)
			: base (encryptionMethod, encryptedKey, true, derivationNonce, derivationLength)
		{
			this.carried_key_name = carriedKeyName;
			this.identifier = encryptingKeyIdentifier;
			this.enc_method = encryptionMethod;
		}

		string carried_key_name, enc_method;
		SecurityKeyIdentifier identifier;

		public string CarriedKeyName {
			get { return carried_key_name; }
		}

		public string EncryptionMethod {
			get { return enc_method; }
		}

		public SecurityKeyIdentifier EncryptingKeyIdentifier {
			get { return identifier; }
		}

		public byte [] GetEncryptedKey ()
		{
			return GetBuffer ();
		}

		public bool Matches (byte [] encryptedKey, string encryptionMethod, string carriedKeyName)
		{
			if (encryptedKey == null)
				throw new ArgumentNullException ("encryptedKey");
			byte [] buf = GetRawBuffer ();
			if (encryptionMethod != this.enc_method ||
			    carriedKeyName != this.carried_key_name ||
			    encryptedKey.Length != buf.Length)
				return false;
			for (int i = 0; i < buf.Length; i++)
				if (encryptedKey [i] != buf [i])
					return false;
			return true;
		}

		public override bool Matches (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			EncryptedKeyIdentifierClause e =
				keyIdentifierClause as EncryptedKeyIdentifierClause;
			return e != null && Matches (e.GetRawBuffer (), e.EncryptionMethod, e.CarriedKeyName);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
