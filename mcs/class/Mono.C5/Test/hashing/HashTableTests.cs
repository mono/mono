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
namespace C5UnitTests.hashtable.set
{
  using CollectionOfInt = HashSet<int>;

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    [Category("NotWorking")]
    public void TestEvents()
    {
      Fun<CollectionOfInt> factory = delegate() { return new CollectionOfInt(TenEqualityComparer.Default); };
      new C5UnitTests.Templates.Events.CollectionTester<CollectionOfInt>().Test(factory);
    }

    [Test]
    public void Extensible()
    {
      C5UnitTests.Templates.Extensible.Clone.Tester<CollectionOfInt>();
      C5UnitTests.Templates.Extensible.Serialization.Tester<CollectionOfInt>();
    }
  }

  static class Factory
  {
    public static ICollection<T> New<T>() { return new HashSet<T>(); }
  }


  namespace Enumerable
  {
    [TestFixture]
    public class Multiops
    {
      private HashSet<int> list;

      private Fun<int, bool> always, never, even;


      [SetUp]
      public void Init()
      {
        list = new HashSet<int>();
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
        list.Add(0);
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
      [Category("NotWorking")]
      public void Apply()
      {
        int sum = 0;
        Act<int> a = delegate(int i) { sum = i + 10 * sum; };

        list.Apply(a);
        Assert.AreEqual(0, sum);
        sum = 0;
        list.Add(5); list.Add(8); list.Add(7); list.Add(5);
        list.Apply(a);
        Assert.AreEqual(758, sum);
      }


      [TearDown]
      public void Dispose() { list = null; }
    }



    [TestFixture]
    public class GetEnumerator
    {
      private HashSet<int> hashset;


      [SetUp]
      public void Init() { hashset = new HashSet<int>(); }


      [Test]
      public void Empty()
      {
        SCG.IEnumerator<int> e = hashset.GetEnumerator();

        Assert.IsFalse(e.MoveNext());
      }


      [Test]
      public void Normal()
      {
        hashset.Add(5);
        hashset.Add(8);
        hashset.Add(5);
        hashset.Add(5);
        hashset.Add(10);
        hashset.Add(1);
        hashset.Add(16);
        hashset.Add(18);
        hashset.Add(17);
        hashset.Add(33);
        Assert.IsTrue(IC.seteq(hashset, 1, 5, 8, 10, 16, 17, 18, 33));
      }

      [Test]
      public void DoDispose()
      {
        hashset.Add(5);
        hashset.Add(8);
        hashset.Add(5);

        SCG.IEnumerator<int> e = hashset.GetEnumerator();

        e.MoveNext();
        e.MoveNext();
        e.Dispose();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterUpdate()
      {
        hashset.Add(5);
        hashset.Add(8);
        hashset.Add(5);

        SCG.IEnumerator<int> e = hashset.GetEnumerator();

        e.MoveNext();
        hashset.Add(99);
        e.MoveNext();
      }


      [TearDown]
      public void Dispose() { hashset = null; }
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
      [Category("NotWorking")]
      public void Format()
      {
        Assert.AreEqual("{  }", coll.ToString());
        coll.AddAll<int>(new int[] { -4, 28, 129, 65530 });
        Assert.AreEqual("{ 65530, -4, 28, 129 }", coll.ToString());
        Assert.AreEqual("{ FFFA, -4, 1C, 81 }", coll.ToString(null, rad16));
        Assert.AreEqual("{ 65530, -4, ... }", coll.ToString("L14", null));
        Assert.AreEqual("{ FFFA, -4, ... }", coll.ToString("L14", rad16));
      }
    }

    [TestFixture]
    public class CollectionOrSink
    {
      private HashSet<int> hashset;


      [SetUp]
      public void Init() { hashset = new HashSet<int>(); }

      [Test]
      public void Choose()
      {
        hashset.Add(7);
        Assert.AreEqual(7, hashset.Choose());
      }

      [Test]
      [ExpectedException(typeof(NoSuchItemException))]
      public void BadChoose()
      {
        hashset.Choose();
      }

      [Test]
      public void CountEtAl()
      {
        Assert.AreEqual(0, hashset.Count);
        Assert.IsTrue(hashset.IsEmpty);
        Assert.IsFalse(hashset.AllowsDuplicates);
        Assert.IsTrue(hashset.Add(0));
        Assert.AreEqual(1, hashset.Count);
        Assert.IsFalse(hashset.IsEmpty);
        Assert.IsTrue(hashset.Add(5));
        Assert.AreEqual(2, hashset.Count);
        Assert.IsFalse(hashset.Add(5));
        Assert.AreEqual(2, hashset.Count);
        Assert.IsFalse(hashset.IsEmpty);
        Assert.IsTrue(hashset.Add(8));
        Assert.AreEqual(3, hashset.Count);
      }


      [Test]
      public void AddAll()
      {
        hashset.Add(3); hashset.Add(4); hashset.Add(5);

        HashSet<int> hashset2 = new HashSet<int>();

        hashset2.AddAll(hashset);
        Assert.IsTrue(IC.seteq(hashset2, 3, 4, 5));
        hashset.Add(9);
        hashset.AddAll(hashset2);
        Assert.IsTrue(IC.seteq(hashset2, 3, 4, 5));
        Assert.IsTrue(IC.seteq(hashset, 3, 4, 5, 9));
      }


      [TearDown]
      public void Dispose() { hashset = null; }
    }

    [TestFixture]
    public class FindPredicate
    {
      private HashSet<int> list;
      Fun<int, bool> pred;

      [SetUp]
      public void Init()
      {
        list = new HashSet<int>(TenEqualityComparer.Default);
        pred = delegate(int i) { return i % 5 == 0; };
      }

      [TearDown]
      public void Dispose() { list = null; }

      [Test]
      [Category("NotWorking")]
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
    }

    [TestFixture]
    public class UniqueItems
    {
      private HashSet<int> list;

      [SetUp]
      public void Init() { list = new HashSet<int>(); }

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
      private HashSet<int> hashset;

      int[] a;


      [SetUp]
      public void Init()
      {
        hashset = new HashSet<int>();
        a = new int[10];
        for (int i = 0; i < 10; i++)
          a[i] = 1000 + i;
      }


      [TearDown]
      public void Dispose() { hashset = null; }


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
        Assert.AreEqual("Alles klar", aeq(hashset.ToArray()));
        hashset.Add(7);
        hashset.Add(3);
        hashset.Add(10);

        int[] r = hashset.ToArray();

        Array.Sort(r);
        Assert.AreEqual("Alles klar", aeq(r, 3, 7, 10));
      }


      [Test]
      [Category("NotWorking")]
      public void CopyTo()
      {
        //Note: for small ints the itemequalityComparer is the identity!
        hashset.CopyTo(a, 1);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
        hashset.Add(6);
        hashset.CopyTo(a, 2);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
        hashset.Add(4);
        hashset.Add(9);
        hashset.CopyTo(a, 4);

        //TODO: make test independent on onterequalityComparer
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 9, 4, 1007, 1008, 1009));
        hashset.Clear();
        hashset.Add(7);
        hashset.CopyTo(a, 9);
        Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 9, 4, 1007, 1008, 7));
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToBad()
      {
        hashset.CopyTo(a, 11);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToBad2()
      {
        hashset.CopyTo(a, -1);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void CopyToTooFar()
      {
        hashset.Add(3);
        hashset.Add(8);
        hashset.CopyTo(a, 9);
      }
    }
  }

  namespace EditableCollection
  {
    [TestFixture]
    public class Collision
    {
      HashSet<int> hashset;


      [SetUp]
      public void Init()
      {
        hashset = new HashSet<int>();
      }


      [Test]
      public void SingleCollision()
      {
        hashset.Add(7);
        hashset.Add(7 - 1503427877);

        //foreach (int cell in hashset) Console.WriteLine("A: {0}", cell);
        hashset.Remove(7);
        Assert.IsTrue(hashset.Contains(7 - 1503427877));
      }


      [TearDown]
      public void Dispose()
      {
        hashset = null;
      }
    }



    [TestFixture]
    public class Searching
    {
      private HashSet<int> hashset;


      [SetUp]
      public void Init() { hashset = new HashSet<int>(); }


      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void NullEqualityComparerinConstructor1()
      {
        new HashSet<int>(null);
      }

      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void NullEqualityComparerinConstructor2()
      {
        new HashSet<int>(5, null);
      }

      [Test]
      [ExpectedException(typeof(NullReferenceException))]
      public void NullEqualityComparerinConstructor3()
      {
        new HashSet<int>(5, 0.5, null);
      }

      [Test]
      public void Contains()
      {
        Assert.IsFalse(hashset.Contains(5));
        hashset.Add(5);
        Assert.IsTrue(hashset.Contains(5));
        Assert.IsFalse(hashset.Contains(7));
        hashset.Add(8);
        hashset.Add(10);
        Assert.IsTrue(hashset.Contains(5));
        Assert.IsFalse(hashset.Contains(7));
        Assert.IsTrue(hashset.Contains(8));
        Assert.IsTrue(hashset.Contains(10));
        hashset.Remove(8);
        Assert.IsTrue(hashset.Contains(5));
        Assert.IsFalse(hashset.Contains(7));
        Assert.IsFalse(hashset.Contains(8));
        Assert.IsTrue(hashset.Contains(10));
        hashset.Add(0); hashset.Add(16); hashset.Add(32); hashset.Add(48); hashset.Add(64);
        Assert.IsTrue(hashset.Contains(0));
        Assert.IsTrue(hashset.Contains(16));
        Assert.IsTrue(hashset.Contains(32));
        Assert.IsTrue(hashset.Contains(48));
        Assert.IsTrue(hashset.Contains(64));
        Assert.IsTrue(hashset.Check());

        int i = 0, j = i;

        Assert.IsTrue(hashset.Find(ref i));
        Assert.AreEqual(j, i);
        j = i = 16;
        Assert.IsTrue(hashset.Find(ref i));
        Assert.AreEqual(j, i);
        j = i = 32;
        Assert.IsTrue(hashset.Find(ref i));
        Assert.AreEqual(j, i);
        j = i = 48;
        Assert.IsTrue(hashset.Find(ref i));
        Assert.AreEqual(j, i);
        j = i = 64;
        Assert.IsTrue(hashset.Find(ref i));
        Assert.AreEqual(j, i);
        j = i = 80;
        Assert.IsFalse(hashset.Find(ref i));
        Assert.AreEqual(j, i);
      }


      [Test]
      public void Many()
      {
        int j = 7373;
        int[] a = new int[j];

        for (int i = 0; i < j; i++)
        {
          hashset.Add(3 * i + 1);
          a[i] = 3 * i + 1;
        }

        Assert.IsTrue(IC.seteq(hashset, a));
      }


      [Test]
      public void ContainsCount()
      {
        Assert.AreEqual(0, hashset.ContainsCount(5));
        hashset.Add(5);
        Assert.AreEqual(1, hashset.ContainsCount(5));
        Assert.AreEqual(0, hashset.ContainsCount(7));
        hashset.Add(8);
        Assert.AreEqual(1, hashset.ContainsCount(5));
        Assert.AreEqual(0, hashset.ContainsCount(7));
        Assert.AreEqual(1, hashset.ContainsCount(8));
        hashset.Add(5);
        Assert.AreEqual(1, hashset.ContainsCount(5));
        Assert.AreEqual(0, hashset.ContainsCount(7));
        Assert.AreEqual(1, hashset.ContainsCount(8));
      }


      [Test]
      public void RemoveAllCopies()
      {
        hashset.Add(5); hashset.Add(7); hashset.Add(5);
        Assert.AreEqual(1, hashset.ContainsCount(5));
        Assert.AreEqual(1, hashset.ContainsCount(7));
        hashset.RemoveAllCopies(5);
        Assert.AreEqual(0, hashset.ContainsCount(5));
        Assert.AreEqual(1, hashset.ContainsCount(7));
        hashset.Add(5); hashset.Add(8); hashset.Add(5);
        hashset.RemoveAllCopies(8);
        Assert.IsTrue(IC.eq(hashset, 7, 5));
      }


      [Test]
      public void ContainsAll()
      {
        HashSet<int> list2 = new HashSet<int>();

        Assert.IsTrue(hashset.ContainsAll(list2));
        list2.Add(4);
        Assert.IsFalse(hashset.ContainsAll(list2));
        hashset.Add(4);
        Assert.IsTrue(hashset.ContainsAll(list2));
        hashset.Add(5);
        Assert.IsTrue(hashset.ContainsAll(list2));
        list2.Add(20);
        Assert.IsFalse(hashset.ContainsAll(list2));
        hashset.Add(20);
        Assert.IsTrue(hashset.ContainsAll(list2));
      }


      [Test]
      public void RetainAll()
      {
        HashSet<int> list2 = new HashSet<int>();

        hashset.Add(4); hashset.Add(5); hashset.Add(6);
        list2.Add(5); list2.Add(4); list2.Add(7);
        hashset.RetainAll(list2);
        Assert.IsTrue(IC.seteq(hashset, 4, 5));
        hashset.Add(6);
        list2.Clear();
        list2.Add(7); list2.Add(8); list2.Add(9);
        hashset.RetainAll(list2);
        Assert.IsTrue(IC.seteq(hashset));
      }

      //Bug in RetainAll reported by Chris Fesler
      //The result has different bitsc 
      [Test]
      public void RetainAll2()
      {
        int LARGE_ARRAY_SIZE = 30;
        int LARGE_ARRAY_MID = 15;
        string[] _largeArrayOne = new string[LARGE_ARRAY_SIZE];
        string[] _largeArrayTwo = new string[LARGE_ARRAY_SIZE];
        for (int i = 0; i < LARGE_ARRAY_SIZE; i++)
        {
          _largeArrayOne[i] = "" + i;
          _largeArrayTwo[i] = "" + (i + LARGE_ARRAY_MID);
        }

        HashSet<string> setOne = new HashSet<string>();
        setOne.AddAll(_largeArrayOne);

        HashSet<string> setTwo = new HashSet<string>();
        setTwo.AddAll(_largeArrayTwo);

        setOne.RetainAll(setTwo);

        Assert.IsTrue(setOne.Check(),"setOne check fails");

        for (int i = LARGE_ARRAY_MID; i < LARGE_ARRAY_SIZE; i++)
          Assert.IsTrue(setOne.Contains(_largeArrayOne[i]), "missing " + i);
      }

      [Test]
      public void RemoveAll()
      {
        HashSet<int> list2 = new HashSet<int>();

        hashset.Add(4); hashset.Add(5); hashset.Add(6);
        list2.Add(5); list2.Add(7); list2.Add(4);
        hashset.RemoveAll(list2);
        Assert.IsTrue(IC.eq(hashset, 6));
        hashset.Add(5); hashset.Add(4);
        list2.Clear();
        list2.Add(6); list2.Add(5);
        hashset.RemoveAll(list2);
        Assert.IsTrue(IC.eq(hashset, 4));
        list2.Clear();
        list2.Add(7); list2.Add(8); list2.Add(9);
        hashset.RemoveAll(list2);
        Assert.IsTrue(IC.eq(hashset, 4));
      }


      [Test]
      public void Remove()
      {
        hashset.Add(4); hashset.Add(4); hashset.Add(5); hashset.Add(4); hashset.Add(6);
        Assert.IsFalse(hashset.Remove(2));
        Assert.IsTrue(hashset.Remove(4));
        Assert.IsTrue(IC.seteq(hashset, 5, 6));
        hashset.Add(7);
        hashset.Add(21); hashset.Add(37); hashset.Add(53); hashset.Add(69); hashset.Add(85);
        Assert.IsTrue(hashset.Remove(5));
        Assert.IsTrue(IC.seteq(hashset, 6, 7, 21, 37, 53, 69, 85));
        Assert.IsFalse(hashset.Remove(165));
        Assert.IsTrue(IC.seteq(hashset, 6, 7, 21, 37, 53, 69, 85));
        Assert.IsTrue(hashset.Remove(53));
        Assert.IsTrue(IC.seteq(hashset, 6, 7, 21, 37, 69, 85));
        Assert.IsTrue(hashset.Remove(37));
        Assert.IsTrue(IC.seteq(hashset, 6, 7, 21, 69, 85));
        Assert.IsTrue(hashset.Remove(85));
        Assert.IsTrue(IC.seteq(hashset, 6, 7, 21, 69));
      }


      [Test]
      public void Clear()
      {
        hashset.Add(7); hashset.Add(7);
        hashset.Clear();
        Assert.IsTrue(hashset.IsEmpty);
      }


      [TearDown]
      public void Dispose() { hashset = null; }
    }

    [TestFixture]
    public class Combined
    {
      private ICollection<KeyValuePair<int, int>> lst;


      [SetUp]
      public void Init()
      {
        lst = new HashSet<KeyValuePair<int, int>>(new KeyValuePairEqualityComparer<int, int>());
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
        KeyValuePair<int, int> q = new KeyValuePair<int, int>();

        Assert.IsTrue(lst.FindOrAdd(ref p));
        Assert.AreEqual(3, p.Key);
        Assert.AreEqual(33, p.Value);
        p = new KeyValuePair<int, int>(13, 79);
        Assert.IsFalse(lst.FindOrAdd(ref p));
        q.Key = 13;
        Assert.IsTrue(lst.Find(ref q));
        Assert.AreEqual(13, q.Key);
        Assert.AreEqual(79, q.Value);
      }


      [Test]
      public void Update()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);
        KeyValuePair<int, int> q = new KeyValuePair<int, int>();

        Assert.IsTrue(lst.Update(p));
        q.Key = 3;
        Assert.IsTrue(lst.Find(ref q));
        Assert.AreEqual(3, q.Key);
        Assert.AreEqual(78, q.Value);
        p = new KeyValuePair<int, int>(13, 78);
        Assert.IsFalse(lst.Update(p));
      }


      [Test]
      public void UpdateOrAdd1()
      {
        KeyValuePair<int, int> p = new KeyValuePair<int, int>(3, 78);
        KeyValuePair<int, int> q = new KeyValuePair<int, int>();

        Assert.IsTrue(lst.UpdateOrAdd(p));
        q.Key = 3;
        Assert.IsTrue(lst.Find(ref q));
        Assert.AreEqual(3, q.Key);
        Assert.AreEqual(78, q.Value);
        p = new KeyValuePair<int, int>(13, 79);
        Assert.IsFalse(lst.UpdateOrAdd(p));
        q.Key = 13;
        Assert.IsTrue(lst.Find(ref q));
        Assert.AreEqual(13, q.Key);
        Assert.AreEqual(79, q.Value);
      }

      [Test]
      public void UpdateOrAdd2()
      {
          ICollection<String> coll = new HashSet<String>();
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
        //KeyValuePair<int,int> q = new KeyValuePair<int,int>();

        Assert.IsTrue(lst.Remove(p, out p));
        Assert.AreEqual(3, p.Key);
        Assert.AreEqual(33, p.Value);
        p = new KeyValuePair<int, int>(13, 78);
        Assert.IsFalse(lst.Remove(p, out p));
      }
    }


  }




  namespace HashingAndEquals
  {
    [TestFixture]
    public class IEditableCollection
    {
      private ICollection<int> dit, dat, dut;


      [SetUp]
      public void Init()
      {
        dit = new HashSet<int>();
        dat = new HashSet<int>();
        dut = new HashSet<int>();
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
        dit = new HashSet<int>();
        dat = new HashSet<int>();
        dut = new HashSet<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        Dit = new HashSet<ICollection<int>>();
        Dat = new HashSet<ICollection<int>>();
        Dut = new HashSet<ICollection<int>>();
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
        dit = new HashSet<int>();
        dat = new HashSet<int>();
        dut = new HashSet<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        Dit = new LinkedList<ICollection<int>>();
        Dat = new LinkedList<ICollection<int>>();
        Dut = new LinkedList<ICollection<int>>();
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
        dit = new LinkedList<int>();
        dat = new LinkedList<int>();
        dut = new LinkedList<int>();
        dot = new LinkedList<int>();
        dit.Add(2); dit.Add(1);
        dat.Add(1); dat.Add(2);
        dut.Add(3);
        dot.Add(2); dot.Add(1);
        Dit = new HashSet<ISequenced<int>>();
        Dat = new HashSet<ISequenced<int>>();
        Dut = new HashSet<ISequenced<int>>();
        Dot = new HashSet<ISequenced<int>>();
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
        Dit.Add(dit); Dit.Add(dut);//Dit.Add(dit);
        Dat.Add(dut); Dat.Add(dit); Dat.Add(dat);
        Dut.Add(dot); Dut.Add(dut);//Dut.Add(dit);
        Dot.Add(dit); Dot.Add(dit); Dot.Add(dut);
        Assert.IsTrue(Dit.UnsequencedEquals(Dit));
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
  }
}