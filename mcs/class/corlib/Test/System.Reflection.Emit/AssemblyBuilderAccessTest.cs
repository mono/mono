// AssemblyBuilderAccessTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//
using System;
using System.Reflection.Emit;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	/// <summary>
	/// A simple class to test the values of the enum AssemblyBuilderAccess.
	/// </summary>
	[TestFixture]
	public class AssemblyBuilderAccessTest : Assertion
	{
		[Test]
		public void RunTest ()
		{
			AssertEquals("Testing Run value",
				(int) AssemblyBuilderAccess.Run , 1);
		}
		
		[Test]
		public void RunAndSaveTest ()
		{
			AssertEquals ("Testing RunAndSave value",
				(int) AssemblyBuilderAccess.RunAndSave , 3);
		}
		
		[Test]
		public void SaveTest()
		{
			AssertEquals ("Testing Save value",
				(int) AssemblyBuilderAccess.Save , 2);
		}

		
	}
}
