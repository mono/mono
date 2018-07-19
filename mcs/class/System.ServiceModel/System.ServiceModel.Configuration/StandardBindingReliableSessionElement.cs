//
// StandardBindingReliableSessionElement.cs
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
	public partial class StandardBindingReliableSessionElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty inactivity_timeout;
		static ConfigurationProperty ordered;

		internal static ConfigurationPropertyCollection CreateBaseProperties () {
			ConfigurationPropertyCollection baseProperties = new ConfigurationPropertyCollection ();
			foreach (ConfigurationProperty prop in properties)
				baseProperties.Add (prop);
			return baseProperties;
		}

		static StandardBindingReliableSessionElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			inactivity_timeout = new ConfigurationProperty ("inactivityTimeout",
				typeof (TimeSpan), "00:10:00", new TimeSpanConverter (), null,
				ConfigurationPropertyOptions.None);

			ordered = new ConfigurationProperty ("ordered",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (inactivity_timeout);
			properties.Add (ordered);
		}

		public StandardBindingReliableSessionElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("inactivityTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:10:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan InactivityTimeout {
			get { return (TimeSpan) base [inactivity_timeout]; }
			set { base [inactivity_timeout] = value; }
		}

		[ConfigurationProperty ("ordered",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool Ordered {
			get { return (bool) base [ordered]; }
			set { base [ordered] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		// Methods

		public void ApplyConfiguration (ReliableSession reliableSession)
		{
			reliableSession.InactivityTimeout = InactivityTimeout;
			reliableSession.Ordered = Ordered;
		}
	}

}
