//
// DefaultSettingsSection.cs
//
// Author:
//	Pablo Ruiz <pruiz@netway.org>
//
// (C) 2010 Pablo Ruiz.
//

#if NET_2_0 && !MOBILE

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace System.Transactions.Configuration
{
	public class DefaultSettingsSection : ConfigurationSection
	{
		// http://msdn.microsoft.com/en-us/library/system.transactions.configuration.defaultsettingssection.timeout.aspx
		[ConfigurationProperty ("timeout", DefaultValue = "00:01:00")]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		public TimeSpan Timeout {
			get { return (TimeSpan)base["timeout"]; }
			set {  
				// FIXME: Validate timespan value
				base["timeout"] = value;
			} 
		}

		// http://msdn.microsoft.com/en-us/library/system.transactions.configuration.defaultsettingssection.distributedtransactionmanagername(v=VS.90).aspx
		[ConfigurationProperty ("distributedTransactionManagerName", DefaultValue = "")]
		public string DistributedTransactionManagerName {
			get { return base["distributedTransactionManagerName"] as string; }
			set { base["distributedTransactionManagerName"] = value; }
		}
	}
}
#endif
