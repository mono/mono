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

namespace Microsoft.JScript {

	public class MathObject : JSObject {

		public const double E = 2.7182818284590452354;
		public const double LN10 = 2.302585092994046;
		public const double LN2 = 0.6931471805599453;
		public const double LOG2E = 1.4426950408889634;
		public const double LOG10E = 0.4342944819032518;
		public const double PI = 3.14159265358979323846;
		public const double SQRT1_2 = 0.7071067811865476;
		public const double SQRT2 = 1.4142135623730951;

		internal MathObject ()
		{
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_abs)]
		public static double abs (double d)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_acos)]
		public static double acos (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_asin)]
		public static double asin (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_atan)]
		public static double atan (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_atan2)]
		public static double atan2 (double dy, double dx)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_ceil)]
		public static double ceil (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_cos)]
		public static double cos (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_exp)]
		public static double exp (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_floor)]
		public static double floor (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_log)]
		public static double log (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_max)]
		public static double max (Object x, Object y, params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_min)]
		public static double min (Object x, Object y, params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_pow)]
		public static double pow (double dx, double dy)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_random)]
		public static double random ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_round)]
		public static double round (double d)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_sin)]
		public static double sin (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_sqrt)]
		public static double sqrt (double x)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Math_tan)]
		public static double tan (double x)
		{
			throw new NotImplementedException ();
		}
	}
}
