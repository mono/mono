//
// MonoTests.System.Collections.Generic.Test.DictionaryTest
//
// Authors:
//	Sureshkumar T (tsureshkumar@novell.com)
//	Ankit Jain (radical@corewars.org)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {
	[TestFixture]
	public class DictionaryTest {
		class MyClass {
			int a;
			int b;
			public MyClass (int a, int b)
			{
				this.a = a;
				this.b = b;
			}
			public override int GetHashCode ()
			{
				return a + b;
			}
	
			public override bool Equals (object obj)
			{
				if (!(obj is MyClass))
					return false;
				return ((MyClass)obj).Value == a;
			}
	
	
			public int Value {
				get { return a; }
			}
	
		}
	
		Dictionary <string, object> _dictionary = null;
		Dictionary <MyClass, MyClass> _dictionary2 = null;
	
		[SetUp]
		public void SetUp ()
		{
			_dictionary = new Dictionary <string, object> ();
			_dictionary2 = new Dictionary <MyClass, MyClass> ();
		}
	
		[Test]
		public void AddTest ()
		{
			_dictionary.Add ("key1", (object)"value");
			Assert.AreEqual ("value", _dictionary ["key1"].ToString (), "Add failed!");
		}
	
		[Test]
		public void AddTest2 ()
		{
			MyClass m1 = new MyClass (10,5);
			MyClass m2 = new MyClass (20,5);
			MyClass m3 = new MyClass (12,3);
			_dictionary2.Add (m1,m1);
			_dictionary2.Add (m2, m2);
			_dictionary2.Add (m3, m3);
			Assert.AreEqual (20, _dictionary2 [m2].Value, "#1");
			Assert.AreEqual (10, _dictionary2 [m1].Value, "#2");
			Assert.AreEqual (12, _dictionary2 [m3].Value, "#3");
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullTest ()
		{
			_dictionary.Add (null, "");
		}
	
		//Tests Add when resize takes place
		[Test]
		public void AddLargeTest ()
		{
			Dictionary <int, int> _dict = new Dictionary <int, int> ();
			int i, numElems = 50;
	
			for (i = 0; i < numElems; i++)
			{
				_dict.Add (i, i);
			}
	
			i = 0;
			foreach (KeyValuePair <int, int> entry in _dict)
			{
				i++;
			}
	
			Assert.AreEqual (i, numElems, "Add with resize failed!");
		}
	
		[Test]
		public void IndexerGetExistingTest ()
		{
			_dictionary.Add ("key1", (object)"value");
			Assert.AreEqual ("value", _dictionary ["key1"].ToString (), "Add failed!");
		}
		
		[Test, ExpectedException(typeof(KeyNotFoundException))]
		public void IndexerGetNonExistingTest ()
		{
			object foo = _dictionary ["foo"];
		}

		[Test]
		public void IndexerSetExistingTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary ["key1"] = (object) "value2";
			Assert.AreEqual (1, _dictionary.Count);
			Assert.AreEqual ("value2", _dictionary ["key1"]);
		}

		[Test]
		public void IndexerSetNonExistingTest ()
		{
			_dictionary ["key1"] = (object) "value1";
			Assert.AreEqual (1, _dictionary.Count);
			Assert.AreEqual ("value1", _dictionary ["key1"]);
		}
	
		[Test]
		public void RemoveTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			Assert.IsTrue (_dictionary.Remove ("key3"));
			Assert.IsFalse (_dictionary.Remove ("foo"));
			Assert.AreEqual (3, _dictionary.Count);
			Assert.IsFalse (_dictionary.ContainsKey ("key3"));
		}
	
		[Test]
		public void RemoveTest2 ()
		{
			MyClass m1 = new MyClass (10, 5);
			MyClass m2 = new MyClass (20, 5);
			MyClass m3 = new MyClass (12, 3);
			_dictionary2.Add (m1, m1);
			_dictionary2.Add (m2, m2);
			_dictionary2.Add (m3, m3);
			_dictionary2.Remove (m1); // m2 is in rehash path
			Assert.AreEqual (20, _dictionary2 [m2].Value, "#4");
			
		}
	
		[Test]
		public void ClearTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			_dictionary.Clear ();
			Assert.AreEqual (0, _dictionary.Count, "Clear method failed!");
			Assert.IsFalse (_dictionary.ContainsKey ("key2"));
		}
	
		[Test]
		public void ContainsKeyTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			bool contains = _dictionary.ContainsKey ("key4");
			Assert.IsTrue (contains, "ContainsKey does not return correct value!");
			contains = _dictionary.ContainsKey ("key5");
			Assert.IsFalse (contains, "ContainsKey for non existant does not return correct value!");
		}
	
		[Test]
		public void ContainsValueTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			bool contains = _dictionary.ContainsValue ("value2");
			Assert.IsTrue(contains, "ContainsValue does not return correct value!");
			contains = _dictionary.ContainsValue ("@@daisofja@@");
			Assert.IsFalse (contains, "ContainsValue for non existant does not return correct value!");
		}
	
		[Test]
		public void TryGetValueTest()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			object value = "";
			_dictionary.TryGetValue ("key4", out value);
			Assert.AreEqual ("value4", (string)value, "TryGetValue does not return value!");
	
			_dictionary.TryGetValue ("key7", out value);
			Assert.IsNull (value, "value for non existant value should be null!");
		}
	
		[Test]
		public void ValueTypeTest ()
		{
			Dictionary <int, float> dict = new Dictionary <int, float> ();
			dict.Add (10, 10.3f);
			dict.Add (11, 10.4f);
			dict.Add (12, 10.5f);
			Assert.AreEqual (10.4f, dict [11], "#5");
		}
	
		private class MyTest
		{
			public string Name;
			public int RollNo;
	
			public MyTest (string name, int number)
			{
			Name = name;
			RollNo = number;
			}
	
			public override bool Equals (object obj)
			{
				MyTest myt = obj as MyTest;
				return myt.Name.Equals (this.Name) &&
						myt.RollNo.Equals (this.RollNo);
			}
	
		}
	
		[Test]
		public void ObjectAsKeyTest ()
		{
			Dictionary <object, object> dict = new Dictionary <object, object> ();
			MyTest key1, key2, key3;
			dict.Add ((object) (key1 = new MyTest ("key1", 234)), (object)"value1");
			dict.Add ((object) (key2 = new MyTest ("key2", 444)), (object)"value2");
			dict.Add ((object) (key3 = new MyTest ("key3", 5655)), (object)"value3");
	
			Assert.AreEqual ((object)"value2", dict [key2], "value is not returned!");
			Assert.AreEqual ((object)"value3", dict [(object)key3], "neg: exception should not be thrown!");
		}
	
		[Test, ExpectedException (typeof (ArgumentException))]
		public void IDictionaryAddTest ()
		{
			IDictionary iDict = _dictionary as IDictionary;
			iDict.Add ((object)"key1", (object)"value1");
			iDict.Add ((object)"key2", (object)"value3");
			Assert.AreEqual (2, iDict.Count, "IDictioanry interface add is not working!");
	
			//Negative test case
			iDict.Add ((object)12, (object)"value");
			iDict.Add ((object)"key", (object)34);
		}
	
		[Test]
		public void IEnumeratorTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			IEnumerator itr = ((IEnumerable)_dictionary).GetEnumerator ();
			while (itr.MoveNext ())	{
				object o = itr.Current;
				Assert.AreEqual (typeof (DictionaryEntry), o.GetType (), "Current should return a type of DictionaryEntry");
				DictionaryEntry entry = (DictionaryEntry)itr.Current;
				if (entry.Key.ToString () == "key4")
					entry.Value = "value33";
			}
			Assert.AreEqual ("value4", _dictionary ["key4"].ToString (), "");
		}
	
	
		[Test]
		public void IEnumeratorGenericTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			IEnumerator <KeyValuePair <string, object>> itr = ((IEnumerable <KeyValuePair <string, object>>)_dictionary).GetEnumerator ();
			while (itr.MoveNext ())	{
				object o = itr.Current;
				Assert.AreEqual (typeof (KeyValuePair <string, object>), o.GetType (), "Current should return a type of DictionaryEntry");
				KeyValuePair <string, object> entry = (KeyValuePair <string, object>)itr.Current;
				if (entry.Key.ToString () == "key4")
					entry.Value = "value33";
			}
			Assert.AreEqual ("value4", _dictionary ["key4"].ToString (), "");
	
		}
	
		[Test]
		public void IDictionaryEnumeratorTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
			IDictionaryEnumerator itr = ((IDictionary)_dictionary).GetEnumerator ();
			while (itr.MoveNext ()) {
				object o = itr.Current;
				Assert.AreEqual (typeof (DictionaryEntry), o.GetType (), "Current should return a type of DictionaryEntry");
				DictionaryEntry entry = (DictionaryEntry)itr.Current;
				if (entry.Key.ToString () == "key4")
					entry.Value = "value33";
			}
			Assert.AreEqual ("value4", _dictionary ["key4"].ToString (), "");
	
		}
	
		[Test]
		public void ForEachTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
	
			int i = 0;
			foreach (KeyValuePair <string, object> entry in _dictionary)
			{
				i++;
				Console.WriteLine (entry.ToString());
			}
			if (i != 4)
				Assert.Fail("fail1: foreach entry failed!");
	
			i = 0;
			foreach (DictionaryEntry entry in ((IEnumerable)_dictionary))
			{
				i++;
			}
			if (i != 4)
				Assert.Fail("fail2: foreach entry failed!");
	
			i = 0;
			foreach (DictionaryEntry entry in ((IDictionary)_dictionary))
			{
				i++;
			}
			if (i != 4)
				Assert.Fail ("fail3: foreach entry failed!");
		}
	
		[Test]
		public void ResizeTest ()
		{
			Dictionary <string, object> dictionary = new Dictionary <string, object> (3);
			dictionary.Add ("key1", (object)"value1");
			dictionary.Add ("key2", (object)"value2");
			dictionary.Add ("key3", (object)"value3");
	
			Assert.AreEqual (3, dictionary.Count);
	
			dictionary.Add ("key4", (object)"value4");
			Assert.AreEqual (4, dictionary.Count);
			Assert.AreEqual ("value1", dictionary ["key1"].ToString (), "");
			Assert.AreEqual ("value2", dictionary ["key2"].ToString (), "");
			Assert.AreEqual ("value4", dictionary ["key4"].ToString (), "");
			Assert.AreEqual ("value3", dictionary ["key3"].ToString (), "");
		}
	
		[Test]
		public void KeyCollectionTest ()
		{
			_dictionary.Add ("key1", (object)"value1");
			_dictionary.Add ("key2", (object)"value2");
			_dictionary.Add ("key3", (object)"value3");
			_dictionary.Add ("key4", (object)"value4");
	
			ICollection <string> keys = ((IDictionary <string, object>)_dictionary).Keys;
			Assert.AreEqual (4, keys.Count);
			int i = 0;
			foreach (string key in keys)
			{
				Console.WriteLine("keys collection : " + key);
				i++;
			}
			Assert.AreEqual(4, i);
		}
	}
}

#endif // NET_2_0
