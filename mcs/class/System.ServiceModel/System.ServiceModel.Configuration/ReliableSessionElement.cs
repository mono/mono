//
// ReliableSessionElement.cs
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
	public sealed class ReliableSessionElement
		 : BindingElementExtensionElement
	{
		public ReliableSessionElement () {
		}

		// Properties

		[ConfigurationProperty ("acknowledgementInterval",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:00:00.2")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan AcknowledgementInterval {
			get { return (TimeSpan) base ["acknowledgementInterval"]; }
			set { base ["acknowledgementInterval"] = value; }
		}

		public override Type BindingElementType {
			get { return typeof (ReliableSessionBindingElement); }
		}

		[ConfigurationProperty ("flowControlEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool FlowControlEnabled {
			get { return (bool) base ["flowControlEnabled"]; }
			set { base ["flowControlEnabled"] = value; }
		}

		[ConfigurationProperty ("inactivityTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:10:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan InactivityTimeout {
			get { return (TimeSpan) base ["inactivityTimeout"]; }
			set { base ["inactivityTimeout"] = value; }
		}

		[ConfigurationProperty ("maxPendingChannels",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "4")]
		[IntegerValidator (MinValue = 1,
			 MaxValue = 16384,
			ExcludeRange = false)]
		public int MaxPendingChannels {
			get { return (int) base ["maxPendingChannels"]; }
			set { base ["maxPendingChannels"] = value; }
		}

		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxRetryCount",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "8")]
		public int MaxRetryCount {
			get { return (int) base ["maxRetryCount"]; }
			set { base ["maxRetryCount"] = value; }
		}

		[ConfigurationProperty ("maxTransferWindowSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "8")]
		[IntegerValidator (MinValue = 1,
			 MaxValue = 4096,
			ExcludeRange = false)]
		public int MaxTransferWindowSize {
			get { return (int) this ["maxTransferWindowSize"]; }
			set { this ["maxTransferWindowSize"] = value; }
		}

		[ConfigurationProperty ("ordered",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool Ordered {
			get { return (bool) this ["ordered"]; }
			set { this ["ordered"] = value; }
		}

		[ConfigurationPropertyAttribute ("reliableMessagingVersion",
			DefaultValue = "WSReliableMessagingFebruary2005")]
		[TypeConverter (typeof (ReliableMessagingVersionConverter))]
		public ReliableMessagingVersion ReliableMessagingVersion {
			get { return (ReliableMessagingVersion) this ["reliableMessagingVersion"]; }
			set { this ["reliableMessagingVersion"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}

		protected internal override BindingElement CreateBindingElement ()
		{
			return new ReliableSessionBindingElement ();
		}

		public override void ApplyConfiguration (BindingElement bindingElement)
		{
			var b = (ReliableSessionBindingElement) bindingElement;
			b.AcknowledgementInterval = AcknowledgementInterval;
			b.FlowControlEnabled = FlowControlEnabled;
			b.InactivityTimeout = InactivityTimeout;
			b.MaxPendingChannels = MaxPendingChannels;
			b.MaxRetryCount = MaxRetryCount;
			b.MaxTransferWindowSize = MaxTransferWindowSize;
			b.Ordered = Ordered;
			b.ReliableMessagingVersion = ReliableMessagingVersion;
		}

		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			var b = (ReliableSessionElement) from;
			AcknowledgementInterval = b.AcknowledgementInterval;
			FlowControlEnabled = b.FlowControlEnabled;
			InactivityTimeout = b.InactivityTimeout;
			MaxPendingChannels = b.MaxPendingChannels;
			MaxRetryCount = b.MaxRetryCount;
			MaxTransferWindowSize = b.MaxTransferWindowSize;
			Ordered = b.Ordered;
			ReliableMessagingVersion = b.ReliableMessagingVersion;
		}

		protected internal override void InitializeFrom (BindingElement bindingElement)
		{
			var b = (ReliableSessionBindingElement) bindingElement;
			AcknowledgementInterval = b.AcknowledgementInterval;
			FlowControlEnabled = b.FlowControlEnabled;
			InactivityTimeout = b.InactivityTimeout;
			MaxPendingChannels = b.MaxPendingChannels;
			MaxRetryCount = b.MaxRetryCount;
			MaxTransferWindowSize = b.MaxTransferWindowSize;
			Ordered = b.Ordered;
			ReliableMessagingVersion = b.ReliableMessagingVersion;
		}

	}

}
