//
// GenericXmlSecurityToken.cs
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
using System.Net;
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Principal;

namespace System.IdentityModel.Tokens
{
	public class GenericXmlSecurityToken : SecurityToken
	{
		XmlElement xml;
		SecurityToken proof_token;
		DateTime from, to;
		SecurityKeyIdentifierClause int_tokenref, ext_tokenref;
		ReadOnlyCollection<IAuthorizationPolicy> auth_policies;

		public GenericXmlSecurityToken (
			XmlElement tokenXml,
			SecurityToken proofToken,
			DateTime effectiveTime,
			DateTime expirationTime,
			SecurityKeyIdentifierClause internalTokenReference,
			SecurityKeyIdentifierClause externalTokenReference,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
		{
			xml = tokenXml;
			proof_token = proofToken;
			from = effectiveTime;
			to = expirationTime;
			int_tokenref = internalTokenReference;
			ext_tokenref = externalTokenReference;
			auth_policies = authorizationPolicies;
		}

		[MonoTODO] // verify
		public override string Id {
			get { return proof_token.Id; }
		}

		public XmlElement TokenXml {
			get { return xml; } 
		}

		public SecurityToken ProofToken { 
			get { return proof_token; } 
		}

		public override DateTime ValidFrom { 
			get { return from; } 
		}
		public override DateTime ValidTo { 
			get { return to; } 
		}

		public SecurityKeyIdentifierClause InternalTokenReference { 
			get { return int_tokenref; } 
		}

		public SecurityKeyIdentifierClause ExternalTokenReference { 
			get { return ext_tokenref; } 
		}

		public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies {
			get { return auth_policies; }
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
