//
// System.Configuration.KeyValueConfigurationElement.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Xml;

namespace System.Configuration
{
	public class KeyValueConfigurationElement: ConfigurationElement
	{
		static ConfigurationProperty keyProp;
		static ConfigurationProperty valueProp;
		static ConfigurationPropertyCollection properties;

		static KeyValueConfigurationElement ()
		{
			keyProp = new ConfigurationProperty ("key", typeof (string), "", ConfigurationPropertyOptions.IsKey);
			valueProp = new ConfigurationProperty ("value", typeof (string), "");

			properties = new ConfigurationPropertyCollection ();
			properties.Add (keyProp);
			properties.Add (valueProp);
		}

		internal KeyValueConfigurationElement ()
		{
		}

		public KeyValueConfigurationElement (string key, string value)
		{
			this[keyProp] = key;
			this[valueProp] = value;
		}
		
		[ConfigurationProperty ("key", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		public string Key {
			get { return (string) this[keyProp]; }
		}
		
		[ConfigurationProperty ("value", DefaultValue = "")]
		public string Value {
			get { return (string) this[valueProp]; }
			set { this [valueProp] = value; }
		}

		[MonoTODO]
		protected internal override void Init ()
		{
		}
		
		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

