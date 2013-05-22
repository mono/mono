//
// System.Configuration.NameValueConfigurationElement.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace System.Configuration {

	public sealed class NameValueConfigurationElement : ConfigurationElement
	{
		private static ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propValue;

		static NameValueConfigurationElement ()
		{
			_properties = new ConfigurationPropertyCollection ();
			
			_propName = new ConfigurationProperty ("name", typeof (string), "", ConfigurationPropertyOptions.IsKey);
			_propValue = new ConfigurationProperty ("value", typeof (string), "");

			_properties.Add (_propName);
			_properties.Add (_propValue);
		}

		public NameValueConfigurationElement (string name, string value)
		{
			this [_propName] = name;
			this [_propValue] = value;
		}

		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) this [_propName]; }
		}

		[ConfigurationProperty ("value", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
		public string Value {
			get { return (string) this [_propValue]; }
			set { this [_propValue] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return _properties; }
		}
	}
}

