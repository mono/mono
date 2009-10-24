//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
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

using System;
using System.Windows.Forms.DataVisualization.Charting;
using NUnit.Framework;

namespace ChartingTests
{
	[TestFixture]
	public class ArrowAnnotationTest
	{
		[Test]
		public void Constructor ()
		{
			ArrowAnnotation aa = new ArrowAnnotation ();

			Assert.AreEqual (5, aa.ArrowSize, "A1");
			Assert.AreEqual (ArrowStyle.Simple, aa.ArrowStyle, "A2");
		}

		[Test]
		public void ArrowSizeProperty ()
		{
			ArrowAnnotation aa = new ArrowAnnotation ();

			Assert.AreEqual (5, aa.ArrowSize, "A1");

			aa.ArrowSize = 3;
			Assert.AreEqual (3, aa.ArrowSize, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ArrowSizePropertyAOORE ()
		{
			ArrowAnnotation aa = new ArrowAnnotation ();
			
			aa.ArrowSize = 0;
		}


		[Test]
		public void ArrowStyleProperty ()
		{
			ArrowAnnotation aa = new ArrowAnnotation ();

			Assert.AreEqual (ArrowStyle.Simple, aa.ArrowStyle, "A1");

			aa.ArrowStyle = ArrowStyle.DoubleArrow;
			Assert.AreEqual (ArrowStyle.DoubleArrow, aa.ArrowStyle, "A2");
		}
	}
}
