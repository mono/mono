//
// System.Web.Configuration.FormsAuthenticationUser
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

using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class FormsAuthenticationUser : ConfigurationElement
	{
		static ConfigurationProperty nameProp;
		static ConfigurationProperty passwordProp;
		static ConfigurationPropertyCollection properties;

		static FormsAuthenticationUser ()
		{
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      new LowerCaseStringConverter (),
							      PropertyHelper.DefaultValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			passwordProp = new ConfigurationProperty ("password", typeof (string), "", ConfigurationPropertyOptions.IsRequired);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (nameProp);
			properties.Add (passwordProp);
		}

		public FormsAuthenticationUser (string name, string password)
		{
			this.Name = name;
			this.Password = password;
		}

		[StringValidator]
		[TypeConverter (typeof (LowerCaseStringConverter))]
		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base[nameProp]; }
			set { base[nameProp] = value; }
		}

		[StringValidator]
		[ConfigurationProperty ("password", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Password {
			get { return (string) base[passwordProp]; }
			set { base[passwordProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
