using System;
using System.IO;
using System.Xml;

public class DomEventDumper
{
	public static void Main ()
	{
		new DomEventDumper ().TestOASIS ();
	}

	public void TestOASIS ()
	{
		XmlDocument doc = new XmlDocument ();
		doc.NodeInserting += new XmlNodeChangedEventHandler (OnInserting);
		doc.NodeInserted += new XmlNodeChangedEventHandler (OnInserted);
		doc.NodeChanging += new XmlNodeChangedEventHandler (OnChanging);
		doc.NodeChanged += new XmlNodeChangedEventHandler (OnChanged);
		doc.NodeRemoving += new XmlNodeChangedEventHandler (OnRemoving);
		doc.NodeRemoved += new XmlNodeChangedEventHandler (OnRemoved);

		foreach (FileInfo fi in
			new DirectoryInfo (@"xml-test-suite/xmlconf/oasis").GetFiles ("*.xml")) {
			try {
				if (fi.Name.IndexOf ("fail") >= 0)
					continue;

				Console.WriteLine ("#### File: " + fi.Name);

				XmlTextReader xtr = new XmlTextReader (fi.FullName);
				xtr.Namespaces = false;
				xtr.Normalization = true;
				doc.RemoveAll ();
				doc.Load (xtr);

			} catch (XmlException ex) {
				if (fi.Name.IndexOf ("pass") >= 0)
					Console.WriteLine ("Incorrectly invalid: " + fi.FullName + "\n" + ex.Message);
			}
		}
	}

	public void OnInserting (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Inserting::: " + e.Node.NodeType + " into " + e.NewParent.NodeType + " Name: " + e.Node.Name + ", Value: " + e.Node.Value);
	}

	public void OnInserted (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Inserted::: " + e.Node.NodeType + " into " + e.NewParent.NodeType + " Name: " + e.Node.Name + ", Value: " + e.Node.Value);
	}

	public void OnChanging (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Changing::: " + e.Node.NodeType);
	}

	public void OnChanged (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Changed::: " + e.Node.NodeType);
	}

	public void OnRemoving (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Removing::: " + e.Node.NodeType);
	}

	public void OnRemoved (object o, XmlNodeChangedEventArgs e)
	{
		Console.WriteLine ("Removed::: " + e.Node.NodeType);
	}
}
