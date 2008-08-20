//
// IssuedTokenClientElement.cs
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
	public sealed partial class IssuedTokenClientElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty cache_issued_tokens;
		static ConfigurationProperty default_key_entropy_mode;
		static ConfigurationProperty issued_token_renewal_threshold_percentage;
		static ConfigurationProperty issuer_channel_behaviors;
		static ConfigurationProperty local_issuer;
		static ConfigurationProperty local_issuer_channel_behaviors;
		static ConfigurationProperty max_issued_token_caching_time;

		static IssuedTokenClientElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			cache_issued_tokens = new ConfigurationProperty ("cacheIssuedTokens",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			default_key_entropy_mode = new ConfigurationProperty ("defaultKeyEntropyMode",
				typeof (SecurityKeyEntropyMode), "CombinedEntropy", null/* FIXME: get converter for SecurityKeyEntropyMode*/, null,
				ConfigurationPropertyOptions.None);

			issued_token_renewal_threshold_percentage = new ConfigurationProperty ("issuedTokenRenewalThresholdPercentage",
				typeof (int), "60", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			issuer_channel_behaviors = new ConfigurationProperty ("issuerChannelBehaviors",
				typeof (IssuedTokenClientBehaviorsElementCollection), null, null/* FIXME: get converter for IssuedTokenClientBehaviorsElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			local_issuer = new ConfigurationProperty ("localIssuer",
				typeof (IssuedTokenParametersEndpointAddressElement), null, null/* FIXME: get converter for IssuedTokenParametersEndpointAddressElement*/, null,
				ConfigurationPropertyOptions.None);

			local_issuer_channel_behaviors = new ConfigurationProperty ("localIssuerChannelBehaviors",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			max_issued_token_caching_time = new ConfigurationProperty ("maxIssuedTokenCachingTime",
				typeof (TimeSpan), "10675199.02:48:05.4775807", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (cache_issued_tokens);
			properties.Add (default_key_entropy_mode);
			properties.Add (issued_token_renewal_threshold_percentage);
			properties.Add (issuer_channel_behaviors);
			properties.Add (local_issuer);
			properties.Add (local_issuer_channel_behaviors);
			properties.Add (max_issued_token_caching_time);
		}

		public IssuedTokenClientElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("cacheIssuedTokens",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool CacheIssuedTokens {
			get { return (bool) base [cache_issued_tokens]; }
			set { base [cache_issued_tokens] = value; }
		}

		[ConfigurationProperty ("defaultKeyEntropyMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "CombinedEntropy")]
		public SecurityKeyEntropyMode DefaultKeyEntropyMode {
			get { return (SecurityKeyEntropyMode) base [default_key_entropy_mode]; }
			set { base [default_key_entropy_mode] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			 MaxValue = 100,
			ExcludeRange = false)]
		[ConfigurationProperty ("issuedTokenRenewalThresholdPercentage",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "60")]
		public int IssuedTokenRenewalThresholdPercentage {
			get { return (int) base [issued_token_renewal_threshold_percentage]; }
			set { base [issued_token_renewal_threshold_percentage] = value; }
		}

		[ConfigurationProperty ("issuerChannelBehaviors",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenClientBehaviorsElementCollection IssuerChannelBehaviors {
			get { return (IssuedTokenClientBehaviorsElementCollection) base [issuer_channel_behaviors]; }
		}

		[ConfigurationProperty ("localIssuer",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenParametersEndpointAddressElement LocalIssuer {
			get { return (IssuedTokenParametersEndpointAddressElement) base [local_issuer]; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("localIssuerChannelBehaviors",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string LocalIssuerChannelBehaviors {
			get { return (string) base [local_issuer_channel_behaviors]; }
			set { base [local_issuer_channel_behaviors] = value; }
		}

		[ConfigurationProperty ("maxIssuedTokenCachingTime",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan MaxIssuedTokenCachingTime {
			get { return (TimeSpan) base [max_issued_token_caching_time]; }
			set { base [max_issued_token_caching_time] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
