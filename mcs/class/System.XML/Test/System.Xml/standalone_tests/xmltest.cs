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
	}

	static void RunOASISTest ()
	{
		XmlDocument doc = new XmlDocument ();
		foreach (FileInfo fi in
			new DirectoryInfo (@"XML-Test-Suite/xmlconf/oasis").GetFiles ("*.xml")) {
			try {
				XmlTextReader xtr = new XmlTextReader (fi.FullName);
				xtr.Namespaces = false;
				xtr.Normalization = true;
				doc.RemoveAll ();
				doc.Load (xtr);
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

		XmlDocument doc = new XmlDocument ();
		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					doc.RemoveAll ();
					doc.Load (xtr);
					Console.WriteLine ("Incorrectly wf: " + fi.FullName);
				} catch (XmlException) {
					// expected
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + fi.FullName + "\n" + ex.Message);
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

		XmlDocument doc = new XmlDocument ();
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
					Console.WriteLine ("Incorrectly not-wf: " + fi.FullName + " " + ex.Message);
				} catch (XmlSchemaException ex) {
					Console.WriteLine ("Incorrectly invalid: " + fi.FullName + " " + ex.Message);
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + fi.FullName + "\n" + ex.Message);
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

		XmlDocument doc = new XmlDocument ();
		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					xtr.Normalization = true;
					doc.RemoveAll ();
					while (!xtr.EOF)
						xtr.Read ();
				} catch (Exception ex) {
					Console.WriteLine ("Incorrectly not-wf: " + fi.FullName + String.Concat ("(", ex.GetType ().Name, ") " + ex.Message));
				}
			}
		}

		foreach (DirectoryInfo di in dirs) {
			foreach (FileInfo fi in di.GetFiles ("*.xml")) {
				try {
					XmlTextReader xtr = new XmlTextReader (fi.FullName);
					xtr.Namespaces = false;
					xtr.Normalization = true;
					doc.RemoveAll ();
					doc.Load (new XmlValidatingReader (xtr));
					Console.WriteLine ("Incorrectly valid: " + fi.FullName);
				} catch (XmlSchemaException) {
					// expected
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected Error: " + fi.FullName + "\n" + ex.Message);
				}
			}
		}
	}
}
