//
// ClaimsIdentity.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
#if NET_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.SimpleLinq;

namespace System.Security.Claims {
	[ComVisible (true)]
	[Serializable]
	public class ClaimsIdentity : IIdentity {
		[NonSerialized]
		public const string DefaultIssuer = "LOCAL AUTHORITY";
		[NonSerialized]
		public const string DefaultNameClaimType = ClaimTypes.Name;
		[NonSerialized]
		public const string DefaultRoleClaimType = ClaimTypes.Role;

		[NonSerialized]
		private const string ClaimsIdentityNamespace = "System.Security.ClaimsIdentity.";
		[NonSerialized]
		private const string ActorSerializeKey = ClaimsIdentityNamespace + "actor";
		[NonSerialized]
		private const string AuthenticationTypeSerializeKey = ClaimsIdentityNamespace + "authenticationType";
		[NonSerialized]
		private const string BootstrapContextSerializeKey = ClaimsIdentityNamespace + "bootstrapContext";
		[NonSerialized]
		private const string ClaimsSerializeKey = ClaimsIdentityNamespace + "claims";
		[NonSerialized]
		private const string LabelSerializeKey = ClaimsIdentityNamespace + "label";
		[NonSerialized]
		private const string NameClaimTypeSerializeKey = ClaimsIdentityNamespace + "nameClaimType";
		[NonSerialized]
		private const string RoleClaimTypeSerializeKey = ClaimsIdentityNamespace + "roleClaimType";
		[NonSerialized]
		private const string VersionSerializeKey = ClaimsIdentityNamespace + "version";

		[NonSerialized]
		private List<Claim> claims = new List<Claim> ();
		[OptionalField (VersionAdded = 2)]
		private string serializedClaims;

		[OptionalField (VersionAdded = 2)]
		private string nameClaimType;
		[OptionalField (VersionAdded = 2)]
		private string roleClaimType;
		[OptionalField (VersionAdded = 2)]
		private string version = "1.0";
		[OptionalField (VersionAdded = 2)]
		private ClaimsIdentity actor;
		[OptionalField (VersionAdded = 2)]
		private string authenticationType;
		[OptionalField (VersionAdded = 2)]
		private object bootstrapContext;
		[OptionalField (VersionAdded = 2)]
		private string label;

		public ClaimsIdentity Actor
		{
			get
			{
				return this.actor;
			}
			set
			{
				if (this.IsReferencingThis (value))
					throw new InvalidOperationException ("Can't set the Actor property to point to itself");
				this.actor = value;
			}
		}

		public virtual string AuthenticationType
		{
			get
			{
				return this.authenticationType;
			}
		}

		public object BootstrapContext
		{
			get
			{
				return this.bootstrapContext;
			}
			[SecurityCritical]
			set
			{
				this.bootstrapContext = value;
			}
		}

		public virtual IEnumerable<Claim> Claims
		{
			get
			{
				return this.claims.AsReadOnly ();
			}
		}

		public virtual bool IsAuthenticated
		{
			get
			{
				return !string.IsNullOrEmpty (this.authenticationType);
			}
		}

		public string Label
		{
			get
			{
				return this.label;
			}
			set
			{
				this.label = value;
			}
		}

		public virtual string Name
		{
			get
			{
				Claim first = this.FindFirst (this.nameClaimType);
				if (first != null)
					return first.Value;

				return null;
			}
		}

		public string NameClaimType
		{
			get
			{
				return this.nameClaimType;
			}
		}

		public string RoleClaimType
		{
			get
			{
				return this.roleClaimType;
			}
		}

		public ClaimsIdentity ()
			: this ((IIdentity) null)
		{
		}

		public ClaimsIdentity (IIdentity identity)
			: this (identity, null)
		{
		}

		public ClaimsIdentity (IEnumerable<Claim> claims)
			: this (null, claims, null, null, null)
		{
		}

		public ClaimsIdentity (string authenticationType)
			: this (null, null, authenticationType, null, null)
		{
		}

		public ClaimsIdentity (IEnumerable<Claim> claims, string authenticationType)
			: this (null, claims, authenticationType, null, null)
		{
		}

		public ClaimsIdentity (IIdentity identity, IEnumerable<Claim> claims)
			: this (identity, claims, null, null, null)
		{
		}

		public ClaimsIdentity (string authenticationType, string nameClaimType, string roleClaimType)
			: this (null, null, authenticationType, nameClaimType, roleClaimType)
		{
		}

