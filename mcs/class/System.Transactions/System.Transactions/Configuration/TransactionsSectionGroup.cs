//
// TransactionSectionGroup.cs
//
// Author:
//	Pablo Ruiz <pruiz@netway.org>
//
// (C) 2010 Pablo Ruiz.
//

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace System.Transactions.Configuration
{
	// http://msdn.microsoft.com/en-us/library/system.transactions.configuration.transactionssectiongroup.aspx
	public class TransactionsSectionGroup : ConfigurationSectionGroup
	{
		public static TransactionsSectionGroup GetSectionGroup(System.Configuration.Configuration config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			return config.GetSectionGroup("system.transactions") as TransactionsSectionGroup;
		}

		[ConfigurationProperty("defaultSettings")]
		public DefaultSettingsSection DefaultSettings
		{
			get { return (DefaultSettingsSection)base.Sections["defaultSettings"]; }
		}

		[ConfigurationProperty("machineSettings")]
		public MachineSettingsSection MachineSettings
		{
			get { return (MachineSettingsSection)base.Sections["machineSettings"]; }
		}
	}
}
#endif