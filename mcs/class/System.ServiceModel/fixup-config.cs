using System;
using System.Xml;

public class FixupXml
{
	public static void Main (string [] args)
	{
		if (args.Length == 0) {
			Console.WriteLine ("pass path-to-machine.config.");
			return;
		}
		XmlDocument doc = new XmlDocument ();
		doc.Load (args [0]);
		XmlElement el = doc.SelectSingleNode ("/configuration/configSections") as XmlElement;
		XmlElement old = el.SelectSingleNode ("sectionGroup[@name='system.serviceModel']") as XmlElement;
		XmlNode up = doc.ReadNode (new XmlTextReader ("fixup-config.xml"));
		if (old != null)
			el.RemoveChild (old);
		el.InsertAfter (up, null);
		XmlTextWriter w = new XmlTextWriter (args [0], null);
		w.Formatting = Formatting.Indented;
		w.IndentChar = '\t';
		w.Indentation = 1;
		doc.Save (w);
		w.Close ();
	}
}

