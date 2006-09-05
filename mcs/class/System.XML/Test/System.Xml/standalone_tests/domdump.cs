using System;
using System.IO;
using System.Xml;

public class DomDumper
{
	public static void Main ()
	{
		new DomDumper ().TestOASIS ();
	}

	public void TestOASIS ()
	{
		XmlDocument doc = new XmlDocument ();
		foreach (FileInfo fi in
			new DirectoryInfo (@"xml-test-suite/xmlconf/oasis").GetFiles ("*.xml")) {
			try {
				if (fi.Name.IndexOf ("fail") >= 0)
					continue;

				Console.WriteLine (fi.Name);

				XmlTextReader xtr = new XmlTextReader (fi.FullName);
				xtr.Namespaces = false;
				xtr.Normalization = true;
				doc.RemoveAll ();
				doc.Load (xtr);

				DumpDom (doc);

			} catch (XmlException ex) {
				if (fi.Name.IndexOf ("pass") >= 0)
					Console.WriteLine ("Incorrectly invalid: " + fi.FullName + "\n" + ex.Message);
			}
		}
	}

	public void DumpDom (XmlNode n)
	{
		Console.Write (n.NodeType);
		Console.Write (' ');
		Console.Write (n.Prefix);
		Console.Write (' ');
		Console.Write (n.Name);
		Console.Write (' ');
		Console.Write (n.LocalName);
		Console.Write (' ');
		Console.Write (n.NamespaceURI);
		Console.Write (' ');
		Console.Write (n.Value);
		Console.WriteLine (' ');

		Console.WriteLine ("Attributes::::");
		Console.Write (n.Attributes != null);
		if (n.Attributes != null) {
			Console.Write (' ');
			Console.Write (n.Attributes.Count);
			Console.Write (' ');
			for (int i = 0; i < n.Attributes.Count; i++)
				DumpDom (n.Attributes [i]);
		}
		Console.WriteLine (":::Attributes End");

		Console.WriteLine ("ChildNodes::::");
		Console.Write (n.ChildNodes != null);
		if (n.ChildNodes != null) {
			Console.Write (' ');
			Console.Write (n.ChildNodes.Count);
			Console.Write (' ');
			for (int i = 0; i < n.ChildNodes.Count; i++)
				DumpDom (n.ChildNodes [i]);
		}
		Console.WriteLine (":::ChildNodes End");
	}
}
