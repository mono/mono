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
using NUnit.Framework.Internal;
using NUnit.TestUtilities;

namespace NUnit.Framework.Assertions
{
	/// <summary>
	/// Summary description for ArrayEqualsFailureMessageFixture.
	/// </summary>
    [TestFixture]
    public class ArrayEqualsFailureMessageFixture : MessageChecker
    {
        [Test, ExpectedException(typeof(AssertionException))]
        public void ArraysHaveDifferentRanks()
        {
            int[] expected = new int[] { 1, 2, 3, 4 };
            int[,] actual = new int[,] { { 1, 2 }, { 3, 4 } };

            expectedMessage =
                "  Expected is <System.Int32[4]>, actual is <System.Int32[2,2]>" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ExpectedArrayIsLonger()
        {
            int[] expected = new int[] { 1, 2, 3, 4, 5 };
            int[] actual = new int[] { 1, 2, 3 };

            expectedMessage =
                "  Expected is <System.Int32[5]>, actual is <System.Int32[3]>" + NL +
                "  Values differ at index [3]" + NL +
                "  Missing:  < 4, 5 >";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ActualArrayIsLonger()
        {
            int[] expected = new int[] { 1, 2, 3 };
            int[] actual = new int[] { 1, 2, 3, 4, 5, 6, 7 };

            expectedMessage =
                "  Expected is <System.Int32[3]>, actual is <System.Int32[7]>" + NL +
                "  Values differ at index [3]" + NL +
                "  Extra:    < 4, 5, 6... >";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureOnSingleDimensionedArrays()
        {
            int[] expected = new int[] { 1, 2, 3 };
            int[] actual = new int[] { 1, 5, 3 };

            expectedMessage =
                "  Expected and actual are both <System.Int32[3]>" + NL +
                "  Values differ at index [1]" + NL +
                TextMessageWriter.Pfx_Expected + "2" + NL +
                TextMessageWriter.Pfx_Actual + "5" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void DoubleDimensionedArrays()
        {
            int[,] expected = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            int[,] actual = new int[,] { { 1, 3, 2 }, { 4, 0, 6 } };

            expectedMessage =
                "  Expected and actual are both <System.Int32[2,3]>" + NL +
                "  Values differ at index [0,1]" + NL +
                TextMessageWriter.Pfx_Expected + "2" + NL +
                TextMessageWriter.Pfx_Actual + "3" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void TripleDimensionedArrays()
        {
            int[, ,] expected = new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };
            int[, ,] actual = new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 0, 6 }, { 7, 8 } } };

            expectedMessage =
                "  Expected and actual are both <System.Int32[2,2,2]>" + NL +
                "  Values differ at index [1,0,0]" + NL +
                TextMessageWriter.Pfx_Expected + "5" + NL +
                TextMessageWriter.Pfx_Actual + "0" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FiveDimensionedArrays()
        {
            int[, , , ,] expected = new int[2, 2, 2, 2, 2] { { { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } } }, { { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } } } };
            int[, , , ,] actual = new int[2, 2, 2, 2, 2] { { { { { 1, 2 }, { 4, 3 } }, { { 5, 6 }, { 7, 8 } } }, { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } } }, { { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } } } };

