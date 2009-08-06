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
namespace C5UnitTests.arrays.circularqueue
{
  using CollectionOfInt = CircularQueue<int>;

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestEvents()
    {
      Fun<CollectionOfInt> factory = delegate() { return new CollectionOfInt(); };
      new C5UnitTests.Templates.Events.QueueTester<CollectionOfInt>().Test(factory);
      new C5UnitTests.Templates.Events.StackTester<CollectionOfInt>().Test(factory);
    }

    [Test]
    public void Extensible()
    {
      //TODO: Test Circular Queue for Clone(?) and Serializable 
      //C5UnitTests.Templates.Extensible.Clone.Tester<CollectionOfInt>();
      //C5UnitTests.Templates.Extensible.Serialization.Tester<CollectionOfInt>();
    }
  }

  //[TestFixture]
  public class Template
  {
    private CircularQueue<int> queue;

    [SetUp]
    public void Init()
    {
      queue = new CircularQueue<int>();
    }

    [Test]
    public void LeTest()
    {
    }

    [TearDown]
    public void Dispose() { queue = null; }

  }

  [TestFixture]
  public class Formatting
  {
    CircularQueue<int> coll;
    IFormatProvider rad16;
    [SetUp]
    public void Init() { coll = new CircularQueue<int>(); rad16 = new RadixFormatProvider(16); }
    [TearDown]
    public void Dispose() { coll = null; rad16 = null; }
    [Test]
    public void Format()
    {
      Assert.AreEqual("{  }", coll.ToString());
      foreach (int i in new int[] { -4, 28, 129, 65530 })
        coll.Enqueue(i);
      Assert.AreEqual("{ -4, 28, 129, 65530 }", coll.ToString());
      Assert.AreEqual("{ -4, 1C, 81, FFFA }", coll.ToString(null, rad16));
      Assert.AreEqual("{ -4, 28, 129... }", coll.ToString("L14", null));
      Assert.AreEqual("{ -4, 1C, 81... }", coll.ToString("L14", rad16));
    }
  }

  [TestFixture]
  public class CircularQueue
  {
    private CircularQueue<int> queue;

    [SetUp]
    public void Init()
    {
      queue = new CircularQueue<int>();
    }

    void loadup1()
    {
      queue.Enqueue(11);
      queue.Enqueue(12);
      queue.Enqueue(13);
      queue.Dequeue();
      queue.Enqueue(103);
      queue.Enqueue(14);
      queue.Enqueue(15);
    }

    void loadup2()
    {
      loadup1();
      for (int i = 0; i < 4; i++)
      {
        queue.Dequeue();
        queue.Enqueue(1000 + i);
      }
    }

    void loadup3()
    {
      for (int i = 0; i < 18; i++)
      {
        queue.Enqueue(i);
        Assert.IsTrue(queue.Check());
      }
      for (int i = 0; i < 14; i++)
      {
        Assert.IsTrue(queue.Check());
        queue.Dequeue();
      }
    }

    [Test]
    public void Expand()
    {
      Assert.IsTrue(queue.Check());
      loadup3();
      Assert.IsTrue(IC.eq(queue, 14, 15, 16, 17));
    }

    [Test]
    public void Simple()
    {
      loadup1();
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(5, queue.Count);
      Assert.IsTrue(IC.eq(queue, 12, 13, 103, 14, 15));
      Assert.AreEqual(12, queue.Choose());
    }

    [Test]
    public void Stack()
    {
      queue.Push(1);
      Assert.IsTrue(queue.Check());
      queue.Push(2);
      Assert.IsTrue(queue.Check());
      queue.Push(3);
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(3, queue.Pop());
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(2, queue.Pop());
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(1, queue.Pop());
      Assert.IsTrue(queue.Check());
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadChoose()
    {
      queue.Choose();
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadDequeue()
    {
      queue.Dequeue();
    }

    [Test]
    public void Simple2()
    {
      loadup2();
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(5, queue.Count);
      Assert.IsTrue(IC.eq(queue, 15, 1000, 1001, 1002, 1003));
      Assert.AreEqual(15, queue.Choose());
    }

    [Test]
    public void Counting()
    {
      Assert.IsTrue(queue.IsEmpty);
      Assert.AreEqual(0, queue.Count);
      Assert.AreEqual(Speed.Constant, queue.CountSpeed);
      queue.Enqueue(11);
      Assert.IsFalse(queue.IsEmpty);
      queue.Enqueue(12);
      Assert.AreEqual(2, queue.Count);
    }

    //This test by Steve Wallace uncovered a bug in the indexing.
    [Test]
    public void SW200602()
    {
      C5.CircularQueue<int> list = new C5.CircularQueue<int>(8);
      for (int count = 0; count <= 7; count++)
      {
        list.Enqueue(count);
      }
      int end = list.Count;
      for (int index = 0; index < end; index++)
      {
        Assert.AreEqual(index, list[0]);
        list.Dequeue();
      }
    }

    [TearDown]
    public void Dispose() { queue = null; }

  }
}