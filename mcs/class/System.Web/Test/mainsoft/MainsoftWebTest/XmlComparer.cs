using System;
using System.Xml;

namespace MonoTests.stand_alone.WebHarness
{
	/// <summary>
	/// Summary description for XmlComparer.
	/// </summary>
	public class XmlComparer
	{
		[Flags]
		public enum Flags {
			IgnoreNone=0,
			IgnoreAttribOrder=1,
		}
		Flags flags;
		bool ignoreWS = true;
		string lastCompare = "";
		
		public XmlComparer (Flags flags, bool ignoreWS) 
		{
			this.flags = flags;
			this.ignoreWS = ignoreWS;
		}

		public XmlComparer (Flags flags) : this (flags, true)
		{
		}

		public XmlComparer () : this (Flags.IgnoreAttribOrder)
		{
		}

		public bool AreEqualAttribs (XmlAttributeCollection attrs1, XmlAttributeCollection attrs2)
		{
			if (attrs1.Count != attrs2.Count)
				return false;
			for (int i=0; i<attrs1.Count; i++) {
				if ((flags & Flags.IgnoreAttribOrder) != 0) {
					string ln = attrs1[i].LocalName;
					string ns = attrs1[i].NamespaceURI;
					string val = attrs1[i].Value;
					XmlAttribute atr2 = attrs2[ln, ns];
					if (atr2 == null || atr2.Value.Trim().ToLower() != val.Trim().ToLower())
						return false;
				} else {
					if (attrs1 [i].LocalName != attrs2 [i].LocalName)
						return false;
					if (attrs1 [i].NamespaceURI != attrs2 [i].NamespaceURI)
						return false;
					if (attrs1 [i].Value.Trim().ToLower() != attrs2 [i].Value.Trim().ToLower())
						return false;
				}
			}
			return true;
		}

		public bool AreEqualNodeList (XmlNodeList lst1, XmlNodeList lst2)
		{
			if (lst1.Count != lst2.Count)
				return false;
			for (int i=0; i<lst1.Count; i++) {
				if (!AreEqual (lst1[i], lst2[i]))
					return false;
			}
			return true;
		}

		public bool AreEqual (XmlNode node1, XmlNode node2)
		{
			lastCompare = node1.OuterXml + "\n" + node2.OuterXml;
			// skip XmlDeclaration
			if ((node1.NodeType == XmlNodeType.XmlDeclaration) &&
				(node2.NodeType == XmlNodeType.XmlDeclaration))
				return true;
			if (node1.NodeType != node2.NodeType)
				return false;
			if (node1.LocalName != node2.LocalName)
				return false;
			if (node1.NamespaceURI != node2.NamespaceURI)
				return false;
			if (node1.Attributes != null && node2.Attributes != null) {
				if (!AreEqualAttribs (node1.Attributes, node2.Attributes))
					return false;
			}
			else //one of nodes has no attrs
				if (node1.Attributes != null || node2.Attributes != null)
					return false;//and another has some
			if (!node1.HasChildNodes && !node2.HasChildNodes) {
				string val1 = node1.Value;
				string val2 = node2.Value;
				if (ignoreWS) //ignore white spaces
				{ 
					if (val1 != null)
						val1 = val1.Trim().Replace("\r\n", "\n");
					if (val2 != null)
						val2 = val2.Trim().Replace("\r\n", "\n");
				}
				return val1 == val2;
			}
			else {//one of nodes has some children
				if (!node1.HasChildNodes || !node2.HasChildNodes)
					return false;//and another has none
				return AreEqualNodeList (node1.ChildNodes, node2.ChildNodes);
			}
		}

		public string LastCompare 
		{
			get {return lastCompare;}
		}
	}
}
