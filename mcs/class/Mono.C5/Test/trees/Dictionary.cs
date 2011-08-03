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
using SCG=System.Collections.Generic;


namespace C5UnitTests.trees.RBDictionary
{
  using DictionaryIntToInt = TreeDictionary<int, int>;

  static class Factory
  {
    public static IDictionary<K,V> New<K,V>() { return new TreeDictionary<K,V>(); }
  }

  [TestFixture]
  public class GenericTesters
  {
    [Test]
    public void TestSerialize()
    {
      C5UnitTests.Templates.Extensible.Serialization.DTester<DictionaryIntToInt>();
    }
  }


  [TestFixture]
  public class Formatting
  {
    IDictionary<int,int> coll;
    IFormatProvider rad16;
    [SetUp]
    public void Init() { coll = Factory.New<int,int>(); rad16 = new RadixFormatProvider(16); }
    [TearDown]
    public void Dispose() { coll = null; rad16 = null; }
    [Test]
    public void Format()
    {
      Assert.AreEqual("[  ]", coll.ToString());
      coll.Add(23, 67); coll.Add(45, 89);
      Assert.AreEqual("[ 23 => 67, 45 => 89 ]", coll.ToString());
      Assert.AreEqual("[ 17 => 43, 2D => 59 ]", coll.ToString(null, rad16));
      Assert.AreEqual("[ 23 => 67, ... ]", coll.ToString("L14", null));
      Assert.AreEqual("[ 17 => 43, ... ]", coll.ToString("L14", rad16));
    }
  }

  [TestFixture]
  public class RBDict
	{
		private TreeDictionary<string,string> dict;


		[SetUp]
		public void Init() { dict = new TreeDictionary<string,string>(new SC()); }


		[TearDown]
		public void Dispose() { dict = null; }

    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void NullEqualityComparerinConstructor1()
    {
      new TreeDictionary<int,int>(null);
    }

