//
// System.Web.Configuration.HttpHandlersSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class HttpHandlersSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			HttpHandlerTypeMapper mapper;
			
			if (parent is HttpHandlerTypeMapper)
				mapper = new HttpHandlerTypeMapper ((HttpHandlerTypeMapper) parent);
			else
				mapper = new HttpHandlerTypeMapper ();
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				throw new ConfigurationException ("Unrecognized attribute", section);

			foreach (XmlNode child in section.ChildNodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;
				
				if (ntype != XmlNodeType.Element)
					throw new ConfigurationException ("Unexpected node type", child);

				string name = child.Name;
				if (name == "clear") {
					if (child.Attributes.Count != 0)
						throw new ConfigurationException ("Unrecognized attribute", child);

					mapper.Clear ();
					continue;
				}
					
				string verb = ExtractAttributeValue ("verb", child);
				string path = ExtractAttributeValue ("path", child);
				string validateStr = ExtractAttributeValue ("validate", child);
				bool validate = (validateStr == "true");
				if (!validate && validateStr != "false")
					throw new ConfigurationException ("Invalid value for validate attribute.",
									   child);

				if (name == "add") {
					string type = ExtractAttributeValue ("type", child);
					if (child.Attributes.Count != 0)
						throw new ConfigurationException ("Unrecognized attribute", child);

					HandlerItem item = new HandlerItem (verb, path, type, validate);
					mapper.Add (item);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes.Count != 0)
						throw new ConfigurationException ("Unrecognized attribute", child);

					if (validate && mapper.Remove (verb, path) == null)
						throw new ConfigurationException ("There's no mapping to remove",
										  child);
					
					continue;
				}
				throw new ConfigurationException ("Unexpected element", child);
			}

			return mapper;
		}

		static string ExtractAttributeValue (string attKey, XmlNode node)
		{
			XmlNode att = node.Attributes.RemoveNamedItem (attKey);
			if (att == null)
				throw new ConfigurationException ("Required attribute not found.", node);

			string value = att.Value;
			if (value == String.Empty)
				throw new ConfigurationException ("Required attribute is empty.", node);

			return value;
		}
	}
}

