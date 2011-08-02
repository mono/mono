//
// System.Web.Configuration.HostingEnvironmentSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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

	public sealed class HostingEnvironmentSection : ConfigurationSection
	{
		static ConfigurationProperty idleTimeoutProp;
		static ConfigurationProperty shadowCopyBinAssembliesProp;
		static ConfigurationProperty shutdownTimeoutProp;
		static ConfigurationPropertyCollection properties;

		static HostingEnvironmentSection ()
		{
			idleTimeoutProp = new ConfigurationProperty ("idleTimeout", typeof (TimeSpan), TimeSpan.MaxValue,
								     PropertyHelper.TimeSpanMinutesOrInfiniteConverter,
								     PropertyHelper.PositiveTimeSpanValidator,
								     ConfigurationPropertyOptions.None);
			shadowCopyBinAssembliesProp = new ConfigurationProperty ("shadowCopyBinAssemblies", typeof (bool), true);
			shutdownTimeoutProp = new ConfigurationProperty ("shutdownTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (30),
									 PropertyHelper.TimeSpanSecondsConverter,
									 PropertyHelper.PositiveTimeSpanValidator,
									 ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (idleTimeoutProp);
			properties.Add (shadowCopyBinAssembliesProp);
			properties.Add (shutdownTimeoutProp);

		}

		[TypeConverter (typeof (TimeSpanMinutesOrInfiniteConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("idleTimeout", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan IdleTimeout {
			get { return (TimeSpan) base [idleTimeoutProp];}
			set { base[idleTimeoutProp] = value; }
		}

		[ConfigurationProperty ("shadowCopyBinAssemblies", DefaultValue = "True")]
		public bool ShadowCopyBinAssemblies {
			get { return (bool) base [shadowCopyBinAssembliesProp];}
			set { base[shadowCopyBinAssembliesProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("shutdownTimeout", DefaultValue = "00:00:30")]
		public TimeSpan ShutdownTimeout {
			get { return (TimeSpan) base [shutdownTimeoutProp];}
			set { base[shutdownTimeoutProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

