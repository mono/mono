//
// SecurityKeyIdentifierClause.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
	public abstract class SecurityKeyIdentifierClause
	{
		protected SecurityKeyIdentifierClause (string clauseType)
		{
			this.clause_type = clauseType;
		}

		protected SecurityKeyIdentifierClause (string clauseType, byte [] nonce, int length)
		{
			this.clause_type = clauseType;
			if (nonce != null)
				this.nonce = (byte []) nonce.Clone ();
			this.deriv_length = length;
		}

		string clause_type;
		byte [] nonce;
		int deriv_length;

		public virtual bool CanCreateKey {
			get { return false; }
		}

		public string ClauseType {
			get { return clause_type; }
		}

		public int DerivationLength {
			get { return deriv_length; }
		}

		public byte [] GetDerivationNonce ()
		{
			return nonce != null ? (byte []) nonce.Clone () : null;
		}

		public string Id { get; set; }

		public virtual SecurityKey CreateKey ()
		{
			throw new NotSupportedException (String.Format ("This '{0}' identifier clause does not support key creation.", GetType ()));
		}

		[MonoTODO]
		public virtual bool Matches (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			throw new NotImplementedException ();
		}
	}
}
