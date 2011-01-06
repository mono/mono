//
// BaseMachine.cs
//
// Author:
// author:	Dan Lewis (dlewis@gmx.co.uk)
//		(c) 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Collections.Generic;

namespace System.Text.RegularExpressions
{
	abstract class BaseMachine : IMachine
	{
		internal delegate void MatchAppendEvaluator (Match match, StringBuilder sb);

		public virtual string Replace (Regex regex, string input, string replacement, int count, int startat)
		{
			ReplacementEvaluator ev = new ReplacementEvaluator (regex, replacement);
			if (regex.RightToLeft)
				return RTLReplace (regex, input, new MatchEvaluator (ev.Evaluate), count, startat);
			else
				return LTRReplace (regex, input, new MatchAppendEvaluator (ev.EvaluateAppend), count, startat, ev.NeedsGroupsOrCaptures);
		}

		virtual public string [] Split (Regex regex, string input, int count, int startat)
		{
			var splits = new List<string> ();
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
					if (grp.Length > 0)
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

			return splits.ToArray ();
		}

		virtual public Match Scan (Regex regex, string text, int start, int end)
		{
			throw new NotImplementedException ("Scan method must be implemented in derived classes");
		}

		virtual public string Result (string replacement, Match match)
		{
			return ReplacementEvaluator.Evaluate (replacement, match);
		}

		internal string LTRReplace (Regex regex, string input, MatchAppendEvaluator evaluator, int count, int startat) {
			return LTRReplace (regex, input, evaluator, count, startat, true);
		}

		internal string LTRReplace (Regex regex, string input, MatchAppendEvaluator evaluator, int count, int startat, bool needs_groups_or_captures)
		{
			this.needs_groups_or_captures = needs_groups_or_captures;
			
			Match m = Scan (regex, input, startat, input.Length);
			if (!m.Success)
				return input;

			StringBuilder result = new StringBuilder (input.Length);
			int ptr = startat;
			int counter = count;

			result.Append (input, 0, ptr);

			do {
				if (count != -1)
					if (counter-- <= 0)
						break;
				if (m.Index < ptr)
					throw new SystemException ("how");
				result.Append (input, ptr, m.Index - ptr);
				evaluator (m, result);

				ptr = m.Index + m.Length;
				m = m.NextMatch ();
			} while (m.Success);

			result.Append (input, ptr, input.Length - ptr);

			return result.ToString ();
		}

		internal string RTLReplace (Regex regex, string input, MatchEvaluator evaluator, int count, int startat)
		{
			Match m = Scan (regex, input, startat, input.Length);
			if (!m.Success)
				return input;

			int ptr = startat;
			int counter = count;
#if NET_2_1
			var pieces = new System.Collections.Generic.List<string> ();
#else
			StringCollection pieces = new StringCollection ();
#endif
			
			pieces.Add (input.Substring (ptr));

			do {
				if (count != -1)
					if (counter-- <= 0)
						break;
				if (m.Index + m.Length > ptr)
					throw new SystemException ("how");
				pieces.Add (input.Substring (m.Index + m.Length, ptr - m.Index - m.Length));
				pieces.Add (evaluator (m));

				ptr = m.Index;
				m = m.NextMatch ();
			} while (m.Success);

			StringBuilder result = new StringBuilder ();

			result.Append (input, 0, ptr);
			for (int i = pieces.Count; i > 0; )
				result.Append (pieces [--i]);

			pieces.Clear ();

			return result.ToString ();
		}

		// Specify whenever Match objects created by this machine need to be fully
		// built. If false, these can be omitted, avoiding some memory allocations and
		// processing time.
		protected bool needs_groups_or_captures = true; 
	}
}
