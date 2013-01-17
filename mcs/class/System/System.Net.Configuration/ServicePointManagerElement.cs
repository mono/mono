//
// System.Net.Configuration.ServicePointManagerElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004,2005 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class ServicePointManagerElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty checkCertificateNameProp;
		static ConfigurationProperty checkCertificateRevocationListProp;
		static ConfigurationProperty dnsRefreshTimeoutProp;
		static ConfigurationProperty enableDnsRoundRobinProp;
		static ConfigurationProperty expect100ContinueProp;
		static ConfigurationProperty useNagleAlgorithmProp;

		#endregion // Fields

		#region Constructors

		static ServicePointManagerElement ()
		{
			checkCertificateNameProp = new ConfigurationProperty ("checkCertificateName", typeof (bool), true);
			checkCertificateRevocationListProp = new ConfigurationProperty ("checkCertificateRevocationList", typeof (bool), false);
			dnsRefreshTimeoutProp = new ConfigurationProperty ("dnsRefreshTimeout", typeof (int), 120000);
			enableDnsRoundRobinProp = new ConfigurationProperty ("enableDnsRoundRobin", typeof (bool), false);
			expect100ContinueProp = new ConfigurationProperty ("expect100Continue", typeof (bool), true);
			useNagleAlgorithmProp = new ConfigurationProperty ("useNagleAlgorithm", typeof (bool), true);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (checkCertificateNameProp);
			properties.Add (checkCertificateRevocationListProp);
			properties.Add (dnsRefreshTimeoutProp);
			properties.Add (enableDnsRoundRobinProp);
			properties.Add (expect100ContinueProp);
			properties.Add (useNagleAlgorithmProp);
		}

		public ServicePointManagerElement ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("checkCertificateName", DefaultValue = "True")]
		public bool CheckCertificateName {
			get { return (bool) base [checkCertificateNameProp]; }
			set { base [checkCertificateNameProp] = value; }
		}

		[ConfigurationProperty ("checkCertificateRevocationList", DefaultValue = "False")]
		public bool CheckCertificateRevocationList {
			get { return (bool) base [checkCertificateRevocationListProp]; }
			set { base [checkCertificateRevocationListProp] = value; }
		}

		[ConfigurationProperty ("dnsRefreshTimeout", DefaultValue = "120000")]
		public int DnsRefreshTimeout {
			get { return (int) base [dnsRefreshTimeoutProp]; }
			set { base [dnsRefreshTimeoutProp] = value; }
		}

		[ConfigurationProperty ("enableDnsRoundRobin", DefaultValue = "False")]
		public bool EnableDnsRoundRobin {
			get { return (bool) base [enableDnsRoundRobinProp]; }
			set { base [enableDnsRoundRobinProp] = value; }
		}

		[ConfigurationProperty ("expect100Continue", DefaultValue = "True")]
		public bool Expect100Continue {
			get { return (bool) base [expect100ContinueProp]; }
			set { base [expect100ContinueProp] = value; }
		}

		[ConfigurationProperty ("useNagleAlgorithm", DefaultValue = "True")]
		public bool UseNagleAlgorithm {
			get { return (bool) base [useNagleAlgorithmProp]; }
			set { base [useNagleAlgorithmProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void PostDeserialize ()
		{
		}

		#endregion // Methods

	}
}

#endif
