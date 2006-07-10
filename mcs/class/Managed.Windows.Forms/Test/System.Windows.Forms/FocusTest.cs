//
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//      Jackson Harper  (jackson@ximian.com)
//

using System;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class FocusTest {

		public class ControlPoker : Button {

			public void _Select (bool directed, bool forward)
			{
				Select (directed, forward);
			}
		}

		private ControlPoker [] flat_controls;

		[SetUp]
		protected virtual void SetUp ()
		{
			flat_controls = null;

			flat_controls = new ControlPoker [] {
				new ControlPoker (), new ControlPoker (), new ControlPoker ()
			};
		}

		[Test]
		public void ControlSelectNextFlatTest ()
		{
			Form form = new Form ();

			form.Controls.AddRange (flat_controls);
			form.Show ();

			Assert.IsTrue (flat_controls [0].Focused, "sanity-1");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "sanity-2");

			form.SelectNextControl (flat_controls [0], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A1");
			Assert.IsTrue (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [1], form.ActiveControl, "A4");

			form.SelectNextControl (flat_controls [1], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A5");
			Assert.IsFalse (flat_controls [1].Focused, "A6");
			Assert.IsTrue (flat_controls [2].Focused, "A7");
			Assert.AreEqual (flat_controls [2], form.ActiveControl, "A8");

			// Can't select anymore because we aren't wrapping
			form.SelectNextControl (flat_controls [2], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A9");
			Assert.IsFalse (flat_controls [1].Focused, "A10");
			Assert.IsTrue (flat_controls [2].Focused, "A11");
			Assert.AreEqual (flat_controls [2], form.ActiveControl, "A12");

			form.SelectNextControl (flat_controls [2], true, false, false, true);
			Assert.IsTrue (flat_controls [0].Focused, "A13");
			Assert.IsFalse (flat_controls [1].Focused, "A14");
			Assert.IsFalse (flat_controls [2].Focused, "A15");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A16");
		}

		[Test]
		public void SelectNextControlNullTest ()
		{
			Form form = new Form ();

			form.Show ();
			form.Controls.AddRange (flat_controls);

			form.SelectNextControl (null, true, false, false, false);
			Assert.IsTrue (flat_controls [0].Focused, "A1");
			Assert.IsFalse (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A4");

			form.SelectNextControl (null, true, false, false, false);
			Assert.IsTrue (flat_controls [0].Focused, "A5");
			Assert.IsFalse (flat_controls [1].Focused, "A6");
			Assert.IsFalse (flat_controls [2].Focused, "A7");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A8");
		}

		[Test]
		public void SelectControlTest ()
		{
			Form form = new Form ();

			form.Show ();
			form.Controls.AddRange (flat_controls);

			flat_controls [0]._Select (false, false);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A1");

			flat_controls [0]._Select (true, false);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A2");

			flat_controls [0]._Select (true, true);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A3");
		}

		[Test]
		public void GetNextControl ()
		{
			
		}

	}

}

