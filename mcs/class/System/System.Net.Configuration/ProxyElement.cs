//
// System.Net.Configuration.ProxyElement.cs
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

#if NET_2_0 && XML_DEP

using System;
using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class ProxyElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty bypassOnLocal = new ConfigurationProperty ("BypassOnLocal", typeof (BypassOnLocalValues), BypassOnLocalValues.Default);
		static ConfigurationProperty proxyAddress = new ConfigurationProperty ("ProxyAddress", typeof (string), null);
		static ConfigurationProperty scriptDownloadInterval = new ConfigurationProperty ("ScriptDownloadInterval", typeof (TimeSpan), TimeSpan.MinValue);
		static ConfigurationProperty scriptDownloadTimeout = new ConfigurationProperty ("ScriptDownloadTimeout", typeof (TimeSpan), TimeSpan.MinValue);
		static ConfigurationProperty useDefaultCredentials = new ConfigurationProperty ("UseDefaultCredentials", typeof (bool), false);
		static ConfigurationProperty useDefaultCredentialsForScriptDownload = new ConfigurationProperty ("UseDefaultCredentialsForScriptDownload", typeof (bool), false);
		static ConfigurationProperty useSystemDefault = new ConfigurationProperty ("UseSystemDefault", typeof (bool), true);

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ProxyElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (bypassOnLocal);
			properties.Add (proxyAddress);
			properties.Add (scriptDownloadInterval);
			properties.Add (scriptDownloadTimeout);
			properties.Add (useDefaultCredentials);
			properties.Add (useDefaultCredentialsForScriptDownload);
			properties.Add (useSystemDefault);
		}

		#endregion // Constructors

		#region Properties

		public BypassOnLocalValues BypassOnLocal {
			get { return (BypassOnLocalValues) base [bypassOnLocal]; }
			set { base [bypassOnLocal] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public string ProxyAddress {
			get { return (string) base [proxyAddress]; }
			set { base [proxyAddress] = value; }
		}

		public TimeSpan ScriptDownloadInterval {
			get { return (TimeSpan) base [scriptDownloadInterval]; }
			set { base [scriptDownloadInterval] = value; }
		}

		public TimeSpan ScriptDownloadTimeout {
			get { return (TimeSpan) base [scriptDownloadTimeout]; }
			set { base [scriptDownloadTimeout] = value; }
		}

		public bool UseDefaultCredentials {
			get { return (bool) base [useDefaultCredentials]; }
			set { base [useDefaultCredentials] = value; }
		}

		public bool UseDefaultCredentialsForScriptDownload {
			get { return (bool) base [useDefaultCredentialsForScriptDownload]; }
			set { base [useDefaultCredentialsForScriptDownload] = value; }
		}

		public bool UseSystemDefault {
			get { return (bool) base [useSystemDefault]; }
			set { base [useSystemDefault] = value; }
		}

		#endregion // Properties

		public enum BypassOnLocalValues
		{
			Default = -1,
			True = 1,
			False = 0
		}
	}
}

#endif
