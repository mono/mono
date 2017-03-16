//
// ClaimSet.cs
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
	public abstract class ClaimSet : IEnumerable<Claim>, IEnumerable
	{
		static ClaimSet system = DefaultClaimSet.CreateSystemClaimSet ();
		static ClaimSet win;

		public static ClaimSet System {
			get { return system; }
		}

		[MonoTODO]
		public static ClaimSet Windows {
			get { return win; }
		}

		protected ClaimSet ()
		{
		}

		public abstract int Count { get; }

		public abstract ClaimSet Issuer { get; }

		public abstract Claim this [int index] { get; }

		public virtual bool ContainsClaim (Claim claim)
		{
			return ContainsClaim (claim, Claim.DefaultComparer);
		}

		public virtual bool ContainsClaim (Claim claim, IEqualityComparer<Claim> comparer)
		{
			foreach (Claim c in this)
				if (comparer.Equals (claim, c))
					return true;
			return false;
		}

		public abstract IEnumerable<Claim> FindClaims (
			string claimType, string right);

		public abstract IEnumerator<Claim> GetEnumerator ();

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
