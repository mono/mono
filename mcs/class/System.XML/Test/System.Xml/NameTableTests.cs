//
// System.Xml.NameTableTests.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc.
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class NameTableTests : Assertion
	{
		NameTable table;
		
		[SetUp]
		public void GetReady ()
		{
			table = new NameTable ();
		}

		//
		// Tests System.Xml.NameTable.Add (string)
		//		
		[Test]
		public void Add1 ()
		{
			string add = "add1";
			string testAdd = table.Add (add);
			AssertEquals (add, testAdd);
			AssertSame (add, testAdd);
		}

		//
		// Tests System.Xml.NameTable.Add (char[], int, int)
		//		
		[Test]
		public void Add2 ()
		{
			char[] test = new char [4] { 'a', 'd', 'd', '2' };
			int index = 0;
			int length = 3; // "add"			

			AssertEquals ("add", table.Add (test, index, length));
		}

		//
		// Tests System.Xml.NameTable.Get (string)
		//
		[Test]
		public void Get1 ()
		{
			string get1 = "get1";
			string testGet = table.Add (get1);			

			AssertEquals (table.Get (get1), testGet);
			AssertSame (get1, testGet );
		}

		//
		// Tests System.Xml.NameTable.Get (char[], int, int)
		//
		[Test]
		public void Get2 ()
		{						
			char[] test = new char [4] { 'g', 'e', 't', '2' };
			int index = 0; 
			int length = 3; // "get"
			
			string testGet = table.Add (test, index, length);			

			AssertEquals (table.Get (test, index, length), testGet);
		}

		//
		// Tests System.Xml.NameTable.Get (char[], int, 0)
		//
		[Test]
		public void Get3 ()
		{
			char[] test = new char [4] { 't', 'e', 's', 't' };
			int index = 0;
			int length = 0;

			AssertEquals (table.Get (test, index, length), String.Empty);
		}
	}
}
