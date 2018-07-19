//
// System.Web.UI.WebControls.SettingElement.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	public sealed class SettingElement
#if (CONFIGURATION_DEP)
		: ConfigurationElement
#endif
	{
#if CONFIGURATION_DEP
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty name_prop, serialize_as_prop, value_prop;
#endif

		static SettingElement ()
		{
#if CONFIGURATION_DEP
			name_prop = new ConfigurationProperty ("name", typeof (string), String.Empty, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			serialize_as_prop = new ConfigurationProperty ("serializeAs", typeof (SettingsSerializeAs), null, ConfigurationPropertyOptions.IsRequired);
			value_prop = new ConfigurationProperty ("value", typeof (SettingValueElement), null, ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (name_prop);
			properties.Add (serialize_as_prop);
			properties.Add (value_prop);
#endif
		}

		public SettingElement ()
		{
		}

		public SettingElement (string name,
				       SettingsSerializeAs serializeAs)
		{
#if CONFIGURATION_DEP
			Name = name;
			SerializeAs = serializeAs;
#endif
		}

#if (CONFIGURATION_DEP)
		[ConfigurationProperty ("name", DefaultValue="",
					Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [name_prop]; }
			set { base [name_prop] = value; } // it does not reject null
		}

		[ConfigurationProperty ("value", DefaultValue=null,
					Options = ConfigurationPropertyOptions.IsRequired)]
		public SettingValueElement Value {
			get { return (SettingValueElement) base [value_prop]; }
			set { base [value_prop] = value; }
		}

		[ConfigurationProperty ("serializeAs", DefaultValue=SettingsSerializeAs.String,
					Options = ConfigurationPropertyOptions.IsRequired)]
		public SettingsSerializeAs SerializeAs {
			get { return base [serialize_as_prop] != null ? (SettingsSerializeAs) base [serialize_as_prop] : default (SettingsSerializeAs); }
			set { base [serialize_as_prop] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public override bool Equals (object settings)
		{
			SettingElement e = settings as SettingElement;
			if (e == null)
				return false;

			return e.SerializeAs == SerializeAs && e.Value == Value && e.Name == Name;
		}

		public override int GetHashCode ()
		{
			int v = (int) SerializeAs ^ 0x7F;
			if (Name != null)
				v += Name.GetHashCode () ^ 0x7F;
			if (Value != null)
				v += Value.GetHashCode ();
			return v;
		}
#endif
	}

}

