//
// BaseMachine.jvm.cs
//
// Author:
//	Arina Itkes  <arinai@mainsoft.com>
//
// Copyright (C) 2007 Mainsoft, Inc.
//

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
using System.Collections;

namespace System.Text.RegularExpressions
{
	abstract class BaseMachine : IMachine
	{
		delegate void MatchAppendEvaluator (Match match, StringBuilder sb);

		virtual public string Replace (Regex regex, string input, string replacement, int count, int startat) {
			ReplacementEvaluator ev = new ReplacementEvaluator (regex, replacement);
			return Replace (regex, input, new MatchAppendEvaluator (ev.EvaluateAppend), count, startat);
		}

		virtual public string [] Split (Regex regex, string input, int count, int startat) {
			ArrayList splits = new ArrayList ();
			if (count == 0)
				count = Int32.MaxValue;

			int ptr = startat;
			Match m = null;
			while (--count > 0) {
				if (m != null)
					m = m.NextMatch ();
				else
					m = regex.Match (input, ptr);

				if (!m.Success)
					break;

				if (regex.RightToLeft)
					splits.Add (input.Substring (m.Index + m.Length, ptr - m.Index - m.Length));
				else
					splits.Add (input.Substring (ptr, m.Index - ptr));

				int gcount = m.Groups.Count;
				for (int gindex = 1; gindex < gcount; gindex++) {
					Group grp = m.Groups [gindex];
					splits.Add (input.Substring (grp.Index, grp.Length));
				}

				if (regex.RightToLeft)
					ptr = m.Index;
				else
					ptr = m.Index + m.Length;

			}

			if (regex.RightToLeft && ptr >= 0)
				splits.Add (input.Substring (0, ptr));
			if (!regex.RightToLeft && ptr <= input.Length)
				splits.Add (input.Substring (ptr));

			return (string []) splits.ToArray (typeof (string));
		}

		virtual public Match Scan (Regex regex, string text, int start, int end) {
			throw new NotImplementedException ("Scan method must be implemented in derived classes");
		}

		virtual public string Result (string replacement, Match match)
		{
			return ReplacementEvaluator.Evaluate (replacement, match);
		}

		private static string Replace (Regex regex, string input, MatchAppendEvaluator evaluator, int count, int startat) {
			StringBuilder result = new StringBuilder ();
			int ptr = startat;
			int counter = count;

			result.Append (input, 0, ptr);

			Match m = regex.Match (input, startat);
			while (m.Success) {
				if (count != -1)
					if (counter-- <= 0)
						break;
				if (m.Index < ptr)
					throw new SystemException ("how");
				result.Append (input, ptr, m.Index - ptr);
				evaluator (m, result);

				ptr = m.Index + m.Length;
				m = m.NextMatch ();
			}

			if (ptr == 0)
				return input;

			result.Append (input, ptr, input.Length - ptr);

			return result.ToString ();
		}
	}
}
