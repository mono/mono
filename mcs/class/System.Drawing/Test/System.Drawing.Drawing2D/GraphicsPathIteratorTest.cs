//
// System.Drawing.Drawing2D.GraphicPathIterator unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Drawing2D {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class GraphicsPathIteratorTest {

		private PointF [] pts_2f = new PointF [2] { new PointF (1, 2), new PointF (20, 30) };

		[Test]
		public void Ctor_Null ()
		{
			using (GraphicsPathIterator gpi = new GraphicsPathIterator (null)) {
				Assert.AreEqual (0, gpi.Count, "Count");
			}
		}

		[Test]
		public void NextMarker_Null ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					Assert.AreEqual (0, gpi.NextMarker (null));
				}
			}
		}

		[Test]
		public void NextSubpath_Null ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					bool closed;
					Assert.AreEqual (0, gpi.NextSubpath (null, out closed));
					Assert.IsTrue (closed, "Closed");
				}
			}
		}

		[Test]
		public void CopyData_NullPoints ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = null;
					byte [] types = new byte [1];
					Assert.Throws<NullReferenceException> (() => gpi.CopyData (ref points, ref types, 0, 1));
				}
			}
		}

		[Test]
		public void CopyData_NullTypes ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = new PointF [1];
					byte [] types = null;
					Assert.Throws<NullReferenceException> (() =>gpi.CopyData (ref points, ref types, 0, 1));
				}
			}
		}

		[Test]
		public void CopyData_DifferentSize ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = new PointF [1];
					byte [] types = new byte [2];
					Assert.Throws<ArgumentException> (() => gpi.CopyData (ref points, ref types, 0, 1));
				}
			}
		}

		[Test]
		public void Enumerate_NullPoints ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = null;
					byte [] types = new byte [2];
					Assert.Throws<NullReferenceException> (() => gpi.Enumerate (ref points, ref types));
				}
			}
		}

		[Test]
		public void Enumerate_NullTypes ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = new PointF [1];
					byte [] types = null;
					Assert.Throws<NullReferenceException> (() => gpi.Enumerate (ref points, ref types));
				}
			}
		}

		[Test]
		public void Enumerate_DifferentSize ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (GraphicsPathIterator gpi = new GraphicsPathIterator (gp)) {
					PointF [] points = new PointF [1];
					byte [] types = new byte [2];
					Assert.Throws<ArgumentException> (() => gpi.Enumerate (ref points, ref types));
				}
			}
		}
	}
}