		public ClaimsIdentity (IEnumerable<Claim> claims, string authenticationType, string nameClaimType, string roleClaimType)
			: this (null, claims, authenticationType, nameClaimType, roleClaimType)
		{
		}

		public ClaimsIdentity (IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameClaimType, string roleClaimType)
		{
			this.nameClaimType = nameClaimType;
			this.roleClaimType = roleClaimType;
			var claimsIdentity = identity as ClaimsIdentity;
			if (claimsIdentity != null) {
				Actor = claimsIdentity.Actor;
				this.bootstrapContext = claimsIdentity.BootstrapContext;
				this.claims.AddRange (claimsIdentity.Claims.Filter (c => c != null));
				this.label = claimsIdentity.Label;
				if (string.IsNullOrEmpty (this.roleClaimType)) {
					this.roleClaimType = claimsIdentity.RoleClaimType;
				}
				if (string.IsNullOrEmpty (this.nameClaimType)) {
					this.nameClaimType = claimsIdentity.NameClaimType;
				}
			} else {
				if (string.IsNullOrEmpty (this.roleClaimType)) {
					this.roleClaimType = DefaultRoleClaimType;
				}
				if (string.IsNullOrEmpty (this.nameClaimType)) {
					this.nameClaimType = DefaultNameClaimType;
				}
				if (identity != null) {
					if (!string.IsNullOrEmpty (identity.Name)) {
						this.AddClaim (
							new Claim (this.nameClaimType, identity.Name, ClaimValueTypes.String, DefaultIssuer, DefaultIssuer, this));
					}
				}
			}
			this.authenticationType = (string.IsNullOrEmpty (this.authenticationType) && identity != null) ? identity.AuthenticationType : authenticationType;
			if (claims != null)
				this.AddClaims (claims.Filter (c => c != null));
		}

		[SecurityCritical]
		protected ClaimsIdentity (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			this.Deserialize (info, context, false);
		}

		[SecurityCritical]
		protected ClaimsIdentity (SerializationInfo info)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			StreamingContext context = new StreamingContext ();
			this.Deserialize (info, context, true);
		}

		public virtual ClaimsIdentity Clone ()
		{
			ClaimsIdentity claimsIdentity = new ClaimsIdentity (this.claims);
			claimsIdentity.authenticationType = this.authenticationType;
			claimsIdentity.bootstrapContext = this.bootstrapContext;
			claimsIdentity.label = this.label;
			claimsIdentity.nameClaimType = this.nameClaimType;
			claimsIdentity.roleClaimType = this.roleClaimType;
			claimsIdentity.Actor = this.Actor;
			return claimsIdentity;
		}

		[SecurityCritical]
		public virtual void AddClaim (Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException ("claim");
			if (!object.ReferenceEquals (claim.Subject, this))
				claim = claim.Clone (this);
			this.claims.Add (claim);
		}


		[SecurityCritical]
		public virtual void AddClaims (IEnumerable<Claim> claims)
		{
			if (claims == null)
				throw new ArgumentNullException ("claims");
			foreach (Claim claim in claims) {
				if (claim != null)
					this.AddClaim (claim);
			}
		}

		[SecurityCritical]
		public virtual bool TryRemoveClaim (Claim claim)
		{
			return this.claims.RemoveAll (c => object.ReferenceEquals (c, claim)) > 0;
		}

		[SecurityCritical]
		public virtual void RemoveClaim (Claim claim)
		{
			if (!this.TryRemoveClaim (claim))
				throw new InvalidOperationException (String.Format ("The Claim could not be removed as it was not found: {0}", claim));
		}

		public virtual IEnumerable<Claim> FindAll (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Claims.Filter (match).ToList ().AsReadOnly ();
		}

		public virtual IEnumerable<Claim> FindAll (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			return this.FindAll (claim => claim != null && string.Equals (claim.Type, type, StringComparison.OrdinalIgnoreCase));
		}

		public virtual bool HasClaim (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Claims.Any (match);
		}

		public virtual bool HasClaim (string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (value == null)
				throw new ArgumentNullException ("value");
			return HasClaim (claim => (string.Equals (claim.Type, type, StringComparison.OrdinalIgnoreCase) && string.Equals (claim.Value, value, StringComparison.Ordinal)));
		}

		public virtual Claim FindFirst (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Claims.Filter (match).FirstOrDefault ();
		}

