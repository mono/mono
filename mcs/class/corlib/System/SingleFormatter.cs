//
// System.SingleFormatter.cs
//
// Author:
//   Pedro Martinez Juliá (yoros@wanadoo.es)
//
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System {

	internal class SingleFormatter {

		private static FloatingPointFormatter fpf;

		const double p = 1000000.0d;
		const double p10 = 10000000.0;
		const int dec_len = 6;
		const int dec_len_min = -14;

		const double p2 = 10000000.0d;
		const double p102 = 100000000.0;
		const int dec_len2 = 7;
		const int dec_len_min2 = -16;

		public static string NumberToString (string format, NumberFormatInfo nfi, float value)
		{
			if (fpf == null) {
				fpf = new FloatingPointFormatter (p, p10, dec_len, dec_len_min, p2, p102, dec_len2, dec_len_min2);
			}
			return fpf.GetStringFrom (format, nfi, value);
		}
	}
}
