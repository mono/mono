//
// ConditionParser.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// (C) 2006 Marek Sieradzki
// (C) 2004-2006 Jaroslaw Kowalski
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
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Build.BuildEngine {

	internal class ConditionParser {
	
		ConditionTokenizer tokenizer;
		string conditionStr;
		
		ConditionParser (string condition)
		{
			tokenizer = new ConditionTokenizer ();
			tokenizer.Tokenize (condition);
			conditionStr = condition;
		}
		
		public static bool ParseAndEvaluate (string condition, Project context)
		{
			if (String.IsNullOrEmpty (condition))
				return true;

			try {
				ConditionExpression ce = ParseCondition (condition);

				if (!ce.CanEvaluateToBool (context))
					throw new InvalidProjectFileException (String.Format ("Can not evaluate \"{0}\" to bool.", condition));

				return ce.BoolEvaluate (context);
			} catch (ExpressionParseException epe) {
				throw new InvalidProjectFileException (
						String.Format ("Unable to parse condition \"{0}\" : {1}", condition, epe.Message),
						epe);
			} catch (ExpressionEvaluationException epe) {
				throw new InvalidProjectFileException (
						String.Format ("Unable to evaluate condition \"{0}\" : {1}", condition, epe.Message),
						epe);
			}
		}

		public static ConditionExpression ParseCondition (string condition)
		{
			ConditionParser parser = new ConditionParser (condition);
			ConditionExpression e = parser.ParseExpression ();
			
			if (!parser.tokenizer.IsEOF ())
				throw new ExpressionParseException (String.Format ("Unexpected token found, {0}, in condition \"{1}\"", parser.tokenizer.Token, condition));
			
			return e;
		}
		
		ConditionExpression ParseExpression ()
		{
			return ParseBooleanExpression ();
		}
		
		ConditionExpression ParseBooleanExpression ()
		{
			return ParseBooleanAnd ();
		}
		
		ConditionExpression ParseBooleanAnd ()
		{
			ConditionExpression e = ParseBooleanOr ();
			
			while (tokenizer.IsToken (TokenType.And)) {
				tokenizer.GetNextToken ();
				e = new ConditionAndExpression (e, ParseBooleanOr ());
			}
			
			return e;
		}
		
		ConditionExpression ParseBooleanOr ()
		{
			ConditionExpression e = ParseRelationalExpression ();
			
			while (tokenizer.IsToken (TokenType.Or)) {
				tokenizer.GetNextToken ();
				e = new ConditionOrExpression (e, ParseRelationalExpression ());
			}
			
			return e;
		}
		
		ConditionExpression ParseRelationalExpression ()
		{
			ConditionExpression e = ParseFactorExpression ();
			
			Token opToken;
			RelationOperator op;
			
			if (tokenizer.IsToken (TokenType.Less) ||
				tokenizer.IsToken (TokenType.Greater) ||
				tokenizer.IsToken (TokenType.Equal) ||
				tokenizer.IsToken (TokenType.NotEqual) ||
				tokenizer.IsToken (TokenType.LessOrEqual) ||
				tokenizer.IsToken (TokenType.GreaterOrEqual)) {
				
				opToken = tokenizer.Token;
				tokenizer.GetNextToken ();
								
				switch (opToken.Type) {
				case TokenType.Equal:
					op = RelationOperator.Equal;
					break;
				case TokenType.NotEqual:
					op = RelationOperator.NotEqual;
					break;
				case TokenType.Less:
					op = RelationOperator.Less;
					break;
				case TokenType.LessOrEqual:
					op = RelationOperator.LessOrEqual;
					break;
				case TokenType.Greater:
					op = RelationOperator.Greater;
					break;
				case TokenType.GreaterOrEqual:
					op = RelationOperator.GreaterOrEqual;
					break;
				default:
					throw new ExpressionParseException (String.Format ("Wrong relation operator {0}", opToken.Value));
				}

				e =  new ConditionRelationalExpression (e, ParseFactorExpression (), op);
			}
			
			return e;
		}
		
		ConditionExpression ParseFactorExpression ()
		{
			ConditionExpression e;
			Token token = tokenizer.Token;
			tokenizer.GetNextToken ();

			if (token.Type == TokenType.LeftParen) {
				e = ParseExpression ();
				tokenizer.Expect (TokenType.RightParen);
			} else if (token.Type == TokenType.String && tokenizer.Token.Type == TokenType.LeftParen) {
				e = ParseFunctionExpression (token.Value);
			} else if (token.Type == TokenType.String) {
				e = new ConditionFactorExpression (token);
			} else if (token.Type == TokenType.Number) {
				e = new ConditionFactorExpression (token);
			} else if (token.Type == TokenType.Item || token.Type == TokenType.Property
					|| token.Type == TokenType.Metadata) {
				e = ParseReferenceExpression (token.Value);
			} else if (token.Type == TokenType.Not) {
				e = ParseNotExpression ();
			} else
				throw new ExpressionParseException (String.Format ("Unexpected token {0}, while parsing condition \"{1}\"", token, conditionStr));
			
			return e;
		}

		ConditionExpression ParseNotExpression ()
		{
			return new ConditionNotExpression (ParseFactorExpression ());
		}

		ConditionExpression ParseFunctionExpression (string function_name)
		{
			return new ConditionFunctionExpression (function_name, ParseFunctionArguments ());
		}
		
		List <ConditionFactorExpression> ParseFunctionArguments ()
		{
			List <ConditionFactorExpression> list = new List <ConditionFactorExpression> ();
			ConditionFactorExpression e;
			
			while (true) {
				tokenizer.GetNextToken ();
				if (tokenizer.Token.Type == TokenType.RightParen) {
					tokenizer.GetNextToken ();
					break;
				}
				if (tokenizer.Token.Type == TokenType.Comma)
					continue;
					
				tokenizer.Putback (tokenizer.Token);
				e = (ConditionFactorExpression) ParseFactorExpression ();
				list.Add (e);
			}
			
			return list;
		}

		//@prefix: @ or $
		ConditionExpression ParseReferenceExpression (string prefix)
		{
			StringBuilder sb = new StringBuilder ();

			string ref_type = prefix [0] == '$' ? "a property" : "an item list";
			int token_pos = tokenizer.Token.Position;
			IsAtToken (TokenType.LeftParen, String.Format (
						"Expected {0} at position {1} in condition \"{2}\". Missing opening parantheses after the '{3}'.",
						ref_type, token_pos, conditionStr, prefix));
			tokenizer.GetNextToken ();

			sb.AppendFormat ("{0}({1}", prefix, tokenizer.Token.Value);

			tokenizer.GetNextToken ();
			if (prefix == "@" && tokenizer.Token.Type == TokenType.Transform) {
				tokenizer.GetNextToken ();
				sb.AppendFormat ("->'{0}'", tokenizer.Token.Value);

				tokenizer.GetNextToken ();
				if (tokenizer.Token.Type == TokenType.Comma) {
					tokenizer.GetNextToken ();
					sb.AppendFormat (", '{0}'", tokenizer.Token.Value);
					tokenizer.GetNextToken ();
				}
			}

			IsAtToken (TokenType.RightParen, String.Format (
						"Expected {0} at position {1} in condition \"{2}\". Missing closing parantheses'.",
						ref_type, token_pos, conditionStr, prefix));
			tokenizer.GetNextToken ();

			sb.Append (")");

			//FIXME: HACKY!
			return new ConditionFactorExpression (new Token (sb.ToString (), TokenType.String, token_pos));
		}

		// used to check current token type
		void IsAtToken (TokenType type, string error_msg)
		{
			if (tokenizer.Token.Type != type) {
				if (!String.IsNullOrEmpty (error_msg))
					throw new ExpressionParseException (error_msg);

				if (tokenizer.Token.Type == TokenType.EOF)
					throw new ExpressionParseException (String.Format (
								"Expected a \"{0}\" but the condition ended abruptly, while parsing condition \"{1}\"",
								Token.TypeAsString (type), conditionStr));

				throw new ExpressionParseException (String.Format (
								"Expected \"{0}\" token,  but got {1}, while parsing \"{2}\"",
								Token.TypeAsString (type), tokenizer.Token, conditionStr));
			}
		}
	}
}

#endif
