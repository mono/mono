//
// System.Web.Configuration.ProfileSection
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

#if NET_2_0

using System;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class ProfileSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty autoSaveEnabledProp;
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty inheritsProp;
		static ConfigurationProperty propertySettingsProp;
		static ConfigurationProperty providersProp;

		static ProfileSection ()
		{
			autoSaveEnabledProp = new ConfigurationProperty ("automaticSaveEnabled", typeof (bool), true);
			defaultProviderProp = new ConfigurationProperty ("defaultProviderProp", typeof (string), "AspNetSqlProfileProvider");
			inheritsProp = new ConfigurationProperty ("inheritsProps", typeof (string), "");
			propertySettingsProp = new ConfigurationProperty ("propertiesProp", typeof (RootProfilePropertySettingsCollection), null);
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection), null);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (autoSaveEnabledProp);
			properties.Add (defaultProviderProp);
			properties.Add (inheritsProp);
			properties.Add (propertySettingsProp);
			properties.Add (providersProp);
		}

		public ProfileSection ()
		{
		}

		[ConfigurationProperty ("automaticSaveEnabled", DefaultValue = true)]
		public bool AutomaticSaveEnabled {
			get { return (bool) base [autoSaveEnabledProp]; }
			set { base [autoSaveEnabledProp] = value; }
		}

		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlProfileProvider")]
		[StringValidator (MinLength = 1)]
		public string DefaultProvider {
			get { return (string) base [defaultProviderProp]; }
			set { base [defaultProviderProp] = value; }
		}

		[ConfigurationProperty ("enabled", DefaultValue = true)]
		public bool Enabled {
			get { return (bool) base [enabledProp]; }
			set { base [enabledProp] = value; }
		}

		[ConfigurationProperty ("inherits", DefaultValue = "")]
		public string Inherits {
			get { return (string) base [inheritsProp]; }
			set { base [inheritsProp] = value; }
		}

		[ConfigurationProperty ("properties")]
		public RootProfilePropertySettingsCollection PropertySettings {
			get {
				return (RootProfilePropertySettingsCollection) base[propertySettingsProp];
			}
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get {
				return (ProviderSettingsCollection) base[providersProp];
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				return properties;
			}
		}

	}
}

#endif
