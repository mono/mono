using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Internal
{
	class ExpressionParserManual
	{
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
		
		static readonly char [] token_starters = "$@%('\"".ToCharArray ();
		
		ExpressionList Parse (int start, int end)
		{
			if (string.IsNullOrWhiteSpace (source))
				return new ExpressionList ();

			var head = new List<Expression> ();
			var tail = new List<Expression> ();
			while (start < end) {
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
					start++;
					int last = source.LastIndexOf (')', end - 1, end - start - 1);
					if (last < 0) {
						if (validation_type == ExpressionValidationType.StrictBoolean)
							throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
						else {
							if (token != '(')
								start--;
							goto default; // treat as raw literal to the section end
						}
					}
					start++;
					if (token == '$')
						head.Add (EvaluatePropertyExpression (start, last));
					else if (token == '%')
						head.Add (EvaluateMetadataExpression (start, last));
					else
						head.Add (EvaluateItemExpression (start, last));
					start = last + 1;
					break;
					
				// Below (until default) are important only for Condition evaluation
				case '(':
					if (validation_type == ExpressionValidationType.LaxString)
						goto default;
					start++;
					last = source.LastIndexOf (')', end - 1, end - start);
					if (last < 0)
						throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
					var contents = Parse (start, last).ToArray ();
					if (contents.Length > 1)
						throw new InvalidProjectFileException (string.Format ("unexpected continuous expression within (){0} in \"{1}\"", contents [1].Column > 0 ? " at " + contents [1].Column : null, source));
					head.Add (contents.First ());
					break;

				default:
					int idx = source.IndexOfAny (token_starters, start + 1);
					string name = idx < 0 ? source.Substring (start, end - start) : source.Substring (start, idx - start);
					var val = new NameToken () { Name = name };
					head.Add (new StringLiteral () { Value = val });
					if (idx >= 0)
						start = idx;
					else
						start = end;

					break;
				}
				SkipSpaces (ref start);
			}
			var ret = new ExpressionList ();
			foreach (var e in head.Concat (((IEnumerable<Expression>) tail).Reverse ()))
				ret.Add (e);
			return ret;
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
			int idx = source.LastIndexOf ('.', start);
			if (idx >= 0) {
				string name = (idx > 0) ? source.Substring (idx, end - idx) : source.Substring (start, end);
				return new PropertyAccessExpression () {
					Access = new PropertyAccess () {
						Name = new NameToken () { Name = name },
						TargetType = PropertyTargetType.Object,
						Target = idx < 0 ? null : Parse (start, idx).FirstOrDefault () 
						}
					};
			} else {
				// static type access
				idx = source.IndexOf ("::", start, StringComparison.Ordinal);
				if (idx >= 0) {
					throw new NotImplementedException ();
				
					string type = source.Substring (start, idx - start);
					if (type.Length < 2 || type [0] != '[' || type [type.Length - 1] != ']')
						throw new InvalidProjectFileException (string.Format ("Static function call misses appropriate type name surrounded by '[' and ']' at {0} in \"{1}\"", start, source));
					int start2 = idx + 2;
					int idx2 = source.IndexOf ('(', idx + 2, end - start2);
					if (idx2 < 0) {
						// access to static property
						string member = source.Substring (start2, end - start2);
					} else {
						// access to static method
						string member = source.Substring (start2, idx2 - start2);
					}
				} else {
					// property access without member specification
					return new PropertyAccessExpression () {
						Access = new PropertyAccess () {
							Name = new NameToken () { Name = source.Substring (start, end - start) },
							TargetType = PropertyTargetType.Object
							}
						};
				}
			}
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

