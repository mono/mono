//
// SecurityElementBase.cs
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
	public class SecurityElementBase
		 : BindingElementExtensionElement
	{
		ConfigurationPropertyCollection _properties;

		public SecurityElementBase () {
		}


		// Properties

		[ConfigurationProperty ("allowSerializedSigningTokenOnReply",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool AllowSerializedSigningTokenOnReply {
			get { return (bool) base ["allowSerializedSigningTokenOnReply"]; }
			set { base ["allowSerializedSigningTokenOnReply"] = value; }
		}

		[ConfigurationProperty ("authenticationMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "SspiNegotiated")]
		public AuthenticationMode AuthenticationMode {
			get { return (AuthenticationMode) base ["authenticationMode"]; }
			set { base ["authenticationMode"] = value; }
		}

		public override Type BindingElementType {
			get { return typeof (SecurityBindingElement); }
		}

		[ConfigurationProperty ("defaultAlgorithmSuite",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Default")]
		[TypeConverter (typeof (SecurityAlgorithmSuiteConverter))]
		public SecurityAlgorithmSuite DefaultAlgorithmSuite {
			get { return (SecurityAlgorithmSuite) base ["defaultAlgorithmSuite"]; }
			set { base ["defaultAlgorithmSuite"] = value; }
		}

		[ConfigurationProperty ("includeTimestamp",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool IncludeTimestamp {
			get { return (bool) base ["includeTimestamp"]; }
			set { base ["includeTimestamp"] = value; }
		}

		[ConfigurationProperty ("issuedTokenParameters",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenParametersElement IssuedTokenParameters {
			get { return (IssuedTokenParametersElement) base ["issuedTokenParameters"]; }
		}

		[ConfigurationProperty ("keyEntropyMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "CombinedEntropy")]
		public SecurityKeyEntropyMode KeyEntropyMode {
			get { return (SecurityKeyEntropyMode) base ["keyEntropyMode"]; }
			set { base ["keyEntropyMode"] = value; }
		}

		[ConfigurationProperty ("localClientSettings",
			 Options = ConfigurationPropertyOptions.None)]
		public LocalClientSecuritySettingsElement LocalClientSettings {
			get { return (LocalClientSecuritySettingsElement) base ["localClientSettings"]; }
		}

		[ConfigurationProperty ("localServiceSettings",
			 Options = ConfigurationPropertyOptions.None)]
		public LocalServiceSecuritySettingsElement LocalServiceSettings {
			get { return (LocalServiceSecuritySettingsElement) base ["localServiceSettings"]; }
		}

		[ConfigurationProperty ("messageProtectionOrder",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "SignBeforeEncryptAndEncryptSignature")]
		public MessageProtectionOrder MessageProtectionOrder {
			get { return (MessageProtectionOrder) base ["messageProtectionOrder"]; }
			set { base ["messageProtectionOrder"] = value; }
		}

		[ConfigurationProperty ("messageSecurityVersion",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Default")]
		[TypeConverter (typeof (MessageSecurityVersionConverter))]
		public MessageSecurityVersion MessageSecurityVersion {
			get { return (MessageSecurityVersion) base ["messageSecurityVersion"]; }
			set { base ["messageSecurityVersion"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {

					_properties = new ConfigurationPropertyCollection ();
					_properties.Add (new ConfigurationProperty ("allowSerializedSigningTokenOnReply", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("authenticationMode", typeof (AuthenticationMode), "SspiNegotiated", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("defaultAlgorithmSuite", typeof (SecurityAlgorithmSuite), "Default", new SecurityAlgorithmSuiteConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("includeTimestamp", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("issuedTokenParameters", typeof (IssuedTokenParametersElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("keyEntropyMode", typeof (SecurityKeyEntropyMode), "CombinedEntropy", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("localClientSettings", typeof (LocalClientSecuritySettingsElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("localServiceSettings", typeof (LocalServiceSecuritySettingsElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("messageProtectionOrder", typeof (MessageProtectionOrder), "SignBeforeEncryptAndEncryptSignature", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("messageSecurityVersion", typeof (MessageSecurityVersion), "Default", new MessageSecurityVersionConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("requireDerivedKeys", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("requireSecurityContextCancellation", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("requireSignatureConfirmation", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("securityHeaderLayout", typeof (SecurityHeaderLayout), "Strict", null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("requireDerivedKeys",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool RequireDerivedKeys {
			get { return (bool) base ["requireDerivedKeys"]; }
			set { base ["requireDerivedKeys"] = value; }
		}

		[ConfigurationProperty ("requireSecurityContextCancellation",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool RequireSecurityContextCancellation {
			get { return (bool) base ["requireSecurityContextCancellation"]; }
			set { base ["requireSecurityContextCancellation"] = value; }
		}

		[ConfigurationProperty ("requireSignatureConfirmation",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool RequireSignatureConfirmation {
			get { return (bool) base ["requireSignatureConfirmation"]; }
			set { base ["requireSignatureConfirmation"] = value; }
		}

		[ConfigurationProperty ("securityHeaderLayout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Strict")]
		public SecurityHeaderLayout SecurityHeaderLayout {
			get { return (SecurityHeaderLayout) base ["securityHeaderLayout"]; }
			set { base ["securityHeaderLayout"] = value; }
		}


		[MonoTODO]
		protected internal override BindingElement CreateBindingElement () {
			throw new NotImplementedException ();
		}

	}

}
