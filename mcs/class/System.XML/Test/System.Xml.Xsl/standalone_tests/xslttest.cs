using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace XsltTest
{
	public class XsltTest
	{
		static bool reportDetails;
		static bool reportAsXml;
		static bool useDomStyle;
		static bool useDomInstance;
		static bool generateOutput;
		static bool whitespaceStyle;
		static bool whitespaceInstance;
		static bool stopImmediately;
		static bool outputAll;
		static readonly ArrayList skipTargets;
		static string explicitTarget;
		static TextWriter reportOutput = Console.Out;
		static XmlTextWriter reportXmlWriter;

		static XsltTest ()
		{
			skipTargets = new ArrayList (new string [] {
"attribset15.xsl",
"lre12.xsl", 
"namespace23.xsl", // under .NET, XPathDocument behavior is different from dom
"namespace40.xsl", 
"namespace42.xsl", 
"namespace43.xsl",
"namespace48.xsl", 
"namespace60.xsl", 
"namespace73.xsl", 
"namespace106.xsl",
// output22,77: not-supported encoding, but MS passes...?
// output72.xsl: should not pass
"output22.xsl", 
"output72.xsl",
"output77.xsl"
			}); 
		}

		static void Usage ()
		{
			Console.WriteLine (@"mono xslttest.exe [options] [targetFileMatch] -report:reportfile

	Options:
		--details : Output detailed output differences.
		--dom : use XmlDocument for both stylesheet and input source.
		--domxsl : use XmlDocument for stylesheet.
		--domsrc : use XmlDocument for input source.
		--generate : generate output files specified in catalog.
				Use this feature only when you want to update
				reference output.
		--outall : Output fine results as OK (omitted by default).
		--stoponerror : stops the test process and throw detailed
			error if happened.
		--ws : preserve spaces for both stylesheet and input source.
		--wsxsl : preserve spaces for stylesheet.
		--wssrc : preserve spaces for input source.
		--xml : report into xml output.
		--report : write reports into specified file.

	FileMatch:
		arbitrary string that specifies part of file name.
		(no * or ? available)
");
		}

		public static void Main (string [] args)
		{
			try {
				RunMain (args);
			} catch (Exception ex) {
				reportOutput.WriteLine (ex);
			} finally {
				reportOutput.Close ();
			}
		}

		static void RunMain (string [] args)
		{
			foreach (string arg in args) {
				switch (arg) {
				case "-?":
					Usage ();
					return;
				case "--dom":
					useDomStyle = true;
					useDomInstance = true;
					break;
				case "--domxsl":
					useDomStyle = true;
					break;
				case "--domsrc":
					useDomInstance = true;
					break;
				case "--details":
					reportDetails = true;
					break;
				case "--generate":
					generateOutput = true;
					break;
				case "--outall":
					outputAll = true;
					break;
				case "--stoponerror":
					stopImmediately = true;
					break;
				case "--ws":
					whitespaceStyle = true;
					whitespaceInstance = true;
					break;
				case "--wsxsl":
					whitespaceStyle = true;
					break;
				case "--wssrc":
					whitespaceInstance = true;
					break;
				case "--xml":
					reportAsXml = true;
					break;
				default:
					if (arg.StartsWith ("--report:")) {
						string reportFile = arg.Substring (9);
						if (reportFile.Length < 0) {
							Usage ();
							Console.WriteLine ("Error: --report option requires filename.");
							return;
						}
						reportOutput = new StreamWriter (reportFile);
						break;
					}
					if (arg.StartsWith ("--")) {
						Usage ();
						return;
					}
					explicitTarget = arg;
					break;
				}
			}

			if (reportAsXml) {
				reportXmlWriter = new XmlTextWriter (reportOutput);
				reportXmlWriter.Formatting = Formatting.Indented;
				reportXmlWriter.WriteStartElement ("test-results");
			}

			if (explicitTarget != null)
				Console.WriteLine ("The specified target is "
					+ explicitTarget);

			XmlDocument whole = new XmlDocument ();
			whole.Load (@"testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml");

			Console.WriteLine ("Started: " + 
				DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			foreach (XmlElement testCase in whole.SelectNodes (
				"test-suite/test-catalog/test-case")) {
				string stylesheetBase = null;
				try {
					string filePath = testCase.SelectSingleNode ("file-path").InnerText;
					string path = @"testsuite/TESTS/Xalan_Conformance_Tests/" + filePath + "/";
					foreach (XmlElement scenario in 
						testCase.SelectNodes ("scenario")) {
						RunTest (scenario, path, stylesheetBase);
					}
				} catch (Exception ex) {
					if (skipTargets.Contains (stylesheetBase))
						continue;
					if (stopImmediately)
						throw;
					Report (testCase, "Exception: " + testCase.GetAttribute ("id") + ": " + ex.Message);
				}
			}
Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			if (reportAsXml)
				reportXmlWriter.WriteEndElement (); // test-results
		}

		static void RunTest (XmlElement scenario, string path, string stylesheetBase)
		{
			XslTransform trans = new XslTransform ();
			stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
			string stylesheet = path + stylesheetBase;
			string srcxml = path + scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText;

			if (explicitTarget != null && stylesheetBase.IndexOf (explicitTarget) < 0)
				return;
			if (skipTargets.Contains (stylesheetBase))
				return;

			XmlTextReader stylextr = new XmlTextReader (stylesheet);
			XmlValidatingReader stylexvr = new XmlValidatingReader (stylextr);
			if (useDomStyle) {
				XmlDocument styledoc = new XmlDocument ();
				if (whitespaceStyle)
					styledoc.PreserveWhitespace = true;
				styledoc.Load (stylesheet);
				trans.Load (styledoc);
			} else
				trans.Load (new XPathDocument (
					stylesheet,
					whitespaceStyle ? XmlSpace.Preserve :
					XmlSpace.Default));

			string outfile = path + scenario.SelectSingleNode ("output-file[@role='principal']").InnerText;

			XmlTextReader xtr = new XmlTextReader (srcxml);
			XmlValidatingReader xvr = new XmlValidatingReader (xtr);
			xvr.ValidationType = ValidationType.None;
			IXPathNavigable input = null;
			if (useDomInstance) {
				XmlDocument dom = new XmlDocument ();
				if (whitespaceInstance)
					dom.PreserveWhitespace = true;
				dom.Load (xvr);
				input = dom;
			} else {
				input = new XPathDocument (xvr,
					whitespaceStyle ? XmlSpace.Preserve :
					XmlSpace.Default);
			}
			StringWriter sw = new StringWriter ();
			trans.Transform (input, null, sw);
			if (generateOutput) {
				StreamWriter fw = new StreamWriter (outfile,
					false, Encoding.UTF8);
				fw.Write (sw.ToString ());
				fw.Close ();
				// ... and don't run comparison
				return;
			}

			if (!File.Exists (outfile)) {
				// Reference output file does not exist.
				return;
			}
			StreamReader sr = new StreamReader (outfile);
			string reference_out = sr.ReadToEnd ();
			string actual_out = sw.ToString ();
			if (reference_out != actual_out)
				Report (scenario.ParentNode as XmlElement, 
					reference_out, actual_out);
			else if (outputAll)
				Report (scenario.ParentNode as XmlElement,
					"OK");
		}

		static void Report (XmlElement testcase, string message)
		{
			if (reportAsXml) {
				reportXmlWriter.WriteStartElement ("testcase");
				reportXmlWriter.WriteAttributeString ("id",
					testcase.GetAttribute ("id"));
				reportXmlWriter.WriteString (message);
				reportXmlWriter.WriteEndElement ();
			}
			else
				reportOutput.WriteLine (message);
		}

		static void Report (XmlElement testCase,
			string reference_out, string actual_out)
		{
			string baseMessage = reportAsXml ? "Different." :
				"Different: " + testCase.GetAttribute ("id");
			if (!reportDetails)
				Report (testCase, baseMessage);
			else
				Report (testCase, baseMessage +
					"\n Actual*****\n" + 
					actual_out + 
					"\n-------------------\nReference*****\n" + 
					reference_out + 
					"\n");
		}
	}
}
