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

					AuthenticationManager.Clear ();
					continue;
				}

				string type = HandlersUtil.ExtractAttributeValue ("type", child);
				if (child.Attributes != null && child.Attributes.Count != 0)
					HandlersUtil.ThrowException ("Unrecognized attribute", child);

				if (name == "add") {
					AuthenticationManager.Register (CreateInstance (type, child));
					continue;
				}

				if (name == "remove") {
					AuthenticationManager.Unregister (CreateInstance (type, child));
					continue;
				}

				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return AuthenticationManager.RegisteredModules;
		}

		static IAuthenticationModule CreateInstance (string typeName, XmlNode node)
		{
			IAuthenticationModule module = null;
			
			try {
				Type type = Type.GetType (typeName, true);
				module = (IAuthenticationModule) Activator.CreateInstance (type);
			} catch (Exception e) {
				HandlersUtil.ThrowException (e.Message, node);
			}

			return module;
		}
	}
}

