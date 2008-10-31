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
	public class LinkLabelTest : TestHelper
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

		[Test] // bug #344012
		public void InvalidateManualLinks ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			LinkLabel l = new LinkLabel ();
			l.Text = "linkLabel1";
			form.Controls.Add (l);

#if NET_2_0
			LinkLabel.Link link = new LinkLabel.Link (2, 5);
			l.Links.Add (link);
#else
			l.Links.Add (2, 5);
#endif

			form.Show ();
			form.Dispose ();
		}
		
		[Test]	// bug 410709
		public void LinkAreaSetter ()
		{
			// Basically this test is to show that setting LinkArea erased
			// any previous links
			LinkLabel l = new LinkLabel ();
			
			l.Text = "Really long text";
			
			Assert.AreEqual (1, l.Links.Count, "A1");
			
			l.Links.Clear ();
			l.Links.Add (0, 3);
			l.Links.Add (5, 3);

			Assert.AreEqual (2, l.Links.Count, "A2");
		
			l.LinkArea = new LinkArea (1, 7);

			Assert.AreEqual (1, l.Links.Count, "A3");
			Assert.AreEqual (1, l.LinkArea.Start, "A4");
			Assert.AreEqual (7, l.LinkArea.Length, "A5");
		}
	}


#if NET_2_0
	[TestFixture]
	public class LinkTest : TestHelper
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

	[TestFixture]
	public class LinkCollectionTest : TestHelper
	{
		[Test] // ctor (LinkLabel)
		public void Constructor1 ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links1 = new LinkLabel.LinkCollection (
				l);
			LinkLabel.LinkCollection links2 = new LinkLabel.LinkCollection (
				l);

			Assert.AreEqual (1, links1.Count, "#A1");
			Assert.IsFalse (links1.IsReadOnly, "#A2");
#if NET_2_0
			Assert.IsFalse (links1.LinksAdded, "#A3");
#endif

			LinkLabel.Link link = links1 [0];
#if NET_2_0
			Assert.IsNull (link.Description, "#B1");
#endif
			Assert.IsTrue (link.Enabled, "#B2");
			Assert.AreEqual (21, link.Length, "#B3");
			Assert.IsNull (link.LinkData, "#B4");
#if NET_2_0
			Assert.IsNotNull (link.Name, "#B5");
			Assert.AreEqual (string.Empty, link.Name, "#B6");
#endif
			Assert.AreEqual (0, link.Start, "#B7");
#if NET_2_0
			Assert.IsNull (link.Tag, "#B8");
#endif
			Assert.IsFalse (link.Visited, "#B9");

			Assert.AreEqual (1, links2.Count, "#C1");
			Assert.IsFalse (links2.IsReadOnly, "#C2");
#if NET_2_0
			Assert.IsFalse (links2.LinksAdded, "#C3");
#endif
			Assert.AreSame (link, links2 [0], "#C4");
		}

		[Test] // ctor (LinkLabel)
		public void Constructor1_Owner_Null ()
		{
			try {
				new LinkLabel.LinkCollection ((LinkLabel) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("owner", ex.ParamName, "#6");
			}
		}

#if NET_2_0
		[Test] // Add (LinkLabel.Link)
		public void Add1 ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links1 = new LinkLabel.LinkCollection (
				l);
			LinkLabel.LinkCollection links2 = new LinkLabel.LinkCollection (
				l);

			LinkLabel.Link linkA = new LinkLabel.Link (0, 7);
			Assert.AreEqual (0, links1.Add (linkA), "#A1");
			Assert.AreEqual (1, links1.Count, "#A2");
			Assert.AreEqual (1, links2.Count, "#A3");
			Assert.IsTrue (links1.LinksAdded, "#A4");
			Assert.IsFalse (links2.LinksAdded, "#A5");
			Assert.AreSame (linkA, links1 [0], "#A6");
			Assert.AreSame (linkA, links2 [0], "#A7");

			LinkLabel.Link linkB = new LinkLabel.Link (8, 7);
			Assert.AreEqual (1, links1.Add (linkB), "#B1");
			Assert.AreEqual (2, links1.Count, "#B2");
			Assert.AreEqual (2, links2.Count, "#B3");
			Assert.IsTrue (links1.LinksAdded, "#B4");
			Assert.IsFalse (links2.LinksAdded, "#B5");
			Assert.AreSame (linkA, links1 [0], "#B6");
			Assert.AreSame (linkA, links2 [0], "#B7");
			Assert.AreSame (linkB, links1 [1], "#B8");
			Assert.AreSame (linkB, links2 [1], "#B9");

			LinkLabel.LinkCollection links3 = new LinkLabel.LinkCollection (
				l);
			Assert.AreEqual (2, links3.Count, "#C1");
			Assert.IsFalse (links3.LinksAdded, "#C2");
			Assert.AreSame (linkA, links3 [0], "#C3");
			Assert.AreSame (linkB, links3 [1], "#C4");
		}

		[Test] // Add (LinkLabel.Link)
		public void Add1_Overlap ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links = new LinkLabel.LinkCollection (
				l);

			LinkLabel.Link linkA = new LinkLabel.Link (0, 7);
			links.Add (linkA);
			Assert.AreEqual (1, links.Count, "#A1");
			Assert.IsTrue (links.LinksAdded, "#A2");
			Assert.AreSame (linkA, links [0], "#A3");

			LinkLabel.Link linkB = new LinkLabel.Link (5, 4);
			try {
				links.Add (linkB);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Overlapping link regions
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			Assert.AreEqual (2, links.Count, "#B5");
			Assert.IsTrue (links.LinksAdded, "#B6");
			Assert.AreSame (linkA, links [0], "#B7");
			Assert.AreSame (linkB, links [1], "#B8");
			Assert.AreEqual (0, linkA.Start, "#B9");
			Assert.AreEqual (7, linkA.Length, "#B10");
			Assert.AreEqual (5, linkB.Start, "#B11");
			Assert.AreEqual (4, linkB.Length, "#B12");

			LinkLabel.Link linkC = new LinkLabel.Link (14, 3);
			try {
				links.Add (linkC);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Overlapping link regions
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			Assert.AreEqual (3, links.Count, "#C5");
			Assert.IsTrue (links.LinksAdded, "#C6");
			Assert.AreSame (linkA, links [0], "#C7");
			Assert.AreSame (linkB, links [1], "#C8");
			Assert.AreSame (linkC, links [2], "#C9");
			Assert.AreEqual (0, linkA.Start, "#C10");
			Assert.AreEqual (7, linkA.Length, "#C11");
			Assert.AreEqual (5, linkB.Start, "#C12");
			Assert.AreEqual (4, linkB.Length, "#C13");
			Assert.AreEqual (14, linkC.Start, "#C14");
			Assert.AreEqual (3, linkC.Length, "#C15");
		}

		[Test] // Add (LinkLabel.Link)
		public void Add1_Value_Null ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links = new LinkLabel.LinkCollection (
				l);
			try {
				links.Add ((LinkLabel.Link) null);
				Assert.Fail ("#1");
			} catch (NullReferenceException) {
			}
		}
