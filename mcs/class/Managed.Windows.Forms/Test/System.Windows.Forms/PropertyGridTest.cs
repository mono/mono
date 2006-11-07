//
// PropertyGridTest.cs: Test cases for PropertyGrid.
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PropertyGridTest
	{
		[Test]
		public void SelectedObject ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			Assert.IsNull (pg.SelectedObject, "#A1");
			Assert.IsNotNull (pg.SelectedObjects, "#A2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#A3");
			pg.SelectedObject = button1;
			Assert.IsNotNull (pg.SelectedObject, "#B1");
			Assert.AreSame (button1, pg.SelectedObject, "#B2");
			Assert.IsNotNull (pg.SelectedObjects, "#B3");
			Assert.AreEqual (1, pg.SelectedObjects.Length, "#B4");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#B5");
			Assert.IsNotNull (pg.SelectedGridItem, "#B6");
		}

		[Test]
		public void SelectedObject_Null ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Assert.IsNull (pg.SelectedObject, "#A1");
			Assert.IsNotNull (pg.SelectedObjects, "#A2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#A3");
			pg.SelectedObject = null;
			Assert.IsNull (pg.SelectedObject, "#B1");
			Assert.IsNotNull (pg.SelectedObjects, "#B2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#B3");
		}

		[Test] // bug #79615
		public void SelectedObjects_Multiple ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			Button button1 = new Button ();
			Button button2 = new Button ();

			PropertyGrid pg = new PropertyGrid ();
			pg.SelectedObjects = new object [] { button1, button2 };
			form.Controls.Add (pg);
			form.Controls.Add (button1);
			form.Controls.Add (button2);
			Assert.IsNotNull (pg.SelectedObjects, "#1");
			Assert.AreEqual (2, pg.SelectedObjects.Length, "#2");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#3");
			Assert.AreSame (button2, pg.SelectedObjects [1], "#4");
			Assert.IsNotNull (pg.SelectedObject, "#5");
			Assert.AreSame (button1, pg.SelectedObject, "#6");
			form.Dispose ();
		}

		[Test]
		public void SelectedObjects_Null ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			pg.SelectedObjects = new object [] { button1 };
			Assert.IsNotNull (pg.SelectedObjects, "#A1");
			Assert.AreEqual (1, pg.SelectedObjects.Length, "#A2");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#A3");
			Assert.AreSame (button1, pg.SelectedObject, "#A4");
			pg.SelectedObjects = null;
			Assert.IsNotNull (pg.SelectedObjects, "#B1");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#B2");
			Assert.IsNull (pg.SelectedObject, "#B3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectedObjects_Null_Item ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			pg.SelectedObjects = new object [] { button1, null };
		}
	}
}
