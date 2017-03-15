//
// X509SecurityTokenParameters.cs
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

namespace System.ServiceModel.Security.Tokens
{
	public class X509SecurityTokenParameters : SecurityTokenParameters
	{
		X509KeyIdentifierClauseType reference_style;

		public X509SecurityTokenParameters ()
			: this (X509KeyIdentifierClauseType.Any)
		{
		}

		public X509SecurityTokenParameters (X509KeyIdentifierClauseType x509ReferenceStyle)
			: this (x509ReferenceStyle, SecurityTokenInclusionMode.AlwaysToRecipient)
		{
		}

		public X509SecurityTokenParameters (X509KeyIdentifierClauseType x509ReferenceStyle, SecurityTokenInclusionMode inclusionMode)
		{
			reference_style = x509ReferenceStyle;
			InclusionMode = inclusionMode;
		}

		protected X509SecurityTokenParameters (X509SecurityTokenParameters other)
			: base (other)
		{
			reference_style = other.reference_style;
		}

		public X509KeyIdentifierClauseType X509ReferenceStyle {
			get { return reference_style; }
			set { reference_style = value; }
		}

		// It is documented as to return false, but that is wrong.
		protected override bool HasAsymmetricKey {
			get { return true; }
		}

		protected override bool SupportsClientAuthentication {
			get { return true; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return true; }
		}

		protected override bool SupportsServerAuthentication {
			get { return true; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new X509SecurityTokenParameters (this);
		}

		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			if (token == null)
				throw new ArgumentNullException ("token");

			if (referenceStyle == SecurityTokenReferenceStyle.Internal)
				return new LocalIdKeyIdentifierClause (token.Id, token.GetType ());

			switch (reference_style) {
			default:
				return token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause> ();
			case X509KeyIdentifierClauseType.Thumbprint:
				return token.CreateKeyIdentifierClause<X509ThumbprintKeyIdentifierClause> ();
			case X509KeyIdentifierClauseType.SubjectKeyIdentifier:
				return token.CreateKeyIdentifierClause<X509SubjectKeyIdentifierClause> ();
			case X509KeyIdentifierClauseType.RawDataKeyIdentifier:
				return token.CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause> ();
			case X509KeyIdentifierClauseType.Any:
				if (token.CanCreateKeyIdentifierClause<X509SubjectKeyIdentifierClause> ())
					goto case X509KeyIdentifierClauseType.SubjectKeyIdentifier;
				goto default;
			}
		}

		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			requirement.TokenType = SecurityTokenTypes.X509Certificate;
			requirement.KeyType = SecurityKeyType.AsymmetricKey;
			requirement.RequireCryptographicToken = true;
		}

		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
