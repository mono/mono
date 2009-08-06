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


namespace C5UnitTests.arrays.hashed
{
  using CollectionOfInt = HashedArrayList<int>;

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestEvents()
    {
      Fun<CollectionOfInt> factory = delegate() { return new CollectionOfInt(TenEqualityComparer.Default); };
      new C5UnitTests.Templates.Events.ListTester<CollectionOfInt>().Test(factory);
    }

    [Test]
    public void Extensible()
    {
      C5UnitTests.Templates.Extensible.Clone.Tester<CollectionOfInt>();
      C5UnitTests.Templates.Extensible.Clone.ViewTester<CollectionOfInt>();
      C5UnitTests.Templates.Extensible.Serialization.Tester<CollectionOfInt>();
      C5UnitTests.Templates.Extensible.Serialization.ViewTester<CollectionOfInt>();
    }

    [Test]
    public void List()
    {
      C5UnitTests.Templates.List.Dispose.Tester<CollectionOfInt>();
      C5UnitTests.Templates.List.SCG_IList.Tester<CollectionOfInt>();
    }
  }

  static class Factory
  {
    public static ICollection<T> New<T>() { return new HashedArrayList<T>(); }
  }

  namespace Events
  {
    class TenEqualityComparer : SCG.IEqualityComparer<int>
    {
      TenEqualityComparer() { }
      public static TenEqualityComparer Default { get { return new TenEqualityComparer(); } }
      public int GetHashCode(int item) { return (item / 10).GetHashCode(); }
      public bool Equals(int item1, int item2) { return item1 / 10 == item2 / 10; }
    }

    [TestFixture]
    public class IList_
    {
      private HashedArrayList<int> list;
      CollectionEventList<int> seen;

      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>(TenEqualityComparer.Default);
        seen = new CollectionEventList<int>(IntEqualityComparer.Default);
      }

      private void listen() { seen.Listen(list, EventTypeEnum.All); }

      [Test]
      public void SetThis()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list[1] = 45;
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(56,1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(45, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(45,1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
      }

      [Test]
      public void Insert()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Insert(1, 45);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(45,1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(45, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
      }

      [Test]
      public void InsertAll()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.InsertAll<int>(1, new int[] { 666, 777, 888 });
        //seen.Print(Console.Error);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(666,1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(666, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(777,2), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(777, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(888,3), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(888, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
        list.InsertAll<int>(1, new int[] {});
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void InsertFirstLast()
      {
        list.Add(4); list.Add(56); list.Add(18);
        listen();
        list.InsertFirst(45);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(45,0), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(45, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
        list.InsertLast(88);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(88,4), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(88, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
      }

      [Test]
      public void Remove()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Remove();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void RemoveFirst()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.RemoveFirst();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(4,0), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(4, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void RemoveLast()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.RemoveLast();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(56,1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void Reverse()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Reverse();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.View(1, 0).Reverse();
        seen.Check(new CollectionEvent<int>[] {});
      }


      [Test]
      public void Sort()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Sort();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.View(1, 0).Sort();
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void Shuffle()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Shuffle();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.View(1, 0).Shuffle();
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void RemoveAt()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.RemoveAt(1);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(56,1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void RemoveInterval()
      {
        list.Add(4); list.Add(56); list.Add(18);
        listen();
        list.RemoveInterval(1, 2);
        seen.Check(new CollectionEvent<int>[] {
           new CollectionEvent<int>(EventTypeEnum.Cleared, new ClearedRangeEventArgs(false,2,1), list),
         new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.RemoveInterval(1, 0);
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void Update()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        list.Update(53);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(53, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
          });
        list.Update(67);
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void FindOrAdd()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        int val = 53;
        list.FindOrAdd(ref val);
        seen.Check(new CollectionEvent<int>[] {});
        val = 67;
        list.FindOrAdd(ref val);
        seen.Check(new CollectionEvent<int>[] { 
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(67, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
      }

      [Test]
      public void UpdateOrAdd()
      {
        list.Add(4); list.Add(56); list.Add(8);
        listen();
        int val = 53;
        list.UpdateOrAdd(val);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(53, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        val = 67;
        list.UpdateOrAdd(val);
        seen.Check(new CollectionEvent<int>[] { 
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(67, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.UpdateOrAdd(51, out val);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(53, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(51, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        val = 67;
        list.UpdateOrAdd(81, out val);
        seen.Check(new CollectionEvent<int>[] { 
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(81, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
      }

      [Test]
      public void RemoveItem()
      {
        list.Add(4); list.Add(56); list.Add(18);
        listen();
        list.Remove(53);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(56, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Remove(11);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(18, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void RemoveAll()
      {
        for (int i = 0; i < 10; i++)
        {
          list.Add(10 * i + 5);
        }
        listen();
        list.RemoveAll<int>(new int[] { 32, 187, 45 });
        //TODO: the order depends on internals of the HashSet
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(35, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(45, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.RemoveAll<int>(new int[] { 200, 300 });
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void Clear()
      {
        list.Add(4); list.Add(56); list.Add(18);
        listen();
        list.View(1, 1).Clear();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Cleared, new ClearedRangeEventArgs(false,1,1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.Clear();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Cleared, new ClearedRangeEventArgs(true,2,0), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.Clear();
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void ListDispose()
      {
        list.Add(4); list.Add(56); list.Add(18);
        listen();
        list.View(1, 1).Dispose();
        seen.Check(new CollectionEvent<int>[] {});
        list.Dispose();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Cleared, new ClearedRangeEventArgs(true,3,0), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)
        });
        list.Dispose();
        seen.Check(new CollectionEvent<int>[] {});
      }


      [Test]
      public void RetainAll()
      {
        for (int i = 0; i < 10; i++)
        {
          list.Add(10 * i + 5);
        }
        listen();
        list.RetainAll<int>(new int[] { 32, 187, 45, 62, 82, 95, 2 });
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(15, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(25, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(55, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(75, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.RetainAll<int>(new int[] { 32, 187, 45, 62, 82, 95, 2 });
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void RemoveAllCopies()
      {
        for (int i = 0; i < 10; i++)
        {
          list.Add(3 * i + 5);
        }
        listen();
        list.RemoveAllCopies(14);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(11, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.RemoveAllCopies(14);
        seen.Check(new CollectionEvent<int>[] {});
      }

      [Test]
      public void Add()
      {
        listen();
        seen.Check(new CollectionEvent<int>[0]);
        list.Add(23);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(23, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void AddAll()
      {
        for (int i = 0; i < 10; i++)
        {
          list.Add(10 * i + 5);
        }
        listen();
        list.AddAll<int>(new int[] { 145, 56, 167 });
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(145, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(167, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.AddAll<int>(new int[] { });
        seen.Check(new CollectionEvent<int>[] {});
      }

      [TearDown]
      public void Dispose() { list = null; seen = null; }
    }

    [TestFixture]
    public class StackQueue
    {

      private ArrayList<int> list;
      CollectionEventList<int> seen;

      [SetUp]
      public void Init()
      {
        list = new ArrayList<int>(TenEqualityComparer.Default);
        seen = new CollectionEventList<int>(IntEqualityComparer.Default);
      }

      private void listen() { seen.Listen(list, EventTypeEnum.All); }

      [Test]
      public void EnqueueDequeue()
      {
        listen();
        list.Enqueue(67);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(67,0), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(67, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Enqueue(2);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(2,1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(2, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Dequeue();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(67,0), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(67, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Dequeue();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(2,0), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(2, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [Test]
      public void PushPop()
      {
        listen();
        seen.Check(new CollectionEvent<int>[0]);
        list.Push(23);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(23,0), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(23, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Push(-12);
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.Inserted, new ItemAtEventArgs<int>(-12,1), list),
          new CollectionEvent<int>(EventTypeEnum.Added, new ItemCountEventArgs<int>(-12, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Pop();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(-12,1), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(-12, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
        list.Pop();
        seen.Check(new CollectionEvent<int>[] {
          new CollectionEvent<int>(EventTypeEnum.RemovedAt, new ItemAtEventArgs<int>(23,0), list),
          new CollectionEvent<int>(EventTypeEnum.Removed, new ItemCountEventArgs<int>(23, 1), list),
          new CollectionEvent<int>(EventTypeEnum.Changed, new EventArgs(), list)});
      }

      [TearDown]
      public void Dispose() { list = null; seen = null; }
    }


  }

  namespace Safety
  {
    /// <summary>
    /// Tests to see if the collection classes are robust for enumerable arguments that throw exceptions.
    /// </summary>
    [TestFixture]
    public class BadEnumerable
    {
      private HashedArrayList<int> list;

      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
      }

      [Test]
      public void InsertAll()
      {
        list.Add(4); list.Add(56); list.Add(18);
        try
        {
          list.InsertAll<int>(1, new BadEnumerable<int>(new BadEnumerableException(), 91, 81, 71));
          Assert.Fail("Should not get here");
        }
        catch (BadEnumerableException) { }
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 91, 81, 71, 56, 18));
      }

      [Test]
      public void AddAll()
      {
        list.Add(4); list.Add(56); list.Add(18);
        try
        {
          list.View(0, 1).AddAll<int>(new BadEnumerable<int>(new BadEnumerableException(), 91, 81, 71));
          Assert.Fail("Should not get here");
        }
        catch (BadEnumerableException) { }
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 91, 81, 71, 56, 18));
      }

      [Test]
      public void RemoveAll()
      {
        list.Add(4); list.Add(56); list.Add(18);
        try
        {
          list.RemoveAll(new BadEnumerable<int>(new BadEnumerableException(), 9, 8, 7));
          Assert.Fail("Should not get here");
        }
        catch (BadEnumerableException) { }
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 56, 18));
      }

      [Test]
      public void RetainAll()
      {
        list.Add(4); list.Add(56); list.Add(18);
        try
        {
          list.RetainAll(new BadEnumerable<int>(new BadEnumerableException(), 9, 8, 7));
          Assert.Fail("Should not get here");
        }
        catch (BadEnumerableException) { }
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 56, 18));
      }


      [Test]
      public void ContainsAll()
      {
        list.Add(4); list.Add(56); list.Add(18);
        try
        {
          list.ContainsAll(new BadEnumerable<int>(new BadEnumerableException(), 4, 18));
          Assert.Fail("Should not get here");
        }
        catch (BadEnumerableException) { }
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 56, 18));
      }


      [TearDown]
      public void Dispose() { list = null; }
    }

    /// <summary>
    /// Tests to see if the collection classes are robust for delegate arguments that throw exceptions.
    /// </summary>
    [TestFixture]
    public class BadFun
    {
      private HashedArrayList<int> list;

      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
      }

      [Test]
      public void NoTests() { }

      [TearDown]
      public void Dispose() { list = null; }
    }
  }


  namespace Enumerable
  {
    [TestFixture]
    public class Multiops
    {
      private HashedArrayList<int> list;

      private Fun<int, bool> always, never, even;


      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
        always = delegate { return true; };
        never = delegate { return false; };
        even = delegate(int i) { return i % 2 == 0; };
      }


      [Test]
      public void All()
      {
        Assert.IsTrue(list.All(always));
        Assert.IsTrue(list.All(never));
        Assert.IsTrue(list.All(even));
        list.Add(8);
        Assert.IsTrue(list.All(always));
        Assert.IsFalse(list.All(never));
        Assert.IsTrue(list.All(even));
        list.Add(5);
        Assert.IsTrue(list.All(always));
        Assert.IsFalse(list.All(never));
        Assert.IsFalse(list.All(even));
      }


      [Test]
      public void Exists()
      {
        Assert.IsFalse(list.Exists(always));
        Assert.IsFalse(list.Exists(never));
        Assert.IsFalse(list.Exists(even));
        list.Add(5);
        Assert.IsTrue(list.Exists(always));
        Assert.IsFalse(list.Exists(never));
        Assert.IsFalse(list.Exists(even));
        list.Add(8);
        Assert.IsTrue(list.Exists(always));
        Assert.IsFalse(list.Exists(never));
        Assert.IsTrue(list.Exists(even));
      }


      [Test]
      public void Apply()
      {
        int sum = 0;
        Act<int> a = delegate(int i) { sum = i + 10 * sum; };

        list.Apply(a);
        Assert.AreEqual(0, sum);
        sum = 0;
        list.Add(5); list.Add(8); list.Add(7); list.Add(5);
        list.Apply(a);
        Assert.AreEqual(587, sum);
      }


      [TearDown]
      public void Dispose() { list = null; }
    }



    [TestFixture]
    public class GetEnumerator
    {
      private HashedArrayList<int> list;


      [SetUp]
      public void Init() { list = new HashedArrayList<int>(); }


      [Test]
      public void Empty()
      {
        SCG.IEnumerator<int> e = list.GetEnumerator();

        Assert.IsFalse(e.MoveNext());
      }


      [Test]
      public void Normal()
      {
        list.Add(5);
        list.Add(8);
        list.Add(5);
        list.Add(5);
        list.Add(10);
        list.Add(1);

        SCG.IEnumerator<int> e = list.GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual(5, e.Current);
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual(8, e.Current);
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual(10, e.Current);
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual(1, e.Current);
        Assert.IsFalse(e.MoveNext());
      }


      [Test]
      public void DoDispose()
      {
        list.Add(5);
        list.Add(8);
        list.Add(5);

        SCG.IEnumerator<int> e = list.GetEnumerator();

        e.MoveNext();
        e.MoveNext();
        e.Dispose();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterUpdate()
      {
        list.Add(5);
        list.Add(8);
        list.Add(5);

        SCG.IEnumerator<int> e = list.GetEnumerator();

        e.MoveNext();
        list.Add(99);
        e.MoveNext();
      }



      [TearDown]
      public void Dispose() { list = null; }
    }
  }

  namespace CollectionOrSink
  {
    [TestFixture]
    public class Formatting
    {
      ICollection<int> coll;
      IFormatProvider rad16;
      [SetUp]
      public void Init() { coll = Factory.New<int>(); rad16 = new RadixFormatProvider(16); }
      [TearDown]
      public void Dispose() { coll = null; rad16 = null; }
      [Test]
      public void Format()
      {
        Assert.AreEqual("[  ]", coll.ToString());
        coll.AddAll<int>(new int[] { -4, 28, 129, 65530 });
        Assert.AreEqual("[ 0:-4, 1:28, 2:129, 3:65530 ]", coll.ToString());
        Assert.AreEqual("[ 0:-4, 1:1C, 2:81, 3:FFFA ]", coll.ToString(null, rad16));
        Assert.AreEqual("[ 0:-4, 1:28... ]", coll.ToString("L14", null));
        Assert.AreEqual("[ 0:-4, 1:1C... ]", coll.ToString("L14", rad16));
      }
    }

    [TestFixture]
    public class CollectionOrSink
    {
      private HashedArrayList<int> list;


      [SetUp]
      public void Init() { list = new HashedArrayList<int>(); }

      [Test]
      public void Choose()
      {
        list.Add(7);
        Assert.AreEqual(7, list.Choose());
      }

      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void BadChoose()
      {
        list.Choose();
      }


      [Test]
      public void CountEtAl()
      {
        Assert.AreEqual(0, list.Count);
        Assert.IsTrue(list.IsEmpty);
        Assert.IsFalse(list.AllowsDuplicates);
        Assert.IsTrue(list.Add(5));
        Assert.AreEqual(1, list.Count);
        Assert.IsFalse(list.IsEmpty);
        Assert.IsFalse(list.Add(5));
        Assert.AreEqual(1, list.Count);
        Assert.IsFalse(list.IsEmpty);
        Assert.IsTrue(list.Add(8));
        Assert.AreEqual(2, list.Count);
      }


      [Test]
      public void AddAll()
      {
        list.Add(3); list.Add(4); list.Add(5);

        HashedArrayList<int> list2 = new HashedArrayList<int>();

        list2.AddAll(list);
        Assert.IsTrue(IC.eq(list2, 3, 4, 5));
        list.AddAll(list2);
        Assert.IsTrue(IC.eq(list2, 3, 4, 5));
        Assert.IsTrue(IC.eq(list, 3, 4, 5));
      }


      [TearDown]
      public void Dispose() { list = null; }
    }

    [TestFixture]
    public class FindPredicate
    {
      private HashedArrayList<int> list;
      Fun<int, bool> pred;

      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>(TenEqualityComparer.Default);
        pred = delegate(int i) { return i % 5 == 0; };
      }

      [TearDown]
      public void Dispose() { list = null; }

      [Test]
      public void Find()
      {
        int i;
        Assert.IsFalse(list.Find(pred, out i));
        list.AddAll<int>(new int[] { 4, 22, 67, 37 });
        Assert.IsFalse(list.Find(pred, out i));
        list.AddAll<int>(new int[] { 45, 122, 675, 137 });
        Assert.IsTrue(list.Find(pred, out i));
        Assert.AreEqual(45, i);
      }

      [Test]
      public void FindLast()
      {
        int i;
        Assert.IsFalse(list.FindLast(pred, out i));
        list.AddAll<int>(new int[] { 4, 22, 67, 37 });
        Assert.IsFalse(list.FindLast(pred, out i));
        list.AddAll<int>(new int[] { 45, 122, 675, 137 });
        Assert.IsTrue(list.FindLast(pred, out i));
        Assert.AreEqual(675, i);
      }

      [Test]
      public void FindIndex()
      {
        Assert.IsFalse(0 <= list.FindIndex(pred));
        list.AddAll<int>(new int[] { 4, 22, 67, 37 });
        Assert.IsFalse(0 <= list.FindIndex(pred));
        list.AddAll<int>(new int[] { 45, 122, 675, 137 });
        Assert.AreEqual(4, list.FindIndex(pred));
      }

      [Test]
      public void FindLastIndex()
      {
        Assert.IsFalse(0 <= list.FindLastIndex(pred));
        list.AddAll<int>(new int[] { 4, 22, 67, 37 });
        Assert.IsFalse(0 <= list.FindLastIndex(pred));
        list.AddAll<int>(new int[] { 45, 122, 675, 137 });
        Assert.AreEqual(6, list.FindLastIndex(pred));
      }
    }

    [TestFixture]
    public class UniqueItems
    {
      private HashedArrayList<int> list;

      [SetUp]
      public void Init() { list = new HashedArrayList<int>(); }

      [TearDown]
      public void Dispose() { list = null; }

      [Test]
      public void Test()
      {
        Assert.IsTrue(IC.seteq(list.UniqueItems()));
        Assert.IsTrue(IC.seteq(list.ItemMultiplicities()));
        list.AddAll<int>(new int[] { 7, 9, 7 });
        Assert.IsTrue(IC.seteq(list.UniqueItems(), 7, 9));
        Assert.IsTrue(IC.seteq(list.ItemMultiplicities(), 7, 1, 9, 1));
      }
    }

    [TestFixture]
    public class ArrayTest
    {
      private HashedArrayList<int> list;

      int[] a;


      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
        a = new int[10];
        for (int i = 0; i < 10; i++)
          a[i] = 1000 + i;
      }


      [TearDown]
      public void Dispose() { list = null; }


      private string aeq(int[] a, params int[] b)
      {
        if (a.Length != b.Length)
          return "Lengths differ: " + a.Length + " != " + b.Length;

        for (int i = 0; i < a.Length; i++)
          if (a[i] != b[i])
            return String.Format("{0}'th elements differ: {1} != {2}", i, a[i], b[i]);

        return "Alles klar";
      }


      [Test]
      public void ToArray()
      {
        Assert.AreEqual("Alles klar", aeq(list.ToArray()));
        list.Add(7);
        list.Add(8);
        Assert.AreEqual("Alles klar", aeq(list.ToArray(), 7, 8));
      }


      [Test]
      public void CopyTo()
      {
        list.CopyTo(a, 1);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
        list.Add(6);
        list.CopyTo(a, 2);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
        list.Add(4);
        list.Add(5);
        list.Add(9);
        list.CopyTo(a, 4);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 4, 5, 9, 1008, 1009));
        list.Clear();
        list.Add(7);
        list.CopyTo(a, 9);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 4, 5, 9, 1008, 7));
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToBad()
      {
        list.CopyTo(a, 11);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToBad2()
      {
        list.CopyTo(a, -1);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToTooFar()
      {
        list.Add(3);
        list.Add(4);
        list.CopyTo(a, 9);
      }
    }



    [TestFixture]
    public class Sync
    {
      private HashedArrayList<int> list;


      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
      }


      [TearDown]
      public void Dispose() { list = null; }


      [Test]
      public void Get()
      {
        Assert.IsNotNull(((System.Collections.IList)list).SyncRoot);
      }
    }
  }




  namespace EditableCollection
  {
    [TestFixture]
    public class Searching
    {
      private HashedArrayList<int> list;


      [SetUp]
      public void Init() { list = new HashedArrayList<int>(); }


      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void NullEqualityComparerinConstructor1()
      {
        new HashedArrayList<int>(null);
      }

      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void NullEqualityComparerinConstructor2()
      {
        new HashedArrayList<int>(5, null);
      }

      [Test]
      public void Contains()
      {
        Assert.IsFalse(list.Contains(5));
        list.Add(5);
        Assert.IsTrue(list.Contains(5));
        Assert.IsFalse(list.Contains(7));
        list.Add(8);
        list.Add(10);
        Assert.IsTrue(list.Contains(5));
        Assert.IsFalse(list.Contains(7));
        Assert.IsTrue(list.Contains(8));
        Assert.IsTrue(list.Contains(10));
        list.Remove(8);
        Assert.IsTrue(list.Contains(5));
        Assert.IsFalse(list.Contains(7));
        Assert.IsFalse(list.Contains(8));
        Assert.IsTrue(list.Contains(10));
      }

      [Test]
      public void BadAdd()
      {
        Assert.IsTrue(list.Add(5));
        Assert.IsTrue(list.Add(8));
        Assert.IsFalse(list.Add(5));
      }


      [Test]
      public void ContainsCount()
      {
        Assert.AreEqual(0, list.ContainsCount(5));
        list.Add(5);
        Assert.AreEqual(1, list.ContainsCount(5));
        Assert.AreEqual(0, list.ContainsCount(7));
        list.Add(8);
        Assert.AreEqual(1, list.ContainsCount(5));
        Assert.AreEqual(0, list.ContainsCount(7));
        Assert.AreEqual(1, list.ContainsCount(8));
      }


      [Test]
      public void RemoveAllCopies()
      {
        list.Add(5); list.Add(7);
        Assert.AreEqual(1, list.ContainsCount(5));
        Assert.AreEqual(1, list.ContainsCount(7));
        list.RemoveAllCopies(5);
        Assert.AreEqual(0, list.ContainsCount(5));
        Assert.AreEqual(1, list.ContainsCount(7));
        list.Add(5); list.Add(8);
        list.RemoveAllCopies(8);
        Assert.IsTrue(IC.eq(list, 7, 5));
      }


      [Test]
      public void FindAll()
      {
        Fun<int, bool> f = delegate(int i) { return i % 2 == 0; };

        Assert.IsTrue(list.FindAll(f).IsEmpty);
        list.Add(5); list.Add(8); list.Add(10);
        Assert.IsTrue(((HashedArrayList<int>)list.FindAll(f)).Check());
        Assert.IsTrue(IC.eq(list.FindAll(f), 8, 10));
      }


      [Test]
      public void ContainsAll()
      {
        HashedArrayList<int> list2 = new HashedArrayList<int>();

        Assert.IsTrue(list.ContainsAll(list2));
        list2.Add(4);
        Assert.IsFalse(list.ContainsAll(list2));
        list.Add(4);
        Assert.IsTrue(list.ContainsAll(list2));
        list.Add(5);
        Assert.IsTrue(list.ContainsAll(list2));
      }


      [Test]
      public void RetainAll()
      {
        HashedArrayList<int> list2 = new HashedArrayList<int>();

        list.Add(4); list.Add(5); list.Add(6);
        list2.Add(5); list2.Add(4); list2.Add(7);
        list.RetainAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4, 5));
        list.Add(5); list.Add(4); list.Add(6);
        list2.Clear();
        list2.Add(5); list2.Add(5); list2.Add(6);
        list.RetainAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 5, 6));
        list2.Clear();
        list2.Add(7); list2.Add(8); list2.Add(9);
        list.RetainAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list));
      }


      [Test]
      public void RemoveAll()
      {
        HashedArrayList<int> list2 = new HashedArrayList<int>();

        list.Add(4); list.Add(5); list.Add(6);
        list2.Add(5); list2.Add(4); list2.Add(7);
        list.RemoveAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 6));
        list.Add(5); list.Add(4); list.Add(6);
        list2.Clear();
        list2.Add(6); list2.Add(5);
        list.RemoveAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4));
        list2.Clear();
        list2.Add(7); list2.Add(8); list2.Add(9);
        list.RemoveAll(list2);
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 4));
      }


      [Test]
      public void Remove()
      {
        list.Add(4); list.Add(5); list.Add(6);
        Assert.IsFalse(list.Remove(2));
        Assert.IsTrue(list.Check());
        Assert.IsTrue(list.Remove(4));
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 5, 6));
        Assert.AreEqual(6, list.RemoveLast());
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 5));
        list.Add(7);
        Assert.AreEqual(5, list.RemoveFirst());
        Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 7));
      }


      [Test]
      public void Clear()
      {
        list.Add(7); list.Add(6);
        list.Clear();
        Assert.IsTrue(list.IsEmpty);
      }


      [TearDown]
      public void Dispose() { list = null; }
    }
  }




  namespace IIndexed
  {
    [TestFixture]
    public class Searching
    {
      private IIndexed<int> dit;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
      }


      [Test]
      public void IndexOf()
      {
        Assert.AreEqual(~0, dit.IndexOf(6));
        dit.Add(7);
        Assert.AreEqual(~1, dit.IndexOf(6));
        Assert.AreEqual(~1, dit.LastIndexOf(6));
        Assert.AreEqual(0, dit.IndexOf(7));
        dit.Add(5); dit.Add(7); dit.Add(8); dit.Add(7);
        Assert.AreEqual(~3, dit.IndexOf(6));
        Assert.AreEqual(0, dit.IndexOf(7));
        Assert.AreEqual(0, dit.LastIndexOf(7));
        Assert.AreEqual(2, dit.IndexOf(8));
        Assert.AreEqual(1, dit.LastIndexOf(5));
      }


      [TearDown]
      public void Dispose()
      {
        dit = null;
      }
    }



    [TestFixture]
    public class Removing
    {
      private IIndexed<int> dit;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
      }


      [Test]
      public void RemoveAt()
      {
        dit.Add(5); dit.Add(7); dit.Add(9); dit.Add(1); dit.Add(2);
        Assert.AreEqual(7, dit.RemoveAt(1));
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 5, 9, 1, 2));
        Assert.AreEqual(5, dit.RemoveAt(0));
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 9, 1, 2));
        Assert.AreEqual(2, dit.RemoveAt(2));
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 9, 1));
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void RemoveAtBad0()
      {
        dit.RemoveAt(0);
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void RemoveAtBadM1()
      {
        dit.RemoveAt(-1);
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void RemoveAtBad1()
      {
        dit.Add(8);
        dit.RemoveAt(1);
      }


      [Test]
      public void RemoveInterval()
      {
        dit.RemoveInterval(0, 0);
        dit.Add(10); dit.Add(20); dit.Add(30); dit.Add(40); dit.Add(50); dit.Add(60);
        dit.RemoveInterval(3, 0);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 10, 20, 30, 40, 50, 60));
        dit.RemoveInterval(3, 1);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 10, 20, 30, 50, 60));
        dit.RemoveInterval(1, 3);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 10, 60));
        dit.RemoveInterval(0, 2);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit));
        dit.Add(10); dit.Add(20); dit.Add(30); dit.Add(40); dit.Add(50); dit.Add(60);
        dit.RemoveInterval(0, 2);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 30, 40, 50, 60));
        dit.RemoveInterval(2, 2);
        Assert.IsTrue(((HashedArrayList<int>)dit).Check());
        Assert.IsTrue(IC.eq(dit, 30, 40));
      }


      [TearDown]
      public void Dispose()
      {
        dit = null;
      }
    }
  }




  namespace IList
  {
    [TestFixture]
    public class Searching
    {
      private IList<int> lst;


      [SetUp]
      public void Init() { lst = new HashedArrayList<int>(); }


      [TearDown]
      public void Dispose() { lst = null; }


      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void FirstBad()
      {
        int f = lst.First;
      }


      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void LastBad()
      {
        int f = lst.Last;
      }


      [Test]
      public void FirstLast()
      {
        lst.Add(19);
        Assert.AreEqual(19, lst.First);
        Assert.AreEqual(19, lst.Last);
        lst.Add(34); lst.InsertFirst(12);
        Assert.AreEqual(12, lst.First);
        Assert.AreEqual(34, lst.Last);
      }


      [Test]
      public void This()
      {
        lst.Add(34);
        Assert.AreEqual(34, lst[0]);
        lst[0] = 56;
        Assert.AreEqual(56, lst.First);
        lst.Add(7); lst.Add(77); lst.Add(777); lst.Add(7777);
        lst[0] = 45; lst[2] = 78; lst[4] = 101;
        Assert.IsTrue(IC.eq(lst, 45, 7, 78, 777, 101));
      }

      [Test]
      public void ThisWithUpdates()
      {
        HashedArrayList<KeyValuePair<int, int>> pairlist = new HashedArrayList<KeyValuePair<int, int>>(new KeyValuePairEqualityComparer<int, int>());
        pairlist.Add(new KeyValuePair<int, int>(10, 50));
        pairlist.Add(new KeyValuePair<int, int>(11, 51));
        pairlist.Add(new KeyValuePair<int, int>(12, 52));
        pairlist.Add(new KeyValuePair<int, int>(13, 53));
        pairlist[2] = new KeyValuePair<int, int>(12, 102);
        Assert.IsTrue(pairlist.Check());
        Assert.AreEqual(new KeyValuePair<int, int>(12, 102), pairlist[2]);
        pairlist[2] = new KeyValuePair<int, int>(22, 202);
        Assert.IsTrue(pairlist.Check());
        Assert.AreEqual(new KeyValuePair<int, int>(22, 202), pairlist[2]);
        pairlist[1] = new KeyValuePair<int, int>(12, 303);
        Assert.IsTrue(pairlist.Check());
        Assert.AreEqual(new KeyValuePair<int, int>(12, 303), pairlist[1]);
        Assert.AreEqual(new KeyValuePair<int, int>(22, 202), pairlist[2]);
      }

      [Test]
      [ExpectedException(typeof(DuplicateNotAllowedException))]
      public void ThisWithUpdatesBad()
      {
        HashedArrayList<KeyValuePair<int, int>> pairlist = new HashedArrayList<KeyValuePair<int, int>>(new KeyValuePairEqualityComparer<int, int>());
        pairlist.Add(new KeyValuePair<int, int>(10, 50));
        pairlist.Add(new KeyValuePair<int, int>(11, 51));
        pairlist.Add(new KeyValuePair<int, int>(12, 52));
        pairlist.Add(new KeyValuePair<int, int>(13, 53));
        pairlist[2] = new KeyValuePair<int, int>(11, 102);
      }



      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadEmptyGet()
      {
        int f = lst[0];
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadLowGet()
      {
        lst.Add(7);

        int f = lst[-1];
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadHiGet()
      {
        lst.Add(6);

        int f = lst[1];
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadEmptySet()
      {
        lst[0] = 4;
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadLowSet()
      {
        lst.Add(7);
        lst[-1] = 9;
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void ThisBadHiSet()
      {
        lst.Add(6);
        lst[1] = 11;
      }
    }



    [TestFixture]
    public class Inserting
    {
      private IList<int> lst;


      [SetUp]
      public void Init() { lst = new HashedArrayList<int>(); }


      [TearDown]
      public void Dispose() { lst = null; }


      [Test]
      public void Insert()
      {
        lst.Insert(0, 5);
        Assert.IsTrue(IC.eq(lst, 5));
        lst.Insert(0, 7);
        Assert.IsTrue(IC.eq(lst, 7, 5));
        lst.Insert(1, 4);
        Assert.IsTrue(IC.eq(lst, 7, 4, 5));
        lst.Insert(3, 2);
        Assert.IsTrue(IC.eq(lst, 7, 4, 5, 2));
      }

      [Test]
      [ExpectedException(typeof(DuplicateNotAllowedException))]
      public void InsertDuplicate()
      {
        lst.Insert(0, 5);
        Assert.IsTrue(IC.eq(lst, 5));
        lst.Insert(0, 7);
        Assert.IsTrue(IC.eq(lst, 7, 5));
        lst.Insert(1, 5);
      }

      [Test]
      public void InsertAllDuplicate1()
      {
        lst.Insert(0, 3);
        Assert.IsTrue(IC.eq(lst, 3));
        lst.Insert(0, 7);
        Assert.IsTrue(IC.eq(lst, 7, 3));
        try
        {
          lst.InsertAll<int>(1, new int[] { 1, 2, 3, 4 });
        }
        catch (DuplicateNotAllowedException)
        {
        }
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 1, 2, 3));
      }

      [Test]
      public void InsertAllDuplicate2()
      {
        lst.Insert(0, 3);
        Assert.IsTrue(IC.eq(lst, 3));
        lst.Insert(0, 7);
        Assert.IsTrue(IC.eq(lst, 7, 3));
        try
        {
          lst.InsertAll<int>(1, new int[] { 5, 6, 5, 8 });
        }
        catch (DuplicateNotAllowedException)
        {
        }
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 5, 6, 3));
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void BadInsertLow()
      {
        lst.Add(7);
        lst.Insert(-1, 9);
      }


      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void BadInsertHi()
      {
        lst.Add(6);
        lst.Insert(2, 11);
      }


      [Test]
      public void FIFO()
      {
        for (int i = 0; i < 7; i++)
          lst.Add(2 * i);

        Assert.IsFalse(lst.FIFO);
        Assert.AreEqual(12, lst.Remove());
        Assert.AreEqual(10, lst.Remove());
        lst.FIFO = true;
        Assert.AreEqual(0, lst.Remove());
        Assert.AreEqual(2, lst.Remove());
        lst.FIFO = false;
        Assert.AreEqual(8, lst.Remove());
        Assert.AreEqual(6, lst.Remove());
      }


      [Test]
      public void InsertFirstLast()
      {
        lst.InsertFirst(4);
        lst.InsertLast(5);
        lst.InsertFirst(14);
        lst.InsertLast(15);
        lst.InsertFirst(24);
        lst.InsertLast(25);
        lst.InsertFirst(34);
        lst.InsertLast(55);
        Assert.IsTrue(IC.eq(lst, 34, 24, 14, 4, 5, 15, 25, 55));
      }


      [Test]
      public void InsertFirst()
      {
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);
        lst.Add(5);
        lst.ViewOf(2).InsertFirst(7);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 2, 3, 4, 5));
        lst.ViewOf(3).InsertFirst(8);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 2, 8, 3, 4, 5));
        lst.ViewOf(5).InsertFirst(9);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 2, 8, 3, 4, 9, 5));
      }


      [Test]
      public void BadFirst()
      {
        lst.Add(2);
        lst.Add(3);
        lst.Add(2);
        lst.Add(5);
        Assert.IsNull(lst.ViewOf(4));
      }


      [Test]
      public void InsertAfter()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);
        lst.Add(5);
        lst.LastViewOf(2).InsertLast(7);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 1, 2, 7, 3, 4, 5));
        lst.LastViewOf(1).InsertLast(8);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 1, 8, 2, 7, 3, 4, 5));
        lst.LastViewOf(5).InsertLast(9);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 1, 8, 2, 7, 3, 4, 5, 9));
      }


      [Test]
      public void BadInsertAfter()
      {
        lst.Add(2);
        lst.Add(3);
        lst.Add(6);
        lst.Add(5);
        Assert.IsNull(lst.ViewOf(4));
      }


      [Test]
      public void InsertAll()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);

        IList<int> lst2 = new HashedArrayList<int>();

        lst2.Add(7); lst2.Add(8); lst2.Add(9);
        lst.InsertAll(0, lst2);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 8, 9, 1, 2, 3, 4));
        lst.RemoveAll(lst2);
        lst.InsertAll(4, lst2);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 1, 2, 3, 4, 7, 8, 9));
        lst.RemoveAll(lst2);
        lst.InsertAll(2, lst2);
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 1, 2, 7, 8, 9, 3, 4));
      }

      [Test]
      [ExpectedException(typeof(DuplicateNotAllowedException))]
      public void InsertAllBad()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);

        IList<int> lst2 = new HashedArrayList<int>();

        lst2.Add(5); lst2.Add(2); lst2.Add(9);
        lst.InsertAll(0, lst2);
      }


      [Test]
      public void Map()
      {
        Fun<int, string> m = delegate(int i) { return "<<" + i + ">>"; };
        IList<string> r = lst.Map(m);

        Assert.IsTrue(((HashedArrayList<string>)r).Check());
        Assert.IsTrue(r.IsEmpty);
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);
        r = lst.Map(m);
        Assert.IsTrue(((HashedArrayList<string>)r).Check());
        Assert.AreEqual(4, r.Count);
        for (int i = 0; i < 4; i++)
          Assert.AreEqual("<<" + (i + 1) + ">>", r[i]);
      }
      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void BadMapper()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        Fun<int, bool> m = delegate(int i) { if (i == 2) lst.Add(7); return true; };
        lst.Map(m);
      }

      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void ModifyingFindAll()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        Fun<int, bool> m = delegate(int i) { if (i == 2) lst.Add(7); return true; };
        lst.FindAll(m);
      }
      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void BadMapperView()
      {
        lst = lst.View(0, 0);
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        Fun<int, bool> m = delegate(int i) { if (i == 2) lst.Add(7); return true; };
        lst.Map(m);
      }

      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void ModifyingFindAllView()
      {
        lst = lst.View(0, 0);
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        Fun<int, bool> m = delegate(int i) { if (i == 2) lst.Add(7); return true; };
        lst.FindAll(m);
      }


      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void BadRemove() { lst.Remove(); }

      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void BadRemoveFirst() { lst.RemoveFirst(); }

      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void BadRemoveLast() { lst.RemoveLast(); }


      [Test]
      public void RemoveFirstLast()
      {
        lst.Add(1);
        lst.Add(2);
        lst.Add(3);
        lst.Add(4);
        Assert.AreEqual(1, lst.RemoveFirst());
        Assert.AreEqual(4, lst.RemoveLast());
        Assert.AreEqual(2, lst.RemoveFirst());
        Assert.AreEqual(3, lst.RemoveLast());
        Assert.IsTrue(lst.IsEmpty);
      }


      [Test]
      public void Reverse()
      {
        for (int i = 0; i < 10; i++)
          lst.Add(i);

        lst.Reverse();
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
        lst.View(0, 3).Reverse();
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 2, 1, 0));
        lst.View(7, 0).Reverse();
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 2, 1, 0));
        lst.View(7, 3).Reverse();
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 0, 1, 2));
        lst.View(5, 1).Reverse();
        Assert.IsTrue(lst.Check());
        Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 0, 1, 2));
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void BadReverse()
      {
        for (int i = 0; i < 10; i++)
          lst.Add(i);

        lst.View(8, 3).Reverse();
      }
    }


    [TestFixture]
    public class Combined
    {
      private IList<KeyValuePair<int, int>> lst;


      [SetUp]
      public void Init()
      {
        lst = new HashedArrayList<KeyValuePair<int, int>>(new KeyValuePairEqualityComparer<int, int>());
        for (int i = 0; i < 10; i++)
          lst.Add(new KeyValuePair<int, int>(i, i + 30));
      }


      [TearDown]
      public void Dispose() { lst = null; }


      [Test]
      public void Find()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);

        Assert.IsTrue(lst.Find(ref p));
        Assert.AreEqual(3, p.Key);
        Assert.AreEqual(33, p.Value);
        p = new KeyValuePair<int, int>(13, 78);
        Assert.IsFalse(lst.Find(ref p));
      }


      [Test]
      public void FindOrAdd()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);

        Assert.IsTrue(lst.FindOrAdd(ref p));
        Assert.AreEqual(3, p.Key);
        Assert.AreEqual(33, p.Value);
        p = new KeyValuePair<int, int>(13, 79);
        Assert.IsFalse(lst.FindOrAdd(ref p));
        Assert.AreEqual(13, lst[10].Key);
        Assert.AreEqual(79, lst[10].Value);
      }


      [Test]
      public void Update()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);

        Assert.IsTrue(lst.Update(p));
        Assert.AreEqual(3, lst[3].Key);
        Assert.AreEqual(78, lst[3].Value);
        p = new KeyValuePair<int, int>(13, 78);
        Assert.IsFalse(lst.Update(p));
      }


      [Test]
      public void UpdateOrAdd1()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);

        Assert.IsTrue(lst.UpdateOrAdd(p));
        Assert.AreEqual(3, lst[3].Key);
        Assert.AreEqual(78, lst[3].Value);
        p = new KeyValuePair<int, int>(13, 79);
        Assert.IsFalse(lst.UpdateOrAdd(p));
        Assert.AreEqual(13, lst[10].Key);
        Assert.AreEqual(79, lst[10].Value);
      }

      [Test]
      public void UpdateOrAdd2()
      {
          ICollection<String> coll = new HashedArrayList<String>();
          // s1 and s2 are distinct objects but contain the same text:
          String old, s1 = "abc", s2 = ("def" + s1).Substring(3);
          Assert.IsFalse(coll.UpdateOrAdd(s1, out old));
          Assert.AreEqual(null, old);
          Assert.IsTrue(coll.UpdateOrAdd(s2, out old));
          Assert.IsTrue(Object.ReferenceEquals(s1, old));
          Assert.IsFalse(Object.ReferenceEquals(s2, old));
      }

      [Test]
      public void RemoveWithReturn()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);

        Assert.IsTrue(lst.Remove(p, out p));
        Assert.AreEqual(3, p.Key);
        Assert.AreEqual(33, p.Value);
        Assert.AreEqual(4, lst[3].Key);
        Assert.AreEqual(34, lst[3].Value);
        p = new KeyValuePair<int, int>(13, 78);
        Assert.IsFalse(lst.Remove(p, out p));
      }
    }


    [TestFixture]
    public class Sorting
    {
      private IList<int> lst;


      [SetUp]
      public void Init() { lst = new HashedArrayList<int>(); }


      [TearDown]
      public void Dispose() { lst = null; }


      [Test]
      public void Sort()
      {
        lst.Add(5); lst.Add(6); lst.Add(55); lst.Add(7); lst.Add(3);
        Assert.IsFalse(lst.IsSorted(new IC()));
        lst.Sort(new IC());
        Assert.IsTrue(lst.IsSorted());
        Assert.IsTrue(lst.IsSorted(new IC()));
        Assert.IsTrue(IC.eq(lst, 3, 5, 6, 7, 55));
      }
    }
  }




  namespace Range
  {
    [TestFixture]
    public class Range
    {
      private IList<int> lst;


      [SetUp]
      public void Init() { lst = new HashedArrayList<int>(); }


      [TearDown]
      public void Dispose() { lst = null; }


      [Test]
      public void GetRange()
      {
        //Assert.IsTrue(IC.eq(lst[0, 0)));
        for (int i = 0; i < 10; i++) lst.Add(i);

        Assert.IsTrue(IC.eq(lst[0, 3], 0, 1, 2));
        Assert.IsTrue(IC.eq(lst[3, 3], 3, 4, 5));
        Assert.IsTrue(IC.eq(lst[6, 3], 6, 7, 8));
        Assert.IsTrue(IC.eq(lst[6, 4], 6, 7, 8, 9));
      }


      [Test]
      public void Backwards()
      {
        for (int i = 0; i < 10; i++) lst.Add(i);

        Assert.IsTrue(IC.eq(lst.Backwards(), 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
        Assert.IsTrue(IC.eq(lst[0, 3].Backwards(), 2, 1, 0));
        Assert.IsTrue(IC.eq(lst[3, 3].Backwards(), 5, 4, 3));
        Assert.IsTrue(IC.eq(lst[6, 4].Backwards(), 9, 8, 7, 6));
      }


      [Test]
      public void DirectionAndCount()
      {
        for (int i = 0; i < 10; i++) lst.Add(i);

        Assert.AreEqual(EnumerationDirection.Forwards, lst.Direction);
        Assert.AreEqual(EnumerationDirection.Forwards, lst[3, 4].Direction);
        Assert.AreEqual(EnumerationDirection.Backwards, lst[3, 4].Backwards().Direction);
        Assert.AreEqual(EnumerationDirection.Backwards, lst.Backwards().Direction);
        Assert.AreEqual(4, lst[3, 4].Count);
        Assert.AreEqual(4, lst[3, 4].Backwards().Count);
        Assert.AreEqual(10, lst.Backwards().Count);
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterUpdate()
      {
        for (int i = 0; i < 10; i++) lst.Add(i);

        foreach (int i in lst)
        {
          lst.Add(45 + i);
        }
      }
    }
  }




  namespace View
  {
    [TestFixture]
    public class Simple
    {
      HashedArrayList<int> list;
      HashedArrayList<int> view;


      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
        list.Add(0); list.Add(1); list.Add(2); list.Add(3);
        view = (HashedArrayList<int>)list.View(1, 2);
      }


      [TearDown]
      public void Dispose()
      {
        list = null;
        view = null;
      }


      void check()
      {
        Assert.IsTrue(list.Check());
        Assert.IsTrue(view.Check());
      }

      [Test]
      public void InsertPointer()
      {
        IList<int> view2 = list.View(2, 0);
        list.Insert(view2, 7);
        check();
        list.Insert(list, 8);
        check();
        view.Insert(view2, 9);
        check();
        view.Insert(list.View(3, 2), 10);
        check();
        view.Insert(list.ViewOf(0), 11);
        check();
        Assert.IsTrue(IC.eq(list, 0, 11, 1, 9, 7, 2, 10, 3, 8));
        Assert.IsTrue(IC.eq(view, 11, 1, 9, 7, 2, 10));
      }

      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void InsertPointerBad1()
      {
        view.Insert(list.View(0, 0), 7);
      }

      [Test]
      [ExpectedException(typeof(IndexOutOfRangeException))]
      public void InsertPointerBad2()
      {
        view.Insert(list, 7);
      }

      [Test]
      [ExpectedException(typeof(IncompatibleViewException))]
      public void InsertPointerBad3()
      {
        list.Insert(new ArrayList<int>(), 7);
      }

      [Test]
      [ExpectedException(typeof(IncompatibleViewException))]
      public void InsertPointerBad4()
      {
        list.Insert(new ArrayList<int>().View(0, 0), 7);
      }


      [Test]
      public void Span()
      {
        IList<int> span = list.View(1, 0).Span(list.View(2, 0));
        Assert.IsTrue(span.Check());
        Assert.AreEqual(1, span.Offset);
        Assert.AreEqual(1, span.Count);
        span = list.View(0, 2).Span(list.View(2, 2));
        Assert.IsTrue(span.Check());
        Assert.AreEqual(0, span.Offset);
        Assert.AreEqual(4, span.Count);
        span = list.View(3, 1).Span(list.View(1, 1));
        Assert.IsNull(span);
      }

      [Test]
      public void ViewOf()
      {
        for (int i = 0; i < 4; i++)
          list.Add(i);
        IList<int> v = view.ViewOf(2);
        Assert.IsTrue(v.Check());
        Assert.IsTrue(IC.eq(v, 2));
        Assert.AreEqual(2, v.Offset);
        v = list.ViewOf(2);
        Assert.IsTrue(v.Check());
        Assert.IsTrue(IC.eq(v, 2));
        Assert.AreEqual(2, v.Offset);
        v = list.LastViewOf(2);
        Assert.IsTrue(v.Check());
        Assert.IsTrue(IC.eq(v, 2));
        Assert.AreEqual(2, v.Offset);
      }

      [Test]
      public void BadViewOf()
      {
        Assert.IsNull(view.ViewOf(5));
        Assert.IsNull(view.LastViewOf(5));
        Assert.IsNull(view.ViewOf(3));
        Assert.IsNull(view.LastViewOf(3));
        Assert.IsNull(view.ViewOf(0));
        Assert.IsNull(view.LastViewOf(0));
      }


      [Test]
      public void ArrayStuff()
      {
        Assert.IsTrue(IC.eq(view.ToArray(), 1, 2));
        int[] extarray = new int[5];
        view.CopyTo(extarray, 2);
        Assert.IsTrue(IC.eq(extarray, 0, 0, 1, 2, 0));
      }


      [Test]
      public void Add()
      {
        check();
        Assert.IsTrue(IC.eq(list, 0, 1, 2, 3));
        Assert.IsTrue(IC.eq(view, 1, 2));
        view.InsertFirst(10);
        check();
        Assert.IsTrue(IC.eq(list, 0, 10, 1, 2, 3));
        Assert.IsTrue(IC.eq(view, 10, 1, 2));
        view.Clear();
        Assert.IsFalse(view.IsReadOnly);
        Assert.IsFalse(view.AllowsDuplicates);
        Assert.IsTrue(view.IsEmpty);
        check();
        Assert.IsTrue(IC.eq(list, 0, 3));
        Assert.IsTrue(IC.eq(view));
        view.Add(8);
        Assert.IsFalse(view.IsEmpty);
        Assert.IsFalse(view.AllowsDuplicates);
        Assert.IsFalse(view.IsReadOnly);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 3));
        Assert.IsTrue(IC.eq(view, 8));
        view.Add(12);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 12, 3));
        Assert.IsTrue(IC.eq(view, 8, 12));
        view./*ViewOf(12).*/InsertLast(15);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 12, 15, 3));
        Assert.IsTrue(IC.eq(view, 8, 12, 15));
        view.ViewOf(12).InsertFirst(18);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 3));
        Assert.IsTrue(IC.eq(view, 8, 18, 12, 15));

        HashedArrayList<int> lst2 = new HashedArrayList<int>();

        lst2.Add(90); lst2.Add(92);
        view.AddAll(lst2);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 90, 92, 3));
        Assert.IsTrue(IC.eq(view, 8, 18, 12, 15, 90, 92));
        view.InsertLast(66);
        check();
        Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 90, 92, 66, 3));
        Assert.IsTrue(IC.eq(view, 8, 18, 12, 15, 90, 92, 66));
      }


      [Test]
      public void Bxxx()
      {
        Assert.IsTrue(IC.eq(view.Backwards(), 2, 1));
        Assert.AreSame(list, view.Underlying);
        Assert.IsNull(list.Underlying);
        Assert.AreEqual(EnumerationDirection.Forwards, view.Direction);
        Assert.AreEqual(EnumerationDirection.Backwards, view.Backwards().Direction);
        Assert.AreEqual(0, list.Offset);
        Assert.AreEqual(1, view.Offset);
      }


      [Test]
      public void Contains()
      {
        Assert.IsTrue(view.Contains(1));
        Assert.IsFalse(view.Contains(0));

        HashedArrayList<int> lst2 = new HashedArrayList<int>();

        lst2.Add(2);
        Assert.IsTrue(view.ContainsAll(lst2));
        lst2.Add(3);
        Assert.IsFalse(view.ContainsAll(lst2));
        Assert.AreEqual(Speed.Constant, view.ContainsSpeed);
        Assert.AreEqual(2, view.Count);
        view.Add(1);
        Assert.AreEqual(1, view.ContainsCount(2));
        Assert.AreEqual(1, view.ContainsCount(1));
        Assert.AreEqual(2, view.Count);
      }


      [Test]
      public void CreateView()
      {
        HashedArrayList<int> view2 = (HashedArrayList<int>)view.View(1, 0);

        Assert.AreSame(list, view2.Underlying);
      }


      [Test]
      public void FIFO()
      {
        Assert.IsFalse(view.FIFO);
        view.FIFO = true;
        view.Add(23); view.Add(24); view.Add(25);
        check();
        Assert.IsTrue(IC.eq(view, 1, 2, 23, 24, 25));
        Assert.AreEqual(1, view.Remove());
        check();
        Assert.IsTrue(IC.eq(view, 2, 23, 24, 25));
        view.FIFO = false;
        Assert.IsFalse(view.FIFO);
        Assert.AreEqual(25, view.Remove());
        check();
        Assert.IsTrue(IC.eq(view, 2, 23, 24));
      }


      [Test]
      public void MapEtc()
      {
        HashedArrayList<double> dbl = (HashedArrayList<double>)view.Map(new Fun<int, double>(delegate(int i) { return i / 10.0; }));

        Assert.IsTrue(dbl.Check());
        Assert.AreEqual(0.1, dbl[0]);
        Assert.AreEqual(0.2, dbl[1]);
        for (int i = 0; i < 10; i++) view.Add(i);

        HashedArrayList<int> list2 = (HashedArrayList<int>)view.FindAll(new Fun<int, bool>(delegate(int i) { return i % 4 == 1; }));

        Assert.IsTrue(list2.Check());
        Assert.IsTrue(IC.eq(list2, 1, 5, 9));
      }


      [Test]
      public void FL()
      {
        Assert.AreEqual(1, view.First);
        Assert.AreEqual(2, view.Last);
      }


      [Test]
      public void Indexing()
      {
        list.Clear();
        for (int i = 0; i < 20; i++) list.Add(i);

        view = (HashedArrayList<int>)list.View(5, 7);
        for (int i = 0; i < 7; i++) Assert.AreEqual(i + 5, view[i]);

        for (int i = 0; i < 7; i++) Assert.AreEqual(i, view.IndexOf(i + 5));

        for (int i = 0; i < 7; i++) Assert.AreEqual(i, view.LastIndexOf(i + 5));
      }


      [Test]
      public void Insert()
      {
        view.Insert(0, 34);
        view.Insert(1, 35);
        view.Insert(4, 36);
        Assert.IsTrue(view.Check());
        Assert.IsTrue(IC.eq(view, 34, 35, 1, 2, 36));

        IList<int> list2 = new HashedArrayList<int>();

        list2.Add(40); list2.Add(41);
        view.InsertAll(3, list2);
        Assert.IsTrue(view.Check());
        Assert.IsTrue(IC.eq(view, 34, 35, 1, 40, 41, 2, 36));
      }


      [Test]
      public void Sort()
      {
        view.Add(45); view.Add(47); view.Add(46); view.Add(48);
        Assert.IsFalse(view.IsSorted(new IC()));
        view.Sort(new IC());
        check();
        Assert.IsTrue(IC.eq(list, 0, 1, 2, 45, 46, 47, 48, 3));
        Assert.IsTrue(IC.eq(view, 1, 2, 45, 46, 47, 48));
      }


      [Test]
      public void Remove()
      {
        view.Add(1); view.Add(5); view.Add(3); view.Add(1); view.Add(3); view.Add(0);
        Assert.IsTrue(IC.eq(view, 1, 2, 5));
        Assert.IsTrue(view.Remove(1));
        check();
        Assert.IsTrue(IC.eq(view, 2, 5));
        Assert.IsFalse(view.Remove(1));
        check();
        Assert.IsTrue(IC.eq(view, 2, 5));
        Assert.IsFalse(view.Remove(0));
        check();
        Assert.IsTrue(IC.eq(view, 2, 5));
        view.RemoveAllCopies(3);
        check();
        Assert.IsTrue(IC.eq(view, 2, 5));
        Assert.IsTrue(IC.eq(list, 0, 2, 5, 3));
        view.Add(1); view.Add(5); view.Add(3); view.Add(1); view.Add(3); view.Add(0);
        Assert.IsTrue(IC.eq(view, 2, 5, 1));

        HashedArrayList<int> l2 = new HashedArrayList<int>();

        l2.Add(1); l2.Add(2); l2.Add(2); l2.Add(3); l2.Add(1);
        view.RemoveAll(l2);
        check();
        Assert.IsTrue(IC.eq(view, 5));
        view.RetainAll(l2);
        check();
        Assert.IsTrue(IC.eq(view));
        view.Add(2); view.Add(4); view.Add(5);
        Assert.AreEqual(2, view.RemoveAt(0));
        Assert.AreEqual(5, view.RemoveAt(1));
        Assert.AreEqual(4, view.RemoveAt(0));
        check();
        Assert.IsTrue(IC.eq(view));
        view.Add(8); view.Add(6); view.Add(78);
        Assert.AreEqual(8, view.RemoveFirst());
        Assert.AreEqual(78, view.RemoveLast());
        view.Add(2); view.Add(5); view.Add(3); view.Add(1);
        view.RemoveInterval(1, 2);
        check();
        Assert.IsTrue(IC.eq(view, 6, 1));
      }


      [Test]
      public void Reverse()
      {
        view.Clear();
        for (int i = 0; i < 10; i++) view.Add(10 + i);

        view.View(3, 4).Reverse();
        check();
        Assert.IsTrue(IC.eq(view, 10, 11, 12, 16, 15, 14, 13, 17, 18, 19));
        view.Reverse();
        Assert.IsTrue(IC.eq(view, 19, 18, 17, 13, 14, 15, 16, 12, 11, 10));
        Assert.IsTrue(IC.eq(list, 0, 19, 18, 17, 13, 14, 15, 16, 12, 11, 10, 3));
      }


      [Test]
      public void Slide()
      {
        view.Slide(1);
        check();
        Assert.IsTrue(IC.eq(view, 2, 3));
        view.Slide(-2);
        check();
        Assert.IsTrue(IC.eq(view, 0, 1));
        view.Slide(0, 3);
        check();
        Assert.IsTrue(IC.eq(view, 0, 1, 2));
        view.Slide(2, 1);
        check();
        Assert.IsTrue(IC.eq(view, 2));
        view.Slide(-1, 0);
        check();
        Assert.IsTrue(IC.eq(view));
        view.Add(28);
        Assert.IsTrue(IC.eq(list, 0, 28, 1, 2, 3));
      }
      [Test]
      public void Iterate()
      {
        list.Clear();
        view = null;
        foreach (int i in new int[] { 2, 4, 8, 13, 6, 1, 10, 11 }) list.Add(i);

        view = (HashedArrayList<int>)list.View(list.Count - 2, 2);
        int j = 666;
        while (true)
        {
          //Console.WriteLine("View: {0}:  {1} --> {2}", view.Count, view.First, view.Last);
          if ((view.Last - view.First) % 2 == 1)
            view.Insert(1, j++);
          check();
          if (view.Offset == 0)
            break;
          else
            view.Slide(-1, 2);
        }
        //foreach (int cell in list) Console.Write(" " + cell);
        //Assert.IsTrue(list.Check());
        Assert.IsTrue(IC.eq(list, 2, 4, 8, 668, 13, 6, 1, 667, 10, 666, 11));
      }


      [Test]
      public void SyncRoot()
      {
        Assert.AreSame(((System.Collections.IList)view).SyncRoot, ((System.Collections.IList)list).SyncRoot);
      }
    }

    [TestFixture]
    public class MulipleViews
    {
      IList<int> list;
      IList<int>[][] views;
      [SetUp]
      public void Init()
      {
        list = new HashedArrayList<int>();
        for (int i = 0; i < 6; i++)
          list.Add(i);
        views = new IList<int>[7][];
        for (int i = 0; i < 7; i++)
        {
          views[i] = new IList<int>[7 - i];
          for (int j = 0; j < 7 - i; j++)
            views[i][j] = list.View(i, j);
        }
      }
      [TearDown]
      public void Dispose()
      {
        list = null;
        views = null;
      }
      [Test]
      public void Insert()
      {
        Assert.IsTrue(list.Check(), "list check before insert");
        list.Insert(3, 777);
        Assert.IsTrue(list.Check(), "list check after insert");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i < 3 || (i == 3 && j == 0) ? i : i + 1, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i < 3 && i + j > 3 ? j + 1 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }
      [Test]
      public void RemoveAt()
      {
        Assert.IsTrue(list.Check(), "list check before remove");
        list.RemoveAt(3);
        Assert.IsTrue(list.Check(), "list check after remove");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i <= 3 ? i : i - 1, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i <= 3 && i + j > 3 ? j - 1 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }

      [Test]
      public void RemoveInterval()
      {
        Assert.IsTrue(list.Check(), "list check before remove");
        list.RemoveInterval(3, 2);
        Assert.IsTrue(list.Check(), "list check after remove");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i <= 3 ? i : i <= 5 ? 3 : i - 2, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(j == 0 ? 0 : i <= 3 && i + j > 4 ? j - 2 : i > 4 || i + j <= 3 ? j : j - 1, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }

      [Test]
      public void InsertAtEnd()
      {
        Assert.IsTrue(list.Check(), "list check before insert");
        list.InsertLast(777);
        Assert.IsTrue(list.Check(), "list check after insert");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }
      [Test]
      public void RemoveAtEnd()
      {
        Assert.IsTrue(list.Check(), "list check before remove");
        list.RemoveAt(5);
        Assert.IsTrue(list.Check(), "list check after remove");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i <= 5 ? i : i - 1, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i <= 5 && i + j > 5 ? j - 1 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }
      [Test]
      public void InsertAtStart()
      {
        Assert.IsTrue(list.Check(), "list check before insert");
        list.Insert(0, 777);
        Assert.IsTrue(list.Check(), "list check after insert");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i == 0 && j == 0 ? 0 : i + 1, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }
      [Test]
      public void RemoveAtStart()
      {
        Assert.IsTrue(list.Check(), "list check before remove");
        list.RemoveAt(0);
        Assert.IsTrue(list.Check(), "list check after remove");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i == 0 ? i : i - 1, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i == 0 && j > 0 ? j - 1 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }
      [Test]
      public void Clear()
      {
        Assert.IsTrue(list.Check(), "list check before clear");
        //for (int i = 0; i < 7; i++)
        //for (int j = 0; j < 7 - i; j++)
        //Console.WriteLine("// view[{0}][{1}] : {2}", i, j, ((HashedArrayList<int>) views[i][j]).GetHashCode());
        views[2][3].Clear();
        Assert.IsTrue(list.Check(), "list check after clear");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i < 2 ? i : i < 6 ? 2 : i - 3, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(s(i, j), views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }

      private int s(int i, int j)
      {
        if (j == 0) return 0;
        int k = i + j - 1; //end
        if (i > 4 || k <= 1) return j;
        if (i >= 2) return k > 4 ? k - 4 : 0;
        if (i <= 2) return k >= 4 ? j - 3 : 2 - i;
        return -1;
      }
      [Test]
      public void InsertAll()
      {
        IList<int> list2 = new HashedArrayList<int>();
        for (int i = 0; i < 5; i++) { list2.Add(100 + i); }
        Assert.IsTrue(list.Check(), "list check before insertAll");
        list.InsertAll(3, list2);
        Assert.IsTrue(list.Check(), "list check after insertAll");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i < 3 || (i == 3 && j == 0) ? i : i + 5, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i < 3 && i + j > 3 ? j + 5 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }

      [Test]
      public void AddAll()
      {
        IList<int> list2 = new HashedArrayList<int>();
        for (int i = 0; i < 5; i++) { list2.Add(100 + i); }
        Assert.IsTrue(list.Check(), "list check before AddAll");
        list.View(1, 2).AddAll(list2);
        Assert.IsTrue(list.Check(), "list check after AddAll");
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            Assert.AreEqual(i < 3 || (i == 3 && j == 0) ? i : i + 5, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            Assert.AreEqual(i < 3 && i + j > 3 ? j + 5 : j, views[i][j].Count, "view[" + i + "][" + j + "] count");
          }
      }

      [Test]
      public void Remove()
      {
        for (int i = 0; i < 7; i++)
        {
          for (int j = 0; j < 7 - i; j++)
          {
            list = new HashedArrayList<int>();
            for (int k = 0; k < 6; k++) list.Add(k);
            HashedArrayList<int> v = (HashedArrayList<int>)list.View(i, j);
            list.Remove(3);
            Assert.IsTrue(list.Check(), "list check after Remove, i=" + i + ", j=" + j);
          }
        }
      }
      [Test]
      public void RemoveAll1()
      {
        IList<int> list2 = new HashedArrayList<int>();
        list2.Add(1); list2.Add(3); list2.Add(4);

        for (int i = 0; i < 7; i++)
        {
          for (int j = 0; j < 7 - i; j++)
          {
            list = new HashedArrayList<int>();
            for (int k = 0; k < 6; k++) list.Add(k);
            HashedArrayList<int> v = (HashedArrayList<int>)list.View(i, j);
            list.RemoveAll(list2);
            Assert.IsTrue(list.Check(), "list check after RemoveAll, i=" + i + ", j=" + j);
          }
        }
      }
      [Test]
      public void RemoveAll2()
      {
        IList<int> list2 = new HashedArrayList<int>();
        list2.Add(1); list2.Add(3); list2.Add(4);
        Assert.IsTrue(list.Check(), "list check before RemoveAll");
        list.RemoveAll(list2);

        Assert.AreEqual(0, views[0][0].Offset, "view [0][0] offset");
        Assert.AreEqual(0, views[0][1].Offset, "view [0][1] offset");
        Assert.AreEqual(0, views[0][2].Offset, "view [0][2] offset");
        Assert.AreEqual(0, views[0][3].Offset, "view [0][3] offset");
        Assert.AreEqual(0, views[0][4].Offset, "view [0][4] offset");
        Assert.AreEqual(0, views[0][5].Offset, "view [0][5] offset");
        Assert.AreEqual(0, views[0][6].Offset, "view [0][6] offset");
        Assert.AreEqual(1, views[1][0].Offset, "view [1][0] offset");
        Assert.AreEqual(1, views[1][1].Offset, "view [1][1] offset");
        Assert.AreEqual(1, views[1][2].Offset, "view [1][2] offset");
        Assert.AreEqual(1, views[1][3].Offset, "view [1][3] offset");
        Assert.AreEqual(1, views[1][4].Offset, "view [1][4] offset");
        Assert.AreEqual(1, views[1][5].Offset, "view [1][5] offset");
        Assert.AreEqual(1, views[2][0].Offset, "view [2][0] offset");
        Assert.AreEqual(1, views[2][1].Offset, "view [2][1] offset");
        Assert.AreEqual(1, views[2][2].Offset, "view [2][2] offset");
        Assert.AreEqual(1, views[2][3].Offset, "view [2][3] offset");
        Assert.AreEqual(1, views[2][4].Offset, "view [2][4] offset");
        Assert.AreEqual(2, views[3][0].Offset, "view [3][0] offset");
        Assert.AreEqual(2, views[3][1].Offset, "view [3][1] offset");
        Assert.AreEqual(2, views[3][2].Offset, "view [3][2] offset");
        Assert.AreEqual(2, views[3][3].Offset, "view [3][3] offset");
        Assert.AreEqual(2, views[4][0].Offset, "view [4][0] offset");
        Assert.AreEqual(2, views[4][1].Offset, "view [4][1] offset");
        Assert.AreEqual(2, views[4][2].Offset, "view [4][2] offset");
        Assert.AreEqual(2, views[5][0].Offset, "view [5][0] offset");
        Assert.AreEqual(2, views[5][1].Offset, "view [5][1] offset");
        Assert.AreEqual(3, views[6][0].Offset, "view [6][0] offset");

        Assert.AreEqual(0, views[0][0].Count, "view [0][0] count");
        Assert.AreEqual(1, views[0][1].Count, "view [0][1] count");
        Assert.AreEqual(1, views[0][2].Count, "view [0][2] count");
        Assert.AreEqual(2, views[0][3].Count, "view [0][3] count");
        Assert.AreEqual(2, views[0][4].Count, "view [0][4] count");
        Assert.AreEqual(2, views[0][5].Count, "view [0][5] count");
        Assert.AreEqual(3, views[0][6].Count, "view [0][6] count");
        Assert.AreEqual(0, views[1][0].Count, "view [1][0] count");
        Assert.AreEqual(0, views[1][1].Count, "view [1][1] count");
        Assert.AreEqual(1, views[1][2].Count, "view [1][2] count");
        Assert.AreEqual(1, views[1][3].Count, "view [1][3] count");
        Assert.AreEqual(1, views[1][4].Count, "view [1][4] count");
        Assert.AreEqual(2, views[1][5].Count, "view [1][5] count");
        Assert.AreEqual(0, views[2][0].Count, "view [2][0] count");
        Assert.AreEqual(1, views[2][1].Count, "view [2][1] count");
        Assert.AreEqual(1, views[2][2].Count, "view [2][2] count");
        Assert.AreEqual(1, views[2][3].Count, "view [2][3] count");
        Assert.AreEqual(2, views[2][4].Count, "view [2][4] count");
        Assert.AreEqual(0, views[3][0].Count, "view [3][0] count");
        Assert.AreEqual(0, views[3][1].Count, "view [3][1] count");
        Assert.AreEqual(0, views[3][2].Count, "view [3][2] count");
        Assert.AreEqual(1, views[3][3].Count, "view [3][3] count");
        Assert.AreEqual(0, views[4][0].Count, "view [4][0] count");
        Assert.AreEqual(0, views[4][1].Count, "view [4][1] count");
        Assert.AreEqual(1, views[4][2].Count, "view [4][2] count");
        Assert.AreEqual(0, views[5][0].Count, "view [5][0] count");
        Assert.AreEqual(1, views[5][1].Count, "view [5][1] count");
        Assert.AreEqual(0, views[6][0].Count, "view [6][0] count");

        Assert.IsTrue(list.Check(), "list check after RemoveAll");
      }

      [Test]
      public void RetainAll()
      {
        IList<int> list2 = new HashedArrayList<int>();
        list2.Add(2); list2.Add(4); list2.Add(5);
        Assert.IsTrue(list.Check(), "list check before RetainAll");
        list.RetainAll(list2);
        Assert.AreEqual(0, views[0][0].Offset, "view [0][0] offset");
        Assert.AreEqual(0, views[0][1].Offset, "view [0][1] offset");
        Assert.AreEqual(0, views[0][2].Offset, "view [0][2] offset");
        Assert.AreEqual(0, views[0][3].Offset, "view [0][3] offset");
        Assert.AreEqual(0, views[0][4].Offset, "view [0][4] offset");
        Assert.AreEqual(0, views[0][5].Offset, "view [0][5] offset");
        Assert.AreEqual(0, views[0][6].Offset, "view [0][6] offset");
        Assert.AreEqual(0, views[1][0].Offset, "view [1][0] offset");
        Assert.AreEqual(0, views[1][1].Offset, "view [1][1] offset");
        Assert.AreEqual(0, views[1][2].Offset, "view [1][2] offset");
        Assert.AreEqual(0, views[1][3].Offset, "view [1][3] offset");
        Assert.AreEqual(0, views[1][4].Offset, "view [1][4] offset");
        Assert.AreEqual(0, views[1][5].Offset, "view [1][5] offset");
        Assert.AreEqual(0, views[2][0].Offset, "view [2][0] offset");
        Assert.AreEqual(0, views[2][1].Offset, "view [2][1] offset");
        Assert.AreEqual(0, views[2][2].Offset, "view [2][2] offset");
        Assert.AreEqual(0, views[2][3].Offset, "view [2][3] offset");
        Assert.AreEqual(0, views[2][4].Offset, "view [2][4] offset");
        Assert.AreEqual(1, views[3][0].Offset, "view [3][0] offset");
        Assert.AreEqual(1, views[3][1].Offset, "view [3][1] offset");
        Assert.AreEqual(1, views[3][2].Offset, "view [3][2] offset");
        Assert.AreEqual(1, views[3][3].Offset, "view [3][3] offset");
        Assert.AreEqual(1, views[4][0].Offset, "view [4][0] offset");
        Assert.AreEqual(1, views[4][1].Offset, "view [4][1] offset");
        Assert.AreEqual(1, views[4][2].Offset, "view [4][2] offset");
        Assert.AreEqual(2, views[5][0].Offset, "view [5][0] offset");
        Assert.AreEqual(2, views[5][1].Offset, "view [5][1] offset");
        Assert.AreEqual(3, views[6][0].Offset, "view [6][0] offset");

        Assert.AreEqual(0, views[0][0].Count, "view [0][0] count");
        Assert.AreEqual(0, views[0][1].Count, "view [0][1] count");
        Assert.AreEqual(0, views[0][2].Count, "view [0][2] count");
        Assert.AreEqual(1, views[0][3].Count, "view [0][3] count");
        Assert.AreEqual(1, views[0][4].Count, "view [0][4] count");
        Assert.AreEqual(2, views[0][5].Count, "view [0][5] count");
        Assert.AreEqual(3, views[0][6].Count, "view [0][6] count");
        Assert.AreEqual(0, views[1][0].Count, "view [1][0] count");
        Assert.AreEqual(0, views[1][1].Count, "view [1][1] count");
        Assert.AreEqual(1, views[1][2].Count, "view [1][2] count");
        Assert.AreEqual(1, views[1][3].Count, "view [1][3] count");
        Assert.AreEqual(2, views[1][4].Count, "view [1][4] count");
        Assert.AreEqual(3, views[1][5].Count, "view [1][5] count");
        Assert.AreEqual(0, views[2][0].Count, "view [2][0] count");
        Assert.AreEqual(1, views[2][1].Count, "view [2][1] count");
        Assert.AreEqual(1, views[2][2].Count, "view [2][2] count");
        Assert.AreEqual(2, views[2][3].Count, "view [2][3] count");
        Assert.AreEqual(3, views[2][4].Count, "view [2][4] count");
        Assert.AreEqual(0, views[3][0].Count, "view [3][0] count");
        Assert.AreEqual(0, views[3][1].Count, "view [3][1] count");
        Assert.AreEqual(1, views[3][2].Count, "view [3][2] count");
        Assert.AreEqual(2, views[3][3].Count, "view [3][3] count");
        Assert.AreEqual(0, views[4][0].Count, "view [4][0] count");
        Assert.AreEqual(1, views[4][1].Count, "view [4][1] count");
        Assert.AreEqual(2, views[4][2].Count, "view [4][2] count");
        Assert.AreEqual(0, views[5][0].Count, "view [5][0] count");
        Assert.AreEqual(1, views[5][1].Count, "view [5][1] count");
        Assert.AreEqual(0, views[6][0].Count, "view [6][0] count");

        Assert.IsTrue(list.Check(), "list check after RetainAll");
      }

      [Test]
      public void RemoveAllCopies()
      {
        IList<int> list2 = new HashedArrayList<int>();
        list2.Add(0); list2.Add(2); list2.Add(82); list2.Add(92); list2.Add(5); list2.Add(2); list2.Add(1);
        for (int i = 0; i < 7; i++)
        {
          for (int j = 0; j < 7 - i; j++)
          {
            list = new HashedArrayList<int>();
            list.AddAll(list2);
            HashedArrayList<int> v = (HashedArrayList<int>)list.View(i, j);
            list.RemoveAllCopies(2);
            Assert.IsTrue(list.Check(), "list check after RemoveAllCopies, i=" + i + ", j=" + j);
          }
        }
      }

      private void checkDisposed(bool reverse, int start, int count)
      {
        int k = 0;
        for (int i = 0; i < 7; i++)
          for (int j = 0; j < 7 - i; j++)
          {
            if (i + j <= start || i >= start + count || (i <= start && i + j >= start + count) || (reverse && start <= i && start + count >= i + j))
            {
              try
              {
                k = views[i][j].Count;
              }
              catch (ViewDisposedException)
              {
                Assert.Fail("view[" + i + "][" + j + "] threw");
              }
              Assert.AreEqual(j, views[i][j].Count, "view[" + i + "][" + j + "] size");
              if (reverse && ((j > 0 && start <= i && start + count >= i + j) || (j == 0 && start < i && start + count > i)))
                Assert.AreEqual(start + (start + count - i - j), views[i][j].Offset, "view[" + i + "][" + j + "] offset (mirrored)");
              else
                Assert.AreEqual(i, views[i][j].Offset, "view[" + i + "][" + j + "] offset");
            }
            else
            {
              try
              {
                k = views[i][j].Count;
                Assert.Fail("view[" + i + "][" + j + "] no throw");
              }
              catch (ViewDisposedException) { }
            }
          }
      }

      [Test]
      public void Reverse()
      {
        int start = 2, count = 3;
        IList<int> list2 = list.View(start, count);
        Assert.IsTrue(list.Check(), "list check before Reverse");
        list2.Reverse();
        Assert.IsTrue(list.Check(), "list check after Reverse");
        checkDisposed(true, start, count);
      }

      [Test]
      public void Sort()
      {
        int start = 2, count = 3;
        IList<int> list2 = list.View(start, count);
        Assert.IsTrue(list.Check(), "list check before Sort");
        list2.Sort();
        Assert.IsTrue(list.Check(), "list check after Sort");
        checkDisposed(false, start, count);
      }
      [Test]
      public void Shuffle()
      {
        int start = 2, count = 3;
        IList<int> list2 = list.View(start, count);
        Assert.IsTrue(list.Check(), "list check before Shuffle");
        list2.Shuffle();
        Assert.IsTrue(list.Check(), "list check after Shuffle");
        checkDisposed(false, start, count);
      }


    }

  }

  namespace HashingAndEquals
  {
    [TestFixture]
    public class IIndexed
    {
      private ISequenced<int> dit, dat, dut;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
      }


      [Test]
      public void EmptyEmpty()
      {
        Assert.IsTrue(dit.SequencedEquals(dat));
      }


      [Test]
      public void EmptyNonEmpty()
      {
        dit.Add(3);
        Assert.IsFalse(dit.SequencedEquals(dat));
        Assert.IsFalse(dat.SequencedEquals(dit));
      }

      [Test]
      public void HashVal()
      {
        Assert.AreEqual(CHC.sequencedhashcode(), dit.GetSequencedHashCode());
        dit.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(3), dit.GetSequencedHashCode());
        dit.Add(7);
        Assert.AreEqual(CHC.sequencedhashcode(3, 7), dit.GetSequencedHashCode());
        Assert.AreEqual(CHC.sequencedhashcode(), dut.GetSequencedHashCode());
        dut.Add(7);
        Assert.AreEqual(CHC.sequencedhashcode(7), dut.GetSequencedHashCode());
        dut.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(7, 3), dut.GetSequencedHashCode());
      }


      [Test]
      public void EqualHashButDifferent()
      {
        dit.Add(0); dit.Add(31);
        dat.Add(1); dat.Add(0);
        Assert.AreEqual(dit.GetSequencedHashCode(), dat.GetSequencedHashCode());
        Assert.IsFalse(dit.SequencedEquals(dat));
      }


      [Test]
      public void Normal()
      {
        dit.Add(3);
        dit.Add(7);
        dat.Add(3);
        Assert.IsFalse(dit.SequencedEquals(dat));
        Assert.IsFalse(dat.SequencedEquals(dit));
        dat.Add(7);
        Assert.IsTrue(dit.SequencedEquals(dat));
        Assert.IsTrue(dat.SequencedEquals(dit));
      }


      [Test]
      public void WrongOrder()
      {
        dit.Add(3);
        dut.Add(3);
        Assert.IsTrue(dit.SequencedEquals(dut));
        Assert.IsTrue(dut.SequencedEquals(dit));
        dit.Add(7);
        ((HashedArrayList<int>)dut).InsertFirst(7);
        Assert.IsFalse(dit.SequencedEquals(dut));
        Assert.IsFalse(dut.SequencedEquals(dit));
      }


      [Test]
      public void Reflexive()
      {
        Assert.IsTrue(dit.SequencedEquals(dit));
        dit.Add(3);
        Assert.IsTrue(dit.SequencedEquals(dit));
        dit.Add(7);
        Assert.IsTrue(dit.SequencedEquals(dit));
      }


      [TearDown]
      public void Dispose()
      {
        dit = null;
        dat = null;
        dut = null;
      }
    }



    [TestFixture]
    public class IEditableCollection
    {
      private ICollection<int> dit, dat, dut;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
      }


      [Test]
      public void EmptyEmpty()
      {
        Assert.IsTrue(dit.UnsequencedEquals(dat));
      }


      [Test]
      public void EmptyNonEmpty()
      {
        dit.Add(3);
        Assert.IsFalse(dit.UnsequencedEquals(dat));
        Assert.IsFalse(dat.UnsequencedEquals(dit));
      }


      [Test]
      public void HashVal()
      {
        Assert.AreEqual(CHC.unsequencedhashcode(), dit.GetUnsequencedHashCode());
        dit.Add(3);
        Assert.AreEqual(CHC.unsequencedhashcode(3), dit.GetUnsequencedHashCode());
        dit.Add(7);
        Assert.AreEqual(CHC.unsequencedhashcode(3, 7), dit.GetUnsequencedHashCode());
        Assert.AreEqual(CHC.unsequencedhashcode(), dut.GetUnsequencedHashCode());
        dut.Add(3);
        Assert.AreEqual(CHC.unsequencedhashcode(3), dut.GetUnsequencedHashCode());
        dut.Add(7);
        Assert.AreEqual(CHC.unsequencedhashcode(7, 3), dut.GetUnsequencedHashCode());
      }


      [Test]
      public void EqualHashButDifferent()
      {
        dit.Add(-1657792980); dit.Add(-1570288808);
        dat.Add(1862883298); dat.Add(-272461342);
        Assert.AreEqual(dit.GetUnsequencedHashCode(), dat.GetUnsequencedHashCode());
        Assert.IsFalse(dit.UnsequencedEquals(dat));
      }


      [Test]
      public void Normal()
      {
        dit.Add(3);
        dit.Add(7);
        dat.Add(3);
        Assert.IsFalse(dit.UnsequencedEquals(dat));
        Assert.IsFalse(dat.UnsequencedEquals(dit));
        dat.Add(7);
        Assert.IsTrue(dit.UnsequencedEquals(dat));
        Assert.IsTrue(dat.UnsequencedEquals(dit));
      }


      [Test]
      public void WrongOrder()
      {
        dit.Add(3);
        dut.Add(3);
        Assert.IsTrue(dit.UnsequencedEquals(dut));
        Assert.IsTrue(dut.UnsequencedEquals(dit));
        dit.Add(7);
        dut.Add(7);
        Assert.IsTrue(dit.UnsequencedEquals(dut));
        Assert.IsTrue(dut.UnsequencedEquals(dit));
      }


      [Test]
      public void Reflexive()
      {
        Assert.IsTrue(dit.UnsequencedEquals(dit));
        dit.Add(3);
        Assert.IsTrue(dit.UnsequencedEquals(dit));
        dit.Add(7);
        Assert.IsTrue(dit.UnsequencedEquals(dit));
      }


      [TearDown]
      public void Dispose()
      {
        dit = null;
        dat = null;
        dut = null;
      }
    }



    [TestFixture]
    public class MultiLevelUnorderedOfUnOrdered
    {
      private ICollection<int> dit, dat, dut;

      private ICollection<ICollection<int>> Dit, Dat, Dut;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        Dit = new HashedArrayList<ICollection<int>>();
        Dat = new HashedArrayList<ICollection<int>>();
        Dut = new HashedArrayList<ICollection<int>>();
      }


      [Test]
      public void Check()
      {
        Assert.IsTrue(dit.UnsequencedEquals(dat));
        Assert.IsFalse(dit.UnsequencedEquals(dut));
      }


      [Test]
      public void Multi()
      {
        Dit.Add(dit); Dit.Add(dut); Dit.Add(dit);
        Dat.Add(dut); Dat.Add(dit); Dat.Add(dat);
        Assert.IsTrue(Dit.UnsequencedEquals(Dat));
        Assert.IsFalse(Dit.UnsequencedEquals(Dut));
      }


      [TearDown]
      public void Dispose()
      {
        dit = dat = dut = null;
        Dit = Dat = Dut = null;
      }
    }



    [TestFixture]
    public class MultiLevelOrderedOfUnOrdered
    {
      private ICollection<int> dit, dat, dut;

      private ISequenced<ICollection<int>> Dit, Dat, Dut;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        Dit = new HashedArrayList<ICollection<int>>();
        Dat = new HashedArrayList<ICollection<int>>();
        Dut = new HashedArrayList<ICollection<int>>();
      }


      [Test]
      public void Check()
      {
        Assert.IsTrue(dit.UnsequencedEquals(dat));
        Assert.IsFalse(dit.UnsequencedEquals(dut));
      }


      [Test]
      public void Multi()
      {
        Dit.Add(dit); Dit.Add(dut); Dit.Add(dit);
        Dat.Add(dut); Dat.Add(dit); Dat.Add(dat);
        Dut.Add(dit); Dut.Add(dut); Dut.Add(dat);
        Assert.IsFalse(Dit.SequencedEquals(Dat));
        Assert.IsTrue(Dit.SequencedEquals(Dut));
      }


      [TearDown]
      public void Dispose()
      {
        dit = dat = dut = null;
        Dit = Dat = Dut = null;
      }
    }



    [TestFixture]
    public class MultiLevelUnOrderedOfOrdered
    {
      private ISequenced<int> dit, dat, dut, dot;

      private ICollection<ISequenced<int>> Dit, Dat, Dut, Dot;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
        dot = new HashedArrayList<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        dot.Add(2); dot.Add(1);
        Dit = new HashedArrayList<ISequenced<int>>();
        Dat = new HashedArrayList<ISequenced<int>>();
        Dut = new HashedArrayList<ISequenced<int>>();
        Dot = new HashedArrayList<ISequenced<int>>();
      }


      [Test]
      public void Check()
      {
        Assert.IsFalse(dit.SequencedEquals(dat));
        Assert.IsTrue(dit.SequencedEquals(dot));
        Assert.IsFalse(dit.SequencedEquals(dut));
      }


      [Test]
      public void Multi()
      {
        Dit.Add(dit); Dit.Add(dut); Dit.Add(dit);
        Dat.Add(dut); Dat.Add(dit); Dat.Add(dat);
        Dut.Add(dot); Dut.Add(dut); Dut.Add(dit);
        Dot.Add(dit); Dot.Add(dit); Dot.Add(dut);
        Assert.IsTrue(Dit.UnsequencedEquals(Dut));
        Assert.IsFalse(Dit.UnsequencedEquals(Dat));
        Assert.IsTrue(Dit.UnsequencedEquals(Dot));
      }


      [TearDown]
      public void Dispose()
      {
        dit = dat = dut = dot = null;
        Dit = Dat = Dut = Dot = null;
      }
    }



    [TestFixture]
    public class MultiLevelOrderedOfOrdered
    {
      private ISequenced<int> dit, dat, dut, dot;

      private ISequenced<ISequenced<int>> Dit, Dat, Dut, Dot;


      [SetUp]
      public void Init()
      {
        dit = new HashedArrayList<int>();
        dat = new HashedArrayList<int>();
        dut = new HashedArrayList<int>();
        dot = new HashedArrayList<int>();
        dit.Add(2); dit.Add(1); //{2,1}
        dat.Add(1); dat.Add(2); //{1,2}
        dut.Add(3);            //{3}
        dot.Add(2); dot.Add(1); //{2,1}
        Dit = new HashedArrayList<ISequenced<int>>();
        Dat = new HashedArrayList<ISequenced<int>>();
        Dut = new HashedArrayList<ISequenced<int>>();
        Dot = new HashedArrayList<ISequenced<int>>();
      }


      [Test]
      public void Check()
      {
        Assert.IsFalse(dit.SequencedEquals(dat));
        Assert.IsTrue(dit.SequencedEquals(dot));
        Assert.IsFalse(dit.SequencedEquals(dut));
      }


      [Test]
      public void Multi()
      {
        Dit.Add(dit); Dit.Add(dut); Dit.Add(dit); // {{2,1},{3}}
        Dat.Add(dut); Dat.Add(dit); Dat.Add(dat); // {{3},{2,1},{1,2}}
        Dut.Add(dot); Dut.Add(dut); Dut.Add(dit); // {{2,1},{3}}
        Dot.Add(dit); Dot.Add(dit); Dot.Add(dut); // {{2,1},{3}}
        Assert.IsTrue(Dit.SequencedEquals(Dut));
        Assert.IsFalse(Dit.SequencedEquals(Dat));
        Assert.IsTrue(Dit.SequencedEquals(Dot));
      }


      [TearDown]
      public void Dispose()
      {
        dit = dat = dut = dot = null;
        Dit = Dat = Dut = Dot = null;
      }
    }
  }
}