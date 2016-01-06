// ***********************************************************************
// Copyright (c) 2004 Charlie Poole
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
// ***********************************************************************

using System;
using System.Text;
using NUnit.Framework;

namespace NUnit.Framework.Assertions
{
	[TestFixture]
	public class SameFixture : MessageChecker
	{
		[Test]
		public void Same()
		{
			string s1 = "S1";
			Assert.AreSame(s1, s1);
		}

		[Test,ExpectedException(typeof(AssertionException))]
		public void SameFails()
		{
			Exception ex1 = new Exception( "one" );
			Exception ex2 = new Exception( "two" );
			expectedMessage =
				"  Expected: same as <System.Exception: one>" + Env.NewLine +
				"  But was:  <System.Exception: two>" + Env.NewLine;
			Assert.AreSame(ex1, ex2);
		}

		[Test,ExpectedException(typeof(AssertionException))]
		public void SameValueTypes()
		{
			int index = 2;
			expectedMessage =
				"  Expected: same as 2" + Env.NewLine +
				"  But was:  2" + Env.NewLine;
			Assert.AreSame(index, index);
		}
	}
}
