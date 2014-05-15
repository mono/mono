//
// ClaimPrincipal.cs
//
// Authors:
//  Miguel de Icaza (miguel@xamarin.com)
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
#if NET_4_5
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Security.Claims {

	[SerializableAttribute]
	public class ClaimsPrincipal : IPrincipal
	{
		List<ClaimsIdentity> identities;

		static ClaimsPrincipal ()
		{
			ClaimsPrincipalSelector = DefaultClaimsPrincipal;
		}

		static ClaimsPrincipal DefaultClaimsPrincipal ()
		{
			return Thread.CurrentPrincipal as ClaimsPrincipal;
		}
		
		public ClaimsPrincipal ()
		{
			identities =  new List<ClaimsIdentity>();
		}

		public ClaimsPrincipal (IEnumerable<ClaimsIdentity> identities)
		{
			if (identities == null)
				throw new ArgumentNullException ("identities");
			
			identities = new List<ClaimsIdentity> (identities);
		}

		public ClaimsPrincipal (IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			// TODO
		}

		public ClaimsPrincipal (IPrincipal principal)
		{
			if (principal == null)
				throw new ArgumentNullException ("principal");
			var cp = principal as ClaimsPrincipal;
			if (cp != null)
				identities = new List<ClaimsIdentity> (cp.identities);
			else {
				identities = new List<ClaimsIdentity> ();
				identities.Add (new ClaimsIdentity (principal.Identity));
			}
		}

		[MonoTODO]
		protected ClaimsPrincipal (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		public virtual IEnumerable<Claim> Claims {
			get {
				foreach (var ci in identities)
					foreach (var claim in ci.Claims)
						yield return claim;
			}
		}

		public static Func<ClaimsPrincipal> ClaimsPrincipalSelector { get; set; }

		public static ClaimsPrincipal Current {
			get {
				return ClaimsPrincipalSelector ();
			}
		}

		public virtual IEnumerable<ClaimsIdentity> Identities {
			get {
				return identities;
			}
		}

		public static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> PrimaryIdentitySelector { get; set; }

		public virtual IIdentity Identity {
			get {
				if (identities == null)
					throw new ArgumentNullException ("Identities");

				if (PrimaryIdentitySelector != null)
					return PrimaryIdentitySelector (identities);
						
				ClaimsIdentity firstCI = null;
				foreach (var ident in identities){
					if (ident is WindowsIdentity)
						return ident;
					if (firstCI == null && ident is ClaimsIdentity)
						firstCI = ident as ClaimsIdentity;
				}
				return firstCI;
			}
		}

		public virtual void AddIdentities (IEnumerable<ClaimsIdentity> identities)
		{
			if (identities == null)
				throw new ArgumentNullException ("identities");
			foreach (var id in identities)
				this.identities.Add (id);
		}

		public virtual void AddIdentity (ClaimsIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			identities.Add (identity);
		}

		public virtual IEnumerable<Claim> FindAll (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var claim in Claims){
				if (match (claim))
					yield return claim;
			}
		}

		public virtual Claim FindFirst (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var claim in Claims)
				if (match (claim))
					return claim;
			return null;
		}

		public virtual bool HasClaim (Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
			foreach (var claim in Claims)
				if (match (claim))
					return true;
			return false;
		}

		public virtual bool IsInRole (string role)
		{
			foreach (var id in identities){
				if (id.HasClaim (id.RoleClaimType, role))
					return true;
			}
			return false;
		}
		
	}
}
#endif