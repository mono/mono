using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace XsltTest
{
	public class XsltTest
	{
		public static void Main ()
		{
// output22,77: not-supported encoding, but MS passes...?
// output72.xsl: should not pass
			ArrayList expectedExceptions = new ArrayList
(new string [] {"lre12.xsl", "namespace40.xsl", "namespace42.xsl", "namespace43.xsl",
"namespace48.xsl", "namespace60.xsl", "namespace73.xsl", "namespace106.xsl",
"output22.xsl", "output72.xsl", "output77.xsl"});

			XmlDocument whole = new XmlDocument ();
			whole.Load (@"testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml");
Console.WriteLine ("Started: " + DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));
			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case")) {
				string stylesheetBase = null;
				try {
					string filePath = testCase.SelectSingleNode ("file-path").InnerText;
					string path = @"testsuite/TESTS/Xalan_Conformance_Tests/" + filePath + "/";
					foreach (XmlElement scenario in testCase.SelectNodes ("scenario")) {
						XslTransform trans = new XslTransform ();
						stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
						string stylesheet = path + stylesheetBase;
						string srcxml = path + scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText;
//if (srcxml.IndexOf ("attribset") < 0)
//	continue;
if (expectedExceptions.Contains (stylesheetBase))
	continue;
						XmlTextReader stylextr = new XmlTextReader (stylesheet);
						XmlValidatingReader stylexvr = new XmlValidatingReader (stylextr);
						XmlDocument styledoc = new XmlDocument ();
						styledoc.Load (stylesheet);
						trans.Load (stylesheet);
//						trans.Load (styledoc);
						XmlTextReader xtr = new XmlTextReader (srcxml);
						XmlValidatingReader xvr = new XmlValidatingReader (xtr);
						xvr.ValidationType = ValidationType.None;
						XmlDocument input = new XmlDocument ();
						input.Load (xvr);
//						input.Load (xtr);
//						XPathDocument input = new XPathDocument (xtr);
						StringWriter sw = new StringWriter ();
						trans.Transform (input, null, sw);
						string outfile = path + scenario.SelectSingleNode ("output-file[@role='principal']").InnerText;
						if (!File.Exists (outfile)) {
//							Console.WriteLine ("Reference output file does not exist.");
							continue;
						}
						StreamReader sr = new StreamReader (outfile);
						string reference_out = sr.ReadToEnd ();
						string actual_out = sw.ToString ();
						if (reference_out != actual_out)
#if false
							Console.WriteLine ("Different: " + testCase.GetAttribute ("id"));
#else
							Console.WriteLine ("Different: " +
								testCase.GetAttribute ("id") +
								"\n" + 
								actual_out + "\n-------------------\n" + reference_out + "\n");
#endif
					}
//				} catch (NotSupportedException ex) {
				} catch (Exception ex) {
					if (expectedExceptions.Contains (stylesheetBase))
						continue;
				Console.WriteLine ("Exception: " + testCase.GetAttribute ("id") + ": " + ex.Message);
				}
			}
Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyyMMdd-HHmmss.fff"));

		}
	}
}
