//
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;

using Mono.Options;

using StandAloneRunnerSupport;

namespace StandAloneRunner
{
	sealed class StandAloneRunner
	{
		static string testsAssembly;
		
		static void Usage (int exitCode, OptionSet options)
		{
			Console.WriteLine (@"Usage: standalone-runner [OPTIONS] <TESTS_ASSEMBLY>");
			if (options != null) {
				Console.WriteLine ();
				Console.WriteLine ("Available options:");
				options.WriteOptionDescriptions (Console.Out);
			}
			Console.WriteLine ();
			
			Environment.Exit (exitCode);
		}
		
		public static void Die (string format, params object[] args)
		{
			Console.WriteLine ("Standalone test failure in assembly '{0}'.", testsAssembly);
			Console.WriteLine ();
			
			if (args == null || args.Length == 0)
				Console.WriteLine ("Error: " + format);
			else
				Console.WriteLine ("Error: " + format, args);

			Environment.Exit (1);
		}
		
		static void Main (string[] args)
		{
			try {
				Run (args);
			} catch (Exception ex) {
				Die ("Exception caught:{0}{1}", Environment.NewLine, ex.ToString ());
			}
		}

		static void Run (string[] args)
		{
			bool showHelp = false;
			string testName = null;
			
			var options = new OptionSet () {
				{"?|h|help", "Show short usage screen.", v => showHelp = true},
				{"t=|test=", "Run this test only (full type name)", (string s) => testName = s},
			}; 
			
			List <string> extra = options.Parse (args);
			int extraCount = extra.Count;

			if (showHelp || extraCount < 1)
				Usage (showHelp ? 0 : 1, options);

			testsAssembly = extra [0];
			
			if (!File.Exists (testsAssembly))
				Die ("Tests assembly '{0}' does not exist.", testsAssembly);
			
			Assembly asm = Assembly.LoadFrom (testsAssembly);
			var tests = new List <StandaloneTest> ();

			LoadTestsFromAssembly (asm, tests);
			if (tests.Count == 0)
				Die ("No tests present in the '{0}' assembly. Tests must be public types decorated with the TestCase attribute and implementing the ITestCase interface.", testsAssembly);

			ApplicationManager appMan = ApplicationManager.GetApplicationManager ();
			int runCounter = 0;
			int failedCounter = 0;
			var reports = new List <string> ();
			
			Console.WriteLine ("Running tests:");
			DateTime start = DateTime.Now;
			
			foreach (StandaloneTest test in tests) {
				if (test.Info.Disabled)
					continue;

				if (!String.IsNullOrEmpty (testName)) {
					if (String.Compare (test.TestType.FullName, testName) != 0)
						continue;
				}
				
				test.Run (appMan);
				runCounter++;
				if (!test.Success) {
					failedCounter++;
					reports.Add (FormatReport (test));
				}
			}
			
			DateTime end = DateTime.Now;
			Console.WriteLine ();

			if (reports.Count > 0) {
				int repCounter = 0;
				int numWidth = reports.Count.ToString ().Length;
				string indent = String.Empty.PadLeft (numWidth + 2);
				string numFormat = "{0," + numWidth + "}) ";
			
				foreach (string r in reports) {
					repCounter++;
					Console.Write (numFormat, repCounter);
					Console.WriteLine (FormatLines (indent, r, Environment.NewLine, true));
				}
			} else
				Console.WriteLine ();
			
			Console.WriteLine ("Tests run: {0}; Total tests: {1}; Failed: {2}; Not run: {3}; Time taken: {4}",
					   runCounter, tests.Count, failedCounter, tests.Count - runCounter, end - start);
		}

		static string FormatReport (StandaloneTest test)
		{
			var sb = new StringBuilder ();
			string newline = Environment.NewLine;
			
			sb.AppendFormat ("{0}{1}", test.Info.Name, newline);			
			sb.AppendFormat ("{0,16}: {1}{2}", "Test", test.TestType, newline);

			if (!String.IsNullOrEmpty (test.Info.Description))
				sb.AppendFormat ("{0,16}: {1}{2}", "Description", test.Info.Description, newline);

			if (!String.IsNullOrEmpty (test.FailedUrl))
				sb.AppendFormat ("{0,16}: {1}{2}", "Failed URL", test.FailedUrl, newline);

			if (!String.IsNullOrEmpty (test.FailedUrlCallbackName))
				sb.AppendFormat ("{0,16}: {1}{2}", "Callback method", test.FailedUrlCallbackName, newline);
			
			if (!String.IsNullOrEmpty (test.FailedUrlDescription))
				sb.AppendFormat ("{0,16}: {1}{2}", "URL description", test.FailedUrlDescription, newline);

			if (!String.IsNullOrEmpty (test.FailureDetails))
				sb.AppendFormat ("{0,16}:{2}{1}{2}", "Failure details", FormatLines ("   ", test.FailureDetails, newline, false), newline);

			if (test.Exception != null)
				sb.AppendFormat ("{0,16}:{2}{1}{2}", "Exception", FormatLines ("   ", test.Exception.ToString (), newline, false), newline);

			return sb.ToString ();
		}

		static string FormatLines (string indent, string text, string newline, bool skipFirst)
		{
			var sb = new StringBuilder ();
			bool first = true;
			
			foreach (string s in text.Split (new string[] { newline }, StringSplitOptions.None)) {
				if (skipFirst && first)
					first = false;
				else
					sb.Append (indent);
				sb.Append (s);
				sb.Append (newline);
			}

			sb.Length -= newline.Length;
			return sb.ToString ();
		}
		
		static void LoadTestsFromAssembly (Assembly asm, List <StandaloneTest> tests)
		{
			Type[] types = asm.GetExportedTypes ();
			if (types.Length == 0)
				return;

			object[] attributes;
			foreach (var t in types) {
				if (!t.IsClass || t.IsAbstract || t.IsGenericTypeDefinition)
					continue;
				
				attributes = t.GetCustomAttributes (typeof (TestCaseAttribute), false);
				if (attributes.Length == 0)
					continue;

				if (!typeof (ITestCase).IsAssignableFrom (t))
					continue;

				tests.Add (new StandaloneTest (t, attributes [0] as TestCaseAttribute));
			}
		}
	}
}
