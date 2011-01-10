//
// System.Web.Configuration.ProfileGroupSettings
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
using System.Xml;

namespace System.Web.Configuration
{
	public sealed class ProfileGroupSettings : ConfigurationElement
	{
		static ConfigurationProperty propertySettingsProp;
		static ConfigurationProperty nameProp;
		
		static ConfigurationPropertyCollection properties;
		
		static ProfileGroupSettings ()
		{
			propertySettingsProp = new ConfigurationProperty (null, typeof (ProfilePropertySettingsCollection), null, null, null,
									  ConfigurationPropertyOptions.IsDefaultCollection);
			nameProp = new ConfigurationProperty ("name", typeof (string), null, null, PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (propertySettingsProp);
			properties.Add (nameProp);
		}
		
		internal ProfileGroupSettings ()
		{
		}

		public ProfileGroupSettings (string name)
		{
			this.Name = name;
		}

		public override bool Equals (object obj)
		{
			ProfileGroupSettings other = obj as ProfileGroupSettings;
			if (other == null)
				return false;

			if (GetType () != other.GetType ())
				return false;

			return Name.Equals (other.Name);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		internal void DoDeserialize (XmlReader reader)
		{
			DeserializeElement (reader, false);
		}
		
		[ConfigurationProperty ("name", IsRequired = true, IsKey = true)]
		public string Name {
			get { return (string)base [nameProp]; }
			internal set { base [nameProp] = value; }
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public ProfilePropertySettingsCollection PropertySettings {
			get { return (ProfilePropertySettingsCollection) base [propertySettingsProp]; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
