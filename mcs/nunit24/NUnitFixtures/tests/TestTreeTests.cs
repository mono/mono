// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using NUnit.Framework;

namespace NUnit.Fixtures.Tests
{
	/// <summary>
	/// Summary description for TestTreeTests.
	/// </summary>
	[TestFixture]
	public class TestTreeTests
	{
		static TestTree tree1 = new TestTree(
			"SomeClass" + Environment.NewLine +
			">Test1" + Environment.NewLine +
			">Test2" + Environment.NewLine +
			">Test3" + Environment.NewLine +
			"AnotherClass" + Environment.NewLine +
			">Test4" + Environment.NewLine +
			">Test5" );

		static TestTree tree2 = new TestTree(
			"SomeClass >Test1 >Test2 >Test3" + Environment.NewLine +
			"AnotherClass >Test4 >Test5" );

		static TestTree tree3 = new TestTree(
			"SomeClass >Test1 >Test2 >Test3 AnotherClass >Test4 >Test5" );

		static TestTree tree4 = new TestTree(
			"SomeClass >Test1 >TestX >Test3 AnotherClass >Test4 >Test5" );

		[Test]
		public void MatchingTreesAreEqual()
		{
			Assert.AreEqual( tree1, tree1 ); 
			Assert.AreEqual( tree1, tree2 ); 
			Assert.AreEqual( tree1, tree3 ); 
		}

		[Test]
		public void NonMatchingTreesAreNotEqual()
		{
			Assert.AreNotEqual( tree1, tree4 ); 
			Assert.AreNotEqual( tree2, tree4 ); 
			Assert.AreNotEqual( tree3, tree4 ); 
		}
	}
}
