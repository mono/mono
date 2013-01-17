//
// QualityValue.cs
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
using System.Collections.Generic;

namespace System.Net.Http.Headers
{
	static class QualityValue
	{
		public static double? GetValue (List<NameValueHeaderValue> parameters)
		{
			if (parameters == null)
				return null;

			var found = parameters.Find (l => string.Equals (l.Name, "q", StringComparison.OrdinalIgnoreCase));
			if (found == null)
				return null;

			double value;
			if (!double.TryParse (found.Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out value))
				return null;

			return value;
		}

		public static void SetValue (ref List<NameValueHeaderValue> parameters, double? value)
		{
			if (value < 0 || value > 1)
				throw new ArgumentOutOfRangeException ("Quality");

			if (parameters == null)
				parameters = new List<NameValueHeaderValue> ();

			parameters.SetValue ("q", value == null ? null : value.Value.ToString (NumberFormatInfo.InvariantInfo));
		}
	}
}
