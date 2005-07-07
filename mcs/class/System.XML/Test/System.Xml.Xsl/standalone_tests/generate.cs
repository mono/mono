using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace MonoTests.oasis_xslt {
	public class Generator: IDisposable
	{
		#region test list fields
		ArrayList skipTargets = new ArrayList ();
		StreamWriter resultExceptionsWriter;
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			if (resultExceptionsWriter!=null)
				resultExceptionsWriter.Close ();
			resultExceptionsWriter = null;
		}

		#endregion

		public static int Main (string [] args) {
			using (Generator test = new Generator (args)) {
				test.Run ();
			}
			return 0;
		}

		string [] _args;

		Generator (string [] args)
		{
			_args = args;
		}

		void Run ()
		{
			string resultExceptionsFilename = Path.Combine (EnvOptions.OutputDir, "res-exceptions.lst");

			if (Directory.Exists (EnvOptions.OutputDir))
				Directory.Delete (EnvOptions.OutputDir, true);
			Directory.CreateDirectory (EnvOptions.OutputDir);

			Helpers.ReadStrings (skipTargets, "ignore.lst");

			resultExceptionsWriter = new StreamWriter (resultExceptionsFilename);

			XmlDocument catalog = new XmlDocument ();
			catalog.Load (@"testsuite/TESTS/catalog-fixed.xml");

			foreach (XmlElement testCase in catalog.SelectNodes ("test-suite/test-catalog/test-case")) {
				ProcessTestCase (testCase);
			}
		}
		
		void ProcessTestCase (XmlElement testCase) {
			string testid = testCase.GetAttribute ("id");
			Console.Out.WriteLine (testid);
			if (skipTargets.Contains (testid))
				return;

			CatalogTestCase ctc = new CatalogTestCase(EnvOptions.OutputDir, testCase);
			if (!ctc.Process ())
				return;

			SingleTestTransform stt = new SingleTestTransform (ctc);
			stt.RunTest ();
			if (stt.Succeeded)
				using (StreamWriter fw = new StreamWriter (ctc.OutFile, false, Encoding.UTF8))
					fw.Write (stt.Result);
			else
				resultExceptionsWriter.WriteLine ("{0}\t{1}", testid, stt.Exception.GetType ().ToString ());
		}

	}
}