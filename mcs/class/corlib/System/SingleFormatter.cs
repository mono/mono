//
// System.SingleFormatter.cs
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

	internal class SingleFormatter {

		const double p = 1000000.0d;
		const double p10 = 10000000.0;
		const int dec_len = 6;
		const int dec_len_min = -14;

		const double p2 = 10000000.0d;
		const double p102 = 100000000.0;
		const int dec_len2 = 7;
		const int dec_len_min2 = -16;

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
