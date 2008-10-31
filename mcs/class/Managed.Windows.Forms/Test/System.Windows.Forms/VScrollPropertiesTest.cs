//
// VScrollPropertiesTest.cs
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
// Copyright (c) 2007 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class VScrollPropertiesTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			Assert.AreEqual (true, sp.Enabled, "A1");
			Assert.AreEqual (10, sp.LargeChange, "A2");
			Assert.AreEqual (100, sp.Maximum, "A3");
			Assert.AreEqual (0, sp.Minimum, "A4");
			Assert.AreEqual (1, sp.SmallChange, "A5");
			Assert.AreEqual (0, sp.Value, "A6");
			Assert.AreEqual (false, sp.Visible, "A7");
		}

		[Test]
		public void PropertyEnabled ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.Enabled = false;
			Assert.AreEqual (false, sp.Enabled, "B1");
		}

		[Test]
		public void PropertyLargeChange ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.LargeChange = 25;
			Assert.AreEqual (25, sp.LargeChange, "B1");
		}

		[Test]
		public void PropertyMaximum ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.Maximum = 200;
			Assert.AreEqual (200, sp.Maximum, "B1");
		}

		[Test]
		public void PropertyMinimum ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.Minimum = 20;
			Assert.AreEqual (20, sp.Minimum, "B1");
		}

		[Test]
		public void PropertySmallChange ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.SmallChange = 5;
			Assert.AreEqual (5, sp.SmallChange, "B1");
		}

		[Test]
		public void PropertyValue ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;
			
			sp.Value = 10;
			Assert.AreEqual (10, sp.Value, "B1");
		}

		[Test]
		public void PropertyVisible ()
		{
			ScrollableControl sc = new ScrollableControl ();
			ScrollProperties sp = sc.VerticalScroll;

			sp.Visible = true;
			Assert.AreEqual (true, sp.Visible, "B1");
		}

	}
}
#endif