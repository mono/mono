//
// System.Net.Configuration.NetConfigurationHandler.cs
//
// Authors:
//	Jerome Laban (jlaban@wanadoo.fr)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			NetConfig config = new NetConfig ();

			XmlNodeList reqHandlers = section.ChildNodes;
			foreach (XmlNode child in reqHandlers) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "ipv6") {
					string enabled = HandlersUtil.ExtractAttributeValue ("enabled", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (enabled == "true")
						config.ipv6Enabled = true;
					else if (enabled != "false")
						HandlersUtil.ThrowException ("Invalid boolean value", child);
						
					continue;
				}

				if (name == "httpWebRequest") {
					string value = HandlersUtil.ExtractAttributeValue
								("maximumResponseHeadersLength", child, false);

					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					try {
						int val = Int32.Parse (value.Trim ());
						if (val < -1)
							HandlersUtil.ThrowException ("Must be -1 or >= 0", child);

						config.MaxResponseHeadersLength = val;
					} catch (Exception e) {
						HandlersUtil.ThrowException ("Invalid int value", child);
					}

					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return config;
		}
#endif
	}
}
