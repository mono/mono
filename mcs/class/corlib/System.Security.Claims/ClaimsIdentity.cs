//
// ClaimIdentity.cs
//
// Authors:
//  Miguel de Icaza (miguel@xamarin.com)
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright 2014 Xamarin Inc
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

using System.Collections.Generic;
using System.Security.Principal;
using System.Runtime.Serialization;

namespace System.Security.Claims {

	[Serializable]
	public class ClaimsIdentity : IIdentity {
		[NonSerializedAttribute]
		public const string DefaultNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
		[NonSerializedAttribute]
		public const string DefaultRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
		[NonSerializedAttribute]
		public const string DefaultIssuer = "LOCAL AUTHORITY";
		
		readonly List<Claim> claims;
		ClaimsIdentity actor;
		readonly string auth_type;

		public ClaimsIdentity ()
			: this (claims: null, authenticationType: null, nameType: null, roleType: null)
		{ }
		
		public ClaimsIdentity(IEnumerable<Claim> claims)
			: this (claims: claims, authenticationType: null, nameType: null, roleType: null)
		{ }
		
		public ClaimsIdentity (string authenticationType)
			: this (claims: null, authenticationType: authenticationType, nameType: null, roleType: null)
		{ }

		public ClaimsIdentity (IEnumerable<Claim> claims, string authenticationType) 
			: this (claims, authenticationType, null, null)
		{}
		
		public ClaimsIdentity (string authenticationType, string nameType, string roleType)
			: this (claims: null, authenticationType: authenticationType, nameType: nameType, roleType: roleType)
		{ }
		
		public ClaimsIdentity (IIdentity identity) : this (identity: identity, claims: null)
		{
		}
		
		public ClaimsIdentity(IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
			: this (identity: null, claims: claims, authenticationType: authenticationType, nameType: nameType, roleType: roleType)
		{
		}

		public ClaimsIdentity (IIdentity identity, IEnumerable<Claim> claims)
			: this (identity, claims, authenticationType: null, nameType: null, roleType: null)
		{
		}
		
		public ClaimsIdentity (IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
		{
			NameClaimType = string.IsNullOrEmpty (nameType) ? DefaultNameClaimType : nameType;
			RoleClaimType = string.IsNullOrEmpty (roleType) ? DefaultRoleClaimType : roleType;
			auth_type = authenticationType;

			this.claims = new List<Claim> ();

			if (identity != null) {
				if (string.IsNullOrEmpty (authenticationType))
					auth_type = identity.AuthenticationType;

				var ci = identity as ClaimsIdentity;
				if (ci != null) {
					actor = ci.Actor;
					BootstrapContext = ci.BootstrapContext;
					foreach (var c in ci.Claims)
						this.claims.Add (c);
				
					Label = ci.Label;
					NameClaimType = string.IsNullOrEmpty (nameType) ? ci.NameClaimType : nameType;
					RoleClaimType = string.IsNullOrEmpty (roleType) ? ci.RoleClaimType : roleType;
				} else if (!string.IsNullOrEmpty (identity.Name)) {
					AddDefaultClaim (identity.Name);
				}
			}

			if (claims != null) {
				AddClaims (claims);
			}
		}

		[MonoTODO]
		protected ClaimsIdentity (SerializationInfo info)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ClaimsIdentity (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			throw new NotImplementedException ();
		}
		
		public ClaimsIdentity Actor {
			get {
				return actor;
			}
			set {
				if (value == this)
					throw new InvalidOperationException ("can not set the Actor property to this instance");

				actor = value;
			}
		}

		public virtual string AuthenticationType {
			get {
				return auth_type;
			}
		}
		public object BootstrapContext { get; set; }
		public string Label { get; set; }
		public virtual string Name {
			get {
				var target = NameClaimType;
				foreach (var c in claims){
					if (c.Type == target)
						return c.Value;
				}
				return null;
			}
		}
		public string NameClaimType { get; private set; }
		public string RoleClaimType { get; private set; }

		public virtual IEnumerable<Claim> Claims {
			get {
				return claims;
			}
		}

		public virtual bool IsAuthenticated {
			get {
				return AuthenticationType != null && AuthenticationType != "";
			}
		}

		public virtual void AddClaim (Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException ("claim");

			if (claim.Subject != this)
				claim = claim.Clone (this);

			claims.Add (claim);
		}

		public virtual void AddClaims (IEnumerable<Claim> claims)
		{
			if (claims == null)
				throw new ArgumentNullException ("claims");

			foreach (var c in claims)
				AddClaim (c);
		}

		internal void AddDefaultClaim (string identityName)
		{
			this.claims.Add (new Claim (NameClaimType, identityName, "http://www.w3.org/2001/XMLSchema#string", DefaultIssuer, DefaultIssuer, this)); 
		}

		public virtual ClaimsIdentity Clone ()
		{
			return new ClaimsIdentity (null, claims, AuthenticationType, NameClaimType, RoleClaimType){
				BootstrapContext = this.BootstrapContext,
				Actor = this.Actor,
				Label = this.Label
			};
		}

		public virtual IEnumerable<Claim> FindAll(Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var c in claims)
				if (match (c))
					yield return c;
		}

		public virtual IEnumerable<Claim> FindAll (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			foreach (var c in claims)
				if (string.Equals (c.Type, type, StringComparison.OrdinalIgnoreCase))
					yield return c;
		}

		public virtual Claim FindFirst (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var c in claims)
				if (match (c))
					return c;
			return null;
		}

		public virtual Claim FindFirst (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			foreach (var c in claims)
				if (string.Equals (c.Type, type, StringComparison.OrdinalIgnoreCase))
					return c;
			return null;
		}

		public virtual bool HasClaim (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var c in claims)
				if (match (c))
					return true;
			return false;
		}

		public virtual bool HasClaim (string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (value == null)
				throw new ArgumentNullException ("value");
			foreach (var c in claims){
				if (string.Equals (c.Type, type, StringComparison.OrdinalIgnoreCase) && c.Value == value)
					return true;
			}
			return false;
		}

		public virtual void RemoveClaim (Claim claim)
		{
			if (!TryRemoveClaim (claim))
				throw new InvalidOperationException ();
		}

		[MonoTODO ("This one should return false if the claim is owned by someone else, this does not exist yet")]
		public virtual bool TryRemoveClaim (Claim claim)
		{
			if (claim == null)
				return true;
			claims.Remove (claim);
			return true;
		}
	}
}