            expectedMessage =
                "  Expected and actual are both <System.Int32[2,2,2,2,2]>" + NL +
                "  Values differ at index [0,0,0,1,0]" + NL +
                TextMessageWriter.Pfx_Expected + "3" + NL +
                TextMessageWriter.Pfx_Actual + "4" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void JaggedArrays()
        {
            int[][] expected = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6, 7 }, new int[] { 8, 9 } };
            int[][] actual = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 0, 7 }, new int[] { 8, 9 } };

            expectedMessage =
                "  Expected and actual are both <System.Int32[3][]>" + NL +
                "  Values differ at index [1]" + NL +
                "    Expected and actual are both <System.Int32[4]>" + NL +
                "    Values differ at index [2]" + NL +
                TextMessageWriter.Pfx_Expected + "6" + NL +
                TextMessageWriter.Pfx_Actual + "0" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void JaggedArrayComparedToSimpleArray()
        {
            int[] expected = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[][] actual = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 0, 7 }, new int[] { 8, 9 } };

            expectedMessage =
                "  Expected is <System.Int32[9]>, actual is <System.Int32[3][]>" + NL +
                "  Values differ at index [0]" + NL +
                TextMessageWriter.Pfx_Expected + "1" + NL +
                TextMessageWriter.Pfx_Actual + "< 1, 2, 3 >" + NL;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ArraysWithDifferentRanksAsCollection()
        {
            int[] expected = new int[] { 1, 2, 3, 4 };
            int[,] actual = new int[,] { { 1, 0 }, { 3, 4 } };

            expectedMessage =
                "  Expected is <System.Int32[4]>, actual is <System.Int32[2,2]>" + NL +
                "  Values differ at expected index [1], actual index [0,1]" + NL +
                TextMessageWriter.Pfx_Expected + "2" + NL +
                TextMessageWriter.Pfx_Actual + "0" + NL;
            Assert.That(actual, Is.EqualTo(expected).AsCollection);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ArraysWithDifferentDimensionsAsCollection()
        {
            int[,] expected = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            int[,] actual = new int[,] { { 1, 2 }, { 3, 0 }, { 5, 6 } };

            expectedMessage =
                "  Expected is <System.Int32[2,3]>, actual is <System.Int32[3,2]>" + NL +
                "  Values differ at expected index [1,0], actual index [1,1]" + NL +
                TextMessageWriter.Pfx_Expected + "4" + NL +
                TextMessageWriter.Pfx_Actual + "0" + NL;
            Assert.That(actual, Is.EqualTo(expected).AsCollection);
        }

        //		[Test,ExpectedException(typeof(AssertionException))]
        //		public void ExpectedArrayIsLonger()
        //		{
        //			string[] array1 = { "one", "two", "three" };
        //			string[] array2 = { "one", "two", "three", "four", "five" };
        //
        //			expectedMessage =
        //				"  Expected is <System.String[5]>, actual is <System.String[3]>" + NL +
        //				"  Values differ at index [3]" + NL +
        //				"  Missing:  < \"four\", \"five\" >";
        //			Assert.That(array1, Is.EqualTo(array2));
        //		}

        [Test, ExpectedException(typeof(AssertionException))]
        public void SameLengthDifferentContent()
        {
            string[] array1 = { "one", "two", "three" };
            string[] array2 = { "one", "two", "ten" };

            expectedMessage =
                "  Expected and actual are both <System.String[3]>" + NL +
                "  Values differ at index [2]" + NL +
                "  Expected string length 3 but was 5. Strings differ at index 1." + NL +
                "  Expected: \"ten\"" + NL +
                "  But was:  \"three\"" + NL +
                "  ------------^" + NL;
            Assert.That(array1, Is.EqualTo(array2));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ArraysDeclaredAsDifferentTypes()
        {
            string[] array1 = { "one", "two", "three" };
            object[] array2 = { "one", "three", "two" };

            expectedMessage =
                "  Expected is <System.Object[3]>, actual is <System.String[3]>" + NL +
                "  Values differ at index [1]" + NL +
                "  Expected string length 5 but was 3. Strings differ at index 1." + NL +
                "  Expected: \"three\"" + NL +
                "  But was:  \"two\"" + NL +
                "  ------------^" + NL;
            Assert.That(array1, Is.EqualTo(array2));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void ArrayAndCollection_Failure()
        {
            int[] a = new int[] { 1, 2, 3 };
            ICollection b = new SimpleObjectCollection(1, 3);
            Assert.AreEqual(a, b);
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void DifferentArrayTypesEqualFails()
        {
            string[] array1 = { "one", "two", "three" };
            object[] array2 = { "one", "three", "two" };

            expectedMessage =
                "  Expected is <System.String[3]>, actual is <System.Object[3]>" + NL +
                "  Values differ at index [1]" + NL +
                "  Expected string length 3 but was 5. Strings differ at index 1." + NL +
                "  Expected: \"two\"" + NL +
                "  But was:  \"three\"" + NL +
                "  ------------^" + NL;
            Assert.AreEqual(array1, array2);
        }
    }
}
