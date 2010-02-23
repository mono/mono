using System;
using System.Globalization;
using System.Collections;
using System.Text;

using Mono.Globalization.Unicode;

namespace MonoTests.System
{
	public class StringNormalizationTest
	{
		ArrayList tests = new ArrayList ();

		class Testcase
		{
			public Testcase (string src, string nfc, string nfd, string nfkc, string nfkd, int testType)
			{
				this.Source = src;
				NFC = nfc;
				NFD = nfd;
				NFKC = nfkc;
				NFKD = nfkd;
				TestType = testType;
			}

			public string Source;
			public string NFC;
			public string NFD;
			public string NFKC;
			public string NFKD;
			public int TestType;
		}

#if TEST_STANDALONE
		public static void Main ()
		{
			new StringNormalizationTest ().Run ();
		}
#endif

		public StringNormalizationTest ()
		{
			Fill ();
		}

		void Run ()
		{
			foreach (Testcase tc in tests) {
				TestString (tc, tc.NFD, NormalizationForm.FormD);
				TestString (tc, tc.NFKD, NormalizationForm.FormKD);
				TestString (tc, tc.NFC, NormalizationForm.FormC);
				TestString (tc, tc.NFKC, NormalizationForm.FormKC);
			}
		}

		void TestString (Testcase tc, string expected, NormalizationForm f)
		{
			string input = tc.Source;
			string actual = null;
			switch (f) {
			default:
				actual = Normalization.Normalize (input, 0); break;
			case NormalizationForm.FormD:
				actual = Normalization.Normalize (input, 1); break;
			case NormalizationForm.FormKC:
				actual = Normalization.Normalize (input, 2); break;
			case NormalizationForm.FormKD:
				actual = Normalization.Normalize (input, 3); break;
			}

			if (actual != expected)
				Console.WriteLine ("Error: expected {0} but was {1} (for {2},type{3} form {4})",
				expected, actual, tc.Source, tc.TestType, f);
		}

		public void Fill ()
		{
@@@@@@ Replace Here @@@@@@
		}
	}
}
