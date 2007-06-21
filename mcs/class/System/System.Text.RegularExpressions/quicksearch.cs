//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	quicksearch.cs
//
// Authors:	Dan Lewis (dlewis@gmx.co.uk)
//	         Juraj Skripsky (juraj@hotfeet.ch)
//
// (c) 2002 Dan Lewis
// (c) 2007 Juraj Skripsky
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

namespace System.Text.RegularExpressions {
	internal class QuickSearch {
		// simplified boyer-moore for fast substring matching
		// (for short strings, we use simple scans)
		public QuickSearch (string str, bool ignore) 
			: this(str, ignore, false)
		{
		}
	
		public QuickSearch (string str, bool ignore, bool reverse) {
			this.str = str;
			this.len = str.Length;
			this.ignore = ignore;
			this.reverse = reverse;

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

		
			if ( reverse ) 
			{
				if (start < end)
					return -1;

				if ( ptr > text.Length) 
				{
					ptr = text.Length;
				}

				// use simple scan for a single-character search string
				if (len == 1) 
				{
					while (--ptr >= end) 
					{
						if(str[0] == GetChar(text[ptr]))
							return ptr ;
						
					}
					return -1;
				}

		
				if ( end < len)
					end =  len - 1 ;

				ptr--;
				while (ptr >= end) 
				{
					int i = len -1 ;
					while (str[i] == GetChar(text[ptr - len +1 + i])) 
					{
						if (-- i <  0)
							return ptr - len + 1;
					}

					if (ptr > end)
					{
						ptr -= GetShiftDistance (text[ptr - len ]);
					
					}
					else
						break;
				}

			}
			else 
			{
				// use simple scan for a single-character search string
				if (len == 1) 
				{
					while (ptr <= end) 
					{
						if(str[0] == GetChar(text[ptr]))
							return ptr;
						else
							ptr++;
					}	
					return -1;
				}

				if (end > text.Length - len)
					end = text.Length - len;

				while (ptr <= end) 
				{
					int i = len - 1;
					while (str[i] == GetChar(text[ptr + i])) 
					{
						if (-- i < 0)
							return ptr;
					}

					if (ptr < end)
						ptr += GetShiftDistance (text[ptr + len]);
					else
						break;
				}
			}

			return -1;
				
		}

			// private

		private void SetupShiftTable () {
			bool needsExtendedTable = len > (byte.MaxValue - 1);

			byte maxLowChar = 0;
			for (int i = 0; i < len; i++) {
				char cur = str [i];
				if (cur <= (char)byte.MaxValue) {
					if ((byte)cur > maxLowChar)
						maxLowChar = (byte)cur;
				} else
					needsExtendedTable = true;
			}
			
			shift = new byte [maxLowChar + 1];
			if (needsExtendedTable)
				shiftExtended = new Hashtable ();

			for (int i = 0, j = len; i < len; i++, j--) {
				char c = str [(!reverse ? i : j - 1)];
				if (c < shift.Length) {
					if (j < byte.MaxValue) {
						shift [c] = (byte)j;
						continue;
					} else {
						shift [c] = byte.MaxValue;
					}
				}
				shiftExtended [c] = j;
			}
		}
	    
		private int GetShiftDistance (char c) {
			if (shift == null)
				return 1;
				
			c = GetChar (c);
			if (c < shift.Length) {
				int dist = shift [c];
				if (dist == 0) {
					return len + 1;
				} else {
					if (dist != byte.MaxValue)
						return dist;
				}
			} else {
				if (c < (char)byte.MaxValue)	
					return len + 1;
			}

			if (shiftExtended == null)
				return len + 1;

			object s = shiftExtended [c];
			return (s != null ? (int)s : len + 1);
		}

		private char GetChar(char c) {
			return (!ignore ? c : Char.ToLower(c));
		}

		private string str;
		private int len;
		private bool ignore;
		private bool reverse;

		// shift[idx] contains value x  means 
		//  x == 0             => shift dist. == len + 1
		//  x == byte.Maxvalue => shift dist. >= 255, look it up in shiftExtended
		//  otherwise          => shift dist. == x 
		private byte[] shift;
		private Hashtable shiftExtended;
		private readonly static int THRESHOLD = 5;
	}

}
