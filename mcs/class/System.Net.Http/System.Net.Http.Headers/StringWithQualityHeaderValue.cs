//
// StringWithQualityHeaderValue.cs
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

using System.Globalization;
namespace System.Net.Http.Headers
{
	public class StringWithQualityHeaderValue : ICloneable
	{
		public StringWithQualityHeaderValue (string value)
		{
			Parser.Token.Check (value);
			this.Value = value;
		}

		public StringWithQualityHeaderValue (string value, double quality)
			: this (value)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException ("quality");

			Quality = quality;
		}

		private StringWithQualityHeaderValue ()
		{
		}

		public double? Quality { get; private set; }
		public string Value { get; private set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as StringWithQualityHeaderValue;
			return source != null &&
				string.Equals (source.Value, Value, StringComparison.OrdinalIgnoreCase) &&
				source.Quality == Quality;
		}

		public override int GetHashCode ()
		{
			return Value.ToLowerInvariant ().GetHashCode () ^ Quality.GetHashCode ();
		}

		public static StringWithQualityHeaderValue Parse (string input)
		{
			StringWithQualityHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}
		
		public static bool TryParse (string input, out StringWithQualityHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var value = new StringWithQualityHeaderValue ();
			value.Value = lexer.GetStringValue (t);

			t = lexer.Scan ();
			if (t == Token.Type.SeparatorSemicolon) {
				t = lexer.Scan ();
				if (t != Token.Type.Token)
					return false;

				var s = lexer.GetStringValue (t);
				if (s != "q" && s != "Q")
					return false;

				t = lexer.Scan ();
				if (t != Token.Type.SeparatorEqual)
					return false;

				t = lexer.Scan ();

				double d;
				if (!lexer.TryGetDoubleValue (t, out d))
					return false;

				if (d > 1)
					return false;

				value.Quality = d;

				t = lexer.Scan ();
			}

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			if (Quality != null)
				return Value + "; q=" + Quality.Value.ToString ("0.0##", CultureInfo.InvariantCulture);

			return Value;
		}
	}
}
