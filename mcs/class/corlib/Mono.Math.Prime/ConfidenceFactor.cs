//
// Mono.Math.Prime.ConfidenceFactor.cs - Confidence factor for prime generation
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//

using System;

namespace Mono.Math.Prime {
	/// <summary>
	/// A factor of confidence.
	/// </summary>
	public enum ConfidenceFactor {
		/// <summary>
		/// Only suitable for development use, probability of failure may be greater than 1/2^20.
		/// </summary>
		ExtraLow,
		/// <summary>
		/// Suitable only for transactions which do not require forward secrecy.  Probability of failure about 1/2^40
		/// </summary>
		Low,
		/// <summary>
		/// Designed for production use. Probability of failure about 1/2^80.
		/// </summary>
		Medium,
		/// <summary>
		/// Suitable for sensitive data. Probability of failure about 1/2^160.
		/// </summary>
		High,
		/// <summary>
		/// Use only if you have lots of time! Probability of failure about 1/2^320.
		/// </summary>
		ExtraHigh,
		/// <summary>
		/// Only use methods which generate provable primes. Not yet implemented.
		/// </summary>
		Provable
	}
}
