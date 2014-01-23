//
// NameValueWithParametersHeaderValue.cs
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
	public class NameValueWithParametersHeaderValue : NameValueHeaderValue, ICloneable
	{
		List<NameValueHeaderValue> parameters;

		public NameValueWithParametersHeaderValue (string name)
			: base (name)
		{
		}

		public NameValueWithParametersHeaderValue (string name, string value)
			: base (name, value)
		{
		}

		protected NameValueWithParametersHeaderValue (NameValueWithParametersHeaderValue source)
			: base (source)
		{
			if (source.parameters != null) {
				foreach (var item in source.parameters)
					Parameters.Add (item);
			}
		}

		private NameValueWithParametersHeaderValue ()
			: base ()
		{
		}

		public ICollection<NameValueHeaderValue> Parameters {
			get {
				return parameters ?? (parameters = new List<NameValueHeaderValue> ());
			}
		}

		object ICloneable.Clone ()
		{
			return new NameValueWithParametersHeaderValue (this);
		}

		public override bool Equals (object obj)
		{
			var source = obj as NameValueWithParametersHeaderValue;
			if (source == null)
				return false;

			return base.Equals (obj) && source.parameters.SequenceEqual (parameters);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode () ^ HashCodeCalculator.Calculate (parameters);
		}

		public static new NameValueWithParametersHeaderValue Parse (string input)
		{
			NameValueWithParametersHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public override string ToString ()
		{
			if (parameters == null || parameters.Count == 0)
				return base.ToString ();

			return base.ToString () + CollectionExtensions.ToString (parameters);
		}

		public static bool TryParse (string input, out NameValueWithParametersHeaderValue parsedValue)
		{
			var lexer = new Lexer (input);
			Token token;
			if (TryParseElement (lexer, out parsedValue, out token) && token == Token.Type.End)
				return true;

			parsedValue = null;
			return false;
		}

		internal static bool TryParse (string input, int minimalCount, out List<NameValueWithParametersHeaderValue> result)
		{
			return CollectionParser.TryParse (input, minimalCount, TryParseElement, out result);
		}

		static bool TryParseElement (Lexer lexer, out NameValueWithParametersHeaderValue parsedValue, out Token t)
		{
			parsedValue = null;

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			parsedValue = new NameValueWithParametersHeaderValue () {
				Name = lexer.GetStringValue (t),
			};

			t = lexer.Scan ();
			if (t == Token.Type.SeparatorEqual) {
				t = lexer.Scan ();

				if (t == Token.Type.Token || t == Token.Type.QuotedString) {
					parsedValue.value = lexer.GetStringValue (t);
					t = lexer.Scan ();
				} else {
					return false;
				}
			}

			if (t == Token.Type.SeparatorSemicolon) {
				List<NameValueHeaderValue> result;
				if (!TryParseParameters (lexer,  out result, out t))
					return false;

				parsedValue.parameters = result;
			}

			return true;
		}
	}
}
