//
// System.Configuration.NameValueFileSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace System.Configuration
{
	public class NameValueFileSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			XmlNode file = null;
			if (section.Attributes != null)
				file = section.Attributes.RemoveNamedItem ("file");

			NameValueCollection pairs = ConfigHelper.GetNameValueCollection (
									parent as NameValueCollection,
									section,
									"key",
									"value");

			if (file != null && file.Value == String.Empty) {
				if (!(file is IConfigXmlNode))
					return null;

				string fileName = ((IConfigXmlNode) file).Filename;
				string fullPath = Path.Combine (Path.GetDirectoryName (fileName), file.Value);
				if (!File.Exists (fullPath))
					return pairs;

				ConfigXmlDocument doc = new ConfigXmlDocument ();
				doc.Load (fullPath);
				if (doc.DocumentElement.Name != section.Name)
					throw new ConfigurationException ("Invalid root element", doc.DocumentElement);

				pairs = ConfigHelper.GetNameValueCollection (pairs, doc.DocumentElement,
									     "key", "value");
			}

			return pairs;
		}
	}
}

