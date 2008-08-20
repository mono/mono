//
// TcpTransportElement.cs
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
	public sealed class TcpTransportElement
		 : ConnectionOrientedTransportElement
	{
		ConfigurationPropertyCollection _properties;

		public TcpTransportElement () {
		}


		// Properties

		public override Type BindingElementType {
			get { return typeof (TcpTransportBindingElement); }
		}

		[ConfigurationProperty ("connectionPoolSettings",
			 Options = ConfigurationPropertyOptions.None)]
		public TcpConnectionPoolSettingsElement ConnectionPoolSettings {
			get { return (TcpConnectionPoolSettingsElement) base ["connectionPoolSettings"]; }
			set { base ["connectionPoolSettings"] = value; }
		}

		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("listenBacklog",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10")]
		public int ListenBacklog {
			get { return (int) base ["listenBacklog"]; }
			set { base ["listenBacklog"] = value; }
		}

		[ConfigurationProperty ("portSharingEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool PortSharingEnabled {
			get { return (bool) base ["portSharingEnabled"]; }
			set { base ["portSharingEnabled"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("connectionPoolSettings", typeof (TcpConnectionPoolSettingsElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("listenBacklog", typeof (int), "10", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("portSharingEnabled", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("teredoEnabled", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("teredoEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool TeredoEnabled {
			get { return (bool) base ["teredoEnabled"]; }
			set { base ["teredoEnabled"] = value; }
		}


		[MonoTODO]
		protected internal override BindingElement CreateBindingElement () {
			throw new NotImplementedException ();
		}

	}

}
