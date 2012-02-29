//
// RangeHeaderValue.cs
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
using System.Text;

namespace System.Net.Http.Headers
{
	public class RangeHeaderValue : ICloneable
	{
		List<RangeItemHeaderValue> ranges;
		string unit;

		public RangeHeaderValue ()
		{
			unit = "bytes";
		}

		public RangeHeaderValue (long? from, long? to)
			: this ()
		{
			Ranges.Add (new RangeItemHeaderValue (from, to));
		}

		private RangeHeaderValue (RangeHeaderValue source)
			: this ()
		{
			if (source.ranges != null) {
				foreach (var item in source.ranges)
					Ranges.Add (item);
			}
		}

		public ICollection<RangeItemHeaderValue> Ranges {
			get {
				return ranges ?? (ranges = new List<RangeItemHeaderValue> ());
			}
		}

		public string Unit {
			get {
				return unit;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Unit");

				Parser.Token.Check (value);

				unit = value;
			}
		}

		object ICloneable.Clone ()
		{
			return new RangeHeaderValue (this);
		}

		public override bool Equals (object obj)
		{
			var source = obj as RangeHeaderValue;
			if (source == null)
				return false;

			return string.Equals (source.Unit, Unit, StringComparison.OrdinalIgnoreCase) &&
				source.ranges.SequenceEqual (ranges);
		}

		public override int GetHashCode ()
		{
			return Unit.ToLowerInvariant ().GetHashCode () ^ HashCodeCalculator.Calculate (ranges);
		}

		public static RangeHeaderValue Parse (string input)
		{
			RangeHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out RangeHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var value = new RangeHeaderValue ();
			value.unit = lexer.GetStringValue (t);

			t = lexer.Scan ();
			if (t != Token.Type.SeparatorEqual)
				return false;

			bool token_read;
			do {
				int? from = null, to = null;
				int number;
				token_read = false;

				t = lexer.Scan ();
				switch (t.Kind) {
				case Token.Type.SeparatorDash:
					t = lexer.Scan ();
					if (!lexer.TryGetNumericValue (t, out number))
						return false;

					to = number;
					break;
				case Token.Type.Token:
					string s = lexer.GetStringValue (t);
					var values = s.Split (new [] { '-' }, StringSplitOptions.RemoveEmptyEntries);
					if (!int.TryParse (values[0], out number))
						return false;

					switch (values.Length) {
					case 1:
						t = lexer.Scan ();
						switch (t.Kind) {
						case Token.Type.SeparatorDash:
							from = number;

							t = lexer.Scan ();
							if (t != Token.Type.Token) {
								token_read = true;
								break;
							}

							if (!lexer.TryGetNumericValue (t, out number))
								return false;

							to = number;
							if (to < from)
								return false;

							break;
						default:
							return false;
						}
						break;
					case 2:
						from = number;

						if (!int.TryParse (values[1], out number))
							return false;

						to = number;
						if (to < from)
							return false;

						break;
					default:
						return false;
					}

					break;
				default:
					return false;
				}

				value.Ranges.Add (new RangeItemHeaderValue (from, to));
				if (!token_read)
					t = lexer.Scan ();

			} while (t == Token.Type.SeparatorComma);

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			var sb = new StringBuilder (unit);
			sb.Append ("=");
			for (int i = 0; i < Ranges.Count; ++i) {
				if (i > 0)
					sb.Append (", ");

				sb.Append (ranges[i]);
			}

			return sb.ToString ();
		}
	}
}
