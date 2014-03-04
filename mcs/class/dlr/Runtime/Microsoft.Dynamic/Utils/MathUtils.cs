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

#if FEATURE_NUMERICS
using BigInt = System.Numerics.BigInteger;
using Complex = System.Numerics.Complex;
#endif

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Utils {
    using Math = System.Math;

    public static class MathUtils {
        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int32.MinValue, y = -1)</remarks>
        public static int FloorDivideUnchecked(int x, int y) {
            int q = x / y;

            if (x >= 0) {
                if (y > 0) {
                    return q;
                } else if (x % y == 0) {
                    return q;
                } else {
                    return q - 1;
                }
            } else {
                if (y > 0) {
                    if (x % y == 0) {
                        return q;
                    } else {
                        return q - 1;
                    }
                } else {
                    return q;
                }
            }
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int64.MinValue, y = -1)</remarks>
        public static long FloorDivideUnchecked(long x, long y) {
            long q = x / y;

            if (x >= 0) {
                if (y > 0) {
                    return q;
                } else if (x % y == 0) {
                    return q;
                } else {
                    return q - 1;
                }
            } else {
                if (y > 0) {
                    if (x % y == 0) {
                        return q;
                    } else {
                        return q - 1;
                    }
                } else {
                    return q;
                }
            }
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        public static int FloorRemainder(int x, int y) {
            if (y == -1) return 0;
            int r = x % y;

            if (x >= 0) {
                if (y > 0) {
                    return r;
                } else if (r == 0) {
                    return 0;
                } else {
                    return r + y;
                }
            } else {
                if (y > 0) {
                    if (r == 0) {
                        return 0;
                    } else {
                        return r + y;
                    }
                } else {
                    return r;
                }
            }
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        public static long FloorRemainder(long x, long y) {
            if (y == -1) return 0;
            long r = x % y;

            if (x >= 0) {
                if (y > 0) {
                    return r;
                } else if (r == 0) {
                    return 0;
                } else {
                    return r + y;
                }
            } else {
                if (y > 0) {
                    if (r == 0) {
                        return 0;
                    } else {
                        return r + y;
                    }
                } else {
                    return r;
                }
            }
        }

        /// <summary>
        /// Behaves like Math.Round(value, MidpointRounding.AwayFromZero)
        /// Needed because CoreCLR doesn't support this particular overload of Math.Round
        /// </summary>
        public static double RoundAwayFromZero(double value) {
#if !SILVERLIGHT && !WP75
            return Math.Round(value, MidpointRounding.AwayFromZero);
#else
            if (value < 0) {
                return -RoundAwayFromZero(-value);
            }
        
            // we can assume positive value
            double result = Math.Floor(value);
            if (value - result >= 0.5) {
                result += 1.0;
            }
            return result;
#endif
        }

        private static readonly double[] _RoundPowersOfTens = new double[] { 1E0, 1E1, 1E2, 1E3, 1E4, 1E5, 1E6, 1E7, 1E8, 1E9, 1E10, 1E11, 1E12, 1E13, 1E14, 1E15 };

        private static double GetPowerOf10(int precision) {
            return (precision < 16) ? _RoundPowersOfTens[precision] : Math.Pow(10, precision);
        }

        /// <summary>
        /// Behaves like Math.Round(value, precision, MidpointRounding.AwayFromZero)
        /// However, it works correctly on negative precisions and cases where precision is
        /// outside of the [-15, 15] range.
        /// 
        /// (This function is also needed because CoreCLR lacks this overload.)
        /// </summary>
        public static double RoundAwayFromZero(double value, int precision) {
            if (double.IsInfinity(value) || double.IsNaN(value)) {
                return value;
            }

            if (precision >= 0) {
                if (precision > 308) {
                    return value;
                }

                double num = GetPowerOf10(precision);
                return RoundAwayFromZero(value * num) / num;
            } else if (precision >= -308) {
                // Note: this code path could be merged with the precision >= 0 path,
                // (by extending the cache to negative powers of 10)
                // but the results seem to be more precise if we do it this way
                double num = GetPowerOf10(-precision);
                return RoundAwayFromZero(value / num) * num;
            } else {
                // Preserve the sign of the input, including +/-0.0
                return value < 0.0 || 1.0 / value < 0.0 ? -0.0 : 0.0;
            }
        }

        public static bool IsNegativeZero(double self) {
#if SILVERLIGHT // BitConverter.DoubleToInt64Bits
            if ( self != 0.0 ) {
              return false;
            }
            byte[] bits = BitConverter.GetBytes(self);
            return (bits[7] == 0x80 && bits[6] == 0x00 && bits[5] == 0x00 && bits[4] == 0x00
                && bits[3] == 0x00 && bits[2] == 0x00 && bits[1] == 0x00 && bits[0] == 0x00);
#else
            return (self == 0.0 && 1.0 / self < 0);
#endif
        }

        #region Special Functions

        public static double Erf(double v0) {
            // Calculate the error function using the approximation method outlined in
            // W. J. Cody's "Rational Chebyshev Approximations for the Error Function"

            if (v0 >= 10.0) {
                return 1.0;
            } else if (v0 <= -10.0) {
                return -1.0;
            }

            if (v0 > 0.47 || v0 < -0.47) {
                return 1.0 - ErfComplement(v0);
            }

            double sq = v0 * v0;
            double numer = EvalPolynomial(sq, ErfNumerCoeffs);
            double denom = EvalPolynomial(sq, ErfDenomCoeffs);

            return v0 * numer / denom;
        }

        public static double ErfComplement(double v0) {
            if (v0 >= 30.0) {
                return 0.0;
            } else if (v0 <= -10.0) {
                return 2.0;
            }

            double a = Math.Abs(v0);
            if (a < 0.47) {
                return 1.0 - Erf(v0);
            }

            // Different approximations are required for different ranges of v0
            double res;
            if (a <= 4.0) {
                // Use the approximation method outlined in W. J. Cody's "Rational Chebyshev
                // Approximations for the Error Function"
                double numer = EvalPolynomial(a, ErfcNumerCoeffs);
                double denom = EvalPolynomial(a, ErfcDenomCoeffs);

                res = Math.Exp(-a * a) * numer / denom;
            } else {
                // Use the approximation method introduced by C. Tellambura and A. Annamalai
                // in "Efficient Computation of erfc(x) for Large Arguments"
                const double h = 0.5;
                const double hSquared = 0.25;
                const int nTerms = 10;
                double sq = a * a;
                res = 0.0;
                for (int i = nTerms; i > 0; i--) {
                    double term = i * i * hSquared;
                    res += Math.Exp(-term) / (term + sq);
                }

                res = h * a * Math.Exp(-sq) / Math.PI * (res * 2 + 1.0 / sq);
            }

            if (v0 < 0.0) {
                res = 2.0 - res;
            }
            return res;
        }

        public static double Gamma(double v0) {
            // Calculate the Gamma function using the Lanczos approximation

            if (double.IsNegativeInfinity(v0)) {
                return double.NaN;
            }
            double a = Math.Abs(v0);

            // Special-case integers
            if (a % 1.0 == 0.0) {
                // Gamma is undefined on non-positive integers
                if (v0 <= 0.0) {
                    return double.NaN;
                }

                // factorial(v0 - 1)
                if (a <= 25.0) {
                    if (a <= 2.0) {
                        return 1.0;
                    }
                    a -= 1.0;
                    v0 -= 1.0;
                    while (--v0 > 1.0) {
                        a *= v0;
                    }
                    return a;
                }
            }

            // lim(Gamma(v0)) = 1.0 / v0 as v0 approaches 0.0
            if (a < 1e-50) {
                return 1.0 / v0;
            }

            double res;
            if (v0 < -150.0) {
                // If Gamma(1 - v0) could overflow for large v0, use the duplication formula to
                // compute Gamma(1 - v0):
                //     Gamma(x) * Gamma(x + 0,5) = sqrt(pi) * 2**(1 - 2x) * Gamma(2x)
                // ==> Gamma(1 - x) = Gamma((1-x)/2) * Gamma((2-x)/2) / (2**x * sqrt(pi))
                // Then apply the reflection formula:
                //     Gamma(x) = pi / sin(pi * x) / Gamma(1 - x)
                double halfV0 = v0 / 2.0;
                res = Math.Pow(Math.PI, 1.5) / SinPi(v0);
                res *= Math.Pow(2.0, v0);
                res /= PositiveGamma(0.5 - halfV0);
                res /= PositiveGamma(1.0 - halfV0);
            } else if (v0 < 0.001) {
                // For values less than or close to zero, just use the reflection formula
                res = Math.PI / SinPi(v0);
                double v1 = 1.0 - v0;
                if (v0 == 1.0 - v1) {
                    res /= PositiveGamma(v1);
                } else {
                    // Computing v1 has resulted in a loss of precision. To avoid this, use the
                    // recurrence relation Gamma(x + 1) = x * Gamma(x).
                    res /= -v0 * PositiveGamma(-v0);
                }
            } else {
                res = PositiveGamma(v0);
            }

            return res;
        }

        public static double LogGamma(double v0) {
            // Calculate the log of the Gamma function using the Lanczos approximation

            if (double.IsInfinity(v0)) {
                return double.PositiveInfinity;
            }
            double a = Math.Abs(v0);

            // Gamma is undefined on non-positive integers
            if (v0 <= 0.0 && a % 1.0 == 0.0) {
                return double.NaN;
            }

            // lim(LGamma(v0)) = -log|v0| as v0 approaches 0.0
            if (a < 1e-50) {
                return -Math.Log(a);
            }

            double res;
            if (v0 < 0.0) {
                // For negative values, use the reflection formula:
                //     Gamma(x) = pi / sin(pi * x) / Gamma(1 - x)
                // ==> LGamma(x) = log(pi / |sin(pi * x)|) - LGamma(1 - x)
                res = Math.Log(Math.PI / AbsSinPi(v0));
                res -= PositiveLGamma(1.0 - v0);
            } else {
                res = PositiveLGamma(v0);
            }

            return res;
        }

        public static double Hypot(double x, double y) {
            //
            // sqrt(x*x + y*y) == sqrt(x*x * (1 + (y*y)/(x*x))) ==
            // sqrt(x*x) * sqrt(1 + (y/x)*(y/x)) ==
            // abs(x) * sqrt(1 + (y/x)*(y/x))
            //

            // Handle infinities
            if (double.IsInfinity(x) || double.IsInfinity(y)) {
                return double.PositiveInfinity;
            }

            //  First, get abs
            if (x < 0.0) x = -x;
            if (y < 0.0) y = -y;

            // Obvious cases
            if (x == 0.0) return y;
            if (y == 0.0) return x;

            // Divide smaller number by bigger number to safeguard the (y/x)*(y/x)
            if (x < y) {
                double temp = y; y = x; x = temp;
            }

            y /= x;

            // calculate abs(x) * sqrt(1 + (y/x)*(y/x))
            return x * System.Math.Sqrt(1 + y * y);
        }

        /// <summary>
        /// Evaluates a polynomial in v0 where the coefficients are ordered in increasing degree
        /// </summary>
        private static double EvalPolynomial(double v0, double[] coeffs) {
            double res = 0.0;
            for (int i = coeffs.Length - 1; i >= 0; i--) {
                res = checked(res * v0 + coeffs[i]);
            }

            return res;
        }

        /// <summary>
        /// Evaluates a polynomial in v0 where the coefficients are ordered in increasing degree
        /// if reverse is false, and increasing degree if reverse is true.
        /// </summary>
        private static double EvalPolynomial(double v0, double[] coeffs, bool reverse) {
            if (!reverse) {
                return EvalPolynomial(v0, coeffs);
            }

            double res = 0.0;
            for (int i = 0; i < coeffs.Length; i++) {
                res = checked(res * v0 + coeffs[i]);
            }

            return res;
        }

        /// <summary>
        /// A numerically precise version of sin(v0 * pi)
        /// </summary>
        private static double SinPi(double v0) {
            double res = Math.Abs(v0) % 2.0;

            if (res < 0.25) {
                res = Math.Sin(res * Math.PI);
            } else if (res < 0.75) {
                res = Math.Cos((res - 0.5) * Math.PI);
            } else if (res < 1.25) {
                res = -Math.Sin((res - 1.0) * Math.PI);
            } else if (res < 1.75) {
                res = -Math.Cos((res - 1.5) * Math.PI);
            } else {
                res = Math.Sin((res - 2.0) * Math.PI);
            }

            return v0 < 0 ? -res : res;
        }

        /// <summary>
        /// A numerically precise version of |sin(v0 * pi)|
        /// </summary>
        private static double AbsSinPi(double v0) {
            double res = Math.Abs(v0) % 1.0;

            if (res < 0.25) {
                res = Math.Sin(res * Math.PI);
            } else if (res < 0.75) {
                res = Math.Cos((res - 0.5) * Math.PI);
            } else {
                res = Math.Sin((res - 1.0) * Math.PI);
            }

            return Math.Abs(res);
        }

        // polynomial coefficients ordered by increasing degree
        private static double[] ErfNumerCoeffs = {
            2.4266795523053175e02, 2.1979261618294152e01,
            6.9963834886191355, -3.5609843701815385e-02
        };
        private static double[] ErfDenomCoeffs = {
            2.1505887586986120e02, 9.1164905404514901e01,
            1.5082797630407787e01, 1.0
        };
        private static double[] ErfcNumerCoeffs = {
            3.004592610201616005e02, 4.519189537118729422e02,
            3.393208167343436870e02, 1.529892850469404039e02,
            4.316222722205673530e01, 7.211758250883093659,
            5.641955174789739711e-01, -1.368648573827167067e-07
        };
        private static double[] ErfcDenomCoeffs = {
            3.004592609569832933e02, 7.909509253278980272e02,
            9.313540948506096211e02, 6.389802644656311665e02,
            2.775854447439876434e02, 7.700015293522947295e01,
            1.278272731962942351e01, 1.0
        };
        private static double[] GammaNumerCoeffs = {
            4.401213842800460895436e13, 4.159045335859320051581e13,
            1.801384278711799677796e13, 4.728736263475388896889e12,
            8.379100836284046470415e11, 1.055837072734299344907e11,
            9.701363618494999493386e09, 6.549143975482052641016e08,
            3.223832294213356530668e07, 1.128514219497091438040e06,
            2.666579378459858944762e04, 3.818801248632926870394e02,
            2.506628274631000502415
        };
        private static double[] GammaDenomCoeffs = {
            0.0, 39916800.0, 120543840.0, 150917976.0,
            105258076.0, 45995730.0, 13339535.0, 2637558.0,
            357423.0, 32670.0, 1925.0, 66.0, 1.0
        };

        /// <summary>
        /// Take the quotient of the 2 polynomials forming the Lanczos approximation
        /// with N=13 and G=13.144565
        /// </summary>
        private static double GammaRationalFunc(double v0) {
            double numer = 0.0;
            double denom = 0.0;

            if (v0 < 1e15) {
                numer = EvalPolynomial(v0, GammaNumerCoeffs);
                denom = EvalPolynomial(v0, GammaDenomCoeffs);
            } else {
                double vRecip = 1.0 / v0;
                numer = EvalPolynomial(vRecip, GammaNumerCoeffs, true);
                denom = EvalPolynomial(vRecip, GammaDenomCoeffs, true);
            }

            return numer / denom;
        }

        /// <summary>
        /// Computes the Gamma function on positive values, using the Lanczos approximation.
        /// Lanczos parameters are N=13 and G=13.144565.
        /// </summary>
        private static double PositiveGamma(double v0) {
            if (v0 > 200.0) {
                return Double.PositiveInfinity;
            }

            double vg = v0 + 12.644565; // v0 + g - 0.5
            double res = GammaRationalFunc(v0);
            res /= Math.Exp(vg);
            if (v0 < 120.0) {
                res *= Math.Pow(vg, v0 - 0.5);
            } else {
                // Use a smaller exponent if we're in danger of overflowing Math.Pow
                double sqrt = Math.Pow(vg, v0 / 2.0 - 0.25);
                res *= sqrt;
                res *= sqrt;
            }

            return res;
        }

        /// <summary>
        /// Computes the Log-Gamma function on positive values, using the Lanczos approximation.
        /// Lanczos parameters are N=13 and G=13.144565.
        /// </summary>
        private static double PositiveLGamma(double v0) {
            double vg = v0 + 12.644565; // v0 + g - 0.5
            double res = Math.Log(GammaRationalFunc(v0)) - vg;
            res += (v0 - 0.5) * Math.Log(vg);

            return res;
        }

        #endregion

        #region BigInteger

        // generated by scripts/radix_generator.py
        private static readonly uint[] maxCharsPerDigit = { 0, 0, 31, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
        private static readonly uint[] groupRadixValues = { 0, 0, 2147483648, 3486784401, 1073741824, 1220703125, 2176782336, 1977326743, 1073741824, 3486784401, 1000000000, 2357947691, 429981696, 815730721, 1475789056, 2562890625, 268435456, 410338673, 612220032, 893871739, 1280000000, 1801088541, 2494357888, 3404825447, 191102976, 244140625, 308915776, 387420489, 481890304, 594823321, 729000000, 887503681, 1073741824, 1291467969, 1544804416, 1838265625, 2176782336 };

        internal static string BigIntegerToString(uint[] d, int sign, int radix, bool lowerCase) {
            if (radix < 2) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            }
            if (radix > 36) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be <= 36");
            }

            int dl = d.Length;
            if (dl == 0) {
                return "0";
            }

            List<uint> digitGroups = new List<uint>();

            uint groupRadix = groupRadixValues[radix];
            while (dl > 0) {
                uint rem = div(d, ref dl, groupRadix);
                digitGroups.Add(rem);
            }

            StringBuilder ret = new StringBuilder();
            if (sign == -1) {
                ret.Append("-");
            }

            int digitIndex = digitGroups.Count - 1;

            char[] tmpDigits = new char[maxCharsPerDigit[radix]];

            AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, false, lowerCase);
            while (digitIndex >= 0) {
                AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, true, lowerCase);
            }
            return ret.Length == 0 ? "0" : ret.ToString();
        }

        private const int BitsPerDigit = 32;

        private static uint div(uint[] n, ref int nl, uint d) {
            ulong rem = 0;
            int i = nl;
            bool seenNonZero = false;
            while (--i >= 0) {
                rem <<= BitsPerDigit;
                rem |= n[i];
                uint v = (uint)(rem / d);
                n[i] = v;
                if (v == 0) {
                    if (!seenNonZero) nl--;
                } else {
                    seenNonZero = true;
                }
                rem %= d;
            }
            return (uint)rem;
        }

        private static void AppendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros, bool lowerCase) {
            string symbols = lowerCase ? "0123456789abcdefghijklmnopqrstuvwxyz" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int digits = tmp.Length;
            int i = digits;
            while (i > 0 && (leadingZeros || rem != 0)) {
                uint digit = rem % radix;
                rem /= radix;
                tmp[--i] = symbols[(int)digit];
            }
            if (leadingZeros) buf.Append(tmp);
            else buf.Append(tmp, i, digits - i);
        }

        // Helper for GetRandBits
        private static uint GetWord(byte[] bytes, int start, int end) {
            uint four = 0;
            int bits = end - start;
            int shift = 0;
            if (bits > 32) {
                bits = 32;
            }
            start /= 8;
            while (bits > 0) {
                uint value = bytes[start];
                if (bits < 8) {
                    value &= (1u << bits) - 1u;
                }
                value <<= shift;
                four |= value;
                bits -= 8;
                shift += 8;
                start++;
            }

            return four;
        }
