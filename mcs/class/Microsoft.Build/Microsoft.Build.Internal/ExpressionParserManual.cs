using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Internal
{
	class ExpressionParserManual
	{
		// FIXME: we are going to not need ExpressionValidationType for this; always LaxString.
		public ExpressionParserManual (string source, ExpressionValidationType validationType)
		{
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
				ret.Add (ParseSingle (ref start, ref end));
				SkipSpaces (ref start);
			}
			return ret;
		}
		
		static readonly char [] token_starters = "$@%(),".ToCharArray ();
		
		Expression ParseSingle (ref int start, ref int end)
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
				if (last < 0)
				if (validation_type == ExpressionValidationType.StrictBoolean)
					throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
				else {
					start--;
					goto default; // treat as raw literal to the section end
				}
				var contents = Parse (start, last).ToArray ();
				if (contents.Length > 1)
					throw new InvalidProjectFileException (string.Format ("unexpected continuous expression within (){0} in \"{1}\"", contents [1].Column > 0 ? " at " + contents [1].Column : null, source));
				return contents.First ();

			default:
				int idx = source.IndexOfAny (token_starters, start + 1);
				string name = idx < 0 ? source.Substring (start, end - start) : source.Substring (start, idx - start);
				var val = new NameToken () { Name = name };
				ret = new StringLiteral () { Value = val };
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
			int dotAt = source.LastIndexOf ('.', start);
			if (dotAt >= 0) {
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
					access.Arguments = ParseFunctionArguments (ref start, ref end);
				}
				return new PropertyAccessExpression () { Access = access };
			} else {
				// static type access
				int colonsAt = source.IndexOf ("::", start, StringComparison.Ordinal);
				if (colonsAt >= 0) {
					string type = source.Substring (start, colonsAt - start);
					if (type.Length < 2 || type [0] != '[' || type [type.Length - 1] != ']')
						throw new InvalidProjectFileException (string.Format ("Static function call misses appropriate type name surrounded by '[' and ']' at {0} in \"{1}\"", start, source));
					type = type.Substring (1, type.Length - 2);
					start = colonsAt + 2;
					int parenAt = source.IndexOf ('(', colonsAt + 2, end - colonsAt - 2);
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
						access.Arguments = ParseFunctionArguments (ref start, ref end);
					}
					return new PropertyAccessExpression () { Access = access };
				} else {
					// property access without member specification
					int parenAt = source.IndexOf ('(', start, end - start);
					string name = parenAt < 0 ? source.Substring (start, end - start) : source.Substring (start, parenAt - start);
					var access = new PropertyAccess () {
						Name = new NameToken () { Name = name },
						TargetType = PropertyTargetType.Object
						};
					if (parenAt > 0) { // method arguments
						start = parenAt + 1;
						access.Arguments = ParseFunctionArguments (ref start, ref end);
					}
					return new PropertyAccessExpression () { Access = access };
				}
			}
		}
		
		ExpressionList ParseFunctionArguments (ref int start, ref int end)
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
				args.Add (ParseSingle (ref start, ref end));
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
						Expressions = Parse (idx, end - idx)
						}
					};
			} else {
				string name = source.Substring (start, end - start);
				return new ItemAccessExpression () {
					Application = new ItemApplication () { Name = new NameToken () { Name = name } }
					};
			}
			
			throw new NotImplementedException ();
		}
		
		MetadataAccessExpression EvaluateMetadataExpression (int start, int end)
		{
			throw new NotImplementedException ();
		}
	}
}

