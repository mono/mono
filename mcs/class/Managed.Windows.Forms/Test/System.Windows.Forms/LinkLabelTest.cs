//
// LinkLabelTest.cs: MWF LinkLabel unit tests.
//
// Author:
//   Everaldo Canuto (ecanuto@novell.com)
//
// (C) 2007 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class LinkLabelTest
	{
		[Test]
		public void LinkLabelAccessibility ()
		{
			LinkLabel l = new LinkLabel ();
			Assert.IsNotNull (l.AccessibilityObject, "#1");
		}

		[Test]
		public void TestTabStop ()
		{
			LinkLabel l = new LinkLabel();

			Assert.IsFalse (l.TabStop, "#1");
			l.Text = "Hello";
			Assert.IsTrue (l.TabStop, "#2");
			l.Text = "";
			Assert.IsFalse (l.TabStop, "#3");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestLinkArea ()
		{
			LinkLabel l = new LinkLabel();

			Assert.AreEqual (0, l.LinkArea.Start, "#1");
			Assert.AreEqual (0, l.LinkArea.Length, "#2");
			l.Text = "Hello";
			Assert.AreEqual (0, l.LinkArea.Start, "#3");
			Assert.AreEqual (5, l.LinkArea.Length, "#4");
			l.Text = "";
			Assert.AreEqual (0, l.LinkArea.Start, "#5");
			Assert.AreEqual (0, l.LinkArea.Length, "#6");
		}
		
#if NET_2_0
		[TestFixture]
		public class LinkTest
		{
			[Test]
			public void Constructor ()
			{
				LinkLabel.Link l = new LinkLabel.Link ();
				
				Assert.AreEqual (null, l.Description, "A1");
				Assert.AreEqual (true, l.Enabled, "A2");
				Assert.AreEqual (0, l.Length, "A3");
				Assert.AreEqual (null, l.LinkData, "A4");
				Assert.AreEqual (string.Empty, l.Name, "A5");
				Assert.AreEqual (0, l.Start, "A6");
				Assert.AreEqual (null, l.Tag, "A7");
				Assert.AreEqual (false, l.Visited, "A8");

				l = new LinkLabel.Link (5, 20);

				Assert.AreEqual (null, l.Description, "A9");
				Assert.AreEqual (true, l.Enabled, "A10");
				Assert.AreEqual (20, l.Length, "A11");
				Assert.AreEqual (null, l.LinkData, "A12");
				Assert.AreEqual (string.Empty, l.Name, "A13");
				Assert.AreEqual (5, l.Start, "A14");
				Assert.AreEqual (null, l.Tag, "A15");
				Assert.AreEqual (false, l.Visited, "A16");

				l = new LinkLabel.Link (3, 7, "test");

				Assert.AreEqual (null, l.Description, "A17");
				Assert.AreEqual (true, l.Enabled, "A18");
				Assert.AreEqual (7, l.Length, "A19");
				Assert.AreEqual ("test", l.LinkData, "A20");
				Assert.AreEqual (string.Empty, l.Name, "A21");
				Assert.AreEqual (3, l.Start, "A22");
				Assert.AreEqual (null, l.Tag, "A23");
				Assert.AreEqual (false, l.Visited, "A24");
			}
		}
#endif
	}
}
