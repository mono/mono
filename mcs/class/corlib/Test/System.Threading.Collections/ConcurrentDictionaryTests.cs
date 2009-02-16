#if NET_4_0

using System;
using System.Threading.Collections;

using NUnit;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixture]
	public class ConcurrentDictionaryTests
	{
		ConcurrentDictionary<string, int> map;
		
		[SetUp]
		public void Setup()
		{
			map = new ConcurrentDictionary<string, int>();
			AddStuff();
		}
		
		void AddStuff()
		{
			map.Add("foo", 1);
			map.Add("bar", 2);
			map.Add("foobar", 3);
		}
		
		[Test]
		public void AddWithoutDuplicateTest()
		{
			map.Add("baz", 2);
			
			Assert.AreEqual(2, map.GetValue("baz"));
			Assert.AreEqual(2, map["baz"]);
			Assert.AreEqual(4, map.Count);
		}
		
		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddWithDuplicate()
		{
			map.Add("foo", 6);
		}
		
		[Test]
		public void GetValueTest()
		{
			Assert.AreEqual(1, map.GetValue("foo"), "#1");
			Assert.AreEqual(2, map["bar"], "#2");
			Assert.AreEqual(3, map.Count, "#3");
		}
		
		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GetValueUnknownTest()
		{
			int val;
			Assert.IsFalse(map.TryGetValue("barfoo", out val));
			map.GetValue("barfoo");
		}
		
		[Test]
		public void ModificationTest()
		{
			map["foo"] = 9;
			int val;
			
			Assert.AreEqual(9, map["foo"], "#1");
			Assert.AreEqual(9, map.GetValue("foo"), "#2");
			Assert.IsTrue(map.TryGetValue("foo", out val), "#3");
			Assert.AreEqual(9, val, "#4");
		}
	}
}
#endif
