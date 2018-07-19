//
// TestEvaluationContext.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

using Authenticator = System.IdentityModel.Selectors.CustomUserNameSecurityTokenAuthenticator;
using PolicyCollection = System.Collections.ObjectModel.ReadOnlyCollection<System.IdentityModel.Policy.IAuthorizationPolicy>;

namespace MonoTests.System.IdentityModel.Selectors
{
	class TestEvaluationContext : EvaluationContext
	{
		Collection<ClaimSet> claim_sets =
			new Collection<ClaimSet> ();
		ReadOnlyCollection<ClaimSet> readonly_claim_sets;
		Dictionary<string,object> properties =
			new Dictionary<string,object> ();
		int generation;
		DateTime expiration;

		public override ReadOnlyCollection<ClaimSet> ClaimSets {
			get {
				if (readonly_claim_sets == null)
					readonly_claim_sets = new ReadOnlyCollection<ClaimSet> (claim_sets);
				return readonly_claim_sets;
			}
		}

		public DateTime ExpirationTime {
			get { return expiration; }
		}

		public override int Generation {
			get { return generation; }
		}

		public override IDictionary<string,object> Properties {
			get { return properties; }
		}

		public override void AddClaimSet (IAuthorizationPolicy policy, ClaimSet claimSet)
		{
			claim_sets.Add (claimSet);
		}

		public override void RecordExpirationTime (DateTime expirationTime)
		{
			expiration = expirationTime;
		}

	}
}
#endif 
