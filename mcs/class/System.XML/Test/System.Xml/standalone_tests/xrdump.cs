using System;
using System.IO;
using System.Xml;

public class XmlReaderDumper
{
	public static void Main ()
	{
		new XmlReaderDumper ().TestOASIS ();
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
				while (!xtr.EOF) {
					DumpReader (xtr, false);
					xtr.Read ();
				}

			} catch (XmlException ex) {
				if (fi.Name.IndexOf ("pass") >= 0)
					Console.WriteLine ("Incorrectly invalid: " + fi.FullName + "\n" + ex.Message);
			}
		}
	}

	public void DumpReader (XmlReader xr, bool attValue)
	{
		Console.WriteLine ("NodeType: " + xr.NodeType);
		Console.WriteLine ("Prefix: " + xr.Prefix);
		Console.WriteLine ("Name: " + xr.Name);
		Console.WriteLine ("LocalName: " + xr.LocalName);
		Console.WriteLine ("NamespaceURI: " + xr.NamespaceURI);
		Console.WriteLine ("Value: " + xr.Value);
		Console.WriteLine ("Depth: " + xr.Depth);
		Console.WriteLine ("IsEmptyElement: " + xr.IsEmptyElement);

		if (xr.NodeType == XmlNodeType.Attribute) {
			Console.WriteLine ("Attribute Values::::");
			while (xr.ReadAttributeValue ())
				DumpReader (xr, true);
			Console.WriteLine (":::Attribute Values End");
		} else if (!attValue) {
			Console.WriteLine ("Attributes::::");
			Console.Write (xr.AttributeCount);
			if (xr.MoveToFirstAttribute ()) {
				do {
					DumpReader (xr, false);
				} while (xr.MoveToNextAttribute ());
				xr.MoveToElement ();
			}
			Console.WriteLine (":::Attributes End");
		}
	}
}
