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
"Keys_PerfRepro3",
});

		static void Process (string id, string path, string data,
			string stylesheet, string output, string resDirName)
		{
			string dirToCheck = Path.Combine(resDirName, path);
			if (!Directory.Exists (dirToCheck))
				Directory.CreateDirectory(dirToCheck);

			string resFileName = Path.Combine ("../..", Path.Combine(dirToCheck, id + ".rst"));

			// hacky!
			if (path [0] >= 'a')
				Directory.SetCurrentDirectory (Path.Combine ("Xalan_Conformance_Tests", path));
			else
				Directory.SetCurrentDirectory (Path.Combine ("MSFT_Conformance_Tests", path));

#if NET_2_0
			XslCompiledTransform xslt = new XslCompiledTransform();
#else
			XslTransform xslt = new XslTransform();
#endif
			StreamWriter strWr = new StreamWriter (resFileName, false, System.Text.Encoding.UTF8);
			XmlTextWriter wr = new XmlTextWriter (strWr);
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
			}
			strWr.Flush();
			strWr.Close();

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

			string pathPrefix = "testsuite/TESTS";
			Directory.SetCurrentDirectory (pathPrefix);

			XmlDocument catalog = new XmlDocument ();
			catalog.Load ("catalog-out.xml");
			XmlNodeList list = catalog.SelectNodes ("//tests/test");
			foreach (XmlNode node in list) {
				if (node.SelectSingleNode ("@ignore")!=null)
					continue;
				string id = node.SelectSingleNode ("@id").InnerText;
				// check if the test is excluded.
				if (excludedTests.Contains (id))
					continue;
				string path = node.SelectSingleNode ("path").InnerText;
				string data = node.SelectSingleNode ("data").InnerText;
				string stylesheet = node.SelectSingleNode ("stylesheet").InnerText;
				string output = node.SelectSingleNode ("output").InnerText;

				Console.Write ("Processing {0} ...", id);
				Process (id, path, data, stylesheet, output, topdir);
				Console.WriteLine ();
			}
		}
	}
}
