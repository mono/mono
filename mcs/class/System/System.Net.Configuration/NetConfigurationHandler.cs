//
// System.Net.Configuration.NetConfigurationHandler.cs
//
// Authors:
//	Jerome Laban (jlaban@wanadoo.fr)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Configuration;
#if (XML_DEP)
using System.Xml;
#endif

namespace System.Net.Configuration
{
	class NetConfigurationHandler : IConfigurationSectionHandler
	{
#if (XML_DEP)
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			NetConfig config = new NetConfig();

			XmlNode node = section.SelectSingleNode("ipv6");
			if(node != null) {
				XmlAttribute attrib = node.Attributes["enabled"];
				if(attrib != null)
					config.ipv6Enabled = String.Compare (attrib.Value, "true", true) == 0;
			}

			return config;
		}
#endif
	}
}