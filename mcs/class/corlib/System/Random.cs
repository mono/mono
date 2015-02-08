//
// System.Random.cs
//
// Authors:
//   Bob Smith (bob@thestuff.net)
//   Ben Maurer (bmaurer@users.sourceforge.net)
//   Sebastien Pouliot  <sebastien@xamarin.com>
//   Konstantin Safonov <kasthack@epicm.org>
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

		private const double REAL_UNIT_INT = 1.0 / ( int.MaxValue + 1.0 );
		private const double REAL_UNIT_UINT = 1.0 / ( uint.MaxValue + 1.0 );
		private const uint Y = 842502087;
		private const uint Z = 3579807591;
		private const uint W = 273326509;

		public Random ()
			: this (Environment.TickCount)
		{
		}

		public Random (int Seed)
		{
			Reset (Seed);
		}

		private void Reset (int seed)
		{
			x = (uint)seed;
			y = Y;
			z = Z;
			c = W;
		}

		public virtual int Next (int minValue, int maxValue)
		{
			if (minValue > maxValue)
				throw new ArgumentOutOfRangeException ("Maximum value is less than minimal value.");
			uint t = x ^ x << 11;
			x = y;
			y = z;
			z = c;
			int range = maxValue - minValue;
			if (range < 0)
			{
				// If range is <0 then an overflow has occured and must resort to using long integer arithmetic instead (slower).
				return minValue + (int)( REAL_UNIT_UINT * ( c = c ^ c >> 19 ^ ( t ^ t >> 8 ) ) * ( (long)maxValue - minValue ) );
			}
			return minValue + (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( c = ( c ^ c >> 19 ) ^ ( t ^ t >> 8 ) ) ) * range );
		}

		public virtual int Next (int maxValue)
		{
			if (maxValue < 0)
				throw new ArgumentOutOfRangeException ("Maximum value is less than minimal value.");
			uint t = x ^ x << 11;
			x = y;
			y = z;
			z = c;
			return (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( c = c ^ c >> 19 ^ ( t ^ t >> 8 ) ) ) * maxValue );
		}

		public virtual int Next ()
		{
			uint t = x ^ x << 11;
			x = y;
			y = z;
			z = c;
			c = c ^ c >> 19 ^ ( t ^ t >> 8 );
			uint rtn = c & 0x7FFFFFFF;
			return (int)rtn;
		}

		public unsafe virtual void NextBytes (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			uint
				x = this.x,
				y = this.y,
				z = this.z,
				c = this.c;
			int i = 0;
			uint t;
			if (buffer.Length > sizeof(int) - 1)
			{
				fixed (byte* bptr = buffer)
				{
					uint* iptr = (uint*)bptr;
					uint* endptr = iptr + buffer.Length / 4;
					do
					{
						t = ( x ^ ( x << 11 ) );
						x = y;
						y = z;
						z = c;
						c = c ^ c >> 19 ^ ( t ^ t >> 8 );
						*iptr = c;
					}
					while (++iptr < endptr);
					i = buffer.Length - buffer.Length % 4;
				}
			}
			if (i < buffer.Length)
			{
				t = ( x ^ ( x << 11 ) );
				x = y; y = z; z = c;
				c = c ^ c >> 19 ^ ( t ^ t >> 8 );
				do
				{
					buffer [i] = (byte)( c >>= 8 );
				}
				while (++i < buffer.Length);
			}
			this.x = x; this.y = y; this.z = z; this.c = c;
		}

		public virtual double NextDouble ()
		{
			// return a double value between [0,1]
			return Sample ();
		}

		protected virtual double Sample ()
		{
			uint t = x ^ x << 11;
			x = y;
			y = z;
			z = c;
			return REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( c = c ^ c >> 19 ^ ( t ^ t >> 8 ) ) );
		}
	}
}