// Tests for System.Drawing.PointF.cs
//
// Author:
//	Ravindra (rkumar@novell.com)
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
using System.Drawing;

namespace MonoTests.System.Drawing 
{
	[TestFixture]	
	public class PointFTest : Assertion 
	{
		PointF pt11_99;
		PointF pt11_0;
		PointF pt0_11;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			pt11_99 = new PointF (1.1F, 9.9F);
			pt11_0 = new PointF (1.1F, 0F);
			pt0_11 = new PointF (0F, 1.1F);
		}

		[Test]
		public void TestConstructors ()
		{
			PointF pt = new PointF (1.5F, 5.8F);
			AssertEquals ("C#1", 1.5F, pt.X);
			AssertEquals ("C#2", 5.8F, pt.Y);
		}

		[Test]
		public void TestEmptyField () 
		{
			PointF pt = new PointF (0.0F, 0.0F);
			AssertEquals ("EMP#1", pt, PointF.Empty);
		}

		[Test]
		public void TestProperties () 
		{
			PointF pt = new PointF (0.0F, 0.0F);
	
			Assert ("P#1", pt.IsEmpty);
			Assert ("P#2", ! pt11_99.IsEmpty);
			AssertEquals ("P#3", 1.1F, pt11_0.X);
			AssertEquals ("P#4", 1.1F, pt0_11.Y);
		}

		[Test]
		public void TestEquals () 
		{
			AssertEquals ("EQ#1", pt11_99, pt11_99);
			AssertEquals ("EQ#2", pt11_99, new PointF (1.1F, 9.9F));
			Assert ("EQ#3", ! pt11_99.Equals (pt11_0));
			Assert ("EQ#4", ! pt11_99.Equals (pt0_11));
			Assert ("EQ#5", ! pt11_0.Equals (pt0_11));
		}

		
		[Test]
		public void TestAddition ()
		{
			AssertEquals ("ADD#1", pt11_0, pt11_0 + new Size (0, 0));
			AssertEquals ("ADD#2", pt0_11, pt0_11 + new Size (0, 0));
		}

		[Test]
		public void TestEqualityOp () 
		{
			Assert ("EOP#1", pt11_99 == pt11_99);
			Assert ("EOP#2", pt11_99 == new PointF (1.1F, 9.9F));
			Assert ("EOP#3", ! (pt11_99 == pt11_0));
			Assert ("EOP#4", ! (pt11_99 == pt0_11));
			Assert ("EOP#5", ! (pt11_0 == pt0_11));
		}

		[Test]
		public void TestInequalityOp () 
		{
			Assert ("IOP#1", ! (pt11_99 != pt11_99));
			Assert ("IOP#2", ! (pt11_99 != new PointF (1.1F, 9.9F)));
			Assert ("IOP#3", pt11_99 != pt11_0);
			Assert ("IOP#4", pt11_99 != pt0_11);
			Assert ("IOP#5", pt11_0 != pt0_11);
		}
	
		[Test]
		public void TestSubtraction () 
		{
			AssertEquals ("SUB#1", pt11_0, pt11_0 - new Size (0, 0));
			AssertEquals ("SUB#2", pt0_11, pt0_11 - new Size (0, 0));
		}
	
	}
}

