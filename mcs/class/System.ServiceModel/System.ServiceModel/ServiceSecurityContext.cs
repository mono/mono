//
// ServiceSecurityContext.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
	public class ServiceSecurityContext
	{
		static ServiceSecurityContext anonymous = new ServiceSecurityContext (new ReadOnlyCollection<IAuthorizationPolicy> (new IAuthorizationPolicy [0]));
		static ServiceSecurityContext current;

		[MonoTODO]
		public static ServiceSecurityContext Anonymous {
			get { return anonymous; }
		}

		[MonoTODO] // null by default?
		public static ServiceSecurityContext Current {
			get { return current; }
		}

		AuthorizationContext context;
		ReadOnlyCollection<IAuthorizationPolicy> policies;
		IIdentity primary_identity;

		public ServiceSecurityContext (AuthorizationContext authorizationContext)
			: this (authorizationContext, new ReadOnlyCollection<IAuthorizationPolicy> (new IAuthorizationPolicy [0]))
		{
		}

		public ServiceSecurityContext (
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
			: this (AuthorizationContext.CreateDefaultAuthorizationContext (authorizationPolicies), authorizationPolicies)
		{
		}

		public ServiceSecurityContext (AuthorizationContext authorizationContext,
			ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
		{
			if (authorizationContext == null)
				throw new ArgumentNullException ("authorizationContext");
			if (authorizationPolicies == null)
				throw new ArgumentNullException ("authorizationPolicies");
			this.policies = authorizationPolicies;
			this.context = authorizationContext;

			// FIXME: get correct identity
			primary_identity = new GenericIdentity (String.Empty);
		}

		public AuthorizationContext AuthorizationContext {
			get { return context; }
		}

		public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies {
			get { return policies; }
			set { policies = value; }
		}

		[MonoTODO]
		public bool IsAnonymous {
			get { return policies.Count == 0; }
		}

		[MonoTODO]
		public IIdentity PrimaryIdentity {
			get { return primary_identity; }
		}

		[MonoTODO]
		public WindowsIdentity WindowsIdentity {
			get { throw new NotImplementedException (); }
		}
	}
}
