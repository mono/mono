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
	public class AnnotationPathPointTest
	{
		[Test]
		public void Constructor1 ()
		{
			AnnotationPathPoint app = new AnnotationPathPoint ();

			Assert.AreEqual (0, app.X, "A1");
			Assert.AreEqual (0, app.Y, "A2");
			Assert.AreEqual (null, app.Tag, "A3");
		}

		[Test]
		public void Constructor2 ()
		{
			AnnotationPathPoint app = new AnnotationPathPoint (3, 3);

			Assert.AreEqual (3, app.X, "A1");
			Assert.AreEqual (3, app.Y, "A2");
			Assert.AreEqual (null, app.Tag, "A3");
		}

		[Test]
		public void Constructor3 ()
		{
			AnnotationPathPoint app = new AnnotationPathPoint (5, 5, 2);

			Assert.AreEqual (5, app.X, "A1");
			Assert.AreEqual (5, app.Y, "A2");
			Assert.AreEqual (null, app.Tag, "A3");
		}

		[Test]
		public void XProperty ()
		{
			AnnotationPathPoint app = new AnnotationPathPoint ();

			Assert.AreEqual (0, app.X, "A1");

			app.X = 5;
			Assert.AreEqual (5, app.X, "A2");

			app.X = -5;
			Assert.AreEqual (-5, app.X, "A3");
		}

		[Test]
		public void YProperty ()
		{
			AnnotationPathPoint app = new AnnotationPathPoint ();

			Assert.AreEqual (0, app.Y, "A1");

			app.Y = 5;
			Assert.AreEqual (5, app.Y, "A2");

			app.Y = -5;
			Assert.AreEqual (-5, app.Y, "A3");
		}
	}
}
