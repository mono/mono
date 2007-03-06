//
// System.Web.Configuration.BackendProvidersSection
//
// Authors:
//      Marek Habersack <grendello@gmail.com>
//
// (C) 2007 Marek Habersack
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
using System.Web.Configuration;

namespace Mainsoft.Web.Configuration
{
	public class BackendProvidersSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty backendsProp;
		
		static BackendProvidersSection ()
		{
			backendsProp = new ConfigurationProperty ("backends", typeof (BackendProviderCollection), null,
								  null, new DefaultValidator (),
								  ConfigurationPropertyOptions.None);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (backendsProp);
		}

		public BackendProvidersSection ()
		{
		}

		[ConfigurationProperty ("backends")]
		public BackendProviderCollection Backends {
			get { return (BackendProviderCollection) base [backendsProp]; }
			set { base [backendsProp] = value; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
#endif
