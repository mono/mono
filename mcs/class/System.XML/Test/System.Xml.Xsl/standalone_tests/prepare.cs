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
			XmlDocument whole = new XmlDocument ();
			string xalan = @"testsuite/TESTS/Xalan_Conformance_Tests/";
			whole.Load (xalan + "catalog.xml");
			foreach (XmlElement testCase in whole.SelectNodes ("test-suite/test-catalog/test-case")) {
				string stylesheetBase = null;
				try {
					string filePath = testCase.SelectSingleNode ("file-path").InnerText;
					string path = xalan + filePath + "/";
					foreach (XmlElement scenario in testCase.SelectNodes ("scenario")) {
						XslTransform trans = new XslTransform ();
						stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
						string stylesheet = path + stylesheetBase;
						string srcxml = path + scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText;
						string outfile = path + scenario.SelectSingleNode ("output-file[@role='principal']").InnerText;
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
						sw.Close ();
						StreamWriter fw = new StreamWriter (outfile);
						fw.Write (sw.ToString ());
						fw.Close ();
					}
				} catch (Exception ex) {
				Console.WriteLine ("\n\n\nException: " + testCase.GetAttribute ("id") + ": " + ex);
				}
			}
		}
	}
}
