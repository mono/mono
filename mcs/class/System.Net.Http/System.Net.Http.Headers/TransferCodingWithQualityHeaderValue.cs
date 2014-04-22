//
// TransferCodingWithQualityHeaderValue.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
	public sealed class TransferCodingWithQualityHeaderValue : TransferCodingHeaderValue
	{
		public TransferCodingWithQualityHeaderValue (string value)
			: base (value)
		{
		}

		public TransferCodingWithQualityHeaderValue (string value, double quality)
			: this (value)
		{
			Quality = quality;
		}

		private TransferCodingWithQualityHeaderValue ()
		{
		}

		public double? Quality {
			get {
				return QualityValue.GetValue (parameters);
			}
			set {
				QualityValue.SetValue (ref parameters, value);
			}
		}

		public new static TransferCodingWithQualityHeaderValue Parse (string input)
		{
			TransferCodingWithQualityHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException ();
		}

		public static bool TryParse (string input, out TransferCodingWithQualityHeaderValue parsedValue)
		{
			var lexer = new Lexer (input);
			Token token;
			if (TryParseElement (lexer, out parsedValue, out token) && token == Token.Type.End)
				return true;

			parsedValue = null;
			return false;
		}

		internal static bool TryParse (string input, int minimalCount, out List<TransferCodingWithQualityHeaderValue> result)
		{
			return CollectionParser.TryParse (input, minimalCount, TryParseElement, out result);
		}

		static bool TryParseElement (Lexer lexer, out TransferCodingWithQualityHeaderValue parsedValue, out Token t)
		{
			parsedValue = null;

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var result = new TransferCodingWithQualityHeaderValue ();
			result.value = lexer.GetStringValue (t);

			t = lexer.Scan ();

			// Parameters parsing
			if (t == Token.Type.SeparatorSemicolon && (!NameValueHeaderValue.TryParseParameters (lexer, out result.parameters, out t) || t != Token.Type.End))
				return false;

			parsedValue = result;
			return true;
		}
	}
}
