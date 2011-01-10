//
// System.Web.Configuration.HealthMonitoringSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class HealthMonitoringSection : ConfigurationSection
	{
		static ConfigurationProperty bufferModesProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty eventMappingsProp;
		static ConfigurationProperty heartbeatIntervalProp;
		static ConfigurationProperty profilesProp;
		static ConfigurationProperty providersProp;
		static ConfigurationProperty rulesProp;
		static ConfigurationPropertyCollection properties;

		static HealthMonitoringSection ()
		{
			bufferModesProp = new ConfigurationProperty ("bufferModes", typeof (BufferModesCollection), null,
								     null, PropertyHelper.DefaultValidator,
								     ConfigurationPropertyOptions.None);
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), true);
			eventMappingsProp = new ConfigurationProperty ("eventMappings", typeof (EventMappingSettingsCollection), null,
								       null, PropertyHelper.DefaultValidator,
								       ConfigurationPropertyOptions.None);
			heartbeatIntervalProp = new ConfigurationProperty ("heartbeatInterval", typeof (TimeSpan), TimeSpan.FromSeconds (0),
									   PropertyHelper.TimeSpanSecondsConverter,
									   new TimeSpanValidator (TimeSpan.Zero, new TimeSpan (24,30,31,23)),
									   ConfigurationPropertyOptions.None);
			profilesProp = new ConfigurationProperty ("profiles", typeof (ProfileSettingsCollection), null,
								  null, PropertyHelper.DefaultValidator,
								  ConfigurationPropertyOptions.None);
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection), null,
								   null, PropertyHelper.DefaultValidator,
								   ConfigurationPropertyOptions.None);
			rulesProp = new ConfigurationProperty ("rules", typeof (RuleSettingsCollection), null,
							       null, PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (bufferModesProp);
			properties.Add (enabledProp);
			properties.Add (eventMappingsProp);
			properties.Add (heartbeatIntervalProp);
			properties.Add (profilesProp);
			properties.Add (providersProp);
			properties.Add (rulesProp);
		}

		[ConfigurationProperty ("bufferModes")]
		public BufferModesCollection BufferModes {
			get { return (BufferModesCollection) base [bufferModesProp];}
		}

		[ConfigurationProperty ("enabled", DefaultValue = "True")]
		public bool Enabled {
			get { return (bool) base [enabledProp];}
			set { base[enabledProp] = value; }
		}

		[ConfigurationProperty ("eventMappings")]
		public EventMappingSettingsCollection EventMappings {
			get { return (EventMappingSettingsCollection) base [eventMappingsProp];}
		}

		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "24.20:31:23")]
		[ConfigurationProperty ("heartbeatInterval", DefaultValue = "00:00:00")]
		public TimeSpan HeartbeatInterval {
			get { return (TimeSpan) base [heartbeatIntervalProp];}
			set { base[heartbeatIntervalProp] = value; }
		}

		[ConfigurationProperty ("profiles")]
		public ProfileSettingsCollection Profiles {
			get { return (ProfileSettingsCollection) base [profilesProp];}
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base [providersProp];}
		}

		[ConfigurationProperty ("rules")]
		public RuleSettingsCollection Rules {
			get { return (RuleSettingsCollection) base [rulesProp];}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

