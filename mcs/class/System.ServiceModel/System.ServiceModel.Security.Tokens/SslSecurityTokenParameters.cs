//
// SslSecurityTokenParameters.cs
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Security.Tokens
{
	public class SslSecurityTokenParameters : SecurityTokenParameters
	{
		public SslSecurityTokenParameters ()
			: this (false, false)
		{
		}

		public SslSecurityTokenParameters (bool requireClientCertificate)
			: this (requireClientCertificate, false)
		{
		}

		public SslSecurityTokenParameters (bool requireClientCertificate,
			bool requireCancellation)
		{
			this.cert = requireClientCertificate;
			this.cancel = requireCancellation;
		}

		protected SslSecurityTokenParameters (SslSecurityTokenParameters other)
			: base (other)
		{
			cert = other.cert;
			cancel = other.cancel;
		}

		bool cert, cancel;

		public bool RequireClientCertificate {
			get { return cert; }
			set { cert = value; }
		}

		public bool RequireCancellation {
			get { return cancel; }
			set { cancel = value; }
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		protected override bool HasAsymmetricKey {
			get { return false; }
		}

		protected override bool SupportsClientAuthentication {
			get { return false; } 
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return false; } 
		}

		protected override bool SupportsServerAuthentication {
			get { return true; } 
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new SslSecurityTokenParameters (this);
		}

		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			if (token == null)
				throw new ArgumentNullException ("token");

			SecurityContextSecurityToken sct = token as SecurityContextSecurityToken;
			if (sct == null)
				throw new ArgumentException (String.Format ("Not supported SecurityToken: '{0}'", token));
			return referenceStyle == SecurityTokenReferenceStyle.Internal ?
				(SecurityKeyIdentifierClause)
				new LocalIdKeyIdentifierClause (sct.Id) :
				new SecurityContextKeyIdentifierClause (sct.ContextId, sct.KeyGeneration);
			/*
			GenericXmlSecurityToken x = token as GenericXmlSecurityToken;
			if (x == null)
				throw new ArgumentException (String.Format ("Not supported SecurityToken: '{0}'", token));
			return referenceStyle == SecurityTokenReferenceStyle.Internal ? x.InternalTokenReference : x.ExternalTokenReference;
			*/
		}

		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			requirement.TokenType =
				RequireClientCertificate ?
				ServiceModelSecurityTokenTypes.MutualSslnego :
				ServiceModelSecurityTokenTypes.AnonymousSslnego;
			requirement.RequireCryptographicToken = true;
			requirement.Properties [ReqType.SupportSecurityContextCancellationProperty] = RequireCancellation;
			requirement.Properties [ReqType.IssuedSecurityTokenParametersProperty] = this.Clone ();
			requirement.KeyType = SecurityKeyType.SymmetricKey;
		}
	}
}
