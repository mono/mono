//
// System.Web.Configuration.SessionStateSection
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
using System.Web.SessionState;

namespace System.Web.Configuration
{
	public sealed class SessionStateSection : ConfigurationSection
	{
		internal static readonly string DefaultSqlConnectionString = "data source=localhost;Integrated Security=SSPI";
		
		static ConfigurationProperty allowCustomSqlDatabaseProp;
		static ConfigurationProperty cookielessProp;
		static ConfigurationProperty cookieNameProp;
		static ConfigurationProperty customProviderProp;
		static ConfigurationProperty modeProp;
		static ConfigurationProperty partitionResolverTypeProp;
		static ConfigurationProperty providersProp;
		static ConfigurationProperty regenerateExpiredSessionIdProp;
		static ConfigurationProperty sessionIDManagerTypeProp;
		static ConfigurationProperty sqlCommandTimeoutProp;
		static ConfigurationProperty sqlConnectionStringProp;
		static ConfigurationProperty stateConnectionStringProp;
		static ConfigurationProperty stateNetworkTimeoutProp;
		static ConfigurationProperty timeoutProp;
		static ConfigurationProperty useHostingIdentityProp;
#if NET_4_0
		static ConfigurationProperty compressionEnabledProp;
		static ConfigurationProperty sqlConnectionRetryIntervalProp;
#endif
		static ConfigurationPropertyCollection properties;

		static ConfigurationElementProperty elementProperty;

		static SessionStateSection ()
		{
			allowCustomSqlDatabaseProp = new ConfigurationProperty ("allowCustomSqlDatabase", typeof (bool), false);
			cookielessProp = new ConfigurationProperty ("cookieless", typeof (string), null);
			cookieNameProp = new ConfigurationProperty ("cookieName", typeof (string), "ASP.NET_SessionId");
			customProviderProp = new ConfigurationProperty ("customProvider", typeof (string), "");
			modeProp = new ConfigurationProperty ("mode", typeof (SessionStateMode), SessionStateMode.InProc,
							      new GenericEnumConverter (typeof (SessionStateMode)), null,
							      ConfigurationPropertyOptions.None);
			partitionResolverTypeProp = new ConfigurationProperty ("partitionResolverType", typeof (string), "");
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection), null,
								   null, null, ConfigurationPropertyOptions.None);
			regenerateExpiredSessionIdProp = new ConfigurationProperty ("regenerateExpiredSessionId", typeof (bool), true);
			sessionIDManagerTypeProp = new ConfigurationProperty ("sessionIDManagerType", typeof (string), "");
			sqlCommandTimeoutProp = new ConfigurationProperty ("sqlCommandTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (30),
									   PropertyHelper.TimeSpanSecondsOrInfiniteConverter, null,
									   ConfigurationPropertyOptions.None);
			sqlConnectionStringProp = new ConfigurationProperty ("sqlConnectionString", typeof (string), DefaultSqlConnectionString);
			stateConnectionStringProp = new ConfigurationProperty ("stateConnectionString", typeof (string), "tcpip=loopback:42424");
			stateNetworkTimeoutProp = new ConfigurationProperty ("stateNetworkTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (10),
									     PropertyHelper.TimeSpanSecondsOrInfiniteConverter,
									     PropertyHelper.PositiveTimeSpanValidator,
									     ConfigurationPropertyOptions.None);
			timeoutProp = new ConfigurationProperty ("timeout", typeof (TimeSpan), TimeSpan.FromMinutes (20),
								 PropertyHelper.TimeSpanMinutesOrInfiniteConverter,
								 new TimeSpanValidator (new TimeSpan (0,1,0), TimeSpan.MaxValue),
								 ConfigurationPropertyOptions.None);
			useHostingIdentityProp = new ConfigurationProperty ("useHostingIdentity", typeof (bool), true);

#if NET_4_0
			compressionEnabledProp = new ConfigurationProperty ("compressionEnabled", typeof (bool), false);
			sqlConnectionRetryIntervalProp = new ConfigurationProperty ("sqlConnectionRetryIntervalProp", typeof (TimeSpan), TimeSpan.FromSeconds (0),
										    PropertyHelper.TimeSpanSecondsOrInfiniteConverter,
										    PropertyHelper.PositiveTimeSpanValidator,
										    ConfigurationPropertyOptions.None);
#endif
			properties = new ConfigurationPropertyCollection ();

