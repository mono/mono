//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	quicksearch.cs
//
// Authors:	Dan Lewis (dlewis@gmx.co.uk)
//		Juraj Skripsky (juraj@hotfeet.ch)
//
// (c) 2002 Dan Lewis
// (c) 2003 Juraj Skripsky
//

using System;
using System.Collections;

namespace System.Text.RegularExpressions {
	class QuickSearch {
		// simplified boyer-moore for fast substring matching
		// (for short strings, we use simple scans)
	
		public QuickSearch (string str, bool ignore) {
			this.str = str;
			this.len = str.Length;
			this.ignore = ignore;

			if (ignore)
				str = str.ToLower ();

			// create the shift table only for "long" search strings
			if(len > THRESHOLD)
				SetupShiftTable ();
		}
		
		public string String {
			get { return str; }
		}

		public int Length {
			get { return len; }
		}

		public bool IgnoreCase {
			get { return ignore; }
		}

		public int Search (string text, int start, int end) {
			int ptr = start;

			// use simple scan for a single-character search string
			if (len == 1) {
				while (ptr <= end) {
					if(str[0] == GetChar(text[ptr]))
						return ptr;
					else
						ptr++;
				}
				return -1;
			}

			if (end > text.Length - len)
				end = text.Length - len;

			while (ptr <= end) {
				int i = len - 1;
				while (str[i] == GetChar(text[ptr + i])) {
					if (-- i < 0)
						return ptr;
				}

				if (ptr < end)
					ptr += GetShiftDistance (text[ptr + len]);
				else
					break;
			}

			return -1;
		}

		// private

		private void SetupShiftTable () {
			shift = new Hashtable ();
			for (int i = 0; i < len; ++ i) {
				char c = str[i];
				shift[GetChar(c)] = len - i;
			}
		}
	    
		private int GetShiftDistance (char c) {
			if(shift == null)
				return 1;

			object s = shift[c];
			return (s != null ? (int)s : len + 1);
		}

		private char GetChar(char c) {
			return (!ignore ? c : Char.ToLower(c));
		}
		
		private string str;
		private int len;
		private bool ignore;

		private Hashtable shift;
		private readonly static int THRESHOLD = 5;
	}

}
