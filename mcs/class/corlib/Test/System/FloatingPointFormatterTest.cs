// FloatingPointFormatterTest.cs - NUnit Test Cases for the System.FloatingPointFormatter class
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class FloatingPointFormatterTest : Assertion
	{
		[Test]
		public void Format1 ()
		{
                        AssertEquals ("F1", "100000000000000", 1.0e+14.ToString ());
                        AssertEquals ("F2", "1.E+15", 1.0e+15.ToString ());
                        AssertEquals ("F3", "1.E+16", 1.0e+16.ToString ());
                        AssertEquals ("F4", "1.E+17", 1.0e+17.ToString ());
		}
        }
}
