//
// WSFederationHttpBindingElement.cs
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
	public partial class WSFederationHttpBindingElement
		 : WSHttpBindingBaseElement,  IBindingConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty privacy_notice_at;
		static ConfigurationProperty privacy_notice_version;
		static ConfigurationProperty security;

		static WSFederationHttpBindingElement ()
		{
		}

		static void FillProperties (ConfigurationPropertyCollection baseProps)
		{
			properties = new ConfigurationPropertyCollection ();
			foreach (ConfigurationProperty item in baseProps)
				properties.Add (item);

			privacy_notice_at = new ConfigurationProperty ("privacyNoticeAt",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			privacy_notice_version = new ConfigurationProperty ("privacyNoticeVersion",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			security = new ConfigurationProperty ("security",
				typeof (WSFederationHttpSecurityElement), null, null/* FIXME: get converter for WSFederationHttpSecurityElement*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (privacy_notice_at);
			properties.Add (privacy_notice_version);
			properties.Add (security);
		}

		public WSFederationHttpBindingElement ()
		{
		}


		// Properties

		protected override Type BindingElementType {
			get { return typeof (WSFederationHttpBinding); }
		}

		[ConfigurationProperty ("privacyNoticeAt",
			 DefaultValue = null,
			 Options = ConfigurationPropertyOptions.None)]
		public Uri PrivacyNoticeAt {
			get { return (Uri) base [privacy_notice_at]; }
			set { base [privacy_notice_at] = value; }
		}

		[ConfigurationProperty ("privacyNoticeVersion",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int PrivacyNoticeVersion {
			get { return (int) base [privacy_notice_version]; }
			set { base [privacy_notice_version] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (properties == null) {
					var baseProps = base.Properties;
					lock (baseProps)
						FillProperties (baseProps);
				}
				return properties;
			}
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public WSFederationHttpSecurityElement Security {
			get { return (WSFederationHttpSecurityElement) base [security]; }
		}

		protected override void OnApplyConfiguration (Binding binding)
		{
			base.OnApplyConfiguration (binding);
			var b = (WSFederationHttpBinding) binding;
			b.PrivacyNoticeAt = PrivacyNoticeAt;
			b.PrivacyNoticeVersion = PrivacyNoticeVersion;
			Security.ApplyConfiguration (b.Security);
		}
	}

}
