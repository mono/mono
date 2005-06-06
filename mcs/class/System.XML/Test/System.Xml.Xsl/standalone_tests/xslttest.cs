using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace XsltTest
{
	public class XsltTest: IDisposable
	{
		#region Options Vars
		bool noExclude;
		bool reportDetails;
		bool reportAsXml;
		bool useDomStyle;
		bool useDomInstance;
		bool generateOutput;
		string outputDir;
		bool whitespaceStyle;
		bool whitespaceInstance;
		bool stopImmediately;
		bool outputOnlyErrors;
		bool runSlow = false;
		#endregion

		#region statistics fields
		int totalCount = 0;
		int performedCount = 0;
		int passedCount = 0;
		int failedCount = 0;
		int differentCount = 0;
		int exceptionCount = 0;
		int regressionsCount = 0; //failures not listed in knownFailures.lst
		int fixedCount = 0; //tested known to fail that passed
		#endregion

		#region test list fields
		ArrayList netExceptions = new ArrayList ();
		ArrayList skipTargets = new ArrayList ();
		ArrayList slowTests = new ArrayList ();
		ArrayList knownFailures = new ArrayList ();
		ArrayList fixmeList = new ArrayList ();
		StreamWriter slowNewList;
		StreamWriter missingFiles;
		StreamWriter failedTests;
		StreamWriter fixedTests;
		StreamWriter netExceptionsWriter;
		#endregion

		#region IDisposable Members
		public void Dispose() {
			if (reportXmlWriter != null)
				reportXmlWriter.Close ();
			if (slowNewList != null)
				slowNewList.Close();
			if (missingFiles != null)
				missingFiles.Close ();
			if (failedTests != null)
				failedTests.Close ();
			if (fixedTests != null)
				fixedTests.Close ();
			if (netExceptionsWriter != null)
				netExceptionsWriter.Close ();
			reportXmlWriter = null;
			slowNewList = null;
			missingFiles = null;
			failedTests = null;
			fixedTests = null;
			netExceptionsWriter = null;
		}

		#endregion

		string explicitTarget;
		TextWriter reportOutput = Console.Out;
		XmlTextWriter reportXmlWriter;

		#region ReadStrings ()
		static void ReadStrings (ArrayList array, string filename) {
			if (!File.Exists (filename))
				return;

			using (StreamReader reader = new StreamReader (filename)) {
				foreach (string s_ in reader.ReadToEnd ().Split ("\n".ToCharArray ())) {
					string s = s_.Trim ();
					if (s.Length > 0)
						array.Add (s);
				}
			}
		}
		#endregion

		enum TestResult
		{
			Crash,		//exception
			Failure,	//no exception but output is different
			Unknown,	//no exception but expected result is unknown
			Success,	//no exception and output is as expected
		};
		
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
	--outErrors	Output only error results (don't print dots).
	--stoponerror	Stops the test process and throw detailed
			error if happened.
	--ws		Preserve spaces for both stylesheet and input source.
	--wsxsl		Preserve spaces for stylesheet.
	--wssrc		Preserve spaces for input source.
	--xml		Report into xml output.
	--report	Write reports into specified file.
	--run-slow	Run all tests, including slow ones.

FileMatch:
	arbitrary string that specifies part of file name.
	(no * or ? available)
");
		}

		void ParseOptions ()
		{
			foreach (string arg in _args) {
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
				case "--outErrors":
					outputOnlyErrors = true;
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
				case "--run-slow":
					runSlow = true;
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
			
		public static int Main (string [] args)
		{
			using (XsltTest test = new XsltTest(args)) {
				if (!test.Run ())
					return 1;
				return 0;
			}
		}

		string [] _args;

		XsltTest (string [] args) 
		{
			_args = args;
		}

		bool Run ()
		{
			ParseOptions ();
			string netExceptionsFilename = Path.Combine (outputDir, "net-exceptions.lst");
			if (!Directory.Exists (outputDir))
				Directory.CreateDirectory (outputDir);

			if (!noExclude) {
				ReadStrings (slowTests, "slow.lst");
				ReadStrings (skipTargets, "ignore.lst");
                ReadStrings (knownFailures, "knownFailures.lst");
				ReadStrings (fixmeList, "fixme.lst");
				ReadStrings (netExceptions, netExceptionsFilename);
			}

			slowNewList = new StreamWriter ("new-slow.lst");
			missingFiles = new StreamWriter ("missing.lst");
			failedTests = new StreamWriter ("failed.lst");
			fixedTests = new StreamWriter ("fixed.lst");

			if (generateOutput)
				netExceptionsWriter = new StreamWriter (netExceptionsFilename);

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
			bool res = true;

			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case")) {
				string testId = testCase.GetAttribute ("id");
				totalCount ++;
				DateTime start = DateTime.Now;
				if (!ProcessTestCase (testCase))
					res = false;
				TimeSpan span = DateTime.Now - start;
				if (span.TotalSeconds > 1) {
					if (slowTests.Contains (testId))
						continue;
					slowNewList.WriteLine (testId);
				}
			}
			if (reportAsXml)
				reportXmlWriter.WriteEndElement (); // test-results

			Console.Error.WriteLine ("\n\n*********");
			Console.Error.WriteLine ("Total:{0}", totalCount);
			Console.Error.WriteLine (" Performed:{0}", performedCount);
			Console.Error.WriteLine ("  Passed:{0}", passedCount);
			Console.Error.WriteLine ("   Fixed:{0}\n", fixedCount);
			Console.Error.WriteLine ("  Failed:{0}", failedCount);
			Console.Error.WriteLine ("   Different:{0}", differentCount);
			Console.Error.WriteLine ("   Exceptions:{0}", exceptionCount);
			Console.Error.WriteLine ("   Regressions:{0}", regressionsCount);

			if (fixedCount > 0)
				Console.Error.WriteLine (@"

ATTENTION!
You must delete the fixed tests (those listed in fixed.lst) from
knownFailures.lst or fixme.lst. If you don't do it, you can miss
regressions in the future.");

			if (regressionsCount > 0)
				Console.Error.WriteLine (@"

ERROR!!! New regressions!
If you see this message for the first time, your last changes had
introduced new bugs! Before you commit, you must do one of the following:

1. Find and fix the bugs, so tests will pass again.
2. Open new bugs in bugzilla and temporily add the tests to fixme.lst
3. Write to devlist and confirm adding the new tests to knownFailures.lst");

			return res;
		}
		
		bool ProcessTestCase (XmlElement testCase)
		{
			string stylesheetBase = null;
			string testid = testCase.GetAttribute ("id");
			if (skipTargets.Contains (testid))
				return true;
			if (!runSlow && slowTests.Contains (testid))
				return true;
			bool res = true;
			try {
				performedCount ++;
				string submitter = testCase.SelectSingleNode ("./parent::test-catalog/@submitter")
					.InnerText;
				string filePath = testCase.SelectSingleNode ("file-path").InnerText;
				string testAuthorDir;
				if (submitter == "Lotus")
					testAuthorDir =  "Xalan_Conformance_Tests";
				else if (submitter == "Microsoft")
					testAuthorDir =  "MSFT_Conformance_Tests";
				else
					return true; //unknown directory

				string relPath = Path.Combine (testAuthorDir, filePath);
				string path = Path.Combine ("testsuite/TESTS", relPath);
				string outputPath = Path.Combine (outputDir, relPath);
				if (!Directory.Exists (outputPath))
					Directory.CreateDirectory (outputPath);
				foreach (XmlElement scenario in 
						testCase.SelectNodes ("scenario[@operation='standard']")) {
					if (!RunTest (testid, scenario, path, outputPath, stylesheetBase))
						res = false;
				}
			} catch (Exception ex) {
				if (stopImmediately)
					throw;
				if (!Report (TestResult.Crash, testid, "Exception: " + ex.Message))
					res = false;
			}
			return res;
		}

		bool RunTest (string testid, XmlElement scenario, string path, string outputPath,
				     string stylesheetBase)
		{
			stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']")
				.InnerText;
			string stylesheet = Path.Combine (path, stylesheetBase);
			
			if (!File.Exists (stylesheet)) {
				missingFiles.WriteLine (stylesheet);
			}
			string srcxml = Path.Combine (path,
				scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText);
			XmlNode outputNode = scenario.SelectSingleNode ("output-file[@role='principal']");
			string outfile = null;
			if (outputNode != null) 
				outfile = Path.Combine (outputPath, outputNode.InnerText);

			XslTransform trans = new XslTransform ();

			if (explicitTarget != null && testid.IndexOf (explicitTarget) < 0)
				return true;
			if (skipTargets.Contains (stylesheetBase))
				return true;

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
			
			string swString;
			XmlTextReader xtr = new XmlTextReader (srcxml);

			try {
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
				using (StringWriter sw = new StringWriter ()) {
					trans.Transform (input, null, sw, null);
					swString = sw.ToString ();
				}
			}
			finally {
				xtr.Close ();
			}
			if (generateOutput) {
				using (StreamWriter fw = new StreamWriter (outfile,
						   false, Encoding.UTF8)) {
					fw.Write (swString);
					fw.Close ();
				}
				return Report (TestResult.Success, testid, "Created reference result");
				// ... and don't run comparison
			}

			if (!File.Exists (outfile)) {
				// Reference output file does not exist.
				return Report (TestResult.Unknown, testid, "No reference file found");
			}
			string reference_out;
			string actual_out;
			using (StreamReader sr = new StreamReader (outfile)) {
				reference_out = sr.ReadToEnd ().Replace ("\r\n","\n");
				actual_out = swString.Replace ("\r\n","\n");
			}
			if (reference_out != actual_out)
				return Report (TestResult.Failure, testid, reference_out, actual_out);
			else if (!outputOnlyErrors)
				return Report (TestResult.Success, testid, "OK");
			else
				return true;
		}

		bool Report (TestResult res, string testid, string message)
		{
			if (TestResult.Success == res || TestResult.Unknown == res) {
				passedCount ++;
				if (fixmeList.Contains (testid) || knownFailures.Contains (testid)) {
					fixedCount ++;
					fixedTests.WriteLine (testid);
					Console.Error.Write ("!");
					return true;
				}
				if (netExceptions.Contains (testid))
					Console.Error.Write (",");
				else if (TestResult.Success == res)
					Console.Error.Write (".");
				else
					Console.Error.Write ("?");
				return true;
			}

			bool return_res = true;

			failedCount ++;
			if (TestResult.Crash == res)
				exceptionCount ++;
			else if (TestResult.Failure == res)
				differentCount ++;
 			
			if (knownFailures.Contains (testid) || fixmeList.Contains (testid))
				Console.Error.Write ("k");
			else if (res == TestResult.Crash && netExceptions.Contains (testid))
				Console.Error.Write ("K"); 
			else {
				regressionsCount ++;
				if (reportAsXml) {
					reportXmlWriter.WriteStartElement ("testcase");
					reportXmlWriter.WriteAttributeString ("id", testid);
					reportXmlWriter.WriteString (message);
					reportXmlWriter.WriteEndElement ();
				}
				return_res = false;
                failedTests.WriteLine (testid + "\t" + message);

				if (TestResult.Crash == res) {
					Console.Error.Write ("E");
					if (generateOutput)
						netExceptionsWriter.WriteLine (testid);
				}
				else
					Console.Error.Write ("e");
			}
			
			return return_res;
		}

		bool Report (TestResult res, string testid, string reference_out, string actual_out)
		{
			string baseMessage = reportAsXml ? "Different." : "Different: " + testid;
			if (!reportDetails)
				return Report (res, testid, baseMessage);
			else
				return Report (res, testid, baseMessage +
					"\n Actual*****\n" + 
					actual_out + 
					"\n-------------------\nReference*****\n" + 
					reference_out + 
					"\n");
		}
	}
}
