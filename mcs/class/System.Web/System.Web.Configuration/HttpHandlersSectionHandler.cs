//
// System.Web.Configuration.HttpHandlersSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class HttpHandlersSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			HandlerFactoryConfiguration mapper;
			
			if (parent is HandlerFactoryConfiguration)
				mapper = new HandlerFactoryConfiguration ((HandlerFactoryConfiguration) parent);
			else
				mapper = new HandlerFactoryConfiguration ();
			
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

					mapper.Clear ();
					continue;
				}
					
				string verb = HandlersUtil.ExtractAttributeValue ("verb", child);
				string path = HandlersUtil.ExtractAttributeValue ("path", child);
				string validateStr = HandlersUtil.ExtractAttributeValue ("validate", child, true);
				bool validate;
				if (validateStr == null) {
					validate = true;
				} else {
					validate = validateStr == "true";
					if (!validate && validateStr != "false")
						HandlersUtil.ThrowException (
								"Invalid value for validate attribute.", child);
				}

				if (name == "add") {
					string type = HandlersUtil.ExtractAttributeValue ("type", child);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					HandlerItem item = new HandlerItem (verb, path, type, validate);
					mapper.Add (item);
					continue;
				}

				if (name == "remove") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (validate && mapper.Remove (verb, path) == null)
						HandlersUtil.ThrowException ("There's no mapping to remove", child);
					
					continue;
				}
				HandlersUtil.ThrowException ("Unexpected element", child);
			}

			return mapper;
		}
	}

	internal class HandlersUtil
	{
		private HandlersUtil ()
		{
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node)
		{
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
		{
			XmlNode att = node.Attributes.RemoveNamedItem (attKey);
			if (att == null) {
				if (optional)
					return null;
				ThrowException ("Required attribute not found: " + attKey, node);
			}

			string value = att.Value;
			if (value == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " attribute is empty: " + attKey, node);
			}

			return value;
		}

		static internal void ThrowException (string msg, XmlNode node)
		{
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";
			throw new ConfigurationException (msg, node);
		}
	}
}

