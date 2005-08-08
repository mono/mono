#warning USE Test/System.Xml/W3C/xmlconf.cs instead.
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Test
{
	static char SEP = Path.DirectorySeparatorChar;

	public static void Main ()
	{
		Console.WriteLine ("WARNING: This test code is outdated. Use Test/System.Xml/W3C/xmlconf.exe instead.");
Console.WriteLine ("Started:  " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
		RunInvalidTest ("xmltest", false);
		RunInvalidTest ("ibm", false);
		RunInvalidTest ("sun", true);

		RunValidTest ("xmltest", false);
		RunValidTest ("ibm", false);
		RunValidTest ("sun", true);

		RunNotWellFormedTest ("xmltest", false);
		RunNotWellFormedTest ("ibm", false);
		RunNotWellFormedTest ("sun", true);

		RunOASISTest ();
Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
	}

	static void RunOASISTest ()
	{
		XmlDocument doc = new XmlDocument ();
		foreach (FileInfo fi in
			new DirectoryInfo (@"xml-test-suite/xmlconf/oasis").GetFiles ("*.xml")) {
			try {
				XmlTextReader xtr = new XmlTextReader (fi.FullName);
				xtr.Namespaces = false;
				xtr.Normalization = true;
				while (!xtr.EOF)
					xtr.Read ();
				if (fi.Name.IndexOf ("fail") >= 0)
					Console.WriteLine ("Incorrectly valid: " + fi.FullName);
			} catch (Exception ex) {
				if (fi.Name.IndexOf ("pass") >= 0)
					Console.WriteLine ("Incorrectly invalid: " + fi.FullName + "\n" + ex.Message);
			}
		}
	}

	static void RunNotWellFormedTest (string subdir, bool isSunTest)
	{
		string basePath = @"xml-test-suite/xmlconf/" + subdir + @"/not-wf";
		DirectoryInfo [] dirs = null;
		if (isSunTest)
			dirs =  new DirectoryInfo [] {new DirectoryInfo (basePath)};
		else
			dirs = new DirectoryInfo (basePath).GetDirectories ();

		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					while (!xtr.EOF)
						xtr.Read ();
					Console.WriteLine ("Incorrectly wf: " + subdir + "/" + di.Name + "/" + fi.Name);
				} catch (XmlException) {
					// expected
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + subdir + "/" + di.Name + "/" + fi.Name + "\n" + ex.Message);
				}
			}
		}
	}

	static void RunValidTest (string subdir, bool isSunTest)
	{
		string basePath = @"xml-test-suite/xmlconf/" + subdir + @"/valid";
		DirectoryInfo [] dirs = null;
		if (isSunTest)
			dirs =  new DirectoryInfo [] {new DirectoryInfo (basePath)};
		else
			dirs = new DirectoryInfo (basePath).GetDirectories ();

		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					xtr.Normalization = true;
					XmlReader xr = new XmlValidatingReader (xtr);
					while (!xr.EOF)
						xr.Read ();
				} catch (XmlException ex) {
					Console.WriteLine ("Incorrectly not-wf: " + subdir + "/" + di.Name + "/" + fi.Name + " " + ex.Message);
				} catch (XmlSchemaException ex) {
					Console.WriteLine ("Incorrectly invalid: " + subdir + "/" + di.Name + "/" + fi.Name + " " + ex.Message);
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + subdir + "/" + di.Name + "/" + fi.Name + "\n" + ex.Message);
				}
			}
		}
	}

	static void RunInvalidTest (string subdir, bool isSunTest)
	{
		string basePath = @"xml-test-suite/xmlconf/" + subdir + @"/invalid";
		DirectoryInfo [] dirs = null;
		if (isSunTest)
			dirs =  new DirectoryInfo [] {new DirectoryInfo (basePath)};
		else
			dirs = new DirectoryInfo (basePath).GetDirectories ();

		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					xtr.Normalization = true;
					while (!xtr.EOF)
						xtr.Read ();
				} catch (Exception ex) {
					Console.WriteLine ("Incorrectly not-wf: " + di.Name + "/" + fi.Name + String.Concat ("(", ex.GetType ().Name, ") " + ex.Message));
				}
			}
		}

		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					xtr.Normalization = true;
					XmlValidatingReader xr =
						new XmlValidatingReader (xtr);
					while (!xr.EOF)
						xr.Read ();
					Console.WriteLine ("Incorrectly valid: " + subdir + "/" + di.Name + "/" + fi.Name);
				} catch (XmlSchemaException) {
					// expected
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + subdir + "/" + di.Name + "/" + fi.Name + "\n" + ex.Message);
				}
			}
		}
	}
}
