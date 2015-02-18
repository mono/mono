//
// MSBuildErrorParser.cs: Parser for MSBuild-format error messages.
//
// Author:
//   Michael Hutchinson (m.j.hutchinson@gmail.com)
//
// Copyright 2014 Xamarin Inc. (http://www.xamarin.com)
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

using System;

namespace Microsoft.Build.Utilities
{
	static class MSBuildErrorParser
	{
		public class Result
		{
			public string Origin { get; set; }
			public int Line { get; set; }
			public int Column { get; set; }
			public int EndLine { get; set; }
			public int EndColumn { get; set; }
			public string Subcategory { get; set; }
			public bool IsError { get; set; }
			public string Code { get; set; }
			public string Message { get; set; }
		}

		// Parses single-line error message in the standard MSBuild error format:
		//
		// [origin[(position)]:][subcategory] category code: [message]
		//
		// Components in [] square brackets are optional.
		// Components are as follows:
		//  origin: tool name or filename, may contain whitespace, no colons except the drive letter
		//  position: line/col position or range in the file, with one of the following forms:
		//      (l), (l,c), (l,c-c), (l,c,l,c)
		//  subcategory: arbitrary text, may contain whitepsace
		//  code: error code, no whietspace or punctuation
		//  message: arbitraty text, no restrictions
		//
		public static Result TryParseLine (string line)
		{
			int originEnd, originStart = 0;
			var result = new Result ();

			MoveNextNonSpace (line, ref originStart);

			if (originStart >= line.Length)
				return null;

			//find the origin section
			//the filename may include a colon for Windows drive e.g. C:\foo, so ignore colon in first 2 chars
			if (line[originStart] != ':') {
				if (originStart + 2 >= line.Length)
					return null;

				if ((originEnd = line.IndexOf (':', originStart + 2) - 1) < 0)
					return null;
			} else {
				originEnd = originStart;
			}

			int categoryStart = originEnd + 2;

			if (categoryStart > line.Length)
				return null;

			MovePrevNonSpace (line, ref originEnd);

			//if there is no origin section, then we can't parse the message
			if (originEnd < 0 || originEnd < originStart)
				return null;

			//find the category section, if there is one
			MoveNextNonSpace (line, ref categoryStart);

			int categoryEnd = line.IndexOf (':', categoryStart) - 1;
			int messageStart = categoryEnd + 2;

			if (categoryEnd >= 0) {
				MovePrevNonSpace (line, ref categoryEnd);
				if (categoryEnd <= categoryStart)
					categoryEnd = -1;
			}

			//if there is a category section and it parses
			if (categoryEnd > 0 && ParseCategory (line, categoryStart, categoryEnd, result)) {
				//then parse the origin section
				if (originEnd > originStart && !ParseOrigin (line, originStart, originEnd, result))
					return null;
			} else {
				//there is no origin, parse the origin section as if it were the category
				if (!ParseCategory (line, originStart, originEnd, result))
					return null;
				messageStart = categoryStart;
			}

			//read the remaining message
			MoveNextNonSpace (line, ref messageStart);
			int messageEnd = line.Length - 1;
			MovePrevNonSpace (line, ref messageEnd, messageStart);
			if (messageEnd > messageStart) {
				result.Message = line.Substring (messageStart, messageEnd - messageStart + 1);
			} else {
				result.Message = "";
			}

			return result;
		}

		// filename (line,col) | tool :
		static bool ParseOrigin (string line, int start, int end, Result result)
		{
			// no line/col
			if (line [end] != ')') {
				result.Origin = line.Substring (start, end - start + 1);
				return true;
			}

			//scan back for matching (, assuming at least one char between them
			int posStart = line.LastIndexOf ('(', end - 2, end - start - 2);
			if (posStart < 0)
				return false;

			if (!ParsePosition (line, posStart + 1, end, result)) {
				result.Origin = line.Substring (start, end - start + 1);
				return true;
			}

			end = posStart - 1;
			MovePrevNonSpace (line, ref end, start);

			result.Origin = line.Substring (start, end - start + 1);
			return true;
		}

		static bool ParseLineColVal (string str, out int val)
		{
			try {
				val = int.Parse (str);
				return true;
			} catch (OverflowException) {
				val = 0;
				return true;
			} catch (FormatException) {
				val = 0;
				return false;
			}
		}

		// Supported combos:
		//
		// (SL,SC,EL,EC)
		// (SL,SC-EC)
		// (SL-EL)
		// (SL,SC)
		// (SL)
		//
		// Unexpected patterns of commas/dashes abort parsing, discarding all values.
		// Any other characters abort parsing and the (...) gets treated as pert of the filename.
		// Overflows are silently treated as zeroes.
		//
		static bool ParsePosition (string str, int start, int end, Result result)
		{
			int line = 0, col = 0, endLine = 0, endCol = 0;

			var a = str.Substring (start, end - start).Split (',');

			if (a.Length > 4 || a.Length == 3)
				return true;

			if (a.Length == 4) {
				bool valid =
					ParseLineColVal (a [0], out line) &&
					ParseLineColVal (a [1], out col) &&
					ParseLineColVal (a [2], out endLine) &&
					ParseLineColVal (a [3], out endCol);
				if (!valid)
					return false;
			} else {
				var b = a [0].Split ('-');
				if (b.Length > 2)
					return true;
				if (!ParseLineColVal (b [0], out line))
					return false;
				if (b.Length == 2) {
					if (a.Length == 2)
						return true;
					if (!ParseLineColVal (b [1], out endLine))
						return false;
				}
				if (a.Length == 2) {
					var c = a [1].Split ('-');
					if (c.Length > 2)
						return true;
					if (!ParseLineColVal (c [0], out col))
						return false;
					if (c.Length == 2) {
						if (!ParseLineColVal (c [1], out endCol))
							return false;
					}
				}
			}

			result.Line = line;
			result.Column = col;
			result.EndLine = endLine;
			result.EndColumn = endCol;
			return true;
		}

		static bool ParseCategory (string line, int start, int end, Result result)
		{
			int idx = end;
			MovePrevWordStart (line, ref idx, start);
			if (idx < start + 1)
				return false;

			string code = line.Substring (idx, end - idx + 1);

			idx--;
			MovePrevNonSpace (line, ref idx, start);
			end = idx;
			MovePrevWordStart (line, ref idx, start);
			if (idx < start)
				return false;

			string category = line.Substring (idx , end - idx + 1);
			if (string.Equals (category, "error", StringComparison.OrdinalIgnoreCase))
				result.IsError = true;
			else if (!string.Equals (category, "warning", StringComparison.OrdinalIgnoreCase))
				return false;

			result.Code = code;

			idx--;
			if (idx > start) {
				MovePrevNonSpace (line, ref idx, start);
				result.Subcategory = line.Substring (start, idx - start + 1);
			} else {
				result.Subcategory = "";
			}

			return true;
		}

		static void MoveNextNonSpace (string s, ref int idx)
		{
			while (idx < s.Length && char.IsWhiteSpace (s[idx]))
				idx++;
		}

		static void MovePrevNonSpace (string s, ref int idx, int min = 0)
		{
			while (idx > min && char.IsWhiteSpace (s[idx]))
				idx--;
		}

		static void MovePrevWordStart (string s, ref int idx, int min = 0)
		{
			while (idx > min && char.IsLetterOrDigit (s[idx - 1]))
				idx--;
		}
	}
}
