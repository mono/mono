//
// System.Web.Configuration.FormsAuthenticationConfiguration
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class FormsAuthenticationConfiguration: ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;

		static ConfigurationProperty cookielessProp;
		static ConfigurationProperty credentialsProp;
		static ConfigurationProperty defaultUrlProp;
		static ConfigurationProperty domainProp;
		static ConfigurationProperty enableCrossAppRedirectsProp;
		static ConfigurationProperty loginUrlProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty pathProp;
		static ConfigurationProperty protectionProp;
		static ConfigurationProperty requireSSLProp;
		static ConfigurationProperty slidingExpirationProp;
		static ConfigurationProperty timeoutProp;

		static ConfigurationElementProperty elementProperty;

		static FormsAuthenticationConfiguration ()
		{
			cookielessProp = new ConfigurationProperty ("cookieless", typeof (HttpCookieMode), HttpCookieMode.UseDeviceProfile,
								    new GenericEnumConverter (typeof (HttpCookieMode)), PropertyHelper.DefaultValidator,
								    ConfigurationPropertyOptions.None);
			credentialsProp = new ConfigurationProperty ("credentials", typeof (FormsAuthenticationCredentials), null,
								     null, PropertyHelper.DefaultValidator,
								     ConfigurationPropertyOptions.None);
			defaultUrlProp = new ConfigurationProperty ("defaultUrl", typeof (string), "default.aspx",
								    TypeDescriptor.GetConverter (typeof (string)),
								    PropertyHelper.NonEmptyStringValidator,
								    ConfigurationPropertyOptions.None);
			domainProp = new ConfigurationProperty ("domain", typeof (string), "");
			enableCrossAppRedirectsProp = new ConfigurationProperty ("enableCrossAppRedirects", typeof (bool), false);
			loginUrlProp = new ConfigurationProperty ("loginUrl", typeof (string), "login.aspx",
								    TypeDescriptor.GetConverter (typeof (string)),
								    PropertyHelper.NonEmptyStringValidator,
								    ConfigurationPropertyOptions.None);
			nameProp = new ConfigurationProperty ("name", typeof (string), ".ASPXAUTH",
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.None);
			pathProp = new ConfigurationProperty ("path", typeof (string), "/",
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.None);
			protectionProp = new ConfigurationProperty ("protection", typeof (FormsProtectionEnum), FormsProtectionEnum.All,
								    new GenericEnumConverter (typeof (FormsProtectionEnum)),
								    PropertyHelper.DefaultValidator,
								    ConfigurationPropertyOptions.None);
			requireSSLProp = new ConfigurationProperty ("requireSSL", typeof (bool), false);
			slidingExpirationProp = new ConfigurationProperty ("slidingExpiration", typeof (bool), true);
			timeoutProp = new ConfigurationProperty ("timeout", typeof (TimeSpan), TimeSpan.FromMinutes (30),
								 PropertyHelper.TimeSpanMinutesConverter,
								 new TimeSpanValidator (new TimeSpan (0,1,0), TimeSpan.MaxValue),
								 ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (cookielessProp);
			properties.Add (credentialsProp);
			properties.Add (defaultUrlProp);
			properties.Add (domainProp);
			properties.Add (enableCrossAppRedirectsProp);
			properties.Add (loginUrlProp);
			properties.Add (nameProp);
			properties.Add (pathProp);
			properties.Add (protectionProp);
			properties.Add (requireSSLProp);
			properties.Add (slidingExpirationProp);
			properties.Add (timeoutProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (FormsAuthenticationConfiguration), ValidateElement));
		}

		public FormsAuthenticationConfiguration ()
		{
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		[ConfigurationProperty ("cookieless", DefaultValue = "UseDeviceProfile")]
		public HttpCookieMode Cookieless {
			get { return (HttpCookieMode)base[cookielessProp]; }
			set { base[cookielessProp] = value; }
		}

		[ConfigurationProperty ("credentials")]
		public FormsAuthenticationCredentials Credentials {
			get { return (FormsAuthenticationCredentials) base[credentialsProp]; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultUrl", DefaultValue = "default.aspx")]
		public string DefaultUrl {
			get { return (string) base[defaultUrlProp]; }
			set { base[defaultUrlProp] = value; }
		}

		[ConfigurationProperty ("domain", DefaultValue = "")]
		public string Domain {
			get { return (string) base[domainProp]; }
			set { base[domainProp] = value; }
		}

		[ConfigurationProperty ("enableCrossAppRedirects", DefaultValue = "False")]
		public bool EnableCrossAppRedirects {
			get { return (bool) base[enableCrossAppRedirectsProp]; }
			set { base[enableCrossAppRedirectsProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("loginUrl", DefaultValue = "login.aspx")]
		public string LoginUrl {
			get { return (string) base[loginUrlProp]; }
			set { base[loginUrlProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("name", DefaultValue = ".ASPXAUTH")]
		public string Name {
			get { return (string) base[nameProp]; }
			set { base[nameProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("path", DefaultValue = "/")]
		public string Path {
			get { return (string) base[pathProp]; }
			set { base[pathProp] = value; }
		}

		[ConfigurationProperty ("protection", DefaultValue = "All")]
		public FormsProtectionEnum Protection {
			get { return (FormsProtectionEnum) base[protectionProp]; }
			set { base[protectionProp] = value; }
		}

		[ConfigurationProperty ("requireSSL", DefaultValue = "False")]
		public bool RequireSSL {
			get { return (bool) base[requireSSLProp]; }
			set { base[requireSSLProp] = value; }
		}

		[ConfigurationProperty ("slidingExpiration", DefaultValue = "True")]
		public bool SlidingExpiration {
			get { return (bool) base[slidingExpirationProp]; }
			set { base[slidingExpirationProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanMinutesConverter))]
		[TimeSpanValidator (MinValueString = "00:01:00")]
		[ConfigurationProperty ("timeout", DefaultValue = "00:30:00")]
		public TimeSpan Timeout {
			get { return (TimeSpan) base[timeoutProp]; }
			set { base [timeoutProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
