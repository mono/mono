using System;
using System.Collections;
using System.Configuration;
using System.Xml;


namespace Mainsoft.Drawing.Configuration
{
	/// <summary>
	/// Summary description for MetadataConfigurationHandler.
	/// </summary>
	public class ResolutionConfigurationHandler : IConfigurationSectionHandler
	{
		public ResolutionConfigurationHandler()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public virtual object Create (object parent, object configContext, XmlNode section) {

			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			ResolutionConfigurationCollection col = 
				new ResolutionConfigurationCollection(parent as ResolutionConfigurationCollection);

			XmlNodeList imageFormats = section.ChildNodes;
			foreach (XmlNode child in imageFormats) {
				
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					HandlersUtil.ThrowException ("Only elements allowed", child);

				string imageFormatName = HandlersUtil.ExtractAttributeValue ("name", child, false, false);

				string xResPath = HandlersUtil.ExtractNodeValue(child["xresolution"]);
				string yResPath = HandlersUtil.ExtractNodeValue(child["yresolution"]);
				string unitsType = HandlersUtil.ExtractNodeValue(child["unitstype"], false, true);

				string xResDefault = HandlersUtil.ExtractAttributeValue ("default", child["xresolution"]);
				string yResDefault = HandlersUtil.ExtractAttributeValue ("default", child["yresolution"]);
				string unitsTypeDefault = HandlersUtil.ExtractAttributeValue ("default", child["unitstype"], true);

				Hashtable unitScale = new Hashtable(3);

				XmlNodeList unitScaleNodes = child.SelectNodes("unitscale");
				foreach (XmlNode unitScaleNode in unitScaleNodes) {
					unitScale.Add(
						HandlersUtil.ExtractAttributeValue ("value", unitScaleNode),
						HandlersUtil.ExtractNodeValue(unitScaleNode) );
				}

				ResolutionConfiguration resConf = new ResolutionConfiguration(
					imageFormatName,
					xResPath, yResPath, unitsType,
					xResDefault, yResDefault, unitsTypeDefault,
					unitScale);
				
				col.Add(resConf);
			}

			col.Sort();
			return col;
		}
	}

	internal sealed class HandlersUtil {
		private HandlersUtil () {
		}

		static internal string ExtractNodeValue(XmlNode node, bool optional, bool allowEmpty) {
			if (node == null) {
				if (optional)
					return null;
				ThrowException ("Required node not found", node);
			}

			string nodeValue = node.InnerText;

			if (!allowEmpty && nodeValue == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " node is empty", node);
			}

			return nodeValue;
		}

		static internal string ExtractNodeValue(XmlNode node, bool optional) {
			return ExtractNodeValue(node, false, false);
		}

		static internal string ExtractNodeValue(XmlNode node) {
			return ExtractNodeValue(node, false);
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node) {
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional) {
			return ExtractAttributeValue (attKey, node, optional, false);
		}
		
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional,
			bool allowEmpty) {
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
			if (!allowEmpty && value == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " attribute is empty: " + attKey, node);
			}

			return value;
		}

		static internal void ThrowException (string msg, XmlNode node) {
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";
			throw new ConfigurationException (msg, node);
		}
	}
}
