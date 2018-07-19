//
// AuthorizationContext.cs
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
using System.Xml;

namespace System.IdentityModel.Policy
{
	public abstract class AuthorizationContext :
		IAuthorizationComponent
	{
		[MonoTODO]
		public static AuthorizationContext 
			CreateDefaultAuthorizationContext (IList<IAuthorizationPolicy> authorizationPolicies)
		{
			if (authorizationPolicies == null)
				throw new ArgumentNullException ("authorizationPolicies");

			string id = new UniqueId ().ToString ();
			DefaultEvaluationContext ctx =
				new DefaultEvaluationContext ();
			foreach (IAuthorizationPolicy a in authorizationPolicies) {
				object o = null;
				a.Evaluate (ctx, ref o);
			}

			return new DefaultAuthorizationContext (id, ctx);
		}

		protected AuthorizationContext ()
		{
		}

		public abstract DateTime ExpirationTime { get; }

		public abstract string Id { get; }

		public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }

		public abstract IDictionary<string,object> Properties { get; }

		// default implementation: this class will be used for 
		// CreateDefaultAuthorizationContext().
		class DefaultAuthorizationContext : AuthorizationContext
		{
			DefaultEvaluationContext ctx;
			string id;

			public DefaultAuthorizationContext (
				string id, DefaultEvaluationContext context)
			{
				this.id = id;
				this.ctx = context;
			}

			public override DateTime ExpirationTime {
				get { return ctx.ExpirationTime; }
			}

			public override string Id {
				get { return id; }
			}

			public override ReadOnlyCollection<ClaimSet> ClaimSets {
				get { return ctx.ClaimSets; }
			}

			public override IDictionary<string,object> Properties {
				get { return ctx.Properties; }
			}
		}

	}
}
