//
// ClaimsPrincipal.cs
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
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.SimpleLinq;

namespace System.Security.Claims {
	[ComVisible (true)]
	[Serializable]
	public class ClaimsPrincipal : IPrincipal {
		[NonSerialized]
		private const string ClaimsPrincipalNamespace = "System.Security.ClaimsPrincipal.";
		[NonSerialized]
		private const string IdentitiesKey = ClaimsPrincipalNamespace + "Identities";
		[NonSerialized]
		private const string VersionKey = ClaimsPrincipalNamespace + "Version";

		[NonSerialized]
		private static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> primaryIdentitySelector = null;
		[NonSerialized]
		private static Func<ClaimsPrincipal> claimsPrincipalSelector = null;

		[OptionalField (VersionAdded = 2)]
		private string version = "1.0";
		[NonSerialized]
		private List<ClaimsIdentity> identities = new List<ClaimsIdentity> ();
		[OptionalField (VersionAdded = 2)]
		private string m_serializedClaimsIdentities;
		#region Statics
		public static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> PrimaryIdentitySelector
		{
			get
			{
				return ClaimsPrincipal.primaryIdentitySelector;
			}
			[SecurityCritical]
			set
			{
				ClaimsPrincipal.primaryIdentitySelector = value;
			}
		}

		public static Func<ClaimsPrincipal> ClaimsPrincipalSelector
		{
			get
			{
				return ClaimsPrincipal.claimsPrincipalSelector;
			}
			[SecurityCritical]
			set
			{
				ClaimsPrincipal.claimsPrincipalSelector = value;
			}
		}

		public static ClaimsPrincipal Current
		{
			get
			{
				if (ClaimsPrincipal.claimsPrincipalSelector != null)
					return ClaimsPrincipal.claimsPrincipalSelector ();
				else
					return ClaimsPrincipal.DefaultClaimsPrincipalSelector ();
			}
		}


		private static ClaimsIdentity DefaultPrimaryIdentitySelector (IEnumerable<ClaimsIdentity> identities)
		{
			if (identities == null)
				throw new ArgumentNullException ("identities");

			var windowsIdentity = identities.Select (i => i as WindowsIdentity).Filter (i => i != null).FirstOrDefault ();
			if (windowsIdentity != null) return windowsIdentity;
			return identities.Filter (i => i != null).FirstOrDefault ();
		}

		private static ClaimsPrincipal DefaultClaimsPrincipalSelector ()
		{
			return Thread.CurrentPrincipal as ClaimsPrincipal ?? new ClaimsPrincipal (Thread.CurrentPrincipal);
		}
		#endregion
		public virtual IEnumerable<Claim> Claims
		{
			get
			{
				return Identities.SelectMany (c => c.Claims);
			}
		}

		public virtual IEnumerable<ClaimsIdentity> Identities
		{
			get
			{
				return this.identities.AsReadOnly ();
			}
		}

		public virtual IIdentity Identity
		{
			get
			{
				if (ClaimsPrincipal.primaryIdentitySelector != null)
					return ClaimsPrincipal.primaryIdentitySelector (this.identities);
				else
					return ClaimsPrincipal.DefaultPrimaryIdentitySelector (this.identities);
			}
		}

		public ClaimsPrincipal ()
		{
		}

		public ClaimsPrincipal (IEnumerable<ClaimsIdentity> identities)
		{
			if (identities == null)
				throw new ArgumentNullException ("identities");
			this.identities.AddRange (identities);
		}

		[SecuritySafeCritical]
		public ClaimsPrincipal (IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			AddIdentity (identity);
		}

		[SecuritySafeCritical]
		public ClaimsPrincipal (IPrincipal principal)
		{
			if (principal == null)
				throw new ArgumentNullException ("principal");
			AddIdentities (principal);
		}

		[SecurityCritical]
		protected ClaimsPrincipal (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			this.Deserialize (info, context);
		}

		[SecurityCritical]
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			info.AddValue (IdentitiesKey, this.SerializeIdentities ());
			info.AddValue (VersionKey, this.version);
		}

		[SecurityCritical]
		private void AddIdentity (IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
			if (claimsIdentity == null && identity != null)
				claimsIdentity = new ClaimsIdentity (identity);
			this.AddIdentity (claimsIdentity);
		}

		[SecurityCritical]
		private void AddIdentities (IPrincipal principal)
		{
			if (identities == null)
				throw new ArgumentNullException ("principal");

			ClaimsPrincipal claimsPrincipal = principal as ClaimsPrincipal;
			if (claimsPrincipal == null) {
				AddIdentity (principal.Identity);
			} else {
				if (claimsPrincipal.Identities != null)
					this.AddIdentities (claimsPrincipal.Identities);
			}
		}

