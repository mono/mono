//
// SecurityContextSecretSecurityToken.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security.Tokens
{
	public class SecurityContextSecurityToken : SecurityToken
	{
		#region Static members 

		public static SecurityContextSecurityToken CreateCookieSecurityContextToken (
			UniqueId contextId,
			string id,
			byte [] key,
			DateTime validFrom,
			DateTime validTo,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies,
			SecurityStateEncoder securityStateEncoder)
		{
			return CreateCookieSecurityContextToken (
				contextId, id, key, validFrom, validTo, new UniqueId (Guid.NewGuid ()), validFrom, validTo, authorizationPolicies, securityStateEncoder);
		}

		public static SecurityContextSecurityToken CreateCookieSecurityContextToken (
			UniqueId contextId,
			string id,
			byte [] key,
			DateTime validFrom,
			DateTime validTo,
			UniqueId keyGeneration,
			DateTime keyEffectiveTime,
			DateTime keyExpirationTime,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies,
			SecurityStateEncoder securityStateEncoder)
		{
			if (securityStateEncoder == null)
				throw new ArgumentNullException ("securityStateEncoder");

			SecurityContextSecurityToken sct = new SecurityContextSecurityToken (
				contextId, id, key, validFrom, validTo,
				keyGeneration, keyEffectiveTime, 
				keyExpirationTime,  authorizationPolicies);
			byte [] rawdata = SslnegoCookieResolver.CreateData (
				contextId, keyGeneration, key,
				validFrom, validTo,
				keyEffectiveTime, keyExpirationTime);
			sct.cookie = securityStateEncoder.EncodeSecurityState (rawdata);
			return sct;
		}

		#endregion

		string id;
		InMemorySymmetricSecurityKey key;
		ReadOnlyCollection<SecurityKey> keys;
		DateTime token_since, token_until, key_since, key_until;
		UniqueId context_id, key_generation;
		ReadOnlyCollection<IAuthorizationPolicy> policies;
		byte [] cookie;

		public SecurityContextSecurityToken (
			UniqueId contextId,
			byte[] key,
			DateTime validFrom,
			DateTime validTo)
			: this (contextId, new UniqueId ().ToString (), key, validFrom, validTo)
		{
		}

		public SecurityContextSecurityToken (
			UniqueId contextId,
			string id,
			byte[] key,
			DateTime validFrom,
			DateTime validTo)
			: this (contextId, id, key, validFrom, validTo, null)
		{
		}

		public SecurityContextSecurityToken (
			UniqueId contextId,
			string id,
			byte[] key,
			DateTime validFrom,
			DateTime validTo,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
		{
			context_id = contextId;
			this.id = id;
			this.key = new InMemorySymmetricSecurityKey (key);
			token_since = validFrom;
			token_until = validTo;
			if (authorizationPolicies == null)
				authorizationPolicies = new ReadOnlyCollection<IAuthorizationPolicy> (new Collection<IAuthorizationPolicy> ());
			policies = authorizationPolicies;
		}

		public SecurityContextSecurityToken (
			UniqueId contextId,
			string id,
			byte[] key,
			DateTime validFrom,
			DateTime validTo,
			UniqueId keyGeneration,
			DateTime keyEffectiveTime,
			DateTime keyExpirationTime,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
			: this (contextId, id, key, validFrom, validTo, authorizationPolicies)
		{
			key_generation = keyGeneration;
			key_since = keyEffectiveTime;
			key_until = keyExpirationTime;
		}

		public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies {
			get { return policies; }
		}

		public UniqueId ContextId {
			get { return context_id; }
		}

		public UniqueId KeyGeneration {
			get { return key_generation; }
		}

		public DateTime KeyEffectiveTime {
			get { return key_since; }
		}

		public DateTime KeyExpirationTime {
			get { return key_until; }
		}

		public override DateTime ValidFrom {
			get { return token_since; }
		}

		public override DateTime ValidTo {
			get { return token_until; }
		}

		public override string Id {
			get { return id; }
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get {
				if (keys == null)
					keys = new ReadOnlyCollection<SecurityKey> (new SecurityKey [] {key});
				return keys;
			}
		}

		internal byte [] Cookie {
			get { return cookie; }
			set { cookie = value; }
		}

		public override bool CanCreateKeyIdentifierClause<T> ()
		{
			return typeof (T) == typeof (SecurityContextKeyIdentifierClause);
		}

		public override T CreateKeyIdentifierClause<T> ()
		{
			Type t = typeof (T);
			if (t == typeof (SecurityContextKeyIdentifierClause))
				return (T) (object) new SecurityContextKeyIdentifierClause (ContextId, KeyGeneration);

			throw new NotSupportedException (String.Format ("X509SecurityToken does not support creation of {0}.", t));
		}

		public override bool MatchesKeyIdentifierClause (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			SecurityContextKeyIdentifierClause sctic =
				keyIdentifierClause as SecurityContextKeyIdentifierClause;
			return sctic != null && sctic.ContextId == ContextId &&
			       sctic.Generation == KeyGeneration;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
