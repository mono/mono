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
			AssertEquals ("CopyArrayOneDimensionalShrinkingFirst", source[0], destination[0]);
			AssertEquals ("CopyArrayOneDimensionalShrinkingSecond", source[1], destination[1]);
			AssertEquals ("CopyArrayOneDimensionalShrinkingResultRank", destination.Rank, result.Rank);
#if NET_1_1
			AssertEquals ("CopyArrayOneDimensionalShrinkingResultLength", destination.LongLength, result.LongLength);
#else
			AssertEquals ("CopyArrayOneDimensionalShrinkingResultLength", destination.Length, result.Length);
#endif
			AssertEquals ("CopyArrayOneDimensionalShrinkingFirstOnResult", source[0], result[0]);
			AssertEquals ("CopyArrayOneDimensionalShrinkingSecondOnResult", source[1], result[1]);
		}

		[Test]
		public void TestCopyArrayOneDimensionalExpanding() {
			string[] source = new string[] { "First", "Second" };
			string[] destination = new string[3];
			string[] result = (string[])Utils.CopyArray(source, destination);
			AssertEquals ("CopyArrayOneDimensionalExpandingFirst", source[0], destination[0]);
			AssertEquals ("CopyArrayOneDimensionalExpandingSecond", source[1], destination[1]);
			AssertEquals ("CopyArrayOneDimensionalExpandingEmptyThird", null, destination[2]);
			AssertEquals ("CopyArrayOneDimensionalExpandingResultRank", destination.Rank, result.Rank);
#if NET_1_1
			AssertEquals ("CopyArrayOneDimensionalExpandingResultLength", destination.LongLength, result.LongLength);
#else
			AssertEquals ("CopyArrayOneDimensionalExpandingResultLength", destination.Length, result.Length);
#endif
			AssertEquals ("CopyArrayOneDimensionalExpandingFirstOnResult", source[0], result[0]);
			AssertEquals ("CopyArrayOneDimensionalExpandingSecondOnResult", source[1], result[1]);
			AssertEquals ("CopyArrayOneDimensionalExpandingEmptyThirdOnResult", null, result[2]);
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
