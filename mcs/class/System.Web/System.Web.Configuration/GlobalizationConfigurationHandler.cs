//
// System.Web.Configuration.GlobalizationConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

using System.Configuration;
using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Web.Configuration
{
	class GlobalizationConfigurationHandler : IConfigurationSectionHandler
	{
		static bool encoding_warning;
		static bool culture_warning;

		public object Create (object parent, object configContext, XmlNode section)
		{
			GlobalizationConfiguration config = new GlobalizationConfiguration (parent);

			if (section.HasChildNodes)
				ThrowException ("No child nodes allowed here.", section);

			string attvalue = AttValue ("requestEncoding", section, true);
			if (attvalue == null)
				config.RequestEncoding = Encoding.Default;
			else
				config.RequestEncoding = GetEncoding (section, "requestEncoding", attvalue);

			attvalue = AttValue ("responseEncoding", section, true);
			if (attvalue == null)
				config.ResponseEncoding = Encoding.Default;
			else
				config.ResponseEncoding = GetEncoding (section, "responseEncoding", attvalue);

			attvalue = AttValue ("fileEncoding", section, true);
			if (attvalue == null)
				config.FileEncoding = Encoding.Default;
			else
				config.FileEncoding = GetEncoding (section, "fileEncoding", attvalue);

			attvalue = AttValue ("culture", section, true);
			if (attvalue != null)
				config.Culture = GetCulture (section, "culture", attvalue);

			attvalue = AttValue ("uiCulture", section, true);
			if (attvalue != null)
				config.UICulture = GetCulture (section, "uiCulture", attvalue);

			if (section.Attributes == null || section.Attributes.Count != 0)
				ThrowException ("Unknown attribute(s).", section);

			return config;
		}

		static Encoding GetEncoding (XmlNode section, string att, string enc)
		{
			Encoding encoding = null;
			try {
				switch (enc.ToLower ()) {
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
					encoding = new UTF8Encoding (false, false);
					break;
				default:
					encoding = Encoding.GetEncoding (enc);
					break;
				}
			} catch {
				EncodingFailed (section, att, enc);
				encoding = new UTF8Encoding (false, false);
			}

			return encoding;
		}
		
		static CultureInfo GetCulture (XmlNode section, string att, string cul)
		{
			CultureInfo culture = null;
			try {
				culture = new CultureInfo (cul);
			} catch {
				CultureFailed (section, att, cul);
				culture = new CultureInfo (0x007f); // Invariant
			}

			return culture;
		}
		
		static void EncodingFailed (XmlNode section, string att, string enc)
		{
			if (encoding_warning)
				return;

			encoding_warning = true;
			Console.WriteLine ("Encoding {1} cannot be loaded. Perhaps your runtime \n" +
					   "don't have ICU support?\n{0}=\"{1}\"\n", att, enc);
		}

		static void CultureFailed (XmlNode section, string att, string cul)
		{
			if (culture_warning)
				return;

			culture_warning = true;
			Console.WriteLine ("Culture {1} cannot be loaded. Perhaps your runtime \n" +
					   "don't have ICU support?\n{0}=\"{1}\"\n", att, cul);
		}

		// A few methods to save some typing
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}

