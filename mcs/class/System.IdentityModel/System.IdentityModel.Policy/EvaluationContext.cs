//
// EvaluationContext.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;

namespace System.IdentityModel.Policy
{
	public abstract class EvaluationContext
	{
		protected EvaluationContext ()
		{
		}

		public abstract int Generation { get; }

		public abstract IDictionary<string,object> Properties { get; }

		public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }

		public abstract void AddClaimSet (
			IAuthorizationPolicy policy,
			ClaimSet claimSet);

		public abstract void RecordExpirationTime (DateTime expirationTime);
	}

	// default implementation

	internal class DefaultEvaluationContext : EvaluationContext
	{
		DateTime expiration_time = DateTime.MaxValue.AddDays (-1);
		int generation;
		Collection<ClaimSet> claim_sets;
		ReadOnlyCollection<ClaimSet> exposed_claim_sets;
		Dictionary<string,object> properties =
			new Dictionary<string,object> ();
		Dictionary<IAuthorizationPolicy,ClaimSet> claim_set_map =
			new Dictionary<IAuthorizationPolicy,ClaimSet> ();

		public DefaultEvaluationContext ()
		{
			claim_sets = new Collection<ClaimSet> ();
			exposed_claim_sets =
				new ReadOnlyCollection<ClaimSet> (claim_sets);
		}

		public override int Generation {
			get { return generation; }
		}

		public override IDictionary<string,object> Properties {
			get { return properties; }
		}

		public override ReadOnlyCollection<ClaimSet> ClaimSets {
			get { return exposed_claim_sets; }
		}

		public override void AddClaimSet (
			IAuthorizationPolicy authorizationPolicy,
			ClaimSet claimSet)
		{
			generation++;
			claim_set_map.Add (authorizationPolicy, claimSet);
			claim_sets.Add (claimSet);
		}

		public override void RecordExpirationTime (DateTime time)
		{
			expiration_time = time;
		}

		internal DateTime ExpirationTime {
			get { return expiration_time; }
		}
	}

}
