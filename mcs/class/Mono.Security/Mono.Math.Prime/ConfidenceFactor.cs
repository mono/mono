//
// Mono.Math.Prime.ConfidenceFactor.cs - Confidence factor for prime generation
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
	/// <summary>
	/// A factor of confidence.
	/// </summary>
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	enum ConfidenceFactor {
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
