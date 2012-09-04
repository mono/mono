//
// System.Net.Configuration.SettingsSection.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

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

#if CONFIGURATION_DEP

using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class SettingsSection : ConfigurationSection
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty httpWebRequestProp;
		static ConfigurationProperty ipv6Prop;
		static ConfigurationProperty performanceCountersProp;
		static ConfigurationProperty servicePointManagerProp;
		static ConfigurationProperty webProxyScriptProp;
		static ConfigurationProperty socketProp;

		#endregion // Fields

		#region Constructors

		static SettingsSection ()
		{
			httpWebRequestProp = new ConfigurationProperty ("httpWebRequest", typeof (HttpWebRequestElement));
			ipv6Prop = new ConfigurationProperty ("ipv6", typeof (Ipv6Element));
			performanceCountersProp = new ConfigurationProperty ("performanceCounters", typeof (PerformanceCountersElement));
			servicePointManagerProp = new ConfigurationProperty ("servicePointManager", typeof (ServicePointManagerElement));
			socketProp = new ConfigurationProperty ("socket", typeof (SocketElement));
			webProxyScriptProp = new ConfigurationProperty ("webProxyScript", typeof (WebProxyScriptElement));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (httpWebRequestProp);
			properties.Add (ipv6Prop);
			properties.Add (performanceCountersProp);
			properties.Add (servicePointManagerProp);
			properties.Add (socketProp);
			properties.Add (webProxyScriptProp);
		}

		public SettingsSection ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("httpWebRequest")]
		public HttpWebRequestElement HttpWebRequest {
			get { return (HttpWebRequestElement) base [httpWebRequestProp]; }
		}

		[ConfigurationProperty ("ipv6")]
		public Ipv6Element Ipv6 {
			get { return (Ipv6Element) base [ipv6Prop]; }
		}

		[ConfigurationProperty ("performanceCounters")]
		public PerformanceCountersElement PerformanceCounters {
			get { return (PerformanceCountersElement) base[performanceCountersProp]; }
		}

		[ConfigurationProperty ("servicePointManager")]
		public ServicePointManagerElement ServicePointManager {
			get { return (ServicePointManagerElement) base [servicePointManagerProp]; }
		}

		[ConfigurationProperty ("socket")]
		public SocketElement Socket {
			get { return (SocketElement) base [socketProp]; }
		}

		[ConfigurationProperty ("webProxyScript")]
		public WebProxyScriptElement WebProxyScript {
			get { return (WebProxyScriptElement) base [webProxyScriptProp]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties
	}
}

#endif
