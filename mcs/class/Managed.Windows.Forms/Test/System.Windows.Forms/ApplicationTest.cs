//
// ApplicationContextTest.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ApplicationTest
	{
		ApplicationContext ctx;

		void form_visible_changed (object sender, EventArgs e)
		{
			Assert.AreEqual (sender, ctx.MainForm, "1");
			((Form)sender).Close();
		}

		[Test]
		public void ContextMainFormTest ()
		{
			Form f1 = new Form ();
			ctx = new ApplicationContext (f1);

			f1.VisibleChanged += new EventHandler (form_visible_changed);

			Application.Run (ctx);

			Assert.IsNull (ctx.MainForm, "2");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void RestartNotSupportedExceptionTest ()
		{
			Application.Restart ();
		}
#endif
	}
}
