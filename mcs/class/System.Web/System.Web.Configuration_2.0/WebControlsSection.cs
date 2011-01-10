//
// System.Web.Configuration.WebControlsSection
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
using System.Collections;
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class WebControlsSection : ConfigurationSection
	{
		static ConfigurationProperty clientScriptsLocationProp;
		static ConfigurationPropertyCollection properties;

		static WebControlsSection ()
		{
			clientScriptsLocationProp = new ConfigurationProperty ("clientScriptsLocation", typeof (string), "/aspnet_client/{0}/{1}/",
									       TypeDescriptor.GetConverter (typeof (string)),
									       PropertyHelper.NonEmptyStringValidator,
									       ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (clientScriptsLocationProp);
		}

		protected internal override object GetRuntimeObject ()
		{
			Hashtable ht = new Hashtable ();

			ht.Add ("clientScriptsLocation", ClientScriptsLocation);

			return ht;
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("clientScriptsLocation", DefaultValue = "/aspnet_client/{0}/{1}/", Options = ConfigurationPropertyOptions.IsRequired)]
		public string ClientScriptsLocation {
			get { return (string) base [clientScriptsLocationProp];}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

