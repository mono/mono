//
// System.Web.Configuration.RuleSettings
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

	public sealed class RuleSettings : ConfigurationElement
	{
		static ConfigurationProperty customProp;
		static ConfigurationProperty eventNameProp;
		static ConfigurationProperty maxLimitProp;
		static ConfigurationProperty minInstancesProp;
		static ConfigurationProperty minIntervalProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty profileProp;
		static ConfigurationProperty providerProp;
		static ConfigurationPropertyCollection properties;

		static RuleSettings ()
		{
			customProp = new ConfigurationProperty ("custom", typeof (string), "");
			eventNameProp = new ConfigurationProperty ("eventName", typeof (string), "", ConfigurationPropertyOptions.IsRequired);
			maxLimitProp = new ConfigurationProperty ("maxLimit", typeof (int), Int32.MaxValue,
								  PropertyHelper.InfiniteIntConverter,
								  PropertyHelper.IntFromZeroToMaxValidator,
								  ConfigurationPropertyOptions.None);
			minInstancesProp = new ConfigurationProperty ("minInstances", typeof (int), 1,
								      TypeDescriptor.GetConverter (typeof (int)),
								      new IntegerValidator (1, Int32.MaxValue),
								      ConfigurationPropertyOptions.None);
			minIntervalProp = new ConfigurationProperty ("minInterval", typeof (TimeSpan), TimeSpan.FromSeconds (0),
								     PropertyHelper.InfiniteTimeSpanConverter, null,
								     ConfigurationPropertyOptions.None);
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			profileProp = new ConfigurationProperty ("profile", typeof (string), "");
			providerProp = new ConfigurationProperty ("provider", typeof (string), "");
			properties = new ConfigurationPropertyCollection ();

			properties.Add (customProp);
			properties.Add (eventNameProp);
			properties.Add (maxLimitProp);
			properties.Add (minInstancesProp);
			properties.Add (minIntervalProp);
			properties.Add (nameProp);
			properties.Add (profileProp);
			properties.Add (providerProp);
		}

		internal RuleSettings ()
		{
		}

		public RuleSettings (string name, string eventName, string provider, string profile, int minInstances, int maxLimit, TimeSpan minInterval, string custom)
		{
			this.Name = name;
			this.EventName = eventName;
			this.Provider = provider;
			this.Profile = profile;
			this.MinInstances = minInstances;
			this.MaxLimit = maxLimit;
			this.MinInterval = minInterval;
			this.Custom = custom;
		}

		public RuleSettings (string name, string eventName, string provider, string profile, int minInstances, int maxLimit, TimeSpan minInterval)
		{
			this.Name = name;
			this.EventName = eventName;
			this.Provider = provider;
			this.Profile = profile;
			this.MinInstances = minInstances;
			this.MaxLimit = maxLimit;
			this.MinInterval = minInterval;
		}

		public RuleSettings (string name, string eventName, string provider)
		{
			this.Name = name;
			this.EventName = eventName;
			this.Provider = provider;
		}

		[ConfigurationProperty ("custom", DefaultValue = "")]
		public string Custom {
			get { return (string) base [customProp];}
			set { base[customProp] = value; }
		}

		[ConfigurationProperty ("eventName", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string EventName {
			get { return (string) base [eventNameProp];}
			set { base[eventNameProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxLimit", DefaultValue = "2147483647")]
		public int MaxLimit {
			get { return (int) base [maxLimitProp];}
			set { base[maxLimitProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("minInstances", DefaultValue = "1")]
		public int MinInstances {
			get { return (int) base [minInstancesProp];}
			set { base[minInstancesProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("minInterval", DefaultValue = "00:00:00")]
		public TimeSpan MinInterval {
			get { return (TimeSpan) base [minIntervalProp];}
			set { base[minIntervalProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [nameProp];}
			set { base[nameProp] = value; }
		}

		[ConfigurationProperty ("profile", DefaultValue = "")]
		public string Profile {
			get { return (string) base [profileProp];}
			set { base[profileProp] = value; }
		}

		[ConfigurationProperty ("provider", DefaultValue = "")]
		public string Provider {
			get { return (string) base [providerProp];}
			set { base[providerProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

