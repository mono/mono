//
// FilterConfiguration.cs: Filter Configuration
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Configuration;
using System.Xml;

namespace Microsoft.Web.Services.Configuration {
	
	public class ConfigurationBase {

		public ConfigurationBase () {}

		protected static void CheckForChildNodes (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.HasChildNodes)
				throw new ConfigurationException ();
		}

		[MonoTODO()]
		protected static void CheckForDuplicateChildNodes (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.HasChildNodes) {
				// TODO check for duplicates
				// throw new ConfigurationException ();
			}
		}

		protected static void CheckForUnrecognizedAttributes (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.Attributes.Count > 0)
				throw new ConfigurationException ();
		}

		[MonoTODO()]
		protected static XmlNode GetAndRemoveAttribute (XmlNode node, string attrib, bool fRequired)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			return null;
		}

		[MonoTODO()]
		protected static XmlNode GetAndRemoveBoolAttribute (XmlNode node, string attrib, bool fRequired, ref bool val)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			return null;
		}

		[MonoTODO()]
		protected static XmlNode GetAndRemoveStringAttribute (XmlNode node, string attrib, bool fRequired, ref string val)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			return null;
		}

		[MonoTODO()]
		protected static void ThrowIfElement (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.NodeType == XmlNodeType.Element)
				throw new ConfigurationException ();
		}

		[MonoTODO()]
		protected static void ThrowIfNotComment (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.NodeType != XmlNodeType.Comment)
				throw new ConfigurationException ();
		}
	}
}