#if !MONO_INTERPRETER
#if !FEATURE_NUMERICS
        public static BigInteger GetRandBits(this Random generator, int bits) {
            ContractUtils.Requires(bits > 0);

            // equivalent to (bits + 7) / 8 without possibility of overflow
            int count = bits % 8 == 0 ? bits / 8 : bits / 8 + 1;

            // Pad the end (most significant) with zeros if we align to the byte
            // to ensure that we end up with a positive value
            byte[] bytes = new byte[bits % 8 == 0 ? count + 1 : count];
            generator.NextBytes(bytes);
            if (bits % 8 == 0) {
                bytes[bytes.Length - 1] = 0;
            } else {
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] & ((1 << (bits % 8)) - 1));
            }

            if (bits <= 32) {
                return (BigInteger)GetWord(bytes, 0, bits);
            } else if (bits <= 64) {
                ulong a = GetWord(bytes, 0, bits);
                ulong b = GetWord(bytes, 32, bits);
                return (BigInteger)(a | (b << 32));
            } else {
                count = (count + 3) / 4;
                uint[] data = new uint[count];
                for (int i = 0; i < count; i++) {
                    data[i] = GetWord(bytes, i * 32, bits);
                }
                return new BigInteger(1, data);
            }
        }

        public static BigInteger Random(this Random generator, BigInteger limit) {
            ContractUtils.Requires(limit.Sign > 0, "limit");
            ContractUtils.RequiresNotNull(generator, "generator");

            // TODO: this doesn't yield a uniform distribution (small numbers will be picked more frequently):
            uint[] result = new uint[limit.GetWordCount() + 1];
            for (int i = 0; i < result.Length; i++) {
                result[i] = unchecked((uint)generator.Next());
            }
            return new BigInteger(1, result) % limit;
        }
