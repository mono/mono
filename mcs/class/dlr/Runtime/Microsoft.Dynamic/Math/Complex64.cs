/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting.Utils;

#if FEATURE_NUMERICS
using BigInt = System.Numerics.BigInteger;
#endif

namespace Microsoft.Scripting.Math {

#if !MONO_INTERPRETER
    /// <summary>
    /// Implementation of the complex number data type.
    /// </summary>
    [Serializable]
    public struct Complex64 {
        public static readonly Complex64 Zero = new Complex64(0.0, 0.0);
        public static readonly Complex64 One = new Complex64(1.0, 0.0);
        public static readonly Complex64 ImaginaryOne = new Complex64(0.0, 1.0);

        private readonly double real, imag;

        public static Complex64 MakeImaginary(double imag) {
            return new Complex64(0.0, imag);
        }

        public static Complex64 MakeReal(double real) {
            return new Complex64(real, 0.0);
        }

        public static Complex64 Make(double real, double imag) {
            return new Complex64(real, imag);
        }

        public Complex64(double real)
            : this(real, 0.0) {
        }

        public Complex64(double real, double imag) {
            this.real = real;
            this.imag = imag;
        }

        public bool IsZero {
            get {
                return real == 0.0 && imag == 0.0;
            }
        }

        public double Real {
            get {
                return real;
            }
        }

        public double Imag {
            get {
                return imag;
            }
        }

        public Complex64 Conjugate() {
            return new Complex64(real, -imag);
        }


        public override string ToString() {
            if (real == 0.0) return imag.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "j";
            else if (imag < 0.0) return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "({0}{1}j)", real, imag);
            else return string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "({0}+{1}j)", real, imag);
        }

        public static implicit operator Complex64(bool b) {
            return b ? One : Zero;
        }

        public static implicit operator Complex64(int i) {
            return MakeReal(i);
        }

        [CLSCompliant(false)]
        public static implicit operator Complex64(uint i) {
            return MakeReal(i);
        }

        public static implicit operator Complex64(short i) {
            return MakeReal(i);
        }
        
        [CLSCompliant(false)]
        public static implicit operator Complex64(ushort i) {
            return MakeReal(i);
        }

        public static implicit operator Complex64(long l) {
            return MakeReal(l);
        }
        [CLSCompliant(false)]
        public static implicit operator Complex64(ulong i) {
            return MakeReal(i);
        }

        [CLSCompliant(false)]
        public static implicit operator Complex64(sbyte i) {
            return MakeReal(i);
        }

        public static implicit operator Complex64(byte i) {
            return MakeReal(i);
        }

        public static implicit operator Complex64(float f) {
            return MakeReal(f);
        }

        public static implicit operator Complex64(double d) {
            return MakeReal(d);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public static implicit operator Complex64(BigInteger i) {
            ContractUtils.RequiresNotNull(i, "i");

            // throws an overflow exception if we can't handle the value.
            return MakeReal((double)i);
        }

#if FEATURE_NUMERICS
        public static implicit operator Complex64(BigInt i) {
            // throws an overflow exception if we can't handle the value.
            return MakeReal((double)i);
        }
#endif

        public static bool operator ==(Complex64 x, Complex64 y) {
            return x.real == y.real && x.imag == y.imag;
        }

        public static bool operator !=(Complex64 x, Complex64 y) {
            return x.real != y.real || x.imag != y.imag;
        }

        public static Complex64 Add(Complex64 x, Complex64 y) {
            return x + y;
        }

        public static Complex64 operator +(Complex64 x, Complex64 y) {
            return new Complex64(x.real + y.real, x.imag + y.imag);
        }

        public static Complex64 Subtract(Complex64 x, Complex64 y) {
            return x - y;
        }

        public static Complex64 operator -(Complex64 x, Complex64 y) {
            return new Complex64(x.real - y.real, x.imag - y.imag);
        }

        public static Complex64 Multiply(Complex64 x, Complex64 y) {
            return x * y;
        }

        public static Complex64 operator *(Complex64 x, Complex64 y) {
            return new Complex64(x.real * y.real - x.imag * y.imag, x.real * y.imag + x.imag * y.real);
        }

        public static Complex64 Divide(Complex64 x, Complex64 y) {
            return x / y;
        }

        public static Complex64 operator /(Complex64 a, Complex64 b) {
            if (b.IsZero) {
                throw new DivideByZeroException("complex division by zero");
            }

            double real, imag, den, r;

            if (System.Math.Abs(b.real) >= System.Math.Abs(b.imag)) {
                r = b.imag / b.real;
                den = b.real + r * b.imag;
                real = (a.real + a.imag * r) / den;
                imag = (a.imag - a.real * r) / den;
            } else {
                r = b.real / b.imag;
                den = b.imag + r * b.real;
                real = (a.real * r + a.imag) / den;
                imag = (a.imag * r - a.real) / den;
            }

            return new Complex64(real, imag);
        }

        public static Complex64 Negate(Complex64 x) {
            return -x;
        }

        public static Complex64 operator -(Complex64 x) {
            return new Complex64(-x.real, -x.imag);
        }

        public static Complex64 Plus(Complex64 x) {
            return +x;
        }

        public static Complex64 operator +(Complex64 x) {
            return x;
        }
        
        [Obsolete("Deprecated - consider using MS.Scripting.Utils.MathUtils.Hypot")]
        public static double Hypot(double x, double y) {
            return MathUtils.Hypot(x, y);
        }

        public double Abs() {
            return MathUtils.Hypot(real, imag);
        }

        public Complex64 Power(Complex64 y) {
            double c = y.real;
            double d = y.imag;
            int power = (int)c;

            if (power == c && power >= 0 && d == .0) {
                Complex64 result = One;
                if (power == 0) return result;
                Complex64 factor = this;
                while (power != 0) {
                    if ((power & 1) != 0) {
                        result = result * factor;
                    }
                    factor = factor * factor;
                    power >>= 1;
                }
                return result;
            } else if (IsZero) {
                return y.IsZero ? One : Zero;
            } else {
                double a = real;
                double b = imag;
                double powers = a * a + b * b;
                double arg = System.Math.Atan2(b, a);
                double mul = System.Math.Pow(powers, c / 2) * System.Math.Exp(-d * arg);
                double common = c * arg + .5 * d * System.Math.Log(powers);
                return new Complex64(mul * System.Math.Cos(common), mul * System.Math.Sin(common));
            }
        }

        public override int GetHashCode() {
            // The Object.GetHashCode function needs to be consistent with the Object.Equals function.
            // Languages that build on top of this may have a more flexible equality function and 
            // so may not be able to use this hash function directly.
            // For example, Python allows that c=Complex64(1.5, 0), f = 1.5f,  c==f.
            // so then the hash(f) == hash(c). Since the python (and other languages) can define an arbitrary
            // hash(float) function, the language may need to define a matching hash(complex) function for
            // the cases where the float and complex numbers overlap.
            return (int)real + (int)imag * 1000003;
        }

        public override bool Equals(object obj) {
            if (!(obj is Complex64)) return false;
            return this == ((Complex64)obj);
        }
    }
#endif

}