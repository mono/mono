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
	public static ITest Suite {
		get {
			return new TestSuite(typeof(RandomTest));
		}
	}

        public RandomTest(string name): base(name){}
        public void TestDouble()
        {
                Random r = new Random();
                int i;
                double c=0;
                for (i=0; i<20; i++) c+=r.NextDouble();
                c/=i;
                Assert (c < .7 && c > .3);
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
                for (i=0; i<20; i++)
                {
                        c = r.Next(1, 10);
                        Assert (c < 10 && c >= 1);
                }
        }
}

}
