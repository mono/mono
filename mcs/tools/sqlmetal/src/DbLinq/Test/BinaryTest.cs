#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
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

using System;
using System.Linq;
using System.Text;

using System.Data.Linq;

using NUnit.Framework;

namespace DbLinqTest
{
    [TestFixture]
    public class BinaryTest
    {
        // XXX: oddly, MSDN documents that while ArgumentNullException is 
        //      thrown, that may change in the future.  Why would this change?
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ValueNull()
        {
            new Binary(null);
        }

        [Test]
        public void Constructor()
        {
            new Binary(Encoding.UTF8.GetBytes("hello!"));
        }

        [Test]
        public void Equals()
        {
            Binary a = new Binary(Encoding.UTF8.GetBytes("a"));
            Assert.IsFalse(a.Equals((Binary)null));
            Assert.IsFalse(a.Equals((object)null));
            Assert.IsFalse(a.Equals(new Binary(Encoding.UTF8.GetBytes("b"))));
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(new Binary(Encoding.UTF8.GetBytes("a"))));
        }

        [Test]
        public void Equality()
        {
            Binary a    = new Binary(Encoding.UTF8.GetBytes("a"));
            Binary a2   = new Binary(Encoding.UTF8.GetBytes("a"));
            Binary b    = new Binary(Encoding.UTF8.GetBytes("b"));

            Assert.IsTrue(a == a);
            Assert.IsTrue(a == a2);
            Assert.IsFalse(a == null);
            Assert.IsFalse(null == a);
            Assert.IsFalse(a == b);
            Assert.IsFalse(b == a);

            a = null;
            b = null;
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Inequality()
        {
            Binary a    = new Binary(Encoding.UTF8.GetBytes("a"));
            Binary a2   = new Binary(Encoding.UTF8.GetBytes("a"));
            Binary b    = new Binary(Encoding.UTF8.GetBytes("b"));

            Assert.IsFalse(a != a);
            Assert.IsFalse(a != a2);
            Assert.IsTrue(a != null);
            Assert.IsTrue(null != a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);

            a = null;
            b = null;
            Assert.IsFalse(a != b);
        }

        [Test]
        public void GetHashCode()
        {
            Binary a = new Binary(Encoding.UTF8.GetBytes("a"));
            Binary b = new Binary(Encoding.UTF8.GetBytes("a"));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Length()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, world!");
            Binary b = new Binary(data);
            Assert.AreEqual(data.Length, b.Length);
        }

        [Test]
        public void ToArray()
        {
            byte[] data = Encoding.UTF8.GetBytes("is the array copied?  Yes.");
            Binary b = new Binary(data);
            Assert.IsTrue(data.SequenceEqual(b.ToArray()));

            data[0] = (byte) 'I';
            Assert.IsFalse(data.SequenceEqual(b.ToArray()));
        }

        [Test]
        public new void ToString()
        {
            byte[] data = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            Binary b = new Binary(data);
            Assert.AreEqual('"' + Convert.ToBase64String(data) + '"', b.ToString());
        }
    }
}
