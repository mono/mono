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
		RunTest ("msxsdtest");
		RunTest ("nisttest");
		RunTest ("suntest");
	}
	
	static void RunTest (string subdir)
	{
Console.WriteLine ("Started:  " + DateTime.Now);
		string basePath = @"Xsd-Test-Suite" + SEP;
		XmlDocument doc = new XmlDocument ();
		doc.Load (basePath + subdir + SEP + "tests-all.xml");
		foreach (XmlElement test in doc.SelectNodes ("/tests/test")) {
			// Test schema
			string schemaFile = test.SelectSingleNode ("@schema").InnerText;
			if (schemaFile.Length > 2)
				schemaFile = schemaFile.Substring (2);
			bool isValidSchema = test.SelectSingleNode ("@out_s").InnerText == "1";
			XmlSchema schema = null;
			try {
				XmlTextReader sxr = new XmlTextReader (basePath + schemaFile);
Console.WriteLine ("BaseURI: " + sxr.BaseURI);
				schema = XmlSchema.Read (sxr, null);
				sxr.Close ();
				schema.Compile (null);
				if (!isValidSchema) {
					Console.WriteLine ("Incorrectly Valid   schema  : " + schemaFile);
					continue;
				}
			} catch (XmlSchemaException ex) {
				if (isValidSchema) {
					Console.WriteLine ("Incorrectly Invalid schema  : " + schemaFile + " " + ex);
					continue;
				}
			} catch (Exception ex) {
				Console.WriteLine ("Unexpected Exception on schema: " + schemaFile + " " + ex);
				continue;
			}
			// Test instances
			string instanceFile = test.SelectSingleNode ("@instance").InnerText;
			if (instanceFile.Length == 0)
				continue;
			else if (instanceFile.StartsWith ("./"))
				instanceFile = instanceFile.Substring (2);
			bool isValidInstance = test.SelectSingleNode ("@out_x").InnerText == "1";
			try {
				XmlValidatingReader xvr = new XmlValidatingReader (new XmlTextReader (basePath + "\\" + instanceFile));
				xvr.Schemas.Add (schema);
				while (!xvr.EOF)
					xvr.Read ();
				if (!isValidInstance)
					Console.WriteLine ("Incorrectly Valid   instance: " + schemaFile);
				xvr.Close ();
			} catch (XmlSchemaException ex) {
				if (isValidInstance)
					Console.WriteLine ("Incorrectly Invalid instance: " + schemaFile + " " + ex);
			} catch (Exception ex) {
				Console.WriteLine ("Unexpected Exception on instance: " + schemaFile + " " + ex);
			}
		}
Console.WriteLine ("Finished: " + DateTime.Now);
	}
}
