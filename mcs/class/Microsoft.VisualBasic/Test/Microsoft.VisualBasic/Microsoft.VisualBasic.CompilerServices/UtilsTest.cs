// UtilsTest.cs - NUnit Test Cases for class Microsoft.VisualBasic.CompilerServices.Utils
//
// Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2004 Rafael Teixeira
// 

using NUnit.Framework;
using System;
using Microsoft.VisualBasic.CompilerServices;

namespace MonoTests.Microsoft.VisualBasic.CompilerServices
{

	[TestFixture]
	public class UtilsTest : Assertion {
		
		[SetUp]
		public void GetReady() {}
	
		[TearDown]
		public void Clean() {}
	
		[Test]
		public void TestCopyArrayOneDimensionalShrinking() {
			string[] source = new string[] { "First", "Second", "Third" };
			string[] destination = new string[2];
			string[] result = (string[])Utils.CopyArray(source, destination);
			AssertEquals ("ResultIsDestination", destination, result);
			AssertEquals ("First", source[0], destination[0]);
			AssertEquals ("Second", source[1], destination[1]);
		}

		[Test]
		public void TestCopyArrayOneDimensionalExpanding() {
			string[] source = new string[] { "First", "Second" };
			string[] destination = new string[3];
			string[] result = (string[])Utils.CopyArray(source, destination);
			AssertEquals ("ResultIsDestination", destination, result);
			AssertEquals ("First", source[0], destination[0]);
			AssertEquals ("Second", source[1], destination[1]);
			AssertEquals ("EmptyThird", null, destination[2]);
		}
	
		[Test]
		public void TestCopyArrayBiDimensionalShrinking() {
			string[,] source = new string[2,2];
			source[0,0] = "First";
			source[0,1] = "Second";
			source[1,0] = "Third";
			source[1,1] = "Fourth";
			string[,] destination = new string[2,1];
			string[,] result = (string[,])Utils.CopyArray(source, destination);
			AssertEquals ("ResultIsDestination", destination, result);
			AssertEquals ("First", source[0,0], destination[0,0]);
			AssertEquals ("Third", source[1,0], destination[1,0]);
		}

		[Test]
		public void TestCopyArrayBiDimensionalExpanding() {
			string[,] source = new string[2,2];
			source[0,0] = "First";
			source[0,1] = "Second";
			source[1,0] = "Third";
			source[1,1] = "Fourth";
			string[,] destination = new string[2,3];
			string[,] result = (string[,])Utils.CopyArray(source, destination);
			AssertEquals ("ResultIsDestination", destination, result);
			AssertEquals ("First", source[0,0], destination[0,0]);
			AssertEquals ("Second", source[0,1], destination[0,1]);
			AssertEquals ("Third", source[1,0], destination[1,0]);
			AssertEquals ("Fourth", source[1,1], destination[1,1]);
			AssertEquals ("EmptyFifth", null, destination[0,2]);
			AssertEquals ("EmptySixth", null, destination[1,2]);
		}
	
		// An nice way to test for exceptions the class under test should 
		// throw is:
		/*
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void OnValid() {
			ConcreteCollection myCollection;
			myCollection = new ConcreteCollection();
			....
			AssertEquals ("#UniqueID", expected, actual);
			....
			Fail ("Message");
		}
		*/

	}
}
