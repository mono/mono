﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.Security.Claims;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public class SessionSecurityTokenHandler : SecurityTokenHandler
	{
		public static readonly ReadOnlyCollection<CookieTransform> DefaultCookieTransforms;
		public static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours (10);

		private bool canValidateToken;
		private bool canWriteToken;
		private string cookieElementName;
		private string cookieNamespace;
		private Type tokenType;

		public override bool CanValidateToken { get { return canValidateToken; } }
		public override bool CanWriteToken { get { return canWriteToken; } }
		public virtual string CookieElementName { get { return cookieElementName; } }
		public virtual string CookieNamespace { get { return cookieNamespace; } }
		public static TimeSpan DefaultTokenLifetime { get { return SessionSecurityTokenHandler.DefaultLifetime; } }
		public virtual TimeSpan TokenLifetime { get; set; }
		public override Type TokenType { get { return tokenType; } }
		public ReadOnlyCollection<CookieTransform> Transforms { get; private set; }

		public SessionSecurityTokenHandler ()
			: this (SessionSecurityTokenHandler.DefaultCookieTransforms)
		{ }

		public SessionSecurityTokenHandler (ReadOnlyCollection<CookieTransform> transforms)
			: this (transforms, SessionSecurityTokenHandler.DefaultLifetime)
		{ }

		public SessionSecurityTokenHandler (ReadOnlyCollection<CookieTransform> transforms, TimeSpan tokenLifetime) {
			Transforms = transforms;
			TokenLifetime = tokenLifetime;
		}

		[MonoTODO]
		protected virtual byte[] ApplyTransforms (byte[] cookie, bool outbound) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanReadToken (XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual SessionSecurityToken CreateSessionSecurityToken (ClaimsPrincipal principal, string context, string endpointId, DateTime validFrom, DateTime validTo) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityToken CreateToken (SecurityTokenDescriptor tokenDescriptor) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string[] GetTokenTypeIdentifiers () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void LoadCustomConfiguration (XmlNodeList customConfigElements) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityToken ReadToken (XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual SecurityToken ReadToken (byte[] token, SecurityTokenResolver tokenResolver) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetTransforms (IEnumerable<CookieTransform> transforms) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ValidateSession (SessionSecurityToken securityToken) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ReadOnlyCollection<ClaimsIdentity> ValidateToken (SecurityToken token) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ReadOnlyCollection<ClaimsIdentity> ValidateToken (SessionSecurityToken token, string endpointId) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual byte[] WriteToken (SessionSecurityToken sessionToken) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteToken (XmlWriter writer, SecurityToken token) {
			throw new NotImplementedException ();
		}
	}
}