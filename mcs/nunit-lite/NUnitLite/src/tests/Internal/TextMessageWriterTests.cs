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
using System.Threading;
using System.Globalization;

namespace NUnit.Framework.Internal
{
    [TestFixture]
    public class TextMessageWriterTests : AssertionHelper
    {
        private static readonly string NL = NUnit.Env.NewLine;

        private TextMessageWriter writer;

		[SetUp]
		public void SetUp()
        {
            writer = new TextMessageWriter();
        }

        [Test]
        public void ConnectorIsWrittenWithSurroundingSpaces()
        {
            writer.WriteConnector("and");
            Expect(writer.ToString(), EqualTo(" and "));
        }

        [Test]
        public void PredicateIsWrittenWithTrailingSpace()
        {
            writer.WritePredicate("contains");
            Expect(writer.ToString(), EqualTo("contains "));
        }

        [Test]
        public void IntegerIsWrittenAsIs()
        {
            writer.WriteValue(42);
            Expect(writer.ToString(), EqualTo("42"));
        }

        [Test]
        public void StringIsWrittenWithQuotes()
        {
            writer.WriteValue("Hello");
            Expect(writer.ToString(), EqualTo("\"Hello\""));
        }

		// This test currently fails because control character replacement is
		// done at a higher level...
		// TODO: See if we should do it at a lower level
//            [Test]
//            public void ControlCharactersInStringsAreEscaped()
//            {
//                WriteValue("Best Wishes,\r\n\tCharlie\r\n");
//                Assert.That(writer.ToString(), Is.EqualTo("\"Best Wishes,\\r\\n\\tCharlie\\r\\n\""));
//            }

        [Test]
        public void FloatIsWrittenWithTrailingF()
        {
            writer.WriteValue(0.5f);
            Expect(writer.ToString(), EqualTo("0.5f"));
        }

        [Test]
        public void FloatIsWrittenToNineDigits()
        {
            writer.WriteValue(0.33333333333333f);
            int digits = writer.ToString().Length - 3;   // 0.dddddddddf
            Expect(digits, EqualTo(9));
            Expect(writer.ToString().Length, EqualTo(12));
        }

        [Test]
        public void DoubleIsWrittenWithTrailingD()
        {
            writer.WriteValue(0.5d);
            Expect(writer.ToString(), EqualTo("0.5d"));
        }

        [Test]
        public void DoubleIsWrittenToSeventeenDigits()
        {
            writer.WriteValue(0.33333333333333333333333333333333333333333333d);
            Expect(writer.ToString().Length, EqualTo(20)); // add 3 for leading 0, decimal and trailing d
        }

        [Test]
        public void DecimalIsWrittenWithTrailingM()
        {
            writer.WriteValue(0.5m);
            Expect(writer.ToString(), EqualTo("0.5m"));
        }

        [Test]
        public void DecimalIsWrittenToTwentyNineDigits()
        {
            writer.WriteValue(12345678901234567890123456789m);
            Expect(writer.ToString(), EqualTo("12345678901234567890123456789m"));
        }

		[Test]
		public void DateTimeTest()
		{
            writer.WriteValue(new DateTime(2007, 7, 4, 9, 15, 30, 123));
            Expect(writer.ToString(), EqualTo("2007-07-04 09:15:30.123"));
		}

        [Test]
        public void DisplayStringDifferences()
        {
            string s72 = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string exp = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXY...";

            writer.DisplayStringDifferences(s72, "abcde", 5, false, true);
            string message = writer.ToString();
            Expect(message, EqualTo(
                TextMessageWriter.Pfx_Expected + Q(exp) + NL +
                TextMessageWriter.Pfx_Actual + Q("abcde") + NL +
                "  ----------------^" + NL));
        }

        [Test]
        public void DisplayStringDifferences_NoClipping()
        {
            string s72 = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            writer.DisplayStringDifferences(s72, "abcde", 5, false, false);
            string message = writer.ToString();
            Expect(message, EqualTo(
                TextMessageWriter.Pfx_Expected + Q(s72) + NL +
                TextMessageWriter.Pfx_Actual + Q("abcde") + NL +
                "  ----------------^" + NL));
        }

        private string Q(string s)
        {
            return "\"" + s + "\"";
        }
    }
}
