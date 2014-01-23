//
// System.Random.cs
//
// Authors:
//   Bob Smith (bob@thestuff.net)
//   Ben Maurer (bmaurer@users.sourceforge.net)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
// (C) 2003 Ben Maurer
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	public class Random
	{
		uint x;
		uint y;
		uint z;
		uint c;

		public Random ()
			: this (Environment.TickCount)
		{
		}

		public Random (int Seed)
		{
			x = (uint) Seed;
			y = (uint) 987654321;
			z = (uint) 43219876;
			c = (uint) 6543217;
		}

		uint JKiss ()
		{
			x = 314527869 * x + 1234567;
			y ^= y << 5;
			y ^= y >> 7;
			y ^= y << 22;
			ulong t = ((ulong) 4294584393 * z + c);
			c = (uint) (t >> 32);
			z = (uint) t;
			return (x + y + z);
		}

		public virtual int Next (int minValue, int maxValue)
		{
			if (minValue > maxValue)
				throw new ArgumentOutOfRangeException ("Maximum value is less than minimal value.");

			// special case: a difference of one (or less) will always return the minimum
			// e.g. -1,-1 or -1,0 will always return -1
			uint diff = (uint) (maxValue - minValue);
			if (diff <= 1)
				return minValue;

			return minValue + ((int) (JKiss () % diff));
		}

		public virtual int Next (int maxValue)
		{
			if (maxValue < 0)
				throw new ArgumentOutOfRangeException ("Maximum value is less than minimal value.");

			return maxValue > 0 ? (int)(JKiss () % maxValue) : 0;
		}

		public virtual int Next ()
		{
			// returns a non-negative, [0 - Int32.MacValue], random number
			// but we want to avoid calls to Math.Abs (call cost and branching cost it requires)
			// and the fact it would throw for Int32.MinValue (so roughly 1 time out of 2^32)
			int random = (int) JKiss ();
			while (random == Int32.MinValue)
				random = (int) JKiss ();
			int mask = random >> 31;
			random ^= mask;
			return random + (mask & 1);
		}

		public virtual void NextBytes (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			// each random `int` can fill 4 bytes
			int p = 0;
			uint random;
			for (int i = 0; i < (buffer.Length >> 2); i++) {
				random = JKiss ();
				buffer [p++] = (byte) (random >> 24);
				buffer [p++] = (byte) (random >> 16);
				buffer [p++] = (byte) (random >> 8);
				buffer [p++] = (byte) random;
			}
			if (p == buffer.Length)
				return;

			// complete the array
			random = JKiss ();
			while (p < buffer.Length) {
				buffer [p++] = (byte) random;
				random >>= 8;
			}
		}

		public virtual double NextDouble ()
		{
			// return a double value between [0,1]
			return Sample ();
		}

		protected virtual double Sample ()
		{
			// a single 32 bits random value is not enough to create a random double value
			uint a = JKiss () >> 6;	// Upper 26 bits
			uint b = JKiss () >> 5;	// Upper 27 bits
			return (a * 134217728.0 + b) / 9007199254740992.0;
		}
	}
}