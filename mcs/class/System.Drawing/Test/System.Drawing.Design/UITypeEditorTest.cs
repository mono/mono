//
// UITypeEditorTest.cs - Unit tests for System.Drawing.Design.UITypeEditor
//
// Author:
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

using NUnit.Framework;

using System;
using System.Drawing;
using System.Drawing.Design;

namespace MonoTests.System.Drawing.Design {

	[TestFixture]
	public class UITypeEditorTest {

		private UITypeEditor editor;
		private Graphics graphics;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			editor = new UITypeEditor ();

			Bitmap bitmap = new Bitmap (20, 20);
			graphics = Graphics.FromImage (bitmap);
		}

		[Test]
		public void DefaultValues ()
		{
			Assert.AreSame (graphics, editor.EditValue (null, graphics), "EditValue(2)");
			Assert.AreSame (graphics, editor.EditValue (null, null, graphics), "EditValue(3)");

			Assert.AreEqual (UITypeEditorEditStyle.None, editor.GetEditStyle (), "GetEditStyle()");
			Assert.AreEqual (UITypeEditorEditStyle.None, editor.GetEditStyle (null), "GetEditStyle(null)");

			Assert.IsFalse (editor.GetPaintValueSupported (), "GetPaintValueSupported()");
			Assert.IsFalse (editor.GetPaintValueSupported (null), "GetPaintValueSupported(null)");
#if NET_2_0
			Assert.IsFalse (editor.IsDropDownResizable, "IsDropDownResizable");
#endif
		}

#if !TARGET_JVM
		[Test]
		public void PaintValue_PaintValueEventArgs_Null ()
		{
			editor.PaintValue (null);
		}

		[Test]
		public void PaintValue_PaintValueEventArgs ()
		{
			editor.PaintValue (new PaintValueEventArgs (null, null, graphics, Rectangle.Empty));
		}

		[Test]
		public void PaintValue ()
		{
			editor.PaintValue (null, graphics, Rectangle.Empty);
		}
#endif
	}
}
