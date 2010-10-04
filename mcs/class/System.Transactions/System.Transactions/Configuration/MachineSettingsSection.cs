<<<<<<< HEAD
//
// MachineSettingsSection.cs
//
// Author:
//	Pablo Ruiz <pruiz@netway.org>
//
// (C) 2010 Pablo Ruiz.
//

#if NET_2_0
=======
//
// MachineSettingsSection.cs
//
// Author:
//	Pablo Ruiz <pruiz@netway.org>
//
// (C) 2010 Pablo Ruiz.
//

#if NET_2_0 && !MOBILE
>>>>>>> 3d577e4060dccd67d1450b790ef12bc0781198be

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
<<<<<<< HEAD
#endif
=======
#endif
>>>>>>> 3d577e4060dccd67d1450b790ef12bc0781198be
