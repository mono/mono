//
// RandomNumberGeneratorTest.cs - NUnit Test Cases for RNG
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using Mono.Security.Cryptography;
using System.Text;

namespace MonoTests.Security.Cryptography {

// References:
// a.	NIST FIPS PUB 140-2: Security requirements for Cryptographic Modules 
//	http://csrc.nist.gov/publications/fips/fips140-2/fips1402.pdf
// b.	NIST SP 800-22: A Statistical Test Suite for Random and Pseudorandom Number Generators for Cryptographic Applications
//	not implemented
//	http://csrc.nist.gov/publications/nistpubs/800-22/sp-800-22-051501.pdf
// c.	IETF RFC1750: Randomness Recommendations for Security
//	not implemented
//	http://www.ietf.org/rfc/rfc1750.txt

public class RandomNumberGeneratorTest : Assertion {

	protected RandomNumberGenerator rng;

	[SetUp]
	void Create () 
	{
		rng = new Mono.Security.Cryptography.RNGCryptoServiceProvider ();
	}

	// count the number of 1
	protected void Monobit (string rngName, byte[] sample) 
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
		Assert (rngName + " Monobit x=" + x, ((9725 < x) && (x < 10275)));
	}

	// 16 patterns (nibbles)
	protected void Poker (string rngName, byte[] sample) 
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
		Assert (rngName + " Poker: " + result, ((result > 2.16) && (result < 46.17)));
	}

	// runs of 1 (or 0)
	protected void Runs (string rngName, byte[] sample) 
	{
		int[] runs = new int[6];
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
							runs[p]++;
						x = 0;
					}
				}
				else {
					if (!zero) {
						one = false;
						zero = true;
						/*int p = Math.Min (x, 6) - 1;
						if (p >= 0)
							runs[p]++;*/
						x = 0;
					}
				}
				x++;
				// next bit
				b >>= 1;
			}
		}
		Assert (rngName + " Runs length=1: " + runs[0], ((runs[0] >= 2343) && (runs[0] <= 2657)));
		Assert (rngName + " Runs length=2: " + runs[1], ((runs[1] >= 1135) && (runs[1] <= 1365)));
		Assert (rngName + " Runs length=3: " + runs[2], ((runs[2] >=  542) && (runs[2] <= 708)));
		Assert (rngName + " Runs length=4: " + runs[3], ((runs[3] >=  251) && (runs[3] <= 373)));
		Assert (rngName + " Runs length=5: " + runs[4], ((runs[4] >=  111) && (runs[4] <= 201)));
		Assert (rngName + " Runs length=6+ " + runs[5], ((runs[5] >=  111) && (runs[5] <= 201)));
	}

	// no long runs of 26 or more (0 or 1)
	protected void LongRuns (string rngName, byte[] sample) 
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
		Assert (rngName + " Long Runs max = " + longestRun, (longestRun < 26));
	}

	// all tests should be done on the same random sample
	public void TestFIPS140 () 
	{
		string name = rng.ToString ();
		// 20,000 bits
		byte[] sample = new byte [2500];
		rng.GetBytes (sample);

		Monobit (name, sample);
		Poker (name, sample);
		Runs (name, sample);
		LongRuns (name, sample);
	}
}

}