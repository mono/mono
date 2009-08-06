/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using C5;
using NUnit.Framework;
using SCG = System.Collections.Generic;

namespace C5UnitTests.RecordsTests
{
  [TestFixture]
  public class Basic
  {
    [SetUp]
    public void Init()
    {
    }
    [Test]
    public void FourElement()
    {
      Rec<string, string, int, int> rec1, rec2, rec3;
      rec1 = new Rec<string, string, int, int>("abe", null, 0, 1);
      rec2 = new Rec<string, string, int, int>("abe", null, 0, 1);
      rec3 = new Rec<string, string, int, int>("abe", "kat", 0, 1);
      Assert.IsTrue(rec1 == rec2);
      Assert.IsFalse(rec1 != rec2);
      Assert.IsFalse(rec1 == rec3);
      Assert.IsTrue(rec1 != rec3);
      Assert.IsTrue(rec1.Equals(rec2));
      Assert.IsFalse(rec1.Equals(rec3));
      //
      Assert.IsFalse(rec1.Equals(null));
      Assert.IsFalse(rec1.Equals("bamse"));
      //
      Assert.IsTrue(rec1.GetHashCode() == rec2.GetHashCode());
      Assert.IsFalse(rec1.GetHashCode() == rec3.GetHashCode());
      //
      Assert.AreEqual("abe", rec1.X1);
      Assert.IsNull(rec1.X2);
      Assert.AreEqual(0, rec1.X3);
      Assert.AreEqual(1, rec1.X4);
    }


    [Test]
    public void ThreeElement()
    {
      Rec<string, string, int> rec1, rec2, rec3;
      rec1 = new Rec<string, string, int>("abe", null, 0);
      rec2 = new Rec<string, string, int>("abe", null, 0);
      rec3 = new Rec<string, string, int>("abe", "kat", 0);
      Assert.IsTrue(rec1 == rec2);
      Assert.IsFalse(rec1 != rec2);
      Assert.IsFalse(rec1 == rec3);
      Assert.IsTrue(rec1 != rec3);
      Assert.IsTrue(rec1.Equals(rec2));
      Assert.IsFalse(rec1.Equals(rec3));
      //
      Assert.IsFalse(rec1.Equals(null));
      Assert.IsFalse(rec1.Equals("bamse"));
      //
      Assert.IsTrue(rec1.GetHashCode() == rec2.GetHashCode());
      Assert.IsFalse(rec1.GetHashCode() == rec3.GetHashCode());
      //
      Assert.AreEqual("abe", rec1.X1);
      Assert.IsNull(rec1.X2);
      Assert.AreEqual(0, rec1.X3);

    }

    [Test]
    public void TwoElement()
    {
      Rec<string, string> rec1, rec2, rec3;
      rec1 = new Rec<string, string>("abe", null);
      rec2 = new Rec<string, string>("abe", null);
      rec3 = new Rec<string, string>("abe", "kat");
      Assert.IsTrue(rec1 == rec2);
      Assert.IsFalse(rec1 != rec2);
      Assert.IsFalse(rec1 == rec3);
      Assert.IsTrue(rec1 != rec3);
      Assert.IsTrue(rec1.Equals(rec2));
      Assert.IsFalse(rec1.Equals(rec3));
      //
      Assert.IsFalse(rec1.Equals(null));
      Assert.IsFalse(rec1.Equals("bamse"));
      //
      Assert.IsTrue(rec1.GetHashCode() == rec2.GetHashCode());
      Assert.IsFalse(rec1.GetHashCode() == rec3.GetHashCode());
      //
      Assert.AreEqual("abe", rec1.X1);
      Assert.IsNull(rec1.X2);
    }

    [TearDown]
    public void Dispose()
    {
    }
  }

}