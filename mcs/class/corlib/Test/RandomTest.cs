//
// System.Random Test Cases
//
// Author: Bob Smith <bob@thestuff.net>
//

using NUnit.Framework;
using System;

public class RandomTest : TestCase
{
        public RandomTest(string name): base(name){}
        public void TestDouble()
        {
                Random r;
                int i;
                double c=0;
                for (i=0; i<20; i++) c+=r.NextDouble();
                c/=i;
                Assert (c < .7 && c > .3);
        }
        public void TestSeed()
        {
                Random r(42), r2(42);
                int i;
                double c=0, c2=0;
                for (i=0; i<20; i++)
                {
                        c += r.NextDouble();
                        c2 += r2.NextDouble();
                }
                AssertEquals(c, c2);
        }
        public void TestNext()
        {
                Random r;
                int i;
                long c;
                for (i=0; i<20; i++)
                {
                        c = r.Next();
                        Assert (c <= Random.MaxValue && c >= Random.MinValue)
                }
        }
        public void TestNextMax()
        {
                Random r;
                int i;
                long c;
                for (i=0; i<20; i++)
                {
                        c = r.Next(10);
                        Assert (c <= 10 && c >= Random.MinValue)
                }
        }
        public void TestNextMinMax()
        {
                Random r;
                int i;
                long c;
                for (i=0; i<20; i++)
                {
                        c = r.Next(1, 10);
                        Assert (c <= 10 && c >= 1)
                }
        }
}
