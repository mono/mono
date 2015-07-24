//
// TreeViewEventsTest.cs: Test cases for TreeView events.
//
// Author:
//   Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// (C) 2007 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeViewEventsTest : TestHelper
	{
		Form f;
		TreeView tv;
		bool event_fired;

		[SetUp]
		protected override void SetUp () {
			f = new Form ();
			tv = new TreeView ();
			f.Controls.Add (tv);

			event_fired = false;
			base.SetUp ();
		}

		[Test]
		public void AfterLabelEditEventTest ()
		{
			tv.LabelEdit = true;

			TreeNode node = new TreeNode ("A");
			tv.Nodes.Add (node);

			tv.AfterLabelEdit += new NodeLabelEditEventHandler (AfterLabelEditHandler);

			f.Show ();

			node.BeginEdit ();
			Assert.IsTrue (node.IsEditing, "#A1");

			node.EndEdit (false);
			Assert.IsTrue (event_fired, "#B1");

			f.Dispose ();
		}

		void AfterLabelEditHandler (object o, NodeLabelEditEventArgs args)
		{
			Assert.AreEqual (false, args.Node.IsEditing, "AfterLabelEditHandler#A1");

			event_fired = true;
		}

		[Test]
		public void BeforeLabelEditEventTest ()
		{
			tv.LabelEdit = true;

			TreeNode node = new TreeNode ("A");
			tv.Nodes.Add (node);

			tv.BeforeLabelEdit += new NodeLabelEditEventHandler (BeforeLabelEditHandler);

			f.Show ();

			node.BeginEdit ();
			Assert.IsTrue (node.IsEditing, "#A1");

			node.EndEdit (false);
			Assert.IsTrue (event_fired, "#B1");

			f.Dispose ();
		}

		void BeforeLabelEditHandler (object o, NodeLabelEditEventArgs args)
		{
			Assert.AreEqual (false, args.Node.IsEditing, "BeforeLabelEditHandler#A1");

			event_fired = true;
		}

	}
}

