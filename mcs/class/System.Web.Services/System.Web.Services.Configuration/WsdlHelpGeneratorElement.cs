//
// System.Web.Services.Configuration.WsdlHelpGeneratorElement
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Services.Configuration {

	public sealed class WsdlHelpGeneratorElement : ConfigurationElement
	{
		static ConfigurationProperty hrefProp;
		static ConfigurationPropertyCollection properties;

		static WsdlHelpGeneratorElement ()
		{
			hrefProp = new ConfigurationProperty ("href", typeof (string), null, ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (hrefProp);

		}

		public WsdlHelpGeneratorElement ()
		{
		}

		[MonoTODO ("probably verifies the Href property here, after deserializing?")]
		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			base.DeserializeElement (reader, serializeCollectionKey);
		}

		[MonoTODO]
		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);
		}

		[ConfigurationProperty ("href", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Href {
			get { return (string) base [hrefProp];}
			set { base[hrefProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

