//
// System.Web.Services.Configuration.WebServicesConfigurationSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Configuration;
using System.Xml;

namespace System.Web.Services.Configuration
{
	[Flags]
	enum WSProtocol
	{
		HttpSoap = 1,
		HttpPost = 1 << 1,
		HttpGet =  1 << 2,
		Documentation = 1 << 3,
		All = 0x0F
	}
	
	class WSConfig
	{
		WSProtocol protocols;
		string wsdlHelpPage;
		
		public WSConfig (WSConfig parent)
		{
			if (parent == null)
				return;
			
			protocols = parent.protocols;
			wsdlHelpPage = parent.wsdlHelpPage;
		}
		
		static WSProtocol ParseProtocol (string protoName, out string error)
		{
			WSProtocol proto;
			error = null;
			try {
				proto = (WSProtocol) Enum.Parse (typeof (WSProtocol), protoName);
			} catch {
				error = "Invalid protocol name";
				return 0;
			}

			return proto;
		}

		// Methods to modify configuration values
		public bool AddProtocol (string protoName, out string error)
		{
			if (protoName == "All") {
				error = "Invalid protocol name";
				return false;
			}

			WSProtocol proto = ParseProtocol (protoName, out error);
			if (error != null)
				return false;

			if ((protocols & proto) != 0) {
				error = "Protocol already added";
				return false;
			}

			protocols |= proto;
			return true;
		}

		// Methods to query/get configuration
		public bool IsSupported (WSProtocol proto)
		{
			return (proto & WSProtocol.All) == proto;
		}

		// Properties
		public string WsdlHelpPage {
			get { return wsdlHelpPage; }
			set { wsdlHelpPage = value; }
		}

	}
	
	class WebServicesConfigurationSectionHandler : IConfigurationSectionHandler
	{
		[MonoTODO("Some nodes not supported, see below")]
		public object Create (object parent, object context, XmlNode section)
		{
			WSConfig config = new WSConfig (parent as WSConfig);	

			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "protocols") {
					ConfigProtocols (child, config);
					continue;
				}

				if (name == "soapExtensionTypes") {
					//TODO: Not supported by now
					continue;
				}

				if (name == "soapExtensionReflectorTypes") {
					//TODO: Not supported by now
					continue;
				}

				if (name == "soapExtensionImporterTypes") {
					//TODO: Not supported by now
					continue;
				}

				if (name == "serviceDescriptionFormatExtensionTypes") {
					//TODO: Not supported by now
					continue;
				}

				if (name == "wsdlHelpGenerator") {
					string href = AttValue ("href", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					config.WsdlHelpPage = href;
					continue;
				}

				ThrowException ("Unexpected element", child);
			}

			return null;
		}

		static void ConfigProtocols (XmlNode section, WSConfig config)
		{
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				string error;
				if (name == "add") {
					string protoName = AttValue ("name", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (!config.AddProtocol (protoName, out error))
						ThrowException (error, child);
					
					continue;
				}

				ThrowException ("Unexpected element", child);
			}
		}
		
		// To save some typing...
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
	
	class HandlersUtil
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
			if (node.Attributes == null) {
				if (optional)
					return null;

				ThrowException ("Required attribute not found: " + attKey, node);
			}

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

