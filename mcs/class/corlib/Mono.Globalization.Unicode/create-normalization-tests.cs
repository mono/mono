using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mono.Globalization.Unicode
{
	public class NormalizationTestGenerator
	{
		public static void Main ()
		{
			new NormalizationTestGenerator ().Run ();
		}

		int line;
		int type;
		ArrayList tests = new ArrayList ();

		void Run ()
		{
			try {
				using (StreamReader sr = new StreamReader ("downloaded/NormalizationTest.txt")) {
					for (line = 1; sr.Peek () >= 0; line++) {
						ProcessLine (sr.ReadLine ());
					}
				}
			} catch (Exception) {
				Console.Error.WriteLine ("Error at line {0}", line);
				throw;
			}

			TextWriter Output = new StringWriter ();
			foreach (Testcase test in tests) {
				Output.Write ("tests.Add (new Testcase (");
				foreach (string data in test.Data) {
					Output.Write ("\"");
					foreach (char c in data)
						Output.Write ("\\u{0:X04}", (int) c);
					Output.Write ("\", ");
				}
				Output.WriteLine ("{0}));", test.TestType);
			}

			StreamReader template = new StreamReader ("StringNormalizationTestSource.cs");
			string ret = template.ReadToEnd ();
			ret = ret.Replace ("@@@@@@ Replace Here @@@@@@", Output.ToString ());
			Console.WriteLine (ret);
		}

		class Testcase
		{
			public int TestType;
			public string [] Data;

			public Testcase (int type, string [] data)
			{
				this.TestType = type;
				this.Data = data;
			}
		}

		void ProcessLine (string s)
		{
			int idx = s.IndexOf ('#');
			if (idx >= 0)
				s = s.Substring (0, idx);
			if (s.Length == 0)
				return;
			if (s [0] == '@') { // @Part0-3
				type++;
				return;
			}

			string [] parts = s.Split (';');
			string [] data = new string [5];
			bool skip = false;
			for (int form = 0; form < 5; form++) {
				string [] values = parts [form].Split (' ');
				char [] raw = new char [values.Length];
				for (int i = 0; i < raw.Length; i++) {
					int x = int.Parse (values [i].Trim (),
						NumberStyles.HexNumber);
					if (x > char.MaxValue) {
						Console.Error.WriteLine ("at line {0} test contains character {1:X} that is larger than char.MaxValue. Ignored.", line, x);
						skip = true;
						break;
					}
					raw [i] = (char) x;
				}
				if (skip)
					break;
				data [form] = new string (raw);
			}
			if (skip)
				return;
			tests.Add (new Testcase (type, data));
		}
	}
}

