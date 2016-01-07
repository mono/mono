// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnitLite.Tests
{
    class MessageWriterTests
    {
        protected TextMessageWriter writer;

        [SetUp]
        public void SetUp()
        {
            writer = new TextMessageWriter();
        }
    }

    [TestFixture]
    class TestMessageWriterTests : MessageWriterTests
    {
        [Test]
        public void ConnectorIsWrittenWithSurroundingSpaces()
        {
            writer.WriteConnector("and");
            Assert.That(writer.ToString(), Is.EqualTo(" and "));
        }

        [Test]
        public void PredicateIsWrittenWithTrailingSpace()
        {
            writer.WritePredicate("contains");
            Assert.That(writer.ToString(), Is.EqualTo("contains "));
        }

        [TestFixture]
        public class ExpectedValueTests : ValueTests
        {
            protected override void WriteValue(object obj)
            {
                writer.WriteExpectedValue(obj);
            }
        }

        [TestFixture]
        public class ActualValueTests : ValueTests
        {
            protected override void WriteValue(object obj)
            {
                writer.WriteActualValue( obj );
            }
        }

        public abstract class ValueTests : MessageWriterTests
        {
            protected abstract void WriteValue( object obj);

            [Test]
            public void IntegerIsWrittenAsIs()
            {
                WriteValue(42);
                Assert.That(writer.ToString(), Is.EqualTo("42"));
            }

            [Test]
            public void StringIsWrittenWithQuotes()
            {
                WriteValue("Hello");
                Assert.That(writer.ToString(), Is.EqualTo("\"Hello\""));
            }

            //[Test]
            //public void ControlCharactersInStringsAreEscaped()
            //{
            //    WriteValue("Best Wishes,\r\n\tCharlie\r\n");
            //    Assert.That(writer.ToString(), Is.EqualTo("\"Best Wishes,\\r\\n\\tCharlie\\r\\n\""));
            //}

            [Test]
            public void FloatIsWrittenWithTrailingF()
            {
                WriteValue(0.5f);
                Assert.That(writer.ToString(), Is.EqualTo("0.5f"));
            }

            [Test]
            public void FloatIsWrittenToNineDigits()
            {
                WriteValue(0.33333333333333f);
                int digits = writer.ToString().Length - 3;   // 0.dddddddddf
                Assert.That(digits, Is.EqualTo(9));
            }

            [Test]
            public void DoubleIsWrittenWithTrailingD()
            {
                WriteValue(0.5d);
                Assert.That(writer.ToString(), Is.EqualTo("0.5d"));
            }

            [Test]
            public void DoubleIsWrittenToSeventeenDigits()
            {
                WriteValue(0.33333333333333333333333333333333333333333333d);
                int digits = writer.ToString().Length - 3;
                Assert.That(digits, Is.EqualTo(17));
            }

            [Test]
            public void DecimalIsWrittenWithTrailingM()
            {
                WriteValue(0.5m);
                Assert.That(writer.ToString(), Is.EqualTo("0.5m"));
            }

            [Test]
            public void DecimalIsWrittenToTwentyNineDigits()
            {
                WriteValue(12345678901234567890123456789m);
                Assert.That(writer.ToString(), Is.EqualTo("12345678901234567890123456789m"));
            }
        }
    }
}
