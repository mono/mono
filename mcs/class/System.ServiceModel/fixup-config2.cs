using System;
using System.Xml;

public class FixupXml
{
	public static void Main (string [] args)
	{
		if (args.Length == 0) {
			Console.WriteLine ("pass path-to-web.config.");
			return;
		}
		XmlDocument doc = new XmlDocument ();
		doc.Load (args [0]);
		XmlElement el = doc.SelectSingleNode ("/configuration/system.web/httpHandlers") as XmlElement;
		XmlElement old = el.SelectSingleNode ("add[@path='*.svc']") as XmlElement;
		XmlNode up = doc.ReadNode (new XmlTextReader ("fixup-config2.xml"));
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

