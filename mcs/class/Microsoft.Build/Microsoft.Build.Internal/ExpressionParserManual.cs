//
// ExpressionParserManual.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Internal.Expressions
{
	class ExpressionParserManual
	{
		// FIXME: we are going to not need ExpressionValidationType for this; always LaxString.
		public ExpressionParserManual (string source, ExpressionValidationType validationType)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			this.source = source;
			validation_type = validationType;
		}
		
		string source;
		ExpressionValidationType validation_type;
		
		public ExpressionList Parse ()
		{
			return Parse (0, source.Length);
		}
		
		ExpressionList Parse (int start, int end)
		{
			if (string.IsNullOrWhiteSpace (source))
				return new ExpressionList ();

			var ret = new ExpressionList ();
			while (start < end) {
				int bak = start;
				ret.Add (ParseSingle (ref start, end));
				SkipSpaces (ref start);
				if (bak == start)
					throw new Exception ("Parser failed to progress token position: " + source);
			}
			return ret;
		}
		
		static readonly char [] token_starters = "$@%(),".ToCharArray ();
		
		Expression ParseSingle (ref int start, int end)
		{
			char token = source [start];
			switch (token) {
			case '$':
			case '@':
			case '%':
				if (start == end || start + 1 == source.Length || source [start + 1] != '(') {
					if (validation_type == ExpressionValidationType.StrictBoolean)
						throw new InvalidProjectFileException (string.Format ("missing '(' after '{0}' at {1} in \"{2}\"", source [start], start, source));
					else
						goto default; // treat as raw literal to the section end
				}
				start += 2;
				int last = FindMatchingCloseParen (start, end);
				if (last < 0) {
					if (validation_type == ExpressionValidationType.StrictBoolean)
						throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
					else {
						start -= 2;
						goto default; // treat as raw literal to the section end
					}
				}
				Expression ret;
				if (token == '$')
					ret = EvaluatePropertyExpression (start, last);
				else if (token == '%')
					ret = EvaluateMetadataExpression (start, last);
				else
					ret = EvaluateItemExpression (start, last);
				start = last + 1;
				return ret;
				
			// Below (until default) are important only for Condition evaluation
			case '(':
				if (validation_type == ExpressionValidationType.LaxString)
					goto default;
				start++;
				last = FindMatchingCloseParen (start, end);
				if (last < 0) {
					if (validation_type == ExpressionValidationType.StrictBoolean)
						throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
					else {
						start--;
						goto default; // treat as raw literal to the section end
					}
				}
				var contents = Parse (start, last).ToArray ();
				if (contents.Length > 1)
					throw new InvalidProjectFileException (string.Format ("unexpected continuous expression within (){0} in \"{1}\"", contents [1].Column > 0 ? " at " + contents [1].Column : null, source));
				return contents.First ();

			default:
				int idx = source.IndexOfAny (token_starters, start + 1);
				string name = idx < 0 ? source.Substring (start, end - start) : source.Substring (start, idx - start);
				var val = new NameToken () { Name = name };
				ret = new RawStringLiteral () { Value = val };
				if (idx >= 0)
					start = idx;
				else
					start = end;

				return ret;
			}
		}
		
		int FindMatchingCloseParen (int start, int end)
		{
			int n = 0;
			for (int i = start; i < end; i++) {
				if (source [i] == '(')
					n++;
				else if (source [i] == ')') {
					if (n-- == 0)
						return i;
				}
			}
			return -1; // invalid
		}
		
		static readonly string spaces = " \t\r\n";
		
		void SkipSpaces (ref int start)
		{
			while (start < source.Length && spaces.Contains (source [start]))
				start++;
		}
		
		PropertyAccessExpression EvaluatePropertyExpression (int start, int end)
		{
			// member access
			int dotAt = source.LastIndexOf ('.', end, end - start);
			int colonsAt = source.LastIndexOf ("::", end, end - start, StringComparison.Ordinal);
			if (dotAt < 0 && colonsAt < 0) {
				// property access without member specification
				int parenAt = source.IndexOf ('(', start, end - start);
				string name = parenAt < 0 ? source.Substring (start, end - start) : source.Substring (start, parenAt - start);
				var access = new PropertyAccess () {
					Name = new NameToken () { Name = name },
					TargetType = PropertyTargetType.Object
					};
				if (parenAt > 0) { // method arguments
					start = parenAt + 1;
					access.Arguments = ParseFunctionArguments (ref start, end);
				}
				return new PropertyAccessExpression () { Access = access };
			}
			if (colonsAt < 0 || colonsAt < dotAt) {
				// property access with member specification
				int mstart = dotAt + 1;
				int parenAt = source.IndexOf ('(', mstart, end - mstart);
				string name = parenAt < 0 ? source.Substring (mstart, end - mstart) : source.Substring (mstart, parenAt - mstart);
				var access = new PropertyAccess () {
					Name = new NameToken () { Name = name },
					TargetType = PropertyTargetType.Object,
					Target = dotAt < 0 ? null : Parse (start, dotAt).FirstOrDefault () 
				};
				if (parenAt > 0) { // method arguments
					start = parenAt + 1;
					access.Arguments = ParseFunctionArguments (ref start, end);
				}
				return new PropertyAccessExpression () { Access = access };
			} else {
				// static type access
				string type = source.Substring (start, colonsAt - start);
				if (type.Length < 2 || type [0] != '[' || type [type.Length - 1] != ']')
					throw new InvalidProjectFileException (string.Format ("Static function call misses appropriate type name surrounded by '[' and ']' at {0} in \"{1}\"", start, source));
				type = type.Substring (1, type.Length - 2);
				start = colonsAt + 2;
				int parenAt = source.IndexOf ('(', start, end - start);
				string member = parenAt < 0 ? source.Substring (start, end - start) : source.Substring (start, parenAt - start);
				if (member.Length == 0)
					throw new InvalidProjectFileException ("Static member name is missing");
				var access = new PropertyAccess () {
					Name = new NameToken () { Name = member },
					TargetType = PropertyTargetType.Type,
					Target = new StringLiteral () { Value = new NameToken () { Name = type } }
				};
				if (parenAt > 0) { // method arguments
					start = parenAt + 1;
					access.Arguments = ParseFunctionArguments (ref start, end);
				}
				return new PropertyAccessExpression () { Access = access };
			}
		}
		
		ExpressionList ParseFunctionArguments (ref int start, int end)
		{
			var args = new ExpressionList ();
			do {
				SkipSpaces (ref start);
				if (start == source.Length)
					throw new InvalidProjectFileException ("unterminated function call arguments.");
				if (source [start] == ')')
					break;
				else if (args.Any ()) {
					if (source [start] != ',')
						throw new InvalidProjectFileException (string.Format ("invalid function call arguments specification. ',' is expected, got '{0}'", source [start]));
					start++;
				}
				args.Add (ParseSingle (ref start, end));
			} while (true);
			start++;
			return args;
		}
		
		ItemAccessExpression EvaluateItemExpression (int start, int end)
		{
			// using property as context and evaluate
			int idx = source.IndexOf ("->", start, StringComparison.Ordinal);
			if (idx > 0) {
				string name = source.Substring (start, idx - start);
				return new ItemAccessExpression () {
					Application = new ItemApplication () {
						Name = new NameToken () { Name = name },
						Expressions = Parse (idx + 2, end)
						}
					};
			} else {
				string name = source.Substring (start, end - start);
				return new ItemAccessExpression () {
					Application = new ItemApplication () { Name = new NameToken () { Name = name } }
					};
			}
		}
		
		MetadataAccessExpression EvaluateMetadataExpression (int start, int end)
		{
			int idx = source.IndexOf ('.', start, end - start);
			string item = idx < 0 ? null : source.Substring (start, idx - start);
			string meta = idx < 0 ? source.Substring (start, end - start) : source.Substring (idx + 1, end - idx - 1);
			var access = new MetadataAccess () {
					ItemType = item == null ? null : new NameToken () { Column = start, Name = item },
					Metadata = new NameToken () { Column = idx < 0 ? start : idx + 1, Name = meta }
					};
			return new MetadataAccessExpression () { Access = access };
		}
	}
}