#else
        public static BigInt GetRandBits(this Random generator, int bits) {
            ContractUtils.Requires(bits > 0);

            // equivalent to (bits + 7) / 8 without possibility of overflow
            int count = bits % 8 == 0 ? bits / 8 : bits / 8 + 1;

            // Pad the end (most significant) with zeros if we align to the byte
            // to ensure that we end up with a positive value
            byte[] bytes = new byte[bits % 8 == 0 ? count + 1 : count];
            generator.NextBytes(bytes);
            if (bits % 8 == 0) {
                bytes[bytes.Length - 1] = 0;
            } else {
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] & ((1 << (bits % 8)) - 1));
            }

            if (bits <= 32) {
                return (BigInt)GetWord(bytes, 0, bits);
            } else if (bits <= 64) {
                ulong a = GetWord(bytes, 0, bits);
                ulong b = GetWord(bytes, 32, bits);
                return (BigInt)(a | (b << 32));
            }
            
            return new BigInt(bytes);
        }

        public static BigInteger Random(this Random generator, BigInteger limit) {
            return new BigInteger(generator.Random(limit.Value));
        }

        public static BigInt Random(this Random generator, BigInt limit) {
            ContractUtils.Requires(limit.Sign > 0, "limit");
            ContractUtils.RequiresNotNull(generator, "generator");

            BigInt res = BigInt.Zero;

            while (true) {
                // if we've run out of significant digits, we can return the total
                if (limit == BigInt.Zero) {
                    return res;
                }

                // if we're small enough to fit in an int, do so
                int iLimit;
                if (limit.AsInt32(out iLimit)) {
                    return res + generator.Next(iLimit);
                }

                // get the 3 or 4 uppermost bytes that fit into an int
                int hiData;
                byte[] data = limit.ToByteArray();
                int index = data.Length;
                while (data[--index] == 0) ;
                if (data[index] < 0x80) {
                    hiData = data[index] << 24;
                    data[index--] = (byte)0;
                } else {
                    hiData = 0;
                }
                hiData |= data[index] << 16;
                data[index--] = (byte)0;
                hiData |= data[index] << 8;
                data[index--] = (byte)0;
                hiData |= data[index];
                data[index--] = (byte)0;

                // get a uniform random number for the uppermost portion of the bigint
                byte[] randomData = new byte[index + 2];
                generator.NextBytes(randomData);
                randomData[index + 1] = (byte)0;
                res += new BigInt(randomData);
                res += (BigInt)generator.Next(hiData) << ((index + 1) * 8);

                // sum it with a uniform random number for the remainder of the bigint
                limit = new BigInt(data);
            }
        }

        public static bool TryToFloat64(this BigInt self, out double result) {
            return StringUtils.TryParseDouble(
                self.ToString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result
            );
        }

        public static double ToFloat64(this BigInt self) {
            return double.Parse(
                self.ToString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
            );
        }
