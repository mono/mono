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
		static bool noExclude;
		static bool reportDetails;
		static bool reportAsXml;
		static bool useDomStyle;
		static bool useDomInstance;
		static bool generateOutput;
		static bool listOutput;
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
			}); 
		}

		static void Usage ()
		{
			Console.WriteLine (@"
mono xslttest.exe [options] [targetFileMatch] -report:reportfile

Options:
	--details	Output detailed output differences.
	--dom		Use XmlDocument for both stylesheet and input source.
	--domxsl	Use XmlDocument for stylesheet.
	--domsrc	Use XmlDocument for input source.
	--generate	Generate output files specified in catalog.
			Use this feature only when you want to update
			reference output.
	--list		Print output list to console.
	--noExclude	Don't exclude meaningless comparison testcases.
	--outall	Output fine results as OK (omitted by default).
	--stoponerror	Stops the test process and throw detailed
			error if happened.
	--ws		Preserve spaces for both stylesheet and input source.
	--wsxsl		Preserve spaces for stylesheet.
	--wssrc		Preserve spaces for input source.
	--xml		Report into xml output.
	--report	Write reports into specified file.

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
				case "--list":
					listOutput = true;
					break;
				case "--noExclude":
					noExclude = true;
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

			if (!noExclude) {
				foreach (string s_ in new StreamReader ("ignore.lst").ReadToEnd ().Split ("\n".ToCharArray ())) {
					string s = s_.Trim ();
					if (s.Length > 0)
						skipTargets.Add (s);
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
			whole.Load (@"testsuite/TESTS/catalog.xml");

			if (!listOutput)
				Console.WriteLine ("Started: " + DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			foreach (XmlElement testCase in whole.SelectNodes (
				"test-suite/test-catalog/test-case")) {
				string stylesheetBase = null;
				string testid = testCase.GetAttribute ("id");
				if (skipTargets.Contains (testid))
					continue;
				try {
					string filePath = testCase.SelectSingleNode ("file-path").InnerText;
					// hack hack
					string testAuthorDir =
						filePath [0] >= 'a' ?
						"Xalan_Conformance_Tests" :
						"MSFT_Conformance_Tests";
					string path = @"testsuite/TESTS/" + testAuthorDir + "/" + filePath + "/";
					foreach (XmlElement scenario in 
						testCase.SelectNodes ("scenario")) {
						RunTest (scenario, path, stylesheetBase);
					}
				} catch (Exception ex) {
					if (stopImmediately)
						throw;
					Report (testCase, "Exception: " + testCase.GetAttribute ("id") + ": " + ex.Message);
				}
			}
			if (!listOutput)
				Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			if (reportAsXml)
				reportXmlWriter.WriteEndElement (); // test-results
		}

		static void RunTest (XmlElement scenario, string path, string stylesheetBase)
		{
			stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
			string id = scenario.ParentNode.Attributes ["id"].Value;
			string stylesheet = path + stylesheetBase;
			string srcxml = path + scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText;
			XmlNode outputNode = scenario.SelectSingleNode ("output-file[@role='principal']");
			string outfile = outputNode != null ? path + outputNode.InnerText : null;

			if (listOutput) {
				if (outfile != null)
					Console.WriteLine (outfile);
				return;
			}

			XslTransform trans = new XslTransform ();

			if (explicitTarget != null && id.IndexOf (explicitTarget) < 0)
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
