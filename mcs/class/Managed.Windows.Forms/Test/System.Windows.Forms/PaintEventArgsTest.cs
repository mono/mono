//
// System.Windows.Forms.PaintEventArgs unit tests
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class PaintEventArgsTest : TestHelper {

		private Graphics default_graphics;
		private Rectangle default_rect;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			default_graphics = Graphics.FromImage (bmp);
			default_rect = new Rectangle (Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue);
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullGraphics ()
		{
			new PaintEventArgs (null, default_rect);
		}
#else
		[Test]
		public void Constructor_NullGraphics ()
		{
			PaintEventArgs pea = new PaintEventArgs (null, default_rect);
			Assert.IsNull (pea.Graphics, "Graphics");
			Assert.AreEqual (default_rect, pea.ClipRectangle);
		}
#endif
		[Test]
		public void Constructor ()
		{
			PaintEventArgs pea = new PaintEventArgs (default_graphics, default_rect);
			Assert.AreSame (default_graphics, pea.Graphics, "Graphics");
			Assert.AreEqual (default_rect, pea.ClipRectangle);
		}

		[Test]
		public void Dispose ()
		{
			PaintEventArgs pea = new PaintEventArgs (default_graphics, default_rect);
			pea.Dispose ();
			// uho, under 2.0 we not really disposing the stuff - it means it's not ours to dispose!
			Assert.IsTrue (pea.Graphics.Transform.IsIdentity, "Graphics.Transform");
		}

		[Test]
		public void IDisposable_IDispose ()
		{
			Bitmap bmp = new Bitmap (1, 1);
			Graphics default_graphics = Graphics.FromImage (bmp);
			Rectangle default_rect = new Rectangle (Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue);

			PaintEventArgs pea = new PaintEventArgs (default_graphics, default_rect);
			(pea as IDisposable).Dispose ();
			// uho, under 2.0 we not really disposing the stuff - it means it's not ours to dispose!
			Assert.IsTrue (pea.Graphics.Transform.IsIdentity, "Graphics");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GraphicsDispose ()
		{
			PaintEventArgs pea = new PaintEventArgs (default_graphics, default_rect);
			pea.Graphics.Dispose ();
			// a disposed graphics won't accept to return it's transformation matrix
			Assert.IsTrue (pea.Graphics.Transform.IsIdentity, "Graphics");
		}

		public class PaintEventArgsTester: PaintEventArgs {

			public PaintEventArgsTester (Graphics graphics, Rectangle clipRect)
				: base (graphics, clipRect)
			{
			}

			public void DisposeBool (bool disposing)
			{
				base.Dispose (disposing);
			}
		}

		[Test]
#if NET_2_0
		// under MS runtime it throws an exception under nunit-console, but not when running under NUnit GUI
		[Category ("NotDotNet")]
#endif
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_True ()
		{
			Bitmap bmp = new Bitmap (1, 1);
			Graphics default_graphics = Graphics.FromImage (bmp);
			Rectangle default_rect = new Rectangle (Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue);

			PaintEventArgsTester pea = new PaintEventArgsTester (default_graphics, default_rect);
			pea.Graphics.Dispose ();
			pea.DisposeBool (true);
			Assert.IsTrue (pea.Graphics.Transform.IsIdentity, "Graphics.Transform");
		}

		[Test]
#if NET_2_0
		// under MS runtime it throws an exception under nunit-console, but not when running under NUnit GUI
		[Category ("NotDotNet")]
#endif
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_False ()
		{
			Bitmap bmp = new Bitmap (1, 1);
			Graphics default_graphics = Graphics.FromImage (bmp);
			Rectangle default_rect = new Rectangle (Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue);

			PaintEventArgsTester pea = new PaintEventArgsTester (default_graphics, default_rect);
			pea.Graphics.Dispose ();
			pea.DisposeBool (false);
			Assert.IsTrue (pea.Graphics.Transform.IsIdentity, "Graphics.Transform");
		}
	}
}
