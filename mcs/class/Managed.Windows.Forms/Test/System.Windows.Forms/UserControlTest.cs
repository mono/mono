//
// UserControlTest.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Authors:
//   	Daniel Nauck    (dna(at)mono-project(dot)de)


using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using System.Collections;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class UserControlTest : TestHelper
	{
		UserControl uc = null;

		[SetUp]
		protected override void SetUp () {
			uc = new UserControl();
			base.SetUp ();
		}

		[Test]
		public void PropertyTest()
		{
			Assert.AreEqual(string.Empty, uc.Text, "#A1");

#if NET_2_0
			Assert.AreEqual(BorderStyle.None, uc.BorderStyle, "#A2");
			uc.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual(BorderStyle.Fixed3D, uc.BorderStyle, "#A3");
			uc.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual(BorderStyle.FixedSingle, uc.BorderStyle, "#A4");
			uc.BorderStyle = BorderStyle.None;
			Assert.AreEqual(BorderStyle.None, uc.BorderStyle, "#A5");
#endif
		}

#if NET_2_0
		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void BorderStyleInvalidEnumArgumentException()
		{
			uc.BorderStyle = (BorderStyle) 9999;
		}
		
		[Test]
		public void MethodCreateParams ()
		{
			ExposeProtectedProperties uc = new ExposeProtectedProperties ();

			Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD, (WindowStyles)uc.CreateParams.Style, "D1");
			Assert.AreEqual (WindowExStyles.WS_EX_CONTROLPARENT, (WindowExStyles)uc.CreateParams.ExStyle, "D2");
		}

		private class ExposeProtectedProperties : UserControl
		{
			public new CreateParams CreateParams { get { return base.CreateParams; } }
		}

		[Test]
		public void AutoSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Panel p = new Panel ();
			p.AutoSize = true;
			f.Controls.Add (p);

			Button b = new Button ();
			b.Size = new Size (200, 200);
			b.Location = new Point (200, 200);
			p.Controls.Add (b);

			f.Show ();

			Assert.AreEqual (new Size (403, 403), p.ClientSize, "A1");

			p.Controls.Remove (b);
			Assert.AreEqual (new Size (200, 100), p.ClientSize, "A2");

			p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Assert.AreEqual (new Size (0, 0), p.ClientSize, "A3");
			
			f.Close ();
		}

		[Test]
		public void PreferredSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			UserControl p = new UserControl ();
			f.Controls.Add (p);

			Button b1 = new Button ();
			b1.Size = new Size (200, 200);
			b1.Dock = DockStyle.Fill;
			p.Controls.Add (b1);

			Button b = new Button ();
			b.Size = new Size (100, 100);
			b.Dock = DockStyle.Top;
			p.Controls.Add (b);
			
			f.Show ();
			
			Assert.AreEqual (new Size (0, 100), p.PreferredSize, "A1");
			
			b1.Dock = DockStyle.Left;
			Assert.AreEqual (new Size (200, 100), p.PreferredSize, "A2");

			b1.Dock = DockStyle.None;
			Assert.AreEqual (new Size (203, 203), p.PreferredSize, "A3");

			b1.Dock = DockStyle.Fill;
			b.Dock = DockStyle.Fill;
			Assert.AreEqual (new Size (0, 0), p.PreferredSize, "A4");
			
			b1.Dock = DockStyle.Top;
			b.Dock = DockStyle.Left;

			Assert.AreEqual (new Size (100, 200), p.PreferredSize, "A5");
		
			Button b2 = new Button ();
			b2.Size = new Size (50, 50);
			p.Controls.Add (b2);

			Assert.AreEqual (new Size (100, 200), p.PreferredSize, "A6");
			
			b2.Left = 300;
			Assert.AreEqual (new Size (353, 200), p.PreferredSize, "A7");

			b2.Top = 300;
			Assert.AreEqual (new Size (353, 353), p.PreferredSize, "A8");

			b2.Anchor = AnchorStyles.Bottom;
			Assert.AreEqual (new Size (100, 200), p.PreferredSize, "A9");

			b2.Anchor = AnchorStyles.Left;
			Assert.AreEqual (new Size (353, 353), p.PreferredSize, "A10");
			
			f.Dispose ();
		}
	
#endif	
	}
}
