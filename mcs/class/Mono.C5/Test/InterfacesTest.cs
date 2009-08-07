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


namespace C5UnitTests.interfaces
{
  [TestFixture]
  public class ICollectionsTests
  {
    public void TryC5Coll(ICollection<double> coll)
    {
      Assert.AreEqual(0, coll.Count);
      double[] arr = { };
      coll.CopyTo(arr, 0);
      Assert.IsFalse(coll.IsReadOnly);
      coll.Add(2.3);
      coll.Add(3.2);
      Assert.AreEqual(2, coll.Count);
      Assert.IsTrue(coll.Contains(2.3));
      Assert.IsFalse(coll.Contains(3.1));
      Assert.IsFalse(coll.Remove(3.1));
      Assert.IsTrue(coll.Remove(3.2));
      Assert.IsFalse(coll.Contains(3.1));
      Assert.AreEqual(1, coll.Count);
      coll.Clear();
      Assert.AreEqual(0, coll.Count);
      Assert.IsFalse(coll.Remove(3.1));
    }

    public void TrySCGColl(SCG.ICollection<double> coll)
    {
      // All members of SCG.ICollection<T>
      Assert.AreEqual(0, coll.Count);
      double[] arr = { };
      coll.CopyTo(arr, 0);
      Assert.IsFalse(coll.IsReadOnly);
      coll.Add(2.3);
      coll.Add(3.2);
      Assert.AreEqual(2, coll.Count);
      Assert.IsTrue(coll.Contains(2.3));
      Assert.IsFalse(coll.Contains(3.1));
      Assert.IsFalse(coll.Remove(3.1));
      Assert.IsTrue(coll.Remove(3.2));
      Assert.IsFalse(coll.Contains(3.1));
      Assert.AreEqual(1, coll.Count);
      coll.Clear();
      Assert.AreEqual(0, coll.Count);
      Assert.IsFalse(coll.Remove(3.1));
    }

    public void TryBothColl(ICollection<double> coll)
    {
      TryC5Coll(coll);
      TrySCGColl(coll);
    }


    [Test]
    public void Test1()
    {
      TryBothColl(new HashSet<double>());
      TryBothColl(new HashBag<double>());
      TryBothColl(new TreeSet<double>());
      TryBothColl(new TreeBag<double>());
      TryBothColl(new ArrayList<double>());
      TryBothColl(new LinkedList<double>());
      TryBothColl(new HashedArrayList<double>());
      TryBothColl(new HashedLinkedList<double>());
      TryBothColl(new SortedArray<double>());
    }
  }

  [TestFixture]
  public class SCIListTests
  {
    class A { }
    class B : A { }
    class C : B { }

