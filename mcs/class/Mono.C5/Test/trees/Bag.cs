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


namespace C5UnitTests.trees.TreeBag
{
  using CollectionOfInt = TreeBag<int>;


  [TestFixture]
  public class NewTest
  {
    // Repro for bug20091113:
    [Test]
    public void A()
    {
      var list = new TreeBag<long>();
      // Sequence generated in FindNodeRandomTest
      // Manually pruned by sestoft 2009-11-14
      list.Add(553284);
      list.Add(155203);
      list.Add(316201);
      list.Add(263469);
      list.Add(263469);

      //list.dump();   // OK
      list.Remove(316201);
      //list.dump();   // Not OK
      Assert.IsTrue(list.Check());
    }
    [Test]
    public void B()
    {
      var l = 100;
      for (int r = 0; r < l; r++)
      {
        var list = new TreeBag<int>();
        for (int i = 0; i < l; i++)
        {
          list.Add(l - i);
          list.Add(l - i);
          list.Add(l - i);
        }
        list.Remove(r);
        list.Remove(r);
        //list.dump();
        list.Remove(r);
        Assert.IsTrue(list.Check("removing" + r));
      }
    }
  }

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestEvents()
    {
      Fun<CollectionOfInt> factory = delegate() { return new CollectionOfInt(TenEqualityComparer.Default); };
      new C5UnitTests.Templates.Events.SortedIndexedTester<CollectionOfInt>().Test(factory);
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
    public static ICollection<T> New<T>() { return new TreeBag<T>(); }
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
    public void Format()
    {
      Assert.AreEqual("{{  }}", coll.ToString());
      coll.AddAll<int>(new int[] { -4, 28, 129, 65530, -4, 28 });
      Assert.AreEqual("{{ -4(*2), 28(*2), 129(*1), 65530(*1) }}", coll.ToString());
      Assert.AreEqual("{{ -4(*2), 1C(*2), 81(*1), FFFA(*1) }}", coll.ToString(null, rad16));
      Assert.AreEqual("{{ -4(*2), 28(*2)... }}", coll.ToString("L18", null));
      Assert.AreEqual("{{ -4(*2), 1C(*2)... }}", coll.ToString("L18", rad16));
    }
  }

  [TestFixture]
  public class Combined
  {
    private IIndexedSorted<KeyValuePair<int, int>> lst;


