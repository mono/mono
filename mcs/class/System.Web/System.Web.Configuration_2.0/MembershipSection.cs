//
// System.Web.Configuration.MembershipSection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2004,2005 Novell, Inc (http://www.novell.com)
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

	public sealed class MembershipSection : ConfigurationSection
	{
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty hashAlgorithmTypeProp;
		static ConfigurationProperty providersProp;
		static ConfigurationProperty userIsOnlineTimeWindowProp;
		static ConfigurationPropertyCollection properties;

		static MembershipSection ()
		{
			defaultProviderProp = new ConfigurationProperty ("defaultProvider", typeof (string), "AspNetSqlMembershipProvider",
									 TypeDescriptor.GetConverter (typeof (string)),
									 PropertyHelper.NonEmptyStringValidator,
									 ConfigurationPropertyOptions.None);
			hashAlgorithmTypeProp = new ConfigurationProperty ("hashAlgorithmType", typeof (string), "");
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection), null,
								   null, PropertyHelper.DefaultValidator,
								   ConfigurationPropertyOptions.None);
			userIsOnlineTimeWindowProp = new ConfigurationProperty ("userIsOnlineTimeWindow", typeof (TimeSpan), TimeSpan.FromMinutes (15),
										PropertyHelper.TimeSpanMinutesConverter,
										new TimeSpanValidator (new TimeSpan (0,1,0), TimeSpan.MaxValue),
										ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (defaultProviderProp);
			properties.Add (hashAlgorithmTypeProp);
			properties.Add (providersProp);
			properties.Add (userIsOnlineTimeWindowProp);
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlMembershipProvider")]
		public string DefaultProvider {
			get { return (string) base [defaultProviderProp];}
			set { base[defaultProviderProp] = value; }
		}

		[ConfigurationProperty ("hashAlgorithmType", DefaultValue = "")]
		public string HashAlgorithmType {
			get { return (string) base [hashAlgorithmTypeProp];}
			set { base[hashAlgorithmTypeProp] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base [providersProp];}
		}

		[TypeConverter (typeof (TimeSpanMinutesConverter))]
		[TimeSpanValidator (MinValueString = "00:01:00")]
		[ConfigurationProperty ("userIsOnlineTimeWindow", DefaultValue = "00:15:00")]
		public TimeSpan UserIsOnlineTimeWindow {
			get { return (TimeSpan) base [userIsOnlineTimeWindowProp];}
			set { base[userIsOnlineTimeWindowProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