#endif
        public static bool TryToFloat64(this BigInteger self, out double result) {
            return StringUtils.TryParseDouble(
                self.ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result
            );
        }

        public static double ToFloat64(this BigInteger self) {
            return double.Parse(
                self.ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
            );
        }

        // Like GetBitCount(Abs(x)), except 0 maps to 0
        public static int BitLength(BigInteger x) {
            if (x.IsZero()) {
                return 0;
            }

            return x.Abs().GetBitCount();
        }

#if FEATURE_NUMERICS
        public static int BitLength(BigInt x) {
            if (x.IsZero) {
                return 0;
            }

            byte[] bytes = BigInt.Abs(x).ToByteArray();
            int index = bytes.Length;
            while (bytes[--index] == 0) ;

            return index * 8 + BitLength((int)bytes[index]);
        }
#endif
#endif
        // Like GetBitCount(Abs(x)), except 0 maps to 0
        public static int BitLength(long x) {
            if (x == 0) {
                return 0;
            }
            if (x == Int64.MinValue) {
                return 64;
            }

            x = Math.Abs(x);
            int res = 1;
            if (x >= 1L << 32) {
                x >>= 32;
                res += 32;
            }
            if (x >= 1L << 16) {
                x >>= 16;
                res += 16;
            }
            if (x >= 1L << 8) {
                x >>= 8;
                res += 8;
            }
            if (x >= 1L << 4) {
                x >>= 4;
                res += 4;
            }
            if (x >= 1L << 2) {
                x >>= 2;
                res += 2;
            }
            if (x >= 1L << 1) {
                res += 1;
            }

            return res;
        }

        // Like GetBitCount(Abs(x)), except 0 maps to 0
        [CLSCompliant(false)]
        public static int BitLengthUnsigned(ulong x) {
            if (x >= 1uL << 63) {
                return 64;
            }
            return BitLength((long)x);
        }

        // Like GetBitCount(Abs(x)), except 0 maps to 0
        public static int BitLength(int x) {
            if (x == 0) {
                return 0;
            }
            if (x == Int32.MinValue) {
                return 32;
            }

            x = Math.Abs(x);
            int res = 1;
            if (x >= 1 << 16) {
                x >>= 16;
                res += 16;
            }
            if (x >= 1 << 8) {
                x >>= 8;
                res += 8;
            }
            if (x >= 1 << 4) {
                x >>= 4;
                res += 4;
            }
            if (x >= 1 << 2) {
                x >>= 2;
                res += 2;
            }
            if (x >= 1 << 1) {
                res += 1;
            }

            return res;
        }

        // Like GetBitCount(Abs(x)), except 0 maps to 0
        [CLSCompliant(false)]
        public static int BitLengthUnsigned(uint x) {
            if (x >= 1u << 31) {
                return 32;
            }
            return BitLength((int)x);
        }

        #region Extending BigInt with BigInteger API
