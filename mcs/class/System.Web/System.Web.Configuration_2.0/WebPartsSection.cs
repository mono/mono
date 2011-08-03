//
// System.Web.Configuration.WebPartsSection
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

	public sealed class WebPartsSection : ConfigurationSection
	{
		static ConfigurationProperty enableExportProp;
		static ConfigurationProperty personalizationProp;
		static ConfigurationProperty transformersProp;
		static ConfigurationPropertyCollection properties;

		static WebPartsSection ()
		{
			enableExportProp = new ConfigurationProperty ("enableExport", typeof (bool), false);
			personalizationProp = new ConfigurationProperty ("personalization", typeof (WebPartsPersonalization), null,
									 null, PropertyHelper.DefaultValidator,
									 ConfigurationPropertyOptions.None);
			transformersProp = new ConfigurationProperty ("transformers", typeof (TransformerInfoCollection), null,
								      null, PropertyHelper.DefaultValidator,
								      ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (enableExportProp);
			properties.Add (personalizationProp);
			properties.Add (transformersProp);
		}

		[MonoTODO ("why override this?")]
		protected internal override object GetRuntimeObject ()
		{
			return this;
		}

		[ConfigurationProperty ("enableExport", DefaultValue = "False")]
		public bool EnableExport {
			get { return (bool) base [enableExportProp];}
			set { base[enableExportProp] = value; }
		}

		[ConfigurationProperty ("personalization")]
		public WebPartsPersonalization Personalization {
			get { return (WebPartsPersonalization) base [personalizationProp];}
		}

		[ConfigurationProperty ("transformers")]
		public TransformerInfoCollection Transformers {
			get { return (TransformerInfoCollection) base [transformersProp];}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

