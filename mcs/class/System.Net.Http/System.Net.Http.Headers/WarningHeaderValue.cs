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

namespace System.Net.Http.Headers
{
	public class WarningHeaderValue : ICloneable
	{
		public WarningHeaderValue (int code, string agent, string text)
		{
			Code = code;
			Agent = agent;
			Text = text;
		}

		public WarningHeaderValue (int code, string agent, string text, DateTimeOffset date)
			: this (code, agent, text)
		{
			Date = date;
		}

		public string Agent { get; private set; }
		public int Code { get; private set; }
		public DateTimeOffset? Date { get; private set; }
		public string Text { get; private set; }

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
			throw new NotImplementedException ();
		}
	}
}
