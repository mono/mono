//
// WarningHeaderValue.cs
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
	public class WarningHeaderValue : ICloneable
	{
		public WarningHeaderValue (int code, string agent, string text)
		{
			if (!IsCodeValid (code))
				throw new ArgumentOutOfRangeException ("code");

			Parser.Uri.Check (agent);
			Parser.Token.CheckQuotedString (text);

			Code = code;
			Agent = agent;
			Text = text;
		}

		public WarningHeaderValue (int code, string agent, string text, DateTimeOffset date)
			: this (code, agent, text)
		{
			Date = date;
		}

		private WarningHeaderValue ()
		{
		}

		public string Agent { get; private set; }
		public int Code { get; private set; }
		public DateTimeOffset? Date { get; private set; }
		public string Text { get; private set; }

		static bool IsCodeValid (int code)
		{
			return code >= 0 && code < 1000;
		}

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as WarningHeaderValue;
			if (source == null)
				return false;

			return Code == source.Code &&
				string.Equals (source.Agent, Agent, StringComparison.OrdinalIgnoreCase) &&
				Text == source.Text &&
				Date == source.Date;
		}

		public override int GetHashCode ()
		{
			int hc = Code.GetHashCode ();
			hc ^= Agent.ToLowerInvariant ().GetHashCode ();
			hc ^= Text.GetHashCode ();
			hc ^= Date.GetHashCode ();

			return hc;
		}

		public static WarningHeaderValue Parse (string input)
		{
			WarningHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}
		
		public static bool TryParse (string input, out WarningHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();

			if (t != Token.Type.Token)
				return false;

			int code;
			if (!lexer.TryGetNumericValue (t, out code) || !IsCodeValid (code))
				return false;

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var next = t;
			if (lexer.PeekChar () == ':') {
				lexer.EatChar ();

				next = lexer.Scan ();
				if (next != Token.Type.Token)
					return false;
			}

			var value = new WarningHeaderValue ();
			value.Code = code;
			value.Agent = lexer.GetStringValue (t, next);

			t = lexer.Scan ();
			if (t != Token.Type.QuotedString)
				return false;

			value.Text = lexer.GetStringValue (t);

			t = lexer.Scan ();
			if (t == Token.Type.QuotedString) {
				DateTimeOffset date;
				if (!lexer.TryGetDateValue (t, out date))
					return false;

				value.Date = date;
				t = lexer.Scan ();
			}

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			string s = Code.ToString ("000") + " " + Agent + " " + Text;
			if (Date.HasValue)
				s = s + " \"" + Date.Value.ToString ("r", CultureInfo.InvariantCulture) + "\"";

			return s;
		}
	}
}
