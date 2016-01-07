// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org.
// ****************************************************************
using System;

namespace NUnit.Framework.Tests
{
#if CLR_2_0 || CLR_4_0
#if !MONO
    [TestFixture, Category("Generics")]
    public class NullableTypesTests
    {
        [Test]
        public void CanTestForNull()
        {
            int? nullInt = null;
            int? five = 5;

            Assert.IsNull(nullInt);
            Assert.IsNotNull(five);
            Assert.That(nullInt, Is.Null);
            Assert.That(five, Is.Not.Null);
        }

#if false
        [Test]
        public void CanCompareNullableInts()
        {
            int? five = 5;
            int? answer = 2 + 3;

            Assert.AreEqual(five, answer);
            Assert.AreEqual(five, 5);
            Assert.AreEqual(5, five);

            Assert.That(five, Is.EqualTo(answer));
            Assert.That(five, Is.EqualTo(5));
            Assert.That(5, Is.EqualTo(five));

            Assert.Greater(five, 3);
            Assert.GreaterOrEqual(five, 5);
            Assert.Less(3, five);
            Assert.LessOrEqual(5, five);

            Assert.That(five, Is.GreaterThan(3));
            Assert.That(five, Is.GreaterThanOrEqualTo(5));
            //Assert.That(3, Is.LessThan(five));
            //Assert.That(5, Is.LessThanOrEqualTo(five));
        }
		
        [Test]
        public void CanCompareNullableDoubles()
        {
            double? five = 5.0;
            double? answer = 2.0 + 3.0;

            Assert.AreEqual(five, answer);
            Assert.AreEqual(five, 5.0);
            Assert.AreEqual(5.0, five);

            Assert.That(five, Is.EqualTo(answer));
            Assert.That(five, Is.EqualTo(5.0));
            Assert.That(5.0, Is.EqualTo(five));

            Assert.Greater(five, 3.0);
            Assert.GreaterOrEqual(five, 5.0);
            Assert.Less(3.0, five);
            Assert.LessOrEqual(5.0, five);

            Assert.That(five, Is.GreaterThan(3.0));
            Assert.That(five, Is.GreaterThanOrEqualTo(5.0));
            //Assert.That(3.0, Is.LessThan(five));
            //Assert.That(5.0, Is.LessThanOrEqualTo(five));
        }
#endif

        [Test]
        public void CanTestForNaN()
        {
            double? anNaN = Double.NaN;
            Assert.That(anNaN, Is.NaN);
        }

#if false
        [Test]
        public void CanCompareNullableDecimals()
        {
            decimal? five = 5m;
            decimal? answer = 2m + 3m;

            Assert.AreEqual(five, answer);
            Assert.AreEqual(five, 5m);
            Assert.AreEqual(5m, five);

            Assert.That(five, Is.EqualTo(answer));
            Assert.That(five, Is.EqualTo(5m));
            Assert.That(5m, Is.EqualTo(five));

            Assert.Greater(five, 3m);
            Assert.GreaterOrEqual(five, 5m);
            Assert.Less(3m, five);
            Assert.LessOrEqual(5m, five);

            Assert.That(five, Is.GreaterThan(3m));
            Assert.That(five, Is.GreaterThanOrEqualTo(5m));
            //Assert.That(3m, Is.LessThan(five));
            //Assert.That(5m, Is.LessThanOrEqualTo(five));
        }
#endif
		
        [Test]
        public void CanCompareWithTolerance()
        {
            double? five = 5.0;

            Assert.AreEqual(5.0000001, five, .0001); 
            Assert.That( five, Is.EqualTo(5.0000001).Within(.0001));

            float? three = 3.0f;

            Assert.AreEqual(3.00001f, three, .001);
            Assert.That( three, Is.EqualTo(3.00001f).Within(.001));
        }

        private enum Colors
        {
            Red,
            Blue,
            Green
        }

        [Test]
        public void CanCompareNullableEnums()
        {
            Colors? color = Colors.Red;
            Colors? other = Colors.Red;

            Assert.AreEqual(color, other);
            Assert.AreEqual(color, Colors.Red);
            Assert.AreEqual(Colors.Red, color);
        }

