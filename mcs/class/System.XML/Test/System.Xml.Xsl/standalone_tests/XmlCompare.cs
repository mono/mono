using System;
using System.Xml;

namespace XmlCompare
{
	/// <summary>
	/// Summary description for XmlCompare.
	/// </summary>
	public class XmlCompare
	{
		[Flags]
		public enum Flags {
			IgnoreNone=0,
			IgnoreAttribOrder=1,
		}
		Flags flags;
		public XmlCompare (Flags flags)
		{
			this.flags = flags;
		}

		public XmlCompare ()
			:this (Flags.IgnoreNone)
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
					if (atr2 == null || atr2.Value != val)
						return false;
				} else {
					if (attrs1 [i].LocalName != attrs2 [i].LocalName)
						return false;
					if (attrs1 [i].NamespaceURI != attrs2 [i].NamespaceURI)
						return false;
					if (attrs1 [i].Value != attrs2 [i].Value)
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
			if (!node1.HasChildNodes && !node2.HasChildNodes)
				return node1.Value == node2.Value;
			else {//one of nodes has some children
				if (!node1.HasChildNodes || !node2.HasChildNodes)
					return false;//and another has none
				return AreEqualNodeList (node1.ChildNodes, node2.ChildNodes);
			}
		}

		static void Main ()
		{
			XmlDocument doc1 = new XmlDocument ();
			XmlDocument doc2 = new XmlDocument ();
			XmlDocument doc3 = new XmlDocument ();
			XmlDocument doc4 = new XmlDocument ();
			
			doc1.LoadXml (@"<?xml version=""1.0""?>
<doc>
	<element attr1=""1"" attr2=""2"" />
</doc>");
			doc2.LoadXml (@"<?xml version=""1.0""?><doc><element attr1=""1"" attr2=""2""/></doc>");
			doc3.LoadXml (@"<?xml version=""1.0""?><doc><element attr2=""2"" attr1=""1""/></doc>");
			doc4.LoadXml (@"<?xml version=""1.0""?><doc><element attr3=""3"" attr2=""2""/></doc>");

			XmlCompare cmp1 = new XmlCompare();
			XmlCompare cmp2 = new XmlCompare(Flags.IgnoreAttribOrder);

			Console.Out.WriteLine (cmp1.AreEqual (doc1, doc2).ToString ());
			Console.Out.WriteLine (cmp1.AreEqual (doc1, doc3).ToString ());
			Console.Out.WriteLine (cmp2.AreEqual (doc1, doc3).ToString ());
			Console.Out.WriteLine (cmp2.AreEqual (doc1, doc4).ToString ());
		}
	}
}