    public void TrySCIList(System.Collections.IList list)
    {
      // Should be called with a C5.IList<B> which is not a WrappedArray
      Assert.AreEqual(0, list.Count);
      list.CopyTo(new A[0], 0);
      list.CopyTo(new B[0], 0);
      list.CopyTo(new C[0], 0);
      Assert.IsTrue(!list.IsFixedSize);
      Assert.IsFalse(list.IsReadOnly);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      Assert.AreEqual(0, list.Add(b1));
      Assert.AreEqual(1, list.Add(c1));
      Assert.AreEqual(2, list.Count);
      Assert.IsTrue(list.Contains(c1));
      Assert.IsFalse(list.Contains(b2));
      list[0] = b2;
      Assert.AreEqual(b2, list[0]);
      list[1] = c2;
      Assert.AreEqual(c2, list[1]);
      Assert.IsTrue(list.Contains(b2));
      Assert.IsTrue(list.Contains(c2));
      Array arrA = new A[2], arrB = new B[2];
      list.CopyTo(arrA, 0);
      list.CopyTo(arrB, 0);
      Assert.AreEqual(b2, arrA.GetValue(0));
      Assert.AreEqual(b2, arrB.GetValue(0));
      Assert.AreEqual(c2, arrA.GetValue(1));
      Assert.AreEqual(c2, arrB.GetValue(1));
      Assert.AreEqual(0, list.IndexOf(b2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      list.Remove(b1);
      list.Remove(b2);
      Assert.IsFalse(list.Contains(b2));
      Assert.AreEqual(1, list.Count); // Contains c2 only
      list.Insert(0, b2);
      list.Insert(2, b1);
      Assert.AreEqual(b2, list[0]);
      Assert.AreEqual(c2, list[1]);
      Assert.AreEqual(b1, list[2]);
      list.Remove(c2);
      Assert.AreEqual(b2, list[0]);
      Assert.AreEqual(b1, list[1]);
      list.RemoveAt(1);
      Assert.AreEqual(b2, list[0]); 
      list.Clear();
      Assert.AreEqual(0, list.Count);
      list.Remove(b1);
    }

    [Test]
    public void Test1()
    {
      TrySCIList(new ArrayList<B>());
      TrySCIList(new HashedArrayList<B>());
      TrySCIList(new LinkedList<B>());
      TrySCIList(new HashedLinkedList<B>());
    }

    [Test]
    public void TryWrappedArrayAsSCIList1()
    {
      B[] myarray = new B[] { new B(), new B(), new C() };
      System.Collections.IList list = new WrappedArray<B>(myarray);
      // Should be called with a three-element WrappedArray<B>
      Assert.AreEqual(3, list.Count);
      Assert.IsTrue(list.IsFixedSize);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Assert.AreEqual(myarray.SyncRoot, list.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      list[0] = b2;
      Assert.AreEqual(b2, list[0]);
      list[1] = c2;
      Assert.AreEqual(c2, list[1]);
      Assert.IsTrue(list.Contains(b2));
      Assert.IsTrue(list.Contains(c2));
      Array arrA = new A[3], arrB = new B[3];
      list.CopyTo(arrA, 0);
      list.CopyTo(arrB, 0);
      Assert.AreEqual(b2, arrA.GetValue(0));
      Assert.AreEqual(b2, arrB.GetValue(0));
      Assert.AreEqual(c2, arrA.GetValue(1));
      Assert.AreEqual(c2, arrB.GetValue(1));
      Assert.AreEqual(0, list.IndexOf(b2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c1));
      Assert.IsFalse(list.Contains(b1));
      Assert.IsFalse(list.Contains(c1));
    }

    [Test]
    public void TryWrappedArrayAsSCIList2()
    {
      B[] myarray = new B[] { };
      System.Collections.IList list = new WrappedArray<B>(myarray);
      // Should be called with an empty WrappedArray<B>
      Assert.AreEqual(0, list.Count);
      list.CopyTo(new A[0], 0);
      list.CopyTo(new B[0], 0);
      list.CopyTo(new C[0], 0);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      Assert.IsFalse(list.Contains(b2));
      Assert.IsFalse(list.Contains(c2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c1));
    }

    [Test]
    public void TryGuardedListAsSCIList1()
    {
      B b1_ = new B(), b2_ = new B();
      C c1_ = new C(), c2_ = new C();
      ArrayList<B> mylist = new ArrayList<B>();
      mylist.AddAll(new B[] { b1_, b2_, c1_ });
      System.Collections.IList list = new GuardedList<B>(mylist);
      Object b1 = b1_, b2 = b2_, c1 = c1_, c2 = c2_;
      // Should be called with a three-element GuardedList<B>
      Assert.AreEqual(3, list.Count);
      Assert.IsTrue(list.IsFixedSize);
      Assert.IsTrue(list.IsReadOnly);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Assert.AreEqual(list.SyncRoot, ((System.Collections.IList)mylist).SyncRoot);
      Assert.IsTrue(list.Contains(b1)); 
      Assert.IsTrue(list.Contains(b2));
      Assert.IsTrue(list.Contains(c1));
      Assert.IsFalse(list.Contains(c2));
      Array arrA = new A[3], arrB = new B[3];
      list.CopyTo(arrA, 0);
      list.CopyTo(arrB, 0);
      Assert.AreEqual(b1, arrA.GetValue(0));
      Assert.AreEqual(b1, arrB.GetValue(0));
      Assert.AreEqual(b2, arrA.GetValue(1));
      Assert.AreEqual(b2, arrB.GetValue(1));
      Assert.AreEqual(0, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c2));
    }

    [Test]
    public void TryGuardedListAsSCIList2()
    {
      System.Collections.IList list = new GuardedList<B>(new ArrayList<B>());
      // Should be called with an empty GuardedList<B>
      Assert.AreEqual(0, list.Count);
      list.CopyTo(new A[0], 0);
      list.CopyTo(new B[0], 0);
      list.CopyTo(new C[0], 0);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      Assert.IsFalse(list.Contains(b2));
      Assert.IsFalse(list.Contains(c2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c1));
    }

    [Test]
    public void TryViewOfGuardedListAsSCIList1()
    {
      B b1_ = new B(), b2_ = new B();
      C c1_ = new C(), c2_ = new C();
      ArrayList<B> mylist = new ArrayList<B>();
      mylist.AddAll(new B[] { new B(), b1_, b2_, c1_, new B()});
      System.Collections.IList list = new GuardedList<B>(mylist).View(1, 3);
      Object b1 = b1_, b2 = b2_, c1 = c1_, c2 = c2_;
      // Should be called with a three-element view of a GuardedList<B>
      Assert.AreEqual(3, list.Count);
      Assert.IsTrue(list.IsFixedSize);
      Assert.IsTrue(list.IsReadOnly);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Assert.AreEqual(list.SyncRoot, ((System.Collections.IList)mylist).SyncRoot);
      Assert.IsTrue(list.Contains(b1));
      Assert.IsTrue(list.Contains(b2));
      Assert.IsTrue(list.Contains(c1));
      Assert.IsFalse(list.Contains(c2));
      Array arrA = new A[3], arrB = new B[3];
      list.CopyTo(arrA, 0);
      list.CopyTo(arrB, 0);
      Assert.AreEqual(b1, arrA.GetValue(0));
      Assert.AreEqual(b1, arrB.GetValue(0));
      Assert.AreEqual(b2, arrA.GetValue(1));
      Assert.AreEqual(b2, arrB.GetValue(1));
      Assert.AreEqual(0, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c2));
    }

    [Test]
    public void TryViewOfGuardedListAsSCIList2()
    {
      System.Collections.IList list = new GuardedList<B>(new ArrayList<B>()).View(0, 0);
      Assert.AreEqual(0, list.Count);
      list.CopyTo(new A[0], 0);
      list.CopyTo(new B[0], 0);
      list.CopyTo(new C[0], 0);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      Assert.IsFalse(list.Contains(b2));
      Assert.IsFalse(list.Contains(c2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c1));
    }

    void TryListViewAsSCIList1(IList<B> mylist)
    {
      B b1_ = new B(), b2_ = new B();
      C c1_ = new C(), c2_ = new C();
      mylist.AddAll(new B[] { new B(), b1_, b2_, c1_, new B() });
      System.Collections.IList list = mylist.View(1, 3);
      Object b1 = b1_, b2 = b2_, c1 = c1_, c2 = c2_;
      // Should be called with a three-element view on ArrayList<B>
      Assert.AreEqual(3, list.Count);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Assert.AreEqual(list.SyncRoot, mylist.SyncRoot);
      Assert.IsTrue(list.Contains(b1));
      Assert.IsTrue(list.Contains(b2));
      Assert.IsTrue(list.Contains(c1));
      Assert.IsFalse(list.Contains(c2));
      Array arrA = new A[3], arrB = new B[3];
      list.CopyTo(arrA, 0);
      list.CopyTo(arrB, 0);
      Assert.AreEqual(b1, arrA.GetValue(0));
      Assert.AreEqual(b1, arrB.GetValue(0));
      Assert.AreEqual(b2, arrA.GetValue(1));
      Assert.AreEqual(b2, arrB.GetValue(1));
      Assert.AreEqual(0, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c2));
    }

