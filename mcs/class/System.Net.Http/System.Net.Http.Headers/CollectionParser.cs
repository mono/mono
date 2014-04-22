//
// CollectionParser.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;

namespace System.Net.Http.Headers
{
	delegate bool ElementTryParser<T> (Lexer lexer, out T parsedValue, out Token token);

	static class CollectionParser
	{
		public static bool TryParse<T> (string input, int minimalCount, ElementTryParser<T> parser, out List<T> result) where T : class
		{
			var lexer = new Lexer (input);
			result = new List<T> ();

			while (true) {
				Token token;
				T parsedValue;
				if (!parser (lexer, out parsedValue, out token))
					return false;

				if (parsedValue != null)
					result.Add (parsedValue);

				if (token == Token.Type.SeparatorComma)
					continue;

				if (token == Token.Type.End) {
					if (minimalCount > result.Count) {
						result = null;
						return false;
					}
						
					return true;
				}

				result = null;
				return false;
			}
		}

		public static bool TryParse (string input, int minimalCount, out List<string> result)
		{
			return TryParse (input, minimalCount, TryParseStringElement, out result);
		}

		public static bool TryParseRepetition (string input, int minimalCount, out List<string> result)
		{
			return TryParseRepetition (input, minimalCount, TryParseStringElement, out result);
		}

		static bool TryParseStringElement (Lexer lexer, out string parsedValue, out Token t)
		{
			t = lexer.Scan ();
			if (t == Token.Type.Token) {
				parsedValue = lexer.GetStringValue (t);
				if (parsedValue.Length == 0)
					parsedValue = null;

				t = lexer.Scan ();
			} else {
				parsedValue = null;
			}

			return true;
		}

		public static bool TryParseRepetition<T> (string input, int minimalCount, ElementTryParser<T> parser, out List<T> result) where T : class
		{
			var lexer = new Lexer (input);
			result = new List<T> ();

			while (true) {
				Token token;
				T parsedValue;
				if (!parser (lexer, out parsedValue, out token))
					return false;

				if (parsedValue != null)
					result.Add (parsedValue);

				if (token == Token.Type.End) {
					if (minimalCount > result.Count)
						return false;
						
					return true;
				}
			}
		}
	}
}