//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	quicksearch.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;

namespace System.Text.RegularExpressions {

	// TODO use simple test for single character strings

	class QuickSearch {
		// simplified boyer-moore for fast substring matching
	
		public QuickSearch (string str, bool ignore) {
			this.str = str;
			this.len = str.Length;
			this.ignore = ignore;
		
			Setup ();
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
			if (end > text.Length - len)
				end = text.Length - len;
		
			int ptr = start;
			if (!ignore) {
				while (ptr <= end) {
					int i = len - 1;
					while (str[i] == text[ptr + i]) {
						if (-- i < 0)
							return ptr;
					}

					if (ptr < end)
						ptr += shift[text[ptr + len]];
					else
						break;
				}
			}
			else {
				// ignore case: same as above, but we convert text
				// to lower case before doing the string compare
			
				while (ptr <= end) {
					int i = len - 1;
					while (str[i] == Char.ToLower (text[ptr + i])) {
						if (-- i < 0)
							return ptr;
					}

					if (ptr < end)
						ptr += shift[text[ptr + len]];
					else
						break;
				}
			}

			return -1;
		}

		// private

		private void Setup () {
			if (ignore)
				str = str.ToLower ();

			// this is a 64k entry shift table. that's 128kb per pattern!
			// is it worth compressing this by only storing shifts within
			// a (lo, hi) character range? for most substrings this would
			// be around 50 bytes...

			shift = new int[0x1000];
			for (int i = 0; i < 0x1000; ++ i)
				shift[i] = len + 1;

			for (int i = 0; i < len; ++ i) {
				char c = str[i];

				shift[c] = len - i;
				if (ignore)
					shift[Char.ToUpper (c)] = len - i;
			}
		}

		private string str;
		private int len;
		private bool ignore;

		private int[] shift;
	}
}
