//
// LenientMathobject.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	public sealed class LenientMathobject : MathObject
	{
		public new const double E = 2.7182818284590452354;
		public new const double LN10 = 2.302585092994046;
		public new const double LN2 = 0.6931471805599453;
		public new const double LOG2E = 1.4426950408889634;
		public new const double LOG10E = 0.4342944819032518;
		public new const double PI = 3.14159265358979323846;
		public new const double SQRT1_2 = 0.7071067811865476;
		public new const double SQRT2 = 1.4142135623730951;

		public new object abs;
		public new object acos;
		public new object asin;
		public new object atan;
		public new object atan2;
		public new object ceil;
		public new object cos;
		public new object exp;
		public new object floor;
		public new object log;
		public new object max;
		public new object min;
		public new object pow;
		public new object random;
		public new object round;
		public new object sin;
		public new object sqrt;
		public new object tan;
	}
}
