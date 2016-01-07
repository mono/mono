// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnitLite.Tests
{
    [TestFixture]
    public class SyntaxTests
    {
        [Test]
        public void NullTests()
        {
            object myObject = null;
            Assert.That(myObject, Is.Null);
            Assert.Null(null);
        }

        [Test]
        public void NotNullTests()
        {
            Assert.That(42, Is.Not.Null);
            Assert.NotNull(42);
        }

        [Test]
        public void TrueTests()
        {
            Assert.That(true, Is.True);
            Assert.True(true);
        }

        [Test]
        public void FalseTests()
        {
            Assert.That(false, Is.False);
            Assert.False(false);
        }

        [Test]
        public void NaNTests()
        {
            Assert.That(double.NaN, Is.NaN);
            Assert.That(float.NaN, Is.NaN);
        }

        [Test]
        public void EmptyTests()
        {
            Assert.That("", Is.Empty);
            Assert.That(new bool[0], Is.Empty);
            Assert.That(new int[] { 1, 2, 3 }, Is.Not.Empty);
        }

        [Test]
        public void TypeTests()
        {
            Assert.That("Hello", Is.TypeOf(typeof(string)));
            Assert.That("Hello", Is.InstanceOf(typeof(string)));
            Assert.That("Hello".GetType(), Is.EqualTo(typeof(string)));
            Assert.That("Hello".GetType().FullName, Is.EqualTo("System.String"));
        }

        [Test]
        public void StringTests()
        {
            string phrase = "Hello World!";
            Assert.That(phrase, Is.Not.Empty);
            Assert.That(phrase, Is.StringContaining("World"));
            Assert.That(phrase, Is.StringStarting("Hello"));
            Assert.That(phrase, Is.StringEnding("!"));
            Assert.That(phrase, Is.EqualTo("hello world!").IgnoreCase);
            Assert.That(new string[] { "Hello", "World" }, Is.EqualTo( new object[] { "HELLO", "WORLD" } ).IgnoreCase);
            Assert.That("", Is.Empty);
        }

        [Test]
        public void EqualToTests()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
            Assert.That(2 + 2 == 4);
            Assert.That(new int[] { 1, 2, 3 }, Is.EqualTo(new double[] { 1.0, 2.0, 3.0 }));
        }

        [Test]
        public void ComparisonTests()
        {
            Assert.That(7, Is.GreaterThan(3));
            Assert.That(7, Is.GreaterThanOrEqualTo(3));
            Assert.That(7, Is.AtLeast(3));
            Assert.That(7, Is.GreaterThanOrEqualTo(7));
            Assert.That(7, Is.AtLeast(7));

            Assert.That(3, Is.LessThan(7));
            Assert.That(3, Is.LessThanOrEqualTo(7));
            Assert.That(3, Is.AtMost(7));
            Assert.That(3, Is.LessThanOrEqualTo(3));
            Assert.That(3, Is.AtMost(3));
        }

        [Test]
        public void AllItemsTests()
        {
            object[] c = new object[] { 1, 2, 3, 4 };
            Assert.That(c, Is.All.Not.Null);
            Assert.That(c, Is.All.InstanceOf(typeof(int)));
        }

        [Test]
        public void CollectionContainsTests()
        {
            Assert.That(new int[] { 1, 2, 3 }, Contains.Item(3));
            Assert.That(new string[] { "a", "b", "c" }, Contains.Item("b"));
        }

        [Test]
        public void CollectionEquivalenceTests()
        {
            int[] ints1to5 = new int[] { 1, 2, 3, 4, 5 };
            Assert.That(new int[] { 2, 1, 4, 3, 5 }, Is.EquivalentTo(ints1to5));
            Assert.That(new int[] { 2, 2, 4, 3, 5 }, Is.Not.EquivalentTo(ints1to5));
            Assert.That(new int[] { 2, 4, 3, 5 }, Is.Not.EquivalentTo(ints1to5));
            Assert.That(new int[] { 2, 2, 1, 1, 4, 3, 5 }, Is.Not.EquivalentTo(ints1to5));
            Assert.That(new int[] { 1, 2, 2, 2, 5 }, Is.Not.EquivalentTo(ints1to5));
        }

        [Test]
        public void SubsetTests()
        {
            int[] ints1to5 = new int[] { 1, 2, 3, 4, 5 };
            Assert.That(new int[] { 1, 3, 5 }, Is.SubsetOf(ints1to5));
            Assert.That(new int[] { 1, 2, 3, 4, 5 }, Is.SubsetOf(ints1to5));
            Assert.That(new int[] { 2, 4, 6 }, Is.Not.SubsetOf(ints1to5));
        }

        [Test]
        public void NotTests()
        {
            Assert.That(42, Is.Not.Null);
            Assert.That(42, Is.Not.True);
            Assert.That(42, Is.Not.False);
            Assert.That(42, !Is.Null);
            Assert.That(2.5, Is.Not.NaN);
            Assert.That(2 + 2, Is.Not.EqualTo(3));
            Assert.That(2 + 2, Is.Not.Not.EqualTo(4));
            Assert.That(2 + 2, Is.Not.Not.Not.EqualTo(5));
        }

        [Test]
        public void AndTests()
        {
            Assert.That(7, Is.GreaterThan(5) & Is.LessThan(10));
        }

        [Test]
        public void OrTests()
        {
            Assert.That(3, Is.LessThan(5) | Is.GreaterThan(10));
        }

        [Test]
        public void ComplexTests()
        {
            Assert.That(7, Is.Not.Null & Is.Not.LessThan(5) & Is.Not.GreaterThan(10));
            Assert.That(7, !Is.Null & !Is.LessThan(5) & !Is.GreaterThan(10));
// TODO: Remove #if when mono compiler can handle null
#if MONO
            Constraint x = null;
            Assert.That(7, !x & !Is.LessThan(5) & !Is.GreaterThan(10));
#else
            Assert.That(7, !(Constraint)null & !Is.LessThan(5) & !Is.GreaterThan(10));
#endif
        }

        // This method contains assertions that should not compile
        // You can check by uncommenting it.
        //public void WillNotCompile()
        //{
        //    Assert.That(42, Is.Not);
        //    Assert.That(42, Is.All);
        //    Assert.That(42, Is.Null.Not);
        //    Assert.That(42, Is.Not.Null.GreaterThan(10));
        //    Assert.That(42, Is.GreaterThan(10).LessThan(99));

        //    object[] c = new object[0];
        //    Assert.That(c, Is.Null.All);
        //    Assert.That(c, Is.Not.All);
        //    Assert.That(c, Is.All.Not);
        //}
    }
}