		public virtual Claim FindFirst (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			return FindFirst (claim => (string.Equals (claim.Type, type, StringComparison.OrdinalIgnoreCase)));
		}

		private bool IsReferencingThis (ClaimsIdentity subject)
		{
			ClaimsIdentity identity = subject;
			while (identity != null) {
				if (object.ReferenceEquals (this, identity))
					return true;
				identity = identity.Actor;
			}
			return false;
		}


		[SecurityCritical]
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			if (this.actor != null) {
				info.AddValue (ActorSerializeKey, ClaimsUtils.SerializeToString (this.actor, binaryFormatter));
			}

			if (!string.IsNullOrEmpty (this.authenticationType))
				info.AddValue (AuthenticationTypeSerializeKey, this.authenticationType);
			if (this.bootstrapContext != null)
				info.AddValue (ActorSerializeKey, ClaimsUtils.SerializeToString (this.bootstrapContext, binaryFormatter));
			info.AddValue (ClaimsSerializeKey, ClaimsUtils.SerializeToString (this.claims));
			if (!string.IsNullOrEmpty (this.label))
				info.AddValue (LabelSerializeKey, this.label);
			info.AddValue (NameClaimTypeSerializeKey, this.nameClaimType);
			info.AddValue (RoleClaimTypeSerializeKey, this.roleClaimType);
			info.AddValue (VersionSerializeKey, this.version);
		}

		[SecurityCritical]
		[OnSerializing]
		private void OnSerializing (StreamingContext context)
		{
			if (this is ISerializable)
				return;
			this.serializedClaims = ClaimsUtils.SerializeToString (this.claims);
		}

		[OnDeserialized]
		[SecurityCritical]
		private void OnDeserialized (StreamingContext context)
		{
			if (this is ISerializable)
				return;
			if (!string.IsNullOrEmpty (this.serializedClaims)) {
				this.DeserializeClaims (this.serializedClaims);
				this.serializedClaims = null;
			}
			this.nameClaimType = string.IsNullOrEmpty (this.nameClaimType) ? DefaultNameClaimType : this.nameClaimType;
			this.roleClaimType = string.IsNullOrEmpty (this.roleClaimType) ? DefaultRoleClaimType : this.roleClaimType;
		}

		[OnDeserializing]
		private void OnDeserializing (StreamingContext context)
		{
			if (this is ISerializable)
				return;
			this.claims = new List<Claim> ();
		}

		[SecurityCritical]
		private void DeserializeClaims (string serializedClaims, BinaryFormatter formatter = null)
		{
			if (!string.IsNullOrEmpty (serializedClaims)) {
				this.claims =
					((List<Claim>) ClaimsUtils.DeserializeString (serializedClaims, formatter)).Filter (c => c != null).Select (c => { c.Subject = this; return c; }).ToList ();
			}
			if (this.claims == null)
				this.claims = new List<Claim> ();
		}

		[SecurityCritical]
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		private void Deserialize (SerializationInfo info, StreamingContext context, bool useContext)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			BinaryFormatter binaryFormatter = !useContext ? new BinaryFormatter () : new BinaryFormatter ((ISurrogateSelector) null, context);
			SerializationInfoEnumerator enumerator = info.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				switch (enumerator.Name) {
				case ActorSerializeKey:
					this.actor = (ClaimsIdentity) ClaimsUtils.DeserializeString (info.GetString (ActorSerializeKey), binaryFormatter);
					continue;
				case AuthenticationTypeSerializeKey:
					this.authenticationType = info.GetString (AuthenticationTypeSerializeKey);
					continue;
				case BootstrapContextSerializeKey:
					this.bootstrapContext = ClaimsUtils.DeserializeString (info.GetString (BootstrapContextSerializeKey), binaryFormatter);
					continue;
				case ClaimsSerializeKey:
					this.DeserializeClaims (info.GetString (ClaimsSerializeKey));
					continue;
				case LabelSerializeKey:
					this.label = info.GetString (LabelSerializeKey);
					continue;
				case NameClaimTypeSerializeKey:
					this.nameClaimType = info.GetString (NameClaimTypeSerializeKey);
					continue;
				case RoleClaimTypeSerializeKey:
					this.roleClaimType = info.GetString (RoleClaimTypeSerializeKey);
					continue;
				case VersionSerializeKey:
					info.GetString (VersionSerializeKey);
					continue;
				default:
					continue;
				}
			}
		}
	}
}
#endif