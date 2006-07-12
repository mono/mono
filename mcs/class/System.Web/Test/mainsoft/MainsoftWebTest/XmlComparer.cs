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
		string _actual = "";
		string _expected = "";

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

		public bool AreEqualNodeList (XmlNodeList expected, XmlNodeList actual)
		{
			if (expected.Count != actual.Count)
				return false;
			for (int i=0; i<expected.Count; i++) {
				if (!AreEqual (expected[i], actual[i]))
					return false;
			}
			return true;
		}

		public bool AreEqual (XmlNode expected, XmlNode actual)
		{
			lastCompare = expected.OuterXml + "\n" + actual.OuterXml;
			_actual = actual.OuterXml;
			_expected = expected.OuterXml;
			// skip XmlDeclaration
			if ((expected.NodeType == XmlNodeType.XmlDeclaration) &&
				(actual.NodeType == XmlNodeType.XmlDeclaration))
				return true;
			if (expected.NodeType != actual.NodeType)
				return false;
			if (expected.LocalName != actual.LocalName)
				return false;
			if (expected.NamespaceURI != actual.NamespaceURI)
				return false;
			if (expected.Attributes != null && actual.Attributes != null) {
				if (!AreEqualAttribs (expected.Attributes, actual.Attributes))
					return false;
			}
			else //one of nodes has no attrs
				if (expected.Attributes != null || actual.Attributes != null)
					return false;//and another has some
			if (!expected.HasChildNodes && !actual.HasChildNodes) {
				string val1 = expected.Value;
				string val2 = actual.Value;
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
				if (!expected.HasChildNodes || !actual.HasChildNodes)
					return false;//and another has none
				return AreEqualNodeList (expected.ChildNodes, actual.ChildNodes);
			}
		}

		public string LastCompare 
		{
			get {return lastCompare;}
		}

		public string Actual
		{
			get { return _actual; }
		}

		public string Expected
		{
			get { return _expected; }
		}
	}
}
