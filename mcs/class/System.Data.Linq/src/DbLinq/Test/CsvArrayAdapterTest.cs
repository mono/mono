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

using System.Linq;
using DbLinq.Schema.Dbml.Adapter;
using DbLinq.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DbLinqTest
{
    /// <summary>
    /// Summary description for TypeContextTest
    /// </summary>
    [TestFixture]
    [TestClass]
    public class CsvArrayAdapterTest
    {
        public class CsvArray
        {
            public string S;
            public ISimpleList<string> A;

            public CsvArray()
            {
                A = new CsvArrayAdapter(this, "S");
            }
        }

        [TestMethod]
        [Test]
        public void ArrayTest()
        {
            var ca = new CsvArray { S = "a,b" };
            var al = ca.A.ToArray();
            Assert.AreEqual(2, al.Count());
            Assert.AreEqual("a", al[0]);
            Assert.AreEqual("b", al[1]);
        }

        [TestMethod]
        [Test]
        public void WriteArrayTest()
        {
            var ca = new CsvArray { S = "a,b" };
            ca.A.Add("c");
            Assert.AreEqual("a,b,c", ca.S);
        }
    }
}
