using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.Security.Claims;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public class SecurityTokenHandlerCollection : Collection<SecurityTokenHandler>
	{
		private SecurityTokenHandlerConfiguration config;
		private IEnumerable<string> tokenTypeIdentifiers = new List<string> ();
		private IEnumerable<Type> tokenTypes = new List<Type> ();

		public SecurityTokenHandlerConfiguration Configuration { get { return this.config; } }
		public IEnumerable<string> TokenTypeIdentifiers { get { return tokenTypeIdentifiers; } }
		public IEnumerable<Type> TokenTypes { get { return tokenTypes; } }
		public SecurityTokenHandler this[SecurityToken token] {
			get {
				if (token == null) { return null; }

				return this[token.GetType ()];
			}
		}
		[MonoTODO]
		public SecurityTokenHandler this[string tokenTypeIdentifier] {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public SecurityTokenHandler this[Type tokenType] {
			get {
				throw new NotImplementedException ();
			}
		}

		public SecurityTokenHandlerCollection ()
			: this(new SecurityTokenHandlerConfiguration ())
		{ }

		public SecurityTokenHandlerCollection (SecurityTokenHandlerConfiguration configuration) {
			config = configuration;
		}

		public SecurityTokenHandlerCollection (IEnumerable<SecurityTokenHandler> handlers)
			: this (handlers, new SecurityTokenHandlerConfiguration ())
		{ }

		public SecurityTokenHandlerCollection (IEnumerable<SecurityTokenHandler> handlers, SecurityTokenHandlerConfiguration configuration) : this (configuration) {
			foreach (var handler in handlers) {
				Add (handler);
			}
		}

		[MonoTODO]
		public void AddOrReplace(SecurityTokenHandler handler) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CanReadKeyIdentifierClause(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool CanReadKeyIdentifierClauseCore(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CanReadToken(string tokenString) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CanReadToken(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CanWriteToken(SecurityToken token) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ClearItems() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SecurityTokenHandlerCollection CreateDefaultSecurityTokenHandlerCollection() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SecurityTokenHandlerCollection CreateDefaultSecurityTokenHandlerCollection(SecurityTokenHandlerConfiguration configuration) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void InsertItem(int index, SecurityTokenHandler item) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SecurityToken ReadToken(string tokenString) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SecurityToken ReadToken(XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void RemoveItem(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetItem(int index, SecurityTokenHandler item) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string WriteToken(SecurityToken token) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteToken(XmlWriter writer, SecurityToken token) {
			throw new NotImplementedException ();
		}
	}
}