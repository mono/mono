//
// System.Random.cs
//
// Authors:
//	Bob Smith (bob@thestuff.net)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
// (C) 2003 Ben Maurer
//

using System;
using System.Globalization;

namespace System {

	[Serializable]
	public class Random {

		const int MBIG = int.MaxValue;
		const int MSEED = 161803398;
		const int MZ = 0;

		int inext, inextp;
		int [] ma = new int [56];
		
		public Random () : this (Environment.TickCount) {}
		
		public Random (int Seed)
		{
			int ii;
			int mj, mk;

			// Numerical Recipes in C online @ http://www.library.cornell.edu/nr/bookcpdf/c7-1.pdf
			mj = MSEED - Math.Abs (Seed);
			ma [55] = mj;
			mk = 1;
			for (int i = 1; i < 55; i++) {  //  [1, 55] is special (Knuth)
				ii = (21 * i) % 55;
				ma [ii] = mk;
				mk = mj - mk;
				if (mk < 0)
					mk += MBIG;
				mj = ma[ii];
			}
			for (int k = 1; k < 5; k++) {
				for (int i = 1; i < 56; i++) {
					ma [i] -= ma [1 + (i + 30) % 55];
					if (ma [i] < 0)
						ma [i] += MBIG;
				}
			}
			inext = 0;
			inextp = 21;
		}

		protected virtual double Sample ()
		{
			int retVal;
			
			if (++inext  >= 56) inext  = 1;
			if (++inextp >= 56) inextp = 1;
			
			retVal = ma [inext] - ma [inextp];
			
			if (retVal < 0)
				retVal += MBIG;
			
			ma [inext] = retVal;

			return retVal * (1.0 / MBIG);
		}

                public virtual int Next ()
                {
                        return (int)(Sample () * int.MaxValue);
                }

                public virtual int Next (int maxValue)
                {
                        if (maxValue < 0)
                                throw new ArgumentOutOfRangeException(Locale.GetText (
                                        "Max value is less then min value."));

                        return (int)(Sample () * maxValue);
                }

                public virtual int Next (int minValue, int maxValue)
                {
                        if (minValue > maxValue)
                                throw new ArgumentOutOfRangeException (Locale.GetText (
                                        "Min value is greater then max value."));

                        return (int)(Sample () * (maxValue - minValue)) + minValue;
                }

                public virtual void NextBytes (byte [] buffer)
                {
			if (buffer==null) throw new ArgumentNullException ("buffer");
			for (int i = 0; i < buffer.Length; i++) {
				buffer [i] = (byte)(Sample () * (byte.MaxValue + 1)); 
			}
                }

                public virtual double NextDouble ()
                {
                        return this.Sample ();
                }
	}
}