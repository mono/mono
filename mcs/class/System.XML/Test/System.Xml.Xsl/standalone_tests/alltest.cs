using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace simpleTests
{
	class TestRunner
	{
		static ArrayList excludedTests = new ArrayList (new string [] {
});

		static void Process (string submitter, string id, string path, string data,
			string stylesheet, string output, string resDirName)
		{
			string dirToCheck = Path.Combine(resDirName, path);
			if (!Directory.Exists (dirToCheck))
				Directory.CreateDirectory(dirToCheck);

			string resFileName = Path.Combine ("../..", Path.Combine(dirToCheck, id + ".rst"));

 			if (submitter == "Lotus")
				Directory.SetCurrentDirectory (Path.Combine ("Xalan_Conformance_Tests", path));
 			else if (submitter == "Microsoft")
				Directory.SetCurrentDirectory (Path.Combine ("MSFT_Conformance_Tests", path));
  			else
 				return; //unknown directory

#if NET_2_0
			XslCompiledTransform xslt = new XslCompiledTransform();
#else
			XslTransform xslt = new XslTransform();
#endif
			StreamWriter strWr = new StreamWriter (resFileName, false, System.Text.Encoding.UTF8);
			XmlTextWriter wr = new XmlTextWriter (strWr);
			bool success = true;
			try {
				XmlDocument xml = new XmlDocument();
				xml.Load (data);
				xslt.Load (stylesheet);
#if NET_2_0
				xslt.Transform (xml, null, wr);
#else
				xslt.Transform (xml, null, wr, null);
#endif
			} catch(Exception x) {
				strWr.Close();
				strWr = new StreamWriter (resFileName, false, System.Text.Encoding.UTF8);
				strWr.Write("<exception>{0}</exception>", x.GetType().ToString());
				success = false;
			}
			strWr.Flush();
			strWr.Close();
			if (success)
				Console.Write (".");
			else
				Console.Write ("E");

			Directory.SetCurrentDirectory ("../..");
		}

		static void Main(string[] args)
		{
			string topdir = "Results";
			bool noExclude = false;
			foreach (string arg in args) {
				switch (arg) {
				case "--noexc":
					noExclude = true;
					continue;
				default:
					topdir = arg;
					break;
				}
			}
			Console.WriteLine ("Setting topdir as {0}", topdir);
			if (!noExclude) {
				foreach (string s_ in new StreamReader ("ignore.lst").ReadToEnd ().Split ("\n".ToCharArray ())) {
					string s = s_.Trim ();
					if (s.Length > 0)
						excludedTests.Add (s);
				}
			}
			Directory.SetCurrentDirectory ("testsuite/TESTS/");


			XmlDocument catalog = new XmlDocument ();
			catalog.Load ("catalog-fixed.xml");
			foreach (XmlElement testCase in catalog.SelectNodes ("test-suite/test-catalog/test-case[scenario/@operation='standard']")) {
				string id = testCase.GetAttribute ("id");
				// check if the test is excluded.
				if (excludedTests.Contains (id)) {
					Console.Write ("N");
					continue;
				}
				string submitter = testCase.SelectSingleNode ("./parent::test-catalog/@submitter").InnerText;
				string path = testCase.SelectSingleNode ("file-path").InnerText;
				string data = testCase.SelectSingleNode ("scenario/input-file[@role='principal-data']")
					.InnerText;
				string stylesheet = testCase.SelectSingleNode ("scenario/input-file[@role='principal-stylesheet']")
					.InnerText;
				string output = testCase.SelectSingleNode ("scenario/output-file").InnerText;

				Process (submitter, id, path, data, stylesheet, output, topdir);
			}
		}
	}
}
