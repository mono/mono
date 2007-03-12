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
	}
}
