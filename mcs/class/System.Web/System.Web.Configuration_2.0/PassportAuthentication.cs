//
// System.Web.Configuration.PassportAuthentication
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Configuration
{
#if NET_4_0
	[Obsolete ("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
#endif
	public sealed class PassportAuthentication : ConfigurationElement
	{
		static ConfigurationProperty redirectUrlProp;
		static ConfigurationPropertyCollection properties;

		static ConfigurationElementProperty elementProperty;

		static PassportAuthentication ()
		{
			redirectUrlProp = new ConfigurationProperty ("redirectUrl", typeof (string), "internal");
			properties = new ConfigurationPropertyCollection ();

			properties.Add (redirectUrlProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (PassportAuthentication), ValidateElement));
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		[StringValidator] /* why is this here? */
		[ConfigurationProperty ("redirectUrl", DefaultValue = "internal")]
		public string RedirectUrl {
			get { return (string) base [redirectUrlProp];}
			set { base[redirectUrlProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

