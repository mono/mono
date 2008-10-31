using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class SplitterTest : TestHelper 
	{

		/* make sure the Capture setting has nothing to do with SplitPosition (reopened bug #78871) */
		[Test]
		public void TestCaptureWhileSettingSplitPosition ()
		{
			Form f = new Form ();

			TextBox TextBox1 = new TextBox();
			TextBox1.Dock = DockStyle.Left;
			Splitter Splitter = new Splitter();
			Splitter.Dock = DockStyle.Left;
			TextBox TextBox2 = new TextBox();
			TextBox2.Dock = DockStyle.Fill;
			f.Controls.AddRange(new Control[] { TextBox2, Splitter, TextBox1 });
			Splitter.Capture = true;
			Splitter.SplitPosition = (f.ClientSize.Width - Splitter.Width) / 2;

			int position_with_capture = Splitter.SplitPosition;

			f.Dispose ();

			f = new Form ();

			TextBox1 = new TextBox();
			TextBox1.Dock = DockStyle.Left;
			Splitter = new Splitter();
			Splitter.Dock = DockStyle.Left;
			TextBox2 = new TextBox();
			TextBox2.Dock = DockStyle.Fill;
			f.Controls.AddRange(new Control[] { TextBox2, Splitter, TextBox1 });
			Splitter.Capture = true;
			Splitter.SplitPosition = (f.ClientSize.Width - Splitter.Width) / 2;

			Assert.AreEqual (Splitter.SplitPosition, position_with_capture, "1");
		}
		
#if NET_2_0
		[Test]
		public void DefaultCursor ()
		{
			MySplitter s = new MySplitter ();
			
			Assert.AreEqual (Cursors.Default, s.PublicDefaultCursor, "A1");
		}
		
		private class MySplitter : Splitter
		{
			public Cursor PublicDefaultCursor { get { return base.DefaultCursor; } }
		}
#endif
	}
}
