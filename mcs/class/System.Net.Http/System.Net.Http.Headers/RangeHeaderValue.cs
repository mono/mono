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
using System.Linq;

namespace System.Net.Http.Headers
{
	public class RangeHeaderValue : ICloneable
	{
		List<RangeItemHeaderValue> ranges;

		public RangeHeaderValue ()
		{
			Unit = "bytes";
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

		public string Unit { get; set; }

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
				Enumerable.SequenceEqual (source.ranges, ranges);
		}

		public override int GetHashCode ()
		{
			return Unit.GetHashCode () ^ HashCodeCalculator.Calculate (ranges);
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
			throw new NotImplementedException ();
		}
	}
}
