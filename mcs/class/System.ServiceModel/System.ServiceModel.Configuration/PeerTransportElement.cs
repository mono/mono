//
// PeerTransportElement.cs
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
	public class PeerTransportElement
		 : BindingElementExtensionElement
	{
		ConfigurationPropertyCollection _properties;

		public PeerTransportElement () {
		}


		// Properties

		public override Type BindingElementType {
			get { return typeof (PeerTransportBindingElement); }
		}

		[TypeConverter (typeof (IPAddressConverter))]
		[ConfigurationProperty ("listenIPAddress",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public IPAddress ListenIPAddress {
			get { return (IPAddress) base ["listenIPAddress"]; }
			set { base ["listenIPAddress"] = value; }
		}

		[LongValidator (MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "524288")]
		public long MaxBufferPoolSize {
			get { return (long) base ["maxBufferPoolSize"]; }
			set { base ["maxBufferPoolSize"] = value; }
		}

		[LongValidator (MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		public long MaxReceivedMessageSize {
			get { return (long) base ["maxReceivedMessageSize"]; }
			set { base ["maxReceivedMessageSize"] = value; }
		}

		[ConfigurationProperty ("port",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "0")]
		[IntegerValidator (MinValue = 0,
			 MaxValue = 65535,
			ExcludeRange = false)]
		public int Port {
			get { return (int) base ["port"]; }
			set { base ["port"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("listenIPAddress", typeof (IPAddress), null, new IPAddressConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, new LongValidator (1, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, new LongValidator (1, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("port", typeof (int), "0", null, new IntegerValidator (0, 65535, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("security", typeof (PeerSecurityElement), null, null/* FIXME: get converter for PeerSecurityElement*/, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public PeerSecurityElement Security {
			get { return (PeerSecurityElement) base ["security"]; }
		}

		[MonoTODO]
		protected internal override BindingElement CreateBindingElement () {
			throw new NotImplementedException ();
		}

	}

}
