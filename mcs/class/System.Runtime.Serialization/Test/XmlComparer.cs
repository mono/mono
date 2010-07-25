using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests
{
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

		public bool AreEqualAttribs (XmlAttributeCollection expected, XmlAttributeCollection actual)
		{
			if (expected.Count != actual.Count)
				return false;
			for (int i=0; i<expected.Count; i++) {
				if ((flags & Flags.IgnoreAttribOrder) != 0) {
					string ln = expected[i].LocalName;
					string ns = expected[i].NamespaceURI;
					string val = expected[i].Value;
					_expected = ns+":"+ln+"="+val;
					XmlAttribute atr2 = actual[ln, ns];
					_actual = atr2 == null ? "<<missing>>" : ns + ":" + ln + "=" + atr2.Value;
					if (atr2 == null || atr2.Value.Trim().ToLower() != val.Trim().ToLower())
						return false;
				} else {
					if (expected [i].LocalName != actual [i].LocalName)
						return false;
					if (expected [i].NamespaceURI != actual [i].NamespaceURI)
						return false;
					if (expected [i].Value.Trim().ToLower() != actual [i].Value.Trim().ToLower())
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

				_actual = actual.OuterXml;
				_expected = expected.OuterXml;
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

		public static void AssertAreEqual (string expected, string actual) {
			AssertAreEqual (expected, actual, String.Empty);
		}

		public static void AssertAreEqual (string expected, string actual, string msg) {

			try {
				XmlDocument or = new XmlDocument ();
				or.LoadXml (expected);
				XmlDocument dr = new XmlDocument ();
				dr.LoadXml (actual);
				XmlComparer comparer = new XmlComparer ();
				if (!comparer.AreEqual (or, dr))
					Assert.AreEqual (comparer.Expected, comparer.Actual, msg);
			}
			catch (AssertionException) {
				throw;
			}
			catch (Exception e) {
				//swallow e when there is XML error and fallback
				//to the text comparison
				Assert.AreEqual (expected, actual, msg);
			}
		}
	}
}
