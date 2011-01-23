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
namespace C5UnitTests.hashtable.dictionary
{
  using DictionaryIntToInt = HashDictionary<int, int>;

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestEvents()
    {
      Fun<DictionaryIntToInt> factory = delegate() { return new DictionaryIntToInt(TenEqualityComparer.Default); };
      new C5UnitTests.Templates.Events.DictionaryTester<DictionaryIntToInt>().Test(factory);
    }

    [Test]
    public void TestSerialize()
    {
      C5UnitTests.Templates.Extensible.Serialization.DTester<DictionaryIntToInt>();
    }
  }

  static class Factory
  {
    public static IDictionary<K, V> New<K, V>() { return new HashDictionary<K, V>(); }
  }

  [TestFixture]
  public class Formatting
  {
    IDictionary<int, int> coll;
    IFormatProvider rad16;
    [SetUp]
    public void Init() { coll = Factory.New<int, int>(); rad16 = new RadixFormatProvider(16); }
    [TearDown]
    public void Dispose() { coll = null; rad16 = null; }
    [Test]
    [Category("NotWorking")]
    public void Format()
    {
      Assert.AreEqual("{  }", coll.ToString());
      coll.Add(23, 67); coll.Add(45, 89);
      Assert.AreEqual("{ 45 => 89, 23 => 67 }", coll.ToString());
      Assert.AreEqual("{ 2D => 59, 17 => 43 }", coll.ToString(null, rad16));
      Assert.AreEqual("{ 45 => 89, ... }", coll.ToString("L14", null));
      Assert.AreEqual("{ 2D => 59, ... }", coll.ToString("L14", rad16));
    }
  }

  [TestFixture]
  public class HashDict
  {
    private HashDictionary<string, string> dict;


    [SetUp]
    public void Init()
    {
      dict = new HashDictionary<string, string>();
      //dict = TreeDictionary<string,string>.MakeNaturalO<string,string>();
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor1()
    {
      new HashDictionary<int, int>(null);
    }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor2()
    {
      new HashDictionary<int, int>(5, 0.5, null);
    }

    [Test]
    public void Choose()
    {
      dict.Add("ER", "FOO");
      Assert.AreEqual(new KeyValuePair<string, string>("ER", "FOO"), dict.Choose());
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadChoose()
    {
      dict.Choose();
    }



    [TearDown]
    public void Dispose()
    {
      dict = null;
    }


    [Test]
    public void Initial()
    {
      bool res;

      Assert.IsFalse(dict.IsReadOnly);
      Assert.AreEqual(0, dict.Count, "new dict should be empty");
      dict.Add("A", "B");
      Assert.AreEqual(1, dict.Count, "bad count");
      Assert.AreEqual("B", dict["A"], "Wrong value for dict[A]");
      dict.Add("C", "D");
      Assert.AreEqual(2, dict.Count, "bad count");
      Assert.AreEqual("B", dict["A"], "Wrong value");
      Assert.AreEqual("D", dict["C"], "Wrong value");
      res = dict.Remove("A");
      Assert.IsTrue(res, "bad return value from Remove(A)");
      Assert.AreEqual(1, dict.Count, "bad count");
      Assert.AreEqual("D", dict["C"], "Wrong value of dict[C]");
      res = dict.Remove("Z");
      Assert.IsFalse(res, "bad return value from Remove(Z)");
      Assert.AreEqual(1, dict.Count, "bad count");
      Assert.AreEqual("D", dict["C"], "Wrong value of dict[C] (2)");
    }


    [Test]
    public void Contains()
    {
      dict.Add("C", "D");
      Assert.IsTrue(dict.Contains("C"));
      Assert.IsFalse(dict.Contains("D"));
    }


    [Test]
    [ExpectedException(typeof(DuplicateNotAllowedException), ExpectedMessage="Key being added: 'A'")]
    public void IllegalAdd()
    {
      dict.Add("A", "B");
      dict.Add("A", "B");
    }


    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void GettingNonExisting()
    {
      Console.WriteLine(dict["R"]);
    }


    [Test]
    public void Setter()
    {
      dict["R"] = "UYGUY";
      Assert.AreEqual("UYGUY", dict["R"]);
      dict["R"] = "UIII";
      Assert.AreEqual("UIII", dict["R"]);
      dict["S"] = "VVV";
      Assert.AreEqual("UIII", dict["R"]);
      Assert.AreEqual("VVV", dict["S"]);
      //dict.dump();
    }

    [Test]
    public void CombinedOps()
    {
      dict["R"] = "UIII";
      dict["S"] = "VVV";
      dict["T"] = "XYZ";

      string s;

      Assert.IsTrue(dict.Remove("S", out s));
      Assert.AreEqual("VVV", s);
      Assert.IsFalse(dict.Contains("S"));
      Assert.IsFalse(dict.Remove("A", out s));

      //
      Assert.IsTrue(dict.Find("T", out s));
      Assert.AreEqual("XYZ", s);
      Assert.IsFalse(dict.Find("A", out s));

      //
      Assert.IsTrue(dict.Update("R", "UHU"));
      Assert.AreEqual("UHU", dict["R"]);
      Assert.IsFalse(dict.Update("A", "W"));
      Assert.IsFalse(dict.Contains("A"));

      //
      s = "KKK";
      Assert.IsFalse(dict.FindOrAdd("B", ref s));
      Assert.AreEqual("KKK", dict["B"]);
      Assert.IsTrue(dict.FindOrAdd("T", ref s));
      Assert.AreEqual("XYZ", s);

      //
      s = "LLL";
      Assert.IsTrue(dict.UpdateOrAdd("R", s));
      Assert.AreEqual("LLL", dict["R"]);
      s = "MMM";
      Assert.IsFalse(dict.UpdateOrAdd("C", s));
      Assert.AreEqual("MMM", dict["C"]);

      // bug20071112 fixed 2008-02-03
      s = "NNN";
      String old;
      Assert.IsTrue(dict.UpdateOrAdd("R", s, out old));
      Assert.AreEqual("NNN", dict["R"]);
      Assert.AreEqual("LLL", old);
      s = "OOO";
      Assert.IsFalse(dict.UpdateOrAdd("D", s, out old));
      Assert.AreEqual("OOO", dict["D"]);
      // Unclear which of these is correct:
      // Assert.AreEqual(null, old);
      // Assert.AreEqual("OOO", old);
    }

    [Test]
    public void DeepBucket()
    {
      HashDictionary<int, int> dict2 = new HashDictionary<int, int>();

      for (int i = 0; i < 5; i++)
        dict2[16 * i] = 5 * i;

      for (int i = 0; i < 5; i++)
        Assert.AreEqual(5 * i, dict2[16 * i]);

      for (int i = 0; i < 5; i++)
        dict2[16 * i] = 7 * i + 1;

      for (int i = 0; i < 5; i++)
        Assert.AreEqual(7 * i + 1, dict2[16 * i]);
      Assert.IsTrue(dict.Check());
    }
  }



  [TestFixture]
  public class Enumerators
  {
    private HashDictionary<string, string> dict;

    private SCG.IEnumerator<KeyValuePair<string, string>> dictenum;


    [SetUp]
    public void Init()
    {
      dict = new HashDictionary<string, string>();
      dict["S"] = "A";
      dict["T"] = "B";
      dict["R"] = "C";
      dictenum = dict.GetEnumerator();
    }


    [TearDown]
    public void Dispose()
    {
      dictenum = null;
      dict = null;
    }


    [Test]
    [Category("NotWorking")]
    public void Keys()
    {
      SCG.IEnumerator<string> keys = dict.Keys.GetEnumerator();

      Assert.IsTrue(keys.MoveNext());
      Assert.AreEqual("R", keys.Current);
      Assert.IsTrue(keys.MoveNext());
      Assert.AreEqual("T", keys.Current);
      Assert.IsTrue(keys.MoveNext());
      Assert.AreEqual("S", keys.Current);
      Assert.IsFalse(keys.MoveNext());
    }


    [Test]
    [Category("NotWorking")]
    public void Values()
    {
      SCG.IEnumerator<string> values = dict.Values.GetEnumerator();

      Assert.IsTrue(values.MoveNext());
      Assert.AreEqual("C", values.Current);
      Assert.IsTrue(values.MoveNext());
      Assert.AreEqual("B", values.Current);
      Assert.IsTrue(values.MoveNext());
      Assert.AreEqual("A", values.Current);
      Assert.IsFalse(values.MoveNext());
    }

    [Test]
    public void Fun()
    {
      Assert.AreEqual("B", dict.Fun("T"));
    }


    [Test]
    [Category("NotWorking")]
    public void NormalUse()
    {
      Assert.IsTrue(dictenum.MoveNext());
      Assert.AreEqual(dictenum.Current, new KeyValuePair<string, string>("R", "C"));
      Assert.IsTrue(dictenum.MoveNext());
      Assert.AreEqual(dictenum.Current, new KeyValuePair<string, string>("T", "B"));
      Assert.IsTrue(dictenum.MoveNext());
      Assert.AreEqual(dictenum.Current, new KeyValuePair<string, string>("S", "A"));
      Assert.IsFalse(dictenum.MoveNext());
    }
  }
}





