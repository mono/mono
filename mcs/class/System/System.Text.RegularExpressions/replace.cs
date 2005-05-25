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
			this.replacement = replacement;
			this.pieces = null;
			this.n_pieces = 0;
			Compile ();
		}

		public string Evaluate (Match match) 
		{
			StringBuilder sb = new StringBuilder ();
			EvaluateAppend (match, sb);
			return sb.ToString ();
		}

		public void EvaluateAppend (Match match, StringBuilder sb)
		{
			int i = 0, k, count;

			if (n_pieces == 0) {
				sb.Append (replacement);
				return;
			}

			while (i < n_pieces) {
				k = pieces [i++];
				if (k >= 0) {
					count = pieces [i++];
					sb.Append (replacement, k, count);
				} else if (k < -3) {
					Group group = match.Groups [-(k + 4)];
					sb.Append (group.Text, group.Index, group.Length);
				} else if (k == -1) {
					sb.Append (match.Text);
				} else if (k == -2) {
					sb.Append (match.Text, 0, match.Index);
				} else { // k == -3
					int matchend = match.Index + match.Length;
					sb.Append (match.Text, matchend, match.Text.Length - matchend);
				} 
			}
		}

		void Ensure (int size)
		{
			int new_size;
			if (pieces == null) {
				new_size = 4;
				if (new_size < size)
					new_size = size;
				pieces = new int [new_size];
			} else if (size >= pieces.Length) {
				new_size = pieces.Length + (pieces.Length >> 1);
				if (new_size < size)
					new_size = size;
				int [] new_pieces = new int [new_size];
				Array.Copy (pieces, new_pieces, n_pieces);
				pieces = new_pieces;
			}
		}

		void AddFromReplacement (int start, int end)
		{
			if (start == end)
				return;
			Ensure (n_pieces + 2);
			pieces [n_pieces++] = start;
			pieces [n_pieces++] = end - start;
		}

		void AddInt (int i)
		{
			Ensure (n_pieces + 1);
			pieces [n_pieces++] = i;
		}

		// private
		private void Compile () {
			replacement = Parser.Unescape (replacement);

			int anchor = 0, ptr = 0, saveptr;
			char c;
			while (ptr < replacement.Length) {
				c = replacement [ptr++];

				if (c != '$')
					continue;

				// If the '$' was the last character, just emit it as is
				if (ptr == replacement.Length)
					break;

				// If we saw a '$$'
				if (replacement [ptr] == '$') {
					// Everthing from 'anchor' upto and including the first '$' is copied from the replacement string
					AddFromReplacement (anchor, ptr);
					// skip over the second '$'.
					anchor = ++ptr;
					continue;
				}

				saveptr = ptr - 1;

				int from_match = CompileTerm (ref ptr);

				// We couldn't recognize the term following the '$'.  Just treat it as a literal.
				// 'ptr' has already been advanced, no need to rewind it back
				if (from_match >= 0)
					continue;

				AddFromReplacement (anchor, saveptr);
				AddInt (from_match);
				anchor = ptr;
			}

			// If we never needed to advance anchor, it means the result is the whole replacement string.
			// We optimize that case by never allocating the pieces array.
			if (anchor != 0)
				AddFromReplacement (anchor, ptr);
		}

		private int CompileTerm (ref int ptr) {
			char c = replacement [ptr];

			if (Char.IsDigit (c)) {		// numbered group
				int n = Parser.ParseDecimal (replacement, ref ptr);
				if (n < 0 || n > regex.GroupCount)
					return 0;
				
				return -n - 4;
			}
			
			++ ptr;

			switch (c) {
			case '{': {			// named group
				string name;
				int n = -1;

				try {
					// The parser is written such that there are few explicit range checks
					// and depends on 'IndexOutOfRangeException' being thrown.

					if (Char.IsDigit (replacement [ptr])) {
						n = Parser.ParseDecimal (replacement, ref ptr);
						name = "";
					} else {
						name = Parser.ParseName (replacement, ref ptr);
					}
				} catch (IndexOutOfRangeException) {
					ptr = replacement.Length;
					return 0;
				}

				if (ptr == replacement.Length || replacement[ptr] != '}' || name == null)
					return 0;
				++ptr; 			// Swallow the '}'

				if (name != "")
					n = regex.GroupNumberFromName (name);

				if (n < 0 || n > regex.GroupCount)
					return 0;

				return -n - 4;
			}

			case '&':			// entire match.  Value should be same as $0
				return -4;

			case '`':			// text before match
				return -2;

			case '\'':			// text after match
				return -3;

			case '+':			// last group
				return -regex.GroupCount - 4;

			case '_':			// entire text
				return -1;

			default:
				return 0;
			}
		}

		private Regex regex;
		int n_pieces;
		private int [] pieces;
		string replacement;
	}
}
