//
// System.Xml.NameTableTests.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Diagnostics;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class NameTableTests : TestCase
	{
		NameTable table;
		
		public NameTableTests (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
			table = new NameTable ();
		}

		//
		// Tests System.Xml.NameTable.Add (string)
		//		
		public void TestAdd1 ()
		{
			string testAdd =  table.Add ("add1");
			AssertEquals ("add1", testAdd);
		}

		//
		// Tests System.Xml.NameTable.Add (char[], int, int)
		//		
		public void TestAdd2 ()
		{
			char[] test = new char [4] { 'a', 'd', 'd', '2' };
			int index = 0;
			int length = 3; // "add"			

			AssertEquals ("add", table.Add (test, index, length));
		}

		//
		// Tests System.Xml.NameTable.Get (string)
		//
		public void TestGet1 ()
		{
			string testGet = table.Add ("get1");			

			AssertEquals (table.Get ("get1"), testGet);
		}

		//
		// Tests System.Xml.NameTable.Get (char[], int, int)
		//
		public void TestGet2 ()
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
		public void TestGet3 ()
		{
			char[] test = new char [4] { 't', 'e', 's', 't' };
			int index = 0;
			int length = 0;

			AssertEquals (table.Get (test, index, length), String.Empty);
		}
	}
}
