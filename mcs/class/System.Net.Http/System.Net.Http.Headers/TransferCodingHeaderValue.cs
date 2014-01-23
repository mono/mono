//
// TransferCodingHeaderValue.cs
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
	public class TransferCodingHeaderValue : ICloneable
	{
		internal string value;
		internal List<NameValueHeaderValue> parameters;

		public TransferCodingHeaderValue (string value)
		{
			Parser.Token.Check (value);
			this.value = value;
		}

		protected TransferCodingHeaderValue (TransferCodingHeaderValue source)
		{
			this.value = source.value;
			if (source.parameters != null) {
				foreach (var p in source.parameters) {
					Parameters.Add (new NameValueHeaderValue (p));
				}
			}
		}

		internal TransferCodingHeaderValue ()
		{
		}

		public ICollection<NameValueHeaderValue> Parameters {
			get {
				return parameters ?? (parameters = new List<NameValueHeaderValue> ());
			}
		}

		public string Value {
			get {
				return value;
			}
		}

		object ICloneable.Clone ()
		{
			return new TransferCodingHeaderValue (this);
		}

		public override bool Equals (object obj)
		{
			var fchv = obj as TransferCodingHeaderValue;
			return fchv != null &&
				string.Equals (value, fchv.value, StringComparison.OrdinalIgnoreCase) &&
				parameters.SequenceEqual (fchv.parameters);
		}

		public override int GetHashCode ()
		{
			var hc = value.ToLowerInvariant ().GetHashCode ();
			if (parameters != null)
				hc ^= HashCodeCalculator.Calculate (parameters);

			return hc;
		}

		public static TransferCodingHeaderValue Parse (string input)
		{
			TransferCodingHeaderValue value;

			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public override string ToString ()
		{
			return value + CollectionExtensions.ToString (parameters);
		}

		public static bool TryParse (string input, out TransferCodingHeaderValue parsedValue)
		{
			var lexer = new Lexer (input);
			Token token;
			if (TryParseElement (lexer, out parsedValue, out token) && token == Token.Type.End)
				return true;

			parsedValue = null;
			return false;
		}

		internal static bool TryParse (string input, int minimalCount, out List<TransferCodingHeaderValue> result)
		{
			return CollectionParser.TryParse (input, minimalCount, TryParseElement, out result);
		}	

		static bool TryParseElement (Lexer lexer, out TransferCodingHeaderValue parsedValue, out Token t)
		{
			parsedValue = null;

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var result = new TransferCodingHeaderValue ();
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
