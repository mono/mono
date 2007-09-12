//
// Mono.Math.Prime.PrimalityTests.cs - Test for primality
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
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

namespace Mono.Math.Prime {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	delegate bool PrimalityTest (BigInteger bi, ConfidenceFactor confidence);

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class PrimalityTests {

		private PrimalityTests ()
		{
		}

		#region SPP Test
		
		private static int GetSPPRounds (BigInteger bi, ConfidenceFactor confidence)
		{
			int bc = bi.BitCount();

			int Rounds;

			// Data from HAC, 4.49
			if      (bc <= 100 ) Rounds = 27;
			else if (bc <= 150 ) Rounds = 18;
			else if (bc <= 200 ) Rounds = 15;
			else if (bc <= 250 ) Rounds = 12;
			else if (bc <= 300 ) Rounds =  9;
			else if (bc <= 350 ) Rounds =  8;
			else if (bc <= 400 ) Rounds =  7;
			else if (bc <= 500 ) Rounds =  6;
			else if (bc <= 600 ) Rounds =  5;
			else if (bc <= 800 ) Rounds =  4;
			else if (bc <= 1250) Rounds =  3;
			else		     Rounds =  2;

			switch (confidence) {
				case ConfidenceFactor.ExtraLow:
					Rounds >>= 2;
					return Rounds != 0 ? Rounds : 1;
				case ConfidenceFactor.Low:
					Rounds >>= 1;
					return Rounds != 0 ? Rounds : 1;
				case ConfidenceFactor.Medium:
					return Rounds;
				case ConfidenceFactor.High:
					return Rounds << 1;
				case ConfidenceFactor.ExtraHigh:
					return Rounds << 2;
				case ConfidenceFactor.Provable:
					throw new Exception ("The Rabin-Miller test can not be executed in a way such that its results are provable");
				default:
					throw new ArgumentOutOfRangeException ("confidence");
			}
		}

		public static bool Test (BigInteger n, ConfidenceFactor confidence)
		{
			// Rabin-Miller fails with smaller primes (at least with our BigInteger code)
			if (n.BitCount () < 33)
				return SmallPrimeSppTest (n, confidence);
			else
				return RabinMillerTest (n, confidence);
		}

		/// <summary>
		///     Probabilistic prime test based on Rabin-Miller's test
		/// </summary>
		/// <param name="n" type="BigInteger.BigInteger">
		///     <para>
		///         The number to test.
		///     </para>
		/// </param>
		/// <param name="confidence" type="int">
		///     <para>
		///	The number of chosen bases. The test has at least a
		///	1/4^confidence chance of falsely returning True.
		///     </para>
		/// </param>
		/// <returns>
		///	<para>
		///		True if "this" is a strong pseudoprime to randomly chosen bases.
		///	</para>
		///	<para>
		///		False if "this" is definitely NOT prime.
		///	</para>
		/// </returns>
		public static bool RabinMillerTest (BigInteger n, ConfidenceFactor confidence)
		{
			int bits = n.BitCount ();
			int t = GetSPPRounds (bits, confidence);

			// n - 1 == 2^s * r, r is odd
			BigInteger n_minus_1 = n - 1;
			int s = n_minus_1.LowestSetBit ();
			BigInteger r = n_minus_1 >> s;

			BigInteger.ModulusRing mr = new BigInteger.ModulusRing (n);
			
			// Applying optimization from HAC section 4.50 (base == 2)
			// not a really random base but an interesting (and speedy) one
			BigInteger y = null;
			// FIXME - optimization disable for small primes due to bug #81857
			if (n.BitCount () > 100)
				y = mr.Pow (2, r);

			// still here ? start at round 1 (round 0 was a == 2)
			for (int round = 0; round < t; round++) {

				if ((round > 0) || (y == null)) {
					BigInteger a = null;

					// check for 2 <= a <= n - 2
					// ...but we already did a == 2 previously as an optimization
					do {
						a = BigInteger.GenerateRandom (bits);
					} while ((a <= 2) && (a >= n_minus_1));

					y = mr.Pow (a, r);
				}

				if (y == 1)
					continue;

				for (int j = 0; ((j < s) && (y != n_minus_1)); j++) {

					y = mr.Pow (y, 2);
					if (y == 1)
						return false;
				}

				if (y != n_minus_1)
					return false;
			}
			return true;
		}

		public static bool SmallPrimeSppTest (BigInteger bi, ConfidenceFactor confidence)
		{
			int Rounds = GetSPPRounds (bi, confidence);

			// calculate values of s and t
			BigInteger p_sub1 = bi - 1;
			int s = p_sub1.LowestSetBit ();

			BigInteger t = p_sub1 >> s;


			BigInteger.ModulusRing mr = new BigInteger.ModulusRing (bi);

			for (int round = 0; round < Rounds; round++) {

				BigInteger b = mr.Pow (BigInteger.smallPrimes [round], t);

				if (b == 1) continue;              // a^t mod p = 1

				bool result = false;
				for (int j = 0; j < s; j++) {

					if (b == p_sub1) {         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
						result = true;
						break;
					}

					b = (b * b) % bi;
				}

				if (result == false)
					return false;
			}
			return true;
		}

		#endregion

		// TODO: Implement the Lucus test
		// TODO: Implement other new primality tests
		// TODO: Implement primality proving
	}
}
