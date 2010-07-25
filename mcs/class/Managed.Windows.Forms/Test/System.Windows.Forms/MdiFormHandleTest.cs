//
// FormTest.cs: Test cases for Form.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MdiFormHandleTest : TestHelper
	{
		private ProtectedForm main;
		private ProtectedForm child1;
		private ProtectedForm child2;
		
		[TearDown]
		protected override void TearDown ()
		{
			if (main != null) {
				main.Dispose ();
				main = null;
			}
			
			if (child1 != null) {
				child1.Dispose ();
				child1 = null;
			}
			
			if (child2 != null) {
				child2.Dispose ();
				child2 = null;
			}
			base.TearDown ();
		}
		
		void SetUp ()
		{
			TearDown ();
			main = new ProtectedForm ();
			main.IsMdiContainer = true;
			main.ShowInTaskbar = false;

			child1 = new ProtectedForm ();
			child1.MdiParent = main;

			child2 = new ProtectedForm ();
			child2.MdiParent = main;
			
			main.Show ();
		}
		
		[Test]
		public void TestPublicProperties ()
		{
			// This long, carpal-tunnel syndrome inducing test shows us that
			// the following properties cause the Handle to be created:
			// - AccessibilityObject	[get]
			// - Capture			[set]
			// - Handle			[get]
			// - Visible			[set]

			// A
			SetUp ();
			object o = child1.AccessibilityObject;
			Assert.IsTrue (child1.IsHandleCreated, "A0");
			
			SetUp ();
			o = child1.AccessibleDefaultActionDescription;
			child1.AccessibleDefaultActionDescription = "playdoh";
			Assert.IsFalse (child1.IsHandleCreated, "A1");
			
			SetUp ();
			o = child1.AccessibleDescription;
			child1.AccessibleDescription = "more playdoh!";
			Assert.IsFalse (child1.IsHandleCreated, "A2");

			SetUp ();
			o = child1.AccessibleName;
			child1.AccessibleName = "playdoh fun factory";
			Assert.IsFalse (child1.IsHandleCreated, "A3");

			SetUp ();
			o = child1.AccessibleRole;
			child1.AccessibleRole = AccessibleRole.Border;
			Assert.IsFalse (child1.IsHandleCreated, "A4");

			SetUp ();
			o = child1.AllowDrop;
			child1.AllowDrop = true;
			Assert.IsFalse (child1.IsHandleCreated, "A5");
			
			// If we don't reset the control, handle creation will fail
			// because AllowDrop requires STAThread, which Nunit doesn't do

			SetUp ();
			o = child1.Anchor;
			child1.Anchor = AnchorStyles.Right;
			Assert.IsFalse (child1.IsHandleCreated, "A6");
#if !__MonoCS__ && NET_2_0
			SetUp ();
			o = child1.AutoScrollOffset;
			child1.AutoScrollOffset = new Point (40, 40);
			Assert.IsFalse (child1.IsHandleCreated, "A7");
#endif
#if NET_2_0
			SetUp ();
			o = child1.AutoSize;
			child1.AutoSize = true;
			Assert.IsFalse (child1.IsHandleCreated, "A8");
#endif

			// A - Form	
			SetUp ();
			o = child1.AcceptButton;
			child1.AcceptButton = null;
			Assert.IsFalse (child1.IsHandleCreated, "FA1");

			SetUp ();
			o = child1.ActiveControl;
			child1.ActiveControl = null;
			Assert.IsFalse (child1.IsHandleCreated, "FA2");

			SetUp ();
			o = child1.ActiveMdiChild;
			Assert.IsFalse (child1.IsHandleCreated, "FA3");

			SetUp ();
			o = child1.AllowTransparency;
			child1.AllowTransparency = !child1.AllowTransparency;
			Assert.IsFalse (child1.IsHandleCreated, "FA4");

#if NET_2_0
			SetUp ();
			o = child1.AutoScaleDimensions;
			child1.AutoScaleDimensions = SizeF.Empty;
			Assert.IsFalse (child1.IsHandleCreated, "FA5");

			SetUp ();
			o = child1.AutoScaleMode;
			child1.AutoScaleMode = AutoScaleMode.Dpi;
			Assert.IsFalse (child1.IsHandleCreated, "FA6");
#endif
			SetUp ();
			o = child1.AutoScroll;
			child1.AutoScroll = !child1.AutoScroll;
			Assert.IsFalse (child1.IsHandleCreated, "FA7");

			SetUp ();
			o = child1.AutoScrollMargin;
			child1.AutoScrollMargin = new Size (child1.AutoScrollMargin.Width + 1, child1.AutoScrollMargin.Height + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FA8");

			SetUp ();
			o = child1.AutoScrollMinSize;
			child1.AutoScrollMinSize = new Size (child1.AutoScrollMinSize.Width + 1, child1.AutoScrollMinSize.Height + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FA9");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			o = child1.AutoScrollOffset;
			child1.AutoScrollOffset = new Point (child1.AutoScrollOffset.X + 1, child1.AutoScrollOffset.Y + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FA10"); 
#endif

			SetUp ();
			o = child1.AutoScrollPosition;
			child1.AutoScrollPosition = new Point (child1.AutoScrollPosition.X + 1, child1.AutoScrollPosition.Y + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FA11");
#if NET_2_0

			SetUp ();
			o = child1.AutoSize;
			child1.AutoSize = !child1.AutoSize;
			Assert.IsFalse (child1.IsHandleCreated, "FA12");
#if !__MonoCS__
			SetUp ();
			o = child1.AutoSizeMode;
			child1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Assert.IsFalse (child1.IsHandleCreated, "FA13");
#endif
			SetUp ();
			o = child1.AutoValidate;
			child1.AutoValidate = AutoValidate.EnableAllowFocusChange;
			Assert.IsFalse (child1.IsHandleCreated, "FA14");

#endif
			// B
			SetUp ();
			o = child1.BackColor;
			child1.BackColor = Color.Green;
			Assert.IsFalse (child1.IsHandleCreated, "A9");

			SetUp ();
			o = child1.BackgroundImage;
			child1.BackgroundImage = new Bitmap (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "A10");
#if NET_2_0
			SetUp ();
			o = child1.BackgroundImageLayout;
			child1.BackgroundImageLayout = ImageLayout.Stretch;
			Assert.IsFalse (child1.IsHandleCreated, "A11");
#endif
			SetUp ();
			o = child1.BindingContext;
			child1.BindingContext = new BindingContext ();
			Assert.IsFalse (child1.IsHandleCreated, "A12");

			SetUp ();
			o = child1.Bottom;
			Assert.IsFalse (child1.IsHandleCreated, "A13");

			SetUp ();
			o = child1.Bounds;
			child1.Bounds = new Rectangle (0, 0, 12, 12);
			Assert.IsFalse (child1.IsHandleCreated, "A14");


			// B - Form
			SetUp ();
			o = child1.BindingContext;
			child1.BindingContext = null;
			Assert.IsFalse (child1.IsHandleCreated, "FB1");

			// C
			SetUp ();
			o = child1.CanFocus;
			Assert.IsFalse (child1.IsHandleCreated, "A15");

			SetUp ();
			o = child1.CanSelect;
			Assert.IsFalse (child1.IsHandleCreated, "A16");

			SetUp ();
			o = child1.Capture;
			Assert.IsFalse (child1.IsHandleCreated, "A17a");

			SetUp ();
			child1.Capture = true;
			Assert.IsTrue (child1.IsHandleCreated, "A17b");

			SetUp ();
			o = child1.CausesValidation;
			child1.CausesValidation = false;
			Assert.IsFalse (child1.IsHandleCreated, "A18");

			SetUp ();
			o = child1.ClientRectangle;
			Assert.IsFalse (child1.IsHandleCreated, "A19");

			SetUp ();
			o = child1.ClientSize;
			child1.ClientSize = new Size (30, 30);
			Assert.IsFalse (child1.IsHandleCreated, "A20");

			SetUp ();
			o = child1.CompanyName;
			Assert.IsFalse (child1.IsHandleCreated, "A21");

			SetUp ();
			o = child1.Container;
			Assert.IsFalse (child1.IsHandleCreated, "A22");

			SetUp ();
			o = child1.ContainsFocus;
			Assert.IsFalse (child1.IsHandleCreated, "A23");

			SetUp ();
			o = child1.ContextMenu;
			child1.ContextMenu = new ContextMenu ();
			Assert.IsFalse (child1.IsHandleCreated, "A24");

#if NET_2_0
			SetUp ();
			o = child1.ContextMenuStrip;
			child1.ContextMenuStrip = new ContextMenuStrip ();
			Assert.IsFalse (child1.IsHandleCreated, "A25");
#endif
			SetUp ();
			o = child1.Controls;
			Assert.IsFalse (child1.IsHandleCreated, "A26");

			SetUp ();
			o = child1.Created;
			Assert.IsFalse (child1.IsHandleCreated, "A27");

			SetUp ();
			o = child1.Cursor;
			child1.Cursor = Cursors.Arrow;
			Assert.IsFalse (child1.IsHandleCreated, "A28");

			// C - Form
			SetUp ();
			o = child1.CancelButton;
			child1.CancelButton = null;
			Assert.IsFalse (child1.IsHandleCreated, "FC1");

			SetUp ();
			o = child1.ClientSize;
			child1.ClientSize = new Size (child1.ClientSize.Width + 1, child1.ClientSize.Height + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FC2");

			SetUp ();
			o = child1.Container;
			Assert.IsFalse (child1.IsHandleCreated, "FC3");

			SetUp ();
			o = child1.ControlBox;
			child1.ControlBox = !child1.ControlBox;
			Assert.IsFalse (child1.IsHandleCreated, "FC4");
#if NET_2_0

			SetUp ();
			o = child1.CurrentAutoScaleDimensions;
			Assert.IsFalse (child1.IsHandleCreated, "FC5"); 
#endif

			// D
			SetUp ();
			o = child1.DataBindings;
			Assert.IsFalse (child1.IsHandleCreated, "A29");

			SetUp ();
			o = child1.DisplayRectangle;
			Assert.IsFalse (child1.IsHandleCreated, "A30");

			SetUp ();
			o = child1.Disposing;
			Assert.IsFalse (child1.IsHandleCreated, "A31");

			SetUp ();
			o = child1.Dock;
			child1.Dock = DockStyle.Fill;
			Assert.IsFalse (child1.IsHandleCreated, "A32");

			// D - Form
			SetUp ();
			o = child1.DataBindings;
			Assert.IsFalse (child1.IsHandleCreated, "FD6");

			SetUp ();
			o = child1.DesktopBounds;
			child1.DesktopBounds = new Rectangle (3, 5, child1.DesktopBounds.Width + 1, child1.DesktopBounds.Height + 1);
			Assert.IsFalse (child1.IsHandleCreated, "FD7");

			SetUp ();
			o = child1.DesktopLocation;
			child1.DesktopLocation = child1.DesktopLocation + new Size (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "FD8");

			SetUp ();
			o = child1.DialogResult;
			child1.DialogResult = DialogResult.Abort;
			Assert.IsFalse (child1.IsHandleCreated, "FD9");

			SetUp ();
			o = child1.DisplayRectangle;
			Assert.IsFalse (child1.IsHandleCreated, "FD10");

			SetUp ();
			o = child1.Disposing;
			Assert.IsFalse (child1.IsHandleCreated, "FD11");

			SetUp ();
			o = child1.Dock;
			child1.Dock = DockStyle.Right;
			Assert.IsFalse (child1.IsHandleCreated, "FD12");

			// E-H
			SetUp ();
			o = child1.Enabled;
			child1.Enabled = false;
			Assert.IsFalse (child1.IsHandleCreated, "A33");

			SetUp ();
			o = child1.Focused;
			Assert.IsFalse (child1.IsHandleCreated, "A34");

			SetUp ();
			o = child1.Font;
			child1.Font = new Font (child1.Font, FontStyle.Bold);
			Assert.IsFalse (child1.IsHandleCreated, "A35");

			SetUp ();
			o = child1.ForeColor;
			child1.ForeColor = Color.Green;
			Assert.IsFalse (child1.IsHandleCreated, "A36");

			SetUp ();
			o = child1.Handle;
			Assert.IsTrue (child1.IsHandleCreated, "A37");

			SetUp ();
			o = child1.HasChildren;
			Assert.IsFalse (child1.IsHandleCreated, "A38");

			SetUp ();
			o = child1.Height;
			child1.Height = 12;
			Assert.IsFalse (child1.IsHandleCreated, "A39");

			// E-H - Form
			SetUp ();
			o = child1.FormBorderStyle;
			child1.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Assert.IsFalse (child1.IsHandleCreated, "FF1");

			SetUp ();
			o = child1.HelpButton;
			child1.HelpButton = !child1.HelpButton;
			Assert.IsFalse (child1.IsHandleCreated, "FH1");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			o = child1.HorizontalScroll;
			Assert.IsFalse (child1.IsHandleCreated, "FH2"); 
#endif
			// I - L
			SetUp ();
			child1.ImeMode = ImeMode.On;
			Assert.IsFalse (child1.IsHandleCreated, "A40");

			SetUp ();
			o = child1.InvokeRequired;
			Assert.IsFalse (child1.IsHandleCreated, "A41");

			SetUp ();
			o = child1.IsAccessible;
			Assert.IsFalse (child1.IsHandleCreated, "A42");

			SetUp ();
			o = child1.IsDisposed;
			Assert.IsFalse (child1.IsHandleCreated, "A43");
#if !__MonoCS__ && NET_2_0
			SetUp ();
			o = child1.IsMirrored;
			Assert.IsFalse (child1.IsHandleCreated, "A44");
#endif
#if NET_2_0
			SetUp ();
			o = child1.LayoutEngine;
			Assert.IsFalse (child1.IsHandleCreated, "A45");
#endif
			SetUp ();
			o = child1.Left;
			child1.Left = 15;
			Assert.IsFalse (child1.IsHandleCreated, "A46");

			SetUp ();
			o = child1.Location;
			child1.Location = new Point (20, 20);
			Assert.IsFalse (child1.IsHandleCreated, "A47");

			// I - L - Form

			SetUp ();
			o = child1.Icon;
			child1.Icon = null;
			Assert.IsFalse (child1.IsHandleCreated, "FI1");

			SetUp ();
			o = child1.IsMdiChild;
			Assert.IsFalse (child1.IsHandleCreated, "FI2");

			SetUp ();
			o = child1.IsMdiContainer;
			child1.IsMdiContainer = false;
			Assert.IsFalse (child1.IsHandleCreated, "FI3");

			SetUp ();
			o = child1.IsRestrictedWindow;
			Assert.IsFalse (child1.IsHandleCreated, "FI4");

			SetUp ();
			o = child1.KeyPreview;
			child1.KeyPreview = !child1.KeyPreview;
			Assert.IsFalse (child1.IsHandleCreated, "FK1");

			SetUp ();
			o = child1.Location;
			child1.Location = child1.Location + new Size (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "FL1");
			
			
			// M - N
#if NET_2_0
			SetUp ();
			o = child1.Margin;
			child1.Margin = new Padding (6);
			Assert.IsFalse (child1.IsHandleCreated, "A48");

			SetUp ();
			o = child1.MaximumSize;
			child1.MaximumSize = new Size (500, 500);
			Assert.IsFalse (child1.IsHandleCreated, "A49");

			SetUp ();
			o = child1.MinimumSize;
			child1.MinimumSize = new Size (100, 100);
			Assert.IsFalse (child1.IsHandleCreated, "A50");
#endif
			SetUp ();
			o = child1.Name;
			child1.Name = "web";
			Assert.IsFalse (child1.IsHandleCreated, "A51");

#if NET_2_0
			// M - O - Form
			SetUp ();
			o = child1.MainMenuStrip;
			child1.MainMenuStrip = null;
			Assert.IsFalse (child1.IsHandleCreated, "FM1"); 
#endif

			SetUp ();
			o = child1.MaximizeBox;
			child1.MaximizeBox = !child1.MaximizeBox;
			Assert.IsFalse (child1.IsHandleCreated, "FM2");

			SetUp ();
			o = child1.MaximumSize;
			child1.MaximumSize = child1.MaximumSize + new Size (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "FM3");

			SetUp ();
			o = child1.MdiChildren;
			Assert.IsFalse (child1.IsHandleCreated, "FM4");

			SetUp ();
			o = child1.MdiParent;
			child1.MdiParent = null;
			Assert.IsFalse (child1.IsHandleCreated, "FM5");

			SetUp ();
			o = child1.Menu;
			child1.Menu = null;
			Assert.IsFalse (child1.IsHandleCreated, "FM6");

			SetUp ();
			o = child1.MergedMenu;
			Assert.IsFalse (child1.IsHandleCreated, "FM7");

			SetUp ();
			o = child1.MinimizeBox;
			child1.MinimizeBox = !child1.MinimizeBox;
			Assert.IsFalse (child1.IsHandleCreated, "FM8");

			SetUp ();
			o = child1.MinimumSize;
			child1.MinimumSize = child1.MinimumSize + new Size (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "FM9");

			SetUp ();
			o = child1.Modal;
			Assert.IsFalse (child1.IsHandleCreated, "FM10");

			SetUp ();
			o = child1.Opacity;
			child1.Opacity = 0.9;
			Assert.IsFalse (child1.IsHandleCreated, "FO1");

			SetUp ();
			o = child1.OwnedForms;
			Assert.IsFalse (child1.IsHandleCreated, "FO2");

			SetUp ();
			o = child1.Owner;
			child1.Owner = null;
			Assert.IsFalse (child1.IsHandleCreated, "FO3");
			
			// P - R
#if NET_2_0
			SetUp ();
			o = child1.Padding;
			child1.Padding = new Padding (4);
			Assert.IsFalse (child1.IsHandleCreated, "A52");
#endif
			try {
				SetUp ();
				o = child1.Parent;
				child1.TopLevel = false;
				child1.Parent = new Form ();
				Assert.Fail ("A53 - Expected ArgumentException, got no exception");
			} catch (ArgumentException ex) {
				TestHelper.RemoveWarning (ex);
			} catch (Exception ex) {
				Assert.Fail ("A53 - Expected ArgumentException, got " + ex.GetType ().Name);
			} finally {
				Assert.IsFalse (child1.IsHandleCreated, "A53");
			}
#if NET_2_0
			SetUp ();
			o = child1.PreferredSize;
			Assert.IsFalse (child1.IsHandleCreated, "A54");
#endif
			SetUp ();
			o = child1.ProductName;
			Assert.IsFalse (child1.IsHandleCreated, "A55");

			SetUp ();
			o = child1.ProductVersion;
			Assert.IsFalse (child1.IsHandleCreated, "A56");

			SetUp ();
			o = child1.RecreatingHandle;
			Assert.IsFalse (child1.IsHandleCreated, "A57");

			SetUp ();
			o = child1.Region;
			child1.Region = new Region (new Rectangle (0, 0, 177, 177));
			Assert.IsFalse (child1.IsHandleCreated, "A58");

			SetUp ();
			o = child1.Right;
			Assert.IsFalse (child1.IsHandleCreated, "A59");

			SetUp ();
			o = child1.RightToLeft;
			child1.RightToLeft = RightToLeft.Yes;
			Assert.IsFalse (child1.IsHandleCreated, "A60");

			// P - R - Form
			SetUp ();
			o = child1.ParentForm;
			Assert.IsFalse (child1.IsHandleCreated, "FP1");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			o = child1.RestoreBounds;
			Assert.IsFalse (child1.IsHandleCreated, "FR1"); 
#endif


			// S - W
			SetUp ();
			o = child1.Site;
			Assert.IsFalse (child1.IsHandleCreated, "A61");

			SetUp ();
			o = child1.Size;
			child1.Size = new Size (188, 188);
			Assert.IsFalse (child1.IsHandleCreated, "A62");

			SetUp ();
			o = child1.TabIndex;
			child1.TabIndex = 5;
			Assert.IsFalse (child1.IsHandleCreated, "A63");

			SetUp ();
			o = child1.Tag;
			child1.Tag = "moooooooo";
			Assert.IsFalse (child1.IsHandleCreated, "A64");

			SetUp ();
			o = child1.Text;
			child1.Text = "meoooowww";
			Assert.IsFalse (child1.IsHandleCreated, "A65");

			SetUp ();
			o = child1.Top;
			child1.Top = 16;
			Assert.IsFalse (child1.IsHandleCreated, "A66");

			SetUp ();
			o = child1.TopLevelControl;
			Assert.IsFalse (child1.IsHandleCreated, "A67");
#if !__MonoCS__ && NET_2_0
			SetUp ();
			o = child1.UseWaitCursor;
			child1.UseWaitCursor = true;
			Assert.IsFalse (child1.IsHandleCreated, "A68");
#endif
			SetUp ();
			o = child1.Visible;
			Assert.IsFalse (child1.IsHandleCreated, "A69-a");
			
			SetUp ();
			child1.Visible = true;
			Assert.IsTrue (child1.IsHandleCreated, "A69-b");
			
			SetUp ();
			o = child1.Width;
			child1.Width = 190;
			Assert.IsFalse (child1.IsHandleCreated, "A70");

			SetUp ();
			o = child1.WindowTarget;
			Assert.IsFalse (child1.IsHandleCreated, "A71");

			// S - W - Form

#if NET_2_0
			SetUp ();
			o = child1.ShowIcon;
			child1.ShowIcon = !child1.ShowIcon;
			Assert.IsFalse (child1.IsHandleCreated, "FS1"); 
#endif

			SetUp ();
			o = child1.ShowInTaskbar;
			child1.ShowInTaskbar = !child1.ShowInTaskbar;
			Assert.IsFalse (child1.IsHandleCreated, "FS2");

			SetUp ();
			o = child1.Size;
			child1.Size = child1.Size + new Size (1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "FS3");

			SetUp ();
			o = child1.SizeGripStyle;
			child1.SizeGripStyle = SizeGripStyle.Show;
			Assert.IsFalse (child1.IsHandleCreated, "FS4");

			SetUp ();
			o = child1.StartPosition;
			child1.StartPosition = FormStartPosition.Manual;
			Assert.IsFalse (child1.IsHandleCreated, "FS5");

			SetUp ();
			o = child1.Text;
			child1.Text = "hooray!";
			Assert.IsFalse (child1.IsHandleCreated, "FT1");

			try {
				SetUp ();
				o = child1.TopLevel;
				child1.TopLevel = !child1.TopLevel;
				Assert.Fail ("FT2 - expected ArgumentException, got no exception.");
			} catch (ArgumentException ex) {
				TestHelper.RemoveWarning (ex);
			} catch (Exception ex) {
				Assert.Fail ("FT2 - expected ArgumentException, got " + ex.GetType ().Name);
			} finally {
				Assert.IsFalse (child1.IsHandleCreated, "FT2");
			}

			SetUp ();
			o = child1.TopMost;
			child1.TopMost = !child1.TopMost;
			Assert.IsFalse (child1.IsHandleCreated, "FT3");

			SetUp ();
			o = child1.TransparencyKey;
			child1.TransparencyKey = Color.BurlyWood;
			Assert.IsFalse (child1.IsHandleCreated, "FT4");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			o = child1.VerticalScroll;
			Assert.IsFalse (child1.IsHandleCreated, "FV1"); 
#endif

			SetUp ();
			o = child1.WindowState;
			child1.WindowState = FormWindowState.Maximized;
			Assert.IsFalse (child1.IsHandleCreated, "FW1");

			TestHelper.RemoveWarning (o);
		}

		[Test]
		public void TestProtectedProperties ()
		{
			// Not a surprise, but none of these cause handle creation.
			// Included just to absolutely certain.
			object o;
#if !__MonoCS__ && NET_2_0
			SetUp ();
			o = child1.PublicCanRaiseEvents;
			Assert.IsFalse (child1.IsHandleCreated, "A1");
#endif
			SetUp ();
			o = child1.PublicCreateParams;
			Assert.IsFalse (child1.IsHandleCreated, "A2");
#if NET_2_0
			SetUp ();
			o = child1.PublicDefaultCursor;
			Assert.IsFalse (child1.IsHandleCreated, "A3");
#endif
			SetUp ();
			o = child1.PublicDefaultImeMode;
			Assert.IsFalse (child1.IsHandleCreated, "A4");
#if NET_2_0
			SetUp ();
			o = child1.PublicDefaultMargin;
			Assert.IsFalse (child1.IsHandleCreated, "A5");
			
			SetUp ();
			o = child1.PublicDefaultMaximumSize;
			Assert.IsFalse (child1.IsHandleCreated, "A6");

			SetUp ();
			o = child1.PublicDefaultMinimumSize;
			Assert.IsFalse (child1.IsHandleCreated, "A7");

			SetUp ();
			o = child1.PublicDefaultPadding;
			Assert.IsFalse (child1.IsHandleCreated, "A8");

			SetUp ();
			o = child1.PublicDefaultSize;
			Assert.IsFalse (child1.IsHandleCreated, "A9");

			SetUp ();
			o = child1.PublicDoubleBuffered;
			child1.PublicDoubleBuffered = !child1.PublicDoubleBuffered;
			Assert.IsFalse (child1.IsHandleCreated, "A10");
#endif
			SetUp ();
			o = child1.PublicFontHeight;
			child1.PublicFontHeight = child1.PublicFontHeight + 1;
			Assert.IsFalse (child1.IsHandleCreated, "A11");

			SetUp ();
			o = child1.PublicRenderRightToLeft;
			Assert.IsFalse (child1.IsHandleCreated, "A12");

			SetUp ();
			o = child1.PublicResizeRedraw;
			child1.PublicResizeRedraw = !child1.PublicResizeRedraw;
			Assert.IsFalse (child1.IsHandleCreated, "A13");
#if !__MonoCS__ && NET_2_0
			SetUp ();
			o = child1.PublicScaleChildren;
			Assert.IsFalse (child1.IsHandleCreated, "A14");
#endif
			SetUp ();
			o = child1.PublicShowFocusCues;
			Assert.IsFalse (child1.IsHandleCreated, "A15");

			SetUp ();
			o = child1.PublicShowKeyboardCues;
			Assert.IsFalse (child1.IsHandleCreated, "A16");

#if NET_2_0
			SetUp ();
			o = child1.PublicAutoScaleFactor;
			Assert.IsFalse (child1.IsHandleCreated, "F1"); 
#endif
			SetUp ();
			o = child1.PublicDesignMode;
			Assert.IsFalse (child1.IsHandleCreated, "F2");

			SetUp ();
			o = child1.PublicEvents;
			Assert.IsFalse (child1.IsHandleCreated, "F3");

			SetUp ();
			o = child1.PublicHScroll;
			child1.PublicHScroll = !child1.PublicHScroll;
			Assert.IsFalse (child1.IsHandleCreated, "F4");

			SetUp ();
			o = child1.PublicMaximizedBounds;
			child1.PublicMaximizedBounds = new Rectangle (1, 1, 1, 1);
			Assert.IsFalse (child1.IsHandleCreated, "F5");

#if NET_2_0
			SetUp ();
			o = child1.PublicShowWithoutActivation;
			Assert.IsFalse (child1.IsHandleCreated, "F6"); 
#endif
			SetUp ();
			o = child1.PublicVScroll;
			child1.PublicVScroll = !child1.PublicVScroll;
			Assert.IsFalse (child1.IsHandleCreated, "F7");
			

			TestHelper.RemoveWarning (o);
		}

		[Test]
		public void TestPublicMethods ()
		{
			// Public Methods that force Handle creation:
			// - CreateGraphics ()
			// - Focus ()
			// - GetChildAtPoint ()
			// - PointToClient ()
			// - PointToScreen ()
			// - RectangleToClient ()
			// - RectangleToScreen ()
			// - Select ()
			
			SetUp ();
			child1.BringToFront ();
			Assert.IsFalse (child1.IsHandleCreated, "A1");

			SetUp ();
			child1.Contains (new Form ());
			Assert.IsFalse (child1.IsHandleCreated, "A2");

			SetUp ();
			child1.CreateControl ();
			Assert.IsFalse (child1.IsHandleCreated, "A3");

			SetUp ();
			Graphics g = child1.CreateGraphics ();
			Assert.IsTrue (child1.IsHandleCreated, "A4");
			g.Dispose ();

			SetUp ();
			child1.Dispose ();
			Assert.IsFalse (child1.IsHandleCreated, "A5");

			// This is weird, it causes a form to appear that won't go away until you move the mouse over it, 
			// but it doesn't create a handle??
			//DragDropEffects d = c.DoDragDrop ("yo", DragDropEffects.None);
			//Assert.IsFalse (c.IsHandleCreated, "A6");
			//Assert.AreEqual (DragDropEffects.None, d, "A6b");
			
			//Bitmap b = new Bitmap (100, 100);
			//c.DrawToBitmap (b, new Rectangle (0, 0, 100, 100));
			//Assert.IsFalse (c.IsHandleCreated, "A7");
			//b.Dispose ();
			SetUp ();
			child1.FindForm ();
			Assert.IsFalse (child1.IsHandleCreated, "A8");

			SetUp ();
			child1.Focus ();
			Assert.IsTrue (child1.IsHandleCreated, "A9");

			SetUp ();
			child1.GetChildAtPoint (new Point (10, 10));
			Assert.IsTrue (child1.IsHandleCreated, "A10");

			SetUp ();
			child1.GetContainerControl ();
			Assert.IsFalse (child1.IsHandleCreated, "A11");
			
			SetUp ();
			child1.GetNextControl (new Control (), true);
			Assert.IsFalse (child1.IsHandleCreated, "A12");
#if NET_2_0
			SetUp ();
			child1.GetPreferredSize (Size.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A13");
#endif
			SetUp ();
			child1.Hide ();
			Assert.IsFalse (child1.IsHandleCreated, "A14");

			SetUp ();
			child1.Invalidate ();
			Assert.IsFalse (child1.IsHandleCreated, "A15");

			child1.Invoke (new InvokeDelegate (InvokeMethod));
			Assert.IsFalse (child1.IsHandleCreated, "A16");
			
			SetUp ();
			child1.PerformLayout ();
			Assert.IsFalse (child1.IsHandleCreated, "A17");

			SetUp ();
			child1.PointToClient (new Point (100, 100));
			Assert.IsTrue (child1.IsHandleCreated, "A18");

			SetUp ();
			child1.PointToScreen (new Point (100, 100));
			Assert.IsTrue (child1.IsHandleCreated, "A19");
			
			//c.PreProcessControlMessage   ???
			//c.PreProcessMessage          ???
			SetUp ();
			child1.RectangleToClient (new Rectangle (0, 0, 100, 100));
			Assert.IsTrue (child1.IsHandleCreated, "A20");
			
			SetUp ();
			child1.RectangleToScreen (new Rectangle (0, 0, 100, 100));
			Assert.IsTrue (child1.IsHandleCreated, "A21");

			SetUp ();
			child1.Refresh ();
			Assert.IsFalse (child1.IsHandleCreated, "A22");

			SetUp ();
			child1.ResetBackColor ();
			Assert.IsFalse (child1.IsHandleCreated, "A23");

			SetUp ();
			child1.ResetBindings ();
			Assert.IsFalse (child1.IsHandleCreated, "A24");

			SetUp ();
			child1.ResetCursor ();
			Assert.IsFalse (child1.IsHandleCreated, "A25");

			SetUp ();
			child1.ResetFont ();
			Assert.IsFalse (child1.IsHandleCreated, "A26");

			SetUp ();
			child1.ResetForeColor ();
			Assert.IsFalse (child1.IsHandleCreated, "A27");

			SetUp ();
			child1.ResetImeMode ();
			Assert.IsFalse (child1.IsHandleCreated, "A28");

			SetUp ();
			child1.ResetRightToLeft ();
			Assert.IsFalse (child1.IsHandleCreated, "A29");

			SetUp ();
			child1.ResetText ();
			Assert.IsFalse (child1.IsHandleCreated, "A30");

			SetUp ();
			child1.SuspendLayout ();
			Assert.IsFalse (child1.IsHandleCreated, "A31");

			SetUp ();
			child1.ResumeLayout ();
			Assert.IsFalse (child1.IsHandleCreated, "A32");
			
#if NET_2_0
			SetUp ();
			child1.Scale (new SizeF (1.5f, 1.5f));
			Assert.IsFalse (child1.IsHandleCreated, "A33");
#endif
			SetUp ();
			child1.Select ();
			Assert.IsTrue (child1.IsHandleCreated, "A34");

			SetUp ();
			child1.SelectNextControl (new Control (), true, true, true, true);
			Assert.IsFalse (child1.IsHandleCreated, "A35");

			SetUp ();
			child1.SetBounds (0, 0, 100, 100);
			Assert.IsFalse (child1.IsHandleCreated, "A36");

			SetUp ();
			child1.Update ();
			Assert.IsFalse (child1.IsHandleCreated, "A37");

			// Form

			SetUp ();
			child1.Activate ();
			Assert.IsFalse (child1.IsHandleCreated, "F1");

			SetUp ();
			child1.AddOwnedForm (new Form ());
			Assert.IsFalse (child1.IsHandleCreated, "F2");

			SetUp ();
			child1.Close ();
			Assert.IsFalse (child1.IsHandleCreated, "F3");

			SetUp ();
			child1.Hide ();
			Assert.IsFalse (child1.IsHandleCreated, "F4");

			SetUp ();
			child1.LayoutMdi (MdiLayout.Cascade);
			Assert.IsFalse (child1.IsHandleCreated, "F5");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			child1.PerformAutoScale ();
			Assert.IsFalse (child1.IsHandleCreated, "F6");
#endif

			SetUp ();
			child1.PerformLayout ();
			Assert.IsFalse (child1.IsHandleCreated, "F7");

			SetUp ();
			child1.AddOwnedForm (new Form ());
			child1.RemoveOwnedForm (child1.OwnedForms [child1.OwnedForms.Length - 1]);
			Assert.IsFalse (child1.IsHandleCreated, "F8");

			SetUp ();
			child1.ScrollControlIntoView (null);
			Assert.IsFalse (child1.IsHandleCreated, "F9");

			SetUp ();
			child1.SetAutoScrollMargin (7, 13);
			Assert.IsFalse (child1.IsHandleCreated, "F10");

			SetUp ();
			child1.SetDesktopBounds (-1, -1, 144, 169);
			Assert.IsFalse (child1.IsHandleCreated, "F11");

			SetUp ();
			child1.SetDesktopLocation (7, 13);
			Assert.IsFalse (child1.IsHandleCreated, "F12");

#if NET_2_0
			try {
				SetUp ();
				child1.Show (null);
				Assert.Fail ("F13 - expected InvalidOperationException, got no exception.");
			} catch (InvalidOperationException ex) {
				TestHelper.RemoveWarning (ex);
			} catch (Exception ex) {
				Assert.Fail ("F13 - expected InvalidOperationException, got " + ex.GetType ().Name);
			} finally {
				Assert.IsFalse (child1.IsHandleCreated, "F13");
			}
#endif
			
			//c.ShowDialog ()

			SetUp ();
			child1.ToString ();
			Assert.IsFalse (child1.IsHandleCreated, "F14");

			SetUp ();
			child1.Validate ();
			Assert.IsFalse (child1.IsHandleCreated, "F15");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			child1.ValidateChildren ();
			Assert.IsFalse (child1.IsHandleCreated, "F16"); 
#endif
		}

		[Test]
		public void Show ()
		{
			SetUp ();
			Assert.IsFalse (child1.IsHandleCreated, "A1");
			child1.HandleCreated += new EventHandler (HandleCreated_WriteStackTrace);
			child1.Show ();
			Assert.IsTrue (child1.IsHandleCreated, "A2");
		}

		void HandleCreated_WriteStackTrace (object sender, EventArgs e)
		{
			Console.WriteLine ("Stacktrace?");//Environment.StackTrace);
		}

		public delegate void InvokeDelegate ();
		public void InvokeMethod () { invokeform.Text = "methodinvoked"; }

		Form invokeform = new Form ();

		[Test]
		public void TestProtectedMethods ()
		{
			// Protected Methods that force Handle creation:
			// - CreateAccessibilityInstance ()
			// - CreateHandle ()
			// - IsInputChar ()
			// - Select ()
			// - SetVisibleCore ()
			
			SetUp ();
			child1.PublicAccessibilityNotifyClients (AccessibleEvents.Focus, 0);
#if NET_2_0
			Assert.IsFalse (child1.IsHandleCreated, "A1");
#else
			Assert.IsTrue (child1.IsHandleCreated, "A1");
#endif
			child1.PublicCreateAccessibilityInstance ();
			Assert.IsTrue (child1.IsHandleCreated, "A2");

			SetUp ();
			child1.PublicCreateControlsInstance ();
			Assert.IsFalse (child1.IsHandleCreated, "A3");

			SetUp ();
			child1.PublicCreateHandle ();
			Assert.IsTrue (child1.IsHandleCreated, "A4");

			SetUp ();
			child1.PublicDestroyHandle ();
			Assert.IsFalse (child1.IsHandleCreated, "A5");

#if NET_2_0
			SetUp ();
			child1.PublicGetAccessibilityObjectById (0);
			Assert.IsFalse (child1.IsHandleCreated, "A6");
#endif
#if !__MonoCS__ && NET_2_0
			SetUp ();
			child1.PublicGetAutoSizeMode ();
			Assert.IsFalse (child1.IsHandleCreated, "A7");

			SetUp ();
			child1.PublicGetScaledBounds (new Rectangle (0, 0, 100, 100), new SizeF (1.5f, 1.5f), BoundsSpecified.All);
			Assert.IsFalse (child1.IsHandleCreated, "A8");
#endif
			SetUp ();
			child1.PublicGetStyle (ControlStyles.FixedHeight);
			Assert.IsFalse (child1.IsHandleCreated, "A9");

			SetUp ();
			child1.PublicGetTopLevel ();
			Assert.IsFalse (child1.IsHandleCreated, "A10");

			SetUp ();
			child1.PublicInitLayout ();
			Assert.IsFalse (child1.IsHandleCreated, "A11");

			SetUp ();
			child1.PublicInvokeGotFocus (child1, EventArgs.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A12");

			SetUp ();
			child1.PublicInvokeLostFocus (child1, EventArgs.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A13");

			SetUp ();
			child1.PublicInvokeOnClick (child1, EventArgs.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A14");

			SetUp ();
			child1.PublicInvokePaint (child1, new PaintEventArgs (Graphics.FromImage (new Bitmap (1, 1)), Rectangle.Empty));
			Assert.IsFalse (child1.IsHandleCreated, "A15");

			SetUp ();
			child1.PublicInvokePaintBackground (child1, new PaintEventArgs (Graphics.FromImage (new Bitmap (1, 1)), Rectangle.Empty));
			Assert.IsFalse (child1.IsHandleCreated, "A16");

			SetUp ();
			child1.PublicIsInputChar ('c');
			Assert.IsTrue (child1.IsHandleCreated, "A17");

			SetUp ();
			child1.PublicIsInputKey (Keys.B);
			Assert.IsFalse (child1.IsHandleCreated, "A18");

			SetUp ();
			child1.PublicNotifyInvalidate (Rectangle.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A19");

			SetUp ();
			child1.PublicOnVisibleChanged (EventArgs.Empty);
			Assert.IsFalse (child1.IsHandleCreated, "A20");

			SetUp ();
			child1.PublicRaiseDragEvent (null, null);
			Assert.IsFalse (child1.IsHandleCreated, "A21");

			SetUp ();
			child1.PublicRaiseKeyEvent (null, null);
			Assert.IsFalse (child1.IsHandleCreated, "A22");

			SetUp ();
			child1.PublicRaiseMouseEvent (null, null);
			Assert.IsFalse (child1.IsHandleCreated, "A23");

			SetUp ();
			child1.PublicRaisePaintEvent (null, null);
			Assert.IsFalse (child1.IsHandleCreated, "A24");

			SetUp ();
			child1.PublicRecreateHandle ();
			Assert.IsFalse (child1.IsHandleCreated, "A25");

			SetUp ();
			child1.PublicResetMouseEventArgs ();
			Assert.IsFalse (child1.IsHandleCreated, "A26");

			SetUp ();
			child1.PublicRtlTranslateAlignment (ContentAlignment.BottomLeft);
			Assert.IsFalse (child1.IsHandleCreated, "A27");

			SetUp ();
			child1.PublicRtlTranslateContent (ContentAlignment.BottomLeft);
			Assert.IsFalse (child1.IsHandleCreated, "A28");

			SetUp ();
			child1.PublicRtlTranslateHorizontal (HorizontalAlignment.Left);
			Assert.IsFalse (child1.IsHandleCreated, "A29");

			SetUp ();
			child1.PublicRtlTranslateLeftRight (LeftRightAlignment.Left);
			Assert.IsFalse (child1.IsHandleCreated, "A30");
#if !__MonoCS__ && NET_2_0
			SetUp ();
			child1.PublicScaleControl (new SizeF (1.5f, 1.5f), BoundsSpecified.All);
			Assert.IsFalse (child1.IsHandleCreated, "A31");
#endif
			SetUp ();
			child1.PublicScaleCore (1.5f, 1.5f);
			Assert.IsFalse (child1.IsHandleCreated, "A32");

			SetUp ();
			child1.PublicSelect ();
			Assert.IsTrue (child1.IsHandleCreated, "A33");
			
#if !__MonoCS__ && NET_2_0
			SetUp ();
			child1.PublicSetAutoSizeMode (AutoSizeMode.GrowAndShrink);
			Assert.IsFalse (child1.IsHandleCreated, "A34");
#endif
			SetUp ();
			child1.PublicSetBoundsCore (0, 0, 100, 100, BoundsSpecified.All);
			Assert.IsFalse (child1.IsHandleCreated, "A35");

			SetUp ();
			child1.PublicSetClientSizeCore (122, 122);
			Assert.IsFalse (child1.IsHandleCreated, "A36");

			SetUp ();
			child1.PublicSetStyle (ControlStyles.FixedHeight, true);
			Assert.IsFalse (child1.IsHandleCreated, "A37");

			try {
				SetUp ();
				child1.PublicSetTopLevel (true);
				Assert.Fail ("A38, expected ArgumentException, got no exception");
			} catch (ArgumentException ex) {
				TestHelper.RemoveWarning (ex);
			} catch (Exception ex) {
				Assert.Fail ("A38, expected ArgumentException, got " + ex.GetType ().Name);
			} finally {
				Assert.IsFalse (child1.IsHandleCreated, "A38");
			}

			SetUp ();
			child1.PublicSetVisibleCore (true);
			Assert.IsTrue (child1.IsHandleCreated, "A39");
#if NET_2_0
			SetUp ();
			child1.PublicSizeFromClientSize (new Size (160, 160));
			Assert.IsFalse (child1.IsHandleCreated, "A40");
#endif

			SetUp ();
			child1.PublicUpdateBounds ();
			Assert.IsFalse (child1.IsHandleCreated, "A41");

			SetUp ();
			child1.PublicUpdateStyles ();
			Assert.IsFalse (child1.IsHandleCreated, "A42");

			SetUp ();
			child1.PublicUpdateZOrder ();
			Assert.IsFalse (child1.IsHandleCreated, "A43");


			// Form

			SetUp ();
			main.PublicActivateMdiChild (child1);
			main.PublicActivateMdiChild (child2);
			Assert.IsFalse (child1.IsHandleCreated, "F1-a");
			Assert.IsFalse (child2.IsHandleCreated, "F1-b");

			SetUp ();
			child1.PublicAdjustFormScrollbars (true);
			Assert.IsFalse (child1.IsHandleCreated, "F2");

			SetUp ();
			child1.PublicCenterToParent ();
			Assert.IsFalse (child1.IsHandleCreated, "F3");

			SetUp ();
			child1.PublicCenterToScreen ();
			Assert.IsFalse (child1.IsHandleCreated, "F4");

			SetUp ();
			child1.PublicGetScrollState (1);
			Assert.IsFalse (child1.IsHandleCreated, "F5");

			SetUp ();
			child1.PublicGetService (typeof (int));
			Assert.IsFalse (child1.IsHandleCreated, "F6");

			SetUp ();
			Message m = new Message ();
			child1.PublicProcessCmdKey (ref m, Keys.C);
			Assert.IsFalse (child1.IsHandleCreated, "F7");

			SetUp ();
			child1.PublicProcessDialogChar ('p');
			Assert.IsFalse (child1.IsHandleCreated, "F8");

			SetUp ();
			child1.PublicProcessDialogKey (Keys.D);
			Assert.IsFalse (child1.IsHandleCreated, "F9");

			SetUp ();
			child1.PublicProcessKeyEventArgs (ref m);
			Assert.IsFalse (child1.IsHandleCreated, "F10");

			SetUp ();
			child1.PublicProcessKeyMessage (ref m);
			Assert.IsFalse (child1.IsHandleCreated, "F11");

			SetUp ();
			child1.PublicProcessKeyPreview (ref m);
			Assert.IsFalse (child1.IsHandleCreated, "F12");

			SetUp ();
			child1.PublicProcessMnemonic ('Z');
			Assert.IsFalse (child1.IsHandleCreated, "F13");

			SetUp ();
			child1.PublicProcessTabKey (true);
			Assert.IsFalse (child1.IsHandleCreated, "F14");

#if NET_2_0 && !__MonoCS__
			SetUp ();
			child1.Controls.Add (new Control ());
			child1.PublicScrollToControl (child1.Controls [0]);
			Assert.IsFalse (child1.IsHandleCreated, "F15");
#endif

			SetUp ();
			child1.PublicSelect (true, true);
			Assert.IsTrue (child1.IsHandleCreated, "F16");

			SetUp ();
			child1.PublicSetDisplayRectLocation (13, 17);
			Assert.IsFalse (child1.IsHandleCreated, "F17");

			SetUp ();
			child1.PublicSetScrollState (5, false);
			Assert.IsFalse (child1.IsHandleCreated, "F18");

			SetUp ();
			child1.PublicUpdateDefaultButton (3, false);
			Assert.IsFalse (child1.IsHandleCreated, "F19");
		}

		private class ProtectedForm : Form
		{
			// Properties
#if NET_2_0
			public SizeF PublicAutoScaleFactor { get { return base.AutoScaleFactor; } } 
#endif
#if !__MonoCS__ && NET_2_0
			public bool PublicCanRaiseEvents { get { return base.CanRaiseEvents; } }
#endif
			public CreateParams PublicCreateParams { get { return base.CreateParams; } }
#if NET_2_0
			public Cursor PublicDefaultCursor { get { return base.DefaultCursor; } }
#endif
			public ImeMode PublicDefaultImeMode { get { return base.DefaultImeMode; } }
#if NET_2_0
			public Padding PublicDefaultMargin { get { return base.DefaultMargin; } }
			public Size PublicDefaultMaximumSize { get { return base.DefaultMaximumSize; } }
			public Size PublicDefaultMinimumSize { get { return base.DefaultMinimumSize; } }
			public Padding PublicDefaultPadding { get { return base.DefaultPadding; } }
			public Size PublicDefaultSize { get { return base.DefaultSize; } }
#endif
			public bool PublicDesignMode { get {return base.DesignMode; } }
#if NET_2_0
			public bool PublicDoubleBuffered { get { return base.DoubleBuffered; } set { base.DoubleBuffered = value; } }
#endif
			public EventHandlerList PublicEvents { get {return base.Events; } }			
			public int PublicFontHeight { get { return base.FontHeight; } set { base.FontHeight = value; } }
			public bool PublicHScroll { get {return base.HScroll; } set { base.HScroll = value;} }
			public Rectangle PublicMaximizedBounds { get {return base.MaximizedBounds; } set { base.MaximizedBounds = value; }}
			public bool PublicRenderRightToLeft { get { return base.RenderRightToLeft; } }
			public bool PublicResizeRedraw { get { return base.ResizeRedraw; } set { base.ResizeRedraw = value; } }
#if !__MonoCS__ && NET_2_0
			public bool PublicScaleChildren { get { return base.ScaleChildren; } }
#endif
			public bool PublicShowFocusCues { get { return base.ShowFocusCues; } }
			public bool PublicShowKeyboardCues { get { return base.ShowKeyboardCues; } }
#if NET_2_0
			public bool PublicShowWithoutActivation { get { return base.ShowWithoutActivation; } } 
#endif
			public bool PublicVScroll { get { return base.VScroll; } set { base.VScroll = value; } }
			
			
			// Methods
			public void PublicAccessibilityNotifyClients (AccessibleEvents accEvent, int childID) { base.AccessibilityNotifyClients (accEvent, childID); }
			public void PublicActivateMdiChild (Form form) { base.ActivateMdiChild (form); }
			public void PublicAdjustFormScrollbars (bool displayScrollbars) {base.AdjustFormScrollbars (displayScrollbars); }
			public void PublicCenterToParent () { base.CenterToParent (); }
			public void PublicCenterToScreen () { base.CenterToScreen (); }
			public void PublicCreateAccessibilityInstance () { base.CreateAccessibilityInstance (); }
			public void PublicCreateControlsInstance () { base.CreateControlsInstance (); }
			public void PublicCreateHandle () { base.CreateHandle (); }
			public void PublicDestroyHandle () { base.DestroyHandle (); }
#if NET_2_0
			public AccessibleObject PublicGetAccessibilityObjectById (int objectId) { return base.GetAccessibilityObjectById (objectId); }
#endif
#if !__MonoCS__ && NET_2_0
			public AutoSizeMode PublicGetAutoSizeMode () { return base.GetAutoSizeMode (); }
			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified) { return base.GetScaledBounds (bounds, factor, specified); }
#endif
			public bool PublicGetScrollState (int bit) { return base.GetScrollState (bit); }
			public object PublicGetService (Type service) { return base.GetService (service); }
			public bool PublicGetStyle (ControlStyles flag) { return base.GetStyle (flag); }
			public bool PublicGetTopLevel () { return base.GetTopLevel (); }
			public void PublicInitLayout () { base.InitLayout (); }
			public void PublicInvokeGotFocus (Control toInvoke, EventArgs e) { base.InvokeGotFocus (toInvoke, e); }
			public void PublicInvokeLostFocus (Control toInvoke, EventArgs e) { base.InvokeLostFocus (toInvoke, e); }
			public void PublicInvokeOnClick (Control toInvoke, EventArgs e) { base.InvokeOnClick (toInvoke, e); }
			public void PublicInvokePaint (Control c, PaintEventArgs e) { base.InvokePaint (c, e); }
			public void PublicInvokePaintBackground (Control c, PaintEventArgs e) { base.InvokePaintBackground (c, e); }
			public bool PublicIsInputChar (char charCode) { return base.IsInputChar (charCode); }
			public bool PublicIsInputKey (Keys keyData) { return base.IsInputKey (keyData); }
			public void PublicNotifyInvalidate (Rectangle invalidatedArea) { base.NotifyInvalidate (invalidatedArea); }
			public void PublicOnVisibleChanged (EventArgs e) { base.OnVisibleChanged (e); }
			public void PublicProcessCmdKey (ref Message msg, Keys keyData) { base.ProcessCmdKey (ref msg, keyData); }
			public void PublicProcessDialogChar (char charCode) { base.ProcessDialogChar (charCode); }
			public void PublicProcessDialogKey (Keys keyData) { base.ProcessDialogKey (keyData); }
			public void PublicProcessKeyEventArgs (ref Message m) { base.ProcessKeyEventArgs (ref m); }
			public void PublicProcessKeyMessage (ref Message m) { base.ProcessKeyMessage (ref m); }
			public void PublicProcessKeyPreview (ref Message m) { base.ProcessKeyPreview (ref m); }
			public void PublicProcessMnemonic (char charCode) { base.ProcessMnemonic (charCode); }
			public void PublicProcessTabKey (bool forward) { base.ProcessTabKey (forward); }
			public void PublicRaiseDragEvent (Object key, DragEventArgs e) { base.RaiseDragEvent (key, e); }
			public void PublicRaiseKeyEvent (Object key, KeyEventArgs e) { base.RaiseKeyEvent (key, e); }
			public void PublicRaiseMouseEvent (Object key, MouseEventArgs e) { base.RaiseMouseEvent (key, e); }
			public void PublicRaisePaintEvent (Object key, PaintEventArgs e) { base.RaisePaintEvent (key, e); }
			public void PublicRecreateHandle () { base.RecreateHandle (); }
			public void PublicResetMouseEventArgs () { base.ResetMouseEventArgs (); }
			public ContentAlignment PublicRtlTranslateAlignment (ContentAlignment align) { return base.RtlTranslateAlignment (align); }
			public ContentAlignment PublicRtlTranslateContent (ContentAlignment align) { return base.RtlTranslateContent (align); }
			public HorizontalAlignment PublicRtlTranslateHorizontal (HorizontalAlignment align) { return base.RtlTranslateHorizontal (align); }
			public LeftRightAlignment PublicRtlTranslateLeftRight (LeftRightAlignment align) { return base.RtlTranslateLeftRight (align); }
#if !__MonoCS__ && NET_2_0
			public void PublicScaleControl (SizeF factor, BoundsSpecified specified) { base.ScaleControl (factor, specified); }
#endif
			public void PublicScaleCore (float dx, float dy) { base.ScaleCore (dx, dy); }
#if NET_2_0 && !__MonoCS__
			public void PublicScrollToControl (Control activeControl) { base.ScrollToControl (activeControl); } 
#endif
			public void PublicSelect () { base.Select (); }
			public void PublicSelect (bool directed, bool forward) { base.Select (directed, forward); }

#if !__MonoCS__ && NET_2_0
			public void PublicSetAutoSizeMode (AutoSizeMode mode) { base.SetAutoSizeMode (mode); }
#endif
			public void PublicSetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) { base.SetBoundsCore (x, y, width, height, specified); }
			public void PublicSetClientSizeCore (int x, int y) { base.SetClientSizeCore (x, y); }
			public void PublicSetDisplayRectLocation (int x, int y) { base.SetDisplayRectLocation (x, y); }
			public void PublicSetScrollState (int bit, bool value) { base.SetScrollState (bit, value); }
			public void PublicSetStyle (ControlStyles flag, bool value) { base.SetStyle (flag, value); }
			public void PublicSetTopLevel (bool value) { base.SetTopLevel (value); }
			public void PublicSetVisibleCore (bool value) { base.SetVisibleCore (value); }
#if NET_2_0
			public Size PublicSizeFromClientSize (Size clientSize) { return base.SizeFromClientSize (clientSize); }
#endif
			public void PublicUpdateBounds () { base.UpdateBounds (); }
			public void PublicUpdateDefaultButton (int bit, bool value) { base.UpdateDefaultButton (); }
			public void PublicUpdateStyles () { base.UpdateStyles (); }
			public void PublicUpdateZOrder () { base.UpdateZOrder (); }
		}
	}
}
