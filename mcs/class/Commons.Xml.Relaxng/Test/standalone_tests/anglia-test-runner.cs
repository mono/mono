using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

public class Driver
{
	public static void Main (string [] args)
	{
		bool outAll = false;
		bool details = false;
		bool stopOnError = false;
		string filter = null;
		foreach (string arg in args) {
			switch (arg) {
			case "--outall":
				outAll = true; break;
			case "--details":
				details = true; break;
			case "--stoponerror":
				stopOnError = true; break;
			default:
				filter = arg; break;
			}
		}

		try {
			XmlDocument doc = new XmlDocument ();
			doc.Load ("test/RNCTest.xml");
			int success = 0;
			int failure = 0;
			foreach (XmlElement el in doc.SelectNodes ("/RNCTestCases/TestCase")) {
				string id = el.GetAttribute ("id");
				if (filter != null && id.IndexOf (filter) < 0)
					continue;
				if (outAll)
					Console.WriteLine ("testing " + id);
				bool isValid = el.GetAttribute ("legal") == "true";
				RncParser p = new RncParser (new NameTable ());
				try {
					string s = new StreamReader ("test" + Path.DirectorySeparatorChar + el.GetAttribute ("path")).ReadToEnd ();
					p.Parse (new StringReader (s));
					if (isValid) {
						success++;
//						Console.Error.WriteLine ("valid " + id);
					} else {
						failure++;
						Console.Error.WriteLine ("INCORRECTLY VALID   " + id);
					}
				} catch (Exception ex) {
					if (isValid) {
						if (stopOnError)
							throw;
						failure++;
						Console.Error.WriteLine ("INCORRECTLY INVALID " + id + " --> " + (details ? ex.ToString () : ex.Message));
					} else {
						success++;
//						Console.Error.WriteLine ("invalid " + id);
					}
				}
			}
			Console.Error.WriteLine ("Total success: " + success);
			Console.Error.WriteLine ("Total failure: " + failure);
		} catch (Exception ex) {
			Console.Error.WriteLine ("Unexpected Exception: " + ex);
		}
	}
}
