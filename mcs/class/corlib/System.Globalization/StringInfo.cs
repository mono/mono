//
// System.Globalization.StringInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Runtime.InteropServices;

namespace System.Globalization {

	[Serializable]
#if NET_2_0
	[ComVisible(true)]
#endif
	public class StringInfo {
		public StringInfo()
		{
		}

#if NET_2_0
		string s;
		int length;

		public StringInfo (string value)
		{
			// Argument check in property
			String = value;
		}

		[ComVisible (false)]
		public override bool Equals (object value)
		{
			StringInfo other = value as StringInfo;
			return other != null && s == other.s;
		}

		[ComVisible (false)]
		public override int GetHashCode ()
		{
			return s.GetHashCode ();
		}

		public int LengthInTextElements {
			get {
				if (length < 0) {
					length = 0;
					for (int idx = 0; idx < s.Length; length++)
						idx += GetNextTextElementLength (s, idx);
				}
				return length;
			}
		}

		public string String {
			get { return s; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				length = -1;
				s = value;
			}
		}

		public string SubstringByTextElements (int startingTextElement)
		{
			if (startingTextElement < 0 || s.Length == 0)
				throw new ArgumentOutOfRangeException ("startingTextElement");
			int idx = 0;
			for (int i = 0; i < startingTextElement; i++) {
				if (idx >= s.Length)
					throw new ArgumentOutOfRangeException ("startingTextElement");
				idx += GetNextTextElementLength (s, idx);
			}
			return s.Substring (idx);
		}

		public string SubstringByTextElements (int startingTextElement, int lengthInTextElements)
		{
			if (startingTextElement < 0 || s.Length == 0)
				throw new ArgumentOutOfRangeException ("startingTextElement");
			if (lengthInTextElements < 0)
				throw new ArgumentOutOfRangeException ("lengthInTextElements");
			int idx = 0;
			for (int i = 0; i < startingTextElement; i++) {
				if (idx >= s.Length)
					throw new ArgumentOutOfRangeException ("startingTextElement");
				idx += GetNextTextElementLength (s, idx);
			}
			int start = idx;
			for (int i = 0; i < lengthInTextElements; i++) {
				if (idx >= s.Length)
					throw new ArgumentOutOfRangeException ("lengthInTextElements");
				idx += GetNextTextElementLength (s, idx);
			}
			return s.Substring (start, idx - start);
		}
#endif

		public static string GetNextTextElement(string str)
		{
			if(str == null || str.Length == 0) {
				throw new ArgumentNullException("string is null");
			}
			return(GetNextTextElement (str, 0));
		}

		public static string GetNextTextElement(string str, int index)
		{
			int len = GetNextTextElementLength (str, index);
			return len != 1 ? str.Substring (index, len) : new string (str [index], 1);
		}
		
		static int GetNextTextElementLength(string str, int index)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

#if NET_2_0
			if(index >= str.Length)
				return 0;
			if(index < 0)
#else
			if(index < 0 || index >= str.Length)
#endif
				throw new ArgumentOutOfRangeException ("Index is not valid");

			/* Find the next base character, surrogate
			 * pair or combining character sequence
			 */

			char ch = str[index];
			UnicodeCategory cat = char.GetUnicodeCategory (ch);

			if (cat == UnicodeCategory.Surrogate) {
				/* Check that it's a high surrogate
				 * followed by a low surrogate
				 */
				if (ch >= 0xD800 && ch <= 0xDBFF) {
					if ((index + 1) < str.Length &&
					    str[index + 1] >= 0xDC00 &&
					    str[index + 1] <= 0xDFFF) {
						/* A valid surrogate pair */
						return 2;
					} else {
						/* High surrogate on its own */
						return 1;
					}
				} else {
					/* Low surrogate on its own */
					return 1;
				}
			} else {
				/* Look for a base character, which
				 * may or may not be followed by a
				 * series of combining characters
				 */

				if (cat == UnicodeCategory.NonSpacingMark ||
				    cat == UnicodeCategory.SpacingCombiningMark ||
				    cat == UnicodeCategory.EnclosingMark) {
					/* Not a base character */
					return 1;
				}
				
				int count = 1;

				while (index + count < str.Length) {
					cat = char.GetUnicodeCategory (str[index + count]);
					if (cat != UnicodeCategory.NonSpacingMark &&
					    cat != UnicodeCategory.SpacingCombiningMark &&
					    cat != UnicodeCategory.EnclosingMark) {
						/* Finished the sequence */
						break;
					}
					count++;
				}

				return count;
			}
		}

		public static TextElementEnumerator GetTextElementEnumerator(string str)
		{
			if(str == null || str.Length == 0) {
				throw new ArgumentNullException("string is null");
			}
			return(new TextElementEnumerator (str, 0));
		}

		public static TextElementEnumerator GetTextElementEnumerator(string str, int index)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

			if(index < 0 || index >= str.Length) {
				throw new ArgumentOutOfRangeException ("Index is not valid");
			}
			
			return(new TextElementEnumerator (str, index));
		}
		
		public static int[] ParseCombiningCharacters(string str)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

			ArrayList indices = new ArrayList (str.Length);
			TextElementEnumerator tee = GetTextElementEnumerator (str);

			tee.Reset ();
			while(tee.MoveNext ()) {
				indices.Add (tee.ElementIndex);
			}

			return((int[])indices.ToArray (typeof (int)));
		}
	}
}
