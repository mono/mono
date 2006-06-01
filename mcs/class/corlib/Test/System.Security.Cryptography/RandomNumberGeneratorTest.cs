//
// RandomNumberGeneratorTest.cs - NUnit Test Cases for RNG
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	NIST FIPS PUB 140-2: Security requirements for Cryptographic Modules 
//	http://csrc.nist.gov/publications/fips/fips140-2/fips1402.pdf
// b.	NIST SP 800-22: A Statistical Test Suite for Random and Pseudorandom Number Generators for Cryptographic Applications
//	not implemented
//	http://csrc.nist.gov/publications/nistpubs/800-22/sp-800-22-051501.pdf
// c.	IETF RFC1750: Randomness Recommendations for Security
//	not implemented
//	http://www.ietf.org/rfc/rfc1750.txt

[TestFixture]
public class RandomNumberGeneratorTest {

	private string name;
	private byte[] sample;

	[TestFixtureSetUp]
	public void SetUp () 
	{
		// all tests should be done on the same random sample
		RandomNumberGenerator rng = RandomNumberGenerator.Create ();
		name = rng.ToString ();
		// 20,000 bits
		sample = new byte[2500];
		rng.GetBytes (sample);
	}

	// count the number of 1
	[Test]
	public void Monobit () 
	{
		int x = 0;
		for (int i=0; i < sample.Length; i++) {
			byte b = sample[i];
			for (int j = 0; j < 8; j++) {
				if ((b & 0x01) == 0x01)
					x++;
				// next bit
				b >>= 1;
			}
		}
		Assert.IsTrue ((9725  < x), String.Format ("{0} Monobit x={1} > 9725",  name, x));
		Assert.IsTrue ((x < 10275), String.Format ("{0} Monobit x={1} < 10275", name, x));
	}

	// 16 patterns (nibbles)
	[Test]
	public void Poker () 
	{
		int[] pattern = new int[16];
		for (int i = 0; i < sample.Length; i++) {
			byte b = sample[i];
			int n = (b & 0x0F);
			pattern[n]++;
			b >>= 4;
			n = b;
			pattern[n]++;
		}
		double result = 0;
		for (int i = 0; i < 16; i++)
			result += (pattern[i] * pattern[i]);
		result = ((16 * result) / 5000) - 5000;

		Assert.IsTrue (((result > 2.16) && (result < 46.17)), name + " Poker: " + result);
	}

	// runs of 1 (or 0)
	[Test]
	public void Runs () 
	{
		int[,] runs = new int[6,2];
		int x = 0;
		bool one = false;
		bool zero = false;
		for (int i = sample.Length - 1; i >= 0 ; i--) {
			byte b = sample[i];
			for (int j = 0; j < 8; j++) {
				if ((b & 0x01) == 0x01) {
					if (!one) {
						one = true;
						zero = false;
						int p = Math.Min (x, 6) - 1;
						if (p >= 0)
							runs[p,0]++;
						x = 0;
					}
				}
				else {
					if (!zero) {
						one = false;
						zero = true;
						int p = Math.Min (x, 6) - 1;
						if (p >= 0)
							runs[p,1]++;
						x = 0;
					}
				}
				x++;
				// next bit
				b >>= 1;
			}
		}
		// don't forget the ast run
		if (x > 0) {
			int p = Math.Min (x, 6) - 1;
			if (p >= 0)
				runs [p, zero ? 0 : 1]++;
		}
		// Updated ranges as per FIPS140-2 Change Notice #1
		// check for runs of zeros
		Assert.IsTrue (((runs[0,0] >= 2315) && (runs[0,0] <= 2685)), name + " 0-Runs length=1: " + runs[0,0]);
		Assert.IsTrue (((runs[1,0] >= 1114) && (runs[1,0] <= 1386)), name + " 0-Runs length=2: " + runs[1,0]);
		Assert.IsTrue (((runs[2,0] >=  527) && (runs[2,0] <=  723)), name + " 0-Runs length=3: " + runs[2,0]);
		Assert.IsTrue (((runs[3,0] >=  240) && (runs[3,0] <=  384)), name + " 0-Runs length=4: " + runs[3,0]);
		Assert.IsTrue (((runs[4,0] >=  103) && (runs[4,0] <=  209)), name + " 0-Runs length=5: " + runs[4,0]);
		Assert.IsTrue (((runs[5,0] >=  103) && (runs[5,0] <=  209)), name + " 0-Runs length=6+ " + runs[5,0]);
		// check for runs of ones
		Assert.IsTrue (((runs[0,1] >= 2315) && (runs[0,1] <= 2685)), name + " 1-Runs length=1: " + runs[0,1]);
		Assert.IsTrue (((runs[1,1] >= 1114) && (runs[1,1] <= 1386)), name + " 1-Runs length=2: " + runs[1,1]);
		Assert.IsTrue (((runs[2,1] >=  527) && (runs[2,1] <=  723)), name + " 1-Runs length=3: " + runs[2,1]);
		Assert.IsTrue (((runs[3,1] >=  240) && (runs[3,1] <=  384)), name + " 1-Runs length=4: " + runs[3,1]);
		Assert.IsTrue (((runs[4,1] >=  103) && (runs[4,1] <=  209)), name + " 1-Runs length=5: " + runs[4,1]);
		Assert.IsTrue (((runs[5,1] >=  103) && (runs[5,1] <=  209)), name + " 1-Runs length=6+ " + runs[5,1]);
	}

	// no long runs of 26 or more (0 or 1)
	[Test]
	public void LongRuns () 
	{
		int longestRun = 0;
		int currentRun = 0;
		bool one = false;
		bool zero = false;
		for (int i = sample.Length - 1; i >= 0 ; i--) {
			byte b = sample[i];
			for (int j = 0; j < 8; j++) {
				if ((b & 0x01) == 0x01) {
					if (!one) {
						one = true;
						zero = false;
						longestRun = Math.Max (longestRun, currentRun);
						currentRun = 0;
					}
					currentRun++;
				}
				else {
					if (!zero) {
						one = false;
						zero = true;
						longestRun = Math.Max (longestRun, currentRun);
						currentRun = 0;
					}
					currentRun++;
				}
				// next bit
				b >>= 1;
			}
		}
		Assert.IsTrue ((longestRun < 26), name + " Long Runs max = " + longestRun);
	}
}

}
