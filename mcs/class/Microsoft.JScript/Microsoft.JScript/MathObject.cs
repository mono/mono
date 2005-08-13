//
// MathObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Collections;

namespace Microsoft.JScript {

	public class MathObject : JSObject {

		internal static Random random_gen = new Random ();
		
		public const double E = 2.7182818284590452354;
		public const double LN10 = 2.302585092994046;
		public const double LN2 = 0.6931471805599453;
		public const double LOG2E = 1.4426950408889634;
		public const double LOG10E = 0.4342944819032518;
		public const double PI = 3.14159265358979323846;
		public const double SQRT1_2 = 0.7071067811865476;
		public const double SQRT2 = 1.4142135623730951;

		internal static MathObject Object = new MathObject ();

		internal MathObject ()
		{
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_abs)]
		public static double abs (double d)
		{
			return Math.Abs (d);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_acos)]
		public static double acos (double x)
		{
			return Math.Acos (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_asin)]
		public static double asin (double x)
		{
			return Math.Asin (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_atan)]
		public static double atan (double x)
		{
			return Math.Atan (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_atan2)]
		public static double atan2 (double dy, double dx)
		{
			return Math.Atan2 (dy, dx);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_ceil)]
		public static double ceil (double x)
		{
			return Math.Ceiling (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_cos)]
		public static double cos (double x)
		{
			return Math.Cos (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_exp)]
		public static double exp (double x)
		{
			return Math.Exp (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_floor)]
		public static double floor (double x)
		{
			return Math.Floor (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_log)]
		public static double log (double x)
		{
			return Math.Log (x);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_max)]
		public static double max (Object x, Object y, params Object [] args)
		{
			ArrayList values = new ArrayList (args);

			if (x != null)
				values.Add (x);
			if (y != null)
				values.Add (y);

			double val;
			double result = Double.NegativeInfinity;

			foreach (object value in values) {
				val = Convert.ToNumber (value);
				if (Double.IsNaN (val))
					return Double.NaN;
				else if (val > result)
					result = val;
			}
			return result;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_min)]
		public static double min (Object x, Object y, params Object [] args)
		{
			ArrayList values = new ArrayList (args);

			if (x != null)
				values.Add (x);
			if (y != null)
				values.Add (y);

			double val;
			double result = Double.PositiveInfinity;

			foreach (object value in values) {
				val = Convert.ToNumber (value);
				if (Double.IsNaN (val))
					return Double.NaN;
				else if (val < result)
					result = val;
			}
			return result;
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_pow)]
		public static double pow (double dx, double dy)
		{
			return Math.Pow (dx, dy);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_random)]
		public static double random ()
		{
			return random_gen.Next (1);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_round)]
		public static double round (double d)
		{
			return Math.Round (d);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_sin)]
		public static double sin (double x)
		{
			return Math.Sin (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_sqrt)]
		public static double sqrt (double x)
		{
			return Math.Sqrt (x);
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_tan)]
		public static double tan (double x)
		{
			return Math.Tan (x);
		}
	}
}
