//
// StatusBarPanelTest.cs: Test cases for StatusBar.
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
	public class StatusBarPanelTest 
	{
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void MinimumWidth1 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual (10, p.MinWidth, "1");
			p.Width = 9;
		}

		[Test]
		[Category ("NotWorking")]
		public void MinimumWidth2 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 100;
			Assert.AreEqual (100, p.Width, "1");
		}

		[Test]
		public void ToStringTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual ("StatusBarPanel: {}", p.ToString(), "1");

			p.Text = "Hello";
			Assert.AreEqual ("StatusBarPanel: {Hello}", p.ToString(), "2");

			p.Text = "}";
			Assert.AreEqual ("StatusBarPanel: {}}", p.ToString(), "3");
		}
	}
}
