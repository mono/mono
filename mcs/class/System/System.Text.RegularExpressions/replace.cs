//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	replace.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Collections;

using Parser = System.Text.RegularExpressions.Syntax.Parser;

namespace System.Text.RegularExpressions {

	class ReplacementEvaluator {
		public static string Evaluate (string replacement, Match match) {
			ReplacementEvaluator ev = new ReplacementEvaluator (match.Regex, replacement);
			return ev.Evaluate (match);
		}

		public ReplacementEvaluator (Regex regex, string replacement) {
			this.regex = regex;
			terms = new ArrayList ();
			Compile (replacement);
		}

		public string Evaluate (Match match) {
			StringBuilder result = new StringBuilder ();
			foreach (Term term in terms)
				result.Append (term.GetResult (match));

			return result.ToString ();
		}

		// private

		private void Compile (string replacement) {
			replacement = Parser.Unescape (replacement);
			StringBuilder literal = new StringBuilder ();

			int ptr = 0;
			char c;
			Term term = null;
			while (ptr < replacement.Length) {
				c = replacement[ptr ++];

				if (c == '$') {
					if (replacement[ptr] != '$') 
						term = CompileTerm (replacement, ref ptr);
					else
						++ ptr;
				}

				if (term != null) {
					term.Literal = literal.ToString ();
					terms.Add (term);

					term = null;
					literal.Length = 0;
				}
				else
					literal.Append (c);
			}

			if (term == null && literal.Length > 0) {
				terms.Add (new Term (literal.ToString ()));
			}
		}

		private Term CompileTerm (string str, ref int ptr) {
			char c = str[ptr];

			if (Char.IsDigit (c)) {		// numbered group
				int n = Parser.ParseDecimal (str, ref ptr);
				if (n < 0 || n > regex.GroupCount)
					throw new ArgumentException ("Bad group number.");
				
				return new Term (TermOp.Match, n);
			}
			
			++ ptr;

			switch (c) {
			case '{': {			// named group
				string name = Parser.ParseName (str, ref ptr);
				if (str[ptr ++] != '}' || name == null)
					throw new ArgumentException ("Bad group name.");
				
				int n = regex.GroupNumberFromName (name);
				
				if (n < 0)
					throw new ArgumentException ("Bad group name.");

				return new Term (TermOp.Match, n);
			}

			case '&':			// entire match
				return new Term (TermOp.Match, 0);

			case '`':			// text before match
				return new Term (TermOp.PreMatch, 0);

			case '\'':			// text after match
				return new Term (TermOp.PostMatch, 0);

			case '+':			// last group
				return new Term (TermOp.Match, regex.GroupCount - 1);

			case '_':			// entire text
				return new Term (TermOp.All, 0);

			default:
				throw new ArgumentException ("Bad replacement pattern.");
			}
		}

		private Regex regex;
		private ArrayList terms;

		private enum TermOp {
			None,				// no action
			Match,				// input within group
			PreMatch,			// input before group
			PostMatch,			// input after group
			All				// entire input
		}

		private class Term {
			public Term (TermOp op, int arg) {
				this.op = op;
				this.arg = arg;
				this.literal = "";
			}

			public Term (string literal) {
				this.op = TermOp.None;
				this.arg = 0;
				this.literal = literal;
			}

			public string Literal {
				set { literal = value; }
			}

			public string GetResult (Match match) {
				Group group = match.Groups[arg];
			
				switch (op) {
				case TermOp.None:
					return literal;

				case TermOp.Match:
					return literal + group.Value;

				case TermOp.PreMatch:
					return literal + group.Text.Substring (0, group.Index);

				case TermOp.PostMatch:
					return literal + group.Text.Substring (group.Index + group.Length);

				case TermOp.All:
					return literal + group.Text;
				}

				return "";
			}
		
			public TermOp op;		// term type
			public int arg;			// group argument
			public string literal;		// literal to prepend

			public override string ToString () {
				return op.ToString () + "(" + arg + ") " + literal;
			}
		}
	}
}
