//
// System.Web.Configuration.ProfilePropertySettingsCollection
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
using System.ComponentModel;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class ProfilePropertySettings : ConfigurationElement
	{
		static ConfigurationProperty allowAnonymousProp;
		static ConfigurationProperty customProviderDataProp;
		static ConfigurationProperty defaultValueProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty providerProp;
		static ConfigurationProperty readOnlyProp;
		static ConfigurationProperty serializeAsProp;
		static ConfigurationProperty typeProp;

		static ConfigurationPropertyCollection properties;

		static ProfilePropertySettings ()
		{
			allowAnonymousProp = new ConfigurationProperty ("allowAnonymous", typeof (bool), false);
			customProviderDataProp = new ConfigurationProperty ("customProviderData", typeof (string), "");
			defaultValueProp = new ConfigurationProperty ("defaultValue", typeof (string), "");
			nameProp = new ConfigurationProperty ("name", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      new ProfilePropertyNameValidator (),
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			providerProp = new ConfigurationProperty ("provider", typeof (string), "");
			readOnlyProp = new ConfigurationProperty ("readOnly", typeof (bool), false);
			serializeAsProp = new ConfigurationProperty ("serializeAs", typeof (SerializationMode), SerializationMode.ProviderSpecific,
								     new GenericEnumConverter (typeof (SerializationMode)),
								     PropertyHelper.DefaultValidator,
								     ConfigurationPropertyOptions.None);
			typeProp = new ConfigurationProperty ("type", typeof (string), "string");

			properties = new ConfigurationPropertyCollection ();
			properties.Add (allowAnonymousProp);
			properties.Add (customProviderDataProp);
			properties.Add (defaultValueProp);
			properties.Add (nameProp);
			properties.Add (providerProp);
			properties.Add (readOnlyProp);
			properties.Add (serializeAsProp);
			properties.Add (typeProp);
		}

		internal ProfilePropertySettings ()
		{
		}

		public ProfilePropertySettings (string name)
		{
			this.Name = name;
		}

		public ProfilePropertySettings (string name, bool readOnly, SerializationMode serializeAs,
						string providerName, string defaultValue, string profileType,
						bool allowAnonymous, string customProviderData)
		{
			this.Name = name;
			this.ReadOnly = readOnly;
			this.SerializeAs = serializeAs;
			this.Provider = providerName;
			this.DefaultValue = defaultValue;
			this.Type = profileType;
			this.AllowAnonymous = allowAnonymous;
			this.CustomProviderData = customProviderData;
		}

		[ConfigurationProperty ("allowAnonymous", DefaultValue = false)]
		public bool AllowAnonymous {
			get { return (bool) base[allowAnonymousProp]; }
			set { base [allowAnonymousProp] = value; }
		}

		[ConfigurationProperty ("customProviderData", DefaultValue = "")]
		public string CustomProviderData {
			get { return (string) base[customProviderDataProp]; }
			set { base[customProviderDataProp] = value; }
		}
		
		[ConfigurationProperty ("defaultValue", DefaultValue = "")]
		public string DefaultValue {
			get { return (string) base[defaultValueProp]; }
			set { base[defaultValueProp] = value; }
		}
		
		[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base[nameProp]; }
			set { base[nameProp] = value; }
		}
		
		[ConfigurationProperty ("provider", DefaultValue = "")]
		public string Provider {
			get { return (string) base[providerProp]; }
			set { base[providerProp] = value; }
		}
		
		[ConfigurationProperty ("readOnly", DefaultValue = false)]
		public bool ReadOnly {
			get { return (bool) base[readOnlyProp]; }
			set { base[readOnlyProp] = value; }
		}
		
		[ConfigurationProperty ("serializeAs", DefaultValue = "ProviderSpecific")]
		public SerializationMode SerializeAs {
			get { return (SerializationMode) base[serializeAsProp]; }
			set { base[serializeAsProp] = value; }
		}
		
		[ConfigurationProperty ("type", DefaultValue = "string")]
		public string Type {
			get { return (string) base[typeProp]; }
			set { base[typeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get {
				return properties;
			}
		}
		
	}

}

#endif
