// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif
using NUnit.Framework.Internal;
using NUnit.TestUtilities;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class EqualConstraintTest : ConstraintTestBase
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new EqualConstraint(4);
            expectedDescription = "4";
            stringRepresentation = "<equal 4>";
        }

        internal object[] SuccessData = new object[] { 4, 4.0f, 4.0d, 4.0000m };

        internal object[] FailureData = new object[] { 
            new TestCaseData( 5, "5" ), 
            new TestCaseData( null, "null" ),
            new TestCaseData( "Hello", "\"Hello\"" ),
            new TestCaseData( double.NaN, "NaN" ),
            new TestCaseData( double.PositiveInfinity, "Infinity" ) };

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void CanMatchSpecialFloatingPointValues(object value)
        {
            Assert.That(value, new EqualConstraint(value));
        }

        [Test]
        public void CanMatchDates()
        {
            DateTime expected = new DateTime(2007, 4, 1);
            DateTime actual = new DateTime(2007, 4, 1);
            Assert.That(actual, new EqualConstraint(expected));
        }

        [Test]
        public void CanMatchDatesWithinTimeSpan()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 13, 1, 0);
            TimeSpan tolerance = TimeSpan.FromMinutes(5.0);
            Assert.That(actual, new EqualConstraint(expected).Within(tolerance));
        }

        [Test]
        public void CanMatchDatesWithinDays()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 4, 13, 0, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(5).Days);
        }

        [Test]
        public void CanMatchDatesWithinHours()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 16, 0, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(5).Hours);
        }

        [Test]
        public void CanMatchDatesWithinMinutes()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 13, 1, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(5).Minutes);
        }

        [Test]
        public void CanMatchTimeSpanWithinMinutes()
        {
            TimeSpan expected = new TimeSpan(10, 0, 0);
            TimeSpan actual = new TimeSpan(10, 2, 30);
            Assert.That(actual, new EqualConstraint(expected).Within(5).Minutes);
        }

        [Test]
        public void CanMatchDatesWithinSeconds()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 13, 1, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(300).Seconds);
        }

        [Test]
        public void CanMatchDatesWithinMilliseconds()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 13, 1, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(300000).Milliseconds);
        }

        [Test]
        public void CanMatchDatesWithinTicks()
        {
            DateTime expected = new DateTime(2007, 4, 1, 13, 0, 0);
            DateTime actual = new DateTime(2007, 4, 1, 13, 1, 0);
            Assert.That(actual, new EqualConstraint(expected).Within(TimeSpan.TicksPerMinute * 5).Ticks);
        }

        #region Dictionary Tests

