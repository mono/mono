//
// ExtendedProtectionPolicyElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0 && CONFIGURATION_DEP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.Security.Authentication.ExtendedProtection.Configuration
{
	[MonoTODO]
	public sealed class ExtendedProtectionPolicyElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty custom_service_names, policy_enforcement, protection_scenario;

		static ExtendedProtectionPolicyElement ()
		{
			properties = new ConfigurationPropertyCollection ();

			var t = typeof (ExtendedProtectionPolicyElement);
			custom_service_names = ConfigUtil.BuildProperty (t, "CustomServiceNames");
			policy_enforcement = ConfigUtil.BuildProperty (t, "PolicyEnforcement");
			protection_scenario = ConfigUtil.BuildProperty (t, "ProtectionScenario");

			foreach (var cp in new ConfigurationProperty [] {custom_service_names, policy_enforcement, protection_scenario})
				properties.Add (cp);
		}
		
		[ConfigurationProperty ("customServiceNames")]
		public ServiceNameElementCollection CustomServiceNames {
			get { return (ServiceNameElementCollection) this [custom_service_names]; }
		}

		[ConfigurationProperty ("policyEnforcement")]
		public PolicyEnforcement PolicyEnforcement {
			get { return (PolicyEnforcement) this [policy_enforcement]; }
			set { this [policy_enforcement] = value; }
		}

		[ConfigurationProperty ("protectionScenario", DefaultValue = ProtectionScenario.TransportSelected)]
		public ProtectionScenario ProtectionScenario {
			get { return (ProtectionScenario) this [protection_scenario]; }
			set { this [protection_scenario] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public ExtendedProtectionPolicy BuildPolicy ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
