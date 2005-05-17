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
#region static Vars
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
		static readonly ArrayList knownFailures = new ArrayList (new string [] { });
		static string explicitTarget;
		static TextWriter reportOutput = Console.Out;
		static XmlTextWriter reportXmlWriter;
		static StreamWriter missingFiles = new StreamWriter ("missing.lst");
		static StreamWriter failedTests = new StreamWriter ("failed.lst");
#endregion

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

		static void ParseOptions (string [] args)
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
		}
			
		static void RunMain (string [] args)
		{
			ParseOptions (args);
			if (!noExclude) {
				foreach (string s_ in new StreamReader ("ignore.lst").ReadToEnd ()
						.Split ("\n".ToCharArray ())) {
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
			whole.Load (@"testsuite/TESTS/catalog-fixed.xml");

			if (!listOutput)
				Console.Error.WriteLine ("Started: " 
						+ DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case"))
				ProcessTestCase (testCase);

			if (!listOutput)
				Console.WriteLine ("Finished: " 
						+ DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

			if (reportAsXml)
				reportXmlWriter.WriteEndElement (); // test-results
		}
		
		static void ProcessTestCase (XmlElement testCase)
		{
			string stylesheetBase = null;
			string testid = testCase.GetAttribute ("id");
			if (skipTargets.Contains (testid))
				return;
			try {
				string submitter = testCase.SelectSingleNode ("./parent::test-catalog/@submitter")
					.InnerText;
				string filePath = testCase.SelectSingleNode ("file-path").InnerText;
				string testAuthorDir;
				if (submitter == "Lotus")
					testAuthorDir =  "Xalan_Conformance_Tests";
				else if (submitter == "Microsoft")
					testAuthorDir =  "MSFT_Conformance_Tests";
				else
					return; //unknown directory

				string path = @"testsuite/TESTS/" + testAuthorDir + "/" + filePath + "/";
				foreach (XmlElement scenario in 
						testCase.SelectNodes ("scenario[@operation='standard']")) {
					RunTest (testid, scenario, path, stylesheetBase);
				}
			} catch (Exception ex) {
				if (stopImmediately)
					throw;
				Report (false, testid, "Exception: " + ex.Message);
			}
		}

		static void RunTest (string testid, XmlElement scenario, string path, string stylesheetBase)
		{
			stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
			string stylesheet = path + stylesheetBase;
			
			if (!File.Exists (stylesheet)) {
				missingFiles.WriteLine (stylesheet);
				missingFiles.Flush ();
			}
			string srcxml = path + scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText;
			XmlNode outputNode = scenario.SelectSingleNode ("output-file[@role='principal']");
			string outfile = outputNode != null ? path + outputNode.InnerText : null;

			if (listOutput) {
				if (outfile != null)
					Console.WriteLine (outfile);
				return;
			}

			XslTransform trans = new XslTransform ();

			if (explicitTarget != null && testid.IndexOf (explicitTarget) < 0)
				return;
			if (skipTargets.Contains (stylesheetBase))
				return;

			if (useDomStyle) {
				XmlDocument styledoc = new XmlDocument ();
				if (whitespaceStyle)
					styledoc.PreserveWhitespace = true;
				styledoc.Load (stylesheet);
				trans.Load (styledoc, null, null);
			} else
				trans.Load (new XPathDocument (
							stylesheet,
							whitespaceStyle ? XmlSpace.Preserve :
							XmlSpace.Default),
						null, null);

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
			trans.Transform (input, null, sw, null);
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
				Report (true, testid, "No reference file found");
				return;
			}
			StreamReader sr = new StreamReader (outfile);
			string reference_out = sr.ReadToEnd ();
			string actual_out = sw.ToString ();
			if (reference_out != actual_out)
				Report (false, testid, reference_out, actual_out);
			else if (outputAll)
				Report (true, testid, "OK");
		}

		static void Report (bool passed, string testid, string message)
		{
			if (passed) {
				Console.Error.Write (".");
				return;
			}
 			failedTests.WriteLine (testid + "\t" + message);
 			failedTests.Flush ();
			if (reportAsXml) {
				reportXmlWriter.WriteStartElement ("testcase");
				reportXmlWriter.WriteAttributeString ("id", testid);
				reportXmlWriter.WriteString (message);
				reportXmlWriter.WriteEndElement ();
				if (knownFailures.Contains (testid))
					Console.Error.Write ("k");
				else
					Console.Error.Write ("E");
			}
			else
				reportOutput.WriteLine (message);
		}

		static void Report (bool passed, string testid, string reference_out, string actual_out)
		{
			string baseMessage = reportAsXml ? "Different." : "Different: " + testid;
			if (!reportDetails)
				Report (passed, testid, baseMessage);
			else
				Report (passed, testid, baseMessage +
					"\n Actual*****\n" + 
					actual_out + 
					"\n-------------------\nReference*****\n" + 
					reference_out + 
					"\n");
		}
	}
}
