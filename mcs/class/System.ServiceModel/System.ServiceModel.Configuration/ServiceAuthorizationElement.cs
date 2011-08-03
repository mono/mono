//
// ServiceAuthorizationElement.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public sealed class ServiceAuthorizationElement
		 : BehaviorExtensionElement
	{
		public ServiceAuthorizationElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("authorizationPolicies",
			 Options = ConfigurationPropertyOptions.None)]
		public AuthorizationPolicyTypeElementCollection AuthorizationPolicies {
			get { return (AuthorizationPolicyTypeElementCollection) base ["authorizationPolicies"]; }
		}

		public override Type BehaviorType {
			get { return typeof(ServiceAuthorizationBehavior); }
		}

		[ConfigurationProperty ("impersonateCallerForAllOperations",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool ImpersonateCallerForAllOperations {
			get { return (bool) base ["impersonateCallerForAllOperations"]; }
			set { base ["impersonateCallerForAllOperations"] = value; }
		}

		[ConfigurationProperty ("principalPermissionMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "UseWindowsGroups")]
		public PrincipalPermissionMode PrincipalPermissionMode {
			get { return (PrincipalPermissionMode) base ["principalPermissionMode"]; }
			set { base ["principalPermissionMode"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("roleProviderName",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string RoleProviderName {
			get { return (string) base ["roleProviderName"]; }
			set { base ["roleProviderName"] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("serviceAuthorizationManagerType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string ServiceAuthorizationManagerType {
			get { return (string) base ["serviceAuthorizationManagerType"]; }
			set { base ["serviceAuthorizationManagerType"] = value; }
		}

		[MonoTODO]
		protected internal override object CreateBehavior ()
		{
			var b = new ServiceAuthorizationBehavior ();
			if (!String.IsNullOrEmpty (ServiceAuthorizationManagerType))
				b.ServiceAuthorizationManager = (ServiceAuthorizationManager) Activator.CreateInstance (ConfigUtil.GetTypeFromConfigString (ServiceAuthorizationManagerType, NamedConfigCategory.None));

			foreach (var apte in AuthorizationPolicies)
				throw new NotImplementedException ();

			if (!String.IsNullOrEmpty (RoleProviderName))
				throw new NotImplementedException ();

			b.ImpersonateCallerForAllOperations = ImpersonateCallerForAllOperations;
			b.PrincipalPermissionMode = PrincipalPermissionMode;

			return b;
		}
		
		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			var e = (ServiceAuthorizationElement) from;
			foreach (AuthorizationPolicyTypeElement ae in e.AuthorizationPolicies)
				AuthorizationPolicies.Add (new AuthorizationPolicyTypeElement (ae.PolicyType));
			ImpersonateCallerForAllOperations = e.ImpersonateCallerForAllOperations;
			PrincipalPermissionMode = e.PrincipalPermissionMode;
			RoleProviderName = e.RoleProviderName;
			ServiceAuthorizationManagerType = e.ServiceAuthorizationManagerType;
		}
	}

}
