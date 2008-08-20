//
// ServiceHostingEnvironmentSection.cs
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
	[MonoTODO]
	public sealed partial class ServiceHostingEnvironmentSection
		 : ConfigurationSection
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty asp_net_compatibility_enabled;
		static ConfigurationProperty min_free_memory_percentage_to_activate_service;
		static ConfigurationProperty transport_configuration_types;

		static ServiceHostingEnvironmentSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			asp_net_compatibility_enabled = new ConfigurationProperty ("aspNetCompatibilityEnabled",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			min_free_memory_percentage_to_activate_service = new ConfigurationProperty ("minFreeMemoryPercentageToActivateService",
				typeof (int), "5", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			transport_configuration_types = new ConfigurationProperty ("",
				typeof (TransportConfigurationTypeElementCollection), null, null/* FIXME: get converter for TransportConfigurationTypeElementCollection*/, null,
				ConfigurationPropertyOptions.IsDefaultCollection);

			properties.Add (asp_net_compatibility_enabled);
			properties.Add (min_free_memory_percentage_to_activate_service);
			properties.Add (transport_configuration_types);
		}

		public ServiceHostingEnvironmentSection ()
		{
		}


		// Properties

		[ConfigurationProperty ("aspNetCompatibilityEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool AspNetCompatibilityEnabled {
			get { return (bool) base [asp_net_compatibility_enabled]; }
			set { base [asp_net_compatibility_enabled] = value; }
		}

		[ConfigurationProperty ("minFreeMemoryPercentageToActivateService",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "5")]
		[IntegerValidator ( MinValue = 0,
			 MaxValue = 99,
			ExcludeRange = false)]
		public int MinFreeMemoryPercentageToActivateService {
			get { return (int) base [min_free_memory_percentage_to_activate_service]; }
			set { base [min_free_memory_percentage_to_activate_service] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("",
			 Options = ConfigurationPropertyOptions.IsDefaultCollection,
			IsDefaultCollection = true)]
		public TransportConfigurationTypeElementCollection TransportConfigurationTypes {
			get { return (TransportConfigurationTypeElementCollection) base [transport_configuration_types]; }
		}


	}

}
