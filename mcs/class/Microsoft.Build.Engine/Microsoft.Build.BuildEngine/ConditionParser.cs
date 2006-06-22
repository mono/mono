//
// ConditionalParser.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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

	internal class ConditionParser {
	
		ConditionTokenizer tokenizer = new ConditionTokenizer ();
		
		private ConditionParser (string condition)
		{
			tokenizer.Tokenize (condition);
		}
		
		public static ConditionExpression ParseCondition (string condition)
		{
			ConditionParser parser = new ConditionParser (condition);
			ConditionExpression e = parser.ParseExpression ();
			
			if (!parser.tokenizer.IsEOF ())
				throw new ExpressionParseException (String.Format ("Unexpected token: {0}", parser.tokenizer.Token.Value));
			
			return e;
		}
		
		private ConditionExpression ParseExpression ()
		{
			return ParseBooleanExpression ();
		}
		
		private ConditionExpression ParseBooleanExpression ()
		{
			return ParseBooleanAnd ();
		}
		
		private ConditionExpression ParseBooleanAnd ()
		{
			ConditionExpression e = ParseBooleanOr ();
			
			while (tokenizer.IsToken (TokenType.And)) {
				tokenizer.GetNextToken ();
				e = new ConditionAndExpression ((ConditionExpression) e, (ConditionExpression) ParseBooleanOr ());
			}
			
			return e;
		}
		
		private ConditionExpression ParseBooleanOr ()
		{
			ConditionExpression e = ParseRelationalExpression ();
			
			while (tokenizer.IsToken (TokenType.Or)) {
				tokenizer.GetNextToken ();
				e = new ConditionOrExpression ((ConditionExpression) e, (ConditionExpression) ParseRelationalExpression ());
			}
			
			return e;
		}
		
		private ConditionExpression ParseRelationalExpression ()
		{
			return null;
		}
	}
}

#endif
