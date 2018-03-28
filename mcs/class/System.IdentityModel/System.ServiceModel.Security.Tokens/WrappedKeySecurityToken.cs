//
// WrappedKeySecurityToken.cs
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
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security.Tokens
{
	public class WrappedKeySecurityToken : SecurityToken
	{
		string id;
		byte [] raw_key;
		byte [] wrapped_key;
		string wrap_alg;
		SecurityToken wrap_token;
		SecurityKeyIdentifier wrap_token_ref;
		DateTime valid_from = DateTime.Now.ToUniversalTime ();
		ReadOnlyCollection<SecurityKey> keys;
		ReferenceList reference_list;
		byte [] keyhash;

		public WrappedKeySecurityToken (
			string id,
			byte [] keyToWrap,
			string wrappingAlgorithm,
			SecurityToken wrappingToken,
			SecurityKeyIdentifier wrappingTokenReference)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (keyToWrap == null)
				throw new ArgumentNullException ("keyToWrap");
			if (wrappingAlgorithm == null)
				throw new ArgumentNullException ("wrappingAlgorithm");
			if (wrappingToken == null)
				throw new ArgumentNullException ("wrappingToken");

			raw_key = keyToWrap;
			this.id = id;
			wrap_alg = wrappingAlgorithm;
			wrap_token = wrappingToken;
			wrap_token_ref = wrappingTokenReference;
			Collection<SecurityKey> l = new Collection<SecurityKey> ();
			foreach (SecurityKey sk in wrappingToken.SecurityKeys) {
				if (sk.IsSupportedAlgorithm (wrappingAlgorithm)) {
					wrapped_key = sk.EncryptKey (wrappingAlgorithm, keyToWrap);
					l.Add (new InMemorySymmetricSecurityKey (keyToWrap));
					break;
				}
			}
			keys = new ReadOnlyCollection<SecurityKey> (l);
			if (wrapped_key == null)
				throw new ArgumentException (String.Format ("None of the security keys in the argument token supports specified wrapping algorithm '{0}'", wrappingAlgorithm));
		}

		internal byte [] RawKey {
			get { return raw_key; }
		}

		// It is kind of compromised solution to output
		// ReferenceList inside e:EncryptedKey and might disappear
		// when non-wrapped key is represented by another token type.
		internal ReferenceList ReferenceList {
			get { return reference_list; }
			set { reference_list = value; }
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

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { return keys; }
		}

		public string WrappingAlgorithm {
			get { return wrap_alg; }
		}

		public SecurityToken WrappingToken {
			get { return wrap_token; }
		}

		public SecurityKeyIdentifier WrappingTokenReference {
			get { return wrap_token_ref; }
		}

		public byte [] GetWrappedKey ()
		{
			return (byte []) wrapped_key.Clone ();
		}

		internal void SetWrappedKey (byte [] value)
		{
			wrapped_key = (byte []) value.Clone ();
		}

		[MonoTODO]
		public override bool CanCreateKeyIdentifierClause<T> ()
		{
			/*
			foreach (SecurityKeyIdentifierClause k in WrappingTokenReference) {
				Type t = k.GetType ();
				if (t == typeof (T) || t.IsSubclassOf (typeof (T)))
					return true;
			}
			*/
			return false;
		}

		[MonoTODO]
		public override T CreateKeyIdentifierClause<T> ()
		{
			/*
			foreach (SecurityKeyIdentifierClause k in WrappingTokenReference) {
				Type t = k.GetType ();
				if (t == typeof (T) || t.IsSubclassOf (typeof (T)))
					return (T) k;
			}
			*/
			throw new NotSupportedException (String.Format ("WrappedKeySecurityToken cannot create '{0}'", typeof (T)));
		}

		public override bool MatchesKeyIdentifierClause (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			LocalIdKeyIdentifierClause lkic = keyIdentifierClause as LocalIdKeyIdentifierClause;
			if (lkic != null && lkic.LocalId == Id)
				return true;

			InternalEncryptedKeyIdentifierClause khic = keyIdentifierClause as InternalEncryptedKeyIdentifierClause;
			if (keyhash == null)
				keyhash = SHA1.Create ().ComputeHash (wrapped_key);
			if (khic != null && khic.Matches (keyhash))
				return true;

			return false;
		}
	}
}
