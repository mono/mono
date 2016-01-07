// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
// ***********************************************************************

using System;
using System.Reflection;
using NUnit.TestUtilities;

namespace NUnit.Framework.Internal
{
    public class RandomizerTests
    {
        [Test]
        public void RandomizersAreUnique()
        {
            int[] values = new int[10];
            for (int i = 0; i < 10; i++)
                values[i] = Randomizer.CreateRandomizer().Next();

            Assert.That(values, Is.Unique);
        }

        [Test]
        public void RandomIntsAreUnique()
        {
            Randomizer r = Randomizer.CreateRandomizer();

            int[] values = new int[10];
            for (int i = 0; i < 10; i++)
                values[i] = r.Next();

            Assert.That(values, Is.Unique);
        }

        [Test]
        public void RandomDoublesAreUnique()
        {
            Randomizer r = Randomizer.CreateRandomizer();

            double[] values = new double[10];
            for (int i = 0; i < 10; i++)
                values[i] = r.NextDouble();

            Assert.That(values, Is.Unique);
        }

        [Test]
        public void CanGetArrayOfRandomInts()
        {
            Randomizer r = Randomizer.CreateRandomizer();

            int[] ints = r.GetInts(1, 100, 10);
            Assert.That(ints.Length, Is.EqualTo(10));
            foreach (int i in ints)
                Assert.That(i, Is.InRange(1, 100));
        }

        [Test]
        public void CanGetArrayOfRandomDoubles()
        {
            Randomizer r = Randomizer.CreateRandomizer();

            double[] doubles = r.GetDoubles(0.5, 1.5, 10);
            Assert.That(doubles.Length, Is.EqualTo(10));
            foreach (double d in doubles)
                Assert.That(d, Is.InRange(0.5, 1.5));

            // Heuristic: Could fail occasionally
            Assert.That(doubles, Is.Unique);
        }

        [Test]
        public void CanGetArrayOfRandomEnums()
        {
            Randomizer r = Randomizer.CreateRandomizer();

            object[] enums = r.GetEnums(10, typeof(AttributeTargets));
            Assert.That(enums.Length, Is.EqualTo(10));
            foreach (object e in enums)
                Assert.That(e, Is.TypeOf(typeof(AttributeTargets)));
        }

        [Test]
        public void RandomizersWithSameSeedsReturnSameValues()
        {
            Randomizer r1 = new Randomizer(1234);
            Randomizer r2 = new Randomizer(1234);

            for (int i = 0; i < 10; i++)
                Assert.That(r1.NextDouble(), Is.EqualTo(r2.NextDouble()));
        }

        [Test]
        public void RandomizersWithDifferentSeedsReturnDifferentValues()
        {
            Randomizer r1 = new Randomizer(1234);
            Randomizer r2 = new Randomizer(4321);

            for (int i = 0; i < 10; i++)
                Assert.That(r1.NextDouble(), Is.Not.EqualTo(r2.NextDouble()));
        }

        [Test]
        public void ReturnsSameRandomizerForSameParameter()
        {
            ParameterInfo p = testMethod1.GetParameters()[0];
            Randomizer r1 = Randomizer.GetRandomizer(p);
            Randomizer r2 = Randomizer.GetRandomizer(p);
            Assert.That(r1, Is.SameAs(r2));
        }

        [Test]
        public void ReturnsSameRandomizerForDifferentParametersOfSameMethod()
        {
            ParameterInfo p1 = testMethod1.GetParameters()[0];
            ParameterInfo p2 = testMethod1.GetParameters()[1];
            Randomizer r1 = Randomizer.GetRandomizer(p1);
            Randomizer r2 = Randomizer.GetRandomizer(p2);
            Assert.That(r1, Is.SameAs(r2));
        }

        [Test]
        public void ReturnsSameRandomizerForSameMethod()
        {
            Randomizer r1 = Randomizer.GetRandomizer(testMethod1);
            Randomizer r2 = Randomizer.GetRandomizer(testMethod1);
            Assert.That(r1, Is.SameAs(r2));
        }

        [Test]
        public void ReturnsDifferentRandomizersForDifferentMethods()
        {
            Randomizer r1 = Randomizer.GetRandomizer(testMethod1);
            Randomizer r2 = Randomizer.GetRandomizer(testMethod2);
            Assert.That(r1, Is.Not.SameAs(r2));
        }

        static readonly MethodInfo testMethod1 =
            typeof(RandomizerTests).GetMethod("TestMethod1", BindingFlags.NonPublic | BindingFlags.Instance);
        private void TestMethod1(int x, int y)
        {
        }

        static readonly MethodInfo testMethod2 =
            typeof(RandomizerTests).GetMethod("TestMethod2", BindingFlags.NonPublic | BindingFlags.Instance);
        private void TestMethod2(int x, int y)
        {
        }
    }
}
