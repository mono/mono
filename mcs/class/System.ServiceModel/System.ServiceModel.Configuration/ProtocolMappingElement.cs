//
// ProtocolMappingElement.cs
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

#if NET_4_0
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
	public sealed class ProtocolMappingElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty binding, binding_configuration, scheme;

		static ProtocolMappingElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			binding = new ConfigurationProperty ("binding", typeof (string), null, null, new StringValidator (0), ConfigurationPropertyOptions.IsRequired);
			binding_configuration = new ConfigurationProperty ("bindingConfiguration", typeof (string), null, null, new StringValidator (0), ConfigurationPropertyOptions.None);
			scheme = new ConfigurationProperty ("scheme", typeof (string), null, null, new StringValidator (0), ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			foreach (var item in new ConfigurationProperty [] {binding, binding_configuration, scheme})
				properties.Add (item);
		}
		
		public ProtocolMappingElement ()
		{
		}
		
		public ProtocolMappingElement (string schemeType, string binding, string bindingConfiguration)
		{
			Binding = binding;
			BindingConfiguration = bindingConfiguration;
			Scheme = schemeType;
		}

		[ConfigurationProperty ("binding", Options = ConfigurationPropertyOptions.IsRequired)]
		[StringValidator (MinLength = 0)]
		public string Binding {
			get { return (string) base [binding]; }
			set { base [binding] = value; }
		}

		[StringValidator (MinLength = 0)]
		[ConfigurationProperty ("bindingConfiguration", Options = ConfigurationPropertyOptions.None)]
		public string BindingConfiguration {
			get { return (string) base [binding_configuration]; }
			set { base [binding_configuration] = value; }
		}

		[StringValidator (MinLength = 0)]
		[ConfigurationProperty ("scheme", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Scheme {
			get { return (string) base [scheme]; }
			set { base [scheme] = value; }
		}
	}
}

#endif
