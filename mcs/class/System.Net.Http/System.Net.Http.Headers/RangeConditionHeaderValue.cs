//
// RangeConditionHeaderValue.cs
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
	public class RangeConditionHeaderValue : ICloneable
	{
		public RangeConditionHeaderValue (DateTimeOffset date)
		{
			Date = date;
		}

		public RangeConditionHeaderValue (EntityTagHeaderValue entityTag)
		{
			if (entityTag == null)
				throw new ArgumentNullException ("entityTag");

			EntityTag = entityTag;
		}

		public RangeConditionHeaderValue (string entityTag)
			: this (new EntityTagHeaderValue (entityTag))
		{
		}

		public DateTimeOffset? Date { get; private set; }
		public EntityTagHeaderValue EntityTag { get; private set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as RangeConditionHeaderValue;
			if (source == null)
				return false;

			return EntityTag != null ?
				EntityTag.Equals (source.EntityTag) :
				Date == source.Date;
		}

		public override int GetHashCode ()
		{
			return EntityTag != null ? EntityTag.GetHashCode () : Date.GetHashCode ();
		}

		public static RangeConditionHeaderValue Parse (string input)
		{
			RangeConditionHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out RangeConditionHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			bool is_weak;

			if (t == Token.Type.Token) {
				if (lexer.GetStringValue (t) != "W") {
					DateTimeOffset date;
					if (!Lexer.TryGetDateValue (input, out date))
						return false;

					parsedValue = new RangeConditionHeaderValue (date);
					return true;
				}

				if (lexer.PeekChar () != '/')
					return false;

				is_weak = true;
				lexer.EatChar ();
				t = lexer.Scan ();
			} else {
				is_weak = false;
			}

			if (t != Token.Type.QuotedString)
				return false;

			if (lexer.Scan () != Token.Type.End)
				return false;

			parsedValue = new RangeConditionHeaderValue (
				new EntityTagHeaderValue () {
					Tag = lexer.GetStringValue (t),
					IsWeak = is_weak
				});

			return true;
		}

		public override string ToString ()
		{
			if (EntityTag != null)
				return EntityTag.ToString ();

			return Date.Value.ToString ("r", CultureInfo.InvariantCulture);
		}
	}
}
