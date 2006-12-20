//
// Utilities.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using System.Text;

namespace Microsoft.Build.BuildEngine {
	public static class Utilities {
	
		static Hashtable charsToEscape;
	
		static Utilities ()
		{
			charsToEscape = new Hashtable ();
			
			charsToEscape.Add ('$', null);
			charsToEscape.Add ('%', null);
			charsToEscape.Add ('\'', null);
			charsToEscape.Add ('(', null);
			charsToEscape.Add (')', null);
			charsToEscape.Add ('*', null);
			charsToEscape.Add (';', null);
			charsToEscape.Add ('?', null);
			charsToEscape.Add ('@', null);
		}
	
		public static string Escape (string unescapedExpression)
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (char c in unescapedExpression) {
				if (charsToEscape.Contains (c))
					sb.AppendFormat ("%{0:x2}", (int) c);
				else
					sb.Append (c);
			}
			
			return sb.ToString ();
		}
		
		// FIXME: add tests for this
		internal static string Unescape (string escapedExpression)
		{
			StringBuilder sb = new StringBuilder ();
			
			int i = 0;
			while (i < escapedExpression.Length) {
				sb.Append (Uri.HexUnescape (escapedExpression, ref i));
			}
			
			return sb.ToString ();
		}
	}
}

#endif
