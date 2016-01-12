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
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Builders;

namespace NUnit.Framework.Attributes
{
    [TestFixture]
    public class PairwiseTest
    {
        [TestFixture]
        public class LiveTest
        {
            private PairCounter pairsTested = new PairCounter();

            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                pairsTested = new PairCounter();
            }

            [TestFixtureTearDown]
            public void TestFixtureTearDown()
            {
                Assert.That(pairsTested.Count, Is.EqualTo(16));
            }

            [Test, Pairwise]
            public void Test(
                [Values("a", "b", "c")] string a,
                [Values("+", "-")] string b,
                [Values("x", "y")] string c)
            {
                Console.WriteLine("Pairwise: {0} {1} {2}", a, b, c);

                pairsTested[a + b] = null;
                pairsTested[a + c] = null;
                pairsTested[b + c] = null;
            }
        }

        // Test data is taken from various sources. See "Lessons Learned
        // in Software Testing" pp 53-59, for example. For orthogonal cases, see 
        // http://www.freequality.org/sites/www_freequality_org/documents/tools/Tagarray_files/tamatrix.htm
        static internal object[] cases = new object[]
        {
#if ORIGINAL
            new TestCaseData( new int[] { 2, 4 }, 8, 8 ).SetName("Test 2x4"),
            new TestCaseData( new int[] { 2, 2, 2 }, 5, 4 ).SetName("Test 2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2 }, 6, 6 ).SetName("Test 3x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2 }, 7, 6 ).SetName("Test 3x2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2, 2 }, 8, 6 ).SetName("Test 3x2x2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2, 2, 2 }, 9, 8 ).SetName("Test 3x2x2x2x2x2"),
            new TestCaseData( new int[] { 3, 3, 3 }, 12, 9 ).SetName("Test 3x3x3"),
            new TestCaseData( new int[] { 4, 4, 4 }, 22, 16 ).SetName("Test 4x4x4"),
            new TestCaseData( new int[] { 5, 5, 5 }, 34, 25 ).SetName("Test 5x5x5")
#else
            new TestCaseData( new int[] { 2, 4 }, 8, 8 ).SetName("Test 2x4"),
            new TestCaseData( new int[] { 2, 2, 2 }, 5, 4 ).SetName("Test 2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2 }, 7, 6 ).SetName("Test 3x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2 }, 8, 6 ).SetName("Test 3x2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2, 2 }, 9, 6 ).SetName("Test 3x2x2x2x2"),
            new TestCaseData( new int[] { 3, 2, 2, 2, 2, 2 }, 9, 8 ).SetName("Test 3x2x2x2x2x2"),
            new TestCaseData( new int[] { 3, 3, 3 }, 9, 9 ).SetName("Test 3x3x3"),
            new TestCaseData( new int[] { 4, 4, 4 }, 17, 16 ).SetName("Test 4x4x4"),
            new TestCaseData( new int[] { 5, 5, 5 }, 27, 25 ).SetName("Test 5x5x5")
#endif
        };

        [Test, TestCaseSource("cases")]
        public void Test(int[] dimensions, int bestSoFar, int targetCases)
        {
            int features = dimensions.Length;

            string[][] sources = new string[features][];

            for (int i = 0; i < features; i++)
            {
                string featureName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(i, 1);

                int n = dimensions[i];
                sources[i] = new string[n];
                for (int j = 0; j < n; j++)
                    sources[i][j] = featureName + j.ToString();
            }

            CombiningStrategy strategy = new PairwiseStrategy(sources);

            PairCounter pairs = new PairCounter();
            int cases = 0;
            foreach (NUnit.Framework.Internal.ParameterSet parms in strategy.GetTestCases())
            {
                for (int i = 1; i < features; i++)
                    for (int j = 0; j < i; j++)
                    {
                        string a = parms.Arguments[i] as string;
                        string b = parms.Arguments[j] as string;
                        pairs[a + b] = null;
                    }

                ++cases;
            }

            int expectedPairs = 0;
            for (int i = 1; i < features; i++)
                for (int j = 0; j < i; j++)
                    expectedPairs += dimensions[i] * dimensions[j];

            Assert.That(pairs.Count, Is.EqualTo(expectedPairs), "Number of pairs is incorrect");
            Assert.That(cases, Is.AtMost(bestSoFar), "Regression: Number of test cases exceeded target previously reached");
#if DEBUG
            //Assert.That(cases, Is.AtMost(targetCases), "Number of test cases exceeded target");
#endif
        }

#if CLR_2_0 || CLR_4_0
        class PairCounter : System.Collections.Generic.Dictionary<string, object> {}
#else
        class PairCounter : Hashtable { }
#endif
    }
}
