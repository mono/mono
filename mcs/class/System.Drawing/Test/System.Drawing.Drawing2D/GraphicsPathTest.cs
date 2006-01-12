//
// System.Drawing.GraphicsPath unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Drawing2D {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class GraphicsPathTest {

		private void CheckEmpty (string prefix, GraphicsPath gp)
		{
			Assert.AreEqual (FillMode.Alternate, gp.FillMode, "FillMode");
			Assert.AreEqual (0, gp.PathData.Points.Length, "PathData.Points");
			Assert.AreEqual (0, gp.PathData.Types.Length, "PathData.Types");
			Assert.AreEqual (0, gp.PointCount, prefix + "PointCount");
		}

		[Test]
		public void GraphicsPath_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			CheckEmpty ("Empty.", gp);

			GraphicsPath clone = (GraphicsPath) gp.Clone ();
			CheckEmpty ("Clone.", gp);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GraphicsPath_Empty_PathPoints ()
		{
			Assert.IsNull (new GraphicsPath ().PathPoints);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GraphicsPath_Empty_PathTypes ()
		{
			Assert.IsNull (new GraphicsPath ().PathTypes);
		}
	}
}
