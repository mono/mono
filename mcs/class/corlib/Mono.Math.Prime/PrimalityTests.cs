//
// Mono.Math.Prime.PrimalityTests.cs - Test for primality
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//

using System;
using System.Security.Cryptography;

namespace Mono.Math.Prime {

	[CLSCompliant(false)]
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	delegate bool PrimalityTest (BigInteger bi, ConfidenceFactor confidence);

	[CLSCompliant(false)]
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class PrimalityTests {

		#region SPP Test
		
		private static int GetSPPRounds (BigInteger bi, ConfidenceFactor confidence)
		{
			int bc = bi.bitCount();

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
					return Rounds <<= 1;
				case ConfidenceFactor.ExtraHigh:
					return Rounds <<= 2;
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

			int bits = bi.bitCount ();
			BigInteger a = null;
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			BigInteger.ModulusRing mr = new BigInteger.ModulusRing (bi);

			for (int round = 0; round < Rounds; round++) {
				while (true) {		           // generate a < n
					a = BigInteger.genRandom (bits, rng);

					// make sure "a" is not 0
					if (a > 1 && a < bi)
						break;
				}

				if (a.gcd (bi) != 1) return false;

				BigInteger b = mr.Pow (a, t);

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
