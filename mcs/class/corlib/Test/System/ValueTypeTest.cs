//
// VersionTest.cs - NUnit Test Cases for the System.ValueType class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{
	struct Blah
	{ 
		public string s;
		public int x;

		public Blah (string s, int x)
		{
			this.s = s;
			this.x = x;
		}
	}

	struct Lalala
	{ 
		public int x;
		public string s;

		public Lalala (string s, int x)
		{
			this.s = s;
			this.x = x;
		}
	}

	struct NullableStruct {
		public Nullable<int> f;
	}

	[TestFixture]
	public class ValueTypeTest 
	{
		[Test]
		public void TestEquals ()
		{
			Blah a = new Blah ("abc", 1);
			Blah b = new Blah (string.Format ("ab{0}", 'c'), 1);
			Assert.AreEqual (a.Equals (b), true, "#01");
		}

		[Test]
		public void TestEquals_Nullable ()
		{
			NullableStruct f1 = new NullableStruct { f = 5 };
			NullableStruct f2 = new NullableStruct { f = 5 };
			Assert.IsTrue (f1.Equals (f2));

			f1 = new NullableStruct { f = null };
			f2 = new NullableStruct { f = null };
			Assert.IsTrue (f1.Equals (f2));

			f1 = new NullableStruct { f = 1 };
			f2 = new NullableStruct { f = 2 };
			Assert.IsFalse (f1.Equals (f2));

			f1 = new NullableStruct { f = 1 };
			f2 = new NullableStruct { f = null };
			Assert.IsFalse (f1.Equals (f2));
		}

		[Test]
		public void TestGetHash ()
		{
			Blah a = new Blah ("abc", 1);
			Blah b = new Blah (string.Format ("ab{0}", 'c'), 1);
			Assert.AreEqual (a.GetHashCode (), b.GetHashCode (), "#01");

			Lalala la = new Lalala ("abc", 1);
			Lalala lb = new Lalala (string.Format ("ab{0}", 'c'), 1);
			Assert.AreEqual (la.GetHashCode (), lb.GetHashCode (), "#02");

			a = new Blah (null, 1);
			b = new Blah (null, 1);
			Assert.AreEqual (la.GetHashCode (), lb.GetHashCode (), "#03");

			la = new Lalala (null, 1);
			lb = new Lalala (null, 1);
			Assert.AreEqual (la.GetHashCode (), lb.GetHashCode (), "#04");
		}

		struct EmptyStruct {}
		[Test]
		public void TestEmptyStructHashCode ()
		{
			Assert.AreEqual (new EmptyStruct ().GetHashCode (), new EmptyStruct ().GetHashCode ());
		}
	}
}

