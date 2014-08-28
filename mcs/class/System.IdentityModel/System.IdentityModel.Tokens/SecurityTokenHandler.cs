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