//
// MemoryCacheElement.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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

namespace System.Runtime.Caching.Configuration
{
	public sealed class MemoryCacheElement : ConfigurationElement
	{
		static ConfigurationProperty cacheMemoryLimitMegabytesProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty physicalMemoryLimitPercentageProp;
		static ConfigurationProperty pollingIntervalProp;
		static ConfigurationPropertyCollection properties;
		
		[ConfigurationProperty ("cacheMemoryLimitMegabytes", DefaultValue = 0)]
		[IntegerValidator (MinValue = 1)]
		public int CacheMemoryLimitMegabytes {
			get { return (int) base [cacheMemoryLimitMegabytesProp]; }
			set { base [cacheMemoryLimitMegabytesProp] = value; }
		}

		[ConfigurationProperty ("name", DefaultValue = "", IsRequired = true, IsKey = true)]
		[TypeConverter (typeof(WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		public string Name {
			get { return (string) base [nameProp]; }
			set { base [nameProp] = value; }
		}

		[ConfigurationProperty ("physicalMemoryLimitPercentage", DefaultValue = 0)]
		[IntegerValidator (MinValue = 1, MaxValue = 100)]
		public int PhysicalMemoryLimitPercentage {
			get { return (int) base [physicalMemoryLimitPercentageProp]; }
			set { base [physicalMemoryLimitPercentageProp] = value; }
		}
		
		[ConfigurationProperty ("pollingInterval", DefaultValue = "00:02:00")]
		[TypeConverter (typeof(InfiniteTimeSpanConverter))]
		public TimeSpan PollingInterval {
			get { return (TimeSpan) base [pollingIntervalProp]; }
			set { base [pollingIntervalProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
		
		static MemoryCacheElement ()
		{
			cacheMemoryLimitMegabytesProp = new ConfigurationProperty ("cacheMemoryLimitMegabytes", typeof (int), 0,
										   TypeDescriptor.GetConverter (typeof (int)),
										   new IntegerValidator (1, Int32.MaxValue),
										   ConfigurationPropertyOptions.None);
			nameProp = new ConfigurationProperty ("name", typeof (string), String.Empty,
							      TypeDescriptor.GetConverter (typeof (string)),
							      new NullableStringValidator (1),
							      ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			physicalMemoryLimitPercentageProp = new ConfigurationProperty ("physicalMemoryLimitPercentage", typeof (int), 0,
										       TypeDescriptor.GetConverter (typeof (int)),
										       new IntegerValidator (1, 100),
										       ConfigurationPropertyOptions.None);
			pollingIntervalProp = new ConfigurationProperty ("pollingInterval", typeof (TimeSpan), TimeSpan.FromMinutes (2),
									 new InfiniteTimeSpanConverter (),
									 new DefaultValidator (),
									 ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (cacheMemoryLimitMegabytesProp);
			properties.Add (nameProp);
			properties.Add (physicalMemoryLimitPercentageProp);
			properties.Add (pollingIntervalProp);
		}

		internal MemoryCacheElement ()
		{
		}
		
		public MemoryCacheElement (string name)
		{
			this.Name = name;
		}
	}
}
