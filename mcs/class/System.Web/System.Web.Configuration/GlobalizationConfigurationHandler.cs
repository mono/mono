//
// System.Web.Configuration.GlobalizationConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Configuration;
using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Web.Configuration
{
	class GlobalizationConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			GlobalizationConfiguration config = new GlobalizationConfiguration (parent);

			if (section.HasChildNodes)
				ThrowException ("No child nodes allowed here.", section);

			string attvalue = AttValue ("requestEncoding", section, true);
			if (attvalue != null)
				config.RequestEncoding = GetEncoding (section, "requestEncoding", attvalue);

			attvalue = AttValue ("responseEncoding", section, true);
			if (attvalue != null)
				config.ResponseEncoding = GetEncoding (section, "responseEncoding", attvalue);

			attvalue = AttValue ("fileEncoding", section, true);
			if (attvalue != null)
				config.FileEncoding = GetEncoding (section, "fileEncoding", attvalue);

			attvalue = AttValue ("culture", section, true);
			if (attvalue != null)
				config.Culture = GetCulture (section, "culture", attvalue);

			attvalue = AttValue ("uiculture", section, true);
			if (attvalue != null)
				config.UICulture = GetCulture (section, "uiculture", attvalue);

			if (section.Attributes == null || section.Attributes.Count != 0)
				ThrowException ("Unknown attribute(s).", section);

			return config;
		}

		static Encoding GetEncoding (XmlNode section, string att, string enc)
		{
			Encoding encoding = null;
			try {
				encoding = Encoding.GetEncoding (enc);
			} catch {
				string msg = String.Format ("Error getting encoding {0} for {1}", enc, att);
				ThrowException (msg, section);
			}

			return encoding;
		}
		
		static CultureInfo GetCulture (XmlNode section, string att, string cul)
		{
			CultureInfo culture = null;
			try {
				culture = new CultureInfo (cul);
			} catch {
				string msg = String.Format ("Error getting culture {0} for {1}", cul, att);
				ThrowException (msg, section);
			}

			return culture;
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

