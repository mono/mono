//
// NamedPipeTransportElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2010 Novell, Inc.  http://www.novell.com
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
	public sealed partial class NamedPipeTransportElement
		 : ConnectionOrientedTransportElement
	{
		ConfigurationPropertyCollection _properties;

		public NamedPipeTransportElement () {
		}


		// Properties

		public override Type BindingElementType {
			get { return typeof (NamedPipeTransportBindingElement); }
		}

		[ConfigurationProperty ("connectionPoolSettings",
			 Options = ConfigurationPropertyOptions.None)]
		public NamedPipeConnectionPoolSettingsElement ConnectionPoolSettings {
			get { return (NamedPipeConnectionPoolSettingsElement) base ["connectionPoolSettings"]; }
			set { base ["connectionPoolSettings"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("connectionPoolSettings", typeof (NamedPipeConnectionPoolSettingsElement), null, null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		public override void ApplyConfiguration (BindingElement bindingElement)
		{
			var b = (NamedPipeTransportBindingElement) bindingElement;
			base.ApplyConfiguration (b);

			var bs = b.ConnectionPoolSettings;
			var cs = ConnectionPoolSettings;
			bs.GroupName = cs.GroupName;
			bs.IdleTimeout = cs.IdleTimeout;
			bs.MaxOutboundConnectionsPerEndpoint = cs.MaxOutboundConnectionsPerEndpoint;
		}

		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			var e = (NamedPipeTransportElement) from;
			base.CopyFrom (from);

			var es = e.ConnectionPoolSettings;
			var cs = ConnectionPoolSettings;
			cs.GroupName = es.GroupName;
			cs.IdleTimeout = es.IdleTimeout;
			cs.MaxOutboundConnectionsPerEndpoint = es.MaxOutboundConnectionsPerEndpoint;
		}

		protected override TransportBindingElement CreateDefaultBindingElement ()
		{
			return new NamedPipeTransportBindingElement ();
		}

		protected internal override void InitializeFrom (BindingElement bindingElement)
		{
			var b = (NamedPipeTransportBindingElement) bindingElement;
			base.InitializeFrom (b);

			var bs = b.ConnectionPoolSettings;
			var cs = ConnectionPoolSettings;
			cs.GroupName = bs.GroupName;
			cs.IdleTimeout = bs.IdleTimeout;
			cs.MaxOutboundConnectionsPerEndpoint = bs.MaxOutboundConnectionsPerEndpoint;
		}
	}

}
