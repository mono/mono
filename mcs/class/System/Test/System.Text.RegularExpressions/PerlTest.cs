//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	PerlTest.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	
	public class PerlTest : TestCase {
		public static ITest Suite {
			get { return new TestSuite (typeof (PerlTest)); }
		}

		public PerlTest () : this ("System.Text.RegularExpressions Perl testsuite") { }
		public PerlTest (string name) : base (name) { }

		public void TestTrials () {
			foreach (RegexTrial trial in PerlTrials.trials) {
				string msg = trial.Execute ();
				if (msg != null)
					Assertion.Fail (msg);
			}
		}

		protected override void SetUp () { }
		protected override void TearDown () { }
	}

	class RegexTrial {
		public string pattern;
		public RegexOptions options;
		public string input;

		public Result expected;
		public uint checksum;

		public RegexTrial (string pattern, RegexOptions options, string input, Result expected, uint checksum) {
			this.pattern = pattern;
			this.options = options;
			this.input = input;
			this.expected = expected;
			this.checksum = checksum;
		}

		public string Execute () {
			try {
				Regex re = new Regex (pattern, options);
				Match m = re.Match (input);

				if (m.Success) {
					uint sum = CreateChecksum (re, m);
					return Report (Result.Pass, sum);
				}

				return Report (Result.Fail, 0);
			}
			catch (Exception) {
				return Report (Result.Error, 0);
			}
		}

		public override string ToString () {
			return
				"Matching input '" + input +
				"' against pattern '" + pattern +
				"' with options '" + options + "'.";
		}

		// private

		private static uint CreateChecksum (Regex re, Match m) {
			Checksum sum = new Checksum ();
		
			// group name mapping

			string[] names = re.GetGroupNames ();
			foreach (string name in re.GetGroupNames ()) {
				sum.Add (name);
				sum.Add ((uint)re.GroupNumberFromName (name));
			}

			// capture history

			foreach (Group group in m.Groups) {
				foreach (Capture cap in group.Captures) {
					sum.Add ((uint)cap.Index);
					sum.Add ((uint)cap.Length);
				}
			}

			return sum.Value;
		}

		private string Report (Result actual, uint sum) {
			if (actual == expected && sum == checksum)
				return null;

			string msg = this.ToString ();
			if (actual != expected) {
				msg +=
					" Expected " + expected +
					", but result was " + actual + ".";
			}

			if (sum != checksum)
				msg += " Bad checksum.";

			return msg;
		}

	}

	enum Result {
		Pass,
		Fail,
		Error
	}

	class Checksum {
		public Checksum () {
			this.sum = 0;
		}

		public uint Value {
			get { return sum; }
		}

		public void Add (string str) {
			for (int i = 0; i < str.Length; ++ i)
				Add (str[i], 16);
		}

		public void Add (uint n) {
			Add (n, 32);
		}

		public void Add (ulong n, int bits) {
			ulong mask = 1ul << (bits - 1);
			for (int i = 0; i < bits; ++ i) {
				Add ((n & mask) != 0);
				mask >>= 1;
			}
		}

		public void Add (bool bit) {
			bool top = (sum & 0x80000000) != 0;
			sum <<= 1;
			sum ^= bit ? (uint)1 : (uint)0;

			if (top)
				sum ^= key;
		}

		private uint sum;
		private readonly uint key = 0x04c11db7;
	}
}