		[SecurityCritical]
		public virtual void AddIdentity (ClaimsIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			this.identities.Add (identity);
		}

		[SecurityCritical]
		public virtual void AddIdentities (IEnumerable<ClaimsIdentity> identities)
		{
			if (identities == null)
				throw new ArgumentNullException ("identities");
			this.identities.AddRange (identities);
		}

		public virtual IEnumerable<Claim> FindAll (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Identities.Filter (i => i != null).SelectMany (i => i.FindAll (match)).ToList ().AsReadOnly ();
		}

		public virtual IEnumerable<Claim> FindAll (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			return this.Identities.Filter (i => i != null).SelectMany (i => i.FindAll (type)).ToList ().AsReadOnly ();
		}

		public virtual Claim FindFirst (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Identities.Filter (i => i != null).Select (i => i.FindFirst (match)).Filter (c => c != null).FirstOrDefault ();
		}

		public virtual Claim FindFirst (string type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			return this.Identities.Filter (i => i != null).Select (i => i.FindFirst (type)).Filter (c => c != null).FirstOrDefault ();
		}

		internal virtual bool HasIdentity (Predicate<ClaimsIdentity> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return this.Identities.Filter (i => i != null).Any (match);
		}

		public virtual bool HasClaim (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			return HasIdentity (i => i.HasClaim (match));
		}

		public virtual bool HasClaim (string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (value == null)
				throw new ArgumentNullException ("value");
			return HasIdentity (i => i.HasClaim (type, value));
		}

		public virtual bool IsInRole (string role)
		{
			return HasIdentity (i => i.HasClaim (i.RoleClaimType, role));
		}

		[OnSerializing]
		[SecurityCritical]
		private void OnSerializing (StreamingContext context)
		{
			if (this is ISerializable)
				return;
			this.m_serializedClaimsIdentities = this.SerializeIdentities ();
		}

		[OnDeserialized]
		[SecurityCritical]
		private void OnDeserialized (StreamingContext context)
		{
			if (this is ISerializable)
				return;
			this.DeserializeIdentities (this.m_serializedClaimsIdentities);
			this.m_serializedClaimsIdentities = (string) null;
		}

		[SecurityCritical]
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)]
		private void Deserialize (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			SerializationInfoEnumerator enumerator = info.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				switch (enumerator.Name) {
				case IdentitiesKey:
					this.DeserializeIdentities (info.GetString (IdentitiesKey));
					continue;
				case VersionKey:
					this.version = info.GetString (VersionKey);
					continue;
				default:
					continue;
				}
			}
		}

		[SecurityCritical]
		private void DeserializeIdentities (string identities)
		{
			this.identities = new List<ClaimsIdentity> ();
			if (string.IsNullOrEmpty (identities))
				return;
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			List<string> list = (List<string>) ClaimsUtils.DeserializeString (identities, binaryFormatter);
			for (int i = 0; i < list.Count; i += 2) {
				var claimsIdentity = (ClaimsIdentity) ClaimsUtils.DeserializeString (list [i + 1], binaryFormatter);
				if (!string.IsNullOrEmpty (list [i])) {
					long result;
					if (!long.TryParse (list [i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
						throw new SerializationException ("The Serialization Stream has been corrupted");
					claimsIdentity = (ClaimsIdentity) new WindowsIdentity (claimsIdentity, new IntPtr (result));
				}
				this.identities.Add (claimsIdentity);
			}
		}

		[SecurityCritical]
		private string SerializeIdentities ()
		{
			List<string> serializedIdentities = new List<string> ();
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			foreach (ClaimsIdentity claimsIdentity in this.identities) {
				if (claimsIdentity.GetType () == typeof (WindowsIdentity)) {
					WindowsIdentity windowsIdentity = claimsIdentity as WindowsIdentity;
					serializedIdentities.Add (windowsIdentity.Token.ToInt64 ().ToString ((IFormatProvider) NumberFormatInfo.InvariantInfo));
					serializedIdentities.Add (ClaimsUtils.SerializeToString (windowsIdentity.CloneClaimsIdentity (), binaryFormatter));
				} else {
					serializedIdentities.Add ("");
					serializedIdentities.Add (ClaimsUtils.SerializeToString (claimsIdentity, binaryFormatter));
				}
			}
			return ClaimsUtils.SerializeToString (serializedIdentities, binaryFormatter);
		}
	}
}
#endif