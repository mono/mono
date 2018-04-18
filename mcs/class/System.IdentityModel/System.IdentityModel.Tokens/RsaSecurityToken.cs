//
// RsaSecurityToken.cs
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
using System.Security.Cryptography;

namespace System.IdentityModel.Tokens
{
	public class RsaSecurityToken : SecurityToken
	{
		public RsaSecurityToken (RSA rsa)
			: this (rsa, new UniqueId ().ToString ())
		{
		}

		public RsaSecurityToken (RSA rsa, string id)
		{
			if (rsa == null)
				throw new ArgumentNullException ("rsa");
			this.rsa = rsa;
			this.id = id;
		}

		RSA rsa;
		string id;
		DateTime valid_from = DateTime.Now.ToUniversalTime ();

		public override DateTime ValidFrom {
			get { return valid_from; }
		}

		public override DateTime ValidTo {
			get { return DateTime.MaxValue.AddDays (-1); }
		}

		public override string Id {
			get { return id; }
		}

		public RSA Rsa {
			get { return rsa; }
		}

		[MonoTODO]
		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool CanCreateKeyIdentifierClause<T> ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override T CreateKeyIdentifierClause<T> ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MatchesKeyIdentifierClause (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			throw new NotImplementedException ();
		}
	}
}
