//
// System.Web.Configuration.CustomErrorsSection
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
using System.Configuration;
using System.Xml;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class CustomErrorsSection : ConfigurationSection
	{
		static ConfigurationProperty defaultRedirectProp;
		static ConfigurationProperty errorsProp;
		static ConfigurationProperty modeProp;
		static ConfigurationPropertyCollection properties;

		static CustomErrorsSection ()
		{
			defaultRedirectProp = new ConfigurationProperty ("defaultRedirect", typeof (string), null);
			errorsProp = new ConfigurationProperty ("", typeof (CustomErrorCollection), null,
								null, PropertyHelper.DefaultValidator,
								ConfigurationPropertyOptions.IsDefaultCollection);
			modeProp = new ConfigurationProperty ("mode", typeof (CustomErrorsMode), CustomErrorsMode.RemoteOnly,
							      new GenericEnumConverter (typeof (CustomErrorsMode)),
							      PropertyHelper.DefaultValidator,
							      ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (defaultRedirectProp);
			properties.Add (errorsProp);
			properties.Add (modeProp);

		}

		// Why override?
		protected internal override void DeserializeSection (XmlReader reader)
		{
			base.DeserializeSection (reader);
		}

		// Why override?
		protected internal override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}
		
		[ConfigurationProperty ("defaultRedirect")]
		public string DefaultRedirect {
			get { return (string) base [defaultRedirectProp];}
			set { base[defaultRedirectProp] = value; }
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public CustomErrorCollection Errors {
			get { return (CustomErrorCollection) base [errorsProp];}
		}

		[ConfigurationProperty ("mode", DefaultValue = "RemoteOnly")]
		public CustomErrorsMode Mode {
			get { return (CustomErrorsMode) base [modeProp];}
			set { base[modeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

