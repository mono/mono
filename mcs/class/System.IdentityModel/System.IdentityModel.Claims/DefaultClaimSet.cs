//
// DefaultClaimSet.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.IdentityModel.Claims
{
	[DataContract (Namespace="http://schemas.xmlsoap.org/ws/2005/05/identity")]
	public class DefaultClaimSet : ClaimSet
	{
		internal static DefaultClaimSet CreateSystemClaimSet ()
		{
			DefaultClaimSet s = new DefaultClaimSet ();
			s.Initialize (s, new Claim [] {Claim.System, new Claim (ClaimTypes.System, "System", Rights.PossessProperty)});
			return s;
		}

		List<Claim> list = new List<Claim> ();
		ClaimSet issuer;

		// Constructors

		internal DefaultClaimSet ()
		{
		}

		public DefaultClaimSet (params Claim[] claims)
		{
			list.AddRange (claims);
		}

		public DefaultClaimSet (IList<Claim> claims)
		{
			list.AddRange (claims);
		}

		public DefaultClaimSet (ClaimSet issuer, params Claim[] claims)
		{
			this.issuer = issuer;
			list.AddRange (claims);
		}

		public DefaultClaimSet (ClaimSet issuer, IList<Claim> claims)
		{
			this.issuer = issuer;
			list.AddRange (claims);
		}

		// Properties

		public override int Count {
			get { return list.Count; }
		}

		public override ClaimSet Issuer {
			get { return issuer; }
		}

		public override Claim this [int index] {
			get { return list [index]; }
		}

		// Methods

		public override bool ContainsClaim (Claim claim)
		{
			return base.ContainsClaim (claim);
		}

		public override IEnumerable<Claim> FindClaims (
			string claimType, string right)
		{
			return new ClaimListFilter (this, claimType);
		}

		public override IEnumerator<Claim> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		protected void Initialize (ClaimSet issuer, IList<Claim> claims)
		{
			this.issuer = issuer;
			foreach (Claim c in claims)
				list.Add (c);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		// Types

		class ClaimListFilter : IEnumerable<Claim>
		{
			DefaultClaimSet source;
			string claim_type;

			public ClaimListFilter (DefaultClaimSet source, string claimType)
			{
				claim_type = claimType;
				this.source = source;
			}

			public IEnumerator<Claim> GetEnumerator ()
			{
				foreach (Claim c in source)
					if (c.ClaimType == claim_type)
						yield return c;
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}
		}
	}
}
