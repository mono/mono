//
// System.Net.Configuration.ServicePointManagerElement.cs
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
	public sealed class ServicePointManagerElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty checkCertificateName = new ConfigurationProperty ("CheckCertificateName", typeof (bool), true);
		static ConfigurationProperty checkCertificateRevocationList = new ConfigurationProperty ("CheckCertificateRevocationList", typeof (bool), false);
		static ConfigurationProperty dnsRefreshTimeout = new ConfigurationProperty ("DnsRefreshTimeout", typeof (TimeSpan), new TimeSpan (0, 2, 0));
		static ConfigurationProperty enableDnsRoundRobin = new ConfigurationProperty ("EnableDnsRoundRobin", typeof (bool), false);
		static ConfigurationProperty expect100Continue = new ConfigurationProperty ("Expect100Continue", typeof (bool), true);
		static ConfigurationProperty useNagleAlgorithm = new ConfigurationProperty ("UseNagleAlgorithm", typeof (bool), true);

		#endregion // Fields

		#region Constructors

		public ServicePointManagerElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (checkCertificateName);
			properties.Add (checkCertificateRevocationList);
			properties.Add (dnsRefreshTimeout);
			properties.Add (enableDnsRoundRobin);
			properties.Add (expect100Continue);
			properties.Add (useNagleAlgorithm);
		}

		#endregion // Constructors

		#region Properties

		public bool CheckCertificateName {
			get { return (bool) base [checkCertificateName]; }
			set { base [checkCertificateName] = value; }
		}

		public bool CheckCertificateRevocationList {
			get { return (bool) base [checkCertificateRevocationList]; }
			set { base [checkCertificateRevocationList] = value; }
		}

		public TimeSpan DnsRefreshTimeout {
			get { return (TimeSpan) base [dnsRefreshTimeout]; }
			set { base [dnsRefreshTimeout] = value; }
		}

		public bool EnableDnsRoundRobin {
			get { return (bool) base [enableDnsRoundRobin]; }
			set { base [enableDnsRoundRobin] = value; }
		}

		public bool Expect100Continue {
			get { return (bool) base [expect100Continue]; }
			set { base [expect100Continue] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public bool UseNagleAlgorithm {
			get { return (bool) base [useNagleAlgorithm]; }
			set { base [useNagleAlgorithm] = value; }
		}

		#endregion // Properties
	}
}

#endif
