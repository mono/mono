//
// NumberPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

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

using System;
using System.Text;
using System.Globalization;

namespace Microsoft.JScript {

	public class NumberPrototype : NumberObject {

		internal static NumberPrototype Proto = new NumberPrototype ();

		public static NumberConstructor constructor {
			get { return NumberConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toExponential)]
		public static string toExponential (object thisObj, object fractionDigits)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toFixed)]
		public static string toFixed (object thisObj, double fractionDigits)
		{
			if (!Convert.IsNumber (thisObj))
				throw new JScriptException (JSError.NumberExpected);

			double value = Convert.ToNumber (thisObj);
			int prec = Convert.ToInt32 (fractionDigits);

			if (prec < 0 || prec > 21)
				throw new JScriptException (JSError.PrecisionOutOfRange);

			return value.ToString ("F" + prec, CultureInfo.InvariantCulture);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toLocaleString)]
		public static string toLocaleString (object thisObj)
		{
			if (!Convert.IsNumber (thisObj))
				throw new JScriptException (JSError.NumberExpected);
			else
				return Convert.ToNumber (thisObj).ToString ("N");
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toPrecision)]
		public static string toPrecision (object thisObj, object precision)
		{
			throw new NotImplementedException ();
		}

		internal static char [] Digits = new char [] {
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
			'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
			'u', 'v', 'w', 'x', 'y', 'z'
		};


		//
		// We aren't 100% compatible to MS JS.NET here, because we sometimes produce slightly more
		// of the fractional digits. This shouldn't cause any trouble.
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_toString)]
		public static string toString (object thisObj, object radix)
		{
			if (!Convert.IsNumber (thisObj))
				throw new JScriptException (JSError.NumberExpected);

			double value = Convert.ToNumber (thisObj);

			if (Double.IsNaN (value))
				return "NaN";
			else if (Double.IsPositiveInfinity (value))
				return "Infinity";
			else if (Double.IsNegativeInfinity (value))
				return "-Infinity";
			
			int _radix = 10;
			if (radix != null) {
				_radix = Convert.ToInt32 (radix);
				if (_radix < 2)
					_radix = 10;
				else if (_radix > Digits.Length)
					_radix = 10;
			}
			if (_radix == 10)
				return value.ToString (CultureInfo.InvariantCulture);

			string result = "";
			bool negative = false;
			if (value < 0) {
				negative = true;
				value = Math.Abs (value);
			}

			long whole = (long) value;
			double frac = value - whole;
			long digit;

			while (whole >= 1) {
				whole = Math.DivRem (whole, _radix, out digit);
				result = Digits [digit] + result;
			}

			if (result.Length == 0)
				result = "0";

			int frac_digits = _radix;
			string frac_buf = "";
			bool has_frac = false;

			while (frac != 0 && frac_digits < 50) {
				frac *= _radix;
				digit = (long) frac;
				frac -= digit;

				if (digit == 0)
					frac_buf += "0";
				else {
					if (!has_frac) result += ".";
					result += frac_buf + Digits [digit];
					frac_buf = "";
					has_frac = true;
				}

				frac_digits++;
			}

			if (negative)
				return "-" + result;
			else
				return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Number_valueOf)]
		public static object valueOf (object thisObj)
		{
			if (!Convert.IsNumber (thisObj))
				throw new JScriptException (JSError.NumberExpected);
			else
				return Convert.ToNumber (thisObj);
		}
	}
}
