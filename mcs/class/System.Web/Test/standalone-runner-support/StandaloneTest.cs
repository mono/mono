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
using System.Web;
using System.Web.Hosting;

using NUnit.Framework;

namespace StandAloneRunnerSupport
{
	public sealed class StandaloneTest
	{
		string failureDetails;
		
		public TestCaseFailureException Exception {
			get; private set;
		}

		public string FailureDetails {
			get {
				if (!String.IsNullOrEmpty (failureDetails))
					return failureDetails;

				TestCaseFailureException ex = Exception;
				if (ex == null)
					return String.Empty;

				return ex.Details;
			}

			private set {
				failureDetails = value;
			}
		}

		public string FailedUrl {
			get; private set;
		}

		public string FailedUrlDescription {
			get; private set;
		}
		
		public TestCaseAttribute Info {
			get; private set;
		}

		public bool Success {
			get; private set;
		}
		
		public Type TestType {
			get; private set;
		}
		
		public StandaloneTest (Type testType, TestCaseAttribute info)
		{
			if (testType == null)
				throw new ArgumentNullException ("testType");
			if (info == null)
				throw new ArgumentNullException ("info");
			
			TestType = testType;
			Info = info;
		}

		public void Run (ApplicationManager appMan)
		{
			try {
				Success = true;
				RunInternal (appMan);
			} catch (TestCaseFailureException ex) {
				Exception = ex;
				Success = false;
			} catch (Exception ex) {
				FailureDetails = String.Format ("Test failed with exception of type '{0}':{1}{2}",
								ex.GetType (), Environment.NewLine, ex.ToString ());
				Success = false;
			}
		}
		
		void RunInternal (ApplicationManager appMan)
		{
			ITestCase test = Activator.CreateInstance (TestType) as ITestCase;
			var runItems = new List <TestRunItem> ();			
			if (!test.SetUp (runItems)) {
				Success = false;
				FailureDetails = "Test aborted in setup phase.";
				return;
			}

			if (runItems.Count == 0) {
				Success = false;
				FailureDetails = "No test run items returned by the test case.";
				return;
			}
			
			string result;
			TestRunner runner;
			try {
				Console.Write ('[');
				foreach (var tri in runItems) {
					if (tri == null)
						continue;

					runner = null;
					try {
						runner = appMan.CreateObject (Info.Name, typeof (TestRunner), test.VirtualPath, test.PhysicalPath, true) as TestRunner;
						if (runner == null) {
							Success = false;
							throw new InvalidOperationException ("runner must not be null.");
						}
						result = runner.Run (tri.Url, tri.PathInfo);
						if (tri.Callback == null)
							continue;

						tri.TestRunData = runner.TestRunData;
						tri.Callback (result, tri);
						Console.Write ('.');
					} catch (Exception) {
						FailedUrl = tri.Url;
						FailedUrlDescription = tri.UrlDescription;
						Console.Write ('F');
						throw;
					} finally {
						if (runner != null) {
							runner.Stop (true);
							AppDomain.Unload (runner.Domain);
						}
						runner = null;
					}
				}
			} catch (AssertionException ex) {
				throw new TestCaseFailureException ("Assertion failed.", ex.Message, ex);
			} finally {
				Console.Write (']');
			}
		}
	}
}
