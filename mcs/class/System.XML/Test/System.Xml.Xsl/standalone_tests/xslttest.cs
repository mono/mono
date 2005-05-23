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
		static string outputDir;
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

		enum TestResult
		{
			Crash,		//exception
			Failure,	//no exception but output is different
			Unknown,	//no exception but expected result is unknown
			Success,	//no exception and output is as expected
		};
		
		static XsltTest ()
		{
			skipTargets = new ArrayList (new string [] {
			}); 
		}
		
		#region Options handling
		static void Usage ()
		{
			Console.Error.WriteLine (@"
mono xslttest.exe [options] [targetFileMatch] -report:reportfile

Options:
	--details	Output detailed output differences.
	--dom		Use XmlDocument for both stylesheet and input source.
	--domxsl	Use XmlDocument for stylesheet.
	--domsrc	Use XmlDocument for input source.
	--generate	Generate output files specified in catalog.
			Use this feature only when you want to update
			reference output.
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
							Console.Error.WriteLine ("Error: --report option requires filename.");
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
			if (useDomStyle || useDomInstance)
				outputDir = "domresults";
			else
				outputDir = "results";
		}
		#endregion
			
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
			ParseOptions (args);
			if (!noExclude) {
				using (StreamReader ignoreReader = new StreamReader ("ignore.lst")) {
					foreach (string s_ in ignoreReader.ReadToEnd ()
						.Split ("\n".ToCharArray ())) {
						string s = s_.Trim ();
						if (s.Length > 0)
							skipTargets.Add (s);
					}
				}
				using (StreamReader knownReader = new StreamReader ("knownFailures.lst")) {
					foreach (string s_ in knownReader.ReadToEnd ()
						.Split ("\n".ToCharArray ())) {
						string s = s_.Trim ();
						if (s.Length > 0)
							knownFailures.Add (s);
					}
				}
			}

			if (reportAsXml) {
				reportXmlWriter = new XmlTextWriter (reportOutput);
				reportXmlWriter.Formatting = Formatting.Indented;
				reportXmlWriter.WriteStartElement ("test-results");
			}

			if (explicitTarget != null)
				Console.Error.WriteLine ("The specified target is "
					+ explicitTarget);

			XmlDocument whole = new XmlDocument ();
			whole.Load (@"testsuite/TESTS/catalog-fixed.xml");

			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case"))
				ProcessTestCase (testCase);

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

				string relPath = Path.Combine (testAuthorDir, filePath);
				string path = Path.Combine ("testsuite/TESTS", relPath);
				string outputPath = Path.Combine (outputDir, relPath);
				if (!Directory.Exists (outputPath))
					Directory.CreateDirectory (outputPath);
				foreach (XmlElement scenario in 
						testCase.SelectNodes ("scenario[@operation='standard']")) {
					RunTest (testid, scenario, path, outputPath, stylesheetBase);
				}
			} catch (Exception ex) {
				if (stopImmediately)
					throw;
				Report (TestResult.Crash, testid, "Exception: " + ex.Message);
			}
		}

		static void RunTest (string testid, XmlElement scenario, string path, string outputPath,
				     string stylesheetBase)
		{
			stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']")
				.InnerText;
			string stylesheet = Path.Combine (path, stylesheetBase);
			
			if (!File.Exists (stylesheet)) {
				missingFiles.WriteLine (stylesheet);
				missingFiles.Flush ();
			}
			string srcxml = Path.Combine (path,
				scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText);
			XmlNode outputNode = scenario.SelectSingleNode ("output-file[@role='principal']");
			string outfile = null;
			if (outputNode != null) 
				outfile = Path.Combine (outputPath, outputNode.InnerText);

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
			string swString;
			using (StringWriter sw = new StringWriter ()) {
				trans.Transform (input, null, sw, null);
				swString = sw.ToString ();
			}
			if (generateOutput) {
				using (StreamWriter fw = new StreamWriter (outfile,
						   false, Encoding.UTF8)) {
					fw.Write (swString);
					fw.Close ();
				}
				Report (TestResult.Success, testid, "Created reference result");
				// ... and don't run comparison
				return;
			}

			if (!File.Exists (outfile)) {
				// Reference output file does not exist.
				Report (TestResult.Unknown, testid, "No reference file found");
				return;
			}
			string reference_out;
			string actual_out;
			using (StreamReader sr = new StreamReader (outfile)) {
				reference_out = sr.ReadToEnd ().Replace ("\r\n","\n");
				actual_out = swString.Replace ("\r\n","\n");
			}
			if (reference_out != actual_out)
				Report (TestResult.Failure, testid, reference_out, actual_out);
			else if (outputAll)
				Report (TestResult.Success, testid, "OK");
		}

		static void Report (TestResult res, string testid, string message)
		{
			if (TestResult.Success == res) {
				Console.Error.Write (".");
				return;
			}
			else if (TestResult.Unknown == res) {
				Console.Error.Write ("?");
				return;
			}
 			
			if (knownFailures.Contains (testid))
				Console.Error.Write ("k");
			else {
				failedTests.WriteLine (testid + "\t" + message);
				failedTests.Flush ();

				if (TestResult.Crash == res)
					   Console.Error.Write ("E");
				else
					   Console.Error.Write ("e");
			}
			
			if (reportAsXml) {
				reportXmlWriter.WriteStartElement ("testcase");
				reportXmlWriter.WriteAttributeString ("id", testid);
				reportXmlWriter.WriteString (message);
				reportXmlWriter.WriteEndElement ();
			}
			
		}

		static void Report (TestResult res, string testid, string reference_out, string actual_out)
		{
			string baseMessage = reportAsXml ? "Different." : "Different: " + testid;
			if (!reportDetails)
				Report (res, testid, baseMessage);
			else
				Report (res, testid, baseMessage +
					"\n Actual*****\n" + 
					actual_out + 
					"\n-------------------\nReference*****\n" + 
					reference_out + 
					"\n");
		}
	}
}
