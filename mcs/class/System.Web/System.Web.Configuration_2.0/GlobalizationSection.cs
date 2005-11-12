/* header comment goes here */

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

		public void PostDeserialize ()
		{
		}

		public void PreSerialize (XmlWriter writer)
		{
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

