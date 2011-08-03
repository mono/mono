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
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Web.Util;

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
			fileEncodingProp = new ConfigurationProperty ("fileEncoding", typeof (string));
			requestEncodingProp = new ConfigurationProperty ("requestEncoding", typeof (string), "utf-8");
			resourceProviderFactoryTypeProp = new ConfigurationProperty ("resourceProviderFactoryType", typeof (string), "");
			responseEncodingProp = new ConfigurationProperty ("responseEncoding", typeof (string), "utf-8");
			responseHeaderEncodingProp = new ConfigurationProperty ("responseHeaderEncoding", typeof (string), "utf-8");
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

		public GlobalizationSection ()
		{
			encodingHash = new Hashtable ();
		}

		void VerifyData ()
		{
			bool fake = false;
			try {
				GetSanitizedCulture (Culture, ref fake);
			}
			catch {
				throw new ConfigurationErrorsException ("the <globalization> tag contains an invalid value for the 'culture' attribute");
			}

			try {
				GetSanitizedCulture (UICulture, ref fake);
			}
			catch {
				throw new ConfigurationErrorsException ("the <globalization> tag contains an invalid value for the 'uiCulture' attribute");
			}
		}

		protected override void PostDeserialize ()
		{
			base.PostDeserialize();

			VerifyData ();
		}

		protected override void PreSerialize (XmlWriter writer)
		{
			base.PreSerialize(writer);

			VerifyData ();
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
			get { return GetEncoding (fileEncodingProp, ref cached_fileencoding); }
			set { base[fileEncodingProp] = value.WebName; }
		}

		[ConfigurationProperty ("requestEncoding", DefaultValue = "utf-8")]
		public Encoding RequestEncoding {
			get { return GetEncoding (requestEncodingProp, ref cached_requestencoding); }
			set { base[requestEncodingProp] = value.WebName; }
		}

		[ConfigurationProperty ("resourceProviderFactoryType", DefaultValue = "")]
		public string ResourceProviderFactoryType {
			get { return (string) base [resourceProviderFactoryTypeProp];}
			set { base[resourceProviderFactoryTypeProp] = value; }
		}

		[ConfigurationProperty ("responseEncoding", DefaultValue = "utf-8")]
		public Encoding ResponseEncoding {
			get { return GetEncoding (responseEncodingProp, ref cached_responseencoding); }
			set { base[responseEncodingProp] = value.WebName; }
		}

		[ConfigurationProperty ("responseHeaderEncoding", DefaultValue = "utf-8")]
		public Encoding ResponseHeaderEncoding {
			get { return GetEncoding (responseHeaderEncodingProp, ref cached_responseheaderencoding); }
			set { base[responseHeaderEncodingProp] = value.WebName; }
		}

		[ConfigurationProperty ("uiCulture", DefaultValue = "")]
		public string UICulture {
			get { return (string) base [uiCultureProp];}
			set { base[uiCultureProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

#region CompatabilityCode
		string cached_fileencoding;
		string cached_requestencoding;
		string cached_responseencoding;
		string cached_responseheaderencoding;
		Hashtable encodingHash;

		string cached_culture;
		CultureInfo cached_cultureinfo;

		string cached_uiculture;
		CultureInfo cached_uicultureinfo;

		static bool encoding_warning;
		static bool culture_warning;

		bool autoCulture;
		bool autoUICulture;

		internal bool IsAutoCulture {
			get { return autoCulture; }
		}

		internal bool IsAutoUICulture {
			get { return autoUICulture; }
		}
		
		CultureInfo GetSanitizedCulture (string culture, ref bool auto)
		{
			auto = false;
			if (culture == null)
				throw new ArgumentNullException ("culture");
			if (culture.Length <= 3)
				return new CultureInfo (culture);
			if (culture.StartsWith ("auto")) {
				auto = true;
				if (culture.Length > 5 && culture[4] == ':')
					return new CultureInfo (culture.Substring (5));
				return Helpers.InvariantCulture;// (0x007f);
			}

			return new CultureInfo (culture);
		}
				
		internal CultureInfo GetUICulture ()
		{
			string uiculture = UICulture;
			if (cached_uiculture != uiculture) {
				try {
					cached_uicultureinfo = GetSanitizedCulture (uiculture, ref autoUICulture);
					cached_uiculture = uiculture;
				} catch {
					CultureFailed ("UICulture", uiculture);
					cached_uicultureinfo = new CultureInfo (0x007f); // Invariant
					cached_uiculture = null;
				}
			}

			return cached_uicultureinfo;
		}

		internal CultureInfo GetCulture ()
		{
			string culture = Culture;
			if (cached_culture != culture) {
				try {
					cached_cultureinfo = GetSanitizedCulture (culture, ref autoCulture);
					cached_culture = culture;
				} catch {
					CultureFailed ("Culture", culture);
					cached_cultureinfo = new CultureInfo (0x007f); // Invariant
					cached_culture = null;
				}
			}

			return cached_cultureinfo;
		}

		Encoding GetEncoding (ConfigurationProperty prop, ref string cached_encoding_name)
		{
			string enc = (string) base [prop];
			if (cached_encoding_name == null)
				cached_encoding_name = ((enc == null) ? "utf-8" : enc);

			Encoding encoding = (Encoding)encodingHash [prop];
			if (encoding == null || encoding.WebName != cached_encoding_name) {
				try {
					switch (cached_encoding_name.ToLower (Helpers.InvariantCulture)) {
					case "utf-16le":
					case "utf-16":
					case "ucs-2":
					case "unicode":
					case "iso-10646-ucs-2":
						encoding = new UnicodeEncoding (false, true);
						break;
					case "utf-16be":
					case "unicodefffe":
						encoding = new UnicodeEncoding (true, true);
						break;
					case "utf-8":
					case "unicode-1-1-utf-8":
					case "unicode-2-0-utf-8":
					case "x-unicode-1-1-utf-8":
					case "x-unicode-2-0-utf-8":
						encoding = Encoding.UTF8;
						break;
					default:
						encoding = Encoding.GetEncoding (cached_encoding_name);
						break;
					}
				} catch {
					EncodingFailed (prop.Name, cached_encoding_name);
					encoding = new UTF8Encoding (false, false);
				}
			}

			encodingHash[prop] = encoding;
			cached_encoding_name = encoding.WebName;

			return encoding;
		}

		static void EncodingFailed (string att, string enc)
		{
			if (encoding_warning)
				return;

			encoding_warning = true;
			Console.WriteLine ("Encoding {1} cannot be loaded.\n" +
					   "{0}=\"{1}\"\n", att, enc);
		}

		static void CultureFailed (string att, string cul)
		{
			if (culture_warning)
				return;

			culture_warning = true;
			Console.WriteLine ("Culture {1} cannot be loaded. Perhaps your runtime \n" +
					   "don't have ICU support?\n{0}=\"{1}\"\n", att, cul);
		}

#endregion
	}

}

#endif

