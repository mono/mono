//
// Mono.Math.Prime.Generator.NextPrimeFinder.cs - Prime Generator
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//

using System;

namespace Mono.Math.Prime.Generator {

	/// <summary>
	/// Finds the next prime after a given number.
	/// </summary>
	[CLSCompliant(false)]
	public class NextPrimeFinder : SequentialSearchPrimeGeneratorBase {
		
		protected override BigInteger GenerateSearchBase (int bits, object Context) 
		{
			if (Context == null) throw new ArgumentNullException ("Context");
			BigInteger ret = new BigInteger ((BigInteger)Context);
			ret.setBit (0);
			return ret;
		}
	}
}