    void TryListViewAsSCIList2(IList<B> mylist)
    {
      System.Collections.IList list = mylist.View(0, 0);
      Assert.AreEqual(0, list.Count);
      list.CopyTo(new A[0], 0);
      list.CopyTo(new B[0], 0);
      list.CopyTo(new C[0], 0);
      Assert.IsFalse(list.IsSynchronized);
      Assert.AreNotEqual(null, list.SyncRoot);
      Assert.AreEqual(list.SyncRoot, mylist.SyncRoot);
      Object b1 = new B(), b2 = new B(), c1 = new C(), c2 = new C();
      Assert.IsFalse(list.Contains(b2));
      Assert.IsFalse(list.Contains(c2));
      Assert.AreEqual(-1, list.IndexOf(b1));
      Assert.AreEqual(-1, list.IndexOf(c1));
    }

    [Test]
    public void TryArrayListViewAsSCIList()
    {
      TryListViewAsSCIList1(new ArrayList<B>());
      TryListViewAsSCIList2(new ArrayList<B>());
    }

    [Test]
    public void TryLinkedListViewAsSCIList()
    {
      TryListViewAsSCIList1(new LinkedList<B>());
      TryListViewAsSCIList2(new LinkedList<B>());
    }

    [Test]
    public void TryHashedArrayListViewAsSCIList()
    {
      TryListViewAsSCIList1(new HashedArrayList<B>());
      TryListViewAsSCIList2(new HashedArrayList<B>());
    }

