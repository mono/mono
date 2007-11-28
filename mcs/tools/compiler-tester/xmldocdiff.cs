#if !NET_2_1

using System;
using System.Collections;
using System.Xml;

public class XmlComparer
{
	public class ComparisonException : Exception
	{
		public ComparisonException (string message)
			: base (message)
		{
		}
	}

	static bool debug = false;
/*

	public static void Main (string [] args)
	{
		if (args.Length < 2) {
			Console.Error.WriteLine ("Usage: xmldocdiff [reference_output.xml] [actual_output.xml]");
			return;
		}
		if (args.Length > 2 && args [2].EndsWith ("-debug"))
			debug = true;

		try {
			Run (args[0], args[1]);
		} catch (Exception ex) {
			Console.WriteLine ("FAIL: " + args [1]);
			throw ex;
		}
		Console.WriteLine ("PASS: " + args [1]);
	}
*/
	public static void Compare (string reference, string output)
	{
		XmlDocument doc1 = new XmlDocument ();
		doc1.Load (reference);
		XmlDocument doc2 = new XmlDocument ();
		doc2.Load (output);

		XmlNodeList memberList1 = doc1.SelectNodes ("/doc/members/member");
		XmlNodeList memberList2 = doc2.SelectNodes ("/doc/members/member");

		Hashtable namedItems = new Hashtable ();

		foreach (XmlElement el in memberList1)
			namedItems.Add (el.GetAttribute ("name"), el);
		foreach (XmlElement el2 in memberList2) {
			string name = el2.GetAttribute ("name");
			XmlElement el1 = namedItems [name] as XmlElement;
			if (el1 == null) {
				Report ("Extraneous element found. Name is '{0}'", name);
				continue;
			}
			namedItems.Remove (name);

			CompareNodes (el1, el2);

		}
		foreach (string name in namedItems.Keys)
			Report ("Expected comment was not found. Name is {0}, XML is {1}", name, ((XmlElement) namedItems [name]).OuterXml);

		// finally, check other nodes than members
		doc1.SelectSingleNode ("/doc/members").RemoveAll ();
		doc2.SelectSingleNode ("/doc/members").RemoveAll ();
		string xml1 = doc1.OuterXml.Replace ("\r", "").Trim ();
		string xml2 = doc2.OuterXml.Replace ("\r", "").Trim ();
		if (xml1 != xml2)
			Report (@"Either of doc, assembly, name, members elements  are different.
doc1: {0}
doc2: {1}", xml1, xml2);
	}

	private static void CompareNodes (XmlNode n1, XmlNode n2)
	{
		if (n2 == null) {
			Report (@"Nodes does not exist:
Node1: {0}", n1.OuterXml);
			return;
		}
		if (n1.NodeType != n2.NodeType) {
			Report (@"Nodes differ:
Node1: {0}
Node2: {1}", n1.OuterXml, n2.OuterXml);
			return;
		}
		if (n1.Name != n2.Name) {
			Report (@"Node names differ:
Node1: {0}
Node2: {1}", n1.OuterXml, n2.OuterXml);
			return;
		}
		if (n1 is XmlElement) {
			for (int i = 0; i < n1.Attributes.Count; i++)
				CompareNodes (n1.Attributes [i],
					n2.Attributes [i]);
			for (int i = 0; i < n1.ChildNodes.Count; i++)
				CompareNodes (n1.ChildNodes [i],
					n2.ChildNodes [i]);
		}
		if (n1.NodeType != XmlNodeType.Comment && n1.Value != null) {
			string v1 = n1.Value.Trim ().Replace ("\r", "");
			string v2 = n2.Value.Trim ().Replace ("\r", "");
			if (v1 != v2)
				Report (@"Node values differ:
Node1: {0}
Node2: {1}", v1, v2);
		}
	}

	static void Report (string format, params object [] args)
	{
		if (debug)
			Console.WriteLine (format, args);
		else
			throw new ComparisonException (String.Format (format, args));
	}
}

#endif
