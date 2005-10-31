//
// System.Web.Configuration.HttpHandlerAction
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
	public sealed class HttpHandlerAction: ConfigurationElement
	{
		static ConfigurationPropertyCollection _properties;
		static ConfigurationProperty pathProp;
		static ConfigurationProperty typeProp;
		static ConfigurationProperty validateProp;
		static ConfigurationProperty verbProp;

		static HttpHandlerAction ()
		{
			pathProp = new ConfigurationProperty ("path", typeof (string), null,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			typeProp = new ConfigurationProperty ("type", typeof (string), null,
							      ConfigurationPropertyOptions.IsRequired);
			validateProp = new ConfigurationProperty ("validate", typeof (bool), true);
			verbProp = new ConfigurationProperty ("verb", typeof (string), null, ConfigurationPropertyOptions.IsRequired);

			_properties = new ConfigurationPropertyCollection ();
			_properties.Add (pathProp);
			_properties.Add (typeProp);
			_properties.Add (validateProp);
			_properties.Add (verbProp);
		}

		public HttpHandlerAction (string path, string type, string verb)
			: this (path, type, verb, true)
		{ }

		public HttpHandlerAction (string path, string type, string verb, bool validate)
		{
			this.path = path;
			this.type = type;
			this.verb = verb;
			this.validate = validate;
		}

		[ConfigurationProperty ("path", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Path {
			get { return (string) base[pathProp]; }
			set { base[pathProp] = value; }
		}

		[ConfigurationProperty ("type", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string) base[typeProp]; }
			set { base[typeProp] = value; }
		}

		[ConfigurationProperty ("validate", DefaultValue = true)]
		public bool Validate {
			get { return (bool) base[validateProp]; }
			set { base[validateProp] = value; }
		}

		[ConfigurationProperty ("verb", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Verb {
			get { return (string) base[verbProp]; }
			set { base[verbProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				return _properties;
			}
		}

		string path;
		string verb;
		string type;
		bool validate;
	}

}

#endif
