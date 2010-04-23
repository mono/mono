//
// Complex.cs: Complex number support
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2009, 2010 Novell, Inc.
//
//
//
// TODO:
// string ToString (string format, IFormatProvider)
// string ToString (string format)
// string ToString (IFormatProvider)
//
// Acos, Asin, Atan, Exp, Log, Log10, Pow, Sqrt

using System;

namespace System.Numerics {

	public struct Complex : IEquatable<Complex>
	{
		double real, imaginary;

		public static readonly Complex ImaginaryOne = new Complex (0, 1);
		public static readonly Complex One = new Complex (1, 0);
		public static readonly Complex Zero = new Complex (0, 0);
		
		public Complex (double real, double imaginary)
		{
			this.imaginary = imaginary;
			this.real = real;
		}

		public static Complex FromPolarCoordinates (double magnitude, double phase)
		{
			return new Complex (magnitude * Math.Cos (phase), magnitude * Math.Sin (phase));
		}

		public static Complex operator + (Complex left, Complex right)
		{
			return new Complex (left.real + right.real, left.imaginary + right.imaginary);
		}

		public static Complex Add (Complex left, Complex right)
		{
			return new Complex (left.real + right.real, left.imaginary + right.imaginary);
		}
		
		public static Complex operator - (Complex left, Complex right)
		{
			return new Complex (left.real - right.real, left.imaginary - right.imaginary);
		}

		public static Complex Subtract (Complex left, Complex right)
		{
			return new Complex (left.real - right.real, left.imaginary - right.imaginary);
		}
		
		public static Complex operator * (Complex left, Complex right)
		{
			return new Complex (
				left.real * right.real - left.imaginary * right.imaginary,
				left.real * right.imaginary + left.imaginary * right.real);
		}

		public static Complex Multiply (Complex left, Complex right)
		{
			return new Complex (
				left.real * right.real - left.imaginary * right.imaginary,
				left.real * right.imaginary + left.imaginary * right.real);
		}

		public static Complex operator / (Complex left, Complex right)
		{
			double rsri = right.real * right.real + right.imaginary * right.imaginary;
			return new Complex (
				(left.real * right.real + left.imaginary * right.imaginary) / rsri,

				(left.imaginary * right.real - left.real * right.imaginary) / rsri);
		}

		public static Complex Divide (Complex left, Complex right)
		{
			double rsri = right.real * right.real + right.imaginary * right.imaginary;
			return new Complex (
				(left.real * right.real + left.imaginary * right.imaginary) / rsri,

				(left.imaginary * right.real - left.real * right.imaginary) / rsri);
		}

		public static bool operator == (Complex left, Complex right)
		{
			return left.real == right.real && left.imaginary == right.imaginary;
		}

		public bool Equals (Complex value)
		{
			return real == value.real && imaginary == value.imaginary;
		}

		public override bool Equals (object value)
		{
			if (value == null || !(value is Complex))
				return false;

			Complex r = (Complex) value;
			return real == r.real && imaginary == r.imaginary;
		}
		
		public static bool operator != (Complex left, Complex right)
		{
			return left.real != right.real || left.imaginary != right.imaginary;
		}
		
		public static Complex operator - (Complex value)
		{
			return new Complex (-value.real, -value.imaginary);
		}

		public static implicit operator Complex (byte value)
		{
			return new Complex (value, 0);
		}

		public static implicit operator Complex (double value)
		{
			return new Complex (value, 0);
		}
		
		public static implicit operator Complex (short value)
		{
			return new Complex (value, 0);
		}
		
		public static implicit operator Complex (int value)
		{
			return new Complex (value, 0);
		}
		
		public static implicit operator Complex (long value)
		{
			return new Complex (value, 0);
		}

		[CLSCompliant (false)]
		public static implicit operator Complex (sbyte value)
		{
			return new Complex (value, 0);
		}

		public static implicit operator Complex (float value)
		{
			return new Complex (value, 0);
		}

		[CLSCompliant (false)]
		public static implicit operator Complex (ushort value)
		{
			return new Complex (value, 0);
		}

		[CLSCompliant (false)]
		public static implicit operator Complex (uint value)
		{
			return new Complex (value, 0);
		}

		[CLSCompliant (false)]
		public static implicit operator Complex (ulong value)
		{
			return new Complex (value, 0);
		}

		public static explicit operator Complex (decimal value)
		{
			return new Complex ((double) value, 0);
		}

		public static explicit operator Complex (BigInteger value)
		{
			return new Complex ((double) value, 0);
		}

		public override string ToString ()
		{
			return String.Format ("({0:G}, {1:G})", real, imaginary);
		}

		public static double Abs (Complex value)
		{
			return Math.Sqrt (value.imaginary * value.imaginary + value.real * value.real);
		}
		
		public static Complex Conjugate (Complex value)
		{
			return new Complex (value.real, -value.imaginary);
		}

		public static Complex Cos (Complex value)
		{
			return new Complex (Math.Cos (value.real) * Math.Cosh (value.imaginary),
					    -Math.Sin (value.real)  * Math.Sinh (value.imaginary));
		}

		public static Complex Cosh (Complex value)
		{
			return new Complex (Math.Cosh (value.real) * Math.Cos (value.imaginary),
					    -Math.Sinh (value.real)  * Math.Sin (value.imaginary));
		}
		
		public static Complex Negate (Complex value)
		{
			return -value;
		}

		public static Complex Sin (Complex value)
		{
			return new Complex (Math.Sin (value.real) * Math.Cosh (value.imaginary),
					    Math.Cos (value.real)  * Math.Sinh (value.imaginary));
		}
		
		public static Complex Sinh (Complex value)
		{
			return new Complex (Math.Sinh (value.real) * Math.Cos (value.imaginary),
					    Math.Cosh (value.real)  * Math.Sin (value.imaginary));
		}
		
		public static Complex Reciprocal (Complex value)
		{
			if (value == Zero)
				return value;
				
			return One / value;
		}
		
		public static Complex Tan (Complex value)
		{
			return Sin (value) / Cos (value);
		}
		
		public static Complex Tanh (Complex value)
		{
			return Sinh (value) / Cosh (value);
		}
		
		public override int GetHashCode ()
		{
			return real.GetHashCode () ^ imaginary.GetHashCode ();
		}
		
		public double Imaginary { get { return imaginary; } }
		public double Real { get { return real; } }

		public double Magnitude {
			get {
				return Math.Sqrt (imaginary * imaginary + real * real);
			}
		}

		public double Phase {
			get {
				return Math.Atan (imaginary / real);
			}
		}
	}
}
