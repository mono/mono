//
// System.Net.Configuration.WebRequestModuleHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Net.Configuration
{
	class WebRequestModuleHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList reqHandlers = section.ChildNodes;
			foreach (XmlNode child in reqHandlers) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					WebRequest.ClearPrefixes ();
					continue;
				}

				string prefix = HandlersUtil.ExtractAttributeValue ("prefix", child);
				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					WebRequest.AddPrefix (prefix, type);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					WebRequest.RemovePrefix (prefix);
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return null;
		}
	}
}

