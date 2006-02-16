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
  static class Factory
  {
    public static IDictionary<K,V> New<K,V>() { return new TreeDictionary<K,V>(); }
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
		public void Pred()
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
		[ExpectedException(typeof(DuplicateNotAllowedException), "Key being added: 'A'")]
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
		public void Keys()
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
		public void Values()
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
