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
	
		public SuiteBuilder ()
		{
		}

		void ReadLists ()
		{
			string exceptionsFilename = Path.Combine (EnvOptions.OutputDir, "res-exceptions.lst");

			Helpers.ReadStrings (skipTargets, "ignore.lst");
#if TARGET_JVM
			Helpers.ReadStrings (knownFailures, "knownFailures.jvm.lst");
#else
			Helpers.ReadStrings (knownFailures, "knownFailures.lst");
#endif
			Helpers.ReadStrings (fixmeList, "fixme.lst");
			ArrayList exceptionsArray = new ArrayList();
			Helpers.ReadStrings (exceptionsArray, exceptionsFilename);
			foreach (string s in exceptionsArray) {
				string [] halves = s.Split ('\t');
				expectedExceptions [halves[0]] = halves[1];
			}
		}

		void Build (TestSuite suite)
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

				suite.Add (new TestFromCatalog (testid, stt, expectedException,
					EnvOptions.InverseResults, isKnownFailure));
			}
		}

		static object lock_obj = new object ();
		static TestSuite _suite;

		[Suite]
		public static TestSuite Suite { 
			get {
				if (_suite == null) { lock (lock_obj) {
				TestSuite suite = new TestSuite ("MonoTests.oasis_xslt.SuiteBuilder");
				new SuiteBuilder ().Build (suite);
				_suite = suite;
				} }
				return _suite;
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
				//this.IsExplicit = true;
			}
			else
				arr.Add ("Clean");
			Categories = arr;
		}

		static string EscapeString (string res)
		{
			MemoryStream s = new MemoryStream ();
			XmlTextWriter w = new XmlTextWriter (s, System.Text.Encoding.ASCII);
			w.WriteString (res);
			w.Close ();	
			
			StringBuilder sb = new StringBuilder (res.Length);
			byte [] arr = s.ToArray ();
			foreach (byte b in arr)
				sb.Append (Convert.ToChar (b));

			return sb.ToString ();
		}

		string CompareResult (string actual, string expected, CatalogTestCase.CompareType compare)
		{
			//TODO: add html comparison
			if (compare== CatalogTestCase.CompareType.XML) {
				try {
					XmlDocument actDoc = new XmlDocument();
					XmlDocument expDoc = new XmlDocument();
					actDoc.LoadXml (actual);
					expDoc.LoadXml (expected);
					XmlCompare.XmlCompare cmp = new XmlCompare.XmlCompare(XmlCompare.XmlCompare.Flags.IgnoreAttribOrder);
					if (cmp.AreEqual (actDoc, expDoc)) {
						return null;
					}
				}
				catch (Exception ex) {
					//could not compare as xml, fallback to text
					if (actual == expected)
						return null;
				}
			}
			else
				if (actual == expected)
					return null;

			string res = "Different.\nActual*****\n"+actual+"\nReference*****\n"+expected;
			return EscapeString (res);
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
						failureMessage = CompareResult (_transform.Result, sr.ReadToEnd ().Replace ("\r\n", "\n"), _transform.TestCase.Compare);
				}
				catch {
					//if there is no reference result because of expectedException, we
					//are OK, otherwise, rethrow
					if (_expectedException!=null)
						failureMessage = null;
					else {
						Console.WriteLine (_transform.TestCase.OutFile);
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