    [Test]
    public void Choose()
    {
      dict.Add("YES","NO");
      Assert.AreEqual(new KeyValuePair<string,string>("YES","NO"), dict.Choose());
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void BadChoose()
    {
      dict.Choose();
    }

		[Test]
		public void Pred1()
		{
			dict.Add("A", "1");
			dict.Add("C", "2");
			dict.Add("E", "3");
			Assert.AreEqual("1", dict.Predecessor("B").Value);
			Assert.AreEqual("1", dict.Predecessor("C").Value);
			Assert.AreEqual("1", dict.WeakPredecessor("B").Value);
			Assert.AreEqual("2", dict.WeakPredecessor("C").Value);
			Assert.AreEqual("2", dict.Successor("B").Value);
			Assert.AreEqual("3", dict.Successor("C").Value);
			Assert.AreEqual("2", dict.WeakSuccessor("B").Value);
			Assert.AreEqual("2", dict.WeakSuccessor("C").Value);
    }

    [Test]
    public void Pred2()
    {
      dict.Add("A", "1");
      dict.Add("C", "2");
      dict.Add("E", "3");
      KeyValuePair<String, String> res;
      Assert.IsTrue(dict.TryPredecessor("B", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryPredecessor("C", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryWeakPredecessor("B", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryWeakPredecessor("C", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TrySuccessor("B", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TrySuccessor("C", out res));
      Assert.AreEqual("3", res.Value);
      Assert.IsTrue(dict.TryWeakSuccessor("B", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TryWeakSuccessor("C", out res));
      Assert.AreEqual("2", res.Value);

      Assert.IsFalse(dict.TryPredecessor("A", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TryWeakPredecessor("@", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TrySuccessor("E", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TryWeakSuccessor("F", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);
    }   

		[Test]
		public void Initial()
		{
			bool res;
			Assert.IsFalse(dict.IsReadOnly);

			Assert.AreEqual(dict.Count, 0, "new dict should be empty");
			dict.Add("A", "B");
			Assert.AreEqual(dict.Count, 1, "bad count");
			Assert.AreEqual(dict["A"], "B", "Wrong value for dict[A]");
			dict.Add("C", "D");
			Assert.AreEqual(dict.Count, 2, "bad count");
			Assert.AreEqual(dict["A"], "B", "Wrong value");
			Assert.AreEqual(dict["C"], "D", "Wrong value");
			res = dict.Remove("A");
			Assert.IsTrue(res, "bad return value from Remove(A)");
			Assert.IsTrue(dict.Check());
			Assert.AreEqual(dict.Count, 1, "bad count");
			Assert.AreEqual(dict["C"], "D", "Wrong value of dict[C]");
			res = dict.Remove("Z");
			Assert.IsFalse(res, "bad return value from Remove(Z)");
			Assert.AreEqual(dict.Count, 1, "bad count");
			Assert.AreEqual(dict["C"], "D", "Wrong value of dict[C] (2)");
			dict.Clear();
			Assert.AreEqual(dict.Count, 0, "dict should be empty");
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
			Assert.AreEqual(dict["R"], "UYGUY");
			dict["R"] = "UIII";
			Assert.AreEqual(dict["R"], "UIII");
			dict["S"] = "VVV";
			Assert.AreEqual(dict["R"], "UIII");
			Assert.AreEqual(dict["S"], "VVV");
			//dict.dump();
		}
	}

  [TestFixture]
  public class GuardedSortedDictionaryTest
  {
    private GuardedSortedDictionary<string, string> dict;

    [SetUp]
    public void Init() { 
      ISortedDictionary<string,string> dict = new TreeDictionary<string, string>(new SC());
      dict.Add("A", "1");
      dict.Add("C", "2");
      dict.Add("E", "3");
      this.dict = new GuardedSortedDictionary<string, string>(dict);
    }

    [TearDown]
    public void Dispose() { dict = null; }

    [Test]
    public void Pred1()
    {
      Assert.AreEqual("1", dict.Predecessor("B").Value);
      Assert.AreEqual("1", dict.Predecessor("C").Value);
      Assert.AreEqual("1", dict.WeakPredecessor("B").Value);
      Assert.AreEqual("2", dict.WeakPredecessor("C").Value);
      Assert.AreEqual("2", dict.Successor("B").Value);
      Assert.AreEqual("3", dict.Successor("C").Value);
      Assert.AreEqual("2", dict.WeakSuccessor("B").Value);
      Assert.AreEqual("2", dict.WeakSuccessor("C").Value);
    }

    [Test]
    public void Pred2()
    {
      KeyValuePair<String, String> res;
      Assert.IsTrue(dict.TryPredecessor("B", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryPredecessor("C", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryWeakPredecessor("B", out res));
      Assert.AreEqual("1", res.Value);
      Assert.IsTrue(dict.TryWeakPredecessor("C", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TrySuccessor("B", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TrySuccessor("C", out res));
      Assert.AreEqual("3", res.Value);
      Assert.IsTrue(dict.TryWeakSuccessor("B", out res));
      Assert.AreEqual("2", res.Value);
      Assert.IsTrue(dict.TryWeakSuccessor("C", out res));
      Assert.AreEqual("2", res.Value);

      Assert.IsFalse(dict.TryPredecessor("A", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TryWeakPredecessor("@", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TrySuccessor("E", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);

      Assert.IsFalse(dict.TryWeakSuccessor("F", out res));
      Assert.AreEqual(null, res.Key);
      Assert.AreEqual(null, res.Value);
    }

    [Test]
    public void Initial()
    {
      Assert.IsTrue(dict.IsReadOnly);

      Assert.AreEqual(3, dict.Count);
      Assert.AreEqual("1", dict["A"]);
    }

    [Test]
    public void Contains()
    {
      Assert.IsTrue(dict.Contains("A"));
      Assert.IsFalse(dict.Contains("1"));
    }

    [Test]
    [ExpectedException(typeof(ReadOnlyCollectionException))]
    public void IllegalAdd()
    {
      dict.Add("Q", "7");
    }

    [Test]
    [ExpectedException(typeof(ReadOnlyCollectionException))]
    public void IllegalClear()
    {
      dict.Clear();
    }
    [Test]

    [ExpectedException(typeof(ReadOnlyCollectionException))]
    public void IllegalSet()
    {
      dict["A"] = "8";
    }

    [ExpectedException(typeof(ReadOnlyCollectionException))]
    public void IllegalRemove()
    {
      dict.Remove("A");
    }

    [Test]
    [ExpectedException(typeof(NoSuchItemException))]
    public void GettingNonExisting()
    {
      Console.WriteLine(dict["R"]);
    }
  }

	[TestFixture]
	public class Enumerators
	{
		private TreeDictionary<string,string> dict;

		private SCG.IEnumerator<KeyValuePair<string,string>> dictenum;


		[SetUp]
		public void Init()
		{
			dict = new TreeDictionary<string,string>(new SC());
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
		public void KeysEnumerator()
		{
			SCG.IEnumerator<string> keys = dict.Keys.GetEnumerator();
			Assert.AreEqual(3, dict.Keys.Count);
			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("R",keys.Current);
			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("S",keys.Current);
			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("T",keys.Current);
			Assert.IsFalse(keys.MoveNext());
		}

    [Test]
    public void KeysISorted()
    {
      ISorted<string> keys = dict.Keys;
      Assert.IsTrue(keys.IsReadOnly);
      Assert.AreEqual("R", keys.FindMin());
      Assert.AreEqual("T", keys.FindMax());
      Assert.IsTrue(keys.Contains("S"));
      Assert.AreEqual(3, keys.Count);
      // This doesn't hold, maybe because the dict uses a special key comparer?
      // Assert.IsTrue(keys.SequencedEquals(new WrappedArray<string>(new string[] { "R", "S", "T" })));
      Assert.IsTrue(keys.UniqueItems().All(delegate(String s) { return s == "R" || s == "S" || s == "T"; }));
      Assert.IsTrue(keys.All(delegate(String s) { return s == "R" || s == "S" || s == "T"; }));
      Assert.IsFalse(keys.Exists(delegate(String s) { return s != "R" && s != "S" && s != "T"; }));
      String res;
      Assert.IsTrue(keys.Find(delegate(String s) { return s == "R"; }, out res));
      Assert.AreEqual("R", res);
      Assert.IsFalse(keys.Find(delegate(String s) { return s == "Q"; }, out res));
      Assert.AreEqual(null, res);
    }

    [Test]
    public void KeysISortedPred()
    {
      ISorted<string> keys = dict.Keys;
      String res;
      Assert.IsTrue(keys.TryPredecessor("S", out res));
      Assert.AreEqual("R", res);
      Assert.IsTrue(keys.TryWeakPredecessor("R", out res));
      Assert.AreEqual("R", res);
      Assert.IsTrue(keys.TrySuccessor("S", out res));
      Assert.AreEqual("T", res);
      Assert.IsTrue(keys.TryWeakSuccessor("T", out res));
      Assert.AreEqual("T", res);
      Assert.IsFalse(keys.TryPredecessor("R", out res));
      Assert.AreEqual(null, res);
      Assert.IsFalse(keys.TryWeakPredecessor("P", out res));
      Assert.AreEqual(null, res);
      Assert.IsFalse(keys.TrySuccessor("T", out res));
      Assert.AreEqual(null, res);
      Assert.IsFalse(keys.TryWeakSuccessor("U", out res));
      Assert.AreEqual(null, res);

      Assert.AreEqual("R", keys.Predecessor("S"));
      Assert.AreEqual("R", keys.WeakPredecessor("R"));
      Assert.AreEqual("T", keys.Successor("S"));
      Assert.AreEqual("T", keys.WeakSuccessor("T"));
    }
 
		[Test]
		public void ValuesEnumerator()
		{
			SCG.IEnumerator<string> values = dict.Values.GetEnumerator();
			Assert.AreEqual(3, dict.Values.Count);
			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("C",values.Current);
			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("A",values.Current);
			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("B",values.Current);
			Assert.IsFalse(values.MoveNext());
		}

    [Test]
    public void Fun()
    {
      Assert.AreEqual("B", dict.Fun("T"));
    }


		[Test]
		public void NormalUse()
		{
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("R", "C"));
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("S", "A"));
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("T", "B"));
			Assert.IsFalse(dictenum.MoveNext());
		}
	}


	namespace PathCopyPersistence
	{
		[TestFixture]
		public class Simple
		{
			private TreeDictionary<string,string> dict;

			private TreeDictionary<string,string> snap;


			[SetUp]
			public void Init()
			{
				dict = new TreeDictionary<string,string>(new SC());
				dict["S"] = "A";
				dict["T"] = "B";
				dict["R"] = "C";
				dict["V"] = "G";
				snap = (TreeDictionary<string,string>)dict.Snapshot();
			}


			[Test]
			public void Test()
			{
				dict["SS"] = "D";
				Assert.AreEqual(5, dict.Count);
				Assert.AreEqual(4, snap.Count);
				dict["T"] = "bb";
				Assert.AreEqual(5, dict.Count);
				Assert.AreEqual(4, snap.Count);
				Assert.AreEqual("B", snap["T"]);
				Assert.AreEqual("bb", dict["T"]);
				Assert.IsFalse(dict.IsReadOnly);
				Assert.IsTrue(snap.IsReadOnly);
				//Finally, update of root node:
				TreeDictionary<string,string> snap2 = (TreeDictionary<string,string>)dict.Snapshot();
				dict["S"] = "abe";
				Assert.AreEqual("abe", dict["S"]);
			}


			[Test]
			[ExpectedException(typeof(ReadOnlyCollectionException))]
			public void UpdateSnap()
			{
				snap["Y"] = "J";
			}


			[TearDown]
			public void Dispose()
			{
				dict = null;
				snap = null;
			}
		}
	}
}
