//
// BasicHttpsBindingElement.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
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
	public class BasicHttpsBindingElement
		 : HttpBindingBaseElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public BasicHttpsBindingElement ()
		{
		}

		public BasicHttpsBindingElement (string name) : base (name) { }

		protected override Type BindingElementType {
			get { return typeof (BasicHttpsBinding); }
		}
		
		// Properties

		[ConfigurationProperty ("messageEncoding",
			 DefaultValue = "Text",
			 Options = ConfigurationPropertyOptions.None)]
		public WSMessageEncoding MessageEncoding {
			get { return (WSMessageEncoding) this ["messageEncoding"]; }
			set { this ["messageEncoding"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("messageEncoding", typeof (WSMessageEncoding), "Text", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("security", typeof (BasicHttpsSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public BasicHttpsSecurityElement Security {
			get { return (BasicHttpsSecurityElement) this ["security"]; }
		}

		protected override void OnApplyConfiguration (Binding binding)
		{
			base.OnApplyConfiguration (binding);
			BasicHttpsBinding basicHttpsBinding = (BasicHttpsBinding) binding;
			
			basicHttpsBinding.MessageEncoding = MessageEncoding;

			basicHttpsBinding.Security.Mode = Security.Mode;
			Security.Transport.ApplyConfiguration (basicHttpsBinding.Security.Transport);
		}

		protected internal override void InitializeFrom (Binding binding)
		{
			BasicHttpsBinding b = (BasicHttpsBinding) binding;
			base.InitializeFrom (binding);

			MessageEncoding = b.MessageEncoding;

			Security.Mode = b.Security.Mode;
			Security.Transport.ApplyConfiguration (b.Security.Transport);
		}
	}
}
