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

		/// <summary>
		///     Probabilistic prime test based on Rabin-Miller's test
		/// </summary>
		/// <param name="bi" type="BigInteger.BigInteger">
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
		public static bool RabinMillerTest (BigInteger bi, ConfidenceFactor confidence)
		{
			int Rounds = GetSPPRounds (bi, confidence);

			// calculate values of s and t
			BigInteger p_sub1 = bi - 1;
			int s = p_sub1.LowestSetBit ();

			BigInteger t = p_sub1 >> s;

			int bits = bi.BitCount ();
			BigInteger a = null;
			BigInteger.ModulusRing mr = new BigInteger.ModulusRing (bi);
			
			// Applying optimization from HAC section 4.50 (base == 2)
			// not a really random base but an interesting (and speedy) one
			BigInteger b = mr.Pow (2, t);
			if (b != 1) {
				bool result = false;
				for (int j=0; j < s; j++) {
					if (b == p_sub1) {         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
						result = true;
						break;
					}

					b = (b * b) % bi;
				}
				if (!result)
					return false;
			}

			// still here ? start at round 1 (round 0 was a == 2)
			for (int round = 1; round < Rounds; round++) {
				while (true) {		           // generate a < n
					a = BigInteger.GenerateRandom (bits);

					// make sure "a" is not 0 (and not 2 as we have already tested that)
					if (a > 2 && a < bi)
						break;
				}

				if (a.GCD (bi) != 1)
					return false;

				b = mr.Pow (a, t);

				if (b == 1)
					continue;              // a^t mod p = 1

				bool result = false;
				for (int j = 0; j < s; j++) {

					if (b == p_sub1) {         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
						result = true;
						break;
					}

					b = (b * b) % bi;
				}

				if (!result)
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
