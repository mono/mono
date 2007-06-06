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
	public class ButtonBaseTest
	{
		[Test]
		public void Constructor ()
		{
			NullButton b = new NullButton ();
			
			Assert.AreEqual (SystemColors.Control, b.BackColor, "A4");
			Assert.AreEqual (FlatStyle.Standard, b.FlatStyle, "A6");
			Assert.AreEqual (null, b.Image, "A7");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.ImageAlign, "A8");
			Assert.AreEqual (-1, b.ImageIndex, "A9");
			Assert.AreEqual (null, b.ImageList, "A11");
			Assert.AreEqual (ImeMode.Disable, b.ImeMode, "A12");
			Assert.AreEqual (string.Empty, b.Text, "A13");
			Assert.AreEqual (ContentAlignment.MiddleCenter, b.TextAlign, "A14");

#if NET_2_0
			Assert.AreEqual (false, b.AutoEllipsis, "A1");
			Assert.AreEqual (false, b.AutoSize, "A2");
			Assert.AreEqual (string.Empty, b.ImageKey, "A10");
			Assert.AreEqual (TextImageRelation.Overlay, b.TextImageRelation, "A15");
			Assert.AreEqual (true, b.UseCompatibleTextRendering, "A16");
			Assert.AreEqual (true, b.UseMnemonic, "A17");
			Assert.AreEqual (true, b.UseVisualStyleBackColor, "A18");
#endif
		}
		
		private class NullButton : ButtonBase
		{
		}
	}
}
