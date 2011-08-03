//
// System.Web.Configuration.IdentitySection
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

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class IdentitySection : ConfigurationSection
	{
		static ConfigurationProperty impersonateProp;
		static ConfigurationProperty passwordProp;
		static ConfigurationProperty userNameProp;
		static ConfigurationPropertyCollection properties;

		static IdentitySection ()
		{
			impersonateProp = new ConfigurationProperty ("impersonate", typeof (bool), false);
			passwordProp = new ConfigurationProperty ("password", typeof (string), "");
			userNameProp = new ConfigurationProperty ("userName", typeof (string), "");
			properties = new ConfigurationPropertyCollection ();

			properties.Add (impersonateProp);
			properties.Add (passwordProp);
			properties.Add (userNameProp);
		}

		[MonoTODO ("why override this?")]
		protected internal override object GetRuntimeObject ()
		{
			return this;
		}

		protected internal override void Reset (ConfigurationElement parentElement)
		{
		}

		protected internal override void Unmerge (ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
		{
		}

		[ConfigurationProperty ("impersonate", DefaultValue = "False")]
		public bool Impersonate {
			get { return (bool) base [impersonateProp];}
			set { base[impersonateProp] = value; }
		}

		[ConfigurationProperty ("password", DefaultValue = "")]
		public string Password {
			get { return (string) base [passwordProp];}
			set { base[passwordProp] = value; }
		}

		[ConfigurationProperty ("userName", DefaultValue = "")]
		public string UserName {
			get { return (string) base [userNameProp];}
			set { base[userNameProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