			properties.Add (allowCustomSqlDatabaseProp);
			properties.Add (cookielessProp);
			properties.Add (cookieNameProp);
			properties.Add (customProviderProp);
			properties.Add (modeProp);
			properties.Add (partitionResolverTypeProp);
			properties.Add (providersProp);
			properties.Add (regenerateExpiredSessionIdProp);
			properties.Add (sessionIDManagerTypeProp);
			properties.Add (sqlCommandTimeoutProp);
			properties.Add (sqlConnectionStringProp);
			properties.Add (stateConnectionStringProp);
			properties.Add (stateNetworkTimeoutProp);
			properties.Add (timeoutProp);
			properties.Add (useHostingIdentityProp);
#if NET_4_0
			properties.Add (compressionEnabledProp);
			properties.Add (sqlConnectionRetryIntervalProp);
#endif

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (SessionStateSection), ValidateElement));
		}

		protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		[ConfigurationProperty ("allowCustomSqlDatabase", DefaultValue = "False")]
		public bool AllowCustomSqlDatabase {
			get { return (bool) base [allowCustomSqlDatabaseProp];}
			set { base[allowCustomSqlDatabaseProp] = value; }
		}

		[ConfigurationProperty ("cookieless")]
		public HttpCookieMode Cookieless {
			get { return ParseCookieMode ((string) base [cookielessProp]); }
			set { base[cookielessProp] = value.ToString(); }
		}

		[ConfigurationProperty ("cookieName", DefaultValue = "ASP.NET_SessionId")]
		public string CookieName {
			get { return (string) base [cookieNameProp];}
			set { base[cookieNameProp] = value; }
		}

		[ConfigurationProperty ("customProvider", DefaultValue = "")]
		public string CustomProvider {
			get { return (string) base [customProviderProp];}
			set { base[customProviderProp] = value; }
		}

		[ConfigurationProperty ("mode", DefaultValue = "InProc")]
		public SessionStateMode Mode {
			get { return (SessionStateMode) base [modeProp];}
			set { base[modeProp] = value; }
		}

		[ConfigurationProperty ("partitionResolverType", DefaultValue = "")]
		public string PartitionResolverType {
			get { return (string) base [partitionResolverTypeProp];}
			set { base[partitionResolverTypeProp] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base [providersProp];}
		}

		[ConfigurationProperty ("regenerateExpiredSessionId", DefaultValue = "True")]
		public bool RegenerateExpiredSessionId {
			get { return (bool) base [regenerateExpiredSessionIdProp];}
			set { base[regenerateExpiredSessionIdProp] = value; }
		}

		[ConfigurationProperty ("sessionIDManagerType", DefaultValue = "")]
		public string SessionIDManagerType {
			get { return (string) base [sessionIDManagerTypeProp];}
			set { base[sessionIDManagerTypeProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsOrInfiniteConverter))]
		[ConfigurationProperty ("sqlCommandTimeout", DefaultValue = "00:00:30")]
		public TimeSpan SqlCommandTimeout {
			get { return (TimeSpan) base [sqlCommandTimeoutProp];}
			set { base[sqlCommandTimeoutProp] = value; }
		}

		[ConfigurationProperty ("sqlConnectionString", DefaultValue = "data source=localhost;Integrated Security=SSPI")]
		public string SqlConnectionString {
			get { return (string) base [sqlConnectionStringProp];}
			set { base[sqlConnectionStringProp] = value; }
		}

		[ConfigurationProperty ("stateConnectionString", DefaultValue = "tcpip=loopback:42424")]
		public string StateConnectionString {
			get { return (string) base [stateConnectionStringProp];}
			set { base[stateConnectionStringProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsOrInfiniteConverter))]
		[ConfigurationProperty ("stateNetworkTimeout", DefaultValue = "00:00:10")]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public TimeSpan StateNetworkTimeout {
			get { return (TimeSpan) base [stateNetworkTimeoutProp];}
			set { base[stateNetworkTimeoutProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanMinutesOrInfiniteConverter))]
		[TimeSpanValidator (MinValueString = "00:01:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("timeout", DefaultValue = "00:20:00")]
		public TimeSpan Timeout {
			get { return (TimeSpan) base [timeoutProp];}
			set { base[timeoutProp] = value; }
		}

		[ConfigurationProperty ("useHostingIdentity", DefaultValue = "True")]
		public bool UseHostingIdentity {
			get { return (bool) base [useHostingIdentityProp];}
			set { base[useHostingIdentityProp] = value; }
		}

#if NET_4_0
		[ConfigurationPropertyAttribute("compressionEnabled", DefaultValue = false)]
		public bool CompressionEnabled {
			get { return (bool) base [compressionEnabledProp]; }
			set { base [compressionEnabledProp] = value; }
		}

		[TypeConverterAttribute(typeof(TimeSpanSecondsOrInfiniteConverter))]
		[ConfigurationPropertyAttribute("sqlConnectionRetryInterval", DefaultValue = "00:00:00")]
		public TimeSpan SqlConnectionRetryInterval {
			get { return (TimeSpan) base [sqlConnectionRetryIntervalProp]; }
			set { base [sqlConnectionRetryIntervalProp] = value; }
		}
#endif
		
		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		HttpCookieMode ParseCookieMode (string s)
		{
			if (s == "true")
				return HttpCookieMode.UseUri;
			else if (s == "false" || s == null)
				return HttpCookieMode.UseCookies;
			else {
				try {
					return (HttpCookieMode)Enum.Parse (typeof(HttpCookieMode), s);
				}
				catch {
					return HttpCookieMode.UseCookies;
				}
			}
		}

#region CompatabilityCode
		internal bool CookieLess {
			get { return Cookieless != HttpCookieMode.UseCookies; }
			set { Cookieless = value ? HttpCookieMode.UseUri : HttpCookieMode.UseCookies; }
		}
#endregion

	}
}
