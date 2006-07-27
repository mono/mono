using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class XsdTest
{
	static readonly char SEP = Path.DirectorySeparatorChar;
	static ValidationEventHandler noValidateHandler =
		new ValidationEventHandler (NoValidate);

	bool version2;
	bool verbose;
	bool stopOnError;
	bool noResolver;
	bool reportAsXml;
	bool reportDetails;
	bool reportSuccess;
	bool testAll;
	bool noValidate;
	string specificTarget;
	TextWriter ReportWriter = Console.Out;
	XmlTextWriter XmlReport;

	public static void Main (string [] args)
	{
		new XsdTest ().Run (args);
	}

	void Usage ()
	{
		Console.WriteLine (@"
USAGE: mono xsdtest.exe options target-pattern

	options:
	--stoponerr:		stops at unexpected error.
	--noresolve:		don't resolve external resources.
	--novalidate:		don't validate and continue reading.
{0}
	--verbose:		includes processing status.
	--xml:			report as XML format.
	--details:		report stack trace for errors.
	--reportsuccess:	report successful test as well.
	--testall:		process NISTTest/SunTest as well as MSXsdTest.

	target-pattern:		Part of the target schema file name.
				(No Regex support.)
",
"	--v2			use XmlReader.Create() [2.0 only]"
	);
		return;
	}

	void Run (string [] args)
	{
		foreach (string s in args) {
			switch (s) {
			case "--help":
				Usage ();
				return;
			case "--v2":
				version2 = true; break;
			case "--verbose":
				verbose = true; break;
			case "--stoponerr":
				stopOnError = true; break;
			case "--noresolve":
				noResolver = true; break;
			case "--novalidate":
				noValidate = true; break;
			case "--xml":
				reportAsXml = true; break;
			case "--details":
				reportDetails = true; break;
			case "--reportsuccess":
				reportSuccess = true; break;
			case "--testall":
				testAll = true; break;
			default:
				if (s.StartsWith ("--report:"))
					ReportWriter = new StreamWriter (
						s.Substring (9));
				else
					specificTarget = s;
				break;
			}
		}
		RunTest ("msxsdtest");
		if (testAll) {
			RunTest ("suntest");
			RunTest ("nisttest");
		}
		ReportWriter.Close ();
	}

	static void NoValidate (object o, ValidationEventArgs e)
	{
	}
	
	void RunTest (string subdir)
	{
		string basePath = @"xsd-test-suite" + SEP;
		XmlDocument doc = new XmlDocument ();
		if (noResolver)
			doc.XmlResolver = null;
		doc.Load (basePath + subdir + SEP + "tests-all.xml");

		if (reportAsXml) {
			XmlReport = new XmlTextWriter (ReportWriter);
			XmlReport.Formatting = Formatting.Indented;
			XmlReport.WriteStartElement ("test-results");
		}

		Console.WriteLine ("Started:  " + DateTime.Now);

		foreach (XmlElement test in doc.SelectNodes ("/tests/test")) {
			// Test schema
			string schemaFile = test.SelectSingleNode ("@schema").InnerText;
			if (specificTarget != null &&
				schemaFile.IndexOf (specificTarget) < 0)
				continue;
			if (schemaFile.Length > 2)
				schemaFile = schemaFile.Substring (2);
			if (verbose)
				Report (schemaFile, true, "compiling", "");
			bool isValidSchema = test.SelectSingleNode ("@out_s").InnerText == "1";
			XmlSchema schema = null;
			XmlTextReader sxr = null;
			try {
				sxr = new XmlTextReader (basePath + schemaFile);
				if (noResolver)
					sxr.XmlResolver = null;
				schema = XmlSchema.Read (sxr, null);
				schema.Compile (noValidate ? noValidateHandler : null, noResolver ? null : new XmlUrlResolver ());
				if (!isValidSchema && !noValidate) {
					Report (schemaFile, true, "should fail", "");
					continue;
				}
				if (reportSuccess)
					Report (schemaFile, true, "OK", "");
			} catch (XmlSchemaException ex) {
				if (isValidSchema)
					Report (schemaFile, true, "should succeed", 
						reportDetails ?
						ex.ToString () : ex.Message);
				else if (reportSuccess)
					Report (schemaFile, true, "OK", "");
				continue;
			} catch (Exception ex) {
				if (stopOnError)
					throw;
				Report (schemaFile, true, "unexpected",
						reportDetails ?
						ex.ToString () : ex.Message);
				continue;
			} finally {
				if (sxr != null)
					sxr.Close ();
			}

			// Test instances
			string instanceFile = test.SelectSingleNode ("@instance").InnerText;
			if (instanceFile.Length == 0)
				continue;
			else if (instanceFile.StartsWith ("./"))
				instanceFile = instanceFile.Substring (2);
			if (verbose)
				Report (instanceFile, false, "reading ", "");
			bool isValidInstance = test.SelectSingleNode ("@out_x").InnerText == "1";
			XmlReader xvr = null;
			try {
				XmlTextReader ixtr = new XmlTextReader (
					Path.Combine (basePath, instanceFile));
				xvr = ixtr;
#if NET_2_0
				if (version2) {
					XmlReaderSettings settings =
						new XmlReaderSettings ();
					settings.ValidationType = ValidationType.Schema;
					if (noValidate)
						settings.ValidationEventHandler +=
							noValidateHandler;
					if (noResolver)
						settings.Schemas.XmlResolver = null;
					settings.Schemas.Add (schema);
					if (noResolver)
						settings.XmlResolver = null;
					xvr = XmlReader.Create (ixtr, settings);
				} else {
#endif
					XmlValidatingReader vr = new XmlValidatingReader (ixtr);
					if (noResolver)
						vr.XmlResolver = null;
					if (noValidate)
						vr.ValidationEventHandler += noValidateHandler;
					vr.Schemas.Add (schema);
					xvr = vr;
#if NET_2_0
				}
#endif
				while (!xvr.EOF)
					xvr.Read ();
				if (!isValidInstance && !noValidate)
					Report (instanceFile, false, "should fail", "");
				else if (reportSuccess)
					Report (instanceFile, false, "OK", "");
			} catch (XmlSchemaException ex) {
				if (isValidInstance)
					Report (instanceFile, false, "should succeed",
						reportDetails ?
						ex.ToString () : ex.Message);
				else if (reportSuccess)
					Report (instanceFile, false, "OK", "");
			} catch (Exception ex) {
				if (stopOnError)
					throw;
				Report (instanceFile, false, "unexpected",
					reportDetails ?
					ex.ToString () : ex.Message);
			} finally {
				if (xvr != null)
					xvr.Close ();
			}
		}

		if (reportAsXml) {
			XmlReport.WriteEndElement ();
			XmlReport.Flush ();
		}

		Console.WriteLine ("Finished: " + DateTime.Now);
	}

	void Report (string id, bool compile, string category, string s)
	{
		string phase = compile ? "compile" : "read";
		if (reportAsXml) {
			XmlReport.WriteStartElement ("testresult");
			XmlReport.WriteAttributeString ("id", id);
			XmlReport.WriteAttributeString ("phase", phase);
			XmlReport.WriteAttributeString ("category", category);
			XmlReport.WriteString (s);
			XmlReport.WriteEndElement ();
		}
		else
			ReportWriter.WriteLine ("{0}/{1} : {2} {3}",
				phase, category, id, s);
	}
}