#if (CLR_2_0 || CLR_4_0) && !NETCF_2_0
#if !SILVERLIGHT
        // TODO: Move these to a separate fixture
        [Test]
        public void CanMatchHashtables_SameOrder()
        {
            Assert.AreEqual(new Hashtable { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Hashtable { { 0, 0 }, { 1, 1 }, { 2, 2 } });
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void CanMatchHashtables_Failure()
        {
            Assert.AreEqual(new Hashtable { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Hashtable { { 0, 0 }, { 1, 5 }, { 2, 2 } });
        }

        [Test]
        public void CanMatchHashtables_DifferentOrder()
        {
            Assert.AreEqual(new Hashtable { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Hashtable { { 0, 0 }, { 2, 2 }, { 1, 1 } });
        }
#endif

        [Test]
        public void CanMatchDictionaries_SameOrder()
        {
            Assert.AreEqual(new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } });
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void CanMatchDictionaries_Failure()
        {
            Assert.AreEqual(new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Dictionary<int, int> { { 0, 0 }, { 1, 5 }, { 2, 2 } });
        }

        [Test]
        public void CanMatchDictionaries_DifferentOrder()
        {
            Assert.AreEqual(new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Dictionary<int, int> { { 0, 0 }, { 2, 2 }, { 1, 1 } });
        }

#if !SILVERLIGHT
        [Test]
        public void CanMatchHashtableWithDictionary()
        {
            Assert.AreEqual(new Hashtable { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                            new Dictionary<int, int> { { 0, 0 }, { 2, 2 }, { 1, 1 } });
        }
#endif
#endif

        #endregion

        [TestCase(20000000000000004.0)]
        [TestCase(19999999999999996.0)]
        public void CanMatchDoublesWithUlpTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(20000000000000000.0).Within(1).Ulps);
        }

        [ExpectedException(typeof(AssertionException), ExpectedMessage = "+/- 1 Ulps", MatchType = MessageMatch.Contains)]
        [TestCase(20000000000000008.0)]
        [TestCase(19999999999999992.0)]
        public void FailsOnDoublesOutsideOfUlpTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(20000000000000000.0).Within(1).Ulps);
        }

        [TestCase(19999998.0f)]
        [TestCase(20000002.0f)]
        public void CanMatchSinglesWithUlpTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(20000000.0f).Within(1).Ulps);
        }

        [ExpectedException(typeof(AssertionException), ExpectedMessage = "+/- 1 Ulps", MatchType = MessageMatch.Contains)]
        [TestCase(19999996.0f)]
        [TestCase(20000004.0f)]
        public void FailsOnSinglesOutsideOfUlpTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(20000000.0f).Within(1).Ulps);
        }

        [TestCase(9500.0)]
        [TestCase(10000.0)]
        [TestCase(10500.0)]
        public void CanMatchDoublesWithRelativeTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(10000.0).Within(10.0).Percent);
        }

        [ExpectedException(typeof(AssertionException), ExpectedMessage = "+/- 10.0d Percent", MatchType = MessageMatch.Contains)]
        [TestCase(8500.0)]
        [TestCase(11500.0)]
        public void FailsOnDoublesOutsideOfRelativeTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(10000.0).Within(10.0).Percent);
        }

        [TestCase(9500.0f)]
        [TestCase(10000.0f)]
        [TestCase(10500.0f)]
        public void CanMatchSinglesWithRelativeTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(10000.0f).Within(10.0f).Percent);
        }

        [ExpectedException(typeof(AssertionException), ExpectedMessage = "+/- 10.0f Percent", MatchType = MessageMatch.Contains)]
        [TestCase(8500.0f)]
        [TestCase(11500.0f)]
        public void FailsOnSinglesOutsideOfRelativeTolerance(object value)
        {
            Assert.That(value, new EqualConstraint(10000.0f).Within(10.0f).Percent);
        }

        /// <summary>Applies both the Percent and Ulps modifiers to cause an exception</summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorWithPercentAndUlpsToleranceModes()
        {
            EqualConstraint shouldFail = new EqualConstraint(100.0f).Within(10.0f).Percent.Ulps;
        }

        /// <summary>Applies both the Ulps and Percent modifiers to cause an exception</summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorWithUlpsAndPercentToleranceModes()
        {
            EqualConstraint shouldFail = new EqualConstraint(100.0f).Within(10.0f).Ulps.Percent;
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfPercentPrecedesWithin()
        {
            Assert.That(1010, Is.EqualTo(1000).Percent.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfUlpsPrecedesWithin()
        {
            Assert.That(1010.0, Is.EqualTo(1000.0).Ulps.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfDaysPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Days.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfHoursPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Hours.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfMinutesPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Minutes.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfSecondsPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Seconds.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfMillisecondsPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Milliseconds.Within(5));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfTicksPrecedesWithin()
        {
            Assert.That(DateTime.Now, Is.EqualTo(DateTime.Now).Ticks.Within(5));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestCase(1000, 1010)]
        [TestCase(1000U, 1010U)]
        [TestCase(1000L, 1010L)]
        [TestCase(1000UL, 1010UL)]
        public void ErrorIfUlpsIsUsedOnIntegralType(object x, object y)
        {
            Assert.That(y, Is.EqualTo(x).Within(2).Ulps);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorIfUlpsIsUsedOnDecimal()
        {
            Assert.That(100m, Is.EqualTo(100m).Within(2).Ulps);
        }

        [Test]
        public void UsesProvidedIComparer()
        {
            SimpleObjectComparer comparer = new SimpleObjectComparer();
            Assert.That(2 + 2, Is.EqualTo(4).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

#if CLR_2_0 || CLR_4_0
        [Test]
        public void UsesProvidedEqualityComparer()
        {
            SimpleEqualityComparer comparer = new SimpleEqualityComparer();
            Assert.That(2 + 2, Is.EqualTo(4).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        [Test]
        public void UsesProvidedEqualityComparerOfT()
        {
            SimpleEqualityComparer<int> comparer = new SimpleEqualityComparer<int>();
            Assert.That(2 + 2, Is.EqualTo(4).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        [Test]
        public void UsesProvidedComparerOfT()
        {
            SimpleEqualityComparer<int> comparer = new SimpleEqualityComparer<int>();
            Assert.That(2 + 2, Is.EqualTo(4).Using(comparer));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        [Test]
        public void UsesProvidedComparisonOfT()
        {
            MyComparison<int> comparer = new MyComparison<int>();
            Assert.That(2 + 2, Is.EqualTo(4).Using(new Comparison<int>(comparer.Compare)));
            Assert.That(comparer.Called, "Comparer was not called");
        }

        class MyComparison<T>
        {
            public bool Called;

            public int Compare(T x, T y)
            {
                Called = true;
                return Comparer<T>.Default.Compare(x, y);
            }
        }

#if !NETCF_2_0
        [Test]
        public void UsesProvidedLambda_IntArgs()
        {
            Assert.That(2 + 2, Is.EqualTo(4).Using<int>((x, y) => x.CompareTo(y)));
        }

        [Test]
        public void UsesProvidedLambda_StringArgs()
        {
            Assert.That("hello", Is.EqualTo("HELLO").Using<string>((x, y) => StringUtil.Compare(x, y, true)));
        }

        [Test]
        public void UsesProvidedListComparer()
        {
            var list1 = new List<int>() { 2, 3 };
            var list2 = new List<int>() { 3, 4 };

            var list11 = new List<List<int>>() { list1 };
            var list22 = new List<List<int>>() { list2 };
            var comparer = new IntListEqualComparer();

            Assert.That(list11, new CollectionEquivalentConstraint(list22).Using(comparer));
        }
#endif

        public class IntListEqualComparer : IEqualityComparer<List<int>>
        {
            public bool Equals(List<int> x, List<int> y)
            {
                return x.Count == y.Count;
            }
 
            public int GetHashCode(List<int> obj)
            {
                return obj.Count.GetHashCode();
            }
        }

#if !NETCF_2_0
        [Test]
        public void UsesProvidedArrayComparer()
        {
            var array1 = new int[] { 2, 3 };
            var array2 = new int[] { 3, 4 };

            var list11 = new List<int[]>() { array1 };
            var list22 = new List<int[]>() { array2 };
            var comparer = new IntArrayEqualComparer();

            Assert.That(list11, new CollectionEquivalentConstraint(list22).Using(comparer));
        }

        public class IntArrayEqualComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y)
            {
                return x.Length == y.Length;
            }

            public int GetHashCode(int[] obj)
            {
                return obj.Length.GetHashCode();
            }
        }
#endif
#endif
    }
}
