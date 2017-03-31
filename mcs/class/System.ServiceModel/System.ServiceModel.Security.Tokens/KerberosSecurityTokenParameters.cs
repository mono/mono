//
// KerberosSecurityTokenParameters.cs
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
	public class KerberosSecurityTokenParameters : SecurityTokenParameters
	{
		public KerberosSecurityTokenParameters ()
		{
		}

		protected KerberosSecurityTokenParameters (KerberosSecurityTokenParameters other)
			: base (other)
		{
		}

		[MonoTODO]
		protected override bool HasAsymmetricKey {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected override bool SupportsClientAuthentication {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected override bool SupportsClientWindowsIdentity {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected override bool SupportsServerAuthentication {
			get { throw new NotImplementedException (); }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new KerberosSecurityTokenParameters (this);
		}

		[MonoTODO]
		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			throw new NotImplementedException ();
		}

		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			if (requirement == null)
				throw new ArgumentNullException ("requirement");
			requirement.TokenType = SecurityTokenTypes.Kerberos;
			requirement.RequireCryptographicToken = true;
			requirement.KeyType = SecurityKeyType.SymmetricKey;
		}
	}
}
