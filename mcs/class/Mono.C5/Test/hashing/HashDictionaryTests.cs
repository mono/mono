#if NET_2_0
/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
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
using MSG = System.Collections.Generic;
namespace nunit.hashtable.dictionary
{
	[TestFixture]
	public class HashDict
	{
		private HashDictionary<string,string> dict;


		[SetUp]
		public void Init()
		{
			dict = new HashDictionary<string,string>();
			//dict = TreeDictionary<string,string>.MakeNaturalO<string,string>();
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
		[ExpectedException(typeof(ArgumentException), "Item has already been added.  Key in dictionary: 'A'  Key being added: 'A'")]
		public void IllegalAdd()
		{
			dict.Add("A", "B");
			dict.Add("A", "B");
		}


		[Test]
		[ExpectedException(typeof(ArgumentException), "Key not present in Dictionary")]
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
		}

		[Test]
		public void DeepBucket()
		{
			HashDictionary<int,int> dict2 = new HashDictionary<int,int>();

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
		private HashDictionary<string,string> dict;

		private MSG.IEnumerator<KeyValuePair<string,string>> dictenum;


		[SetUp]
		public void Init()
		{
			dict = new HashDictionary<string,string>();
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
		[Ignore("This is also failing on windows. Martin")]
		public void Keys()
		{
			MSG.IEnumerator<string> keys = dict.Keys.GetEnumerator();

			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("R", keys.Current);
			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("T", keys.Current);
			Assert.IsTrue(keys.MoveNext());
			Assert.AreEqual("S", keys.Current);
			Assert.IsFalse(keys.MoveNext());
		}


		[Test]
		[Ignore("This is also failing on windows. Martin")]
		public void Values()
		{
			MSG.IEnumerator<string> values = dict.Values.GetEnumerator();

			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("C", values.Current);
			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("B", values.Current);
			Assert.IsTrue(values.MoveNext());
			Assert.AreEqual("A", values.Current);
			Assert.IsFalse(values.MoveNext());
		}


		[Test]
		[Ignore("This is also failing on windows. Martin")]
		public void NormalUse()
		{
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("R", "C"));
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("T", "B"));
			Assert.IsTrue(dictenum.MoveNext());
			Assert.AreEqual(dictenum.Current, new KeyValuePair<string,string>("S", "A"));
			Assert.IsFalse(dictenum.MoveNext());
		}
	}
}
#endif
