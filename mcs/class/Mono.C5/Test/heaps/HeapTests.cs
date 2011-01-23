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

namespace C5UnitTests.heaps
{
  using CollectionOfInt = IntervalHeap<int>;

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestEvents()
    {
      Fun<CollectionOfInt> factory = delegate() { return new CollectionOfInt(TenEqualityComparer.Default); };
      new C5UnitTests.Templates.Events.PriorityQueueTester<CollectionOfInt>().Test(factory);
    }

    [Test]
    public void Extensible()
    {
      C5UnitTests.Templates.Extensible.Clone.Tester<CollectionOfInt>();
      C5UnitTests.Templates.Extensible.Serialization.Tester<CollectionOfInt>();
    }
  }

  [TestFixture]
  public class Events
  {
    IPriorityQueue<int> queue;
    ArrayList<KeyValuePair<Acts, int>> events;


    [SetUp]
    public void Init()
    {
      queue = new IntervalHeap<int>();
      events = new ArrayList<KeyValuePair<Acts, int>>();
    }


    [TearDown]
    public void Dispose() { queue = null; events = null; }

    [Test]
    public void Listenable()
    {
      Assert.AreEqual(EventTypeEnum.Basic, queue.ListenableEvents);
    }

    enum Acts
    {
      Add, Remove, Changed
    }

    [Test]
    public void Direct()
    {
      CollectionChangedHandler<int> cch;
      ItemsAddedHandler<int> iah;
      ItemsRemovedHandler<int> irh;
      Assert.AreEqual(EventTypeEnum.None, queue.ActiveEvents);
      queue.CollectionChanged += (cch = new CollectionChangedHandler<int>(queue_CollectionChanged));
      Assert.AreEqual(EventTypeEnum.Changed, queue.ActiveEvents);
      queue.ItemsAdded += (iah = new ItemsAddedHandler<int>(queue_ItemAdded));
      Assert.AreEqual(EventTypeEnum.Changed | EventTypeEnum.Added, queue.ActiveEvents);
      queue.ItemsRemoved += (irh = new ItemsRemovedHandler<int>(queue_ItemRemoved));
      Assert.AreEqual(EventTypeEnum.Changed | EventTypeEnum.Added | EventTypeEnum.Removed, queue.ActiveEvents);
      queue.Add(34);
      queue.Add(56);
      queue.AddAll<int>(new int[] { });
      queue.Add(34);
      queue.Add(12);
      queue.DeleteMax();
      queue.DeleteMin();
      queue.AddAll<int>(new int[] { 4, 5, 6, 2 });
      Assert.AreEqual(17, events.Count);
      int[] vals = { 34, 0, 56, 0, 34, 0, 12, 0, 56, 0, 12, 0, 4, 5, 6, 2, 0 };
      Acts[] acts = { Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, 
                Acts.Remove, Acts.Changed, Acts.Remove, Acts.Changed, Acts.Add, Acts.Add, Acts.Add, Acts.Add, Acts.Changed };
      for (int i = 0; i < vals.Length; i++)
      {
        //Console.WriteLine("{0}", events[cell]);
        Assert.AreEqual(acts[i], events[i].Key, "Action " + i);
        Assert.AreEqual(vals[i], events[i].Value, "Value " + i);
      }
      queue.CollectionChanged -= cch;
      Assert.AreEqual(EventTypeEnum.Added | EventTypeEnum.Removed, queue.ActiveEvents);
      queue.ItemsAdded -= iah;
      Assert.AreEqual(EventTypeEnum.Removed, queue.ActiveEvents);
      queue.ItemsRemoved -= irh;
      Assert.AreEqual(EventTypeEnum.None, queue.ActiveEvents);
    }

    [Test]
    public void Guarded()
    {
      ICollectionValue<int> guarded = new GuardedCollectionValue<int>(queue);
      guarded.CollectionChanged += new CollectionChangedHandler<int>(queue_CollectionChanged);
      guarded.ItemsAdded += new ItemsAddedHandler<int>(queue_ItemAdded);
      guarded.ItemsRemoved += new ItemsRemovedHandler<int>(queue_ItemRemoved);
      queue.Add(34);
      queue.Add(56);
      queue.Add(34);
      queue.Add(12);
      queue.DeleteMax();
      queue.DeleteMin();
      queue.AddAll<int>(new int[] { 4, 5, 6, 2 });
      Assert.AreEqual(17, events.Count);
      int[] vals = { 34, 0, 56, 0, 34, 0, 12, 0, 56, 0, 12, 0, 4, 5, 6, 2, 0 };
      Acts[] acts = { Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, Acts.Add, Acts.Changed, 
                Acts.Remove, Acts.Changed, Acts.Remove, Acts.Changed, Acts.Add, Acts.Add, Acts.Add, Acts.Add, Acts.Changed };
      for (int i = 0; i < vals.Length; i++)
      {
        //Console.WriteLine("{0}", events[cell]);
        Assert.AreEqual(vals[i], events[i].Value);
        Assert.AreEqual(acts[i], events[i].Key);
      }
    }


    void queue_CollectionChanged(object sender)
    {
      events.Add(new KeyValuePair<Acts, int>(Acts.Changed, 0));
    }
    void queue_ItemAdded(object sender, ItemCountEventArgs<int> e)
    {
      events.Add(new KeyValuePair<Acts, int>(Acts.Add, e.Item));
    }
    void queue_ItemRemoved(object sender, ItemCountEventArgs<int> e)
    {
      events.Add(new KeyValuePair<Acts, int>(Acts.Remove, e.Item));
    }
  }

  [TestFixture]
  public class Formatting
  {
    IntervalHeap<int> coll;
    IFormatProvider rad16;
    [SetUp]
    public void Init() { coll = new IntervalHeap<int>(); rad16 = new RadixFormatProvider(16); }
    [TearDown]
    public void Dispose() { coll = null; rad16 = null; }
    [Test]
    public void Format()
    {
      Assert.AreEqual("{  }", coll.ToString());
      coll.AddAll<int>(new int[] { -4, 28, 129, 65530 });
      Assert.AreEqual("{ -4, 65530, 28, 129 }", coll.ToString());
      Assert.AreEqual("{ -4, FFFA, 1C, 81 }", coll.ToString(null, rad16));
      Assert.AreEqual("{ -4, 65530, ... }", coll.ToString("L14", null));
      Assert.AreEqual("{ -4, FFFA, ... }", coll.ToString("L14", rad16));
    }
  }


  [TestFixture]
  public class IntervalHeapTests
  {
    IPriorityQueue<int> queue;


    [SetUp]
    public void Init() { queue = new IntervalHeap<int>(); }


    [TearDown]
    public void Dispose() { queue = null; }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor1()
    {
      new IntervalHeap<int>(null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor2()
    {
      new IntervalHeap<int>(5, null);
    }

    [Test]
    public void Handles()
    {
      IPriorityQueueHandle<int>[] handles = new IPriorityQueueHandle<int>[10];

      queue.Add(ref handles[0], 7);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[1], 72);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[2], 27);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[3], 17);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[4], 70);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[5], 1);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[6], 2);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[7], 7);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[8], 8);
      Assert.IsTrue(queue.Check());
      queue.Add(ref handles[9], 9);
      Assert.IsTrue(queue.Check());
      queue.Delete(handles[2]);
      Assert.IsTrue(queue.Check());
      queue.Delete(handles[0]);
      Assert.IsTrue(queue.Check());
      queue.Delete(handles[8]);
      Assert.IsTrue(queue.Check());
      queue.Delete(handles[4]);
      Assert.IsTrue(queue.Check());
      queue.Delete(handles[6]);
      Assert.IsTrue(queue.Check());
      Assert.AreEqual(5, queue.Count);
    }

    [Test]
    public void Replace()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(6);
      queue.Add(10);
      queue.Add(ref handle, 7);
      queue.Add(21);
      Assert.AreEqual(7, queue.Replace(handle, 12));
      Assert.AreEqual(21, queue.FindMax());
      Assert.AreEqual(12, queue.Replace(handle, 34));
      Assert.AreEqual(34, queue.FindMax());
      Assert.IsTrue(queue.Check());
      //replace max
      Assert.AreEqual(34, queue.Replace(handle, 60));
      Assert.AreEqual(60, queue.FindMax());
      Assert.AreEqual(60, queue.Replace(handle, queue[handle] + 80));
      Assert.AreEqual(140, queue.FindMax());
      Assert.IsTrue(queue.Check());
    }

    [Test]
    public void Replace2()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(6);
      queue.Add(10);
      queue.Add(ref handle, 7);
      //Replace last item in queue with something large
      Assert.AreEqual(7, queue.Replace(handle, 12));
      Assert.IsTrue(queue.Check());
    }

    /// <summary>
    /// bug20070504.txt by Viet Yen Nguyen 
    /// </summary>
    [Test]
    public void Replace3()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(ref handle, 10);
      Assert.AreEqual(10, queue.Replace(handle, 12));
      Assert.IsTrue(queue.Check());
    }

    /// <summary>
    /// bug20080222.txt by Thomas Dufour
    /// </summary>
    [Test]
    public void Replace4a()
    {
      IPriorityQueueHandle<int> handle1 = null;
      queue.Add(ref handle1, 4);
      Assert.AreEqual(4, queue.FindMin());
      queue.Add(3);
      Assert.AreEqual(3, queue.FindMin());
      Assert.AreEqual(4, queue.Replace(handle1, 2));
      Assert.AreEqual(2, queue.FindMin());
    }

    [Test]
    public void Replace4b()
    {
      IPriorityQueueHandle<int> handle1 = null;
      queue.Add(ref handle1, 2);
      Assert.AreEqual(2, queue.FindMax());
      queue.Add(3);
      Assert.AreEqual(3, queue.FindMax());
      Assert.AreEqual(2, queue.Replace(handle1, 4));
      Assert.AreEqual(4, queue.FindMax());
    }

    [Test]
    public void Replace5a()
    {
      for (int size = 0; size < 130; size++)
      {
        IPriorityQueue<double> q = new IntervalHeap<double>();
        IPriorityQueueHandle<double> handle1 = null;
        q.Add(ref handle1, 3.0);
        Assert.AreEqual(3.0, q.FindMin());
        for (int i = 1; i < size; i++)
          q.Add(i + 3.0);
        Assert.AreEqual(3.0, q.FindMin());
        for (int min = 2; min >= -10; min--)
        {
          Assert.AreEqual(min + 1.0, q.Replace(handle1, min));
          Assert.AreEqual(min, q.FindMin());
        }
        Assert.AreEqual(-10.0, q.DeleteMin());
        for (int i = 1; i < size; i++)
          Assert.AreEqual(i + 3.0, q.DeleteMin());
        Assert.IsTrue(q.IsEmpty);
      }
    }

    [Test]
    public void Replace5b()
    {
      for (int size = 0; size < 130; size++)
      {
        IPriorityQueue<double> q = new IntervalHeap<double>();
        IPriorityQueueHandle<double> handle1 = null;
        q.Add(ref handle1, -3.0);
        Assert.AreEqual(-3.0, q.FindMax());
        for (int i = 1; i < size; i++)
          q.Add(-i - 3.0);
        Assert.AreEqual(-3.0, q.FindMax());
        for (int max = -2; max <= 10; max++)
        {
          Assert.AreEqual(max - 1.0, q.Replace(handle1, max));
          Assert.AreEqual(max, q.FindMax());
        }
        Assert.AreEqual(10.0, q.DeleteMax());
        for (int i = 1; i < size; i++)
          Assert.AreEqual(- i - 3.0, q.DeleteMax());
        Assert.IsTrue(q.IsEmpty);
      }
    }

    [Test]
    public void Delete1a()
    {
      IPriorityQueueHandle<int> handle1 = null;
      queue.Add(ref handle1, 4);
      Assert.AreEqual(4, queue.FindMin());
      queue.Add(3);
      Assert.AreEqual(3, queue.FindMin());
      queue.Add(2);
      Assert.AreEqual(4, queue.Delete(handle1));
      Assert.AreEqual(2, queue.FindMin());
      Assert.AreEqual(3, queue.FindMax());
    }

    [Test]
    public void Delete1b()
    {
      IPriorityQueueHandle<int> handle1 = null;
      queue.Add(ref handle1, 2);
      Assert.AreEqual(2, queue.FindMax());
      queue.Add(3);
      Assert.AreEqual(3, queue.FindMax());
      queue.Add(4);
      Assert.AreEqual(2, queue.Delete(handle1));
      Assert.AreEqual(3, queue.FindMin());
      Assert.AreEqual(4, queue.FindMax());
    }

    [Test]
    public void ReuseHandle()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(ref handle, 7);
      queue.Delete(handle);
      queue.Add(ref handle, 8);
    }

    [Test]
    [ExpectedException(typeof(InvalidPriorityQueueHandleException))]
    public void ErrorAddValidHandle()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(ref handle, 7);
      queue.Add(ref handle, 8);
    }

    [Test]
    [ExpectedException(typeof(InvalidPriorityQueueHandleException))]
    public void ErrorDeleteInvalidHandle()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(ref handle, 7);
      queue.Delete(handle);
      queue.Delete(handle);
    }

    [Test]
    [ExpectedException(typeof(InvalidPriorityQueueHandleException))]
    public void ErrorReplaceInvalidHandle()
    {
      IPriorityQueueHandle<int> handle = null;
      queue.Add(ref handle, 7);
      queue.Delete(handle);
      queue.Replace(handle, 13);
    }

    [Test]
    public void Simple()
    {
      Assert.IsTrue(queue.AllowsDuplicates);
      Assert.AreEqual(0, queue.Count);
      queue.Add(8); queue.Add(18); queue.Add(8); queue.Add(3);
      Assert.AreEqual(4, queue.Count);
      Assert.AreEqual(18, queue.DeleteMax());
      Assert.AreEqual(3, queue.Count);
      Assert.AreEqual(3, queue.DeleteMin());
      Assert.AreEqual(2, queue.Count);
      Assert.AreEqual(8, queue.FindMax());
      Assert.AreEqual(8, queue.DeleteMax());
      Assert.AreEqual(8, queue.FindMax());
      queue.Add(15);
      Assert.AreEqual(15, queue.FindMax());
      Assert.AreEqual(8, queue.FindMin());
      Assert.IsTrue(queue.Comparer.Compare(2, 3) < 0);
      Assert.IsTrue(queue.Comparer.Compare(4, 3) > 0);
      Assert.IsTrue(queue.Comparer.Compare(3, 3) == 0);

    }


    [Test]
    public void Enumerate()
    {
      int[] a = new int[4];
      int siz = 0;
      foreach (int i in queue)
        siz++;
      Assert.AreEqual(0, siz);

      queue.Add(8); queue.Add(18); queue.Add(8); queue.Add(3);

      foreach (int i in queue)
        a[siz++] = i;
      Assert.AreEqual(4, siz);
      Array.Sort(a, 0, siz);
      Assert.AreEqual(3, a[0]);
      Assert.AreEqual(8, a[1]);
      Assert.AreEqual(8, a[2]);
      Assert.AreEqual(18, a[3]);

      siz = 0;
      Assert.AreEqual(18, queue.DeleteMax());
      foreach (int i in queue)
        a[siz++] = i;
      Assert.AreEqual(3, siz);
      Array.Sort(a, 0, siz);
      Assert.AreEqual(3, a[0]);
      Assert.AreEqual(8, a[1]);
      Assert.AreEqual(8, a[2]);

      siz = 0;
      Assert.AreEqual(8, queue.DeleteMax());
      foreach (int i in queue)
        a[siz++] = i;
      Assert.AreEqual(2, siz);
      Array.Sort(a, 0, siz);
      Assert.AreEqual(3, a[0]);
      Assert.AreEqual(8, a[1]);

      siz = 0;
      Assert.AreEqual(8, queue.DeleteMax());
      foreach (int i in queue)
        a[siz++] = i;
      Assert.AreEqual(1, siz);
      Assert.AreEqual(3, a[0]);
    }

    [Test]
    public void Random()
    {
      int length = 1000;
      int[] a = new int[length];
      Random ran = new Random(6754);

      for (int i = 0; i < length; i++)
        queue.Add(a[i] = ran.Next());

      Assert.IsTrue(queue.Check());
      Array.Sort(a);
      for (int i = 0; i < length / 2; i++)
      {
        Assert.AreEqual(a[length - i - 1], queue.DeleteMax());
        Assert.IsTrue(queue.Check());
        Assert.AreEqual(a[i], queue.DeleteMin());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.IsEmpty);
    }

    [Test]
    public void RandomWithHandles()
    {
      int length = 1000;
      int[] a = new int[length];
      Random ran = new Random(6754);

      for (int i = 0; i < length; i++)
      {
        IPriorityQueueHandle<int> h = null;
        queue.Add(ref h, a[i] = ran.Next());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.Check());
      Array.Sort(a);
      for (int i = 0; i < length / 2; i++)
      {
        Assert.AreEqual(a[length - i - 1], queue.DeleteMax());
        Assert.IsTrue(queue.Check());
        Assert.AreEqual(a[i], queue.DeleteMin());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.IsEmpty);
    }

    [Test]
    public void RandomWithDeleteHandles()
    {
      Random ran = new Random(6754);
      int length = 1000;
      int[] a = new int[length];
      ArrayList<int> shuffle = new ArrayList<int>(length);
      IPriorityQueueHandle<int>[] h = new IPriorityQueueHandle<int>[length];

      for (int i = 0; i < length; i++)
      {
        shuffle.Add(i);
        queue.Add(ref h[i], a[i] = ran.Next());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.Check());
      shuffle.Shuffle(ran);
      for (int i = 0; i < length; i++)
      {
        int j = shuffle[i];
        Assert.AreEqual(a[j], queue.Delete(h[j]));
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.IsEmpty);
    }

    [Test]
    public void RandomIndexing()
    {
      Random ran = new Random(6754);
      int length = 1000;
      int[] a = new int[length];
      int[] b = new int[length];
      ArrayList<int> shuffle = new ArrayList<int>(length);
      IPriorityQueueHandle<int>[] h = new IPriorityQueueHandle<int>[length];

      for (int i = 0; i < length; i++)
      {
        shuffle.Add(i);
        queue.Add(ref h[i], a[i] = ran.Next());
        b[i] = ran.Next();
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.Check());
      shuffle.Shuffle(ran);
      for (int i = 0; i < length; i++)
      {
        int j = shuffle[i];
        Assert.AreEqual(a[j], queue[h[j]]);
        queue[h[j]] = b[j];
        Assert.AreEqual(b[j], queue[h[j]]);
        Assert.IsTrue(queue.Check());
      }
    }



    [Test]
    public void RandomDuplicates()
    {
      int length = 1000;
      int s;
      int[] a = new int[length];
      Random ran = new Random(6754);

      for (int i = 0; i < length; i++)
        queue.Add(a[i] = ran.Next(3, 13));
      Assert.IsTrue(queue.Check());

      Array.Sort(a);

      for (int i = 0; i < length / 2; i++)
      {
        Assert.AreEqual(a[i], queue.DeleteMin());
        Assert.IsTrue(queue.Check());
        Assert.AreEqual(a[length - i - 1], s = queue.DeleteMax());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.IsEmpty);
    }


    [Test]
    public void AddAll()
    {
      int length = 1000;
      int[] a = new int[length];
      Random ran = new Random(6754);

      LinkedList<int> lst = new LinkedList<int>();
      for (int i = 0; i < length; i++)
        lst.Add(a[i] = ran.Next());

      queue.AddAll(lst);
      Assert.IsTrue(queue.Check());
      Array.Sort(a);
      for (int i = 0; i < length / 2; i++)
      {
        Assert.AreEqual(a[length - i - 1], queue.DeleteMax());
        Assert.IsTrue(queue.Check());
        Assert.AreEqual(a[i], queue.DeleteMin());
        Assert.IsTrue(queue.Check());
      }

      Assert.IsTrue(queue.IsEmpty);
    }

  }


}