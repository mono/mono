//
// SecurityTokenHandler.cs
//
// Author:
//   Noesis Labs (Ryan.Melena@noesislabs.com)
//
// Copyright (C) 2014 Noesis Labs, LLC  https://noesislabs.com
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
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.Security.Claims;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public abstract class SecurityTokenHandler : ICustomIdentityConfiguration
	{
		public virtual bool CanValidateToken { get { return false; } }
		public virtual bool CanWriteToken { get { return false; } }
		public SecurityTokenHandlerConfiguration Configuration { get; set; }
		public SecurityTokenHandlerCollection ContainingCollection { get; internal set; }
		public abstract Type TokenType { get; }

		public virtual bool CanReadKeyIdentifierClause (XmlReader reader) {
			return false;
		}

		public virtual bool CanReadToken (string tokenString) {
			return false;
		}

		public virtual bool CanReadToken (XmlReader reader) {
			return false;
		}

		public virtual bool CanWriteKeyIdentifierClause (SecurityKeyIdentifierClause securityKeyIdentifierClause) {
			return false;
		}

		public virtual SecurityKeyIdentifierClause CreateSecurityTokenReference (SecurityToken token, bool attached) {
			throw new NotImplementedException ();
		}

		public virtual SecurityToken CreateToken (SecurityTokenDescriptor tokenDescriptor) {
			throw new NotImplementedException ();
		}

		protected virtual void DetectReplayedToken (SecurityToken token) {
			throw new NotImplementedException ();
		}

		public abstract string[] GetTokenTypeIdentifiers ();

		public virtual void LoadCustomConfiguration (XmlNodeList nodelist) {
			throw new NotImplementedException ();
		}

		public virtual SecurityKeyIdentifierClause ReadKeyIdentifierClause (XmlReader reader) {
			throw new NotImplementedException ();
		}


		public virtual SecurityToken ReadToken (string tokenString) {
			throw new NotImplementedException ();
		}

		public virtual SecurityToken ReadToken (XmlReader reader) {
			throw new NotImplementedException ();
		}

		public virtual SecurityToken ReadToken (XmlReader reader, SecurityTokenResolver tokenResolver) {
			return this.ReadToken (reader);
		}

		protected void TraceTokenValidationFailure (SecurityToken token, string errorMessage) {
			throw new NotImplementedException ();
		}

		protected void TraceTokenValidationSuccess (SecurityToken token) {
			throw new NotImplementedException ();
		}

		public virtual ReadOnlyCollection<ClaimsIdentity> ValidateToken (SecurityToken token) {
			throw new NotImplementedException ();
		}

		public virtual void WriteKeyIdentifierClause (XmlWriter writer, SecurityKeyIdentifierClause securityKeyIdentifierClause) {
			throw new NotImplementedException ();
		}

		public virtual string WriteToken (SecurityToken token) {
			throw new NotImplementedException ();
		}

		public virtual void WriteToken (XmlWriter writer, SecurityToken token) {
			throw new NotImplementedException ();
		}
	}
}
