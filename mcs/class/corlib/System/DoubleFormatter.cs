//
// System.DoubleFormatter.cs
//
// Author:
//   Pedro Martinez Juliá  <yoros@wanadoo.es>
//
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
//

using System;
using System.Text;
using System.Collections;
using System.Globalization;


namespace System {

	internal class DoubleFormatter {

		const double p = 100000000000000.0d;
		const double p10 = 1000000000000000.0;
		const int dec_len = 14;
		const int dec_len_min = -30;

		const double p2 = 10000000000000000.0d;
		const double p102 = 100000000000000000.0;
		const int dec_len2 = 16;
		const int dec_len_min2 = -34;

		public static string NumberToString (string format,
				NumberFormatInfo nfi, double value) {
			FloatingPointFormatter fpf = new FloatingPointFormatter
				(format, nfi, value,
				p, p10, dec_len, dec_len_min,
				p2, p102, dec_len2, dec_len_min2);
			return fpf.String;
		}
		
	}

}
