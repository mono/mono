using System;
using System.Text.RegularExpressions;

namespace MonoTests.System.Text.RegularExpressions {

	class RegexTrial {
		public string pattern;
		public RegexOptions options;
		public string input;

		public string expected;
		public string error = "";

		public RegexTrial (string pattern, RegexOptions options, string input, string expected) {
			this.pattern = pattern;
			this.options = options;
			this.input = input;
			this.expected = expected;
		}

		public string Expected {
			get { return expected; }
		}
		
		public string Error {
			get {
				return this.error;
			}
		}

		public string Execute () {
			string result;
			try {
				Regex re = new Regex (pattern, options);
				Match m = re.Match (input);

				if (m.Success) {
					result = "Pass.";

					for (int i = 0; i < m.Groups.Count; ++ i) {
						Group group = m.Groups[i];
						
						result += " Group[" + i + "]=";
						foreach (Capture cap in group.Captures) {
							result += "(" + cap.Index + "," + cap.Length + ")";
						}
					}
				}
				else
					result = "Fail.";
			}
			catch (Exception e) {
				
				error = e.Message + "\n" + e.StackTrace + "\n\n";
				
				result = "Error.";
			}

			return result;
		}

		public override string ToString () {
			return
				"Matching input '" + input +
				"' against pattern '" + pattern +
				"' with options '" + options + "'.";
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
