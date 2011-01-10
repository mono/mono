//
// System.Web.Configuration.CacheSection
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

	public sealed class CacheSection : ConfigurationSection
	{
		static ConfigurationProperty disableExpirationProp;
		static ConfigurationProperty disableMemoryCollectionProp;
		static ConfigurationProperty percentagePhysicalMemoryUsedLimitProp;
		static ConfigurationProperty privateBytesLimitProp;
		static ConfigurationProperty privateBytesPollTimeProp;
		static ConfigurationPropertyCollection properties;

		static CacheSection ()
		{
			disableExpirationProp = new ConfigurationProperty("disableExpiration", typeof (bool), false);
			disableMemoryCollectionProp = new ConfigurationProperty("disableMemoryCollection", typeof (bool), false);
			percentagePhysicalMemoryUsedLimitProp = new ConfigurationProperty("percentagePhysicalMemoryUsedLimit",
											  typeof (int), 0,
											  TypeDescriptor.GetConverter (typeof (int)),
											  PropertyHelper.IntFromZeroToMaxValidator,
											  ConfigurationPropertyOptions.None);
			privateBytesLimitProp = new ConfigurationProperty("privateBytesLimit", typeof (long), 0L,
									  TypeDescriptor.GetConverter (typeof (long)),
									  new LongValidator (0, Int64.MaxValue),
									  ConfigurationPropertyOptions.None);
			privateBytesPollTimeProp = new ConfigurationProperty("privateBytesPollTime",
									     typeof (TimeSpan),
									     TimeSpan.FromMinutes (2),
									     PropertyHelper.InfiniteTimeSpanConverter,
									     PropertyHelper.PositiveTimeSpanValidator,
									     ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection();

			properties.Add (disableExpirationProp);
			properties.Add (disableMemoryCollectionProp);
			properties.Add (percentagePhysicalMemoryUsedLimitProp);
			properties.Add (privateBytesLimitProp);
			properties.Add (privateBytesPollTimeProp);
		}

		[ConfigurationProperty ("disableExpiration", DefaultValue = "False")]
		public bool DisableExpiration {
			get { return (bool) base [disableExpirationProp];}
			set { base[disableExpirationProp] = value; }
		}

		[ConfigurationProperty ("disableMemoryCollection", DefaultValue = "False")]
		public bool DisableMemoryCollection {
			get { return (bool) base [disableMemoryCollectionProp];}
			set { base[disableMemoryCollectionProp] = value; }
		}
			
		[IntegerValidator (MinValue = 0, MaxValue = 100)]
		[ConfigurationProperty ("percentagePhysicalMemoryUsedLimit", DefaultValue = "0")]
		public int PercentagePhysicalMemoryUsedLimit {
			get { return (int) base [percentagePhysicalMemoryUsedLimitProp];}
			set { base[percentagePhysicalMemoryUsedLimitProp] = value; }
		}

		[LongValidator (MinValue = (long) 0, MaxValue = Int64.MaxValue)]
		[ConfigurationProperty ("privateBytesLimit", DefaultValue = "0")]
		public long PrivateBytesLimit {
			get { return (long) base [privateBytesLimitProp];}
			set { base[privateBytesLimitProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("privateBytesPollTime", DefaultValue = "00:02:00")]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public TimeSpan PrivateBytesPollTime {
			get { return (TimeSpan) base [privateBytesPollTimeProp];}
			set { base[privateBytesPollTimeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

