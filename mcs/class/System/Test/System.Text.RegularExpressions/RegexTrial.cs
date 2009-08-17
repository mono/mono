using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {

	class RegexTrial {
		public string pattern;
		public RegexOptions options;
		public string input;

		public string expected;
		public string error = "";

		public RegexTrial (string pattern, RegexOptions options, string input, string expected)
		{
			this.pattern = pattern;
			this.options = options;
			this.input = input;
			this.expected = expected;
		}

		public string Expected {
			get { return expected; }
		}
		
		public string Error {
			get { return this.error; }
		}

		public void Execute ()
		{
			string result;

			for (int compiled = 0; compiled < 2; ++compiled) {
				RegexOptions real_options = (compiled == 1) ? (options | RegexOptions.Compiled) : options;
				try {
					Regex re = new Regex (pattern, real_options);
					int [] group_nums = re.GetGroupNumbers ();
					Match m = re.Match (input);

					if (m.Success) {
						result = "Pass.";

						for (int i = 0; i < m.Groups.Count; ++ i) {
							int gid = group_nums [i];
							Group group = m.Groups [gid];

							result += " Group[" + gid + "]=";
							foreach (Capture cap in group.Captures)
								result += "(" + cap.Index + "," + cap.Length + ")";
						}
					} else {
						result = "Fail.";
					}
				}
				catch (Exception e) {
					error = e.Message + "\n" + e.StackTrace + "\n\n";

					result = "Error.";
				}

				Assert.AreEqual (expected, result,
								 "Matching input '{0}' against pattern '{1}' with options '{2}'", input, pattern, real_options);
			}
		}
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
