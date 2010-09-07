//
// ServiceAuthorizationBehavior.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web.Security;

namespace System.ServiceModel.Description
{
	public sealed class ServiceAuthorizationBehavior : IServiceBehavior
	{
		bool impersonate;
		PrincipalPermissionMode perm_mode =
			PrincipalPermissionMode.UseWindowsGroups; // funky default value
		RoleProvider role_provider;
		ServiceAuthorizationManager svc_auth_manager;
		ReadOnlyCollection<IAuthorizationPolicy> ext_auth_policies;

		public ServiceAuthorizationBehavior ()
		{
		}

		public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies {
			get { return ext_auth_policies; }
			set { ext_auth_policies = value; }
		}

		public bool ImpersonateCallerForAllOperations {
			get { return impersonate; }
			set { impersonate = value; }
		}

		public PrincipalPermissionMode PrincipalPermissionMode {
			get { return perm_mode; }
			set { perm_mode = value; }
		}

		public RoleProvider RoleProvider {
			get { return role_provider; }
			set { role_provider = value; }
		}

		public ServiceAuthorizationManager ServiceAuthorizationManager {
			get { return svc_auth_manager; }
			set { svc_auth_manager = value; }
		}

		void IServiceBehavior.AddBindingParameters (
			ServiceDescription description,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection parameters)
		{
		}

		void IServiceBehavior.ApplyDispatchBehavior (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
			foreach (var cdb in serviceHostBase.ChannelDispatchers) {
				var cd = cdb as ChannelDispatcher;
				if (cd == null) // non-ChannelDispatcher ChannelDispatcherBase instance.
					continue;
				foreach (var ed in cd.Endpoints) {
					var dr = ed.DispatchRuntime;
					dr.ExternalAuthorizationPolicies = ExternalAuthorizationPolicies;
					dr.ImpersonateCallerForAllOperations = ImpersonateCallerForAllOperations;
					dr.PrincipalPermissionMode = PrincipalPermissionMode;
					dr.RoleProvider = RoleProvider;
					dr.ServiceAuthorizationManager = ServiceAuthorizationManager;
				}
			}
		}

		[MonoTODO]
		void IServiceBehavior.Validate (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
		}
	}
}
