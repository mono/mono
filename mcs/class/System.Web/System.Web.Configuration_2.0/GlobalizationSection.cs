//
// System.Web.Configuration.GlobalizationSection
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
using System.Text;
using System.Xml;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class GlobalizationSection : ConfigurationSection
	{
		static ConfigurationProperty cultureProp;
		static ConfigurationProperty enableBestFitResponseEncodingProp;
		static ConfigurationProperty enableClientBasedCultureProp;
		static ConfigurationProperty fileEncodingProp;
		static ConfigurationProperty requestEncodingProp;
		static ConfigurationProperty resourceProviderFactoryTypeProp;
		static ConfigurationProperty responseEncodingProp;
		static ConfigurationProperty responseHeaderEncodingProp;
		static ConfigurationProperty uiCultureProp;
		static ConfigurationPropertyCollection properties;

		static GlobalizationSection ()
		{
			cultureProp = new ConfigurationProperty ("culture", typeof (string), "");
			enableBestFitResponseEncodingProp = new ConfigurationProperty ("enableBestFitResponseEncoding", typeof (bool), false);
			enableClientBasedCultureProp = new ConfigurationProperty ("enableClientBasedCulture", typeof (bool), false);
			fileEncodingProp = new ConfigurationProperty ("fileEncoding", typeof (Encoding));
			requestEncodingProp = new ConfigurationProperty ("requestEncoding", typeof (Encoding), Encoding.UTF8);
			resourceProviderFactoryTypeProp = new ConfigurationProperty ("resourceProviderFactoryType", typeof (string), "");
			responseEncodingProp = new ConfigurationProperty ("responseEncoding", typeof (Encoding), Encoding.UTF8);
			responseHeaderEncodingProp = new ConfigurationProperty ("responseHeaderEncoding", typeof (Encoding), Encoding.UTF8);
			uiCultureProp = new ConfigurationProperty ("uiCulture", typeof (string), "");
			properties = new ConfigurationPropertyCollection ();

			properties.Add (cultureProp);
			properties.Add (enableBestFitResponseEncodingProp);
			properties.Add (enableClientBasedCultureProp);
			properties.Add (fileEncodingProp);
			properties.Add (requestEncodingProp);
			properties.Add (resourceProviderFactoryTypeProp);
			properties.Add (responseEncodingProp);
			properties.Add (responseHeaderEncodingProp);
			properties.Add (uiCultureProp);
		}

		[MonoTODO]
		protected override void PostDeserialize ()
		{
			base.PostDeserialize();
		}

		[MonoTODO]
		protected override void PreSerialize (XmlWriter writer)
		{
			base.PostDeserialize();
		}

		[ConfigurationProperty ("culture", DefaultValue = "")]
		public string Culture {
			get { return (string) base [cultureProp];}
			set { base[cultureProp] = value; }
		}

		[ConfigurationProperty ("enableBestFitResponseEncoding", DefaultValue = "False")]
		public bool EnableBestFitResponseEncoding {
			get { return (bool) base [enableBestFitResponseEncodingProp];}
			set { base[enableBestFitResponseEncodingProp] = value; }
		}

		[ConfigurationProperty ("enableClientBasedCulture", DefaultValue = "False")]
		public bool EnableClientBasedCulture {
			get { return (bool) base [enableClientBasedCultureProp];}
			set { base[enableClientBasedCultureProp] = value; }
		}

		[ConfigurationProperty ("fileEncoding")]
		public Encoding FileEncoding {
			get { return (Encoding) base [fileEncodingProp];}
			set { base[fileEncodingProp] = value; }
		}

		[ConfigurationProperty ("requestEncoding", DefaultValue = "utf-8")]
		public Encoding RequestEncoding {
			get { return (Encoding) base [requestEncodingProp];}
			set { base[requestEncodingProp] = value; }
		}

		[ConfigurationProperty ("resourceProviderFactoryType", DefaultValue = "")]
		public string ResourceProviderFactoryType {
			get { return (string) base [resourceProviderFactoryTypeProp];}
			set { base[resourceProviderFactoryTypeProp] = value; }
		}

		[ConfigurationProperty ("responseEncoding", DefaultValue = "utf-8")]
		public Encoding ResponseEncoding {
			get { return (Encoding) base [responseEncodingProp];}
			set { base[responseEncodingProp] = value; }
		}

		[ConfigurationProperty ("responseHeaderEncoding", DefaultValue = "utf-8")]
		public Encoding ResponseHeaderEncoding {
			get { return (Encoding) base [responseHeaderEncodingProp];}
			set { base[responseHeaderEncodingProp] = value; }
		}

		[ConfigurationProperty ("uiCulture", DefaultValue = "")]
		public string UICulture {
			get { return (string) base [uiCultureProp];}
			set { base[uiCultureProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

