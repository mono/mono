using System;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class NotifyIconTest : TestHelper
	{
		[Test]
		public void PropertyTag ()
		{
			NotifyIcon ni = new NotifyIcon ();
			object o = "tomato";
			
			Assert.AreEqual (null, ni.Tag, "A1");
			
			ni.Tag = o;
			Assert.AreSame (o, ni.Tag, "A2");
		}

		[Test]
		public void PropertyContextMenuStrip ()
		{
			NotifyIcon ni = new NotifyIcon ();
			ContextMenuStrip cms = new ContextMenuStrip ();
			cms.Items.Add ("test item");

			Assert.AreEqual (null, ni.ContextMenuStrip, "B1");

			ni.ContextMenuStrip = cms;
			Assert.AreSame (cms, ni.ContextMenuStrip, "B2");
		}
		
		[Test]
		public void Balloon ()
		{
			NotifyIcon ni = new NotifyIcon ();
			ni.Text = "NotifyIcon Text";
			ni.BalloonTipTitle = "Balloon Tip Title";
			ni.BalloonTipText = "Balloon Tip Text.";
			ni.BalloonTipIcon = ToolTipIcon.None;
			ni.Icon = SystemIcons.Information;
			ni.Visible = true;
			ni.ShowBalloonTip (1);
			ni.Dispose ();
		}
	}
}
