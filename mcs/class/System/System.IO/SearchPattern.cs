//
// System.IO.SearchPattern2.cs: Filename glob support.
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
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

// Copied from corlib/System.IO/SearchPatter.cs
using System;

namespace System.IO {

	// FIXME: there's a complication with this algorithm under windows.
	// the pattern '*.*' matches all files (i think . matches the extension),
	// whereas under UNIX it should only match files containing the '.' character.

	class SearchPattern2 {
		public SearchPattern2 (string pattern) : this (pattern, false) { }

		public SearchPattern2 (string pattern, bool ignore)
		{
			this.ignore = ignore;
			this.pattern = pattern;
			Compile (pattern);
		}

		// OSX has a retarded case-insensitive yet case-aware filesystem
		// so we need a overload in here for the Kqueue watcher
		public bool IsMatch (string text, bool ignorecase)
		{
			if (!hasWildcard)
				return (String.Compare (pattern, text, ignorecase) == 0);

			return Match (ops, text, 0);
		}

		public bool IsMatch (string text)
		{
			return IsMatch (text, ignore);
		}

		public bool HasWildcard {
			get { return hasWildcard; }
		}
		// private

		Op ops;		// the compiled pattern
		bool ignore;	// ignore case
		bool hasWildcard;
		string pattern;

		private void Compile (string pattern)
		{
			if (pattern == null || pattern.IndexOfAny (InvalidChars) >= 0)
				throw new ArgumentException ("Invalid search pattern: '" + pattern + "'");

			if (pattern == "*") {	// common case
				ops = new Op (OpCode.True);
				hasWildcard = true;
				return;
			}

			ops = null;

			int ptr = 0;
			Op last_op = null;
			while (ptr < pattern.Length) {
				Op op;
			
				switch (pattern [ptr]) {
				case '?':
					op = new Op (OpCode.AnyChar);
					++ ptr;
					hasWildcard = true;
					break;

				case '*':
					op = new Op (OpCode.AnyString);
					++ ptr;
					hasWildcard = true;
					break;
					
				default:
					op = new Op (OpCode.ExactString);
					int end = pattern.IndexOfAny (WildcardChars, ptr);
					if (end < 0)
						end = pattern.Length;

					op.Argument = pattern.Substring (ptr, end - ptr);
					if (ignore)
						op.Argument = op.Argument.ToLower ();

					ptr = end;
					break;
				}

				if (last_op == null)
					ops = op;
				else
					last_op.Next = op;

				last_op = op;
			}

			if (last_op == null)
				ops = new Op (OpCode.End);
			else
				last_op.Next = new Op (OpCode.End);
		}

		private bool Match (Op op, string text, int ptr)
		{
			while (op != null) {
				switch (op.Code) {
				case OpCode.True:
					return true;

				case OpCode.End:
					if (ptr == text.Length)
						return true;

					return false;
				
				case OpCode.ExactString:
					int length = op.Argument.Length;
					if (ptr + length > text.Length)
						return false;

					string str = text.Substring (ptr, length);
					if (ignore)
						str = str.ToLower ();

					if (str != op.Argument)
						return false;

					ptr += length;
					break;

				case OpCode.AnyChar:
					if (++ ptr > text.Length)
						return false;
					break;

				case OpCode.AnyString:
					while (ptr <= text.Length) {
						if (Match (op.Next, text, ptr))
							return true;

						++ ptr;
					}

					return false;
				}

				op = op.Next;
			}

			return true;
		}

		// private static

		internal static readonly char [] WildcardChars = { '*', '?' };
		internal static readonly char [] InvalidChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

		private class Op {
			public Op (OpCode code)
			{
				this.Code = code;
				this.Argument = null;
				this.Next = null;
			}
		
			public OpCode Code;
			public string Argument;
			public Op Next;
		}

		private enum OpCode {
			ExactString,		// literal
			AnyChar,		// ?
			AnyString,		// *
			End,			// end of pattern
			True			// always succeeds
		};
	}
}
