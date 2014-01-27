//
// NameValueHeaderValue.cs
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
	public class NameValueHeaderValue : ICloneable
	{
		internal string value;

		public NameValueHeaderValue (string name)
			: this (name, null)
		{
		}

		public NameValueHeaderValue (string name, string value)
		{
			Parser.Token.Check (name);

			this.Name = name;
			this.Value = value;
		}

		protected internal NameValueHeaderValue (NameValueHeaderValue source)
		{
			this.Name = source.Name;
			this.value = source.value;
		}

		internal NameValueHeaderValue ()
		{
		}

		public string Name { get; internal set; }

		public string Value {
			get {
				return value;
			}
			set {
				if (!string.IsNullOrEmpty (value)) {
					var lexer = new Lexer (value);
					var token = lexer.Scan ();
					if (lexer.Scan () != Token.Type.End || !(token == Token.Type.Token || token == Token.Type.QuotedString))
						throw new FormatException ();

					value = lexer.GetStringValue (token);
				}

				this.value = value;
			}
		}

		internal static NameValueHeaderValue Create (string name, string value)
		{
			return new NameValueHeaderValue () {
				Name = name,
				value = value
			};
		}

		object ICloneable.Clone ()
		{
			return new NameValueHeaderValue (this);
		}

		public override int GetHashCode ()
		{
			int hc = Name.ToLowerInvariant ().GetHashCode ();
			if (!string.IsNullOrEmpty (value)) {
				hc ^= value.ToLowerInvariant ().GetHashCode ();
			}

			return hc;
		}

		public override bool Equals (object obj)
		{
			var source = obj as NameValueHeaderValue;
			if (source == null || !string.Equals (source.Name, Name, StringComparison.OrdinalIgnoreCase))
				return false;

			if (string.IsNullOrEmpty (value))
				return string.IsNullOrEmpty (source.value);

			return string.Equals (source.value, value, StringComparison.OrdinalIgnoreCase);
		}

		public static NameValueHeaderValue Parse (string input)
		{
			NameValueHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		internal static bool TryParsePragma (string input, int minimalCount, out List<NameValueHeaderValue> result)
		{
			return CollectionParser.TryParse (input, minimalCount, TryParseElement, out result);
		}

		internal static bool TryParseParameters (Lexer lexer, out List<NameValueHeaderValue> result, out Token t)
		{		
			var list = new List<NameValueHeaderValue> ();
			result = null;

			while (true) {
				var attr = lexer.Scan ();
				if (attr != Token.Type.Token) {
					t = Token.Empty;
					return false;
				}

				string value = null;

				t = lexer.Scan ();
				if (t == Token.Type.SeparatorEqual) {
					t = lexer.Scan ();
					if (t != Token.Type.Token && t != Token.Type.QuotedString)
						return false;

					value = lexer.GetStringValue (t);

					t = lexer.Scan ();
				}

				list.Add (new NameValueHeaderValue () {
					Name = lexer.GetStringValue (attr),
					value = value
				});

				if (t == Token.Type.SeparatorSemicolon)
					continue;

				result = list;
				return true;
			}
		}

		public override string ToString ()
		{
			if (string.IsNullOrEmpty (value))
				return Name;

			return Name + "=" + value;
		}

		public static bool TryParse (string input, out NameValueHeaderValue parsedValue)
		{
			var lexer = new Lexer (input);
			Token token;
			if (TryParseElement (lexer, out parsedValue, out token) && token == Token.Type.End)
				return true;

			parsedValue = null;
			return false;
		}

		static bool TryParseElement (Lexer lexer, out NameValueHeaderValue parsedValue, out Token t)
		{
			parsedValue = null;

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			parsedValue = new NameValueHeaderValue () {
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

			return true;
		}
	}
}
