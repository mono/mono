//
// Mono.Math.Prime.Generator.SequentialSearchPrimeGeneratorBase.cs - Prime Generator
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace Mono.Math.Prime.Generator {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class SequentialSearchPrimeGeneratorBase : PrimeGeneratorBase {

		protected virtual BigInteger GenerateSearchBase (int bits, object context)
		{
			BigInteger ret = BigInteger.GenerateRandom (bits);
			ret.SetBit (0);
			return ret;
		}


		public override BigInteger GenerateNewPrime (int bits)
		{
			return GenerateNewPrime (bits, null);
		}


		public virtual BigInteger GenerateNewPrime (int bits, object context)
		{
			//
			// STEP 1. Find a place to do a sequential search
			//
			BigInteger curVal = GenerateSearchBase (bits, context);

			const uint primeProd1 = 3u* 5u * 7u * 11u * 13u * 17u * 19u * 23u * 29u;

			uint pMod1 = curVal % primeProd1;

			int DivisionBound = TrialDivisionBounds;
			uint[] SmallPrimes = BigInteger.smallPrimes;
			//
			// STEP 2. Search for primes
			//
			while (true) {

				//
				// STEP 2.1 Sieve out numbers divisible by the first 9 primes
				//
				if (pMod1 %  3 == 0) goto biNotPrime;
				if (pMod1 %  5 == 0) goto biNotPrime;
				if (pMod1 %  7 == 0) goto biNotPrime;
				if (pMod1 % 11 == 0) goto biNotPrime;
				if (pMod1 % 13 == 0) goto biNotPrime;
				if (pMod1 % 17 == 0) goto biNotPrime;
				if (pMod1 % 19 == 0) goto biNotPrime;
				if (pMod1 % 23 == 0) goto biNotPrime;
				if (pMod1 % 29 == 0) goto biNotPrime;

				//
				// STEP 2.2 Sieve out all numbers divisible by the primes <= DivisionBound
				//
				for (int p = 10; p < SmallPrimes.Length && SmallPrimes [p] <= DivisionBound; p++) {
					if (curVal % SmallPrimes [p] == 0)
						goto biNotPrime;
				}

				//
				// STEP 2.3 Is the potential prime acceptable?
				//
				if (!IsPrimeAcceptable (curVal, context))
					goto biNotPrime;

				//
				// STEP 2.4 Filter out all primes that pass this step with a primality test
				//
				if (PrimalityTest (curVal, Confidence))
					return curVal;

				//
				// STEP 2.4
				//
			biNotPrime:
				pMod1 += 2;
				if (pMod1 >= primeProd1)
					pMod1 -= primeProd1;
				curVal.Incr2 ();
			}
		}

		protected virtual bool IsPrimeAcceptable (BigInteger bi, object context)
		{
			return true;
		}
	}
}
