using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Collections;
using System.IO;

namespace MonoTests.oasis_xslt {
	class EnvOptions {
		static readonly bool useDomStyle;
		static readonly bool useDomInstance;
		static readonly string outputDir;
		static readonly bool whitespaceStyle;
		static readonly bool whitespaceInstance;
		static readonly bool inverseResults;

		public static bool UseDomStyle {
			get {return useDomStyle;}
		}
		public static bool UseDomInstance {
			get {return useDomInstance;}
		}
		public static string OutputDir {
			get {return outputDir;}
		}
		public static bool WhitespaceStyle {
			get {return whitespaceStyle;}
		}
		public static bool WhitespaceInstance {
			get {return whitespaceInstance;}
		}
		public static bool InverseResults {
			get {return inverseResults;}
		}

		static EnvOptions () {
			IDictionary env = System.Environment.GetEnvironmentVariables();
			if (env.Contains ("XSLTTEST_DOM")) {
				useDomStyle = true;
				useDomInstance = true;
			}
			if (env.Contains ("XSLTTEST_DOMXSL"))
				useDomStyle = true;
			if (env.Contains ("XSLTTEST_DOMINSTANCE"))
				useDomInstance = true;
			if (env.Contains ("XSLTTEST_WS")) {
				whitespaceStyle = true;
				whitespaceInstance = true;
			}
			if (env.Contains ("XSLTTEST_WSXSL"))
				whitespaceStyle = true;
			if (env.Contains ("XSLTTEST_WSSRC"))
				whitespaceInstance = true;
	
			if (env.Contains ("XSLTTEST_INVERSE_RESULTS"))
				inverseResults = true;

			if (useDomStyle || useDomInstance)
				outputDir = "domresults";
			else
				outputDir = "results";
		}
	}

	class Helpers
	{
		public static void ReadStrings (ArrayList array, string filename)
		{
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
	}

	class CatalogTestCase
	{
		string _stylesheet;
		string _srcxml;
		string _outfile;
		public enum CompareType {
			Text,
			HTML,
			XML
		}
		CompareType _compare;
		XmlElement _testCase;
		string _outputDir;

		public CatalogTestCase (string outputDir, XmlElement testCase)
		{
			_testCase = testCase;
			_outputDir = outputDir;
		}

		public bool Process ()
		{
			string relPath = GetRelPath ();

			string path = Path.Combine (Path.Combine ("testsuite", "TESTS"), relPath);
			string outputPath = Path.Combine (_outputDir, relPath);
			if (!Directory.Exists (outputPath))
				Directory.CreateDirectory (outputPath);

			//FIXME: this ignores negative tests. Read README if you want to fix it
			XmlNode scenario = _testCase.SelectSingleNode ("scenario[@operation='standard']");
			if (scenario == null)
				return false;

			ProcessScenario (path, outputPath, scenario);
			return true;
		}

		string GetRelPath ()
		{
			string filePath = _testCase.SelectSingleNode ("file-path").InnerText;
			string submitter = _testCase.SelectSingleNode ("./parent::test-catalog/@submitter").InnerText;
			if (submitter == "Lotus")
				return Path.Combine ("Xalan_Conformance_Tests", filePath);
			else if (submitter == "Microsoft")
				return Path.Combine ("MSFT_Conformance_Tests", filePath);
			else
				throw new System.Exception ("unknown submitter in the catalog");
		}


		void ProcessScenario (string path, string outputPath, XmlNode scenario)
		{ 
			string stylesheetBase = scenario.SelectSingleNode ("input-file[@role='principal-stylesheet']").InnerText;
			_stylesheet = Path.Combine (path, stylesheetBase);
			if (!File.Exists (_stylesheet)) {
				using (StreamWriter wr = new StreamWriter ("missing.lst", true))
					wr.WriteLine (_stylesheet);
			}

			_srcxml = Path.Combine (path, scenario.SelectSingleNode ("input-file[@role='principal-data']").InnerText);
			XmlNode outputNode = scenario.SelectSingleNode ("output-file[@role='principal']");
			if (outputNode != null) {
				_outfile = Path.Combine (outputPath, outputNode.InnerText);
				switch (outputNode.Attributes ["compare"].Value) {
				case "XML":
					_compare = CompareType.XML;
					break;
				case "HTML":
					_compare = CompareType.HTML;
					break;
				default:
					_compare = CompareType.Text;
					break;
				}
			}
			else {
				_outfile = null;
				_compare = CompareType.Text;
			}
		}

		public CompareType Compare {
			get {return _compare;}
		}

		public string StyleSheet {
			get {return _stylesheet;}
		}

		public string SrcXml {
			get {return _srcxml;}
		}

		public string OutFile {
			get {return _outfile;}
		}
	}

	class SingleTestTransform
	{
		CatalogTestCase _testCase;

		public SingleTestTransform (CatalogTestCase testCase)
		{
			_testCase = testCase;
		}

		string _result;
		public string Result {
			get {return _result;}
		}

		System.Exception _exception;
		public System.Exception Exception {
			get {return _exception;}
		}

		public bool Succeeded {
			get {return this.Exception == null;}
		}

		public CatalogTestCase TestCase {
			get {return _testCase;}
		}

		XslTransform LoadTransform ()
		{
			XslTransform trans = new XslTransform ();

			if (EnvOptions.UseDomStyle) {
				XmlDocument styledoc = new XmlDocument ();
				if (EnvOptions.WhitespaceStyle)
					styledoc.PreserveWhitespace = true;
				styledoc.Load (_testCase.StyleSheet);
				trans.Load (styledoc, null, null);
			} else
				trans.Load (new XPathDocument (
					_testCase.StyleSheet,
					EnvOptions.WhitespaceStyle ? XmlSpace.Preserve :
					XmlSpace.Default),
					null, null);
			return trans;
		}

		IXPathNavigable LoadInput ()
		{
			XmlTextReader xtr=null;
			try {
				xtr = new XmlTextReader (_testCase.SrcXml);
				XmlValidatingReader xvr = new XmlValidatingReader (xtr);
				xvr.ValidationType = ValidationType.None;
				IXPathNavigable input = null;
				if (EnvOptions.UseDomInstance) {
					XmlDocument dom = new XmlDocument ();
					if (EnvOptions.WhitespaceInstance)
						dom.PreserveWhitespace = true;
					dom.Load (xvr);
					input = dom;
				} else {
					input = new XPathDocument (xvr,
						EnvOptions.WhitespaceInstance ? XmlSpace.Preserve :
						XmlSpace.Default);
				}
				return input;
			}
			finally {
				if (xtr!=null)
					xtr.Close ();
			}
		}

		public void RunTest ()
		{
			try {
				XslTransform trans = LoadTransform ();
				IXPathNavigable input = LoadInput ();
				using (StringWriter sw = new StringWriter ()) {
					trans.Transform (input, null, sw, null);
					_result = sw.ToString ().Replace ("\r\n", "\n");
				}
			}
			catch (System.Exception e) {
				_exception = e;
			}
		}
	}
}
