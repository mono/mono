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
	public class FormHandleTest : TestHelper
	{
		[Test]
		public void TestConstructors ()
		{
			Form c = new Form ();
			Assert.IsFalse (c.IsHandleCreated, "A1");
		}

		[Test]
		public void TestContextMenu ()
		{
			Form c = new Form ();
			c.ContextMenu = new ContextMenu ();
			c.ContextMenu.MenuItems.Add (new MenuItem ());
			c.ContextMenu.MenuItems [0].Text = "New";
			Assert.IsFalse (c.IsHandleCreated);
		}

		[Test] // bug #81272
		public void TestMenu ()
		{
			Form c = new Form ();
			c.Menu = new MainMenu ();
			c.Menu.MenuItems.Add (new MenuItem ());
			c.Menu.MenuItems [0].Text = "New";
			Assert.IsFalse (c.IsHandleCreated);
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
			
			Form c = new Form ();
			// A
			object o = c.AccessibilityObject;
			Assert.IsTrue (c.IsHandleCreated, "A0");
			c.Dispose ();
			
			c = new Form ();
			o = c.AccessibleDefaultActionDescription;
			c.AccessibleDefaultActionDescription = "playdoh";
			Assert.IsFalse (c.IsHandleCreated, "A1");
			o = c.AccessibleDescription;
			c.AccessibleDescription = "more playdoh!";
			Assert.IsFalse (c.IsHandleCreated, "A2");
			o = c.AccessibleName;
			c.AccessibleName = "playdoh fun factory";
			Assert.IsFalse (c.IsHandleCreated, "A3");
			o = c.AccessibleRole;
			c.AccessibleRole = AccessibleRole.Border;
			Assert.IsFalse (c.IsHandleCreated, "A4");
			o = c.AllowDrop;
			c.AllowDrop = true;
			Assert.IsFalse (c.IsHandleCreated, "A5");
			// If we don't reset the control, handle creation will fail
			// because AllowDrop requires STAThread, which Nunit doesn't do
			c.Dispose ();
			
			c = new Form ();
			o = c.Anchor;
			c.Anchor = AnchorStyles.Right;
			Assert.IsFalse (c.IsHandleCreated, "A6");
#if !MONO
			o = c.AutoScrollOffset;
			c.AutoScrollOffset = new Point (40, 40);
			Assert.IsFalse (c.IsHandleCreated, "A7");
#endif
			o = c.AutoSize;
			c.AutoSize = true;
			Assert.IsFalse (c.IsHandleCreated, "A8");

			// A - Form			
			o = c.AcceptButton;
			c.AcceptButton = null;
			Assert.IsFalse (c.IsHandleCreated, "FA1");
			
			o = c.ActiveControl;
			c.ActiveControl = null;
			Assert.IsFalse (c.IsHandleCreated, "FA2");
			
			o = c.ActiveMdiChild;
			Assert.IsFalse (c.IsHandleCreated, "FA3");
			
			o = c.AllowTransparency;
			c.AllowTransparency = !c.AllowTransparency;
			Assert.IsFalse (c.IsHandleCreated, "FA4");

			o = c.AutoScaleDimensions;
			c.AutoScaleDimensions = SizeF.Empty;
			Assert.IsFalse (c.IsHandleCreated, "FA5");
			
			o = c.AutoScaleMode;
			c.AutoScaleMode = AutoScaleMode.Dpi;
			Assert.IsFalse (c.IsHandleCreated, "FA6");
			o = c.AutoScroll;
			c.AutoScroll = !c.AutoScroll;
			Assert.IsFalse (c.IsHandleCreated, "FA7");
			
			o = c.AutoScrollMargin;
			c.AutoScrollMargin = new Size (c.AutoScrollMargin.Width + 1, c.AutoScrollMargin.Height + 1);
			Assert.IsFalse (c.IsHandleCreated, "FA8");
			
			o = c.AutoScrollMinSize;
			c.AutoScrollMinSize = new Size (c.AutoScrollMinSize.Width + 1, c.AutoScrollMinSize.Height + 1);
			Assert.IsFalse (c.IsHandleCreated, "FA9");

#if !MONO
			o = c.AutoScrollOffset;
			c.AutoScrollOffset = new Point (c.AutoScrollOffset.X + 1, c.AutoScrollOffset.Y + 1);
			Assert.IsFalse (c.IsHandleCreated, "FA10"); 
#endif
			
			o = c.AutoScrollPosition;
			c.AutoScrollPosition = new Point (c.AutoScrollPosition.X + 1, c.AutoScrollPosition.Y + 1);
			Assert.IsFalse (c.IsHandleCreated, "FA11");

			o = c.AutoSize;
			c.AutoSize = !c.AutoSize;
			Assert.IsFalse (c.IsHandleCreated, "FA12"); 
#if !MONO
			o = c.AutoSizeMode;
			c.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Assert.IsFalse (c.IsHandleCreated, "FA13");
#endif		
			o = c.AutoValidate;
			c.AutoValidate = AutoValidate.EnableAllowFocusChange;
			Assert.IsFalse (c.IsHandleCreated, "FA14");

			// B
			o = c.BackColor;
			c.BackColor = Color.Green;
			Assert.IsFalse (c.IsHandleCreated, "A9");
			o = c.BackgroundImage;
			c.BackgroundImage = new Bitmap (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "A10");
			o = c.BackgroundImageLayout;
			c.BackgroundImageLayout = ImageLayout.Stretch;
			Assert.IsFalse (c.IsHandleCreated, "A11");
			o = c.BindingContext;
			c.BindingContext = new BindingContext ();
			Assert.IsFalse (c.IsHandleCreated, "A12");
			o = c.Bottom;
			Assert.IsFalse (c.IsHandleCreated, "A13");
			o = c.Bounds;
			c.Bounds = new Rectangle (0, 0, 12, 12);
			Assert.IsFalse (c.IsHandleCreated, "A14");


			// B - Form
			o = c.BindingContext;
			c.BindingContext = null;
			Assert.IsFalse (c.IsHandleCreated, "FB1");
			
			// C
			o = c.CanFocus;
			Assert.IsFalse (c.IsHandleCreated, "A15");
			o = c.CanSelect;
			Assert.IsFalse (c.IsHandleCreated, "A16");
			o = c.Capture;
			Assert.IsFalse (c.IsHandleCreated, "A17a");
			c.Capture = true;
			Assert.IsTrue (c.IsHandleCreated, "A17b");
			c.Dispose ();
			
			c = new Form ();
			o = c.CausesValidation;
			c.CausesValidation = false;
			Assert.IsFalse (c.IsHandleCreated, "A18");
			o = c.ClientRectangle;
			Assert.IsFalse (c.IsHandleCreated, "A19");
			o = c.ClientSize;
			c.ClientSize = new Size (30, 30);
			Assert.IsFalse (c.IsHandleCreated, "A20");
			o = c.CompanyName;
			Assert.IsFalse (c.IsHandleCreated, "A21");
			o = c.Container;
			Assert.IsFalse (c.IsHandleCreated, "A22");
			o = c.ContainsFocus;
			Assert.IsFalse (c.IsHandleCreated, "A23");
			o = c.ContextMenu;
			c.ContextMenu = new ContextMenu ();
			Assert.IsFalse (c.IsHandleCreated, "A24");
			o = c.ContextMenuStrip;
			c.ContextMenuStrip = new ContextMenuStrip ();
			Assert.IsFalse (c.IsHandleCreated, "A25");
			o = c.Controls;
			Assert.IsFalse (c.IsHandleCreated, "A26");
			o = c.Created;
			Assert.IsFalse (c.IsHandleCreated, "A27");
			o = c.Cursor;
			c.Cursor = Cursors.Arrow;
			Assert.IsFalse (c.IsHandleCreated, "A28");

			// C - Form
			o = c.CancelButton;
			c.CancelButton = null;
			Assert.IsFalse (c.IsHandleCreated, "FC1");
			
			o = c.ClientSize;
			c.ClientSize = new Size (c.ClientSize.Width + 1, c.ClientSize.Height + 1);
			Assert.IsFalse (c.IsHandleCreated, "FC2");
			
			o = c.Container;
			Assert.IsFalse (c.IsHandleCreated, "FC3");
			
			o = c.ControlBox;
			c.ControlBox = !c.ControlBox;
			Assert.IsFalse (c.IsHandleCreated, "FC4");

			o = c.CurrentAutoScaleDimensions;
			Assert.IsFalse (c.IsHandleCreated, "FC5"); 

			// D
			o = c.DataBindings;
			Assert.IsFalse (c.IsHandleCreated, "A29");
			o = c.DisplayRectangle;
			Assert.IsFalse (c.IsHandleCreated, "A30");
			o = c.Disposing;
			Assert.IsFalse (c.IsHandleCreated, "A31");
			o = c.Dock;
			c.Dock = DockStyle.Fill;
			Assert.IsFalse (c.IsHandleCreated, "A32");

			// D - Form
			o = c.DataBindings;
			Assert.IsFalse (c.IsHandleCreated, "FD6");
			
			o = c.DesktopBounds;
			c.DesktopBounds = new Rectangle (3, 5, c.DesktopBounds.Width + 1, c.DesktopBounds.Height + 1);
			Assert.IsFalse (c.IsHandleCreated, "FD7");
			
			o = c.DesktopLocation;
			c.DesktopLocation = c.DesktopLocation + new Size (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "FD8");
			
			o = c.DialogResult;
			c.DialogResult = DialogResult.Abort;
			Assert.IsFalse (c.IsHandleCreated, "FD9");
			
			o = c.DisplayRectangle;
			Assert.IsFalse (c.IsHandleCreated, "FD10");
			
			o = c.Disposing;
			Assert.IsFalse (c.IsHandleCreated, "FD11");
			
			o = c.Dock;
			c.Dock = DockStyle.Right;
			Assert.IsFalse (c.IsHandleCreated, "FD12");

			// E-H
			o = c.Enabled;
			c.Enabled = false;
			Assert.IsFalse (c.IsHandleCreated, "A33");
			c.Dispose ();
			
			c = new Form ();  //Reset just in case enable = false affects things
			o = c.Focused;
			Assert.IsFalse (c.IsHandleCreated, "A34");
			o = c.Font;
			c.Font = new Font (c.Font, FontStyle.Bold);
			Assert.IsFalse (c.IsHandleCreated, "A35");
			o = c.ForeColor;
			c.ForeColor = Color.Green;
			Assert.IsFalse (c.IsHandleCreated, "A36");
			o = c.Handle;
			Assert.IsTrue (c.IsHandleCreated, "A37");
			c.Dispose ();
			
			c = new Form ();
			o = c.HasChildren;
			Assert.IsFalse (c.IsHandleCreated, "A38");
			o = c.Height;
			c.Height = 12;
			Assert.IsFalse (c.IsHandleCreated, "A39");

			// E-H - Form
			o = c.FormBorderStyle;
			c.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Assert.IsFalse (c.IsHandleCreated, "FF1");
			
			o = c.HelpButton;
			c.HelpButton = !c.HelpButton;
			Assert.IsFalse (c.IsHandleCreated, "FH1");

#if !MONO
			o = c.HorizontalScroll;
			Assert.IsFalse (c.IsHandleCreated, "FH2"); 
#endif
			// I - L
			c.ImeMode = ImeMode.On;
			Assert.IsFalse (c.IsHandleCreated, "A40");
			o = c.InvokeRequired;
			Assert.IsFalse (c.IsHandleCreated, "A41");
			o = c.IsAccessible;
			Assert.IsFalse (c.IsHandleCreated, "A42");
			o = c.IsDisposed;
			Assert.IsFalse (c.IsHandleCreated, "A43");
#if !MONO
			o = c.IsMirrored;
			Assert.IsFalse (c.IsHandleCreated, "A44");
#endif
			o = c.LayoutEngine;
			Assert.IsFalse (c.IsHandleCreated, "A45");
			o = c.Left;
			c.Left = 15;
			Assert.IsFalse (c.IsHandleCreated, "A46");
			o = c.Location;
			c.Location = new Point (20, 20);
			Assert.IsFalse (c.IsHandleCreated, "A47");

			// I - L - Form
			
			o = c.Icon;
			c.Icon = null;
			Assert.IsFalse (c.IsHandleCreated, "FI1");
			
			o = c.IsMdiChild;
			Assert.IsFalse (c.IsHandleCreated, "FI2");

			o = c.IsMdiContainer;
			c.IsMdiContainer = false;
			Assert.IsFalse (c.IsHandleCreated, "FI3");
			
			o = c.IsRestrictedWindow;
			Assert.IsFalse (c.IsHandleCreated, "FI4");
			
			o = c.KeyPreview;
			c.KeyPreview = !c.KeyPreview;
			Assert.IsFalse (c.IsHandleCreated, "FK1");
			
			o = c.Location;
			c.Location = c.Location + new Size (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "FL1");
			
			
			// M - N
			o = c.Margin;
			c.Margin = new Padding (6);
			Assert.IsFalse (c.IsHandleCreated, "A48");
			o = c.MaximumSize;
			c.MaximumSize = new Size (500, 500);
			Assert.IsFalse (c.IsHandleCreated, "A49");
			o = c.MinimumSize;
			c.MinimumSize = new Size (100, 100);
			Assert.IsFalse (c.IsHandleCreated, "A50");
			o = c.Name;
			c.Name = "web";
			Assert.IsFalse (c.IsHandleCreated, "A51");

			// M - O - Form
			o = c.MainMenuStrip;
			c.MainMenuStrip = null;
			Assert.IsFalse (c.IsHandleCreated, "FM1"); 
			
			o = c.MaximizeBox;
			c.MaximizeBox = !c.MaximizeBox;
			Assert.IsFalse (c.IsHandleCreated, "FM2");
			
			o = c.MaximumSize;
			c.MaximumSize = c.MaximumSize + new Size (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "FM3");
			
			o = c.MdiChildren;
			Assert.IsFalse (c.IsHandleCreated, "FM4");
			
			o = c.MdiParent;
			c.MdiParent = null;
			Assert.IsFalse (c.IsHandleCreated, "FM5");
			
			o = c.Menu;
			c.Menu = null;
			Assert.IsFalse (c.IsHandleCreated, "FM6");
			
			o = c.MergedMenu;
			Assert.IsFalse (c.IsHandleCreated, "FM7");
			
			o = c.MinimizeBox;
			c.MinimizeBox = !c.MinimizeBox;
			Assert.IsFalse (c.IsHandleCreated, "FM8");
			
			o = c.MinimumSize;
			c.MinimumSize = c.MinimumSize + new Size (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "FM9");
			
			o = c.Modal;
			Assert.IsFalse (c.IsHandleCreated, "FM10");
			
			o = c.Opacity;
			c.Opacity = 0.9;
			Assert.IsFalse (c.IsHandleCreated, "FO1");
			
			o = c.OwnedForms;
			Assert.IsFalse (c.IsHandleCreated, "FO2");
			
			o = c.Owner;
			c.Owner = null;
			Assert.IsFalse (c.IsHandleCreated, "FO3");
			
			// P - R
			o = c.Padding;
			c.Padding = new Padding (4);
			Assert.IsFalse (c.IsHandleCreated, "A52");
			o = c.Parent;
			c.TopLevel = false;
			c.Parent = new Form ();
			Assert.IsFalse (c.IsHandleCreated, "A53");
			c.Close ();
			
			c = new Form ();
			o = c.PreferredSize;
			Assert.IsFalse (c.IsHandleCreated, "A54");
			o = c.ProductName;
			Assert.IsFalse (c.IsHandleCreated, "A55");
			o = c.ProductVersion;
			Assert.IsFalse (c.IsHandleCreated, "A56");
			o = c.RecreatingHandle;
			Assert.IsFalse (c.IsHandleCreated, "A57");
			o = c.Region;
			c.Region = new Region (new Rectangle (0, 0, 177, 177));
			Assert.IsFalse (c.IsHandleCreated, "A58");
			o = c.Right;
			Assert.IsFalse (c.IsHandleCreated, "A59");
			o = c.RightToLeft;
			c.RightToLeft = RightToLeft.Yes;
			Assert.IsFalse (c.IsHandleCreated, "A60");

			// P - R - Form
			o = c.ParentForm;
			Assert.IsFalse (c.IsHandleCreated, "FP1");

#if !MONO
			o = c.RestoreBounds;
			Assert.IsFalse (c.IsHandleCreated, "FR1"); 
#endif
			

			// S - W
			o = c.Site;
			Assert.IsFalse (c.IsHandleCreated, "A61");
			o = c.Size;
			c.Size = new Size (188, 188);
			Assert.IsFalse (c.IsHandleCreated, "A62");
			o = c.TabIndex;
			c.TabIndex = 5;
			Assert.IsFalse (c.IsHandleCreated, "A63");
			o = c.Tag;
			c.Tag = "moooooooo";
			Assert.IsFalse (c.IsHandleCreated, "A64");
			o = c.Text;
			c.Text = "meoooowww";
			Assert.IsFalse (c.IsHandleCreated, "A65");
			o = c.Top;
			c.Top = 16;
			Assert.IsFalse (c.IsHandleCreated, "A66");
			o = c.TopLevelControl;
			Assert.IsFalse (c.IsHandleCreated, "A67");
#if !MONO
			o = c.UseWaitCursor;
			c.UseWaitCursor = true;
			Assert.IsFalse (c.IsHandleCreated, "A68");
#endif
			o = c.Visible;
			Assert.IsFalse (c.IsHandleCreated, "A69");
			
			c.Visible = true;
			Assert.IsTrue (c.IsHandleCreated, "A69-b");
			c.Dispose ();
			c = new Form ();
			
			o = c.Width;
			c.Width = 190;
			Assert.IsFalse (c.IsHandleCreated, "A70");
			o = c.WindowTarget;
			Assert.IsFalse (c.IsHandleCreated, "A71");

			// S - W - Form

			o = c.ShowIcon;
			c.ShowIcon = !c.ShowIcon;
			Assert.IsFalse (c.IsHandleCreated, "FS1"); 
			
			o = c.ShowInTaskbar;
			c.ShowInTaskbar = !c.ShowInTaskbar;
			Assert.IsFalse (c.IsHandleCreated, "FS2");
			
			o = c.Size;
			c.Size = c.Size + new Size (1, 1);
			Assert.IsFalse (c.IsHandleCreated, "FS3");
			
			o = c.SizeGripStyle;
			c.SizeGripStyle = SizeGripStyle.Show;
			Assert.IsFalse (c.IsHandleCreated, "FS4");
			
			o = c.StartPosition;
			c.StartPosition = FormStartPosition.Manual;
			Assert.IsFalse (c.IsHandleCreated, "FS5");
			
			o = c.Text;
			c.Text = "hooray!";
			Assert.IsFalse (c.IsHandleCreated, "FT1");
			
			o = c.TopLevel;
			c.TopLevel = true;
			Assert.IsFalse (c.IsHandleCreated, "FT2-a");

			o = c.TopLevel;
			c.TopLevel = false;
			Assert.IsFalse (c.IsHandleCreated, "FT2-b");
			
			o = c.TopMost;
			c.TopMost = !c.TopMost;
			Assert.IsFalse (c.IsHandleCreated, "FT3");
			
			o = c.TransparencyKey;
			c.TransparencyKey = Color.BurlyWood;
			Assert.IsFalse (c.IsHandleCreated, "FT4");

#if !MONO
			o = c.VerticalScroll;
			Assert.IsFalse (c.IsHandleCreated, "FV1"); 
#endif
			
			o = c.WindowState;
			c.WindowState = FormWindowState.Maximized;
			Assert.IsFalse (c.IsHandleCreated, "FW1");

			c.Dispose ();
			TestHelper.RemoveWarning (o);
		}

		[Test]
		public void TestProtectedProperties ()
		{
			// Not a surprise, but none of these cause handle creation.
			// Included just to absolutely certain.
			ProtectedPropertyForm c = new ProtectedPropertyForm ();

			object o;
#if !MONO
			o = c.PublicCanRaiseEvents;
			Assert.IsFalse (c.IsHandleCreated, "A1");
#endif
			o = c.PublicCreateParams;
			Assert.IsFalse (c.IsHandleCreated, "A2");
			o = c.PublicDefaultCursor;
			Assert.IsFalse (c.IsHandleCreated, "A3");
			o = c.PublicDefaultImeMode;
			Assert.IsFalse (c.IsHandleCreated, "A4");
			o = c.PublicDefaultMargin;
			Assert.IsFalse (c.IsHandleCreated, "A5");
			o = c.PublicDefaultMaximumSize;
			Assert.IsFalse (c.IsHandleCreated, "A6");
			o = c.PublicDefaultMinimumSize;
			Assert.IsFalse (c.IsHandleCreated, "A7");
			o = c.PublicDefaultPadding;
			Assert.IsFalse (c.IsHandleCreated, "A8");
			o = c.PublicDefaultSize;
			Assert.IsFalse (c.IsHandleCreated, "A9");
			o = c.PublicDoubleBuffered;
			c.PublicDoubleBuffered = !c.PublicDoubleBuffered;
			Assert.IsFalse (c.IsHandleCreated, "A10");
			o = c.PublicFontHeight;
			c.PublicFontHeight = c.PublicFontHeight + 1;
			Assert.IsFalse (c.IsHandleCreated, "A11");
			o = c.PublicRenderRightToLeft;
			Assert.IsFalse (c.IsHandleCreated, "A12");
			o = c.PublicResizeRedraw;
			c.PublicResizeRedraw = !c.PublicResizeRedraw;
			Assert.IsFalse (c.IsHandleCreated, "A13");
#if !MONO
			o = c.PublicScaleChildren;
			Assert.IsFalse (c.IsHandleCreated, "A14");
#endif
			o = c.PublicShowFocusCues;
			Assert.IsFalse (c.IsHandleCreated, "A15");
			o = c.PublicShowKeyboardCues;
			Assert.IsFalse (c.IsHandleCreated, "A16");

			o = c.PublicAutoScaleFactor;
			Assert.IsFalse (c.IsHandleCreated, "F1"); 
			
			o = c.PublicDesignMode;
			Assert.IsFalse (c.IsHandleCreated, "F2");
			
			o = c.PublicEvents;
			Assert.IsFalse (c.IsHandleCreated, "F3");
			
			o = c.PublicHScroll;
			c.PublicHScroll = !c.PublicHScroll;
			Assert.IsFalse (c.IsHandleCreated, "F4");
			
			o = c.PublicMaximizedBounds;
			c.PublicMaximizedBounds = new Rectangle (1, 1, 1, 1);
			Assert.IsFalse (c.IsHandleCreated, "F5");

			o = c.PublicShowWithoutActivation;
			Assert.IsFalse (c.IsHandleCreated, "F6"); 
						
			o = c.PublicVScroll;
			c.PublicVScroll = !c.PublicVScroll;
			Assert.IsFalse (c.IsHandleCreated, "F7");
			

			TestHelper.RemoveWarning (o);
		}

		Form invokeform = new Form ();

		[Test]
		public void TestPublicMethods ()
		{
			// Public Methods that force Handle creation:
			// - CreateGraphics ()
			// - GetChildAtPoint ()
			// - Invoke, BeginInvoke throws InvalidOperationException if Handle has not been created
			// - PointToClient ()
			// - PointToScreen ()
			// - RectangleToClient ()
			// - RectangleToScreen ()
			// - Select ()
			// - Show (IWin32Window)
			// Notes:
			// - CreateControl does NOT force Handle creation!
			
			Form c = new Form ();

			c.BringToFront ();
			Assert.IsFalse (c.IsHandleCreated, "A1");
			
			c.Contains (new Form ());
			Assert.IsFalse (c.IsHandleCreated, "A2");
			
			c.CreateControl ();
			Assert.IsFalse (c.IsHandleCreated, "A3");
			
			c = new Form ();
			Graphics g = c.CreateGraphics ();
			g.Dispose ();
			Assert.IsTrue (c.IsHandleCreated, "A4");
			c.Dispose ();
			c = new Form ();
			
			c.Dispose ();
			Assert.IsFalse (c.IsHandleCreated, "A5");
			c = new Form ();

			// This is weird, it causes a form to appear that won't go away until you move the mouse over it, 
			// but it doesn't create a handle??
			//DragDropEffects d = c.DoDragDrop ("yo", DragDropEffects.None);
			//Assert.IsFalse (c.IsHandleCreated, "A6");
			//Assert.AreEqual (DragDropEffects.None, d, "A6b");
			
			//Bitmap b = new Bitmap (100, 100);
			//c.DrawToBitmap (b, new Rectangle (0, 0, 100, 100));
			//Assert.IsFalse (c.IsHandleCreated, "A7");
			//b.Dispose ();
			c.FindForm ();
			Assert.IsFalse (c.IsHandleCreated, "A8");
			
			c.Focus ();
			Assert.IsFalse (c.IsHandleCreated, "A9");

			c.GetChildAtPoint (new Point (10, 10));
			Assert.IsTrue (c.IsHandleCreated, "A10");
			c.Dispose ();
			c = new Form ();
			
			c.GetContainerControl ();
			Assert.IsFalse (c.IsHandleCreated, "A11");
			c.Dispose ();
			
			c = new Form ();
			c.GetNextControl (new Control (), true);
			Assert.IsFalse (c.IsHandleCreated, "A12");
			c.GetPreferredSize (Size.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A13");
			c.Hide ();
			Assert.IsFalse (c.IsHandleCreated, "A14");
			
			c.Invalidate ();
			Assert.IsFalse (c.IsHandleCreated, "A15");
			
			//c.Invoke (new InvokeDelegate (InvokeMethod));
			//Assert.IsFalse (c.IsHandleCreated, "A16");
			c.PerformLayout ();
			Assert.IsFalse (c.IsHandleCreated, "A17");
			
			c.PointToClient (new Point (100, 100));
			Assert.IsTrue (c.IsHandleCreated, "A18");
			c.Dispose ();
			c = new Form ();
			
			c.PointToScreen (new Point (100, 100));
			Assert.IsTrue (c.IsHandleCreated, "A19");
			c.Dispose ();
			
			c = new Form ();
			
			//c.PreProcessControlMessage   ???
			//c.PreProcessMessage          ???
			c.RectangleToClient (new Rectangle (0, 0, 100, 100));
			Assert.IsTrue (c.IsHandleCreated, "A20");
			c.Dispose ();
			c = new Form ();
			c.RectangleToScreen (new Rectangle (0, 0, 100, 100));
			Assert.IsTrue (c.IsHandleCreated, "A21");
			c.Dispose ();
			c = new Form ();
			c.Refresh ();
			Assert.IsFalse (c.IsHandleCreated, "A22");
			c.ResetBackColor ();
			Assert.IsFalse (c.IsHandleCreated, "A23");
			c.ResetBindings ();
			Assert.IsFalse (c.IsHandleCreated, "A24");
			c.ResetCursor ();
			Assert.IsFalse (c.IsHandleCreated, "A25");
			c.ResetFont ();
			Assert.IsFalse (c.IsHandleCreated, "A26");
			c.ResetForeColor ();
			Assert.IsFalse (c.IsHandleCreated, "A27");
			c.ResetImeMode ();
			Assert.IsFalse (c.IsHandleCreated, "A28");
			c.ResetRightToLeft ();
			Assert.IsFalse (c.IsHandleCreated, "A29");
			c.ResetText ();
			Assert.IsFalse (c.IsHandleCreated, "A30");
			c.SuspendLayout ();
			Assert.IsFalse (c.IsHandleCreated, "A31");
			c.ResumeLayout ();
			Assert.IsFalse (c.IsHandleCreated, "A32");
			c.Scale (new SizeF (1.5f, 1.5f));
			Assert.IsFalse (c.IsHandleCreated, "A33");
			c.Select ();
			Assert.IsTrue (c.IsHandleCreated, "A34");
			c.Dispose ();
			
			c = new Form ();
			
			c.SelectNextControl (new Control (), true, true, true, true);
			Assert.IsFalse (c.IsHandleCreated, "A35");
			c.SetBounds (0, 0, 100, 100);
			Assert.IsFalse (c.IsHandleCreated, "A36");
			c.Update ();
			Assert.IsFalse (c.IsHandleCreated, "A37");
			
			// Form
			
			c.Activate ();
			Assert.IsFalse (c.IsHandleCreated, "F1");
			
			c.AddOwnedForm (new Form ());
			Assert.IsFalse (c.IsHandleCreated, "F2");
			
			c.Close ();
			Assert.IsFalse (c.IsHandleCreated, "F3");
			
			c.Hide ();
			Assert.IsFalse (c.IsHandleCreated, "F4");
			
			c.LayoutMdi (MdiLayout.Cascade);
			Assert.IsFalse (c.IsHandleCreated, "F5");

#if !MONO
			c.PerformAutoScale ();
			Assert.IsFalse (c.IsHandleCreated, "F6"); 
#endif
			
			c.PerformLayout ();
			Assert.IsFalse (c.IsHandleCreated, "F7");
			
			c.AddOwnedForm (new Form ());
			c.RemoveOwnedForm (c.OwnedForms [c.OwnedForms.Length - 1]);
			Assert.IsFalse (c.IsHandleCreated, "F8");
			
			c.ScrollControlIntoView (null);
			Assert.IsFalse (c.IsHandleCreated, "F9");
			
			c.SetAutoScrollMargin (7, 13);
			Assert.IsFalse (c.IsHandleCreated, "F10");
			
			c.SetDesktopBounds (-1, -1, 144, 169);
			Assert.IsFalse (c.IsHandleCreated, "F11");
			
			c.SetDesktopLocation (7, 13);
			Assert.IsFalse (c.IsHandleCreated, "F12");

			c = new Form ();
			c.Show (null);
			Assert.IsTrue (c.IsHandleCreated, "F13");
			c.Close ();
			c = new Form (); 
			
			//c.ShowDialog ()
			
			c.ToString ();
			Assert.IsFalse (c.IsHandleCreated, "F14");
			
			c.Validate ();
			Assert.IsFalse (c.IsHandleCreated, "F15");

#if !MONO
			c.ValidateChildren ();
			Assert.IsFalse (c.IsHandleCreated, "F16"); 
#endif

			c.Close ();
		}

		[Test]
		public void Show ()
		{
			Form c = new Form ();
			Assert.IsFalse (c.IsHandleCreated, "A1");
			c.HandleCreated += new EventHandler (HandleCreated_WriteStackTrace);
			c.Show ();
			Assert.IsTrue (c.IsHandleCreated, "A2");
			c.Dispose ();
		}

		void HandleCreated_WriteStackTrace (object sender, EventArgs e)
		{
			//Console.WriteLine (Environment.StackTrace);
		}

		public delegate void InvokeDelegate ();
		public void InvokeMethod () { invokeform.Text = "methodinvoked"; }

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InvokeIOE ()
		{
			Form c = new Form ();
			c.Invoke (new InvokeDelegate (InvokeMethod));
		}

		public class ProtectedPropertyForm : Form
		{
			public SizeF PublicAutoScaleFactor { get { return base.AutoScaleFactor; } } 
#if !MONO
			public bool PublicCanRaiseEvents { get { return base.CanRaiseEvents; } }
#endif
			public CreateParams PublicCreateParams { get { return base.CreateParams; } }
			public Cursor PublicDefaultCursor { get { return base.DefaultCursor; } }
			public ImeMode PublicDefaultImeMode { get { return base.DefaultImeMode; } }
			public Padding PublicDefaultMargin { get { return base.DefaultMargin; } }
			public Size PublicDefaultMaximumSize { get { return base.DefaultMaximumSize; } }
			public Size PublicDefaultMinimumSize { get { return base.DefaultMinimumSize; } }
			public Padding PublicDefaultPadding { get { return base.DefaultPadding; } }
			public Size PublicDefaultSize { get { return base.DefaultSize; } }
			public bool PublicDesignMode { get {return base.DesignMode; } }
			public bool PublicDoubleBuffered { get { return base.DoubleBuffered; } set { base.DoubleBuffered = value; } }
			public EventHandlerList PublicEvents { get {return base.Events; } }			
			public int PublicFontHeight { get { return base.FontHeight; } set { base.FontHeight = value; } }
			public bool PublicHScroll { get {return base.HScroll; } set { base.HScroll = value;} }
			public Rectangle PublicMaximizedBounds { get {return base.MaximizedBounds; } set { base.MaximizedBounds = value; }}
			public bool PublicRenderRightToLeft { get { return base.RenderRightToLeft; } }
			public bool PublicResizeRedraw { get { return base.ResizeRedraw; } set { base.ResizeRedraw = value; } }
#if !MONO
			public bool PublicScaleChildren { get { return base.ScaleChildren; } }
#endif
			public bool PublicShowFocusCues { get { return base.ShowFocusCues; } }
			public bool PublicShowKeyboardCues { get { return base.ShowKeyboardCues; } }
			public bool PublicShowWithoutActivation { get { return base.ShowWithoutActivation; } } 
			public bool PublicVScroll { get { return base.VScroll; } set { base.VScroll = value; } }
		}

		[Test]
		public void TestProtectedMethods ()
		{
			// Protected Methods that force Handle creation:
			// - CreateAccessibilityInstance ()
			// - CreateHandle ()
			// - IsInputChar ()
			// - Select ()
			// - SetVisibleCore ()
			// - CenterToParent ()
			// - CenterToScreen ()
			ProtectedMethodsForm c = new ProtectedMethodsForm ();

			c.PublicAccessibilityNotifyClients (AccessibleEvents.Focus, 0);
			Assert.IsFalse (c.IsHandleCreated, "A1");
			c.PublicCreateAccessibilityInstance ();
			Assert.IsTrue (c.IsHandleCreated, "A2");
			c.Dispose ();
			c = new ProtectedMethodsForm ();
			c.PublicCreateControlsInstance ();
			Assert.IsFalse (c.IsHandleCreated, "A3");
			c.PublicCreateHandle ();
			Assert.IsTrue (c.IsHandleCreated, "A4");
			c.Dispose ();
			
			c = new ProtectedMethodsForm ();
			c.PublicDestroyHandle ();
			Assert.IsFalse (c.IsHandleCreated, "A5");
			c.Dispose ();
			c = new ProtectedMethodsForm ();
			c.PublicGetAccessibilityObjectById (0);
			Assert.IsFalse (c.IsHandleCreated, "A6");
#if !MONO
			c.PublicGetAutoSizeMode ();
			Assert.IsFalse (c.IsHandleCreated, "A7");
			c.PublicGetScaledBounds (new Rectangle (0, 0, 100, 100), new SizeF (1.5f, 1.5f), BoundsSpecified.All);
			Assert.IsFalse (c.IsHandleCreated, "A8");
#endif
			c.PublicGetStyle (ControlStyles.FixedHeight);
			Assert.IsFalse (c.IsHandleCreated, "A9");
			c.PublicGetTopLevel ();
			Assert.IsFalse (c.IsHandleCreated, "A10");
			c.PublicInitLayout ();
			Assert.IsFalse (c.IsHandleCreated, "A11");
			c.PublicInvokeGotFocus (c, EventArgs.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A12");
			c.PublicInvokeLostFocus (c, EventArgs.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A13");
			c.PublicInvokeOnClick (c, EventArgs.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A14");
			c.PublicInvokePaint (c, new PaintEventArgs (Graphics.FromImage (new Bitmap (1, 1)), Rectangle.Empty));
			Assert.IsFalse (c.IsHandleCreated, "A15");
			c.PublicInvokePaintBackground (c, new PaintEventArgs (Graphics.FromImage (new Bitmap (1, 1)), Rectangle.Empty));
			Assert.IsFalse (c.IsHandleCreated, "A16");
			c.PublicIsInputChar ('c');
			Assert.IsTrue (c.IsHandleCreated, "A17");
			c.Dispose ();
			
			c = new ProtectedMethodsForm ();
			c.PublicIsInputKey (Keys.B);
			Assert.IsFalse (c.IsHandleCreated, "A18");
			c.PublicNotifyInvalidate (Rectangle.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A19");
			Form f = new Form ();
			c.TopLevel = false;
			f.Controls.Add (c);
			c.PublicOnVisibleChanged (EventArgs.Empty);
			Assert.IsFalse (c.IsHandleCreated, "A20");
			c.Dispose ();
			c = new ProtectedMethodsForm ();
			c.PublicRaiseDragEvent (null, null);
			Assert.IsFalse (c.IsHandleCreated, "A21");
			c.PublicRaiseKeyEvent (null, null);
			Assert.IsFalse (c.IsHandleCreated, "A22");
			c.PublicRaiseMouseEvent (null, null);
			Assert.IsFalse (c.IsHandleCreated, "A23");
			c.PublicRaisePaintEvent (null, null);
			Assert.IsFalse (c.IsHandleCreated, "A24");
			c.PublicRecreateHandle ();
			Assert.IsFalse (c.IsHandleCreated, "A25");
			c.PublicResetMouseEventArgs ();
			Assert.IsFalse (c.IsHandleCreated, "A26");
			c.PublicRtlTranslateAlignment (ContentAlignment.BottomLeft);
			Assert.IsFalse (c.IsHandleCreated, "A27");
			c.PublicRtlTranslateContent (ContentAlignment.BottomLeft);
			Assert.IsFalse (c.IsHandleCreated, "A28");
			c.PublicRtlTranslateHorizontal (HorizontalAlignment.Left);
			Assert.IsFalse (c.IsHandleCreated, "A29");
			c.PublicRtlTranslateLeftRight (LeftRightAlignment.Left);
			Assert.IsFalse (c.IsHandleCreated, "A30");
#if !MONO
			c.PublicScaleControl (new SizeF (1.5f, 1.5f), BoundsSpecified.All);
			Assert.IsFalse (c.IsHandleCreated, "A31");
#endif
			c.PublicScaleCore (1.5f, 1.5f);
			Assert.IsFalse (c.IsHandleCreated, "A32");
			c.PublicSelect ();
			Assert.IsTrue (c.IsHandleCreated, "A33");
			c.Dispose ();
			
			c = new ProtectedMethodsForm ();
#if !MONO
			c.PublicSetAutoSizeMode (AutoSizeMode.GrowAndShrink);
			Assert.IsFalse (c.IsHandleCreated, "A34");
#endif
			c.PublicSetBoundsCore (0, 0, 100, 100, BoundsSpecified.All);
			Assert.IsFalse (c.IsHandleCreated, "A35");
			c.PublicSetClientSizeCore (122, 122);
			Assert.IsFalse (c.IsHandleCreated, "A36");
			c.PublicSetStyle (ControlStyles.FixedHeight, true);
			Assert.IsFalse (c.IsHandleCreated, "A37");
			
			c.PublicSetTopLevel (true);
			Assert.IsFalse (c.IsHandleCreated, "A38");
			c.Dispose ();
			
			c = new ProtectedMethodsForm ();
			
			c.PublicSetVisibleCore (true);
			Assert.IsTrue (c.IsHandleCreated, "A39");
			c.Dispose ();
			
			c = new ProtectedMethodsForm ();
			c.PublicSizeFromClientSize (new Size (160, 160));
			Assert.IsFalse (c.IsHandleCreated, "A40");
			c.PublicUpdateBounds ();
			Assert.IsFalse (c.IsHandleCreated, "A41");
			c.PublicUpdateStyles ();
			Assert.IsFalse (c.IsHandleCreated, "A42");
			c.PublicUpdateZOrder ();
			Assert.IsFalse (c.IsHandleCreated, "A43");
			c.Dispose ();
			
			// Form
			c = new ProtectedMethodsForm ();
			c.IsMdiContainer = true;
			new Form ().MdiParent = c;
			new Form ().MdiParent = c;
			c.PublicActivateMdiChild (c.MdiChildren [0]);
			c.PublicActivateMdiChild (c.MdiChildren [1]);
			Assert.IsFalse (c.IsHandleCreated, "F1");
			c.Dispose ();
			c = new ProtectedMethodsForm();

			c.PublicAdjustFormScrollbars (true);
			Assert.IsFalse (c.IsHandleCreated, "F2");
			
			c.PublicCenterToParent ();
			Assert.IsTrue (c.IsHandleCreated, "F3");
			c.Dispose ();
			c = new ProtectedMethodsForm ();
			
			c.PublicCenterToScreen ();
			Assert.IsTrue (c.IsHandleCreated, "F4");
			c.Dispose ();
			c = new ProtectedMethodsForm ();
			
			c.PublicGetScrollState (1);
			Assert.IsFalse (c.IsHandleCreated, "F5");
			
			c.PublicGetService (typeof (int));
			Assert.IsFalse (c.IsHandleCreated, "F6");

			Message m = new Message ();
			c.PublicProcessCmdKey (ref m, Keys.C);
			Assert.IsFalse (c.IsHandleCreated, "F7");
			
			c.PublicProcessDialogChar ('p');
			Assert.IsFalse (c.IsHandleCreated, "F8");
			
			c.PublicProcessDialogKey (Keys.D);
			Assert.IsFalse (c.IsHandleCreated, "F9");
			
			c.PublicProcessKeyEventArgs (ref m);
			Assert.IsFalse (c.IsHandleCreated, "F10");
			
			c.PublicProcessKeyMessage (ref m);
			Assert.IsFalse (c.IsHandleCreated, "F11");
			
			c.PublicProcessKeyPreview (ref m);
			Assert.IsFalse (c.IsHandleCreated, "F12");

			c.PublicProcessMnemonic ('Z');
			Assert.IsFalse (c.IsHandleCreated, "F13");
			
			c.PublicProcessTabKey (true);
			Assert.IsFalse (c.IsHandleCreated, "F14");

#if !MONO
			c.Controls.Add (new Control ());
			c.PublicScrollToControl (c.Controls [0]);
			Assert.IsFalse (c.IsHandleCreated, "F15");
			c.Dispose ();
			c = new ProtectedMethodsForm (); 
#endif
			
			c.PublicSelect (true, true);
			Assert.IsTrue (c.IsHandleCreated, "F16");
			c.Dispose ();
			
			c = new ProtectedMethodsForm();
			
			c.PublicSetDisplayRectLocation (13, 17);
			Assert.IsFalse (c.IsHandleCreated, "F17");
			
			c.PublicSetScrollState (5, false);
			Assert.IsFalse (c.IsHandleCreated, "F18");
			
			c.PublicUpdateDefaultButton (3, false);
			Assert.IsFalse (c.IsHandleCreated, "F19");

			c.Dispose ();
		}

		public class ProtectedMethodsForm : Form
		{
			public void PublicAccessibilityNotifyClients (AccessibleEvents accEvent, int childID) { base.AccessibilityNotifyClients (accEvent, childID); }
			public void PublicActivateMdiChild (Form form) { base.ActivateMdiChild (form); }
			public void PublicAdjustFormScrollbars (bool displayScrollbars) {base.AdjustFormScrollbars (displayScrollbars); }
			public void PublicCenterToParent () { base.CenterToParent (); }
			public void PublicCenterToScreen () { base.CenterToScreen (); }
			public void PublicCreateAccessibilityInstance () { base.CreateAccessibilityInstance (); }
			public void PublicCreateControlsInstance () { base.CreateControlsInstance (); }
			public void PublicCreateHandle () { base.CreateHandle (); }
			public void PublicDestroyHandle () { base.DestroyHandle (); }
			public AccessibleObject PublicGetAccessibilityObjectById (int objectId) { return base.GetAccessibilityObjectById (objectId); }
#if !MONO
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
#if !MONO
			public void PublicScaleControl (SizeF factor, BoundsSpecified specified) { base.ScaleControl (factor, specified); }
#endif
			public void PublicScaleCore (float dx, float dy) { base.ScaleCore (dx, dy); }
#if !MONO
			public void PublicScrollToControl (Control activeControl) { base.ScrollToControl (activeControl); } 
#endif
			public void PublicSelect () { base.Select (); }
			public void PublicSelect (bool directed, bool forward) { base.Select (directed, forward); }

#if !MONO
			public void PublicSetAutoSizeMode (AutoSizeMode mode) { base.SetAutoSizeMode (mode); }
#endif
			public void PublicSetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) { base.SetBoundsCore (x, y, width, height, specified); }
			public void PublicSetClientSizeCore (int x, int y) { base.SetClientSizeCore (x, y); }
			public void PublicSetDisplayRectLocation (int x, int y) { base.SetDisplayRectLocation (x, y); }
			public void PublicSetScrollState (int bit, bool value) { base.SetScrollState (bit, value); }
			public void PublicSetStyle (ControlStyles flag, bool value) { base.SetStyle (flag, value); }
			public void PublicSetTopLevel (bool value) { base.SetTopLevel (value); }
			public void PublicSetVisibleCore (bool value) { base.SetVisibleCore (value); }
			public Size PublicSizeFromClientSize (Size clientSize) { return base.SizeFromClientSize (clientSize); }
			public void PublicUpdateBounds () { base.UpdateBounds (); }
			public void PublicUpdateDefaultButton (int bit, bool value) { base.UpdateDefaultButton (); }
			public void PublicUpdateStyles () { base.UpdateStyles (); }
			public void PublicUpdateZOrder () { base.UpdateZOrder (); }
		}
	}
}