#if FEATURE_NUMERICS

        public static bool AsInt32(this BigInt self, out int ret) {
            if (self >= Int32.MinValue && self <= Int32.MaxValue) {
                ret = (Int32)self;
                return true;
            }
            ret = 0;
            return false;
        }

        public static bool AsInt64(this BigInt self, out long ret) {
            if (self >= Int64.MinValue && self <= Int64.MaxValue) {
                ret = (long)self;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt32(this BigInt self, out uint ret) {
            if (self >= UInt32.MinValue && self <= UInt32.MaxValue) {
                ret = (UInt32)self;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt64(this BigInt self, out ulong ret) {
            if (self >= UInt64.MinValue && self <= UInt64.MaxValue) {
                ret = (UInt64)self;
                return true;
            }
            ret = 0;
            return false;
        }

        public static BigInt Abs(this BigInt self) {
            return BigInt.Abs(self);
        }

        public static bool IsZero(this BigInt self) {
            return self.IsZero;
        }

        public static bool IsPositive(this BigInt self) {
            return self.Sign > 0;
        }

        public static bool IsNegative(this BigInt self) {
            return self.Sign < 0;
        }

        public static double Log(this BigInt self) {
            return BigInt.Log(self);
        }

        public static double Log(this BigInt self, double baseValue) {
            return BigInt.Log(self, baseValue);
        }

        public static double Log10(this BigInt self) {
            return BigInt.Log10(self);
        }

        public static BigInt Power(this BigInt self, int exp) {
            return BigInt.Pow(self, exp);
        }

        public static BigInt ModPow(this BigInt self, int power, BigInt mod) {
            return BigInt.ModPow(self, power, mod);
        }

        public static BigInt ModPow(this BigInt self, BigInt power, BigInt mod) {
            return BigInt.ModPow(self, power, mod);
        }

        public static string ToString(this BigInt self, int radix) {
            const string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            }
            if (radix > 36) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be <= 36");
            }

            bool isNegative = false;
            if (self < BigInt.Zero) {
                self = -self;
                isNegative = true;
            } else if (self == BigInt.Zero) {
                return "0";
            }

            List<char> digits = new List<char>();
            while (self > 0) {
                digits.Add(symbols[(int)(self % radix)]);
                self /= radix;
            }

            StringBuilder ret = new StringBuilder();
            if (isNegative) {
                ret.Append('-');
            }
            for (int digitIndex = digits.Count - 1; digitIndex >= 0; digitIndex--) {
                ret.Append(digits[digitIndex]);
            }
            return ret.ToString();
        }
#endif
        #endregion

        #region Exposing underlying data
#if FEATURE_NUMERICS

        [CLSCompliant(false)]
        public static uint[] GetWords(this BigInt self) {
            if (self.IsZero) {
                return new uint[] { 0 };
            }

            int hi;
            byte[] bytes;
            GetHighestByte(self, out hi, out bytes);

            uint[] result = new uint[(hi + 1 + 3) / 4];
            int i = 0;
            int j = 0;
            uint u = 0;
            int shift = 0;
            while (i < bytes.Length) {
                u |= (uint)bytes[i++] << shift;
                if (i % 4 == 0) {
                    result[j++] = u;
                    u = 0;
                }
                shift += 8;
            }
            if (u != 0) {
                result[j] = u;
            }
            return result;
        }

        [CLSCompliant(false)]
        public static uint GetWord(this BigInt self, int index) {
            return GetWords(self)[index];
        }

        public static int GetWordCount(this BigInt self) {
            int index;
            byte[] bytes;
            GetHighestByte(self, out index, out bytes);
            return index / 4 + 1; // return (index + 1 + 3) / 4;
        }

        public static int GetByteCount(this BigInt self) {
            int index;
            byte[] bytes;
            GetHighestByte(self, out index, out bytes);
            return index + 1;
        }

        public static int GetBitCount(this BigInt self) {
            if (self.IsZero) {
                return 1;
            }
            byte[] bytes = BigInt.Abs(self).ToByteArray();

            int index = bytes.Length;
            while (bytes[--index] == 0) ;

            int count = index * 8;
            for (int hiByte = bytes[index]; hiByte > 0; hiByte >>= 1) {
                count++;
            }
            return count;
        }

        private static byte GetHighestByte(BigInt self, out int index, out byte[] byteArray) {
            byte[] bytes = BigInt.Abs(self).ToByteArray();
            if (self.IsZero) {
                byteArray = bytes;
                index = 0;
                return 1;
            }

            int hi = bytes.Length;
            byte b;
            do {
                b = bytes[--hi];
            } while (b == 0);
            index = hi;
            byteArray = bytes;
            return b;
        }

#endif
        #endregion

        #endregion

#if !MONO_INTERPRETER
        #region Complex

#if !FEATURE_NUMERICS
        public static Complex64 MakeReal(double real) {
            return new Complex64(real, 0.0);
        }

        public static Complex64 MakeImaginary(double imag) {
            return new Complex64(0.0, imag);
        }

        public static Complex64 MakeComplex(double real, double imag) {
            return new Complex64(real, imag);
        }

        public static double Imaginary(this Complex64 self) {
            return self.Imag;
        }

        public static bool IsZero(this Complex64 self) {
            return self.IsZero;
        }

        public static Complex64 Pow(this Complex64 self, Complex64 power) {
            return self.Power(power);
        }
#else
        public static Complex MakeReal(double real) {
            return new Complex(real, 0.0);
        }

        public static Complex MakeImaginary(double imag) {
            return new Complex(0.0, imag);
        }

        public static Complex MakeComplex(double real, double imag) {
            return new Complex(real, imag);
        }

        public static double Imaginary(this Complex self) {
            return self.Imaginary;
        }

        public static bool IsZero(this Complex self) {
            return self.Equals(Complex.Zero);
        }

        public static Complex Conjugate(this Complex self) {
            return new Complex(self.Real, -self.Imaginary);
        }

        public static double Abs(this Complex self) {
            return Complex.Abs(self);
        }

        public static Complex Pow(this Complex self, Complex power) {
            return Complex.Pow(self, power);
        }
#endif

        #endregion
#endif
    }

}
