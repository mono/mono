//
// RetryConditionHeaderValue.cs
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
	public class RetryConditionHeaderValue : ICloneable
	{
		public RetryConditionHeaderValue (DateTimeOffset date)
		{
			Date = date;
		}

		public RetryConditionHeaderValue (TimeSpan delta)
		{
			if (delta.TotalSeconds > uint.MaxValue)
				throw new ArgumentOutOfRangeException ("delta");

			Delta = delta;
		}

		public DateTimeOffset? Date { get; private set; }
		public TimeSpan? Delta { get; private set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as RetryConditionHeaderValue;
			return source != null && source.Date == Date && source.Delta == Delta;
		}

		public override int GetHashCode ()
		{
			return Date.GetHashCode () ^ Delta.GetHashCode ();
		}

		public static RetryConditionHeaderValue Parse (string input)
		{
			RetryConditionHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public static bool TryParse (string input, out RetryConditionHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);
			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var ts = lexer.TryGetTimeSpanValue (t);
			if (ts != null) {
				if (lexer.Scan () != Token.Type.End)
					return false;

				parsedValue = new RetryConditionHeaderValue (ts.Value);
			} else {
				DateTimeOffset date;
				if (!Lexer.TryGetDateValue (input, out date))
					return false;

				parsedValue = new RetryConditionHeaderValue (date);
			}

			return true;
		}

		public override string ToString ()
		{
			return Delta != null ?
				Delta.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture) :
				Date.Value.ToString ("r", CultureInfo.InvariantCulture);
		}
	}
}
