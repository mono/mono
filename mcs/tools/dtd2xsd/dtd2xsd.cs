using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using BF = System.Reflection.BindingFlags;

class Dtd2XsdDriver
{
	public static void Main (string [] args)
	{
		try {
			Run (args);
		} catch (Exception ex) {
			Console.WriteLine ("ERROR: " + ex.Message);
		}
	}

	static void Run (string [] args)
	{
		if (args.Length < 1) {
			Console.WriteLine ("USAGE: mono dtd2xsd.exe instance-xmlfile [output-xsdfile]");
			return;
		}
		XmlTextReader xtr;
		if (args [0].EndsWith (".dtd"))
			xtr = new XmlTextReader ("<!DOCTYPE dummy SYSTEM '" + args [0] + "'><dummy/>",
				XmlNodeType.Document, null);
		else
			xtr = new XmlTextReader (args [0]);
		XmlSchema xsd = Dtd2Xsd.Run (xtr);
		if (args.Length > 1)
			xsd.Write (new StreamWriter (args [1]));
		else
			xsd.Write (Console.Out);
	}
}

public class Dtd2Xsd
{
	public static XmlSchema Run (XmlTextReader xtr)
	{
		while (xtr.NodeType != XmlNodeType.DocumentType) {
			if (!xtr.Read ())
				throw new Exception ("DTD did not appeare.");
		}

		// Hacky reflection part
		object impl = xtr;
		BF flag = BF.NonPublic | BF.Instance;

		// In Mono NET_2_0 XmlTextReader is just a wrapper which 
		// does not contain DTD directly.
		FieldInfo fi = typeof (XmlTextReader).GetField ("source", flag);
		if (fi != null)
			impl = fi.GetValue (xtr);

		PropertyInfo pi = impl.GetType ().GetProperty ("DTD", flag);
		object dtd = pi.GetValue (impl, null);
		MethodInfo mi =
			dtd.GetType ().GetMethod ("CreateXsdSchema", flag);
		object o = mi.Invoke (dtd, null);
		return (XmlSchema) o;
	}
}

