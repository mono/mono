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
	public class UserControlTest
	{
		UserControl uc = null;

		[SetUp]
		public void SetUp()
		{
			uc = new UserControl();
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
		}
#endif	
	}
}
