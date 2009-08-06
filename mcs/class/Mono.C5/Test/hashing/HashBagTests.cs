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
namespace C5UnitTests.hashtable.bag
{
  using CollectionOfInt = HashBag<int>;

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
    public static ICollection<T> New<T>() { return new HashBag<T>(); }
  }


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
      Assert.AreEqual("{{  }}", coll.ToString());
      coll.AddAll<int>(new int[] { -4, 28, 129, 65530, -4, 28 });
      Assert.AreEqual("{{ 65530(*1), -4(*2), 28(*2), 129(*1) }}", coll.ToString());
      Assert.AreEqual("{{ FFFA(*1), -4(*2), 1C(*2), 81(*1) }}", coll.ToString(null, rad16));
      Assert.AreEqual("{{ 65530(*1), -4(*2)... }}", coll.ToString("L18", null));
      Assert.AreEqual("{{ FFFA(*1), -4(*2)... }}", coll.ToString("L18", rad16));
    }
  }

  [TestFixture]
  public class Combined
  {
    private ICollection<KeyValuePair<int, int>> lst;


    [SetUp]
    public void Init()
    {
      lst = new HashBag<KeyValuePair<int, int>>(new KeyValuePairEqualityComparer<int, int>());
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
        ICollection<String> coll = new HashBag<String>();
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
      //KeyValuePair<int, int> q = new KeyValuePair<int, int>();

      Assert.IsTrue(lst.Remove(p, out p));
      Assert.AreEqual(3, p.Key);
      Assert.AreEqual(33, p.Value);
      p = new KeyValuePair<int, int>(13, 78);
      Assert.IsFalse(lst.Remove(p, out p));
    }
  }



  [TestFixture]
  public class CollectionOrSink
  {
    private HashBag<int> hashbag;


    [SetUp]
    public void Init() { hashbag = new HashBag<int>(); }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor1()
    {
      new HashBag<int>(null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor2()
    {
      new HashBag<int>(5, null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor3()
    {
      new HashBag<int>(5, 0.5, null);
    }

    [Test]
    public void Choose()
    {
      hashbag.Add(7);
      Assert.AreEqual(7, hashbag.Choose());
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadChoose()
    {
      hashbag.Choose();
    }


    [Test]
    public void CountEtAl()
    {
      Assert.IsFalse(hashbag.IsReadOnly);
      // Assert.IsFalse(hashbag.SyncRoot == null);
      Assert.AreEqual(0, hashbag.Count);
      Assert.IsTrue(hashbag.IsEmpty);
      Assert.IsTrue(hashbag.AllowsDuplicates);
      Assert.IsTrue(hashbag.Add(0));
      Assert.AreEqual(1, hashbag.Count);
      Assert.IsFalse(hashbag.IsEmpty);
      Assert.IsTrue(hashbag.Add(5));
      Assert.AreEqual(2, hashbag.Count);
      Assert.IsTrue(hashbag.Add(5));
      Assert.AreEqual(3, hashbag.Count);
      Assert.IsFalse(hashbag.IsEmpty);
      Assert.IsTrue(hashbag.Add(8));
      Assert.AreEqual(4, hashbag.Count);
      Assert.AreEqual(2, hashbag.ContainsCount(5));
      Assert.AreEqual(1, hashbag.ContainsCount(8));
      Assert.AreEqual(1, hashbag.ContainsCount(0));
    }


    [Test]
    public void AddAll()
    {
      hashbag.Add(3); hashbag.Add(4); hashbag.Add(4); hashbag.Add(5); hashbag.Add(4);

      HashBag<int> hashbag2 = new HashBag<int>();

      hashbag2.AddAll(hashbag);
      Assert.IsTrue(IC.seteq(hashbag2, 3, 4, 4, 4, 5));
      hashbag.Add(9);
      hashbag.AddAll(hashbag2);
      Assert.IsTrue(IC.seteq(hashbag2, 3, 4, 4, 4, 5));
      Assert.IsTrue(IC.seteq(hashbag, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 9));
    }


    [Test]
    public void ContainsCount()
    {
      Assert.AreEqual(0, hashbag.ContainsCount(5));
      hashbag.Add(5);
      Assert.AreEqual(1, hashbag.ContainsCount(5));
      Assert.AreEqual(0, hashbag.ContainsCount(7));
      hashbag.Add(8);
      Assert.AreEqual(1, hashbag.ContainsCount(5));
      Assert.AreEqual(0, hashbag.ContainsCount(7));
      Assert.AreEqual(1, hashbag.ContainsCount(8));
      hashbag.Add(5);
      Assert.AreEqual(2, hashbag.ContainsCount(5));
      Assert.AreEqual(0, hashbag.ContainsCount(7));
      Assert.AreEqual(1, hashbag.ContainsCount(8));
    }


    [Test]
    [Category("NotWorking")]
    public void RemoveAllCopies()
    {
      hashbag.Add(5); hashbag.Add(7); hashbag.Add(5);
      Assert.AreEqual(2, hashbag.ContainsCount(5));
      Assert.AreEqual(1, hashbag.ContainsCount(7));
      hashbag.RemoveAllCopies(5);
      Assert.AreEqual(0, hashbag.ContainsCount(5));
      Assert.AreEqual(1, hashbag.ContainsCount(7));
      hashbag.Add(5); hashbag.Add(8); hashbag.Add(5);
      hashbag.RemoveAllCopies(8);
      Assert.IsTrue(IC.eq(hashbag, 7, 5, 5));
    }


    [Test]
    public void ContainsAll()
    {
      HashBag<int> list2 = new HashBag<int>();

      Assert.IsTrue(hashbag.ContainsAll(list2));
      list2.Add(4);
      Assert.IsFalse(hashbag.ContainsAll(list2));
      hashbag.Add(4);
      Assert.IsTrue(hashbag.ContainsAll(list2));
      hashbag.Add(5);
      Assert.IsTrue(hashbag.ContainsAll(list2));
      list2.Add(20);
      Assert.IsFalse(hashbag.ContainsAll(list2));
      hashbag.Add(20);
      Assert.IsTrue(hashbag.ContainsAll(list2));
      list2.Add(4);
      Assert.IsFalse(hashbag.ContainsAll(list2));
      hashbag.Add(4);
      Assert.IsTrue(hashbag.ContainsAll(list2));
    }


    [Test]
    public void RetainAll()
    {
      HashBag<int> list2 = new HashBag<int>();

      hashbag.Add(4); hashbag.Add(5); hashbag.Add(4); hashbag.Add(6); hashbag.Add(4);
      list2.Add(5); list2.Add(4); list2.Add(7); list2.Add(4);
      hashbag.RetainAll(list2);
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 5));
      hashbag.Add(6);
      list2.Clear();
      list2.Add(7); list2.Add(8); list2.Add(9);
      hashbag.RetainAll(list2);
      Assert.IsTrue(IC.eq(hashbag));
    }


    [Test]
    public void RemoveAll()
    {
      HashBag<int> list2 = new HashBag<int>();

      hashbag.Add(4); hashbag.Add(5); hashbag.Add(6); hashbag.Add(4); hashbag.Add(5);
      list2.Add(5); list2.Add(4); list2.Add(7); list2.Add(4);
      hashbag.RemoveAll(list2);
      Assert.IsTrue(IC.seteq(hashbag, 5, 6));
      hashbag.Add(5); hashbag.Add(4);
      list2.Clear();
      list2.Add(6); list2.Add(5);
      hashbag.RemoveAll(list2);
      Assert.IsTrue(IC.seteq(hashbag, 4, 5));
      list2.Clear();
      list2.Add(7); list2.Add(8); list2.Add(9);
      hashbag.RemoveAll(list2);
      Assert.IsTrue(IC.seteq(hashbag, 4, 5));
    }


    [Test]
    public void Remove()
    {
      hashbag.Add(4); hashbag.Add(4); hashbag.Add(5); hashbag.Add(4); hashbag.Add(6);
      Assert.IsFalse(hashbag.Remove(2));
      Assert.IsTrue(hashbag.Remove(4));
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 5, 6));
      hashbag.Add(7);
      hashbag.Add(21); hashbag.Add(37); hashbag.Add(53); hashbag.Add(69); hashbag.Add(53); hashbag.Add(85);
      Assert.IsTrue(hashbag.Remove(5));
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 6, 7, 21, 37, 53, 53, 69, 85));
      Assert.IsFalse(hashbag.Remove(165));
      Assert.IsTrue(hashbag.Check());
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 6, 7, 21, 37, 53, 53, 69, 85));
      Assert.IsTrue(hashbag.Remove(53));
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 6, 7, 21, 37, 53, 69, 85));
      Assert.IsTrue(hashbag.Remove(37));
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 6, 7, 21, 53, 69, 85));
      Assert.IsTrue(hashbag.Remove(85));
      Assert.IsTrue(IC.seteq(hashbag, 4, 4, 6, 7, 21, 53, 69));
    }


    [TearDown]
    public void Dispose() { hashbag = null; }
  }

  [TestFixture]
  public class FindPredicate
  {
    private HashBag<int> list;
    Fun<int, bool> pred;

    [SetUp]
    public void Init()
    {
      list = new HashBag<int>(TenEqualityComparer.Default);
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
    private HashBag<int> list;

    [SetUp]
    public void Init() { list = new HashBag<int>(); }

    [TearDown]
    public void Dispose() { list = null; }

    [Test]
    public void Test()
    {
      Assert.IsTrue(IC.seteq(list.UniqueItems()));
      Assert.IsTrue(IC.seteq(list.ItemMultiplicities()));
      list.AddAll<int>(new int[] { 7, 9, 7 });
      Assert.IsTrue(IC.seteq(list.UniqueItems(), 7, 9));
      Assert.IsTrue(IC.seteq(list.ItemMultiplicities(), 7, 2, 9, 1));
    }
  }

  [TestFixture]
  public class ArrayTest
  {
    private HashBag<int> hashbag;

    int[] a;


    [SetUp]
    public void Init()
    {
      hashbag = new HashBag<int>();
      a = new int[10];
      for (int i = 0; i < 10; i++)
        a[i] = 1000 + i;
    }


    [TearDown]
    public void Dispose() { hashbag = null; }


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
      Assert.AreEqual("Alles klar", aeq(hashbag.ToArray()));
      hashbag.Add(7);
      hashbag.Add(3);
      hashbag.Add(10);
      hashbag.Add(3);

      int[] r = hashbag.ToArray();

      Array.Sort(r);
      Assert.AreEqual("Alles klar", aeq(r, 3, 3, 7, 10));
    }


    [Test]
    [Category("NotWorking")]
    public void CopyTo()
    {
      //Note: for small ints the itemequalityComparer is the identity!
      hashbag.CopyTo(a, 1);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
      hashbag.Add(6);
      hashbag.CopyTo(a, 2);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
      hashbag.Add(4);
      hashbag.Add(6);
      hashbag.Add(9);
      hashbag.CopyTo(a, 4);

      //TODO: make independent of interequalityComparer
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 6, 9, 4, 1008, 1009));
      hashbag.Clear();
      hashbag.Add(7);
      hashbag.CopyTo(a, 9);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 6, 9, 4, 1008, 7));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToBad()
    {
      hashbag.CopyTo(a, 11);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToBad2()
    {
      hashbag.CopyTo(a, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToTooFar()
    {
      hashbag.Add(3);
      hashbag.Add(8);
      hashbag.CopyTo(a, 9);
    }
  }



  [TestFixture]
  public class HashingEquals
  {
    private ICollection<int> h1, h2;


    [SetUp]
    public void Init()
    {
      h1 = new HashBag<int>();
      h2 = new LinkedList<int>();
    }


    [TearDown]
    public void Dispose()
    {
      h1 = h2 = null;
    }


    [Test]
    public void Hashing()
    {
      Assert.AreEqual(h1.GetUnsequencedHashCode(), h2.GetUnsequencedHashCode());
      h1.Add(7);
      h2.Add(9);
      Assert.IsTrue(h1.GetUnsequencedHashCode() != h2.GetUnsequencedHashCode());
      h2.Add(7);
      h1.Add(9);
      Assert.IsTrue(h1.GetUnsequencedHashCode() == h2.GetUnsequencedHashCode());
    }


    [Test]
    public void Equals()
    {
      Assert.IsTrue(h1.UnsequencedEquals(h2));
//            Code 1550734257, Pair (3 x 1602896434, 2 x 1186320090) number 169185 matched oth
//er pair (3 x -1615223932, 2 x 1019546595)
      h1.Add(1602896434);
      h1.Add(1186320090);
      h1.Add(1602896434);
      h1.Add(1186320090);
      h1.Add(1602896434);
      h2.Add(-1615223932);
      h2.Add(1019546595);
      h2.Add(-1615223932);
      h2.Add(1019546595);
      h2.Add(-1615223932);
      Assert.IsTrue(h1.GetUnsequencedHashCode() == h2.GetUnsequencedHashCode());
      Assert.IsTrue(!h1.UnsequencedEquals(h2));
      h1.Clear();
      h2.Clear();
      h1.Add(1);
      h1.Add(2);
      h2.Add(2);
      h2.Add(1);
      Assert.IsTrue(h1.GetUnsequencedHashCode() == h2.GetUnsequencedHashCode());
      Assert.IsTrue(h1.UnsequencedEquals(h2));
    }
  }
}