#endif

		[Test] // Add (int, int)
		public void Add2 ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links1 = new LinkLabel.LinkCollection (
				l);
			LinkLabel.LinkCollection links2 = new LinkLabel.LinkCollection (
				l);

			LinkLabel.Link linkA = links1.Add (0, 7);
			Assert.AreEqual (1, links1.Count, "#A1");
			Assert.AreEqual (1, links2.Count, "#A2");
#if NET_2_0
			Assert.IsTrue (links1.LinksAdded, "#A3");
			Assert.IsFalse (links2.LinksAdded, "#A4");
#endif
			Assert.AreSame (linkA, links1 [0], "#A5");
			Assert.AreSame (linkA, links2 [0], "#A6");

			LinkLabel.Link linkB = links1.Add (8, 7);
			Assert.AreEqual (2, links1.Count, "#B1");
			Assert.AreEqual (2, links2.Count, "#B2");
#if NET_2_0
			Assert.IsTrue (links1.LinksAdded, "#B3");
			Assert.IsFalse (links2.LinksAdded, "#B4");
#endif
			Assert.AreSame (linkA, links1 [0], "#B5");
			Assert.AreSame (linkA, links2 [0], "#B6");
			Assert.AreSame (linkB, links1 [1], "#B7");
			Assert.AreSame (linkB, links2 [1], "#B8");

			LinkLabel.LinkCollection links3 = new LinkLabel.LinkCollection (
				l);
			Assert.AreEqual (2, links3.Count, "#C1");
#if NET_2_0
			Assert.IsFalse (links3.LinksAdded, "#C2");
#endif
			Assert.AreSame (linkA, links3 [0], "#C3");
			Assert.AreSame (linkB, links3 [1], "#C4");
		}

		[Test] // Add (int, int)
		public void Add2_Overlap ()
		{
			LinkLabel l = new LinkLabel ();
			l.Text = "Managed Windows Forms";

			LinkLabel.LinkCollection links = new LinkLabel.LinkCollection (
				l);

			LinkLabel.Link linkA = links.Add (0, 7);
			Assert.AreEqual (1, links.Count, "#A1");
#if NET_2_0
			Assert.IsTrue (links.LinksAdded, "#A2");
#endif
			Assert.AreSame (linkA, links [0], "#A3");

			try {
				links.Add (5, 4);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Overlapping link regions
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			Assert.AreEqual (2, links.Count, "#B5");
#if NET_2_0
			Assert.IsTrue (links.LinksAdded, "#B6");
#endif
			Assert.AreSame (linkA, links [0], "#B7");
			Assert.IsNotNull (links [1], "#B8");
			Assert.AreEqual (0, linkA.Start, "#B9");
			Assert.AreEqual (7, linkA.Length, "#B10");
			Assert.AreEqual (5, links [1].Start, "#B11");
			Assert.AreEqual (4, links [1].Length, "#B12");

			try {
				links.Add (14, 3);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Overlapping link regions
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			Assert.AreEqual (3, links.Count, "#C5");
#if NET_2_0
			Assert.IsTrue (links.LinksAdded, "#C6");
#endif
			Assert.AreSame (linkA, links [0], "#C7");
			Assert.IsNotNull (links [1], "#C8");
			Assert.IsNotNull (links [2], "#C9");
			Assert.AreEqual (0, linkA.Start, "#C10");
			Assert.AreEqual (7, linkA.Length, "#C11");
			Assert.AreEqual (5, links [1].Start, "#C12");
			Assert.AreEqual (4, links [1].Length, "#C13");
			Assert.AreEqual (14, links [2].Start, "#C14");
			Assert.AreEqual (3, links [2].Length, "#C15");
		}
	}
}
