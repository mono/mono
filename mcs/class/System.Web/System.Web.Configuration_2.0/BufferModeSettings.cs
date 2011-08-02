//
// System.Web.Configuration.BufferModeSettings
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

	public sealed class BufferModeSettings : ConfigurationElement
	{
		static ConfigurationProperty maxBufferSizeProp;
		static ConfigurationProperty maxBufferThreadsProp;
		static ConfigurationProperty maxFlushSizeProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty regularFlushIntervalProp;
		static ConfigurationProperty urgentFlushIntervalProp;
		static ConfigurationProperty urgentFlushThresholdProp;
		static ConfigurationPropertyCollection properties;
		static ConfigurationElementProperty elementProperty;

		static BufferModeSettings ()
		{
			IntegerValidator iv = new IntegerValidator (1, Int32.MaxValue);
			
			maxBufferSizeProp = new ConfigurationProperty ("maxBufferSize", typeof (int), Int32.MaxValue,
								       PropertyHelper.InfiniteIntConverter, iv,
								       ConfigurationPropertyOptions.IsRequired);
			maxBufferThreadsProp = new ConfigurationProperty ("maxBufferThreads", typeof (int), 1,
									  PropertyHelper.InfiniteIntConverter, iv,
									  ConfigurationPropertyOptions.None);
			maxFlushSizeProp = new ConfigurationProperty ("maxFlushSize", typeof (int), Int32.MaxValue,
								      PropertyHelper.InfiniteIntConverter, iv,
								      ConfigurationPropertyOptions.IsRequired);
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      TypeDescriptor.GetConverter (typeof (string)), PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			regularFlushIntervalProp = new ConfigurationProperty ("regularFlushInterval", typeof (TimeSpan), TimeSpan.FromSeconds (1),
									      PropertyHelper.InfiniteTimeSpanConverter,
									      PropertyHelper.PositiveTimeSpanValidator,
									      ConfigurationPropertyOptions.IsRequired);
			urgentFlushIntervalProp = new ConfigurationProperty ("urgentFlushInterval", typeof (TimeSpan), TimeSpan.FromSeconds (0),
									     PropertyHelper.InfiniteTimeSpanConverter, null,
									     ConfigurationPropertyOptions.IsRequired);
			urgentFlushThresholdProp = new ConfigurationProperty ("urgentFlushThreshold", typeof (int), Int32.MaxValue,
									      PropertyHelper.InfiniteIntConverter, iv,
									      ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (nameProp);
			properties.Add (maxBufferSizeProp);
			properties.Add (maxBufferThreadsProp);
			properties.Add (maxFlushSizeProp);
			properties.Add (regularFlushIntervalProp);
			properties.Add (urgentFlushIntervalProp);
			properties.Add (urgentFlushThresholdProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (BufferModeSettings), ValidateElement));
		}

		internal BufferModeSettings ()
		{
		}

		public BufferModeSettings (string name, int maxBufferSize, int maxFlushSize, int urgentFlushThreshold,
					   TimeSpan regularFlushInterval, TimeSpan urgentFlushInterval, int maxBufferThreads)
		{
			this.Name = name;
			this.MaxBufferSize = maxBufferSize;
			this.MaxFlushSize = maxFlushSize;
			this.UrgentFlushThreshold = urgentFlushThreshold;
			this.RegularFlushInterval = regularFlushInterval;
			this.UrgentFlushInterval = urgentFlushInterval;
			this.MaxBufferThreads = maxBufferThreads;
		}

		[MonoTODO("Should do some validation here")]
		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxBufferSize", DefaultValue = "2147483647", Options = ConfigurationPropertyOptions.IsRequired)]
		public int MaxBufferSize {
			get { return (int) base [maxBufferSizeProp];}
			set { base[maxBufferSizeProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxBufferThreads", DefaultValue = "1")]
		public int MaxBufferThreads {
			get { return (int) base [maxBufferThreadsProp];}
			set { base[maxBufferThreadsProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxFlushSize", DefaultValue = "2147483647", Options = ConfigurationPropertyOptions.IsRequired)]
		public int MaxFlushSize {
			get { return (int) base [maxFlushSizeProp];}
			set { base[maxFlushSizeProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [nameProp];}
			set { base[nameProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("regularFlushInterval", DefaultValue = "00:00:01", Options = ConfigurationPropertyOptions.IsRequired)]
		public TimeSpan RegularFlushInterval {
			get { return (TimeSpan) base [regularFlushIntervalProp];}
			set { base[regularFlushIntervalProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("urgentFlushInterval", DefaultValue = "00:00:00", Options = ConfigurationPropertyOptions.IsRequired)]
		public TimeSpan UrgentFlushInterval {
			get { return (TimeSpan) base [urgentFlushIntervalProp];}
			set { base[urgentFlushIntervalProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("urgentFlushThreshold", DefaultValue = "2147483647", Options = ConfigurationPropertyOptions.IsRequired)]
		public int UrgentFlushThreshold {
			get { return (int) base [urgentFlushThresholdProp];}
			set { base[urgentFlushThresholdProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

