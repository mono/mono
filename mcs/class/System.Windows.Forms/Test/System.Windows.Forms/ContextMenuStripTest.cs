//
// ContextMenuTestStrip.cs: Test cases for ContextMenuStrip
//
// Author:
//   Nikita Voronchev (nikita.voronchev@ru.axxonsoft.com)
//
// (C) 2020 AxxonSoft (https://www.axxonsoft.com/)
//

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	// TODO:
	//  -- Tests around `OwnerItem`.

	[TestFixture]
	public class ContextMenuStripTest : TestHelper
	{
		static TestExtendedForm form;
		static Label explicitMenuSrcLabel;
		static TestExtendedLabel testExtendedLabel;
		static ContextMenuStrip contextMenuStrip;

		static readonly Lazy<Control>[] testCaseExplicitMenuSources = new Lazy<Control>[] {
			new Lazy<Control>(() => null),
			new Lazy<Control>(() => explicitMenuSrcLabel)
		};  // Involve `Lazy` to use `TestCaseSource` attribute.

		static readonly Lazy<ITestExtendedControl>[] testCaseAssociatedControls = new Lazy<ITestExtendedControl>[] {
			new Lazy<ITestExtendedControl>(() => form),
			new Lazy<ITestExtendedControl>(() => testExtendedLabel)
		};  // Involve `Lazy` to use `TestCaseSource` attribute.

		[SetUp]
		public void SetUp()
		{
			form = new TestExtendedForm ();
			explicitMenuSrcLabel = new Label ();
			testExtendedLabel = new TestExtendedLabel ();
			contextMenuStrip = new ContextMenuStrip ();
			
			form.ShowInTaskbar = false;
			form.Controls.Add (explicitMenuSrcLabel);
			form.Controls.Add (testExtendedLabel);
		}

		[TearDown]
		public void TearDown()
		{
			contextMenuStrip.Close ();
			form.Controls.Clear ();

			contextMenuStrip.Dispose ();
			testExtendedLabel.Dispose ();
			explicitMenuSrcLabel.Dispose ();
			form.Dispose ();
		}

		[Test, TestCaseSource ("testCaseExplicitMenuSources")]
		public void DirectShowTest01 (Lazy<Control> explicitMenuSrc)
		{
			AssingOwner (explicitMenuSrc.Value);
			contextMenuStrip.Show ();
			Assert.IsNull (contextMenuStrip.SourceControl, "SourceControl");
		}

		[Test, TestCaseSource ("testCaseExplicitMenuSources")]
		public void DirectShowTest02 (Lazy<Control> explicitMenuSrc)
		{
			AssingOwner (explicitMenuSrc.Value);
			contextMenuStrip.Show (form, Point.Empty);
			Assert.AreEqual (form, contextMenuStrip.SourceControl, "SourceControl");
		}

		[Test, TestCaseSource ("testCaseExplicitMenuSources")]
		public void DirectShowTest03 (Lazy<Control> explicitMenuSrc)
		{
			AssingOwner (explicitMenuSrc.Value);
			contextMenuStrip.Show (explicitMenuSrcLabel, Point.Empty);
			Assert.AreEqual (explicitMenuSrcLabel, contextMenuStrip.SourceControl, "SourceControl");
		}

		[Test, TestCaseSource ("testCaseExplicitMenuSources")]
		public void DirectShowTest04 (Lazy<Control> explicitMenuSrc)
		{
			AssingOwner (explicitMenuSrc.Value);
			contextMenuStrip.Show (testExtendedLabel, Point.Empty);
			Assert.AreEqual (testExtendedLabel, contextMenuStrip.SourceControl, "SourceControl");
		}

		[Test, TestCaseSource ("testCaseExplicitMenuSources")]
		public void DirectShowTest05 (Lazy<Control> explicitMenuSrc)
		{
			AssingOwner (explicitMenuSrc.Value);
			contextMenuStrip.Show (form, Point.Empty);
			contextMenuStrip.Close ();
			contextMenuStrip.Show ();
			Assert.IsNull (contextMenuStrip.SourceControl, "SourceControl");
		}

		[Test, TestCaseSource("testCaseAssociatedControls")]
		public void ContextShowTest (Lazy<ITestExtendedControl> associatedControl)
		{
			bool menuHasBeenOpened = false;
			contextMenuStrip.Opened += (sender, args) => { menuHasBeenOpened = true; };
			
			var assCtrl = associatedControl.Value;
			assCtrl.ContextMenuStrip = contextMenuStrip;
			
			Assert.IsFalse (menuHasBeenOpened, "menuHasBeenOpened");
			assCtrl.EmulateWmContextMenu ();
			Assert.IsTrue (menuHasBeenOpened, "menuHasBeenOpened");
			Assert.AreEqual (assCtrl, contextMenuStrip.SourceControl, "SourceControl");

		}

		#region Helpers

		private void AssingOwner (Control explicitMenuSrc)
		{
			if (explicitMenuSrc != null)
				explicitMenuSrc.ContextMenuStrip = contextMenuStrip;
		}

		public interface ITestExtendedControl
		{
			void EmulateWmContextMenu ();
			ContextMenuStrip ContextMenuStrip { set; }
		}

		class TestExtendedForm : Form, ITestExtendedControl
		{
			public void EmulateWmContextMenu ()
			{
				var m = TestExtendedControlHelper.MakeWmContextMenu ();
				WndProc (ref m);
			}
		}

		class TestExtendedLabel : Label, ITestExtendedControl
		{
			public void EmulateWmContextMenu ()
			{
				var m = TestExtendedControlHelper.MakeWmContextMenu ();
				WndProc (ref m);
			}
		}

		static class TestExtendedControlHelper
		{
			public static Message MakeWmContextMenu ()
			{
				return new Message () {
					Msg = (int)Msg.WM_CONTEXTMENU,
					LParam = IntPtr.Zero
				};
			}
		}

		#endregion  // end of Helpers
	}
}
