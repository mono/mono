// UtilsTest.cs - NUnit Test Cases for class Microsoft.VisualBasic.CompilerServices.Utils
//
// Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2004 Rafael Teixeira
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic.CompilerServices;

namespace MonoTests.Microsoft.VisualBasic.CompilerServices
{

	[TestFixture]
	public class UtilsTest {
		
		[SetUp]
		public void GetReady() {}
	
		[TearDown]
		public void Clean() {}
	
		[Test]
		public void TestCopyArrayOneDimensionalShrinking() {
			string[] source = new string[] { "First", "Second", "Third" };
			string[] destination = new string[2];
			string[] result = (string[])Utils.CopyArray(source, destination);
			Assert.AreSame (destination, result, "ResultIsDestination");
			Assert.AreEqual (source[0], destination[0], "First");
			Assert.AreEqual (source[1], destination[1], "Second");
		}

		[Test]
		public void TestCopyArrayOneDimensionalExpanding() {
			string[] source = new string[] { "First", "Second" };
			string[] destination = new string[3];
			string[] result = (string[])Utils.CopyArray(source, destination);
			Assert.AreSame (destination, result, "ResultIsDestination");
			Assert.AreEqual (source[0], destination[0], "First");
			Assert.AreEqual (source[1], destination[1], "Second");
			Assert.IsNull (destination[2], "EmptyThird");
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
			Assert.AreSame (destination, result, "ResultIsDestination");
			Assert.AreEqual (source[0,0], destination[0,0], "First");
			Assert.AreEqual (source[1,0], destination[1,0], "Third");
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
			Assert.AreSame (destination, result, "ResultIsDestination");
			Assert.AreEqual (source[0,0], destination[0,0], "First");
			Assert.AreEqual (source[0,1], destination[0,1], "Second");
			Assert.AreEqual (source[1,0], destination[1,0], "Third");
			Assert.AreEqual (source[1,1], destination[1,1], "Fourth");
			Assert.IsNull (destination[0,2], "EmptyFifth");
			Assert.IsNull (destination[1,2], "EmptySixth");
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
			Assert.AreEqual ("#UniqueID", expected, actual);
			....
			Fail ("Message");
		}
		*/

	}
}
