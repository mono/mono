//
// System.Net.Configuration.NetAuthenticationModuleHandler
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
	class NetAuthenticationModuleHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			ArrayList result = new ArrayList (parent as ArrayList);
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList httpHandlers = section.ChildNodes;
			foreach (XmlNode child in httpHandlers) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					result.Clear ();
					continue;
				}

				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					result.Add (type);
					continue;
				}

				if (name == "remove") {
					string mname = HandlersUtil.ExtractAttributeValue ("name", child);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					result.Remove (mname);
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return result;
		}
	}
}

