#if NET_4_0
// ConcurrentDictionaryTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Linq;
using System.Threading;
using MonoTests.System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NUnit;
using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture]
	public class ConcurrentDictionaryTests
	{
		ConcurrentDictionary<string, int> map;
		
		[SetUp]
		public void Setup ()
		{
			map = new ConcurrentDictionary<string, int> ();
			AddStuff();
		}
		
		void AddStuff ()
		{
			map.TryAdd ("foo", 1);
			map.TryAdd ("bar", 2);
			map["foobar"] = 3;
		}
		
		[Test]
		public void AddWithoutDuplicateTest ()
		{
			map.TryAdd("baz", 2);
			int val;
			
			Assert.IsTrue (map.TryGetValue("baz", out val));
			Assert.AreEqual(2, val);
			Assert.AreEqual(2, map["baz"]);
			Assert.AreEqual(4, map.Count);
		}
		
		[Test]
		public void AddParallelWithoutDuplicateTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Setup ();
				int index = 0;
				
				ParallelTestHelper.ParallelStressTest (map, delegate {
					int own = Interlocked.Increment (ref index);
					
					while (!map.TryAdd ("monkey" + own.ToString (), own));
					
				}, 4);
				
				Assert.AreEqual (7, map.Count);
				int value;
				
				Assert.IsTrue (map.TryGetValue ("monkey1", out value), "#1");
				Assert.AreEqual (1, value, "#1b");
				
				Assert.IsTrue (map.TryGetValue ("monkey2", out value), "#2");
				Assert.AreEqual (2, value, "#2b");
				
				Assert.IsTrue (map.TryGetValue ("monkey3", out value), "#3");
				Assert.AreEqual (3, value, "#3b");
				
				Assert.IsTrue (map.TryGetValue ("monkey4", out value), "#4");
				Assert.AreEqual (4, value, "#4b");
			});
		}
		
		[Test]
		public void RemoveParallelTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Setup ();
				int index = 0;
				bool r1 = false, r2 = false, r3 = false;
				int val;
				
				ParallelTestHelper.ParallelStressTest (map, delegate {
					int own = Interlocked.Increment (ref index);
					switch (own) {
					case 1:
						r1 = map.TryRemove ("foo", out val);
						break;
					case 2:
						r2 =map.TryRemove ("bar", out val);
						break;
					case 3:
						r3 = map.TryRemove ("foobar", out val);
						break;
					}
				}, 3);
				
				Assert.AreEqual (0, map.Count);
				int value;
	
				Assert.IsTrue (r1, "1");
				Assert.IsTrue (r2, "2");
				Assert.IsTrue (r3, "3");
				
				Assert.IsFalse (map.TryGetValue ("foo", out value), "#1b " + value.ToString ());
				Assert.IsFalse (map.TryGetValue ("bar", out value), "#2b");
				Assert.IsFalse (map.TryGetValue ("foobar", out value), "#3b");
			});
		}
		
		[Test]
		public void AddWithDuplicate()
		{
			Assert.IsFalse (map.TryAdd("foo", 6));
		}
		
		[Test]
		public void GetValueTest()
		{
			Assert.AreEqual(1, map["foo"], "#1");
			Assert.AreEqual(2, map["bar"], "#2");
			Assert.AreEqual(3, map.Count, "#3");
		}
		
		[Test, ExpectedException(typeof(KeyNotFoundException))]
		public void GetValueUnknownTest()
		{
			int val;
			Assert.IsFalse(map.TryGetValue("barfoo", out val));
			val = map["barfoo"];
		}
		
		[Test]
		public void ModificationTest()
		{
			map["foo"] = 9;
			int val;
			
			Assert.AreEqual(9, map["foo"], "#1");
			Assert.IsTrue(map.TryGetValue("foo", out val), "#3");
			Assert.AreEqual(9, val, "#4");
		}

		[Test]
		public void IterateTest ()
		{
			string[] keys = { "foo", "bar", "foobar" };
			int[] occurence = new int[3];

			foreach (var kvp in map) {
				int index = Array.IndexOf (keys, kvp.Key);
				Assert.AreNotEqual (-1, index, "#a");
				Assert.AreEqual (index + 1, kvp.Value, "#b");
				Assert.Less (++occurence[index], 2, "#c");
			}
		}

		[Test]
		public void GetOrAddTest ()
		{
			Assert.AreEqual (1, map.GetOrAdd ("foo", (_) => 12));
			Assert.AreEqual (13, map.GetOrAdd ("baz", (_) => 13));
		}

		[Test]
		public void TryUpdateTest ()
		{
			Assert.IsFalse (map.TryUpdate ("foo", 12, 11));
			Assert.AreEqual (1, map["foo"]);
			Assert.IsTrue (map.TryUpdate ("foo", 11, 1));
			Assert.AreEqual (11, map["foo"]);
		}

		[Test]
		public void AddOrUpdateTest ()
		{
			Assert.AreEqual (11, map.AddOrUpdate ("bar", (_) => 12, (_, __) => 11));
			Assert.AreEqual (12, map.AddOrUpdate ("baz", (_) => 12, (_, __) => 11));
		}

		[Test]
		public void ContainsTest ()
		{
			Assert.IsTrue (map.ContainsKey ("foo"));
			Assert.IsTrue (map.ContainsKey ("bar"));
			Assert.IsTrue (map.ContainsKey ("foobar"));
			Assert.IsFalse (map.ContainsKey ("baz"));
			Assert.IsFalse (map.ContainsKey ("oof"));
		}

		class DumbClass : IEquatable<DumbClass>
		{
			int foo;

			public DumbClass (int foo)
			{
				this.foo = foo;
			}

			public int Foo {
				get {
					return foo;
				}
			}

			public override bool Equals (object rhs)
			{
				DumbClass temp = rhs as DumbClass;
				return temp == null ? false : Equals (temp);
			}

			public bool Equals (DumbClass rhs)
			{
				return this.foo == rhs.foo;
			}

			public override int GetHashCode ()
			{
				return 5;
			}
		}

		[Test]
		public void SameHashCodeInsertTest ()
		{
			var classMap = new ConcurrentDictionary<DumbClass, string> ();

			var class1 = new DumbClass (1);
			var class2 = new DumbClass (2);

			Assert.IsTrue (classMap.TryAdd (class1, "class1"), "class 1");
			Console.WriteLine ();
			Assert.IsTrue (classMap.TryAdd (class2, "class2"), "class 2");

			Assert.AreEqual ("class1", classMap[class1], "class 1 check");
			Assert.AreEqual ("class2", classMap[class2], "class 2 check");
		}

		[Test]
		public void InitWithEnumerableTest ()
		{
			int[] data = {1,2,3,4,5,6,7,8,9,10};
			var ndic = data.ToDictionary (x => x);
			var cdic = new ConcurrentDictionary<int, int> (ndic);

			foreach (var index in data) {
				Assert.IsTrue (cdic.ContainsKey (index));
				int val;
				Assert.IsTrue (cdic.TryGetValue (index, out val));
				Assert.AreEqual (index, val);
			}
		}
	}
}
#endif
