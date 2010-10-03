using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace System.Transactions.Configuration
{
	public class MachineSettingsSection : ConfigurationSection
	{
		// http://msdn.microsoft.com/en-us/library/system.transactions.configuration.machinesettingssection.maxtimeout.aspx
		[ConfigurationProperty("maxTimeout", DefaultValue = "00:10:00")]
		[TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		public TimeSpan MaxTimeout {
			get { return (TimeSpan)base["maxTimeout"]; }
			set { 
				// FIXME: Validate timespan value..
				base["maxTimeout"] = value;
			}
		}
	}
}