    [Test]
    public void TryHashedLinkedListViewAsSCIList()
    {
      TryListViewAsSCIList1(new HashedLinkedList<B>());
      TryListViewAsSCIList2(new HashedLinkedList<B>());
    }

    [Test]
    public void TryGuardedViewAsSCIList()
    {
      ArrayList<B> mylist = new ArrayList<B>();
      TryListViewAsSCIList2(new GuardedList<B>(mylist));
    }
  }

  [TestFixture]
  public class IDictionaryTests
  {
    public void TryDictionary(IDictionary<string,string> dict)
    {
      Assert.AreEqual(0, dict.Count);
      Assert.IsTrue(dict.IsEmpty);
      Assert.IsFalse(dict.IsReadOnly);
      KeyValuePair<string,string>[] arr = { };
      dict.CopyTo(arr, 0);
      dict["R"] = "A";
      dict["S"] = "B";
      dict["T"] = "C";
      String old;
      Assert.IsTrue(dict.Update("R", "A1"));
      Assert.AreEqual("A1", dict["R"]);

      Assert.IsFalse(dict.Update("U", "D1"));
      Assert.IsFalse(dict.Contains("U"));
      
      Assert.IsTrue(dict.Update("R", "A2", out old));
      Assert.AreEqual("A2", dict["R"]);
      Assert.AreEqual("A1", old);
      
      Assert.IsFalse(dict.Update("U", "D2", out old));
      Assert.AreEqual(null, old);
      Assert.IsFalse(dict.Contains("U"));

      Assert.IsTrue(dict.UpdateOrAdd("R", "A3"));
      Assert.AreEqual("A3", dict["R"]);
      
      Assert.IsFalse(dict.UpdateOrAdd("U", "D3"));
      Assert.IsTrue(dict.Contains("U"));
      Assert.AreEqual("D3", dict["U"]);

      Assert.IsTrue(dict.UpdateOrAdd("R", "A4", out old));
      Assert.AreEqual("A4", dict["R"]);
      Assert.AreEqual("A3", old);
      
      Assert.IsTrue(dict.UpdateOrAdd("U", "D4", out old));
      Assert.IsTrue(dict.Contains("U"));
      Assert.AreEqual("D4", dict["U"]);
      Assert.AreEqual("D3", old);

      Assert.IsFalse(dict.UpdateOrAdd("V", "E1", out old));
      Assert.IsTrue(dict.Contains("V"));
      Assert.AreEqual("E1", dict["V"]);
      Assert.AreEqual(null, old);
    }

    [Test]
    public void TestHashDictionary()
    {
      TryDictionary(new HashDictionary<string,string>());
    }

    [Test]
    public void TestTreeDictionary()
    {
      TryDictionary(new TreeDictionary<string, string>());
    }
  }
}
