//
// ContentRangeHeaderValue.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Globalization;

namespace System.Net.Http.Headers
{
	public class ContentRangeHeaderValue : ICloneable
	{
		string unit = "bytes";

		private ContentRangeHeaderValue ()
		{
		}

		public ContentRangeHeaderValue (long length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length");

			this.Length = length;
		}

		public ContentRangeHeaderValue (long from, long to)
		{
			if (from < 0 || from > to)
				throw new ArgumentOutOfRangeException ("from");

			this.From = from;
			this.To = to;
		}

		public ContentRangeHeaderValue (long from, long to, long length)
			: this (from, to)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length");

			if (to > length)
				throw new ArgumentOutOfRangeException ("to");

			this.Length = length;
		}

		public long? From { get; private set; }

		public bool HasLength {
			get {
				return Length != null;
			}
		}

		public bool HasRange {
			get {
				return From != null;
			}
		}

		public long? Length { get; private set; }
		public long? To { get; private set; }

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
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as ContentRangeHeaderValue;
			if (source == null)
				return false;

			return source.Length == Length && source.From == From && source.To == To &&
				string.Equals (source.unit, unit, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode ()
		{
			return Unit.GetHashCode () ^ Length.GetHashCode () ^
				From.GetHashCode () ^ To.GetHashCode () ^
				unit.ToLowerInvariant ().GetHashCode ();
		}

		public static ContentRangeHeaderValue Parse (string input)
		{
			ContentRangeHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out ContentRangeHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var value = new ContentRangeHeaderValue ();
			value.unit = lexer.GetStringValue (t);

			t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			int nvalue;
			if (!lexer.IsStarStringValue (t)) {
				if (!lexer.TryGetNumericValue (t, out nvalue)) {
					var s = lexer.GetStringValue (t);
					if (s.Length < 3)
						return false;

					var sep = s.Split ('-');
					if (sep.Length != 2)
						return false;

					if (!int.TryParse (sep[0], NumberStyles.None, CultureInfo.InvariantCulture, out nvalue))
						return false;

					value.From = nvalue;

					if (!int.TryParse (sep[1], NumberStyles.None, CultureInfo.InvariantCulture, out nvalue))
						return false;

					value.To = nvalue;
				} else {
					value.From = nvalue;

					t = lexer.Scan ();
					if (t != Token.Type.SeparatorDash)
						return false;

					t = lexer.Scan ();

					if (!lexer.TryGetNumericValue (t, out nvalue))
						return false;

					value.To = nvalue;
				}
			}

			t = lexer.Scan ();

			if (t != Token.Type.SeparatorSlash)
				return false;

			t = lexer.Scan ();

			if (!lexer.IsStarStringValue (t)) {
				if (!lexer.TryGetNumericValue (t, out nvalue))
					return false;

				value.Length = nvalue;
			}

			t = lexer.Scan ();

			if (t != Token.Type.End)
				return false;

			parsedValue = value;
 
			return true;
		}

		public override string ToString ()
		{
			var sb = new StringBuilder (unit);
			sb.Append (" ");
			if (From == null) {
				sb.Append ("*");
			} else {
				sb.Append (From.Value.ToString (CultureInfo.InvariantCulture));
				sb.Append ("-");
				sb.Append (To.Value.ToString (CultureInfo.InvariantCulture));
			}

			sb.Append ("/");
			sb.Append (Length == null ? "*" :
				Length.Value.ToString (CultureInfo.InvariantCulture));

			return sb.ToString ();
		}
	}
}