    [SetUp]
    public void Init()
    {
      lst = new TreeBag<KeyValuePair<int, int>>(new KeyValuePairComparer<int, int>(new IC()));
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
      Assert.AreEqual(13, lst[11].Key);
      Assert.AreEqual(79, lst[11].Value);
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
        ICollection<String> coll = new TreeBag<String>();
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
  public class Simple
  {
    private TreeBag<string> bag;


    [SetUp]
    public void Init()
    {
      bag = new TreeBag<string>(new SC());
    }


    [TearDown]
    public void Dispose()
    {
      bag = null;
    }

    [Test]
    public void Initial()
    {
      bool res;

      Assert.IsFalse(bag.IsReadOnly);
      Assert.AreEqual(0, bag.Count, "new bag should be empty");
      Assert.AreEqual(0, bag.ContainsCount("A"));
      Assert.AreEqual(0, bag.ContainsCount("B"));
      Assert.AreEqual(0, bag.ContainsCount("C"));
      Assert.IsFalse(bag.Contains("A"));
      Assert.IsFalse(bag.Contains("B"));
      Assert.IsFalse(bag.Contains("C"));
      bag.Add("A");
      Assert.AreEqual(1, bag.Count);
      Assert.AreEqual(1, bag.ContainsCount("A"));
      Assert.AreEqual(0, bag.ContainsCount("B"));
      Assert.AreEqual(0, bag.ContainsCount("C"));
      Assert.IsTrue(bag.Contains("A"));
      Assert.IsFalse(bag.Contains("B"));
      Assert.IsFalse(bag.Contains("C"));
      bag.Add("C");
      Assert.AreEqual(2, bag.Count);
      Assert.AreEqual(1, bag.ContainsCount("A"));
      Assert.AreEqual(0, bag.ContainsCount("B"));
      Assert.AreEqual(1, bag.ContainsCount("C"));
      Assert.IsTrue(bag.Contains("A"));
      Assert.IsFalse(bag.Contains("B"));
      Assert.IsTrue(bag.Contains("C"));
      bag.Add("C");
      Assert.AreEqual(3, bag.Count);
      Assert.AreEqual(1, bag.ContainsCount("A"));
      Assert.AreEqual(0, bag.ContainsCount("B"));
      Assert.AreEqual(2, bag.ContainsCount("C"));
      Assert.IsTrue(bag.Contains("A"));
      Assert.IsFalse(bag.Contains("B"));
      Assert.IsTrue(bag.Contains("C"));
      bag.Add("B");
      bag.Add("C");
      Assert.AreEqual(5, bag.Count);
      Assert.AreEqual(1, bag.ContainsCount("A"));
      Assert.AreEqual(1, bag.ContainsCount("B"));
      Assert.AreEqual(3, bag.ContainsCount("C"));
      Assert.IsTrue(bag.Contains("A"));
      Assert.IsTrue(bag.Contains("B"));
      Assert.IsTrue(bag.Contains("C"));
      res = bag.Remove("C");
      Assert.AreEqual(4, bag.Count);
      Assert.AreEqual(1, bag.ContainsCount("A"));
      Assert.AreEqual(1, bag.ContainsCount("B"));
      Assert.AreEqual(2, bag.ContainsCount("C"));
      Assert.IsTrue(bag.Contains("A"));
      Assert.IsTrue(bag.Contains("B"));
      Assert.IsTrue(bag.Contains("C"));
      res = bag.Remove("A");
      Assert.AreEqual(3, bag.Count);
      Assert.AreEqual(0, bag.ContainsCount("A"));
      Assert.AreEqual(1, bag.ContainsCount("B"));
      Assert.AreEqual(2, bag.ContainsCount("C"));
      Assert.IsFalse(bag.Contains("A"));
      Assert.IsTrue(bag.Contains("B"));
      Assert.IsTrue(bag.Contains("C"));
      bag.RemoveAllCopies("C");
      Assert.AreEqual(1, bag.Count);
      Assert.AreEqual(0, bag.ContainsCount("A"));
      Assert.AreEqual(1, bag.ContainsCount("B"));
      Assert.AreEqual(0, bag.ContainsCount("C"));
      Assert.IsFalse(bag.Contains("A"));
      Assert.IsTrue(bag.Contains("B"));
      Assert.IsFalse(bag.Contains("C"));
      Assert.IsFalse(bag.Contains("Z"));
      Assert.IsFalse(bag.Remove("Z"));
      bag.RemoveAllCopies("Z");
      Assert.AreEqual(1, bag.Count);
      Assert.AreEqual(0, bag.ContainsCount("A"));
      Assert.AreEqual(1, bag.ContainsCount("B"));
      Assert.AreEqual(0, bag.ContainsCount("C"));
      Assert.IsFalse(bag.Contains("A"));
      Assert.IsTrue(bag.Contains("B"));
      Assert.IsFalse(bag.Contains("C"));
      Assert.IsFalse(bag.Contains("Z"));
    }
  }

  [TestFixture]
  public class FindOrAdd
  {
    private TreeBag<KeyValuePair<int, string>> bag;


    [SetUp]
    public void Init()
    {
      bag = new TreeBag<KeyValuePair<int, string>>(new KeyValuePairComparer<int, string>(new IC()));
    }


    [TearDown]
    public void Dispose()
    {
      bag = null;
    }


    [Test]
    public void Test()
    {
      KeyValuePair<int, string> p = new KeyValuePair<int, string>(3, "tre");
      Assert.IsFalse(bag.FindOrAdd(ref p));
      p.Value = "drei";
      Assert.IsTrue(bag.FindOrAdd(ref p));
      Assert.AreEqual("tre", p.Value);
      p.Value = "three";
      Assert.AreEqual(2, bag.ContainsCount(p));
      Assert.AreEqual("tre", bag[0].Value);
    }
  }


  [TestFixture]
  public class Enumerators
  {
    private TreeBag<string> bag;

    private SCG.IEnumerator<string> bagenum;


    [SetUp]
    public void Init()
    {
      bag = new TreeBag<string>(new SC());
      foreach (string s in new string[] { "A", "B", "A", "A", "B", "C", "D", "B" })
        bag.Add(s);

      bagenum = bag.GetEnumerator();
    }


    [TearDown]
    public void Dispose()
    {
      bagenum = null;
      bag = null;
    }


    [Test]
    [ExpectedException(typeof(CollectionModifiedException))]
    public void MoveNextOnModified()
    {
      //TODO: also problem before first MoveNext!!!!!!!!!!
      bagenum.MoveNext();
      bag.Add("T");
      bagenum.MoveNext();
    }


    [Test]
    public void NormalUse()
    {
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "A");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "A");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "A");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "B");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "B");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "B");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "C");
      Assert.IsTrue(bagenum.MoveNext());
      Assert.AreEqual(bagenum.Current, "D");
      Assert.IsFalse(bagenum.MoveNext());
    }
  }

  [TestFixture]
  public class Ranges
  {
    private TreeBag<int> tree;

    private SCG.IComparer<int> c;


    [SetUp]
    public void Init()
    {
      c = new IC();
      tree = new TreeBag<int>(c);
      for (int i = 1; i <= 10; i++)
      {
        tree.Add(i * 2); tree.Add(i);
      }
    }


    [Test]
    public void Enumerator()
    {
      SCG.IEnumerator<int> e = tree.RangeFromTo(5, 17).GetEnumerator();
      int i = 0;
      int[] all = new int[] { 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16 };
      while (e.MoveNext())
      {
        Assert.AreEqual(all[i++], e.Current);
      }

      Assert.AreEqual(12, i);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Enumerator2()
    {
      SCG.IEnumerator<int> e = tree.RangeFromTo(5, 17).GetEnumerator();
      int i = e.Current;
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Enumerator3()
    {
      SCG.IEnumerator<int> e = tree.RangeFromTo(5, 17).GetEnumerator();

      while (e.MoveNext()) ;

      int i = e.Current;
    }


    [Test]
    public void Remove()
    {
      int[] all = new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18, 20 };

      tree.RemoveRangeFrom(18);
      Assert.IsTrue(IC.eq(tree, new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16 }));
      tree.RemoveRangeFrom(28);
      Assert.IsTrue(IC.eq(tree, new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16 }));
      tree.RemoveRangeFrom(1);
      Assert.IsTrue(IC.eq(tree));
      foreach (int i in all) tree.Add(i);

      tree.RemoveRangeTo(10);
      Assert.IsTrue(IC.eq(tree, new int[] { 10, 10, 12, 14, 16, 18, 20 }));
      tree.RemoveRangeTo(2);
      Assert.IsTrue(IC.eq(tree, new int[] { 10, 10, 12, 14, 16, 18, 20 }));
      tree.RemoveRangeTo(21);
      Assert.IsTrue(IC.eq(tree));
      foreach (int i in all) tree.Add(i);

      tree.RemoveRangeFromTo(4, 8);
      Assert.IsTrue(IC.eq(tree, 1, 2, 2, 3, 8, 8, 9, 10, 10, 12, 14, 16, 18, 20));
      tree.RemoveRangeFromTo(14, 28);
      Assert.IsTrue(IC.eq(tree, 1, 2, 2, 3, 8, 8, 9, 10, 10, 12));
      tree.RemoveRangeFromTo(0, 9);
      Assert.IsTrue(IC.eq(tree, 9, 10, 10, 12));
      tree.RemoveRangeFromTo(0, 81);
      Assert.IsTrue(IC.eq(tree));
    }


    [Test]
    public void Normal()
    {
      int[] all = new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18, 20 };

      Assert.IsTrue(IC.eq(tree, all));
      Assert.IsTrue(IC.eq(tree.RangeAll(), all));
      Assert.AreEqual(20, tree.RangeAll().Count);
      Assert.IsTrue(IC.eq(tree.RangeFrom(11), new int[] { 12, 14, 16, 18, 20 }));
      Assert.AreEqual(5, tree.RangeFrom(11).Count);
      Assert.IsTrue(IC.eq(tree.RangeFrom(12), new int[] { 12, 14, 16, 18, 20 }));
      Assert.IsTrue(IC.eq(tree.RangeFrom(1), all));
      Assert.IsTrue(IC.eq(tree.RangeFrom(0), all));
      Assert.IsTrue(IC.eq(tree.RangeFrom(21), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeFrom(20), new int[] { 20 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(8), new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(7), new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6 }));
      Assert.AreEqual(9, tree.RangeTo(7).Count);
      Assert.IsTrue(IC.eq(tree.RangeTo(1), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeTo(0), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeTo(3), new int[] { 1, 2, 2 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(20), new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(21), all));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(7, 12), new int[] { 7, 8, 8, 9, 10, 10 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 11), new int[] { 6, 6, 7, 8, 8, 9, 10, 10 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(1, 12), new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10 }));
      Assert.AreEqual(15, tree.RangeFromTo(1, 12).Count);
      Assert.IsTrue(IC.eq(tree.RangeFromTo(2, 12), new int[] { 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 21), new int[] { 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18, 20 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 20), new int[] { 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18 }));
    }


    [Test]
    public void Backwards()
    {
      int[] all = new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9, 10, 10, 12, 14, 16, 18, 20 };
      int[] lla = new int[] { 20, 18, 16, 14, 12, 10, 10, 9, 8, 8, 7, 6, 6, 5, 4, 4, 3, 2, 2, 1 };

      Assert.IsTrue(IC.eq(tree, all));
      Assert.IsTrue(IC.eq(tree.RangeAll().Backwards(), lla));
      Assert.IsTrue(IC.eq(tree.RangeFrom(11).Backwards(), new int[] { 20, 18, 16, 14, 12 }));
      Assert.IsTrue(IC.eq(tree.RangeFrom(12).Backwards(), new int[] { 20, 18, 16, 14, 12 }));
      Assert.IsTrue(IC.eq(tree.RangeFrom(1).Backwards(), lla));
      Assert.IsTrue(IC.eq(tree.RangeFrom(0).Backwards(), lla));
      Assert.IsTrue(IC.eq(tree.RangeFrom(21).Backwards(), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeFrom(20).Backwards(), new int[] { 20 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(8).Backwards(), new int[] { 7, 6, 6, 5, 4, 4, 3, 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(7).Backwards(), new int[] { 6, 6, 5, 4, 4, 3, 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(1).Backwards(), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeTo(0).Backwards(), new int[] { }));
      Assert.IsTrue(IC.eq(tree.RangeTo(3).Backwards(), new int[] { 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(20).Backwards(), new int[] { 18, 16, 14, 12, 10, 10, 9, 8, 8, 7, 6, 6, 5, 4, 4, 3, 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeTo(21).Backwards(), lla));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(7, 12).Backwards(), new int[] { 10, 10, 9, 8, 8, 7 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 11).Backwards(), new int[] { 10, 10, 9, 8, 8, 7, 6, 6 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(0, 12).Backwards(), new int[] { 10, 10, 9, 8, 8, 7, 6, 6, 5, 4, 4, 3, 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(1, 12).Backwards(), new int[] { 10, 10, 9, 8, 8, 7, 6, 6, 5, 4, 4, 3, 2, 2, 1 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 21).Backwards(), new int[] { 20, 18, 16, 14, 12, 10, 10, 9, 8, 8, 7, 6, 6 }));
      Assert.IsTrue(IC.eq(tree.RangeFromTo(6, 20).Backwards(), new int[] { 18, 16, 14, 12, 10, 10, 9, 8, 8, 7, 6, 6 }));
    }


    [Test]
    public void Direction()
    {
      Assert.AreEqual(EnumerationDirection.Forwards, tree.Direction);
      Assert.AreEqual(EnumerationDirection.Forwards, tree.RangeFrom(20).Direction);
      Assert.AreEqual(EnumerationDirection.Forwards, tree.RangeTo(7).Direction);
      Assert.AreEqual(EnumerationDirection.Forwards, tree.RangeFromTo(1, 12).Direction);
      Assert.AreEqual(EnumerationDirection.Forwards, tree.RangeAll().Direction);
      Assert.AreEqual(EnumerationDirection.Backwards, tree.Backwards().Direction);
      Assert.AreEqual(EnumerationDirection.Backwards, tree.RangeFrom(20).Backwards().Direction);
      Assert.AreEqual(EnumerationDirection.Backwards, tree.RangeTo(7).Backwards().Direction);
      Assert.AreEqual(EnumerationDirection.Backwards, tree.RangeFromTo(1, 12).Backwards().Direction);
      Assert.AreEqual(EnumerationDirection.Backwards, tree.RangeAll().Backwards().Direction);
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
      c = null;
    }
  }



  [TestFixture]
  public class BagItf
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
      for (int i = 10; i < 20; i++)
      {
        tree.Add(i);
        tree.Add(i + 5);
      }
    }


    [Test]
    public void Both()
    {
      Assert.AreEqual(0, tree.ContainsCount(7));
      Assert.AreEqual(1, tree.ContainsCount(10));
      Assert.AreEqual(2, tree.ContainsCount(17));
      tree.RemoveAllCopies(17);
      Assert.AreEqual(0, tree.ContainsCount(17));
      tree.RemoveAllCopies(7);
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }



  [TestFixture]
  public class Div
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
    }


    private void loadup()
    {
      for (int i = 10; i < 20; i++)
      {
        tree.Add(i);
        tree.Add(i + 5);
      }
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor1()
    {
      new TreeBag<int>(null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor3()
    {
      new TreeBag<int>(null, EqualityComparer<int>.Default);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor4()
    {
      new TreeBag<int>(Comparer<int>.Default, null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor5()
    {
      new TreeBag<int>(null, null);
    }

    [Test]
    public void Choose()
    {
      tree.Add(7);
      Assert.AreEqual(7, tree.Choose());
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadChoose()
    {
      tree.Choose();
    }


    [Test]
    public void NoDuplicates()
    {
      Assert.IsTrue(tree.AllowsDuplicates);
      loadup();
      Assert.IsTrue(tree.AllowsDuplicates);
    }


    [Test]
    public void Add()
    {
      Assert.IsTrue(tree.Add(17));
      Assert.IsTrue(tree.Add(17));
      Assert.IsTrue(tree.Add(18));
      Assert.IsTrue(tree.Add(18));
      Assert.AreEqual(4, tree.Count);
      Assert.IsTrue(IC.eq(tree, 17, 17, 18, 18));
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }

  [TestFixture]
  public class FindPredicate
  {
    private TreeBag<int> list;
    Fun<int, bool> pred;

    [SetUp]
    public void Init()
    {
      list = new TreeBag<int>(TenEqualityComparer.Default);
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
      Assert.AreEqual(3, list.FindIndex(pred));
    }

    [Test]
    public void FindLastIndex()
    {
      Assert.IsFalse(0 <= list.FindLastIndex(pred));
      list.AddAll<int>(new int[] { 4, 22, 67, 37 });
      Assert.IsFalse(0 <= list.FindLastIndex(pred));
      list.AddAll<int>(new int[] { 45, 122, 675, 137 });
      Assert.AreEqual(7, list.FindLastIndex(pred));
    }
  }

  [TestFixture]
  public class UniqueItems
  {
    private TreeBag<int> list;

    [SetUp]
    public void Init() { list = new TreeBag<int>(); }

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
    private TreeBag<int> tree;

    int[] a;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
      a = new int[10];
      for (int i = 0; i < 10; i++)
        a[i] = 1000 + i;
    }


    [TearDown]
    public void Dispose() { tree = null; }


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
      Assert.AreEqual("Alles klar", aeq(tree.ToArray()));
      tree.Add(4);
      tree.Add(7);
      tree.Add(4);
      Assert.AreEqual("Alles klar", aeq(tree.ToArray(), 4, 4, 7));
    }


    [Test]
    public void CopyTo()
    {
      tree.CopyTo(a, 1);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
      tree.Add(6);
      tree.Add(6);
      tree.CopyTo(a, 2);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 6, 1004, 1005, 1006, 1007, 1008, 1009));
      tree.Add(4);
      tree.Add(9);
      tree.CopyTo(a, 4);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 6, 4, 6, 6, 9, 1008, 1009));
      tree.Clear();
      tree.Add(7);
      tree.CopyTo(a, 9);
      Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 6, 4, 6, 6, 9, 1008, 7));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToBad()
    {
      tree.CopyTo(a, 11);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToBad2()
    {
      tree.CopyTo(a, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToTooFar()
    {
      tree.Add(3);
      tree.Add(4);
      tree.CopyTo(a, 9);
    }
  }



  [TestFixture]
  public class Remove
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
      for (int i = 10; i < 20; i++)
      {
        tree.Add(i);
        tree.Add(i + 5);
      }
      //10,11,12,13,14,15,15,16,16,17,17,18,18,19,19,20,21,22,23,24
    }


    [Test]
    public void SmallTrees()
    {
      tree.Clear();
      tree.Add(9);
      tree.Add(7);
      tree.Add(9);
      Assert.IsTrue(tree.Remove(7));
      Assert.IsTrue(tree.Check(""));
    }


    [Test]
    public void ByIndex()
    {
      //Remove root!
      int n = tree.Count;
      int i = tree[10];

      Assert.AreEqual(17, tree.RemoveAt(10));
      Assert.IsTrue(tree.Check(""));
      Assert.IsTrue(tree.Contains(i));
      Assert.AreEqual(n - 1, tree.Count);
      Assert.AreEqual(17, tree.RemoveAt(9));
      Assert.IsTrue(tree.Check(""));
      Assert.IsFalse(tree.Contains(i));
      Assert.AreEqual(n - 2, tree.Count);

      //Low end
      i = tree.FindMin();
      tree.RemoveAt(0);
      Assert.IsTrue(tree.Check(""));
      Assert.IsFalse(tree.Contains(i));
      Assert.AreEqual(n - 3, tree.Count);

      //high end
      i = tree.FindMax();
      tree.RemoveAt(tree.Count - 1);
      Assert.IsTrue(tree.Check(""));
      Assert.IsFalse(tree.Contains(i));
      Assert.AreEqual(n - 4, tree.Count);

      //Some leaf
      //tree.dump();
      i = 18;
      Assert.AreEqual(i, tree.RemoveAt(9));
      Assert.IsTrue(tree.Check(""));
      Assert.AreEqual(i, tree.RemoveAt(8));
      Assert.IsTrue(tree.Check(""));
      Assert.IsFalse(tree.Contains(i));
      Assert.AreEqual(n - 6, tree.Count);
    }


    [Test]
    public void AlmostEmpty()
    {
      //Almost empty
      tree.Clear();
      tree.Add(3);
      tree.RemoveAt(0);
      Assert.IsTrue(tree.Check(""));
      Assert.IsFalse(tree.Contains(3));
      Assert.AreEqual(0, tree.Count);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException), ExpectedMessage="Index out of range for sequenced collectionvalue")]
    public void Empty()
    {
      tree.Clear();
      tree.RemoveAt(0);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException), ExpectedMessage="Index out of range for sequenced collectionvalue")]
    public void HighIndex()
    {
      tree.RemoveAt(tree.Count);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException), ExpectedMessage="Index out of range for sequenced collectionvalue")]
    public void LowIndex()
    {
      tree.RemoveAt(-1);
    }


    [Test]
    public void Normal()
    {
      //Note: ids does not match for bag
      Assert.IsFalse(tree.Remove(-20));

      //1b
      Assert.IsTrue(tree.Remove(20));
      Assert.IsTrue(tree.Check("T1"));
      Assert.IsFalse(tree.Remove(20));

      //1b
      Assert.IsTrue(tree.Remove(10));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //case 1c
      Assert.IsTrue(tree.Remove(24));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //1a (terminating)
      Assert.IsTrue(tree.Remove(16));
      Assert.IsTrue(tree.Remove(16));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //2
      Assert.IsTrue(tree.Remove(18));
      Assert.IsTrue(tree.Remove(17));
      Assert.IsTrue(tree.Remove(18));
      Assert.IsTrue(tree.Remove(17));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //2+1b
      Assert.IsTrue(tree.Remove(15));
      Assert.IsTrue(tree.Remove(15));
      for (int i = 0; i < 5; i++) tree.Add(17 + i);
      Assert.IsTrue(tree.Remove(23));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //1a+1b
      Assert.IsTrue(tree.Remove(11));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //2+1c
      for (int i = 0; i < 10; i++)
        tree.Add(50 - 2 * i);

      Assert.IsTrue(tree.Remove(42));
      Assert.IsTrue(tree.Remove(38));
      Assert.IsTrue(tree.Remove(22));
      Assert.IsTrue(tree.Remove(40));

      //
      for (int i = 0; i < 48; i++)
        tree.Remove(i);

      //Almost empty tree:*
      Assert.IsFalse(tree.Remove(26));
      Assert.IsTrue(tree.Remove(48));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");

      //Empty tree:*
      Assert.IsFalse(tree.Remove(26));
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }



  [TestFixture]
  public class PredecessorStructure
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
    }


    private void loadup()
    {
      for (int i = 0; i < 20; i++)
        tree.Add(2 * i);
      for (int i = 0; i < 10; i++)
        tree.Add(4 * i);
    }


    [Test]
    public void Predecessor()
    {
      loadup();
      Assert.AreEqual(6, tree.Predecessor(7));
      Assert.AreEqual(6, tree.Predecessor(8));

      //The bottom
      Assert.AreEqual(0, tree.Predecessor(1));

      //The top
      Assert.AreEqual(38, tree.Predecessor(39));
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void PredecessorTooLow1()
    {
      tree.Predecessor(-2);
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void PredecessorTooLow2()
    {
      tree.Predecessor(0);
    }


    [Test]
    public void WeakPredecessor()
    {
      loadup();
      Assert.AreEqual(6, tree.WeakPredecessor(7));
      Assert.AreEqual(8, tree.WeakPredecessor(8));

      //The bottom
      Assert.AreEqual(0, tree.WeakPredecessor(1));
      Assert.AreEqual(0, tree.WeakPredecessor(0));

      //The top
      Assert.AreEqual(38, tree.WeakPredecessor(39));
      Assert.AreEqual(38, tree.WeakPredecessor(38));
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void WeakPredecessorTooLow1()
    {
      tree.WeakPredecessor(-2);
    }


    [Test]
    public void Successor()
    {
      loadup();
      Assert.AreEqual(8, tree.Successor(7));
      Assert.AreEqual(10, tree.Successor(8));

      //The bottom
      Assert.AreEqual(2, tree.Successor(0));
      Assert.AreEqual(0, tree.Successor(-1));

      //The top
      Assert.AreEqual(38, tree.Successor(37));
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void SuccessorTooHigh1()
    {
      tree.Successor(38);
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void SuccessorTooHigh2()
    {
      tree.Successor(39);
    }


    [Test]
    public void WeakSuccessor()
    {
      loadup();
      Assert.AreEqual(6, tree.WeakSuccessor(6));
      Assert.AreEqual(8, tree.WeakSuccessor(7));

      //The bottom
      Assert.AreEqual(0, tree.WeakSuccessor(-1));
      Assert.AreEqual(0, tree.WeakSuccessor(0));

      //The top
      Assert.AreEqual(38, tree.WeakSuccessor(37));
      Assert.AreEqual(38, tree.WeakSuccessor(38));
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void WeakSuccessorTooHigh1()
    {
      tree.WeakSuccessor(39);
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }



  [TestFixture]
  public class PriorityQueue
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
    }


    private void loadup()
    {
      foreach (int i in new int[] { 1, 2, 3, 4 })
        tree.Add(i);
      tree.Add(1);
      tree.Add(3);
    }


    [Test]
    public void Normal()
    {
      loadup();
      Assert.AreEqual(1, tree.FindMin());
      Assert.AreEqual(4, tree.FindMax());
      Assert.AreEqual(1, tree.DeleteMin());
      Assert.AreEqual(4, tree.DeleteMax());
      Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
      Assert.AreEqual(1, tree.FindMin());
      Assert.AreEqual(3, tree.FindMax());
      Assert.AreEqual(1, tree.DeleteMin());
      Assert.AreEqual(3, tree.DeleteMax());
      Assert.IsTrue(tree.Check("Normal test 2"), "Bad tree");
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void Empty1()
    {
      tree.FindMin();
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void Empty2()
    {
      tree.FindMax();
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void Empty3()
    {
      tree.DeleteMin();
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void Empty4()
    {
      tree.DeleteMax();
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }



  [TestFixture]
  public class IndexingAndCounting
  {
    private TreeBag<int> tree;


    [SetUp]
    public void Init()
    {
      tree = new TreeBag<int>(new IC());
    }


    private void populate()
    {
      tree.Add(30);
      tree.Add(30);
      tree.Add(50);
      tree.Add(10);
      tree.Add(70);
      tree.Add(70);
    }


    [Test]
    public void ToArray()
    {
      populate();

      int[] a = tree.ToArray();

      Assert.AreEqual(6, a.Length);
      Assert.AreEqual(10, a[0]);
      Assert.AreEqual(30, a[1]);
      Assert.AreEqual(30, a[2]);
      Assert.AreEqual(50, a[3]);
      Assert.AreEqual(70, a[4]);
      Assert.AreEqual(70, a[5]);
    }


    [Test]
    public void GoodIndex()
    {
      Assert.AreEqual(-1, tree.IndexOf(20));
      Assert.AreEqual(-1, tree.LastIndexOf(20));
      populate();
      Assert.AreEqual(10, tree[0]);
      Assert.AreEqual(30, tree[1]);
      Assert.AreEqual(30, tree[2]);
      Assert.AreEqual(50, tree[3]);
      Assert.AreEqual(70, tree[4]);
      Assert.AreEqual(70, tree[5]);
      Assert.AreEqual(0, tree.IndexOf(10));
      Assert.AreEqual(1, tree.IndexOf(30));
      Assert.AreEqual(3, tree.IndexOf(50));
      Assert.AreEqual(4, tree.IndexOf(70));
      Assert.AreEqual(~1, tree.IndexOf(20));
      Assert.AreEqual(~0, tree.IndexOf(0));
      Assert.AreEqual(~6, tree.IndexOf(90));
      Assert.AreEqual(0, tree.LastIndexOf(10));
      Assert.AreEqual(2, tree.LastIndexOf(30));
      Assert.AreEqual(3, tree.LastIndexOf(50));
      Assert.AreEqual(5, tree.LastIndexOf(70));
      Assert.AreEqual(~1, tree.LastIndexOf(20));
      Assert.AreEqual(~0, tree.LastIndexOf(0));
      Assert.AreEqual(~6, tree.LastIndexOf(90));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexTooLarge()
    {
      populate();
      Console.WriteLine(tree[6]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void IndexTooSmall()
    {
      populate();
      Console.WriteLine(tree[-1]);
    }


    [Test]
    public void FilledTreeOutsideInput()
    {
      populate();
      Assert.AreEqual(0, tree.CountFrom(90));
      Assert.AreEqual(0, tree.CountFromTo(-20, 0));
      Assert.AreEqual(0, tree.CountFromTo(80, 100));
      Assert.AreEqual(0, tree.CountTo(0));
      Assert.AreEqual(6, tree.CountTo(90));
      Assert.AreEqual(6, tree.CountFromTo(-20, 90));
      Assert.AreEqual(6, tree.CountFrom(0));
    }


    [Test]
    public void FilledTreeIntermediateInput()
    {
      populate();
      Assert.AreEqual(5, tree.CountFrom(20));
      Assert.AreEqual(2, tree.CountFromTo(20, 40));
      Assert.AreEqual(3, tree.CountTo(40));
    }


    [Test]
    public void FilledTreeMatchingInput()
    {
      populate();
      Assert.AreEqual(5, tree.CountFrom(30));
      Assert.AreEqual(3, tree.CountFromTo(30, 70));
      Assert.AreEqual(0, tree.CountFromTo(50, 30));
      Assert.AreEqual(0, tree.CountFromTo(50, 50));
      Assert.AreEqual(0, tree.CountTo(10));
      Assert.AreEqual(3, tree.CountTo(50));
    }


    [Test]
    public void CountEmptyTree()
    {
      Assert.AreEqual(0, tree.CountFrom(20));
      Assert.AreEqual(0, tree.CountFromTo(20, 40));
      Assert.AreEqual(0, tree.CountTo(40));
    }


    [TearDown]
    public void Dispose()
    {
      tree = null;
    }
  }




  namespace ModificationCheck
  {
    [TestFixture]
    public class Enumerator
    {
      private TreeBag<int> tree;

      private SCG.IEnumerator<int> e;


      [SetUp]
      public void Init()
      {
        tree = new TreeBag<int>(new IC());
        for (int i = 0; i < 10; i++)
          tree.Add(i);
        tree.Add(3);
        tree.Add(7);
        e = tree.GetEnumerator();
      }


      [Test]
      public void CurrentAfterModification()
      {
        e.MoveNext();
        tree.Add(34);
        Assert.AreEqual(0, e.Current);
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterAdd()
      {
        tree.Add(34);
        e.MoveNext();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterRemove()
      {
        tree.Remove(34);
        e.MoveNext();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterClear()
      {
        tree.Clear();
        e.MoveNext();
      }


      [TearDown]
      public void Dispose()
      {
        tree = null;
        e = null;
      }
    }



    [TestFixture]
    public class RangeEnumerator
    {
      private TreeBag<int> tree;

      private SCG.IEnumerator<int> e;


      [SetUp]
      public void Init()
      {
        tree = new TreeBag<int>(new IC());
        for (int i = 0; i < 10; i++)
          tree.Add(i);

        e = tree.RangeFromTo(3, 7).GetEnumerator();
      }


      [Test]
      public void CurrentAfterModification()
      {
        e.MoveNext();
        tree.Add(34);
        Assert.AreEqual(3, e.Current);
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterAdd()
      {
        tree.Add(34);
        e.MoveNext();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterRemove()
      {
        tree.Remove(34);
        e.MoveNext();
      }


      [Test]
      [ExpectedException(typeof(CollectionModifiedException))]
      public void MoveNextAfterClear()
      {
        tree.Clear();
        e.MoveNext();
      }


      [TearDown]
      public void Dispose()
      {
        tree = null;
        e = null;
      }
    }
  }




  namespace PathcopyPersistence
  {
    [TestFixture]
    public class Navigation
    {
      private TreeBag<int> tree, snap;

      private SCG.IComparer<int> ic;


      [SetUp]
      public void Init()
      {
        ic = new IC();
        tree = new TreeBag<int>(ic);
        for (int i = 0; i <= 20; i++)
          tree.Add(2 * i + 1);
        tree.Add(13);
        snap = (TreeBag<int>)tree.Snapshot();
        for (int i = 0; i <= 10; i++)
          tree.Remove(4 * i + 1);
      }


      private bool twomodeleven(int i)
      {
        return i % 11 == 2;
      }


      [Test]
      public void InternalEnum()
      {
        Assert.IsTrue(IC.eq(snap.FindAll(new Fun<int, bool>(twomodeleven)), 13, 13, 35));
      }


      public void MoreCut()
      {
        //TODO: Assert.Fail("more tests of Cut needed");
      }


      [Test]
      public void Cut()
      {
        int lo, hi;
        bool lv, hv;

        Assert.IsFalse(snap.Cut(new HigherOrder.CubeRoot(64), out lo, out lv, out hi, out hv));
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(5, hi);
        Assert.AreEqual(3, lo);
        Assert.IsTrue(snap.Cut(new HigherOrder.CubeRoot(125), out lo, out lv, out hi, out hv));
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(7, hi);
        Assert.AreEqual(3, lo);
        Assert.IsFalse(snap.Cut(new HigherOrder.CubeRoot(125000), out lo, out lv, out hi, out hv));
        Assert.IsTrue(lv && !hv);
        Assert.AreEqual(41, lo);
        Assert.IsFalse(snap.Cut(new HigherOrder.CubeRoot(-27), out lo, out lv, out hi, out hv));
        Assert.IsTrue(!lv && hv);
        Assert.AreEqual(1, hi);
      }


      [Test]
      public void Range()
      {
        Assert.IsTrue(IC.eq(snap.RangeFromTo(5, 16), 5, 7, 9, 11, 13, 13, 15));
        Assert.IsTrue(IC.eq(snap.RangeFromTo(5, 17), 5, 7, 9, 11, 13, 13, 15));
        Assert.IsTrue(IC.eq(snap.RangeFromTo(6, 16), 7, 9, 11, 13, 13, 15));
      }


      [Test]
      public void Contains()
      {
        Assert.IsTrue(snap.Contains(5));
        Assert.IsTrue(snap.Contains(13));
        Assert.AreEqual(1, snap.ContainsCount(5));
        Assert.AreEqual(2, snap.ContainsCount(13));
      }


      [Test]
      public void FindMin()
      {
        Assert.AreEqual(1, snap.FindMin());
      }


      [Test]
      public void FindMax()
      {
        Assert.AreEqual(41, snap.FindMax());
      }


      [Test]
      public void Predecessor()
      {
        Assert.AreEqual(13, snap.Predecessor(15));
        Assert.AreEqual(15, snap.Predecessor(16));
        Assert.AreEqual(15, snap.Predecessor(17));
        Assert.AreEqual(17, snap.Predecessor(18));
      }


      [Test]
      public void Successor()
      {
        Assert.AreEqual(17, snap.Successor(15));
        Assert.AreEqual(17, snap.Successor(16));
        Assert.AreEqual(19, snap.Successor(17));
        Assert.AreEqual(19, snap.Successor(18));
      }


      [Test]
      public void WeakPredecessor()
      {
        Assert.AreEqual(15, snap.WeakPredecessor(15));
        Assert.AreEqual(15, snap.WeakPredecessor(16));
        Assert.AreEqual(17, snap.WeakPredecessor(17));
        Assert.AreEqual(17, snap.WeakPredecessor(18));
      }


      [Test]
      public void WeakSuccessor()
      {
        Assert.AreEqual(15, snap.WeakSuccessor(15));
        Assert.AreEqual(17, snap.WeakSuccessor(16));
        Assert.AreEqual(17, snap.WeakSuccessor(17));
        Assert.AreEqual(19, snap.WeakSuccessor(18));
      }


      [Test]
      [ExpectedException(typeof(NotSupportedException), ExpectedMessage="Indexing not supported for snapshots")]
      public void CountTo()
      {
        int j = snap.CountTo(15);
      }


      [Test]
      [ExpectedException(typeof(NotSupportedException), ExpectedMessage="Indexing not supported for snapshots")]
      public void Indexing()
      {
        int j = snap[4];
      }


      [Test]
      [ExpectedException(typeof(NotSupportedException), ExpectedMessage="Indexing not supported for snapshots")]
      public void Indexing2()
      {
        int j = snap.IndexOf(5);
      }


      [TearDown]
      public void Dispose()
      {
        tree = null;
        ic = null;
      }
    }



    [TestFixture]
    public class Single
    {
      private TreeBag<int> tree;

      private SCG.IComparer<int> ic;


      [SetUp]
      public void Init()
      {
        ic = new IC();
        tree = new TreeBag<int>(ic);
        for (int i = 0; i < 10; i++)
          tree.Add(2 * i + 1);
      }


      [Test]
      public void EnumerationWithAdd()
      {
        int[] orig = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        int i = 0;
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        foreach (int j in snap)
        {
          Assert.AreEqual(1 + 2 * i++, j);
          tree.Add(21 - j);
          Assert.IsTrue(snap.Check("M"), "Bad snap!");
          Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
          Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        }
      }


      [Test]
      public void Remove()
      {
        int[] orig = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        tree.Remove(19);
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
      }


      [Test]
      public void RemoveNormal()
      {
        tree.Clear();
        for (int i = 10; i < 20; i++)
        {
          tree.Add(i);
          tree.Add(i + 10);
        }
        tree.Add(15);

        int[] orig = new int[] { 10, 11, 12, 13, 14, 15, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        Assert.IsFalse(tree.Remove(-20));
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");


        //decrease items case
        Assert.IsTrue(tree.Remove(15));
        Assert.IsTrue(tree.Check("T1"));
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        //snap.dump();
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //No demote case, with move_item
        Assert.IsTrue(tree.Remove(20));
        Assert.IsTrue(tree.Check("T1"));
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsFalse(tree.Remove(20));

        //plain case 2
        tree.Snapshot();
        Assert.IsTrue(tree.Remove(14));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //case 1b
        Assert.IsTrue(tree.Remove(25));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //case 1c
        Assert.IsTrue(tree.Remove(29));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //1a (terminating)
        Assert.IsTrue(tree.Remove(10));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //2+1b
        Assert.IsTrue(tree.Remove(12));
        tree.Snapshot();
        Assert.IsTrue(tree.Remove(11));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //1a+1b
        Assert.IsTrue(tree.Remove(18));
        Assert.IsTrue(tree.Remove(13));
        Assert.IsTrue(tree.Remove(15));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //2+1c
        for (int i = 0; i < 10; i++)
          tree.Add(50 - 2 * i);

        Assert.IsTrue(tree.Remove(42));
        Assert.IsTrue(tree.Remove(38));
        Assert.IsTrue(tree.Remove(28));
        Assert.IsTrue(tree.Remove(40));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //
        Assert.IsTrue(tree.Remove(16));
        Assert.IsTrue(tree.Remove(23));
        Assert.IsTrue(tree.Remove(17));
        Assert.IsTrue(tree.Remove(19));
        Assert.IsTrue(tree.Remove(50));
        Assert.IsTrue(tree.Remove(26));
        Assert.IsTrue(tree.Remove(21));
        Assert.IsTrue(tree.Remove(22));
        Assert.IsTrue(tree.Remove(24));
        for (int i = 0; i < 48; i++)
          tree.Remove(i);

        //Almost empty tree:
        Assert.IsFalse(tree.Remove(26));
        Assert.IsTrue(tree.Remove(48));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //Empty tree:
        Assert.IsFalse(tree.Remove(26));
        Assert.IsTrue(tree.Check("Normal test 1"), "Bad tree");
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
      }


      [Test]
      public void Add()
      {
        int[] orig = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        tree.Add(10);
        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        tree.Add(16);
        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");

        tree.Add(9);
        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");

        //Promote+zigzig
        tree.Add(40);
        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        for (int i = 1; i < 4; i++)
          tree.Add(40 - 2 * i);

        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");

        //Zigzag:
        tree.Add(32);
        Assert.IsTrue(snap.Check("M"), "Bad snap!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
      }


      [Test]
      public void Clear()
      {
        int[] orig = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        tree.Clear();
        Assert.IsTrue(snap.Check("Snap"), "Bad snap!");
        Assert.IsTrue(tree.Check("Tree"), "Bad tree!");
        Assert.IsTrue(IC.eq(snap, orig), "Snap was changed!");
        Assert.AreEqual(0, tree.Count);
      }


      [Test]
      [ExpectedException(typeof(InvalidOperationException), ExpectedMessage="Cannot snapshot a snapshot")]
      public void SnapSnap()
      {
        TreeBag<int> snap = (TreeBag<int>)tree.Snapshot();

        snap.Snapshot();
      }


      [TearDown]
      public void Dispose()
      {
        tree = null;
        ic = null;
      }
    }



    [TestFixture]
    public class Multiple
    {
      private TreeBag<int> tree;

      private SCG.IComparer<int> ic;


      private bool eq(SCG.IEnumerable<int> me, int[] that)
      {
        int i = 0, maxind = that.Length - 1;

        foreach (int item in me)
          if (i > maxind || ic.Compare(item, that[i++]) != 0)
            return false;

        return true;
      }


      [SetUp]
      public void Init()
      {
        ic = new IC();
        tree = new TreeBag<int>(ic);
        for (int i = 0; i < 10; i++)
          tree.Add(2 * i + 1);
      }


      [Test]
      public void First()
      {
        TreeBag<int>[] snaps = new TreeBag<int>[10];

        for (int i = 0; i < 10; i++)
        {
          snaps[i] = (TreeBag<int>)(tree.Snapshot());
          tree.Add(2 * i);
        }

        for (int i = 0; i < 10; i++)
        {
          Assert.AreEqual(i + 10, snaps[i].Count);
        }

        snaps[5] = null;
        snaps[9] = null;
        GC.Collect();
        snaps[8].Dispose();
        tree.Remove(14);

        int[] res = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 16, 17, 18, 19 };
        int[] snap7 = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 17, 19 };
        int[] snap3 = new int[] { 0, 1, 2, 3, 4, 5, 7, 9, 11, 13, 15, 17, 19 };

        Assert.IsTrue(IC.eq(snaps[3], snap3), "Snap 3 was changed!");
        Assert.IsTrue(IC.eq(snaps[7], snap7), "Snap 7 was changed!");
        Assert.IsTrue(IC.eq(tree, res));
        Assert.IsTrue(tree.Check("B"));
        Assert.IsTrue(snaps[3].Check("B"));
        Assert.IsTrue(snaps[7].Check("B"));
      }


      [Test]
      public void CollectingTheMaster()
      {
        TreeBag<int>[] snaps = new TreeBag<int>[10];

        for (int i = 0; i < 10; i++)
        {
          snaps[i] = (TreeBag<int>)(tree.Snapshot());
          tree.Add(2 * i);
        }

        tree = null;
        GC.Collect();
        for (int i = 0; i < 10; i++)
        {
          Assert.AreEqual(i + 10, snaps[i].Count);
        }

        snaps[5] = null;
        snaps[9] = null;
        GC.Collect();
        snaps[8].Dispose();

        int[] snap7 = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 17, 19 };
        int[] snap3 = new int[] { 0, 1, 2, 3, 4, 5, 7, 9, 11, 13, 15, 17, 19 };

        Assert.IsTrue(IC.eq(snaps[3], snap3), "Snap 3 was changed!");
        Assert.IsTrue(IC.eq(snaps[7], snap7), "Snap 7 was changed!");
        Assert.IsTrue(snaps[3].Check("B"));
        Assert.IsTrue(snaps[7].Check("B"));
      }


      [TearDown]
      public void Dispose()
      {
        tree = null;
        ic = null;
      }
    }
  }




  namespace HigherOrder
  {
    internal class CubeRoot : IComparable<int>
    {
      private int c;


      internal CubeRoot(int c) { this.c = c; }


      public int CompareTo(int that) { return c - that * that * that; }
      public bool Equals(int that) { return c == that * that * that; }
    }



    class Interval : IComparable<int>
    {
      private int b, t;


      internal Interval(int b, int t) { this.b = b; this.t = t; }


      public int CompareTo(int that) { return that < b ? 1 : that > t ? -1 : 0; }
      public bool Equals(int that) { return that >= b && that <= t; }
    }



    [TestFixture]
    public class Simple
    {
      private TreeBag<int> tree;

      private SCG.IComparer<int> ic;


      [SetUp]
      public void Init()
      {
        ic = new IC();
        tree = new TreeBag<int>(ic);
      }


      private bool never(int i) { return false; }


      private bool always(int i) { return true; }


      private bool even(int i) { return i % 2 == 0; }


      private string themap(int i) { return String.Format("AA {0,4} BB", i); }


      private string badmap(int i) { return String.Format("AA {0} BB", i); }


      private int appfield1;

      private int appfield2;


      private void apply(int i) { appfield1++; appfield2 += i * i; }


      [Test]
      public void Apply()
      {
        Simple simple1 = new Simple();

        tree.Apply(new Act<int>(simple1.apply));
        Assert.AreEqual(0, simple1.appfield1);
        Assert.AreEqual(0, simple1.appfield2);

        Simple simple2 = new Simple();

        for (int i = 0; i < 10; i++) tree.Add(i);
        tree.Add(2);

        tree.Apply(new Act<int>(simple2.apply));
        Assert.AreEqual(11, simple2.appfield1);
        Assert.AreEqual(289, simple2.appfield2);
      }


      [Test]
      public void All()
      {
        Assert.IsTrue(tree.All(new Fun<int, bool>(never)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(always)));
        for (int i = 0; i < 10; i++) tree.Add(i);

        Assert.IsFalse(tree.All(new Fun<int, bool>(never)));
        Assert.IsFalse(tree.All(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(always)));
        tree.Clear();
        for (int i = 0; i < 10; i++) tree.Add(i * 2);

        Assert.IsFalse(tree.All(new Fun<int, bool>(never)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(always)));
        tree.Clear();
        for (int i = 0; i < 10; i++) tree.Add(i * 2 + 1);

        Assert.IsFalse(tree.All(new Fun<int, bool>(never)));
        Assert.IsFalse(tree.All(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.All(new Fun<int, bool>(always)));
      }


      [Test]
      public void Exists()
      {
        Assert.IsFalse(tree.Exists(new Fun<int, bool>(never)));
        Assert.IsFalse(tree.Exists(new Fun<int, bool>(even)));
        Assert.IsFalse(tree.Exists(new Fun<int, bool>(always)));
        for (int i = 0; i < 10; i++) tree.Add(i);

        Assert.IsFalse(tree.Exists(new Fun<int, bool>(never)));
        Assert.IsTrue(tree.Exists(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.Exists(new Fun<int, bool>(always)));
        tree.Clear();
        for (int i = 0; i < 10; i++) tree.Add(i * 2);

        Assert.IsFalse(tree.Exists(new Fun<int, bool>(never)));
        Assert.IsTrue(tree.Exists(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.Exists(new Fun<int, bool>(always)));
        tree.Clear();
        for (int i = 0; i < 10; i++) tree.Add(i * 2 + 1);

        Assert.IsFalse(tree.Exists(new Fun<int, bool>(never)));
        Assert.IsFalse(tree.Exists(new Fun<int, bool>(even)));
        Assert.IsTrue(tree.Exists(new Fun<int, bool>(always)));
      }


      [Test]
      public void FindAll()
      {
        Assert.AreEqual(0, tree.FindAll(new Fun<int, bool>(never)).Count);
        for (int i = 0; i < 10; i++)
          tree.Add(i);
        tree.Add(2);

        Assert.AreEqual(0, tree.FindAll(new Fun<int, bool>(never)).Count);
        Assert.AreEqual(11, tree.FindAll(new Fun<int, bool>(always)).Count);
        Assert.AreEqual(6, tree.FindAll(new Fun<int, bool>(even)).Count);
        Assert.IsTrue(((TreeBag<int>)tree.FindAll(new Fun<int, bool>(even))).Check("R"));
      }


      [Test]
      public void Map()
      {
        Assert.AreEqual(0, tree.Map(new Fun<int, string>(themap), new SC()).Count);
        for (int i = 0; i < 14; i++)
          tree.Add(i * i * i);
        tree.Add(1);

        IIndexedSorted<string> res = tree.Map(new Fun<int, string>(themap), new SC());

        Assert.IsTrue(((TreeBag<string>)res).Check("R"));
        Assert.AreEqual(15, res.Count);
        Assert.AreEqual("AA    0 BB", res[0]);
        Assert.AreEqual("AA    1 BB", res[1]);
        Assert.AreEqual("AA    1 BB", res[2]);
        Assert.AreEqual("AA    8 BB", res[3]);
        Assert.AreEqual("AA   27 BB", res[4]);
        Assert.AreEqual("AA  125 BB", res[6]);
        Assert.AreEqual("AA 1000 BB", res[11]);
      }


      [Test]
      [ExpectedException(typeof(ArgumentException), ExpectedMessage="mapper not monotonic")]
      public void BadMap()
      {
        for (int i = 0; i < 11; i++)
          tree.Add(i * i * i);

        ISorted<string> res = tree.Map(new Fun<int, string>(badmap), new SC());
      }


      [Test]
      public void Cut()
      {
        for (int i = 0; i < 10; i++)
          tree.Add(i);
        tree.Add(3);

        int low, high;
        bool lval, hval;

        Assert.IsTrue(tree.Cut(new CubeRoot(27), out low, out lval, out high, out hval));
        Assert.IsTrue(lval && hval);
        Assert.AreEqual(4, high);
        Assert.AreEqual(2, low);
        Assert.IsFalse(tree.Cut(new CubeRoot(30), out low, out lval, out high, out hval));
        Assert.IsTrue(lval && hval);
        Assert.AreEqual(4, high);
        Assert.AreEqual(3, low);
      }


      [Test]
      public void CutInt()
      {
        for (int i = 0; i < 10; i++)
          tree.Add(2 * i);

        int low, high;
        bool lval, hval;

        Assert.IsFalse(tree.Cut(new IC(3), out low, out lval, out high, out hval));
        Assert.IsTrue(lval && hval);
        Assert.AreEqual(4, high);
        Assert.AreEqual(2, low);
        Assert.IsTrue(tree.Cut(new IC(6), out low, out lval, out high, out hval));
        Assert.IsTrue(lval && hval);
        Assert.AreEqual(8, high);
        Assert.AreEqual(4, low);
      }


      [Test]
      public void CutInterval()
      {
        for (int i = 0; i < 10; i++)
          tree.Add(2 * i);

        int lo, hi;
        bool lv, hv;

        Assert.IsTrue(tree.Cut(new Interval(5, 9), out lo, out lv, out hi, out hv));
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(10, hi);
        Assert.AreEqual(4, lo);
        Assert.IsTrue(tree.Cut(new Interval(6, 10), out lo, out lv, out hi, out hv));
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(12, hi);
        Assert.AreEqual(4, lo);
        for (int i = 0; i < 100; i++)
          tree.Add(2 * i);

        tree.Cut(new Interval(77, 105), out lo, out lv, out hi, out hv);
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(106, hi);
        Assert.AreEqual(76, lo);
        tree.Cut(new Interval(5, 7), out lo, out lv, out hi, out hv);
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(8, hi);
        Assert.AreEqual(4, lo);
        tree.Cut(new Interval(80, 110), out lo, out lv, out hi, out hv);
        Assert.IsTrue(lv && hv);
        Assert.AreEqual(112, hi);
        Assert.AreEqual(78, lo);
      }


      [Test]
      public void UpperCut()
      {
        for (int i = 0; i < 10; i++)
          tree.Add(i);

        int l, h;
        bool lv, hv;

        Assert.IsFalse(tree.Cut(new CubeRoot(1000), out l, out lv, out h, out hv));
        Assert.IsTrue(lv && !hv);
        Assert.AreEqual(9, l);
        Assert.IsFalse(tree.Cut(new CubeRoot(-50), out l, out lv, out h, out hv));
        Assert.IsTrue(!lv && hv);
        Assert.AreEqual(0, h);
      }


      [TearDown]
      public void Dispose() { ic = null; tree = null; }
    }
  }




  namespace MultiOps
  {
    [TestFixture]
    public class AddAll
    {
      private int sqr(int i) { return i * i; }


      TreeBag<int> tree;


      [SetUp]
      public void Init() { tree = new TreeBag<int>(new IC()); }


      [Test]
      public void EmptyEmpty()
      {
        tree.AddAll(new FunEnumerable(0, new Fun<int, int>(sqr)));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
      }


      [Test]
      public void SomeEmpty()
      {
        for (int i = 4; i < 9; i++) tree.Add(i);

        tree.AddAll(new FunEnumerable(0, new Fun<int, int>(sqr)));
        Assert.AreEqual(5, tree.Count);
        Assert.IsTrue(tree.Check());
      }


      [Test]
      public void EmptySome()
      {
        tree.AddAll(new FunEnumerable(4, new Fun<int, int>(sqr)));
        Assert.AreEqual(4, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(0, tree[0]);
        Assert.AreEqual(1, tree[1]);
        Assert.AreEqual(4, tree[2]);
        Assert.AreEqual(9, tree[3]);
      }


      [Test]
      public void SomeSome()
      {
        for (int i = 5; i < 9; i++) tree.Add(i);
        tree.Add(1);

        tree.AddAll(new FunEnumerable(4, new Fun<int, int>(sqr)));
        Assert.AreEqual(9, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 1, 4, 5, 6, 7, 8, 9));
      }


      [TearDown]
      public void Dispose() { tree = null; }
    }



    [TestFixture]
    public class AddSorted
    {
      private int sqr(int i) { return i * i; }

      private int step(int i) { return i / 3; }


      private int bad(int i) { return i * (5 - i); }


      TreeBag<int> tree;


      [SetUp]
      public void Init() { tree = new TreeBag<int>(new IC()); }


      [Test]
      public void EmptyEmpty()
      {
        tree.AddSorted(new FunEnumerable(0, new Fun<int, int>(sqr)));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
      }


      [Test]
      public void SomeEmpty()
      {
        for (int i = 4; i < 9; i++) tree.Add(i);

        tree.AddSorted(new FunEnumerable(0, new Fun<int, int>(sqr)));
        Assert.AreEqual(5, tree.Count);
        Assert.IsTrue(tree.Check());
      }


      [Test]
      public void EmptySome()
      {
        tree.AddSorted(new FunEnumerable(4, new Fun<int, int>(sqr)));
        Assert.AreEqual(4, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(0, tree[0]);
        Assert.AreEqual(1, tree[1]);
        Assert.AreEqual(4, tree[2]);
        Assert.AreEqual(9, tree[3]);
      }

      [Test]
      public void EmptySome2()
      {
        tree.AddSorted(new FunEnumerable(4, new Fun<int, int>(step)));
        //tree.dump();
        Assert.AreEqual(4, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(0, tree[0]);
        Assert.AreEqual(0, tree[1]);
        Assert.AreEqual(0, tree[2]);
        Assert.AreEqual(1, tree[3]);
      }


      [Test]
      public void SomeSome()
      {
        for (int i = 5; i < 9; i++) tree.Add(i);
        tree.Add(1);

        tree.AddSorted(new FunEnumerable(4, new Fun<int, int>(sqr)));
        Assert.AreEqual(9, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 1, 4, 5, 6, 7, 8, 9));
      }


      [Test]
      [ExpectedException(typeof(ArgumentException), ExpectedMessage="Argument not sorted")]
      public void EmptyBad()
      {
        tree.AddSorted(new FunEnumerable(9, new Fun<int, int>(bad)));
      }


      [TearDown]
      public void Dispose() { tree = null; }
    }



    [TestFixture]
    public class Rest
    {
      TreeBag<int> tree, tree2;


      [SetUp]
      public void Init()
      {
        tree = new TreeBag<int>(new IC());
        tree2 = new TreeBag<int>(new IC());
        for (int i = 0; i < 10; i++)
          tree.Add(i);
        tree.Add(4);

        for (int i = 0; i < 10; i++)
          tree2.Add(2 * i);
      }


      [Test]
      public void RemoveAll()
      {
        tree.RemoveAll(tree2.RangeFromTo(3, 7));
        Assert.AreEqual(9, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 2, 3, 4, 5, 7, 8, 9));
        tree.RemoveAll(tree2.RangeFromTo(3, 7));
        Assert.AreEqual(8, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 2, 3, 5, 7, 8, 9));
        tree.RemoveAll(tree2.RangeFromTo(13, 17));
        Assert.AreEqual(8, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 2, 3, 5, 7, 8, 9));
        tree.RemoveAll(tree2.RangeFromTo(3, 17));
        Assert.AreEqual(7, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 0, 1, 2, 3, 5, 7, 9));
        for (int i = 0; i < 10; i++) tree2.Add(i);

        tree.RemoveAll(tree2.RangeFromTo(-1, 10));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree));
      }

      private void pint<T>(SCG.IEnumerable<T> e)
      {
        foreach (T i in e)
          Console.Write("{0} ", i);

        Console.WriteLine();
      }

      [Test]
      public void RetainAll()
      {
        tree.Add(8); tree2.Add(6);
        //pint<int>(tree);
        //pint<int>(tree2);
        tree.RetainAll(tree2.RangeFromTo(3, 17));
        Assert.AreEqual(3, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 4, 6, 8));
        tree.RetainAll(tree2.RangeFromTo(1, 17));
        Assert.AreEqual(3, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 4, 6, 8));
        tree.RetainAll(tree2.RangeFromTo(3, 5));
        Assert.AreEqual(1, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree, 4));
        tree.RetainAll(tree2.RangeFromTo(7, 17));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree));
        for (int i = 0; i < 10; i++) tree.Add(i);

        tree.RetainAll(tree2.RangeFromTo(5, 5));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree));
        for (int i = 0; i < 10; i++) tree.Add(i);

        tree.RetainAll(tree2.RangeFromTo(15, 25));
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(tree.Check());
        Assert.IsTrue(IC.eq(tree));
      }


      [Test]
      public void ContainsAll()
      {
        Assert.IsFalse(tree.ContainsAll(tree2));
        Assert.IsTrue(tree.ContainsAll(tree));
        tree2.Clear();
        Assert.IsTrue(tree.ContainsAll(tree2));
        tree.Clear();
        Assert.IsTrue(tree.ContainsAll(tree2));
        tree2.Add(8);
        Assert.IsFalse(tree.ContainsAll(tree2));
      }


      [Test]
      public void RemoveInterval()
      {
        tree.RemoveInterval(3, 4);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(7, tree.Count);
        Assert.IsTrue(IC.eq(tree, 0, 1, 2, 6, 7, 8, 9));
        tree.RemoveInterval(2, 3);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(4, tree.Count);
        Assert.IsTrue(IC.eq(tree, 0, 1, 8, 9));
        tree.RemoveInterval(0, 4);
        Assert.IsTrue(tree.Check());
        Assert.AreEqual(0, tree.Count);
        Assert.IsTrue(IC.eq(tree));
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void RemoveRangeBad1()
      {
        tree.RemoveInterval(-3, 8);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void RemoveRangeBad2()
      {
        tree.RemoveInterval(3, -8);
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void RemoveRangeBad3()
      {
        tree.RemoveInterval(3, 9);
      }


      [Test]
      public void GetRange()
      {
        Assert.IsTrue(IC.eq(tree[3, 0]));
        Assert.IsTrue(IC.eq(tree[3, 1], 3));
        Assert.IsTrue(IC.eq(tree[3, 2], 3, 4));
        Assert.IsTrue(IC.eq(tree[3, 3], 3, 4, 4));
        Assert.IsTrue(IC.eq(tree[3, 4], 3, 4, 4, 5));
        Assert.IsTrue(IC.eq(tree[4, 0]));
        Assert.IsTrue(IC.eq(tree[4, 1], 4));
        Assert.IsTrue(IC.eq(tree[4, 2], 4, 4));
        Assert.IsTrue(IC.eq(tree[4, 3], 4, 4, 5));
        Assert.IsTrue(IC.eq(tree[4, 4], 4, 4, 5, 6));
        Assert.IsTrue(IC.eq(tree[5, 0]));
        Assert.IsTrue(IC.eq(tree[5, 1], 4));
        Assert.IsTrue(IC.eq(tree[5, 2], 4, 5));
        Assert.IsTrue(IC.eq(tree[5, 3], 4, 5, 6));
        Assert.IsTrue(IC.eq(tree[5, 4], 4, 5, 6, 7));
        Assert.IsTrue(IC.eq(tree[5, 6], 4, 5, 6, 7, 8, 9));
      }

      [Test]
      public void GetRangeBug20090616()
      {
        C5.TreeBag<double> tree = new C5.TreeBag<double>() { 
          0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 3.0, 3.0, 4.0 };
        for (int start = 0; start <= tree.Count - 2; start++)
        {
          double[] range = tree[start, 2].ToArray();
          Assert.AreEqual(range[0], tree[start]);
          Assert.AreEqual(range[1], tree[start+1]);
        }
      }

      [Test]
      public void GetRangeBackwards()
      {
        Assert.IsTrue(IC.eq(tree[3, 0].Backwards()));
        Assert.IsTrue(IC.eq(tree[3, 1].Backwards(), 3));
        Assert.IsTrue(IC.eq(tree[3, 2].Backwards(), 4, 3));
        Assert.IsTrue(IC.eq(tree[3, 3].Backwards(), 4, 4, 3));
        Assert.IsTrue(IC.eq(tree[3, 4].Backwards(), 5, 4, 4, 3));
        Assert.IsTrue(IC.eq(tree[4, 0].Backwards()));
        Assert.IsTrue(IC.eq(tree[4, 1].Backwards(), 4));
        Assert.IsTrue(IC.eq(tree[4, 2].Backwards(), 4, 4));
        Assert.IsTrue(IC.eq(tree[4, 3].Backwards(), 5, 4, 4));
        Assert.IsTrue(IC.eq(tree[4, 4].Backwards(), 6, 5, 4, 4));
        Assert.IsTrue(IC.eq(tree[5, 0].Backwards()));
        Assert.IsTrue(IC.eq(tree[5, 1].Backwards(), 4));
        Assert.IsTrue(IC.eq(tree[5, 2].Backwards(), 5, 4));
        Assert.IsTrue(IC.eq(tree[5, 3].Backwards(), 6, 5, 4));
        Assert.IsTrue(IC.eq(tree[5, 4].Backwards(), 7, 6, 5, 4));
      }

      [Test]
      public void GetRangeBackwardsBug20090616()
      {
        C5.TreeBag<double> tree = new C5.TreeBag<double>() { 
          0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 3.0, 3.0, 4.0 };
        for (int start = 0; start <= tree.Count - 2; start++)
        {
          double[] range = tree[start, 2].Backwards().ToArray();
          Assert.AreEqual(range[1], tree[start]);
          Assert.AreEqual(range[0], tree[start + 1]);
        }
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void GetRangeBad1()
      {
        object foo = tree[-3, 0];
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void GetRangeBad2()
      {
        object foo = tree[3, -1];
      }


      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void GetRangeBad3()
      {
        object foo = tree[3, 9];
      }


      [TearDown]
      public void Dispose() { tree = null; tree2 = null; }
    }
  }


  namespace Hashing
  {
    [TestFixture]
    public class ISequenced
    {
      private ISequenced<int> dit, dat, dut;


      [SetUp]
      public void Init()
      {
        dit = new TreeBag<int>(Comparer<int>.Default, EqualityComparer<int>.Default);
        dat = new TreeBag<int>(Comparer<int>.Default, EqualityComparer<int>.Default);
        dut = new TreeBag<int>(new RevIC(), EqualityComparer<int>.Default);
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
        dut.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(3), dut.GetSequencedHashCode());
        dut.Add(7);
        Assert.AreEqual(CHC.sequencedhashcode(7, 3), dut.GetSequencedHashCode());
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
        dut.Add(7);
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
        dit = new TreeBag<int>(Comparer<int>.Default, EqualityComparer<int>.Default);
        dat = new TreeBag<int>(Comparer<int>.Default, EqualityComparer<int>.Default);
        dut = new TreeBag<int>(new RevIC(), EqualityComparer<int>.Default);
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
  }

}
