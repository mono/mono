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
				throw new ConfigurationException (Locale.GetText ("has child nodes"));
		}

		protected static void CheckForDuplicateChildNodes (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			int n = node.ChildNodes.Count;
			if (n > 1) {
				// (n - 1) last is unimportant
				for (int i=0; i < n - 1; i++) {
					string xname = node.ChildNodes [i].Name;
					// (i + 1) don't look back (again)
					for (int j=i+1; j < n; i++) {
						if (xname == node.ChildNodes [j].Name)
							throw new ConfigurationException (Locale.GetText ("found duplicate nodes"));
					}
				}
			}
		}

		protected static void CheckForUnrecognizedAttributes (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.Attributes.Count > 0)
				throw new ConfigurationException (Locale.GetText ("node has attributes"));
		}

		protected static XmlNode GetAndRemoveAttribute (XmlNode node, string attrib, bool fRequired)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			XmlNode xn = node.Attributes [attrib];
			if (xn != null)
				node.Attributes.Remove (xn as XmlAttribute);
			else if (fRequired)
				throw new ConfigurationException (Locale.GetText ("missing required attribute"));
			return xn;
		}

		protected static XmlNode GetAndRemoveBoolAttribute (XmlNode node, string attrib, bool fRequired, ref bool val)
		{
			XmlNode xn = GetAndRemoveAttribute (node, attrib, fRequired);
			if (xn != null)
				val = Convert.ToBoolean (xn.Value);
			return xn;
		}
#if WSE2
		protected static XmlNode GetAndRemoveIntegerAttribute (XmlNode node, string attrib, bool fRequired, ref int val)
		{
			XmlNode xn = GetAndRemoveAttribute (node, attrib, fRequired);
			if (xn != null)
				val = Convert.ToInt32 (xn.Value);
			return xn;
		}
#endif
		protected static XmlNode GetAndRemoveStringAttribute (XmlNode node, string attrib, bool fRequired, ref string val) 
		{
			XmlNode xn = GetAndRemoveAttribute (node, attrib, fRequired);
			if (xn != null)
				val = xn.Value;
			return xn;
		}

		protected static void ThrowIfElement (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.NodeType == XmlNodeType.Element)
				throw new ConfigurationException (Locale.GetText ("node is XmlNodeType.Element"));
		}

		protected static void ThrowIfNotComment (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			if (node.NodeType != XmlNodeType.Comment)
				throw new ConfigurationException (Locale.GetText ("node isn't XmlNodeType.Comment"));
		}
	}
}
