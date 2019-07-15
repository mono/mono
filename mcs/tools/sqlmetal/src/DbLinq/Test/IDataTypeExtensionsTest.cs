#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections.Generic;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;
using NUnit.Framework;

namespace DbLinqTest
{
    /// <summary>
    ///This is a test class for SchemaLoaderTest and is intended
    ///to contain all SchemaLoaderTest Unit Tests
    ///</summary>
    [TestFixture]
    public class IDataTypeExtensionsTest
    {

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [Test]
        public void UnpackDbType1Test()
        {
            string rawType = "int";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("int", dataType.SqlType);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            //Assert.AreEqual(null, dataType.Unsigned); // irrelevant
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [Test]
        public void UnpackDbType2Test()
        {
            string rawType = "int(12)";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("int", dataType.SqlType);
            Assert.AreEqual(12, dataType.Length);
            Assert.AreEqual(12, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            //Assert.AreEqual(null, dataType.Unsigned); // irrelevant
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [Test]
        public void UnpackDbType3Test()
        {
            string rawType = "number(15,5)";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("number", dataType.SqlType);
            Assert.AreEqual(15, dataType.Length);
            Assert.AreEqual(15, dataType.Precision);
            Assert.AreEqual(5, dataType.Scale);
            Assert.AreEqual(false, dataType.Unsigned);
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [Test]
        public void UnpackDbType4Test()
        {
            string rawType = "type()";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("type", dataType.SqlType);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
        }

        /// <summary>
        ///A test for UnpackDbType
        ///</summary>
        [Test]
        public void UnpackDbType5Test()
        {
            string rawType = "smallint unsigned";
            IDataType dataType = new SchemaLoader.DataType();
            dataType.UnpackRawDbType(rawType);
            Assert.AreEqual("smallint", dataType.SqlType);
            Assert.AreEqual(null, dataType.Length);
            Assert.AreEqual(null, dataType.Precision);
            Assert.AreEqual(null, dataType.Scale);
            Assert.AreEqual(true, dataType.Unsigned);
        }
    }
}
