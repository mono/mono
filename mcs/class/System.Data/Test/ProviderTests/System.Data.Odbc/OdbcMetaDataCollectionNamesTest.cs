// OdbcMetaDataCollectionNamesTest.cs - NUnit Test Cases for testing the
// OdbcMetaDataCollectionNames Test.
//
// Authors:
//      Amit Biswas (amit@amitbiswas.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
    [TestFixture]
    [Category("odbc")]
    public class OdbcMetaDataCollectionNamesTest
    {
        [Test]
        public void EqualsTest()
        {
            object TestObj1;
            object TestObj2;

            TestObj1 = "Bangalore";
            TestObj2 = "Bangalore";
            Assert.AreEqual(true, OdbcMetaDataCollectionNames.Equals(TestObj1, TestObj2), "#1 Objects with same value");

            TestObj1 = "Bangalore";
            TestObj2 = "Mumbai";
            Assert.AreEqual(false, OdbcMetaDataCollectionNames.Equals(TestObj1, TestObj2), "#2 Objects with different value");

            TestObj1 = null;
            TestObj2 = "Bangalore";
            Assert.AreEqual(false, OdbcMetaDataCollectionNames.Equals(TestObj1, TestObj2), "#3 null to not-null");

            TestObj1 = "Bangalore";
            TestObj2 = null;
            Assert.AreEqual(false, OdbcMetaDataCollectionNames.Equals(TestObj1, TestObj2), "#4 not-null to null");

            TestObj1 = null;
            TestObj2 = null;
            Assert.AreEqual(true, OdbcMetaDataCollectionNames.Equals(TestObj1, TestObj2), "#5 null to null");
        }

    }
}