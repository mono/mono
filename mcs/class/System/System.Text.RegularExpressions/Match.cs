//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	Match.cs
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

namespace System.Text.RegularExpressions {

	[Serializable]
	public class Match : Group {
		public static Match Empty {
			get { return empty; }
		}
		
		public static Match Synchronized (Match inner) {
			return inner;	// FIXME need to sync on machine access
		}
		
		public virtual GroupCollection Groups {
			get { return groups; }
		}

		public Match NextMatch () {
			if (this == Empty)
				return Empty;

			int scan_ptr = regex.RightToLeft ? Index : Index + Length;

			// next match after an empty match: make sure scan ptr makes progress
			
			if (Length == 0)
				scan_ptr += regex.RightToLeft ? -1 : +1;

			return machine.Scan (regex, Text, scan_ptr, text_length);
		}

		public virtual string Result (string replacement) {
			return ReplacementEvaluator.Evaluate (replacement, this);
		}

		// internal

		internal Match () : base (null, null) {
			this.regex = null;
			this.machine = null;
			this.text_length = 0;
			this.groups = new GroupCollection ();

			groups.Add (this);
		}
		
		internal Match (Regex regex, IMachine machine, string text, int text_length, int[][] grps) :
			base (text, grps[0])
		{
			this.regex = regex;
			this.machine = machine;
			this.text_length = text_length;

			this.groups = new GroupCollection ();
			groups.Add (this);
			for (int i = 1; i < grps.Length; ++ i)
				groups.Add (new Group (text, grps[i]));
		}

		internal Regex Regex {
			get { return regex; }
		}

		// private

		private Regex regex;
		private IMachine machine;
		private int text_length;
		private GroupCollection groups;

		private static Match empty = new Match ();
	}
}
