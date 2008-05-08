using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace TestMonkey
{
	public class NUnitReport
	{
		private static string input_dir = string.Empty;
		private static string output_file = string.Empty;
		
		static void Main (string[] args)
		{
			if (args.Length != 2) {
				Console.WriteLine ("Expected Usage:");
				Console.WriteLine ("  mono NUnitReport.exe input_directory output_file");
				return;
			}
			
			// Get our input directory and our output file
			input_dir = args[0];
			output_file = args[1];
			
			// Start the output file
			StreamWriter sw = new StreamWriter (output_file);
			StartReport (sw);
			
			int assembly = 0;
			int fail_total = 0;
			int pass_total = 0;
			int run_total = 0;
			
			// Loop through the inputs, outputting the results to the output file
			foreach (string file in Directory.GetFiles (input_dir)) {
				assembly++;
				
				Dictionary<string, FailedTest> failed_tests = new Dictionary<string,FailedTest> ();
				List<string> ignored_tests = new List<string> ();
				
				int tests_passed = PopulateFailureTable (file, failed_tests, ignored_tests);

				fail_total += failed_tests.Count;
				pass_total += tests_passed;
				run_total += failed_tests.Count + tests_passed;
				
				if (failed_tests.Count > 0) {
					sw.WriteLine ("        <tr class='errorrow' onclick=\"toggle('el{0}')\" onmouseover='highlight(this)' onmouseout='unhighlight(this)'>", assembly);
					sw.WriteLine (@"            <td><img src='Media/fail.png' /></td>");
				} else {
					sw.WriteLine (@"        <tr>");
					sw.WriteLine (@"            <td><img src='Media/pass.png' /></td>");
				}
				
				sw.WriteLine (@"            <td>{0}</td>", Path.GetFileName (file));
				sw.WriteLine (@"            <td>{0}</td>", failed_tests.Count);
				sw.WriteLine (@"            <td>{0}</td>", tests_passed);
				sw.WriteLine (@"            <td>{0}</td>", tests_passed + failed_tests.Count);
				sw.WriteLine (@"        </tr>");
				
				if (failed_tests.Count == 0)
					continue;

				sw.WriteLine (@"        <tr id='el{0}' class='errorlist' style='display: none'>", assembly);
				sw.WriteLine (@"            <td></td>");
				sw.WriteLine (@"            <td colspan='4'>");
				sw.WriteLine (@"                <table cellpadding='2' cellspacing='0' width='100%'>");
					
				int test_num = 0;
				
				foreach (FailedTest ft in failed_tests.Values) {
					sw.WriteLine ("                    <tr onclick=\"toggle('as{0}ed{1}')\" onmouseover='highlight(this)' onmouseout='unhighlight(this)'>", assembly, test_num);
					sw.WriteLine (@"                        <td style='width: 17px'><img src='Media/bullet.png' /></td>");
					sw.WriteLine (@"                        <td>{0}</td>", ft.Name);
					sw.WriteLine (@"                    </tr>");
					sw.WriteLine (@"                    <tr id='as{0}ed{1}' class='errordetail' style='display: none'>", assembly, test_num);
					sw.WriteLine (@"                        <td></td>");
					sw.WriteLine (@"                        <td>");
					sw.WriteLine (@"{0}", ft.Message.Trim ().Trim ('\n').Replace ("\n", "<br/>"));
					if (!string.IsNullOrEmpty (ft.StackTrace.Trim ()))
						sw.WriteLine (@"<br /><br /><strong>StackTrace:</strong><br />{0}", ft.StackTrace.Replace ("\n", "<br/>"));
					sw.WriteLine (@"                        </td>");
					sw.WriteLine (@"                    </tr>");
					
					test_num++;
				}

				sw.WriteLine (@"                </table>");
				sw.WriteLine (@"            </td>");
				sw.WriteLine (@"        </tr>");
			}
			
			// Write totals
			WriteTotals (sw, fail_total, pass_total, run_total);
			
			// Finish up the output file
			FinishReport (sw);
			sw.Close ();
			sw.Dispose ();
		}

		public static int PopulateFailureTable (string filename, Dictionary<string, FailedTest> output, List<string> ignored)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (filename);

			return FindTestCases (doc.DocumentElement, output, ignored);
		}

		public static int FindTestCases (XmlElement xe, Dictionary<string, FailedTest> output, List<string> ignored)
		{
			if (xe.Name == "test-case") {
				OutputFailedTestCase (xe, output, ignored);
				return 1;
			}

			int i = 0;
			
			foreach (XmlElement child in xe.ChildNodes)
				i += FindTestCases (child, output, ignored);
				
			return i;
		}

		public static void OutputFailedTestCase (XmlElement xe, Dictionary<string, FailedTest> output, List<string> ignored)
		{
			if (xe.GetAttribute ("executed") == "False")
				ignored.Add (xe.GetAttribute ("name"));

			if (xe.GetAttribute ("success") == "True" || xe.GetAttribute ("executed") == "False")
				return;

			FailedTest ft = new FailedTest (xe.GetAttribute ("name"), xe["failure"]["message"].InnerText, xe["failure"]["stack-trace"].InnerText);
			output[ft.Name] = ft;
		}

		public static void StartReport (StreamWriter sw)
		{
			sw.WriteLine (@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">");
			sw.WriteLine (@"<html xmlns=""http://www.w3.org/1999/xhtml""><head>");
			sw.WriteLine (@"<title>Mono: Class Libraries NUnit Test Results</title>");
			sw.WriteLine (@"<link rel=""stylesheet"" type=""text/css"" href=""Media/style.css"" />");
			sw.WriteLine (@"<script type=""text/ecmascript"" src=""Media/scripts.js""></script></head>");
			sw.WriteLine (@"<body>");
			sw.WriteLine (@"    <div class='header'>");
			sw.WriteLine (@"        <div class='headerimage'>");
			sw.WriteLine (@"            <img src='Media/rupert.png' width='48' height='48' />");
			sw.WriteLine (@"        </div>");
			sw.WriteLine (@"        <div class='headertext'>Class Libraries NUnit Test Results</div>");
			sw.WriteLine (@"    </div>");
			sw.WriteLine (@"    <div class='legend'>");
			sw.WriteLine (@"        Generated:<br />");
			sw.WriteLine (@"        {0}<br /><br />", DateTime.Now.ToString ());
			sw.WriteLine (@"        Click on failure row for more details.<br /><br />");
			sw.WriteLine (@"        Icons courtesy of <a href='http://www.famfamfam.com/lab/icons/silk'>famfamfam</a>");
			sw.WriteLine (@"    </div>");
			sw.WriteLine (@"    <table cellpadding='2' cellspacing='0' class='maintable'>");
			sw.WriteLine (@"        <tr class='tableheader'>");
			sw.WriteLine (@"            <td style='width: 17px'></td>");
			sw.WriteLine (@"            <td>Tested Assembly</td>");
			sw.WriteLine (@"            <td>Failed</td>");
			sw.WriteLine (@"            <td>Passed</td>");
			sw.WriteLine (@"            <td>Run</td>");
			sw.WriteLine (@"        </tr>");
		}

		public static void WriteTotals (StreamWriter sw, int failed, int passed, int run)
		{
			sw.WriteLine (@"        <tr class='tabletotal'>");
			sw.WriteLine (@"            <td style='width: 17px'></td>");
			sw.WriteLine (@"            <td>Totals</td>");
			sw.WriteLine (@"            <td>{0}</td>", failed);
			sw.WriteLine (@"            <td>{0}</td>", passed);
			sw.WriteLine (@"            <td>{0}</td>", run);
			sw.WriteLine (@"        </tr>");
		}
		
		public static void FinishReport (StreamWriter sw)
		{
			sw.WriteLine (@"    </table>");
			sw.WriteLine (@"</body>");
			sw.WriteLine (@"</html>");
		}
	}
}
