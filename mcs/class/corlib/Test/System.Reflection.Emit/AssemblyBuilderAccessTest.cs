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
	public class AssemblyBuilderAccessTest
	{
		[Test]
		public void RunTest ()
		{
			Assert.AreEqual((int) AssemblyBuilderAccess.Run, 1);
		}
		
		[Test]
		public void RunAndSaveTest ()
		{
			Assert.AreEqual ((int) AssemblyBuilderAccess.RunAndSave, 3);
		}
		
		[Test]
		public void SaveTest()
		{
			Assert.AreEqual ((int) AssemblyBuilderAccess.Save, 2);
		}		
	}
}
