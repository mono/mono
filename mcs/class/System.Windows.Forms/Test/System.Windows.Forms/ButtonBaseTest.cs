//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//      Jonathan Pobst  <monkey@jpobst.com>
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ButtonBaseTest : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			MockButton b = new MockButton ();
			
			Assert.AreEqual (SystemColors.Control, b.BackColor, "A4");
			Assert.AreEqual (FlatStyle.Standard, b.FlatStyle, "A6");
			Assert.IsNull (b.Image, "A7");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.ImageAlign, "A8");
			Assert.AreEqual (-1, b.ImageIndex, "A9");
			Assert.IsNull (b.ImageList, "A11");
			Assert.AreEqual (ImeMode.Disable, b.ImeMode, "A12");
			Assert.AreEqual (string.Empty, b.Text, "A13");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.TextAlign, "A14");

			Assert.IsFalse (b.AutoEllipsis, "A1");
			Assert.IsFalse (b.AutoSize, "A2");
			Assert.AreEqual (string.Empty, b.ImageKey, "A10");
			Assert.AreEqual (TextImageRelation.Overlay, b.TextImageRelation, "A15");
			Assert.IsTrue (b.UseCompatibleTextRendering, "A16");
			Assert.IsTrue (b.UseMnemonic, "A17");
			Assert.IsTrue (b.UseVisualStyleBackColor, "A18");
			Assert.AreEqual (AccessibleStates.Focusable, b.AccessibilityObject.State, "A19");
		}

		[Test]
		public void IsDefault ()
		{
			MockButton b = new MockButton ();
			Assert.IsFalse (b.IsDefault, "#1");
			b.IsDefault = true;
			Assert.IsTrue (b.IsDefault, "#2");
			b.IsDefault = false;
			Assert.IsFalse (b.IsDefault, "#3");
		}
		
		private class MockButton : ButtonBase
		{
			public new bool IsDefault {
				get { return base.IsDefault; }
				set { base.IsDefault = value; }
			}
		}
	}
}
