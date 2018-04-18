// Tests for System.Drawing.Size.cs
//
// Author: Ravindra (rkumar@novell.com)
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
using System.Security.Permissions;

namespace MonoTests.System.Drawing 
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class SizeTest 
	{
		Size sz1_1;
		Size sz1_0;
		Size sz0_1;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			sz1_1 = new Size (1, 1);
			sz1_0 = new Size (1, 0);
			sz0_1 = new Size (0, 1);
		}

		[Test]
		public void TestConstructors ()
		{
			Size sz_wh = new Size (1, 5);
			Assert.AreEqual (1, sz_wh.Width, "C#1");
			Assert.AreEqual (5, sz_wh.Height, "C#2");

			Size sz_pt = new Size (new Point (1, 5));
			Assert.AreEqual (1, sz_pt.Width, "C#3");
			Assert.AreEqual (5, sz_pt.Height, "C#4");

			Assert.AreEqual (sz_wh, sz_pt, "C#5");
		}

		[Test]
		public void TestEmptyField () 
		{
			Size sz = new Size (0, 0);
			Assert.AreEqual (sz, Size.Empty, "EMP#1");
		}

		[Test]
		public void TestProperties () 
		{
			Size sz = new Size (0, 0);
	
			Assert.IsTrue (sz.IsEmpty, "P#1");
			Assert.IsTrue (! sz1_1.IsEmpty, "P#2");
			Assert.AreEqual (1, sz1_0.Width, "P#3");
			Assert.AreEqual (1, sz0_1.Height, "P#4");
		}

		[Test]
		public void TestCeiling ()
		{
			SizeF sf = new SizeF (0.5F, 0.6F);
			Assert.AreEqual (sz1_1, Size.Ceiling (sf), "CL#1");

			sf = new SizeF (1.0F, 1.0F);
			Assert.AreEqual (sz1_1, Size.Ceiling (sf), "CL#2");
		}

		[Test]
		public void TestEquals () 
		{
			Assert.AreEqual (sz1_1, sz1_1, "EQ#1");
			Assert.AreEqual (sz1_1, new Size (1, 1), "EQ#2");
			Assert.IsTrue (! sz1_1.Equals (sz1_0), "EQ#3");
			Assert.IsTrue (! sz1_1.Equals (sz0_1), "EQ#4");
			Assert.IsTrue (! sz1_0.Equals (sz0_1), "EQ#5");
		}

		[Test]
		public void TestRound ()
		{
			SizeF sf = new SizeF (0.3F, 0.7F);
			Assert.AreEqual (sz0_1, Size.Round (sf), "CL#1");

			sf = new SizeF (0.6F, 0.6F);
			Assert.AreEqual (sz1_1, Size.Round (sf), "CL#2");

			sf = new SizeF (1.0F, 1.0F);
			Assert.AreEqual (sz1_1, Size.Round (sf), "CL#3");
		}

		
		[Test]
		public void TestTruncate ()
		{
			SizeF sf = new SizeF (0.8f, 1.3f);
			Assert.AreEqual (sz0_1, Size.Truncate (sf), "TR#1");

			sf = new SizeF (1.9f, 1.9f);
			Assert.AreEqual (sz1_1, Size.Truncate (sf), "TR#2");

			sf = new SizeF (1.0f, 1.0f);
			Assert.AreEqual (sz1_1, Size.Truncate (sf), "TR#3");
		}

		[Test]
		public void TestAddition ()
		{
			Assert.AreEqual (sz1_1, sz1_0 + sz0_1, "ADD#1");
			Assert.AreEqual (sz1_1, sz1_1 + new Size (0, 0), "ADD#2");
		}

		[Test]
		public void TestEqualityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (sz1_1 == sz1_1, "EOP#1");
#pragma warning restore 1718
			Assert.IsTrue (sz1_1 == new Size (1, 1), "EOP#2");
			Assert.IsTrue (! (sz1_1 == sz1_0), "EOP#3");
			Assert.IsTrue (! (sz1_1 == sz0_1), "EOP#4");
			Assert.IsTrue (! (sz1_0 == sz0_1), "EOP#5");
		}

		[Test]
		public void TestInequalityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (! (sz1_1 != sz1_1), "IOP#1");
#pragma warning restore 1718
			Assert.IsTrue (! (sz1_1 != new Size (1, 1)), "IOP#2");
			Assert.IsTrue (sz1_1 != sz1_0, "IOP#3");
			Assert.IsTrue (sz1_1 != sz0_1, "IOP#4");
			Assert.IsTrue (sz1_0 != sz0_1, "IOP#5");
		}
	
		[Test]
		public void TestSubtraction () 
		{
			Assert.AreEqual (sz1_0, sz1_1 - sz0_1, "SUB#1");
			Assert.AreEqual (sz0_1, sz1_1 - sz1_0, "SUB#2");
		}

		[Test]
		public void TestSize2Point ()
		{
			Point pt1 = new Point (1, 1);
			Point pt2 = (Point) sz1_1;
	
			Assert.AreEqual (pt1, pt2, "SZ2PT#1");
		}
	
		[Test]
		public void TestSize2SizeF ()
		{
			SizeF sf1 = new SizeF (1.0F, 1.0F);
			SizeF sf2 = (SizeF) sz1_1;

			Assert.AreEqual (sf1, sf2, "SZ2SF#1");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual (sz1_0.ToString (), "{Width=1, Height=0}");
			Assert.AreEqual (Size.Empty.ToString (), "{Width=0, Height=0}");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			Assert.AreEqual (new Size (1, 0).GetHashCode (), sz1_0.GetHashCode (), "#1");
		}

		[Test]
		public void AddTest ()
		{
			Assert.AreEqual (sz1_1, Size.Add (sz1_0, sz0_1), "ADD#1");
			Assert.AreEqual (sz1_1, Size.Add (sz1_1, new Size (0, 0)), "ADD#2");
		}

		[Test]
		public void SubtractTest ()
		{
			Assert.AreEqual (sz1_0, Size.Subtract (sz1_1, sz0_1), "SUB#1");
			Assert.AreEqual (sz0_1, Size.Subtract (sz1_1, sz1_0), "SUB#2");
		}

	}
}

