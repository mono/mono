//
// System.Random Test Cases
//
// Author: Bob Smith <bob@thestuff.net>
//

using NUnit.Framework;
using System;

namespace MonoTests.System {

public class RandomTest : TestCase
{
	public RandomTest() {}
        public void TestDouble()
        {
                Random r = new Random();
                int i;
                double c=0;
                for (i=0; i<20; i++) c+=r.NextDouble();
                c/=i;
                Assert (c.ToString() + " is out of range.", c < .7 && c > .3);
        }
        public void TestSeed()
        {
                Random r = new Random(42);
                Random r2 = new Random(42);
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
                Random r = new Random();
                int i;
                long c;
                for (i=0; i<20; i++)
                {
                        c = r.Next();
                        Assert (c < Int32.MaxValue && c >= 0);
                }
        }
        public void TestNextMax()
        {
                Random r = new Random();
                int i;
                long c;
                for (i=0; i<20; i++)
                {
                        c = r.Next(10);
                        Assert (c < 10 && c >= 0);
                }
        }
        public void TestNextMinMax()
        {
                Random r = new Random();
                int i;
                long c;
		AssertEquals ("#1 Failed where min == max", 42, r.Next (42, 42));
		AssertEquals ("#2 Failed where min == max", Int32.MaxValue, r.Next (Int32.MaxValue,Int32.MaxValue));
		AssertEquals ("#3 Failed where min == max", Int32.MinValue, r.Next (Int32.MinValue,Int32.MinValue));
		AssertEquals ("#4 Failed where min == max", 0, r.Next (0, 0));
                for (i = 1; i <= Int32.MaxValue / 2; i *= 2)
                {
                        c = r.Next (i, i * 2);
			Assert ("At i=" + i + " c < i*2 failed", c < i * 2);
                        Assert ("At i=" + i + " c >= i failed", c >= i);
                }
                for (i = -1; i >= Int32.MinValue / 2; i *= 2)
                {
                        c = r.Next (i * 2, i);
			Assert ("At i=" + i + " c < i*2 failed", c < i);
                        Assert ("At i=" + i + " c >= i failed", c >= i * 2);
                }
        }
}

}
