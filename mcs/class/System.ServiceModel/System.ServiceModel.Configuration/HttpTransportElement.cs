//
// HttpTransportElement.cs
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
#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
using System.Security.Authentication.ExtendedProtection.Configuration;
#endif
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
	public class HttpTransportElement
		 : TransportElement
	{
		ConfigurationPropertyCollection _properties;

		public HttpTransportElement () {
		}


		// Properties

		[ConfigurationProperty ("allowCookies",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool AllowCookies {
			get { return (bool) base ["allowCookies"]; }
			set { base ["allowCookies"] = value; }
		}

		[ConfigurationProperty ("authenticationScheme",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Anonymous")]
		public AuthenticationSchemes AuthenticationScheme {
			get { return (AuthenticationSchemes) base ["authenticationScheme"]; }
			set { base ["authenticationScheme"] = value; }
		}

		public override Type BindingElementType {
			get { return typeof (HttpTransportBindingElement); }
		}

		[ConfigurationProperty ("bypassProxyOnLocal",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool BypassProxyOnLocal {
			get { return (bool) base ["bypassProxyOnLocal"]; }
			set { base ["bypassProxyOnLocal"] = value; }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "StrongWildcard")]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) base ["hostNameComparisonMode"]; }
			set { base ["hostNameComparisonMode"] = value; }
		}

		[ConfigurationProperty ("keepAliveEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool KeepAliveEnabled {
			get { return (bool) base ["keepAliveEnabled"]; }
			set { base ["keepAliveEnabled"] = value; }
		}

		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		public int MaxBufferSize {
			get { return (int) base ["maxBufferSize"]; }
			set { base ["maxBufferSize"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					foreach (ConfigurationProperty cp in base.Properties)
						_properties.Add (cp);
					_properties.Add (new ConfigurationProperty ("allowCookies", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("authenticationScheme", typeof (AuthenticationSchemes), "Anonymous", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("bypassProxyOnLocal", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("keepAliveEnabled", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferSize", typeof (int), "65536", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("proxyAddress", typeof (Uri), null, new UriTypeConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("proxyAuthenticationScheme", typeof (AuthenticationSchemes), "Anonymous", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("realm", typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transferMode", typeof (TransferMode), "Buffered", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("unsafeConnectionNtlmAuthentication", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("useDefaultWebProxy", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
#if NET_4_0
					_properties.Add (new ConfigurationProperty ("decompressionEnabled", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("extendedProtectionPolicy", typeof (ExtendedProtectionPolicyElement), null, new ExtendedProtectionPolicyTypeConverter (), null, ConfigurationPropertyOptions.None));
#endif
				}
				return _properties;
			}
		}

#if NET_4_0
		[ConfigurationProperty ("decompressionEnabled",
			 Options = ConfigurationPropertyOptions.None)]
		public bool DecompressionEnabled {
			get { return (bool) base ["decompressionEnabled"]; }
			set { base ["decompressionEnabled"] = value; }
		}

		[ConfigurationProperty ("extendedProtectionPolicy",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtendedProtectionPolicyElement ExtendedProtectionPolicy {
			get { return (ExtendedProtectionPolicyElement) base ["extendedProtectionPolicy"]; }
			set { base ["extendedProtectionPolicy"] = value; }
		}
#endif

		[ConfigurationProperty ("proxyAddress",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public Uri ProxyAddress {
			get { return (Uri) base ["proxyAddress"]; }
			set { base ["proxyAddress"] = value; }
		}

		[ConfigurationProperty ("proxyAuthenticationScheme",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Anonymous")]
		public AuthenticationSchemes ProxyAuthenticationScheme {
			get { return (AuthenticationSchemes) base ["proxyAuthenticationScheme"]; }
			set { base ["proxyAuthenticationScheme"] = value; }
		}

		[ConfigurationProperty ("realm",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		[StringValidator (MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string Realm {
			get { return (string) base ["realm"]; }
			set { base ["realm"] = value; }
		}

		[ConfigurationProperty ("transferMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Buffered")]
		public TransferMode TransferMode {
			get { return (TransferMode) base ["transferMode"]; }
			set { base ["transferMode"] = value; }
		}

		[ConfigurationProperty ("unsafeConnectionNtlmAuthentication",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UnsafeConnectionNtlmAuthentication {
			get { return (bool) base ["unsafeConnectionNtlmAuthentication"]; }
			set { base ["unsafeConnectionNtlmAuthentication"] = value; }
		}

		[ConfigurationProperty ("useDefaultWebProxy",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool UseDefaultWebProxy {
			get { return (bool) base ["useDefaultWebProxy"]; }
			set { base ["useDefaultWebProxy"] = value; }
		}

		public override void ApplyConfiguration (BindingElement bindingElement)
		{
			var b = (HttpTransportBindingElement) bindingElement;
			base.ApplyConfiguration (b);
			b.AllowCookies = AllowCookies;
			b.AuthenticationScheme = AuthenticationScheme;
			b.BypassProxyOnLocal = BypassProxyOnLocal;
			b.HostNameComparisonMode = HostNameComparisonMode;
			b.KeepAliveEnabled = KeepAliveEnabled;
			b.MaxBufferSize = MaxBufferSize;
			b.ProxyAddress = ProxyAddress;
			b.ProxyAuthenticationScheme = ProxyAuthenticationScheme;
			b.Realm = Realm;
			b.TransferMode = TransferMode;
			b.UnsafeConnectionNtlmAuthentication = UnsafeConnectionNtlmAuthentication;
			b.UseDefaultWebProxy = UseDefaultWebProxy;
#if NET_4_0
			b.DecompressionEnabled = DecompressionEnabled;
			// FIXME: enable this.
			//b.ExtendedProtectionPolicy = ExtendedProtectionPolicy.BuildPolicy ();
#endif
		}

		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			var e = (HttpTransportElement) from;
			base.CopyFrom (from);
			AllowCookies = e.AllowCookies;
			AuthenticationScheme = e.AuthenticationScheme;
			BypassProxyOnLocal = e.BypassProxyOnLocal;
			HostNameComparisonMode = e.HostNameComparisonMode;
			KeepAliveEnabled = e.KeepAliveEnabled;
			MaxBufferSize = e.MaxBufferSize;
			ProxyAddress = e.ProxyAddress;
			ProxyAuthenticationScheme = e.ProxyAuthenticationScheme;
			Realm = e.Realm;
			TransferMode = e.TransferMode;
			UnsafeConnectionNtlmAuthentication = e.UnsafeConnectionNtlmAuthentication;
			UseDefaultWebProxy = e.UseDefaultWebProxy;
#if NET_4_0
			DecompressionEnabled = e.DecompressionEnabled;
			// FIXME: enable this.
			/*
			ExtendedProtectionPolicy = new ExtendedProtectionPolicyElement () { PolicyEnforcement = e.ExtendedProtectionPolicy.PolicyEnforcement, ProtectionScenario = e.ExtendedProtectionPolicy.ProtectionScenario };
			foreach (var sne in ExtendedProtectionPolicy.CustomServiceNames)
				ExtendedProtectionPolicy.CustomServiceNames.Add (sne);
			*/
#endif
		}

		protected override TransportBindingElement CreateDefaultBindingElement ()
		{
			return new HttpTransportBindingElement ();
		}

		protected internal override void InitializeFrom (BindingElement bindingElement)
		{
			var b = (HttpTransportBindingElement) bindingElement;
			base.InitializeFrom (b);
			AllowCookies = b.AllowCookies;
			AuthenticationScheme = b.AuthenticationScheme;
			BypassProxyOnLocal = b.BypassProxyOnLocal;
			HostNameComparisonMode = b.HostNameComparisonMode;
			KeepAliveEnabled = b.KeepAliveEnabled;
			MaxBufferSize = b.MaxBufferSize;
			ProxyAddress = b.ProxyAddress;
			ProxyAuthenticationScheme = b.ProxyAuthenticationScheme;
			Realm = b.Realm;
			TransferMode = b.TransferMode;
			UnsafeConnectionNtlmAuthentication = b.UnsafeConnectionNtlmAuthentication;
			UseDefaultWebProxy = b.UseDefaultWebProxy;
#if NET_4_0
			DecompressionEnabled = b.DecompressionEnabled;
			// FIXME: enable this.
			/*
			ExtendedProtectionPolicy = new ExtendedProtectionPolicyElement () { PolicyEnforcement = b.ExtendedProtectionPolicy.PolicyEnforcement, ProtectionScenario = b.ExtendedProtectionPolicy.ProtectionScenario };
			foreach (var sn in b.ExtendedProtectionPolicy.CustomServiceNames)
				ExtendedProtectionPolicy.CustomServiceNames.Add (new ServiceNameElement () { Name = sn.ToString () });
			*/
#endif
		}
	}
}
