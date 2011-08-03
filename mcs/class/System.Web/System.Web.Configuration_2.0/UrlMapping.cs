//
// System.Web.Configuration.UrlMapping
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
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class UrlMapping : ConfigurationElement
	{
		static ConfigurationProperty mappedUrlProp;
		static ConfigurationProperty urlProp;
		static ConfigurationPropertyCollection properties;

		static void ValidateUrl (object value)
		{
			string url = value as string;
			if (String.IsNullOrEmpty (url))
				return;
			if (!VirtualPathUtility.IsAppRelative (url))
				throw new ConfigurationException ("Only app-relative (~/) URLs are allowed");
		}

		static UrlMapping ()
		{
			mappedUrlProp = new ConfigurationProperty ("mappedUrl", typeof (string), null,
								   PropertyHelper.WhiteSpaceTrimStringConverter,
								   PropertyHelper.NonEmptyStringValidator,
								   ConfigurationPropertyOptions.IsRequired);
			urlProp = new ConfigurationProperty ("url", typeof (string), null,
							     PropertyHelper.WhiteSpaceTrimStringConverter,
							     new CallbackValidator (typeof (string), ValidateUrl),
							     ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (mappedUrlProp);
			properties.Add (urlProp);
		}

		internal UrlMapping ()
		{
		}

		public UrlMapping (string url, string mappedUrl)
		{
			this.Url = url;
			this.MappedUrl = mappedUrl;
		}

		[ConfigurationProperty ("mappedUrl", Options = ConfigurationPropertyOptions.IsRequired)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string MappedUrl {
			get { return (string) base [mappedUrlProp];}
			internal set { base [mappedUrlProp] = value;}
		}

		[ConfigurationProperty ("url", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Url {
			get { return (string) base [urlProp];}
			internal set { base [urlProp] = value;}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