        [Test]
        public void CanCompareNullableMixedNumerics()
        {
            int? int5 = 5;
            double? double5 = 5.0;
            decimal? decimal5 = 5.00m;

            Assert.AreEqual(int5, double5);
            Assert.AreEqual(int5, decimal5);
            Assert.AreEqual(double5, int5);
            Assert.AreEqual(double5, decimal5);
            Assert.AreEqual(decimal5, int5);
            Assert.AreEqual(decimal5, double5);

            Assert.That(int5, Is.EqualTo(double5));
            Assert.That(int5, Is.EqualTo(decimal5));
            Assert.That(double5, Is.EqualTo(int5));
            Assert.That(double5, Is.EqualTo(decimal5));
            Assert.That(decimal5, Is.EqualTo(int5));
            Assert.That(decimal5, Is.EqualTo(double5));

            Assert.AreEqual(5, double5);
            Assert.AreEqual(5, decimal5);
            Assert.AreEqual(5.0, int5);
            Assert.AreEqual(5.0, decimal5);
            Assert.AreEqual(5m, int5);
            Assert.AreEqual(5m, double5);

            Assert.That(5, Is.EqualTo(double5));
            Assert.That(5, Is.EqualTo(decimal5));
            Assert.That(5.0, Is.EqualTo(int5));
            Assert.That(5.0, Is.EqualTo(decimal5));
            Assert.That(5m, Is.EqualTo(int5));
            Assert.That(5m, Is.EqualTo(double5));

            Assert.AreEqual(double5, 5);
            Assert.AreEqual(decimal5, 5);
            Assert.AreEqual(int5, 5.0);
            Assert.AreEqual(decimal5, 5.0);
            Assert.AreEqual(int5, 5m);
            Assert.AreEqual(double5, 5m);

            Assert.That(double5, Is.EqualTo(5));
            Assert.That(decimal5, Is.EqualTo(5));
            Assert.That(int5, Is.EqualTo(5.0));
            Assert.That(decimal5, Is.EqualTo(5.0));
            Assert.That(int5, Is.EqualTo(5m));
            Assert.That(double5, Is.EqualTo(5m));

#if false
            Assert.Greater(int5, 3.0);
            Assert.Greater(int5, 3m);
            Assert.Greater(double5, 3);
            Assert.Greater(double5, 3m);
            Assert.Greater(decimal5, 3);
            Assert.Greater(decimal5, 3.0);

            Assert.That(int5, Is.GreaterThan(3.0));
            Assert.That(int5, Is.GreaterThan(3m));
            Assert.That(double5, Is.GreaterThan(3));
            Assert.That(double5, Is.GreaterThan(3m));
            Assert.That(decimal5, Is.GreaterThan(3));
            Assert.That(decimal5, Is.GreaterThan(3.0));

            Assert.Less(3.0, int5);
            Assert.Less(3m, int5);
            Assert.Less(3, double5);
            Assert.Less(3m, double5);
            Assert.Less(3, decimal5);
            Assert.Less(3.0, decimal5);
#endif
            //Assert.That(3.0, Is.LessThan(int5));
            //Assert.That(3m, Is.LessThan(int5));
            //Assert.That(3, Is.LessThan(double5));
            //Assert.That(3m, Is.LessThan(double5));
            //Assert.That(3, Is.LessThan(decimal5));
            //Assert.That(3.0, Is.LessThan(decimal5));
        }

        private struct MyStruct
        {
            int i;
            string s;

            public MyStruct(int i, string s)
            {
                this.i = i;
                this.s = s;
            }
        }

        [Test]
        public void CanCompareNullableStructs()
        {
            MyStruct struct1 = new MyStruct(5, "Hello");
            MyStruct struct2 = new MyStruct(5, "Hello");
            Nullable<MyStruct> one = new MyStruct(5, "Hello");
            Nullable<MyStruct> two = new MyStruct(5, "Hello");

            Assert.AreEqual(struct1, struct2); // Control
            Assert.AreEqual(one, two);
            Assert.AreEqual(one, struct1);
            Assert.AreEqual(struct2, two);
        }
    }
#endif
#endif
}
