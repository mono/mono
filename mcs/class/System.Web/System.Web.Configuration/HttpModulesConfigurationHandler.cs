//
// System.Web.Configuration.HttpModulesConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Configuration;
using System.Xml;
using System.Web.Security;

namespace System.Web.Configuration
{
	class HttpModulesConfigurationHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			ModulesConfiguration mapper;
			
			if (parent is ModulesConfiguration)
				mapper = new ModulesConfiguration ((ModulesConfiguration) parent);
			else
				mapper = new ModulesConfiguration ();
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			XmlNodeList httpModules = section.ChildNodes;

			foreach (XmlNode child in httpModules) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);

				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					mapper.Clear ();
					continue;
				}

				string nameAtt = HandlersUtil.ExtractAttributeValue ("name", child);
				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child);
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					// FIXME: gotta remove this. Just here to make it work with my local config
					if (type.StartsWith ("System.Web.Mobile"))
						continue;

					ModuleItem item = new ModuleItem (nameAtt, type);
					mapper.Add (item);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (mapper.Remove (nameAtt) == null)
						HandlersUtil.ThrowException ("Module not loaded", child);
					continue;
				}

				HandlersUtil.ThrowException ("Unrecognized element", child);
			}

			mapper.Add (new ModuleItem ("DefaultAuthentication", typeof (DefaultAuthenticationModule)));
			return mapper;
		}
	}
}

