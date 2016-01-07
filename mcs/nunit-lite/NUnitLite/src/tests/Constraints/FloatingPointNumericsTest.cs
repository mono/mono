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

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class FloatingPointNumericsTest
    {

        /// <summary>Tests the floating point value comparison helper</summary>
        [Test]
        public void FloatEqualityWithUlps()
        {
            Assert.IsTrue(
                FloatingPointNumerics.AreAlmostEqualUlps(0.00000001f, 0.0000000100000008f, 1)
            );
            Assert.IsFalse(
                FloatingPointNumerics.AreAlmostEqualUlps(0.00000001f, 0.0000000100000017f, 1)
            );

            Assert.IsTrue(
                FloatingPointNumerics.AreAlmostEqualUlps(1000000.00f, 1000000.06f, 1)
            );
            Assert.IsFalse(
                FloatingPointNumerics.AreAlmostEqualUlps(1000000.00f, 1000000.13f, 1)
            );
        }

        /// <summary>Tests the double precision floating point value comparison helper</summary>
        [Test]
        public void DoubleEqualityWithUlps()
        {
            Assert.IsTrue(
                FloatingPointNumerics.AreAlmostEqualUlps(0.00000001, 0.000000010000000000000002, 1)
            );
            Assert.IsFalse(
                FloatingPointNumerics.AreAlmostEqualUlps(0.00000001, 0.000000010000000000000004, 1)
            );

            Assert.IsTrue(
                FloatingPointNumerics.AreAlmostEqualUlps(1000000.00, 1000000.0000000001, 1)
            );
            Assert.IsFalse(
                FloatingPointNumerics.AreAlmostEqualUlps(1000000.00, 1000000.0000000002, 1)
            );
        }

        /// <summary>Tests the integer reinterpretation functions</summary>
        [Test]
        public void MirroredIntegerReinterpretation()
        {
            Assert.AreEqual(
                12345.0f,
                FloatingPointNumerics.ReinterpretAsFloat(
                    FloatingPointNumerics.ReinterpretAsInt(12345.0f)
                )
            );
        }

        /// <summary>Tests the long reinterpretation functions</summary>
        [Test]
        public void MirroredLongReinterpretation()
        {
            Assert.AreEqual(
                12345.67890,
                FloatingPointNumerics.ReinterpretAsDouble(
                    FloatingPointNumerics.ReinterpretAsLong(12345.67890)
                )
            );
        }

        /// <summary>Tests the floating point reinterpretation functions</summary>
        [Test]
        public void MirroredFloatReinterpretation()
        {
            Assert.AreEqual(
                12345,
                FloatingPointNumerics.ReinterpretAsInt(
                    FloatingPointNumerics.ReinterpretAsFloat(12345)
                )
            );
        }


        /// <summary>
        ///   Tests the double prevision floating point reinterpretation functions
        /// </summary>
        [Test]
        public void MirroredDoubleReinterpretation()
        {
            Assert.AreEqual(
                1234567890,
                FloatingPointNumerics.ReinterpretAsLong(
                    FloatingPointNumerics.ReinterpretAsDouble(1234567890)
                )
            );
        }

  }
}
