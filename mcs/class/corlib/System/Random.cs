//
// System.Random.cs
//
// Author:
//   Bob Smith (bob@thestuff.net)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
//

using System;
using System.Globalization;

namespace System
{
        public class Random
        {
                private int S = 1;
                private const int A = 16807;
                private const int M = 2147483647;
                private const int Q = 127773;
                private const int R = 2836;

                public Random()
                {
                        S = (int)(DateTime.Now.Ticks);
                }

                public Random(int Seed)
                {
                        S = Seed;
                }

                public virtual int Next()
                {
                        return (int)(this.Sample()*Int32.MaxValue);
                }

                public virtual int Next(int maxValue)
                {
                        if (maxValue < 0)
                                throw new ArgumentOutOfRangeException(Locale.GetText (
					"Max value is less then min value."));
                        else if (maxValue == 0)
                                return 0;
                        return (int)(this.Sample()*maxValue);
                }

                public virtual int Next(int minValue, int maxValue)
                {
                        if (minValue > maxValue)
                                throw new ArgumentOutOfRangeException(Locale.GetText (
					"Min value is greater then max value."));
                        else if (minValue == maxValue)
                                return minValue;
                        return (int)(this.Sample()*maxValue)+minValue;
                }
                public virtual void NextBytes(byte[] buffer)
                {
                        int i, l;
                        if (buffer == null)
                                throw new ArgumentNullException();
                        l = buffer.GetUpperBound(0);
                        for (i = buffer.GetLowerBound(0); i < l; i++)
                        {
                                buffer[i] = (byte)(this.Sample()*Byte.MaxValue);
                        }
                }

                public virtual double NextDouble ()
                {
                        return this.Sample();
                }

                protected virtual double Sample ()
		{
                        S = A*(S%Q)-R*(S/Q);
                        if (S < 0)
				S+=M;
                        return S/(double)Int32.MaxValue;
                }
        }
}
