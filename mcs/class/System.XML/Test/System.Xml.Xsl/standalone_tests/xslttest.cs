using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using NUnit.Core;
using NUnit.Framework;

namespace MonoTests.oasis_xslt {
	public class SuiteBuilder {
		#region test list fields
		IDictionary expectedExceptions = new Hashtable ();
		ArrayList skipTargets = new ArrayList ();
		ArrayList knownFailures = new ArrayList ();
		ArrayList fixmeList = new ArrayList ();
		#endregion
	
		TestSuite _suite;
		SuiteBuilder (TestSuite suite)
		{
			_suite = suite;
		}

		void ReadLists ()
		{
			string exceptionsFilename = Path.Combine (EnvOptions.OutputDir, "res-exceptions.lst");

			Helpers.ReadStrings (skipTargets, "ignore.lst");
			Helpers.ReadStrings (knownFailures, "knownFailures.lst");
			Helpers.ReadStrings (fixmeList, "fixme.lst");
			ArrayList exceptionsArray = new ArrayList();
			Helpers.ReadStrings (exceptionsArray, exceptionsFilename);
			foreach (string s in exceptionsArray) {
				string [] halves = s.Split ('\t');
				expectedExceptions [halves[0]] = halves[1];
			}
		}

		public void Build ()
		{
//			if (Environment.GetEnvironmentVariables().Contains("START_DEBUG"))
//				System.Diagnostics.Debugger.Launch ();
			ReadLists ();
			XmlDocument whole = new XmlDocument ();
			whole.Load (@"testsuite/TESTS/catalog-fixed.xml");

			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case")) {
				string testid = testCase.GetAttribute ("id");

				if (skipTargets.Contains (testid))
					continue;

				CatalogTestCase ctc = new CatalogTestCase(EnvOptions.OutputDir, testCase);
				if (!ctc.Process ())
					continue;

				SingleTestTransform stt = new SingleTestTransform (ctc);

				string expectedException = (string) expectedExceptions[testid];
				bool isKnownFailure = knownFailures.Contains (testid) || fixmeList.Contains (testid);

				_suite.Add (new TestFromCatalog (testid, stt, expectedException,
					EnvOptions.InverseResults, isKnownFailure));
			}
		}

		[Suite]
		public static TestSuite Suite { 
			get {
				TestSuite suite = new TestSuite ("MonoTests.oasis_xslt.SuiteBuilder");
				SuiteBuilder builder = new SuiteBuilder (suite);
				builder.Build ();
				return suite;
			}
		}
	}

	class TestFromCatalog: NUnit.Core.TestCase {
		bool _inverseResult;
		string _testid;
		string _expectedException;
		SingleTestTransform _transform;

		public TestFromCatalog (string testid, SingleTestTransform transform,
			string expectedException, bool inverseResult, bool isKnownFailure)
			:base (null, testid)
		{
			_testid = testid;
			_expectedException = expectedException;
			_transform = transform;
			_inverseResult = inverseResult;
			
			ArrayList arr = new ArrayList ();
			if (isKnownFailure) {
				arr.Add ("KnownFailures");
				this.IsExplicit = true;
			}
			else
				arr.Add ("Clean");
			Categories = arr;
		}

		string CompareResult (string actual, string expected)
		{
			//TODO: add xml comparison
			if (actual == expected)
				return null;
			else
#if !FAILURE_DETAILED_MESSAGE
				return "Different.";
#else
				return "Different.\nActual*****\n"+actual+"\nReference*****\n"+expected;
#endif
		}

		string CompareException (Exception actual, string testid)
		{
			if (_expectedException == null)
				return "Unexpected exception: " + actual.ToString ();

			string actualType = actual.GetType ().ToString ();
			if (actualType != _expectedException)
				return "Different exception thrown.\nActual*****\n"+actualType+
					"\nReference*****\n"+_expectedException;

            return null;
		}

		void ReportResult (string failureMessage, string stackTrace, TestCaseResult res)
		{
			if (_inverseResult) {
				if (failureMessage != null)
					res.Success ();
				else
					res.Failure ("The following test was FIXED: "+_testid, null);
			}
			else {
				if (failureMessage != null)
					res.Failure (failureMessage, stackTrace);
				else
					res.Success ();
			}
		}

		public override void Run (TestCaseResult res)
		{
			_transform.RunTest ();

			string failureMessage;
			string stackTrace = null;
			if (_transform.Succeeded) {
				try {
					using (StreamReader sr = new StreamReader (_transform.TestCase.OutFile))
						failureMessage = CompareResult (_transform.Result, sr.ReadToEnd ());
				}
				catch {
					//if there is no reference result because of expectedException, we
					//are OK, otherwise, rethrow
					if (_expectedException!=null)
						failureMessage = null;
					else {
						Console.WriteLine ("ERROR: No reference result, and no expected exception.");
						throw;
					}
				}
			}
			else {
				failureMessage = CompareException (_transform.Exception, _testid);
				stackTrace = _transform.Exception.StackTrace;
			}

			ReportResult (failureMessage, stackTrace, res);
		}
	}
}
