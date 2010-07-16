//
// Based on the C implementation from the RFC:
// http://tools.ietf.org/html/rfc3492
//
// See the following url for documentation:
//     http://www.mono-project.com/Mono_DataConvert
//
// Compilation Options:
//     MONO_PUNYCODE_PUBLIC:
//         Makes the class public instead of the default internal.
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
// 
using System;
using System.Text;

namespace Mono.Util {
#if MONO_PUNYCODE_PUBLIC
public
#else
internal
#endif
	static class Punycode {
		const int BASE = 36;
		const int DAMP = 700;
		const int TMIN = 1;
		const int TMAX = 26;
		const int SKEW = 38;
		
		//  0..25 map to ASCII a..z or A..Z 
		// 26..35 map to ASCII 0..9         
		static char encode_digit (int d)
		{
			return (char) (d + 22 + (d < 26 ? 75 : 0));
		}
	
		/* Bias adaptation function */
		static int adapt (int delta, int numpoints, bool firsttime)
		{
			int k;
	
			delta = firsttime ? delta / DAMP : delta >> 1;
			/* delta >> 1 is a faster way of doing delta / 2 */
			delta += delta / numpoints;
	
			for (k = 0;  delta > ((BASE - TMIN) * TMAX) / 2;  k += BASE) 
				delta /= BASE - TMIN;
	
			return k + (BASE - TMIN + 1) * delta / (delta + SKEW);
		}
		
		public static string Encode (string input)
		{
			var result = new StringBuilder ();
	
			foreach (char c in input){
				if (c < 0x80)
					result.Append (c);
			}
			int h, b;
			h = b = result.Length;
			
			if (h > 0)
				result.Append ('-');
	
			int n = 128;
			int delta = 0;
			int bias = 72;
	
			while (h < input.Length) {
				int m = Int32.MaxValue;
	
				// Find the next code point
				foreach (char c in input){
					int ic = (int) c;
					if (ic > n && ic < m)
						m = ic;
				}
				
				if (m - n > (Int32.MaxValue - delta) / (h + 1))
					throw new OverflowException ();
				
				delta += (m - n) * (h + 1);
				n = m;
	
				foreach (char c in input){
					int ic = (int) c;
	
					// Punycode does not need to check whether input[j] is basic
					if (ic < n)
						if (++delta == 0)
							throw new OverflowException ();
	
					/* Represent delta as a generalized variable-length integer: */
					if (ic == n) {
						int q = delta;
						for (int k = BASE;  ;  k += BASE) {
							int t = k <= bias ? TMIN : k >= bias + TMAX ? TMAX : k - bias;
							if (q < t)
								break;
							result.Append (encode_digit (t + (q - t) % (BASE - t)));
							q = (q - t) / (BASE - t);
						}
	
						result.Append (encode_digit (q));
						bias = adapt (delta, h + 1, h == b);
						delta = 0;
						++h;
					}
				}
	
				++delta;
				++n;
			}
			return result.ToString ();
		}
	}
}