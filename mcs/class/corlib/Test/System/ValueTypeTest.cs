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

	[TestFixture]
	public class ValueTypeTest : Assertion
	{
		[Test]
		public void TestEquals ()
		{
			Blah a = new Blah ("abc", 1);
			Blah b = new Blah (string.Format ("ab{0}", 'c'), 1);
			AssertEquals ("#01", a.Equals (b), true);
		}

		[Test]
		public void TestGetHash ()
		{
			Blah a = new Blah ("abc", 1);
			Blah b = new Blah (string.Format ("ab{0}", 'c'), 1);
			AssertEquals ("#01", a.GetHashCode (), b.GetHashCode ());

			Lalala la = new Lalala ("abc", 1);
			Lalala lb = new Lalala (string.Format ("ab{0}", 'c'), 1);
			AssertEquals ("#02", la.GetHashCode (), lb.GetHashCode ());

			a = new Blah (null, 1);
			b = new Blah (null, 1);
			AssertEquals ("#03", la.GetHashCode (), lb.GetHashCode ());

			la = new Lalala (null, 1);
			lb = new Lalala (null, 1);
			AssertEquals ("#04", la.GetHashCode (), lb.GetHashCode ());
		}
	}
}

