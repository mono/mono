using System;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class NotifyIconTest
	{
#if NET_2_0
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
#endif
	}
}
