//
// ToolBarButtonTest.cs: Test cases for ToolBarButton.
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolBarButtonTest : TestHelper 
	{
		[Test]
		public void CtorTest1 ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.IsNull (tbb.DropDownMenu, "A3");
			Assert.IsTrue (tbb.Enabled, "A4");
			Assert.AreEqual (-1, tbb.ImageIndex, "A5");
			Assert.IsFalse (tbb.PartialPush, "A6");
			Assert.IsFalse (tbb.Pushed, "A7");
			Assert.AreEqual (Rectangle.Empty, tbb.Rectangle, "A8");
			Assert.AreEqual (ToolBarButtonStyle.PushButton, tbb.Style, "A8");
			Assert.IsNull (tbb.Tag, "A9");
			Assert.AreEqual ("", tbb.Text, "A10");
			Assert.AreEqual ("", tbb.ToolTipText, "A11");
			Assert.IsTrue (tbb.Visible, "A12");
		}

		[Test]
		public void CtorTest2 ()
		{
			ToolBarButton tbb = new ToolBarButton ("hi there");
			Assert.IsNull (tbb.DropDownMenu, "A3");
			Assert.IsTrue (tbb.Enabled, "A4");
			Assert.AreEqual (-1, tbb.ImageIndex, "A5");
			Assert.IsFalse (tbb.PartialPush, "A6");
			Assert.IsFalse (tbb.Pushed, "A7");
			Assert.AreEqual (Rectangle.Empty, tbb.Rectangle, "A8");
			Assert.AreEqual (ToolBarButtonStyle.PushButton, tbb.Style, "A8");
			Assert.IsNull (tbb.Tag, "A9");
			Assert.AreEqual ("hi there", tbb.Text, "A10");
			Assert.AreEqual ("", tbb.ToolTipText, "A11");
			Assert.IsTrue (tbb.Visible, "A12");
		}

		[Test]
		public void ToolTipText ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.AreEqual ("", tbb.ToolTipText, "A1");

			tbb.ToolTipText = "hi there";
			Assert.AreEqual ("hi there", tbb.ToolTipText, "A2");

			tbb.ToolTipText = null;
			Assert.AreEqual ("", tbb.ToolTipText, "A3");
		}

		[Test]
		public void Text ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.AreEqual ("", tbb.Text, "A1");

			tbb.Text = "hi there";
			Assert.AreEqual ("hi there", tbb.Text, "A2");

			tbb.Text = null;
			Assert.AreEqual ("", tbb.Text, "A3");
		}
		
#if NET_2_0
		[Test]
		public void Name ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.AreEqual ("", tbb.Name, "A1");
			tbb.Name = "abc";
			Assert.AreEqual ("abc", tbb.Name, "A2");
			tbb.Name = "";
			Assert.AreEqual ("", tbb.Name, "A3");
			tbb.Name = null;
			Assert.AreEqual ("", tbb.Name, "A4");
		}
		
		[Test]
		public void BehaviorImageIndexAndKey ()
		{
			// Basically, this shows that whichever of [ImageIndex|ImageKey]
			// is set last resets the other to the default state
			ToolBarButton b = new ToolBarButton ();

			Assert.AreEqual (-1, b.ImageIndex, "D1");
			Assert.AreEqual (string.Empty, b.ImageKey, "D2");

			b.ImageIndex = 6;
			Assert.AreEqual (6, b.ImageIndex, "D3");
			Assert.AreEqual (string.Empty, b.ImageKey, "D4");
			
			b.ImageKey = "test";
			Assert.AreEqual (-1, b.ImageIndex, "D5");
			Assert.AreEqual ("test", b.ImageKey, "D6");
		}
#endif
	}

}
