//
// System.Web.Configuration.AuthenticationSection
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
	public sealed class AuthenticationSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty formsProp;
		static ConfigurationProperty passportProp;
		static ConfigurationProperty modeProp;
		
		static AuthenticationSection ()
		{
			formsProp = new ConfigurationProperty ("forms", typeof(FormsAuthenticationConfiguration), null,
							       null,
							       PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.None);
			passportProp = new ConfigurationProperty ("passport", typeof(PassportAuthentication), null,
								  null,
								  PropertyHelper.DefaultValidator,
								  ConfigurationPropertyOptions.None);
			modeProp = new ConfigurationProperty ("mode", typeof(AuthenticationMode), AuthenticationMode.Windows,
							      new GenericEnumConverter (typeof (AuthenticationMode)),
							      PropertyHelper.DefaultValidator,
							      ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (formsProp);
			properties.Add (passportProp);
			properties.Add (modeProp);
		}
		
		public AuthenticationSection ()
		{
		}

		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}

		[ConfigurationProperty ("forms")]
		public FormsAuthenticationConfiguration Forms {
			get { return (FormsAuthenticationConfiguration) base [formsProp]; }
		}
		
		[ConfigurationProperty ("passport")]
#if NET_4_0
		[Obsolete ("This property is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
#endif
		public PassportAuthentication Passport {
			get { return (PassportAuthentication) base [passportProp]; }
		}
		
		[ConfigurationProperty ("mode", DefaultValue = "Windows")]
		public AuthenticationMode Mode {
			get { return (AuthenticationMode) base [modeProp]; }
			set { base [modeProp] = value; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}
}

#endif
