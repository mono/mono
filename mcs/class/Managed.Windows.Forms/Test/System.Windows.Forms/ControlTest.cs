//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//		Stefan Noack (noackstefan@googlemail.com)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
#if NET_2_0
using System.Windows.Forms.Layout;
#endif

using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ControlTest : TestHelper
	{
		[Test] // .ctor ()
		public void Constructor1 ()
		{
			MockControl c = new MockControl ();

			Assert.AreEqual (0, c.OnLocationChangedCount, "OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "Height");
			Assert.AreEqual (0, c.Left, "Left");
			Assert.AreEqual (string.Empty, c.Name, "Name");
			Assert.IsNull (c.Parent, "Parent");
			Assert.AreEqual (string.Empty, c.Text, "#A:Text");
			Assert.AreEqual (0, c.Top, "Top");
			Assert.AreEqual (0, c.Width, "Width");
		}

		[Test] // .ctor (String)
		public void Constructor2 ()
		{
			MockControl c = new MockControl ((string) null);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#A:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#A:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#A:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#A:Height");
			Assert.AreEqual (0, c.Left, "#A:Left");
			Assert.AreEqual (string.Empty, c.Name, "#A:Name");
			Assert.IsNull (c.Parent, "#A:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#A:Text");
			Assert.AreEqual (0, c.Top, "#A:Top");
			Assert.AreEqual (0, c.Width, "#A:Width");

			c = new MockControl ("child");

			Assert.AreEqual (0, c.OnLocationChangedCount, "#B:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#B:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#B:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#B:Height");
			Assert.AreEqual (0, c.Left, "#B:Left");
			Assert.AreEqual (string.Empty, c.Name, "#B:Name");
			Assert.IsNull (c.Parent, "#B:Parent");
			Assert.AreEqual ("child", c.Text, "#B:Text");
			Assert.AreEqual (0, c.Top, "#B:Top");
			Assert.AreEqual (0, c.Width, "#B:Width");
		}

		[Test] // .ctor (Control, String)
		public void Constructor3 ()
		{
			Control parent = new Control ("parent");
			MockControl c = new MockControl ((Control) null, (string) null);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#A:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#A:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#A:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#A:Height");
			Assert.AreEqual (0, c.Left, "#A:Left");
			Assert.AreEqual (string.Empty, c.Name, "#A:Name");
			Assert.IsNull (c.Parent, "#A:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#A:Text");
			Assert.AreEqual (0, c.Top, "#A:Top");
			Assert.AreEqual (0, c.Width, "#A:Width");

			c = new MockControl ((Control) null, "child");

			Assert.AreEqual (0, c.OnLocationChangedCount, "#B:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#B:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#B:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#B:Height");
			Assert.AreEqual (0, c.Left, "#B:Left");
			Assert.AreEqual (string.Empty, c.Name, "#B:Name");
			Assert.IsNull (c.Parent, "#B:Parent");
			Assert.AreEqual ("child", c.Text, "#B:Text");
			Assert.AreEqual (0, c.Top, "#B:Top");
			Assert.AreEqual (0, c.Width, "#B:Width");

			c = new MockControl (parent, (string) null);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#C:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#C:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#C:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#C:Height");
			Assert.AreEqual (0, c.Left, "#C:Left");
			Assert.AreEqual (string.Empty, c.Name, "#C:Name");
			Assert.AreSame (parent, c.Parent, "#C:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#C:Text");
			Assert.AreEqual (0, c.Top, "#C:Top");
			Assert.AreEqual (0, c.Width, "#C:Width");

			c = new MockControl (parent, "child");

			Assert.AreEqual (0, c.OnLocationChangedCount, "#D:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#D:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#D:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#D:Height");
			Assert.AreEqual (0, c.Left, "#D:Left");
			Assert.AreEqual (string.Empty, c.Name, "#D:Name");
			Assert.AreSame (parent, c.Parent, "#D:Parent");
			Assert.AreEqual ("child", c.Text, "#D:Text");
			Assert.AreEqual (0, c.Top, "#D:Top");
			Assert.AreEqual (0, c.Width, "#D:Width");
		}

		[Test] // .ctor (String, Int32, Int32, Int32, Int32)
		public void Constructor4 ()
		{
			MockControl c = new MockControl ((string) null, 0, 0, 0, 0);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#A:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#A:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#A:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#A:Height");
			Assert.AreEqual (0, c.Left, "#A:Left");
			Assert.AreEqual (string.Empty, c.Name, "#A:Name");
			Assert.IsNull (c.Parent, "#A:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#A:Text");
			Assert.AreEqual (0, c.Top, "#A:Top");
			Assert.AreEqual (0, c.Width, "#A:Width");

			c = new MockControl ((string) null, 1, 0, 0, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#B:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#B:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#B:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#B:Height");
			Assert.AreEqual (1, c.Left, "#B:Left");
			Assert.AreEqual (string.Empty, c.Name, "#B:Name");
			Assert.IsNull (c.Parent, "#B:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#B:Text");
			Assert.AreEqual (0, c.Top, "#B:Top");
			Assert.AreEqual (0, c.Width, "#B:Width");

			c = new MockControl ("child", 0, 1, 0, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#C:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#C:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#C:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#C:Height");
			Assert.AreEqual (0, c.Left, "#C:Left");
			Assert.AreEqual (string.Empty, c.Name, "#C:Name");
			Assert.IsNull (c.Parent, "#C:Parent");
			Assert.AreEqual ("child", c.Text, "#C:Text");
			Assert.AreEqual (1, c.Top, "#C:Top");
			Assert.AreEqual (0, c.Width, "#C:Width");

			c = new MockControl ("child", 0, 0, 1, 0);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#D:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#D:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#D:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#D:Height");
			Assert.AreEqual (0, c.Left, "#D:Left");
			Assert.AreEqual (string.Empty, c.Name, "#D:Name");
			Assert.IsNull (c.Parent, "#D:Parent");
			Assert.AreEqual ("child", c.Text, "#D:Text");
			Assert.AreEqual (0, c.Top, "#D:Top");
			Assert.AreEqual (1, c.Width, "#D:Width");

			c = new MockControl ("child", 0, 0, 0, 1);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#E:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#E:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#E:OnSizeChangedCount");
			Assert.AreEqual (1, c.Height, "#E:Height");
			Assert.AreEqual (0, c.Left, "#E:Left");
			Assert.AreEqual (string.Empty, c.Name, "#E:Name");
			Assert.IsNull (c.Parent, "#E:Parent");
			Assert.AreEqual ("child", c.Text, "#E:Text");
			Assert.AreEqual (0, c.Top, "#E:Top");
			Assert.AreEqual (0, c.Width, "#E:Width");

			c = new MockControl ("child", 1, 0, 1, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#F:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#F:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#F:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#F:Height");
			Assert.AreEqual (1, c.Left, "#F:Left");
			Assert.AreEqual (string.Empty, c.Name, "#F:Name");
			Assert.IsNull (c.Parent, "#F:Parent");
			Assert.AreEqual ("child", c.Text, "#F:Text");
			Assert.AreEqual (0, c.Top, "#F:Top");
			Assert.AreEqual (1, c.Width, "#F:Width");

			c = new MockControl ("child", 0, 1, 0, 1);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#G:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#G:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#G:OnSizeChangedCount");
			Assert.AreEqual (1, c.Height, "#G:Height");
			Assert.AreEqual (0, c.Left, "#G:Left");
			Assert.AreEqual (string.Empty, c.Name, "#G:Name");
			Assert.IsNull (c.Parent, "#G:Parent");
			Assert.AreEqual ("child", c.Text, "#G:Text");
			Assert.AreEqual (1, c.Top, "#G:Top");
			Assert.AreEqual (0, c.Width, "#G:Width");
		}

		[Test] // .ctor (Control, String, Int32, Int32, Int32, Int32)
		public void Constructor5 ()
		{
			Control parent = new Control ("parent");
			MockControl c = new MockControl ((Control) null,
				(string) null, 0, 0, 0, 0);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#A:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#A:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#A:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#A:Height");
			Assert.AreEqual (0, c.Left, "#A:Left");
			Assert.AreEqual (string.Empty, c.Name, "#A:Name");
			Assert.IsNull (c.Parent, "#A:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#A:Text");
			Assert.AreEqual (0, c.Top, "#A:Top");
			Assert.AreEqual (0, c.Width, "#A:Width");

			c = new MockControl (parent, (string) null, 1, 0, 0, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#B:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#B:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#B:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#B:Height");
			Assert.AreEqual (1, c.Left, "#B:Left");
			Assert.AreEqual (string.Empty, c.Name, "#B:Name");
			Assert.AreSame (parent, c.Parent, "#B:Parent");
			Assert.AreEqual (string.Empty, c.Text, "#B:Text");
			Assert.AreEqual (0, c.Top, "#B:Top");
			Assert.AreEqual (0, c.Width, "#B:Width");

			c = new MockControl ((Control) null, "child", 0, 1, 0, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#C:OnLocationChangedCount");
			Assert.AreEqual (0, c.OnResizeCount, "#C:OnResizeCount");
			Assert.AreEqual (0, c.OnSizeChangedCount, "#C:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#C:Height");
			Assert.AreEqual (0, c.Left, "#C:Left");
			Assert.AreEqual (string.Empty, c.Name, "#C:Name");
			Assert.IsNull (c.Parent, "#C:Parent");
			Assert.AreEqual ("child", c.Text, "#C:Text");
			Assert.AreEqual (1, c.Top, "#C:Top");
			Assert.AreEqual (0, c.Width, "#C:Width");

			c = new MockControl (parent, "child", 0, 0, 1, 0);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#D:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#D:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#D:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#D:Height");
			Assert.AreEqual (0, c.Left, "#D:Left");
			Assert.AreEqual (string.Empty, c.Name, "#D:Name");
			Assert.AreSame (parent, c.Parent, "#D:Parent");
			Assert.AreEqual ("child", c.Text, "#D:Text");
			Assert.AreEqual (0, c.Top, "#D:Top");
			Assert.AreEqual (1, c.Width, "#D:Width");

			c = new MockControl (parent, "child", 0, 0, 0, 1);

			Assert.AreEqual (0, c.OnLocationChangedCount, "#E:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#E:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#E:OnSizeChangedCount");
			Assert.AreEqual (1, c.Height, "#E:Height");
			Assert.AreEqual (0, c.Left, "#E:Left");
			Assert.AreEqual (string.Empty, c.Name, "#E:Name");
			Assert.AreSame (parent, c.Parent, "#E:Parent");
			Assert.AreEqual ("child", c.Text, "#E:Text");
			Assert.AreEqual (0, c.Top, "#E:Top");
			Assert.AreEqual (0, c.Width, "#E:Width");

			c = new MockControl (parent, "child", 1, 0, 1, 0);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#F:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#F:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#F:OnSizeChangedCount");
			Assert.AreEqual (0, c.Height, "#F:Height");
			Assert.AreEqual (1, c.Left, "#F:Left");
			Assert.AreEqual (string.Empty, c.Name, "#F:Name");
			Assert.AreSame (parent, c.Parent, "#F:Parent");
			Assert.AreEqual ("child", c.Text, "#F:Text");
			Assert.AreEqual (0, c.Top, "#F:Top");
			Assert.AreEqual (1, c.Width, "#F:Width");

			c = new MockControl (parent, "child", 0, 1, 0, 1);

			Assert.AreEqual (1, c.OnLocationChangedCount, "#G:OnLocationChangedCount");
			Assert.AreEqual (1, c.OnResizeCount, "#G:OnResizeCount");
			Assert.AreEqual (1, c.OnSizeChangedCount, "#G:OnSizeChangedCount");
			Assert.AreEqual (1, c.Height, "#G:Height");
			Assert.AreEqual (0, c.Left, "#G:Left");
			Assert.AreEqual (string.Empty, c.Name, "#G:Name");
			Assert.AreSame (parent, c.Parent, "#G:Parent");
			Assert.AreEqual ("child", c.Text, "#G:Text");
			Assert.AreEqual (1, c.Top, "#G:Top");
			Assert.AreEqual (0, c.Width, "#G:Width");
		}

		[Test]
		public void DisposeTest ()
		{
			ControlDisposeTester control = new ControlDisposeTester ();
			control.Visible = true;
			control.Dispose ();

			try {
				control.CreateControl ();
			} catch (ObjectDisposedException ex) {
				Console.WriteLine (ex);
				Assert.Fail ("#1");
			}
			Assert.IsFalse (control.IsHandleCreated, "#2");

			// The control remains Visible until WM_DESTROY is received.
			Assert.IsTrue (control.Visible, "#3");

			try {
				control.InvokeCreateHandle ();
				Assert.Fail ("#4");
			} catch (ObjectDisposedException ex) {
				//Console.WriteLine (ex);
			}
		}

		private class ControlDisposeTester : Control
		{
			public void InvokeCreateHandle ()
			{
				CreateHandle ();
			}
		}
#if NET_2_0
		[Test]
		public void AutoSizeTest ()
		{
			ControlAutoSizeTester c = new ControlAutoSizeTester (new Size (23, 17), AutoSizeMode.GrowAndShrink);
			
			Form f = new Form();
			f.Size = new Size (200, 200);
			c.Parent = f;
			f.Show();
			
			Size s = new Size (42, 42);
			c.Size = s;
			
			Point l = new Point (10, 10);
			c.Location = l;
			
			//Check wether normal size setting is OK
			Assert.AreEqual (s, c.Size, "#S1");
			
			//Check wether size remains without GetPreferredSize implemented even when AutoSize turned on.
			c.AutoSize = true;
			f.PerformLayout();
			Assert.AreEqual (s, c.Size, "#S2");
			
			//Simulate a Control implementing GetPreferredSize
			c.UseCustomPrefSize = true;
			f.PerformLayout();
			
			//Check wether size shrinks to preferred size
			Assert.AreEqual (c.CustomPrefSize, c.Size, "#S3");
			//Check wether Location stays constant
			Assert.AreEqual (l, c.Location, "#L1");
			
			//Check wether Dock is respected
			c.Dock = DockStyle.Bottom;
			Assert.AreEqual (f.ClientSize.Width, c.Width, "#D1");
			
			//Check wether size shrinks to preferred size again
			c.Dock = DockStyle.None;
			Assert.AreEqual (c.CustomPrefSize, c.Size, "#S4");
			
			//Check wether Anchor is respected for adjusting Locatioon
			c.Anchor = AnchorStyles.Bottom;
			f.Height += 50;
			Assert.AreEqual (l.Y + 50, c.Top, "#A1");
			//Check wether size is still OK
			Assert.AreEqual (c.CustomPrefSize, c.Size, "#S5");
			
			
			//just tidy up
			c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			c.Location = l;
			
			//Check wether shrinking to zero is possible 
			c.CustomPrefSize = new Size (0, 0);
			f.PerformLayout();
			Assert.AreEqual (c.CustomPrefSize, c.Size, "#S6");
			
			//Check wether MinimumSize is honored
			c.MinimumSize = new Size (10, 12);
			c.CustomPrefSize = new Size (5, 5);
			f.PerformLayout();
			Assert.AreEqual (c.MinimumSize, c.Size, "#S7");
			c.MinimumSize = new Size (0, 0);
			
			//Check wether MaximumSize is honored
			c.MaximumSize = new Size (100, 120); 
			c.CustomPrefSize = new Size (500, 500);
			f.PerformLayout();
			Assert.AreEqual (c.MaximumSize, c.Size, "#S8");
			
			//Check wether shrinking does not happen when GrowOnly
			c.AutoSize = false;
			s = new Size (23, 23);
			c.Size = s;
			c.CustomPrefSize = new Size (5, 5);
			c.AutoSizeMode = AutoSizeMode.GrowOnly;
			c.AutoSize = true;
			f.PerformLayout();
			Assert.AreEqual (s, c.Size, "#S9");
			f.Close ();
		}
		
		public class ControlAutoSizeTester : Control {
			

			public ControlAutoSizeTester (Size customPrefSize, AutoSizeMode autoSizeMode)
			{
				custom_prefsize = customPrefSize;
				AutoSizeMode = autoSizeMode;
			}
			
			public AutoSizeMode AutoSizeMode {
				set {
					base.SetAutoSizeMode (value);
				}
			}

			private bool use_custom_prefsize = false;
			public bool UseCustomPrefSize {
				set {
					use_custom_prefsize = value;
				}
			}
			
			private Size custom_prefsize;
			
			public Size CustomPrefSize {
				get {
					return custom_prefsize;
				}
				set {
					custom_prefsize = value;
				}
			}
			
			
			public override Size GetPreferredSize (Size proposedSize)
			{
				if (use_custom_prefsize) {
					return custom_prefsize;
				} else {	
					return base.GetPreferredSize(proposedSize);
				}
			}
			
		}
#endif
		
		[Test]
		public void Bug82748 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			Control c = new Control ();
			c.Size = new Size (100, 100);
			
			Control c2 = new Control ();
			c2.Size = c.Size;
			c2.Controls.Add (c);
			
			c.Anchor = AnchorStyles.Right;
			
			f.Controls.Add (c);
			
			f.Show ();
			
			Assert.AreEqual (0, c.Left, "A1");
			
			f.Close ();
			f.Dispose ();
		}
		
		[Test]
		public void InvokeTestParentHandle ()
		{
			Control child, parent;
			
			parent = new Control ();
			child = new Control ();
			parent.Controls.Add (child);
			parent.Visible = true;
			parent.CreateControl ();
			child.Invoke (new CrossAppDomainDelegate (dummy)) ;
		}
		
		public void dummy ()
		{
		}
		
		public class ControlStylesTester : Control {
			private WindowStyles style;
			private WindowStyles ex_style;
			private bool or_styles;

			public ControlStylesTester (WindowStyles Style, WindowStyles ExStyle, bool OrStyles)
			{
				style = Style;
				ex_style = ExStyle;
				or_styles = OrStyles;
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams result = base.CreateParams;
					if (or_styles) {
						result.Style |= (int) style;
						result.ExStyle |= (int) ex_style;
					} else {
						result.Style = (int) style;
						result.ExStyle = (int) ex_style;
					}
					return result;
				}
			}
		}
		
		[Test]
		public void ControlSizeTest ()
		{
			ControlStylesTester c = new ControlStylesTester (WindowStyles.WS_CHILD, 0, true);
			Assert.AreEqual ("{X=0,Y=0}", c.Location.ToString (), "#L1");
			Assert.AreEqual ("{Width=0, Height=0}", c.Size.ToString (), "#S1");
		}

#if NET_2_0
		[Test]
		public void CaptureTest ()
		{
			Form frm = new Form ();
			ControlOverrideLogger log = new ControlOverrideLogger ();
			frm.Controls.Add (log);
			log.Visible = true;
			log.Capture = true;
			log.Capture = false;
			log.BackColor = Color.Blue;
			log.Size = new Size (100, 100);
			
			frm.Show ();
			Application.DoEvents ();
			
			frm.Dispose ();
			Assert.IsTrue (log.Log.ToString ().IndexOf ("OnMouseCaptureChanged") > -1, "#01");
			
			log = new ControlOverrideLogger ();
			log.Capture = true;
			log.Capture = false;
			Assert.IsTrue (log.Log.ToString ().IndexOf ("OnMouseCaptureChanged") > -1, "#02");

			log = new ControlOverrideLogger ();
			log.Capture = true;
			Assert.IsTrue (log.IsHandleCreated, "#03");
			Assert.IsTrue (log.Log.ToString ().IndexOf ("OnMouseCaptureChanged") == -1, "#04");
			
			log = new ControlOverrideLogger ();
			log.Capture = false;
			Assert.IsFalse (log.IsHandleCreated, "#05");
			Assert.IsTrue (log.Log.ToString ().IndexOf ("OnMouseCaptureChanged") == -1, "#06");
		}
#endif
	
		public class OnPaintTester : Form
		{
			int counter;
			int total;
			ArrayList list = new ArrayList ();
			public bool Recursive;
			public bool TestRefresh;
			public bool TestInvalidate;
			public bool TestUpdate;
#if NET_2_0
			public new bool DoubleBuffered {
				get {
					return base.DoubleBuffered;
				}
				set {
					base.DoubleBuffered = value;
				}
			}
#endif
			protected override void OnPaint (PaintEventArgs pevent)
			{
				Assert.IsFalse (list.Contains (pevent.Graphics), "OnPaintTester.OnPaint: Got the same Graphics twice");
				list.Add (pevent.Graphics);
				
				if (total > 10)
					return;
				Recursive = counter > 0 || Recursive;
				counter++;
				if (counter < 2) {
					if (TestRefresh)
						Refresh ();
					else if (TestInvalidate)
						Invalidate ();
					else {
						Update ();
					}
				}
				base.OnPaint (pevent);
				counter--;
				total++;
			}
			public new void Show ()
			{
				base.Show ();
				Application.DoEvents ();
			}
		}

		[Test]
		public void ControlCollectionTest ()
		{
			Form frm = new Form ();
			frm.IsMdiContainer = true;
			Form child = new Form ();
			Control.ControlCollection c = new Control.ControlCollection (frm);
			child.MdiParent = frm;
			c.Add (child);
			
			child.Dispose ();
			frm.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ControlCollectionExceptionTest ()
		{
			Form frm = new Form ();
			frm.IsMdiContainer = true;
			Form child = new Form ();
			Control.ControlCollection c = new Control.ControlCollection (frm);
			//child.MdiParent = frm;
			c.Add (child);
			
			child.Dispose ();
			frm.Dispose ();			
		}
		
		[Test]
		[Category ("NotWorking")]
		public void OnPaintTest ()
		{
			using (OnPaintTester t = new OnPaintTester ()) {
				t.TestRefresh = true;
				t.Show ();
				Assert.IsTrue (t.Recursive, "#1");
			}

			using (OnPaintTester t = new OnPaintTester ()) {
				t.TestUpdate = true;
				t.Show ();
				Assert.IsFalse (t.Recursive, "#2");
			}

			using (OnPaintTester t = new OnPaintTester ()) {
				t.TestInvalidate = true;
				t.Show ();
				Assert.IsFalse (t.Recursive, "#3");
			}
		}
#if NET_2_0
		[Test]
		[Category ("Interactive")]
		public void OnPaintDoubleBufferedTest ()
		{
			using (OnPaintTester t = new OnPaintTester ()) {
				t.DoubleBuffered = true;
				t.TestRefresh = true;
				t.Show ();
				Assert.IsTrue (t.Recursive, "#1");
			}

			using (OnPaintTester t = new OnPaintTester ()) {
				t.DoubleBuffered = true;
				t.TestUpdate = true;
				t.Show ();
				Assert.IsFalse (t.Recursive, "#2");
			}

			using (OnPaintTester t = new OnPaintTester ()) {
				t.DoubleBuffered = true;
				t.TestInvalidate = true;
				t.Show ();
				Assert.IsFalse (t.Recursive, "#3");
			}
		}
#endif
		public class PaintEventForm : Form
		{
			public ArrayList overrides = new ArrayList ();
			public ArrayList events = new ArrayList ();

			public PaintEventForm ()
			{
				Paint += new PaintEventHandler (DoubleBufferEventForm_Paint);
			}
			public bool GetControlStyle (ControlStyles style)
			{
				return base.GetStyle (style);
			}

			public void SetControlStyle (ControlStyles style, bool value)
			{
				base.SetStyle (style, value);
			}
			
			void DoubleBufferEventForm_Paint (object sender, PaintEventArgs e)
			{
				events.Add("Paint");
			}
			
			protected override void OnPaintBackground (PaintEventArgs e)
			{
				base.OnPaintBackground (e);
				overrides.Add("OnPaintBackground");
			}

			protected override void OnPaint (PaintEventArgs pevent)
			{
				base.OnPaint (pevent);
				overrides.Add("OnPaint");
			}
		}

		[Test]
		[Ignore ("Can't find a reliable way to generate a paint message on Windows.")]
		public void EventStyleTest ()
		{
#if NET_2_0
			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.OptimizedDoubleBuffer, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#A1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#A2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#A3");
			}
#endif
			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.DoubleBuffer, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#B1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#B2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#B3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.AllPaintingInWmPaint, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#C1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#C2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#C3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.UserPaint, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#D1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#D2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#D3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#E1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#E2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#E3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#F1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#F2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#F3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#G1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#G2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#G3");
			}

			using (PaintEventForm f = new PaintEventForm ()) {
				f.Show ();
				f.SetControlStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
				f.Refresh ();
				Assert.IsTrue (f.overrides.Contains ("OnPaintBackground"), "#H1");
				Assert.IsTrue (f.overrides.Contains ("OnPaint"), "#H2");
				Assert.IsTrue (f.events.Contains ("Paint"), "#H3");
			}
		}
#if NET_2_0
		public class DoubleBufferedForm : Form
		{
			public bool painted;
			public bool failed;
			public DoubleBufferedForm ()
			{
				this.DoubleBuffered = true;
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				if (failed || painted)
					return;
				painted = true;
				Height = Height + 1;
				try {
					e.Graphics.DrawString (Size.ToString (), Font, Brushes.AliceBlue, new Point (2, 2));
				} catch (Exception exception) {
					Console.WriteLine (exception.StackTrace);
					failed = true;
				}
			}
		}

		public class DoubleBufferControl : Control
		{
			public bool IsDoubleBuffered
			{
				get { return base.DoubleBuffered; }
				set { base.DoubleBuffered = value; }
			}

			public bool GetControlStyle (ControlStyles style)
			{
				return base.GetStyle (style);
			}

			public void SetControlStyle (ControlStyles style, bool value)
			{
				base.SetStyle (style, value);
			}
		}

		[Test]
		[Ignore ("Can't find a reliable way to generate a paint message on Windows.")]
		public void DoubleBufferTest ()
		{
			DoubleBufferedForm f = new DoubleBufferedForm ();
			f.ShowInTaskbar = false;
			f.Show ();
			f.Refresh ();
			
			Assert.IsFalse (f.failed, "#01");
			Assert.IsTrue (f.painted, "The control was never painted, so please check the test");
			f.Close ();
		}

		[Test]
		public void DoubleBufferedTest ()
		{
			DoubleBufferControl c = new DoubleBufferControl ();
			Assert.IsFalse (c.IsDoubleBuffered, "#A1");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#A2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#A3");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#A4");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.UserPaint), "#A5");

			c.SetControlStyle (ControlStyles.OptimizedDoubleBuffer, true);
			Assert.IsTrue (c.IsDoubleBuffered, "#B1");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#B2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#B3");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#B4");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.UserPaint), "#A5");

			c.SetControlStyle (ControlStyles.AllPaintingInWmPaint, false);
			c.SetControlStyle (ControlStyles.UserPaint, false);
			Assert.IsTrue (c.IsDoubleBuffered, "#C1");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#C2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#C3");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#C4");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.UserPaint), "#C5");

			c.SetControlStyle (ControlStyles.OptimizedDoubleBuffer, false);
			Assert.IsFalse (c.IsDoubleBuffered, "#D1");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#D2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#D3");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#D4");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.UserPaint), "#D5");

			c.SetControlStyle (ControlStyles.DoubleBuffer, true);
			Assert.IsFalse (c.IsDoubleBuffered, "#E1");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#E2");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.DoubleBuffer), "#E3");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#E4");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.UserPaint), "#E5");

			c.SetControlStyle (ControlStyles.DoubleBuffer, false);
			Assert.IsFalse (c.IsDoubleBuffered, "#F1");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#F2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#F3");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#F4");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.UserPaint), "#F5");

			c.IsDoubleBuffered = true;
			Assert.IsTrue (c.IsDoubleBuffered, "#G1");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#G2");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.DoubleBuffer), "#G3");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#G4");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.UserPaint), "#G5");

			c.SetControlStyle (ControlStyles.AllPaintingInWmPaint, true);
			c.SetControlStyle (ControlStyles.OptimizedDoubleBuffer, true);
			c.SetControlStyle (ControlStyles.DoubleBuffer, true);
			c.SetControlStyle (ControlStyles.UserPaint, true);
			c.IsDoubleBuffered = false;
			Assert.IsFalse (c.IsDoubleBuffered, "#H1");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#H2");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.DoubleBuffer), "#H3");
			Assert.IsFalse (c.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#H4");
			Assert.IsTrue (c.GetControlStyle (ControlStyles.UserPaint), "#H5");
		}

		[Test]
		public void DoubleBufferedStyleTest ()
		{
			DoubleBufferControl c = new DoubleBufferControl ();
			TestControlStyle.CheckStyles (c, "#A1", ControlStyles.UserPaint, ControlStyles.StandardClick, ControlStyles.Selectable, ControlStyles.StandardDoubleClick, ControlStyles.AllPaintingInWmPaint, ControlStyles.UseTextForAccessibility);

			c.IsDoubleBuffered = true;
			TestControlStyle.CheckStyles (c, "#A2", ControlStyles.UserPaint, ControlStyles.StandardClick, ControlStyles.Selectable, ControlStyles.StandardDoubleClick, ControlStyles.AllPaintingInWmPaint, ControlStyles.UseTextForAccessibility, ControlStyles.OptimizedDoubleBuffer);

			c.IsDoubleBuffered = false;
			TestControlStyle.CheckStyles (c, "#A3", ControlStyles.UserPaint, ControlStyles.StandardClick, ControlStyles.Selectable, ControlStyles.StandardDoubleClick, ControlStyles.AllPaintingInWmPaint, ControlStyles.UseTextForAccessibility);

			c = new DoubleBufferControl ();
			foreach (ControlStyles style in Enum.GetValues (typeof(ControlStyles))) {
				c.SetControlStyle (style, false);
			}

			TestControlStyle.CheckStyles (c, "#B1");

			c.IsDoubleBuffered = true;
			TestControlStyle.CheckStyles (c, "#B2", ControlStyles.OptimizedDoubleBuffer, ControlStyles.AllPaintingInWmPaint);

			c.IsDoubleBuffered = false;
			TestControlStyle.CheckStyles (c, "#B3", ControlStyles.AllPaintingInWmPaint);

		}
#endif

		class Helper {
			public static void TestAccessibility(Control c, string Default, string Description, string Name, AccessibleRole Role)
			{
				Assert.IsNotNull (c.AccessibilityObject, "Acc1");
				Assert.AreEqual (Default, c.AccessibleDefaultActionDescription, "Acc2");
				Assert.AreEqual (Description, c.AccessibleDescription, "Acc3");
				Assert.AreEqual (Name, c.AccessibleName, "Acc4");
				Assert.AreEqual (Role, c.AccessibleRole, "Acc5");
			}

			public static string TestControl(Control container, Control start, bool forward) {
				Control ctl;

				ctl = container.GetNextControl(start, forward);

				if (ctl == null) {
					return null;
				}

				return ctl.Text;
			}
		}

		[Test]
		public void CreatedTest ()
		{
			Control c = new Control ();
			Assert.IsFalse (c.Created, "A1");
		}

		class CustomA11yEnabledControl : Control {
			public CustomA11yEnabledControl () : base() { }
			protected override AccessibleObject CreateAccessibilityInstance () { return new CustomAccessibleObject (this); }

			class CustomAccessibleObject : ControlAccessibleObject {
				public CustomAccessibleObject (CustomA11yEnabledControl control) : base(control) { }
				public override string Name { get { return "custom name"; } }
				public override string DefaultAction { get { return "custom default action"; } }
				public override string Description { get { return "custom description"; } }
				public override AccessibleRole Role { get { return AccessibleRole.Alert; } }
			}
		}

		[Test]
		public void CreatedAccessibilityTest ()
		{
			CustomA11yEnabledControl c = new CustomA11yEnabledControl ();
			Assert.IsFalse(c.Created, "A1");

			// Tests default values

			Assert.AreEqual (null, c.AccessibleDefaultActionDescription, "A2.0");
			Assert.IsFalse(c.IsHandleCreated, "A2.1");

			Assert.AreEqual (null, c.AccessibleDescription, "A3.0");
			Assert.IsFalse(c.IsHandleCreated, "A3.1");

			Assert.AreEqual (null, c.AccessibleName, "A4.0");
			Assert.IsFalse(c.IsHandleCreated, "A4.1");

			Assert.AreEqual (AccessibleRole.Default, c.AccessibleRole, "A5.0");
			Assert.IsFalse(c.IsHandleCreated, "A5.1");

			object o = c.AccessibilityObject;
			Assert.IsTrue(c.IsHandleCreated, "A6");

			// Tests to confirm that:
			// - calling Control.AccessibleXXXXX is not returning AccessibleObject.XXXXX
			// - Handle is not Created when calling Control.AccessibleXXXXX
			c = new CustomA11yEnabledControl ();

 			string accessibleDefaultActionDescription = "default action description";
			c.AccessibleDefaultActionDescription = accessibleDefaultActionDescription;
			Assert.IsFalse (c.IsHandleCreated, "A7.0");
			Assert.AreEqual (accessibleDefaultActionDescription, c.AccessibleDefaultActionDescription, "A7.1");

			string accessibleDescription = "accessible description";
			c.AccessibleDescription = accessibleDescription;
			Assert.IsFalse (c.IsHandleCreated, "A8.0");
			Assert.AreEqual (accessibleDescription, c.AccessibleDescription, "A8.1");

			string accessibleName = "accessible name";
			c.AccessibleName = accessibleName;
			Assert.IsFalse (c.IsHandleCreated, "A9.0");
			Assert.AreEqual (accessibleName, c.AccessibleName, "A9.1");

			AccessibleRole accessibleRole = AccessibleRole.Diagram;
			c.AccessibleRole = accessibleRole;
			Assert.AreEqual (accessibleRole, c.AccessibleRole, "A10.0");
			Assert.IsFalse (c.IsHandleCreated, "A10.1");

			c.Dispose ();
			Assert.IsFalse (c.Created, "A11");
		}

		[Test]
		[Category ("NotWorking")]
		public void BoundsTest ()
		{
			Control c = new Control ();
			Assert.IsTrue (c.Bounds.IsEmpty, "A1");
			Assert.IsTrue (c.Size.IsEmpty, "A2");
			Assert.IsTrue (c.ClientSize.IsEmpty, "A3");
			Assert.IsTrue (c.ClientRectangle.IsEmpty, "A4");

			Assert.AreEqual (((IWin32Window)c).Handle, c.Handle, "A5");

			/* this part fails on linux because we can't allocate X windows which are 0x0,
			   and the Control bounds directly reflect the size of the X window */

			Assert.IsTrue (c.Bounds.IsEmpty, "A6");
			Assert.IsTrue (c.Size.IsEmpty, "A7");
			Assert.IsTrue (c.ClientSize.IsEmpty, "A8");
			Assert.IsTrue (c.ClientRectangle.IsEmpty, "A9");
		}

		[Test]
		[Ignore ("Depends on specific DPI")]
		public void FontHeightTest ()
		{
			MockControl c = new MockControl ();
			Assert.AreEqual (13, c.font_height);
		}

		[Test]
		public void FontTest ()
		{
			Control c = new Control ();
			Assert.IsFalse (c.Font.Bold, "#A1");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.FontFamily.Name, "#A2");
			Assert.IsFalse (c.Font.Italic, "#A3");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.Name, "#A4");
			Assert.AreEqual (8.25, c.Font.Size, "#A5");
			Assert.AreEqual (8.25, c.Font.SizeInPoints, "#A6");
			Assert.IsFalse (c.Font.Strikeout, "#A7");
			Assert.IsFalse (c.Font.Underline, "#A8");
			Assert.AreEqual (GraphicsUnit.Point, c.Font.Unit, "#A9");
#if NET_2_0
			Assert.IsTrue (c.Font.IsSystemFont, "#A10");
#endif

			c.Font = new Font (c.Font.FontFamily, 3, FontStyle.Italic);
			Assert.IsFalse (c.Font.Bold, "#B1");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.FontFamily.Name, "#B2");
			Assert.IsTrue (c.Font.Italic, "#B3");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.Name, "#B4");
			Assert.AreEqual (3, c.Font.Size, "#B5");
			Assert.AreEqual (3, c.Font.SizeInPoints, "#B6");
			Assert.IsFalse (c.Font.Strikeout, "#B7");
			Assert.IsFalse (c.Font.Underline, "#B8");
			Assert.AreEqual (GraphicsUnit.Point, c.Font.Unit, "#B9");
#if NET_2_0
			Assert.AreEqual (false, c.Font.IsSystemFont, "#B10");
#endif
		}

		[Test]
		[Category ("NotWorking")] // on Unix mapping is done to Bitstream Vera Sans
		public void FontTest_Names ()
		{
			Control c = new Control ();
			Assert.AreEqual ("Microsoft Sans Serif", c.Font.FontFamily.Name, "#1");
			Assert.AreEqual ("Microsoft Sans Serif", c.Font.Name, "#2");
		}

		[Test]
		public void PubPropTest()
		{
			Control c = new Control();

			Assert.IsFalse (c.AllowDrop , "A1");
			Assert.AreEqual(AnchorStyles.Top | AnchorStyles.Left, c.Anchor, "A2");

			Assert.AreEqual ("Control", c.BackColor.Name , "B1");
			Assert.IsNull (c.BackgroundImage, "B2");
			Assert.IsNull (c.BindingContext, "B3");
#if NET_2_0
			Assert.AreEqual (ImageLayout.Tile, c.BackgroundImageLayout, "B4");
#endif

			Assert.IsFalse (c.CanFocus, "C1");
			Assert.IsTrue (c.CanSelect, "C2");
			Assert.IsFalse (c.Capture, "C3");
			Assert.IsTrue (c.CausesValidation, "C4");

			Assert.IsNotNull (c.CompanyName, "C7");
			Assert.IsNull (c.Container, "C8");
			Assert.IsFalse (c.ContainsFocus, "C9");
			Assert.IsNull (c.ContextMenu, "C10");
			Assert.AreEqual (0, c.Controls.Count, "C11");
			Assert.IsFalse (c.Created, "C12");
			Assert.AreEqual (Cursors.Default, c.Cursor, "C13");

			Assert.IsNotNull(c.DataBindings, "D1");
			Assert.AreEqual("Control", Control.DefaultBackColor.Name, "D2");
			Assert.AreEqual("ControlText", Control.DefaultForeColor.Name, "D3");
			Assert.AreEqual(FontStyle.Regular, Control.DefaultFont.Style, "D4");
			Assert.AreEqual (new Rectangle(0, 0, 0, 0), c.DisplayRectangle , "D5");
			Assert.IsFalse (c.Disposing, "D6");
			Assert.AreEqual(DockStyle.None, c.Dock, "D7");

			Assert.IsTrue (c.Enabled, "E1");

			Assert.IsFalse  (c.Focused, "F1");
			Assert.AreEqual (FontStyle.Regular, c.Font.Style, "F2");
			Assert.AreEqual (SystemColors.ControlText, c.ForeColor, "F3");

			Assert.IsFalse  (c.HasChildren, "H2");

			Assert.AreEqual (ImeMode.NoControl, c.ImeMode, "I1");
			Assert.IsFalse (c.InvokeRequired, "I2");
			Assert.IsFalse (c.IsAccessible, "I3");
			Assert.IsFalse (c.IsDisposed, "I4");
			Assert.IsFalse (c.IsHandleCreated, "I5");

			Assert.AreEqual(Point.Empty, c.Location, "L2");

#if NET_2_0
			Assert.IsTrue(c.MaximumSize.IsEmpty);
			Assert.IsTrue(c.MinimumSize.IsEmpty);
#endif
			Assert.AreEqual (Keys.None, Control.ModifierKeys, "M1");
			Assert.IsTrue (Control.MousePosition.X >= 0 && Control.MousePosition.Y >= 0, "M2");
			Assert.AreEqual (MouseButtons.None, Control.MouseButtons, "M3");

			Assert.AreEqual("", c.Name, "N1");
			c.Name = "Control Name";
			Assert.AreEqual("Control Name", c.Name, "N2");

			Assert.IsNull (c.Parent, "P1");
			Assert.IsNotNull (c.ProductName, "P2");
			Assert.IsTrue (c.ProductName != "", "P3");
			Assert.IsNotNull (c.ProductVersion, "P4");
			Assert.IsTrue (c.ProductVersion != "", "P5");

			Assert.IsFalse (c.RecreatingHandle, "R1");
			Assert.IsNull (c.Region, "R2");
			Assert.AreEqual (RightToLeft.No, c.RightToLeft, "R4");

			Assert.IsNull (c.Site, "S1");

			Assert.AreEqual (0, c.TabIndex , "T1");
			Assert.IsTrue (c.TabStop, "T2");
			Assert.IsNull (c.Tag, "T3");
			Assert.AreEqual ("", c.Text, "T4");

			Assert.IsTrue (c.Visible, "V1");
		}

		[Test]
		public void SizeChangeTest ()
		{
			Form f = new Form ();
			Control c = new Control ();
			f.Controls.Add(c);
			f.Show();
			c.Resize += new EventHandler(SizeChangedTest_ResizeHandler);
			c.Tag = true;
			c.Size = c.Size;
			Assert.AreEqual (true, (bool) c.Tag, "#1");
			f.Close ();
		}

		private void SizeChangedTest_ResizeHandler (object sender, EventArgs e)
		{
			((Control) sender).Tag = false;
		}

		[Test]
		public void NegativeHeightTest ()
		{
			Control c = new Control ();
			IntPtr handle = c.Handle;
			c.Resize += new EventHandler(NegativeHeightTest_ResizeHandler);
			c.Tag = -2;
			c.Height = 2;
			c.Height = -2;
			Assert.AreEqual (0, (int) c.Tag, "#1");
			c.Dispose ();
			Assert.AreEqual (handle, handle, "Removes warning.");
		}
		
		private void NegativeHeightTest_ResizeHandler (object sender, EventArgs e)
		{
			Control c = (Control) sender;
			c.Tag = c.Height;
		}
		
		[Test]
		public void TopLevelControlTest () {
			Control c = new Control ();

			Assert.AreEqual(null, c.TopLevelControl, "T1");

			Panel p = new Panel ();

			p.Controls.Add (c);

			Assert.AreEqual(null, c.TopLevelControl, "T2");

			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Controls.Add (p);

			Assert.AreEqual (f, c.TopLevelControl, "T3");
			Assert.AreEqual (f, f.TopLevelControl, "T4");
			
			f.Dispose ();
		}

		[Test]
		public void RelationTest() {
			Control c1;
			Control c2;

			c1 = new Control();
			c2 = new Control();

			Assert.AreEqual(true , c1.Visible , "Rel1");
			Assert.AreEqual(false, c1.Contains(c2) , "Rel2");
			Assert.AreEqual("System.Windows.Forms.Control", c1.ToString() , "Rel3");

			c1.Controls.Add(c2);
			Assert.AreEqual(true , c2.Visible , "Rel4");
			Assert.AreEqual(true, c1.Contains(c2) , "Rel5");

			c1.Anchor = AnchorStyles.Top;
			c1.SuspendLayout ();
			c1.Anchor = AnchorStyles.Left ;
			c1.ResumeLayout ();
			Assert.AreEqual(AnchorStyles.Left , c1.Anchor, "Rel6");

			c1.SetBounds(10, 20, 30, 40) ;
			Assert.AreEqual(new Rectangle(10, 20, 30, 40), c1.Bounds, "Rel7");

			Assert.AreEqual(c1, c2.Parent, "Rel8");
		}

		[Test]
		public void AnchorDockTest ()
		{
			Control c = new Control ();

			Assert.AreEqual (DockStyle.None, c.Dock, "1");
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, c.Anchor, "2");

			c.Dock = DockStyle.Top;
			Assert.AreEqual (DockStyle.Top, c.Dock, "3");
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, c.Anchor, "4");

			c.Anchor = AnchorStyles.Top;
			Assert.AreEqual (DockStyle.None, c.Dock, "5");
			Assert.AreEqual (AnchorStyles.Top, c.Anchor, "6");
		}

		[Test]
		[Category ("NotWorking")]
		public void TabOrder()
		{
			Form		form;
			//Control		active;

			Label		label1 = new Label();		// To test non-tabstop items as well
			Label		label2 = new Label();

			GroupBox	group1 = new GroupBox();
			GroupBox	group2 = new GroupBox();
			GroupBox	group3 = new GroupBox();

			TextBox		text1 = new TextBox();

			RadioButton	radio11 = new RadioButton();
			RadioButton	radio12 = new RadioButton();
			RadioButton	radio13 = new RadioButton();
			RadioButton	radio14 = new RadioButton();
			RadioButton	radio21 = new RadioButton();
			RadioButton	radio22 = new RadioButton();
			RadioButton	radio23 = new RadioButton();
			RadioButton	radio24 = new RadioButton();
			RadioButton	radio31 = new RadioButton();
			RadioButton	radio32 = new RadioButton();
			RadioButton	radio33 = new RadioButton();
			RadioButton	radio34 = new RadioButton();

			form = new Form();
			form.ShowInTaskbar = false;

			form.ClientSize = new Size (520, 520);
			Assert.AreEqual(new Size(520, 520), form.ClientSize, "Tab1");

			form.Text = "SWF Taborder Test App Form";
			Assert.AreEqual("SWF Taborder Test App Form", form.Text, "Tab2");

			label1.Location = new Point(10, 10);
			Assert.AreEqual(new Point(10, 10), label1.Location, "Tab3");
			label1.Text = "Label1";
			form.Controls.Add(label1);

			label2.Location = new Point(200, 10);
			label2.Text = "Label2";
			form.Controls.Add(label2);

			group1.Text = "Group1";
			group2.Text = "Group2";
			group3.Text = "Group3";

			group1.Size = new Size(200, 400);
			group2.Size = new Size(200, 400);
			group3.Size = new Size(180, 180);
			Assert.AreEqual(new Size(180, 180), group3.Size, "Tab4");

			group1.Location = new Point(10, 40);
			group2.Location = new Point(220, 40);
			group3.Location = new Point(10, 210);

			group1.TabIndex = 30;
			Assert.AreEqual(30, group1.TabIndex, "Tab5");
			group1.TabStop = true;

			// Don't assign, test automatic assignment
			//group2.TabIndex = 0;
			group2.TabStop = true;
			Assert.AreEqual(0, group2.TabIndex, "Tab6");

			group3.TabIndex = 35;
			group3.TabStop = true;

			// Test default tab index
			Assert.AreEqual(0, radio11.TabIndex, "Tab7");

			text1.Text = "Edit Control";

			radio11.Text = "Radio 1-1 [Tab1]";
			radio12.Text = "Radio 1-2 [Tab2]";
			radio13.Text = "Radio 1-3 [Tab3]";
			radio14.Text = "Radio 1-4 [Tab4]";

			radio21.Text = "Radio 2-1 [Tab4]";
			radio22.Text = "Radio 2-2 [Tab3]";
			radio23.Text = "Radio 2-3 [Tab2]";
			radio24.Text = "Radio 2-4 [Tab1]";

			radio31.Text = "Radio 3-1 [Tab1]";
			radio32.Text = "Radio 3-2 [Tab3]";
			radio33.Text = "Radio 3-3 [Tab2]";
			radio34.Text = "Radio 3-4 [Tab4]";

			// We don't assign TabIndex for radio1X; test automatic assignment
			text1.TabStop = true;
			radio11.TabStop = true;

			radio21.TabIndex = 4;
			radio22.TabIndex = 3;
			radio23.TabIndex = 2;
			radio24.TabIndex = 1;
			radio24.TabStop = true;

			radio31.TabIndex = 11;
			radio31.TabStop = true;
			radio32.TabIndex = 13;
			radio33.TabIndex = 12;
			radio34.TabIndex = 14;

			text1.Location = new Point(10, 100);

			radio11.Location = new Point(10, 20);
			radio12.Location = new Point(10, 40);
			radio13.Location = new Point(10, 60);
			radio14.Location = new Point(10, 80);

			radio21.Location = new Point(10, 20);
			radio22.Location = new Point(10, 40);
			radio23.Location = new Point(10, 60);
			radio24.Location = new Point(10, 80);

			radio31.Location = new Point(10, 20);
			radio32.Location = new Point(10, 40);
			radio33.Location = new Point(10, 60);
			radio34.Location = new Point(10, 80);

			text1.Size = new Size(150, text1.PreferredHeight);

			radio11.Size = new Size(150, 20);
			radio12.Size = new Size(150, 20);
			radio13.Size = new Size(150, 20);
			radio14.Size = new Size(150, 20);

			radio21.Size = new Size(150, 20);
			radio22.Size = new Size(150, 20);
			radio23.Size = new Size(150, 20);
			radio24.Size = new Size(150, 20);

			radio31.Size = new Size(150, 20);
			radio32.Size = new Size(150, 20);
			radio33.Size = new Size(150, 20);
			radio34.Size = new Size(150, 20);

			group1.Controls.Add(text1);

			group1.Controls.Add(radio11);
			group1.Controls.Add(radio12);
			group1.Controls.Add(radio13);
			group1.Controls.Add(radio14);

			group2.Controls.Add(radio21);
			group2.Controls.Add(radio22);
			group2.Controls.Add(radio23);
			group2.Controls.Add(radio24);

			group3.Controls.Add(radio31);
			group3.Controls.Add(radio32);
			group3.Controls.Add(radio33);
			group3.Controls.Add(radio34);

			form.Controls.Add(group1);
			form.Controls.Add(group2);
			group2.Controls.Add(group3);

			// Perform some tests, the TabIndex stuff below will alter the outcome
			Assert.AreEqual(null, Helper.TestControl(group2, radio34, true), "Tab8");
			Assert.AreEqual(31, group2.TabIndex, "Tab9");

			// Does the taborder of containers and non-selectable things change behaviour?
			label1.TabIndex = 5;
			label2.TabIndex = 4;
			group1.TabIndex = 3;
			group2.TabIndex = 1;

			// Start verification
			Assert.AreEqual(null, Helper.TestControl(group2, radio34, true), "Tab10");
			Assert.AreEqual(radio24.Text, Helper.TestControl(group2, group2, true), "Tab11");
			Assert.AreEqual(radio31.Text, Helper.TestControl(group2, group3, true), "Tab12");
			Assert.AreEqual(null, Helper.TestControl(group1, radio14, true), "Tab13");
			Assert.AreEqual(radio23.Text, Helper.TestControl(group2, radio24, true), "Tab14");
			Assert.AreEqual(group3.Text, Helper.TestControl(group2, radio21, true), "Tab15");
			Assert.AreEqual(radio13.Text, Helper.TestControl(form, radio12, true), "Tab16");
			Assert.AreEqual(label2.Text, Helper.TestControl(form, radio14, true), "Tab17");
			Assert.AreEqual(group1.Text, Helper.TestControl(form, radio34, true), "Tab18");
			Assert.AreEqual(radio23.Text, Helper.TestControl(group2, radio24, true), "Tab19");

			// Sanity checks
			Assert.AreEqual(null, Helper.TestControl(radio11, radio21, true), "Tab20");
			Assert.AreEqual(text1.Text, Helper.TestControl(group1, radio21, true), "Tab21");

			Assert.AreEqual(radio14.Text, Helper.TestControl(form, label2, false), "Tab22");
			Assert.AreEqual(radio21.Text, Helper.TestControl(group2, group3, false), "Tab23");

			Assert.AreEqual(4, radio21.TabIndex, "Tab24");
			Assert.AreEqual(1, radio11.TabIndex, "Tab25");
			Assert.AreEqual(3, radio13.TabIndex, "Tab26");
			Assert.AreEqual(35, group3.TabIndex, "Tab27");
			Assert.AreEqual(1, group2.TabIndex, "Tab28");

			Assert.AreEqual(label1.Text, Helper.TestControl(form, form, false), "Tab29");
			Assert.AreEqual(radio14.Text, Helper.TestControl(group1, group1, false), "Tab30");
			Assert.AreEqual(radio34.Text, Helper.TestControl(group3, group3, false), "Tab31");

			Assert.AreEqual(null, Helper.TestControl(label1, label1, false), "Tab31");
			Assert.AreEqual(null, Helper.TestControl(radio11, radio21, false), "Tab32");
			form.Dispose ();
		}

		[Test]
		public void ScaleTest()
		{
			Control r1 = new Control();

			r1.Width = 40;
			r1.Height = 20;
			r1.Scale(2);
			Assert.AreEqual(80, r1.Width, "Scale1");
			Assert.AreEqual(40, r1.Height, "Scale2");
		}

#if NET_2_0
		[Test]
		public void ScaleChildrenTest ()
		{
			ScaleChildrenControl c = new ScaleChildrenControl ();
			Assert.AreEqual (true, c.PublicScaleChildren, "A1");
		}
		
		private class ScaleChildrenControl : Control
		{
			public bool PublicScaleChildren {
				get { return base.ScaleChildren; }
			}
		}
		
		[Test]
		public void ScaleControlTest ()
		{
			ScaleControl c = new ScaleControl ();
			
			c.Location = new Point (5, 10);
			c.Size = new Size (15, 20);
			
			Assert.AreEqual (new Rectangle (5, 10, 15, 20), c.Bounds, "A1");

			c.PublicScaleControl (new SizeF (1.5f, 1.3f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (8, 13, 22, 26), c.Bounds, "A2");

			c.PublicScaleControl (new SizeF (2f, 1.5f), BoundsSpecified.Location);
			Assert.AreEqual (new Rectangle (16, 20, 22, 26), c.Bounds, "A3");

			c.PublicScaleControl (new SizeF (1.5f, 2f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (16, 20, 33, 52), c.Bounds, "A4");

			c.PublicScaleControl (new SizeF (1.5f, 1.5f), BoundsSpecified.Width);
			Assert.AreEqual (new Rectangle (16, 20, 50, 52), c.Bounds, "A5");

			c.PublicScaleControl (new SizeF (1.5f, 1.3f), BoundsSpecified.None);
			Assert.AreEqual (new Rectangle (16, 20, 50, 52), c.Bounds, "A6");
			
			// Test with ScaleChildren
			c = new ScaleControl ();

			c.Location = new Point (5, 10);
			c.Size = new Size (50, 50);
			
			Control c2 = new Control ();
			c2.Location = new Point (15, 15);
			c2.Size = new Size (25, 25);
			c.Controls.Add (c2);

			Assert.AreEqual (new Rectangle (5, 10, 50, 50), c.Bounds, "B1");
			Assert.AreEqual (new Rectangle (15, 15, 25, 25), c2.Bounds, "B2");

			c.scale_children = false;

			c.PublicScaleControl (new SizeF (2f, 2f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (10, 20, 100, 100), c.Bounds, "B3");
			Assert.AreEqual (new Rectangle (15, 15, 25, 25), c2.Bounds, "B4");

			c.scale_children = true;

			// Will not scale children in ScaleControl
			c.PublicScaleControl (new SizeF (2f, 2f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (20, 40, 200, 200), c.Bounds, "B5");
			Assert.AreEqual (new Rectangle (15, 15, 25, 25), c2.Bounds, "B6");
			
			// Does scale children in Scale
			c.Scale (new SizeF (2f, 2f));
			Assert.AreEqual (new Rectangle (40, 80, 400, 400), c.Bounds, "B7");
			Assert.AreEqual (new Rectangle (30, 30, 50, 50), c2.Bounds, "B8");
		}
		
		[Test]
		public void GetScaledBoundsTest ()
		{
			ScaleControl c = new ScaleControl ();
			
			Rectangle r = new Rectangle (10, 20, 30, 40);

			Assert.AreEqual (new Rectangle (20, 10, 60, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A1");
			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A2");
			Assert.AreEqual (new Rectangle (10, 20, 60, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A3");
			Assert.AreEqual (new Rectangle (10, 20, 30, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A4");
			Assert.AreEqual (new Rectangle (20, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A5");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A6");
			
			c.PublicSetTopLevel (true);

			Assert.AreEqual (new Rectangle (10, 20, 60, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A7");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A8");
			Assert.AreEqual (new Rectangle (10, 20, 60, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A9");
			Assert.AreEqual (new Rectangle (10, 20, 30, 20), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A10");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A11");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A12");

			c = new ScaleControl ();
			c.PublicSetStyle (ControlStyles.FixedHeight, true);
			c.PublicSetStyle (ControlStyles.FixedWidth, true);

			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.All), "A13");
			Assert.AreEqual (new Rectangle (20, 10, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Location), "A14");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Size), "A15");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.Height), "A16");
			Assert.AreEqual (new Rectangle (20, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.X), "A17");
			Assert.AreEqual (new Rectangle (10, 20, 30, 40), c.PublicGetScaledBounds (r, new SizeF (2f, .5f), BoundsSpecified.None), "A18");
		}
		
		private class ScaleControl : Control
		{
			public bool scale_children = true;
			
			public void PublicScaleControl (SizeF factor, BoundsSpecified specified)
			{
				base.ScaleControl (factor, specified);
			}

			public Rectangle PublicGetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
			{
				return base.GetScaledBounds (bounds, factor, specified);
			}
			
			public void PublicSetStyle (ControlStyles flag, bool value)
			{
				base.SetStyle (flag, value);
			}
			
			public void PublicSetTopLevel (bool value)
			{
				base.SetTopLevel (value);
			}
			
			protected override bool ScaleChildren {
				get { return scale_children; }
			}
		}
#endif

		[Test]	// Bug #347282
		public void ScaleHierarchy ()
		{
			Form f = new Form ();
			Panel p = new Panel ();
			Button b = new Button ();
			
			f.ClientSize = new Size (300, 300);

			f.Controls.Add (p);
			p.Controls.Add (b);
			
			f.AutoScaleBaseSize = new Size (3, 11);
			f.Show ();
			
			// Due to font differences, all we can guarantee is that
			// the button is larger that the default.
			Assert.IsTrue (b.Width > 75, "A1");
			Assert.IsTrue (b.Height > 23, "A2");
			
			f.Dispose ();
		}
		
		class TestWindowTarget : IWindowTarget
		{
			public void OnHandleChange (IntPtr newHandle) {
			}

			public void OnMessage (ref Message m) {
			}
		}

		[Test]
		public void WindowTargetTest()
		{
			Control c = new Control ();
			Assert.IsNotNull (c.WindowTarget, "WindowTarget1");
			c.WindowTarget = null;
			Assert.IsNull (c.WindowTarget, "WindowTarget2");

			IWindowTarget existing_target = c.WindowTarget;
			IWindowTarget new_target = new TestWindowTarget ();
			c.WindowTarget = new_target;
			Assert.AreSame (new_target, c.WindowTarget, "WindowTarget3");
			
			TestHelper.RemoveWarning (existing_target);
		}

		[Test]
		public void TextTest()
		{
			Control r1 = new Control();
			r1.Text = "Hi" ;
			Assert.AreEqual("Hi" , r1.Text , "Text1");

			r1.ResetText();
			Assert.AreEqual("" , r1.Text , "Text2");
		}

		[Test]
		public void PubMethodTest7()
		{
			Control r1 = new Control();
			r1.RightToLeft = RightToLeft.Yes ;
			r1.ResetRightToLeft() ;
			Assert.AreEqual(RightToLeft.No , r1.RightToLeft , "#81");
			r1.ImeMode = ImeMode.Off ;
			r1.ResetImeMode () ;
			Assert.AreEqual(ImeMode.NoControl , r1.ImeMode , "#82");
			r1.ForeColor= SystemColors.GrayText ;
			r1.ResetForeColor() ;
			Assert.AreEqual(SystemColors.ControlText , r1.ForeColor , "#83");
			//r1.Font = Font.FromHdc();
			r1.ResetFont () ;
			//Assert.AreEqual(FontFamily.GenericSansSerif , r1.Font , "#83");
			r1.Cursor = Cursors.Hand ;
			r1.ResetCursor () ;
			Assert.AreEqual(Cursors.Default , r1.Cursor , "#83");
			//r1.DataBindings = System.Windows.Forms.Binding ;
			//r1.ResetBindings() ;
			//Assert.AreEqual(ControlBindingsCollection , r1.DataBindings  , "#83");
			r1.BackColor = Color.Black ;
			r1.ResetBackColor() ;
			Assert.AreEqual( SystemColors.Control , r1.BackColor  , "#84");
			r1.BackColor = Color.Black ;
			r1.Refresh() ;
			Assert.AreEqual( null , r1.Region , "#85");
			Rectangle M = new Rectangle(10, 20, 30 ,40);
			r1.RectangleToScreen(M) ;
			Assert.AreEqual( null , r1.Region , "#86");
		}

		[Test]
		public void ScreenClientCoords()
		{
			Label l;
			Point p1;
			Point p2;
			Point p3;

			l = new Label();
			l.Left = 10;
			l.Top  = 12;
			l.Visible = true;
			p1 = new Point (10,10);
			p2 = l.PointToScreen(p1);
			p3 = l.PointToClient(p2);

			Assert.AreEqual (p1, p3, "SC1");
		}

		[Test]
		public void ContainsTest ()
		{
			Control t = new Control ();
			Control s = new Control ();

			t.Controls.Add (s);

			Assert.AreEqual (true, t.Contains (s), "Con1");
			Assert.AreEqual (false, s.Contains (t), "Con2");
			Assert.AreEqual (false, s.Contains (null), "Con3");
			Assert.AreEqual (false, t.Contains (new Control ()), "Con4");
		}

		[Test]
		public void IsHandleCreated_NotVisible ()
		{
			Control c = new Control ();
			c.Visible = false;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (c);
			form.Show ();

			Assert.IsFalse (c.IsHandleCreated, "#1");
			c.Visible = true;
			Assert.IsTrue (c.IsHandleCreated, "#2");
			c.Visible = false;
			Assert.IsTrue (c.IsHandleCreated, "#3");
			form.Close ();
		}

		class OnCreateControlTest : Control {
			public bool reached = false;
			protected override void OnCreateControl () {
				reached = true;
			}
		}

		[Test]
		public void CreateControlVisibleTest ()
		{
			OnCreateControlTest test = new OnCreateControlTest ();
			test.Visible = false;
			Assert.IsFalse (test.IsHandleCreated, "0");
			Assert.IsFalse (test.Visible, "1");
			test.Visible = true;
			Assert.IsTrue (test.Visible, "2");
			Assert.IsTrue (test.reached, "3");
		}


		[Test]
		public void CreateGraphicsTest ()
		{
			Graphics g = null;
			Pen p = null;

			try {
				Control c = new Control ();
				c.SetBounds (0,0, 20, 20);
				g = c.CreateGraphics ();
				Assert.IsNotNull (g, "Graph1");
			} finally {
				if (p != null)
					p.Dispose ();
				if (g != null)
					g.Dispose ();
			}
		}

		bool delegateCalled = false;
		public delegate void TestDelegate ();

		public void delegate_call () {
			delegateCalled = true;

			TestHelper.RemoveWarning (delegateCalled);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void InvokeException1 () {
			Control c = new Control ();
			IAsyncResult result;

			result = c.BeginInvoke (new TestDelegate (delegate_call));
			c.EndInvoke (result);
		}

		[Test]
		public void FindFormTest () {
			Form f = new Form ();

			f.ShowInTaskbar = false;
			f.Name = "form";
			Control c = null;

			try {
				f.Controls.Add (c = new Control ());
				Assert.AreEqual (f.Name, c.FindForm ().Name, "Find1");

				f.Controls.Remove (c);

				GroupBox g = new GroupBox ();
				g.Name = "box";
				f.Controls.Add (g);
				g.Controls.Add (c);

				Assert.AreEqual (f.Name, f.FindForm ().Name, "Find2");

				g.Controls.Remove (c);
				Assert.IsNull(c.FindForm (), "Find3");

			} finally {
				if (c != null)
					c.Dispose ();
				if (f != null)
					f.Dispose ();
			}
		}

		[Test]
		public void FocusTest ()
		{
			Form f = null;
			Button c = null, d = null;

			try {
				f = new Form ();
				f.ShowInTaskbar = false;
				f.Visible = true;
				c = new Button ();
				c.Visible = true;
				f.Controls.Add (c);

				d = new Button ();
				d.Visible = false;
				f.Controls.Add (d);

				Assert.IsTrue (c.CanFocus, "Focus1");
				Assert.IsFalse (c.Focused, "Focus2");
				c.Focus ();
				Assert.IsTrue (c.Focused, "Focus3");
				d.Focus ();
				Assert.IsFalse (d.Focused, "Focus4");

				d.Visible = true;
				d.Focus ();
				Assert.IsTrue (d.Focused, "Focus5");
				Assert.IsFalse (c.Focused, "Focus6");

				c.Enabled = false;
				Assert.IsFalse (c.Focused, "Focus7");
			} finally {
				if (f != null)
					f.Dispose ();
				if (c != null)
					c.Dispose ();
				if (d != null)
					d.Dispose ();
			}
		}

		[Test]
		public void FromHandleTest ()
		{
			Control c1 = null;
			Control c2 = null;

			try {
				c1 = new Control ();
				c2 = new Control ();

				c1.Name = "parent";
				c2.Name = "child";
				c1.Controls.Add(c2);

				// Handle
				Assert.AreEqual (c1.Name, Control.FromHandle (c1.Handle).Name, "Handle1");
				Assert.IsNull (Control.FromHandle (IntPtr.Zero), "Handle2");

				// ChildHandle
				Assert.AreEqual (c1.Name, Control.FromChildHandle (c1.Handle).Name, "Handle3");
				Assert.IsNull (Control.FromChildHandle (IntPtr.Zero), "Handle4");


			} finally {
				if (c1 != null)
					c1.Dispose ();

				if (c2 != null)
					c2.Dispose ();
			}
		}

		[Test]
		public void GetChildAtPointTest ()
		{
			Control c = null, d = null;
			TransparentControl e = null;

			try {
				c = new Control ();
				c.Name = "c1";
				c.SetBounds (0, 0, 100, 100);

				d = new Control ();
				d.Name = "d1";
				d.SetBounds (10, 10, 40, 40);
				c.Controls.Add (d);

				e = new TransparentControl ();
				e.Name = "e1";
				e.SetBounds (55, 55, 10, 10);

				Control l = c.GetChildAtPoint (new Point (15, 15));
				Assert.AreEqual (d.Name, l.Name, "Child1");
				Assert.IsFalse (e.Name == l.Name, "Child2");

				l = c.GetChildAtPoint (new Point (57, 57));
				Assert.AreEqual (null, l, "Child3");

				l = c.GetChildAtPoint (new Point (10, 10));
				Assert.AreEqual (d.Name, l.Name, "Child4");

				// GetChildAtPointSkip is not implemented and the following test is breaking for Net_2_0 profile
#if NET_2_0
				c.Controls.Add (e);
				e.Visible = false;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
				Assert.IsNull (l, "Child5");

				e.Visible = true;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Invisible);
				Assert.AreSame (e.Name, l.Name, "Child6");

				e.Enabled = false;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Disabled);
				Assert.IsNull (l, "Child7");

				e.Enabled = true;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Disabled);
				Assert.AreSame (e.Name, l.Name, "Child8");

				
				e.BackColor = Color.Transparent;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Transparent);
				Assert.IsNull (l, "Child9");

				e.BackColor = Color.Green;
				l = c.GetChildAtPoint (new Point (57, 57), GetChildAtPointSkip.Transparent);
				Assert.AreSame (e.Name, l.Name, "Child10");

#endif // NET_2_0
			} finally {
				if (c != null)
					c.Dispose ();
				if (d != null)
					d.Dispose ();
			}
		}

		private class TransparentControl : Control
		{
			public TransparentControl ()
			{
				SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			}

			protected override CreateParams CreateParams
			{
				get
				{
					CreateParams cp = base.CreateParams;
					cp.ExStyle |= 0x00000020;
					return cp;
				}
			}
		}
		
		[Test]
		public void ResetFontTest ()
		{
			Control c = new Control ();
			c.Font = new Font (c.Font.FontFamily, 3, FontStyle.Italic);
			c.ResetFont ();

			Assert.IsFalse (c.Font.Bold, "#1");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.FontFamily.Name, "#2");
			Assert.IsFalse (c.Font.Italic, "#3");
			//Assert.AreEqual ("Microsoft Sans Serif", c.Font.Name, "#4");
			Assert.AreEqual (8.25, c.Font.Size, "#5");
			Assert.AreEqual (8.25, c.Font.SizeInPoints, "#6");
			Assert.IsFalse (c.Font.Strikeout, "#7");
			Assert.IsFalse (c.Font.Underline, "#8");
			Assert.AreEqual (GraphicsUnit.Point, c.Font.Unit, "#9");
#if NET_2_0
			Assert.AreEqual (true, c.Font.IsSystemFont, "#10");
#endif
		}

		public class LayoutTestControl : Control {
			public int LayoutCount;

			public LayoutTestControl () : base() {
				LayoutCount = 0;
			}

			protected override void OnLayout(LayoutEventArgs levent) {
				LayoutCount++;
				base.OnLayout (levent);
			}
		}

		[Test]
		public void LayoutTest() {
			LayoutTestControl c;

			c = new LayoutTestControl();

			c.SuspendLayout();
			c.SuspendLayout();
			c.SuspendLayout();
			c.SuspendLayout();

			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.ResumeLayout(true);
			c.PerformLayout();
			c.SuspendLayout();
			c.PerformLayout();

			Assert.AreEqual(5, c.LayoutCount, "Layout Suspend/Resume locking does not bottom out at 0");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TransparentBackgroundTest1() {
			Control	c;

			c = new Control();
			c.BackColor = Color.Transparent;
		}

		[Test]
		public void TransparentBackgroundTest2() {
			Panel	c;

			c = new Panel();
			c.BackColor = Color.Transparent;
			Assert.AreEqual(Color.Transparent, c.BackColor, "Transparent background not set");
		}

		[Test]
		public void TransparentBackgroundTest3() {
			Control	c;

			c = new Control();
			c.BackColor = Color.Empty;
			Assert.AreEqual(Control.DefaultBackColor, c.BackColor, "Setting empty color failed");
		}

		[Test]
		public void Dock_Value_Invalid ()
		{
			Control c = new Control ();
			try {
				c.Dock = (DockStyle) 666;
				Assert.Fail ("#1");
			} catch (InvalidEnumArgumentException ex) {
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void EnabledTest1() {
			Control	child;
			Control	parent;
			Control	grandma;

			grandma = new Control();
			parent = new Control();
			child = new Control();

			grandma.Controls.Add(parent);
			parent.Controls.Add(child);
			grandma.Enabled = false;
			Assert.AreEqual(grandma.Enabled, child.Enabled, "Child did not inherit disabled state");
		}

		int EnabledCalledCount = 0;
		private void EnabledTest2EnabledChanged(object sender, EventArgs e) {
			EnabledCalledCount++;
		}

		[Test]
		public void EnabledTest2() {
			// Check nesting of enabled calls
			// OnEnabled is not called for disabled child controls
			Control	child;
			Control	parent;
			Control	grandma;

			EnabledCalledCount = 0;

			grandma = new Control();
			parent = new Control();
			child = new Control();
			child.EnabledChanged += new EventHandler(EnabledTest2EnabledChanged);

			grandma.Controls.Add(parent);
			parent.Controls.Add(child);
			grandma.Enabled = false;

			Assert.AreEqual(1, EnabledCalledCount, "Child Enabled Event not properly fired");
			grandma.Enabled = true;
			Assert.AreEqual(2, EnabledCalledCount, "Child Enabled Event not properly fired");
			child.Enabled = false;
			grandma.Enabled = false;
			Assert.AreEqual(3, EnabledCalledCount, "Child Enabled Event not properly fired");
		}

		[Test]
		public void ControlsRemoveNullTest ()
		{
			Control c = new Control ();
			c.Controls.Remove (null);
		}

		[Test]
		public void ControlsAddNullTest ()
		{
			Control c = new Control ();
			c.Controls.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ControlsSetChildIndexNullTest ()
		{
			Control c = new Control ();
			c.Controls.SetChildIndex (null, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ControlsAddRangeNullTest ()
		{
			Control c = new Control ();
			c.Controls.AddRange (null);
		}

		[Test]
		public void ControlsAddRangeNullElementTest ()
		{
			Control c = new Control ();
			Control[] subcontrols = new Control[2];
			subcontrols[0] = new Control ();
			subcontrols[1] = null;

			c.Controls.AddRange (subcontrols);
		}

		[Test]
		public void RegionTest () {
			Form f = new Form ();
			f.ShowInTaskbar = false;
			Control c = new Control ();
			f.Controls.Add (c);
			Assert.IsNull (c.Region, "#A1");
			f.Show ();
			Assert.IsNull (c.Region, "#A2");
			c.Region = null;
			Assert.IsNull (c.Region, "#A3");
			f.Dispose ();

			Region region = new Region ();
			f = new Form ();
			f.ShowInTaskbar = false;
			c = new Control ();
			f.Controls.Add (c);
			c.Region = region;
			Assert.IsNotNull (c.Region, "#B1");
			Assert.AreSame (region, c.Region, "#B2");
			f.Show ();
			c.Region = null;
			Assert.IsNull (c.Region, "#B3");

			f.Dispose ();
		}

		[Test] // bug #330501
		public void OnValidating_Parent_Close ()
		{
			MockControl control = new MockControl ();

			Form f = new Form ();
			f.Controls.Add (control);
			f.ShowInTaskbar = false;

			f.Show ();
			Assert.AreEqual (0, control.OnValidatingCount, "#A1");
			f.Close ();
			Assert.AreEqual (1, control.OnValidatingCount, "#A2");
			f.Dispose ();
		}

		[Test] // bug #80280
		public void Validated_Multiple_Containers ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			UserControl control1 = new UserControl();
			UserControl container1 = new UserControl();
			control1.Tag = true;
			control1.Validated += new EventHandler (Control_ValidatedHandler);
			container1.Controls.Add(control1);
			form.Controls.Add (container1);

			UserControl container2 = new UserControl();
			UserControl control2 = new UserControl();
			container2.Controls.Add(control2);
			form.Controls.Add (container2);

			Assert.IsTrue ((bool) control1.Tag, "#1");
			control1.Select();
			Assert.IsTrue ((bool) control1.Tag, "#2");
			control2.Select();
			Assert.IsFalse ((bool) control1.Tag, "#3");

			form.Dispose ();
		}

		private void Control_ValidatedHandler (object sender, EventArgs e)
		{
			((Control) sender).Tag = false;
		}
		
		[Test]
		public void ControlReparentLocationTest ()
		{
			Form form = new Form ();
			Label l = new Label ();
			l.Location = new Point (0, 0);
			form.Controls.Add (l);
			form.Show ();
			Assert.AreEqual (0, l.Left, "#A1");
			Assert.AreEqual (0, l.Top, "#A2");
			form.Hide ();
			form.Controls.Remove (l);
			form.Show ();
			form.Controls.Add (l);
			Assert.AreEqual (0, l.Left, "#A3");
			Assert.AreEqual (0, l.Top, "#A4");
			
			form.Dispose ();
		}

#if NET_2_0
		[Test]
		public void UseWaitCursorTest ()
		{
			Control c = new Control ();
			Assert.IsFalse (c.UseWaitCursor, "#1");
			c.Cursor = Cursors.Hand;
			c.UseWaitCursor = true;
			Assert.AreEqual (Cursors.WaitCursor, c.Cursor, "#2");
			c.UseWaitCursor = false;
			Assert.AreEqual (Cursors.Hand, c.Cursor, "#3");
			
			c.UseWaitCursor = true;
			c.Cursor = Cursors.Help;
			Assert.AreEqual (Cursors.WaitCursor, c.Cursor, "#4");
			Assert.AreEqual (true, c.UseWaitCursor, "#5");
			
			c.UseWaitCursor = false;
			Assert.AreEqual (Cursors.Help, c.Cursor, "#6");
		}

		[Test] // bug #80621, #81125
		public void DontCallSizeFromClientSize ()
		{
			SizeControl sc = new SizeControl ();
			
			Assert.AreEqual (0, sc.size_from_client_size_count, "A1");
			
			sc.ClientSize = new Size (300, 300);
			Assert.AreEqual (0, sc.size_from_client_size_count, "A2");
			
			SizeForm sf = new SizeForm ();
			sf.ShowInTaskbar = false;
			sf.Show ();
			
			Assert.AreEqual (0, sc.size_from_client_size_count, "A3");

			sc.ClientSize = new Size (300, 300);
			Assert.AreEqual (0, sc.size_from_client_size_count, "A4");	
			
			sf.Dispose ();	
		}
		
		private class SizeControl : Control
		{
			public int size_from_client_size_count = 0;
			
			protected override Size SizeFromClientSize (Size clientSize)
			{
				size_from_client_size_count++;
				return base.SizeFromClientSize (clientSize);
			}
		}

		private class SizeForm : Form
		{
			public int size_from_client_size_count = 0;

			protected override Size SizeFromClientSize (Size clientSize)
			{
				size_from_client_size_count++;
				return base.SizeFromClientSize (clientSize);
			}
		}
#endif

		public class MockControl : Control
		{
			public int OnValidatingCount;
			public int OnSizeChangedCount;
			public int OnResizeCount;
			public int OnLocationChangedCount;

			public MockControl ()
			{
			}

			public MockControl (string text)
				: base (text)
			{
			}

			public MockControl (Control parent, string text)
				: base (parent, text)
			{
			}

			public MockControl (string text, int left, int top, int width, int height)
				: base (text, left, top, width, height)
			{
			}

			public MockControl (Control parent, string text, int left, int top, int width, int height)
				: base (parent, text, left, top, width, height)
			{
			}

			public int font_height
			{
				get { return base.FontHeight; }
				set { base.FontHeight = value; }
			}

			protected override void OnLocationChanged (EventArgs e)
			{
				OnLocationChangedCount++;
				base.OnLocationChanged (e);
			}

			protected override void OnSizeChanged (EventArgs e)
			{
				OnSizeChangedCount++;
				base.OnSizeChanged (e);
			}

			protected override void OnResize (EventArgs e)
			{
				OnResizeCount++;
				base.OnResize (e);
			}

			protected override void OnValidating (CancelEventArgs e)
			{
				OnValidatingCount++;
			}
		}

		const int WM_KEYDOWN = 0x0100;
		const int WM_CHAR = 0x0102;
		const int WM_SYSCHAR = 0x0106;
		const int WM_KEYUP = 0x0101;

#if NET_2_0
		[Test]
		public void MethodPreProcessControlMessage ()
		{
			bool testing_callstack = false;

			MyControl c = new MyControl ();
			Message m = new Message ();
			m.HWnd = c.Handle;
			m.Msg = WM_KEYDOWN;
			m.WParam = (IntPtr)Keys.Down;
			m.LParam = IntPtr.Zero;

			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A1");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.OnPreviewKeyDown);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNeeded, c.PreProcessControlMessage (ref m), "A2");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessCmdKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageProcessed, c.PreProcessControlMessage (ref m), "A3");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNeeded, c.PreProcessControlMessage (ref m), "A4");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageProcessed, c.PreProcessControlMessage (ref m), "A5");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_CHAR;
			c.SetState (State.None);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A6");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNeeded, c.PreProcessControlMessage (ref m), "A7");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageProcessed, c.PreProcessControlMessage (ref m), "A8");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_SYSCHAR;
			c.SetState (State.None);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A9");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNeeded, c.PreProcessControlMessage (ref m), "A10");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageProcessed, c.PreProcessControlMessage (ref m), "A11");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_KEYUP;
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A12");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.OnPreviewKeyDown);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A13");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessCmdKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A14");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A15");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (PreProcessControlState.MessageNotNeeded, c.PreProcessControlMessage (ref m), "A16");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());
		}
#endif

		[Test]
		public void MethodPreProcessMessage ()
		{
			bool testing_callstack = false;

			MyControl c = new MyControl ();
			Message m = new Message ();
			m.HWnd = c.Handle;
			m.Msg = WM_KEYDOWN;
			m.WParam = (IntPtr)Keys.Down;
			m.LParam = IntPtr.Zero;

			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A1");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.OnPreviewKeyDown);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A2");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessCmdKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (true, c.PreProcessMessage (ref m), "A3");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A4");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (true, c.PreProcessMessage (ref m), "A5");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_CHAR;
			c.SetState (State.None);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A6");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A7");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (true, c.PreProcessMessage (ref m), "A8");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_SYSCHAR;
			c.SetState (State.None);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A9");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A10");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogChar);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (true, c.PreProcessMessage (ref m), "A11");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());


			m.Msg = WM_KEYUP;
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A12");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.OnPreviewKeyDown);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A13");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessCmdKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A14");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.IsInputKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A15");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());

			c.SetState (State.ProcessDialogKey);
			if (testing_callstack) Console.WriteLine ("Start");
			Assert.AreEqual (false, c.PreProcessMessage (ref m), "A16");
			if (testing_callstack) Console.WriteLine ("End {0}\n", m.WParam.ToString ());
		}
		private enum State
		{
			None,
			ProcessCmdKey,
			OnPreviewKeyDown,
			IsInputChar,
			IsInputKey,
			PreProcessMessage,
			ProcessDialogKey,
			ProcessDialogChar
		}

		private class MyControl : Control
		{

			private State current_state;
			bool testing_callstack = false;

			public void SetState (State state)
			{
				current_state = state;
			}

			protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
			{
				if (testing_callstack) Console.Write ("ProcessCmdKey[");
				if (current_state == State.ProcessCmdKey) {
					if (testing_callstack) Console.WriteLine ("]");
					return true;
				}

				bool retval = base.ProcessCmdKey (ref msg, keyData);
				if (testing_callstack) Console.WriteLine ("]");
				return retval;
			}

#if NET_2_0
			protected override void OnPreviewKeyDown (PreviewKeyDownEventArgs e)
			{
				if (testing_callstack) Console.Write ("OnPreviewKeyDown[");
				if (current_state == State.OnPreviewKeyDown) {
					e.IsInputKey = true;
					if (testing_callstack) Console.WriteLine ("]");
					return;
				}

				base.OnPreviewKeyDown (e);
				if (testing_callstack) Console.WriteLine ("]");
			}
#endif

			protected override bool IsInputChar (char charCode)
			{
				if (testing_callstack) Console.Write ("IsInputChar[");
				if (current_state == State.IsInputChar) {
					if (testing_callstack) Console.WriteLine ("true]");
					return true;
				}

				bool retval = base.IsInputChar (charCode);
				if (testing_callstack) Console.WriteLine ("{0}]", retval.ToString ());
				return retval;
			}

			protected override bool IsInputKey (Keys keyData)
			{
				if (testing_callstack) Console.Write ("IsInputKey[");
				if (current_state == State.IsInputKey) {
					if (testing_callstack) Console.WriteLine ("]");
					return true;
				}

				bool retval = base.IsInputKey (keyData);
				if (testing_callstack) Console.WriteLine ("]");
				return retval;
			}

			public override bool PreProcessMessage (ref Message msg)
			{
				if (testing_callstack) Console.Write ("PreProcessMessage[");
				if (current_state == State.PreProcessMessage) {
					if (testing_callstack) Console.WriteLine ("]");
					return true;
				}

				bool retval = base.PreProcessMessage (ref msg);
				if (testing_callstack) Console.WriteLine ("]");
				return retval;
			}

			protected override bool ProcessDialogKey (Keys keyData)
			{
				if (testing_callstack) Console.Write ("ProcessDialogKey[");
				if (current_state == State.ProcessDialogKey) {
					if (testing_callstack) Console.WriteLine ("]");
					return true;
				}

				bool retval = base.ProcessDialogKey (keyData);
				if (testing_callstack) Console.WriteLine ("]");
				return retval;
			}

			protected override bool ProcessDialogChar (char charCode)
			{
				if (testing_callstack) Console.Write ("ProcessDialogChar[");
				if (current_state == State.ProcessDialogChar) {
					if (testing_callstack) Console.WriteLine ("]");
					return true;
				}

				bool retval = base.ProcessDialogChar (charCode);
				if (testing_callstack) Console.WriteLine ("]");
				return retval;
			}
		}
		
		[Test]
		public void MethodIsInputChar ()
		{
			// Basically, show that this method always returns false
			InputCharControl m = new InputCharControl ();
			bool result = false;
			
			for (int i = 0; i < 256; i++)
				result |= m.PublicIsInputChar ((char)i);
			
			Assert.AreEqual (false, result, "I1");
		}

		private class InputCharControl : Control
		{
			public bool PublicIsInputChar (char charCode)
			{
				return base.IsInputChar (charCode);
			}

		}

		[Test] // bug #81118, 81718
		public void VisibleTriggersLayout ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			Control c = new Control ();
			c.Visible = false;
			
			f.Controls.Add (c);
			
			c.Dock = DockStyle.Fill;
			c.Visible = true;
			
			Assert.AreEqual (f.ClientSize.Width, c.Width, "L1");
			
			f.Dispose ();
		}
		
		[Test]
		public void ResumeLayoutEffects ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.ClientSize = new Size (300, 300);
			
			Button button1 = new Button ();
			f.Controls.Add (button1);
			button1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
			button1.Location = new Point (f.ClientSize.Width - button1.Width, f.ClientSize.Height - button1.Height);

			f.Show ();
			
			Assert.AreEqual (new Point (225, 277), button1.Location, "A1");
			
			f.SuspendLayout ();
			f.Height += 10;
			f.ResumeLayout (false);
			f.PerformLayout ();

			Assert.AreEqual (new Point (225, 277), button1.Location, "A2");

			f.SuspendLayout ();
			f.Height += 10;
			f.ResumeLayout ();

			Assert.AreEqual (new Point (225, 287), button1.Location, "A3");
			f.Dispose ();
		}
		
		[Test]
		public void DisposeEnumerator ()
		{
			// We can modify the collection while looping through it without crashing
			Control c = new Control ();

			c.Controls.Add (new Control ());
			c.Controls.Add (new Control ());
			
			foreach (Control c1 in c.Controls)
				c.Controls.Remove (c1);
		}
		
		[Test]
		public void MethodSetBounds ()
		{
			Control myControl = new Control();
			myControl.SetBounds(10, 20, 30, 40);
			myControl.SetBounds(50, 60, 70, 70, BoundsSpecified.Location);

			Assert.AreEqual (new Rectangle (50, 60, 30, 40), myControl.Bounds, "A1");
		}
		
		[Test]
		public void Bug386450 ()
		{
			// Should not crash.  We have to check for the font object
			// being different, not just if they represent the same font.
			Form f = new Form ();
			Label l = new Label ();
			l.Text = "Hello";

			Font f1 = new Font ("Arial", 12);
			Font f2 = new Font ("Arial", 12);

			l.Font = f1;
			l.Font = f2;

			f1.Dispose ();

			f.Controls.Add (l);
			f.Show ();
			f.Dispose ();
		}

		[Test]
		public void DisplayRectangle_SamePadding ()
		{
			// The MSDN documentation says that for a control ClientRectangle == DisplayRectangle
			using (var c = new Control ())
			{
				c.Size = new Size (100, 100);
				c.Padding = new Padding (4);
				Assert.AreEqual (new Rectangle (0, 0, 100, 100), c.ClientRectangle);
				Assert.AreEqual (c.ClientRectangle, c.DisplayRectangle);
			}
		}
	}

	[TestFixture]
	public class ControlSetTopLevelTest : TestHelper
	{
		class ControlPoker : Control {
			public void DoSetTopLevel ()
			{
				SetTopLevel (true);
			}
			public bool DoGetTopLevel ()
			{
				return GetTopLevel ();
			}
		}

		[Test]
		public void TestControl ()
		{
			ControlPoker c = new ControlPoker ();
			c.Visible = false;
			c.DoSetTopLevel ();
			Assert.IsTrue (c.DoGetTopLevel (), "1");
			Assert.IsFalse (c.Visible, "2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestChildControl ()
		{
			Control c1 = new Control();
			ControlPoker c2 = new ControlPoker ();

			c1.Controls.Add (c2);
			c2.DoSetTopLevel ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestTopLevelAdd () {
			Form f = new Form();
			Form f1 = new Form();
			f.Controls.Add(f1);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestForm ()
		{
			Form f = new Form ();
			Assert.IsFalse (f.Visible, "3");
			f.TopLevel = true;
			Assert.IsFalse (f.Visible, "4");
			
			f.Dispose ();
		}
	}

	[TestFixture]
	public class ControlResizeLayoutTest : TestHelper
	{
		class ControlPoker : Control {
			public void DoOnResize ()
			{
				OnResize (EventArgs.Empty);
			}
		}

		int child_event;
		string child_affected_property;
		void ChildLayoutEvent (object sender, LayoutEventArgs e)
		{
			child_event ++;
			child_affected_property = e.AffectedProperty;
		}

		int parent_event;
		string parent_affected_property;
		void ParentLayoutEvent (object sender, LayoutEventArgs e)
		{
			parent_event ++;
			parent_affected_property = e.AffectedProperty;

			TestHelper.RemoveWarning (parent_affected_property);
		}

		[Test]
		public void Test ()
		{
			Panel p = new Panel ();
			ControlPoker c = new ControlPoker();

			p.Controls.Add (c);

			p.Layout += new LayoutEventHandler (ParentLayoutEvent);
			c.Layout += new LayoutEventHandler (ChildLayoutEvent);

			c.DoOnResize ();

			Assert.AreEqual (1, child_event, "1");
			Assert.AreEqual ("Bounds", child_affected_property, "2");

			Assert.AreEqual (0, parent_event, "3");
		}
		
	}

	[TestFixture]
	[Category ("NotWorking")]
	public class ControlInvokeTest  : TestHelper {

		[TearDown]
		protected override void TearDown ()
		{
			if (f != null && !f.IsDisposed)			
				f.Dispose ();
			base.TearDown ();
		}

		
		public delegate void TestDelegate ();

		Form f;
		Control c;
		Thread control_t;
		ApplicationContext control_context;
		bool delegateCalled = false;
		bool threadDied = false;

		object m;

		void CreateControl ()
		{
			try {
			f = new Form ();
			f.ShowInTaskbar = false;
			
			c = new Control ();

			f.Controls.Add (c);

			Console.WriteLine ("f.Handle = {0}", f.Handle);
			Console.WriteLine ("c.Handle = {0}", c.Handle);

			control_context = new ApplicationContext (f);

			Monitor.Enter (m);
			Console.WriteLine ("pulsing");
			Monitor.Pulse (m);
			Monitor.Exit (m);
			Console.WriteLine ("control thread running");
			Application.Run (control_context);
			c.Dispose ();
			Console.WriteLine ("dying");
			threadDied = true;
			Monitor.Enter (m);
			Console.WriteLine ("pulsing again");
			Monitor.Pulse (m);
			Monitor.Exit (m);
			} catch (Exception e) { Console.WriteLine (e); }
		}

		[Test]
		public void InvokeTest ()
		{
			m = new object ();

			control_t = new Thread(new ThreadStart(CreateControl));

			Monitor.Enter (m);

			control_t.Start ();

			Console.WriteLine ("waiting on monitor");
			Monitor.Wait (m);

			Console.WriteLine ("making async call");

			IAsyncResult result;
			result = c.BeginInvoke (new TestDelegate (delegate_call));
			c.EndInvoke (result);

			Assert.IsTrue (delegateCalled, "Invoke1");

			Monitor.Wait (m);
			Assert.IsTrue (threadDied, "Invoke2");
		}

		public void delegate_call () {
			try {
			/* invoked on control_context's thread */
			delegateCalled = true;
			f.Dispose ();
			Console.WriteLine ("calling Application.Exit");
			control_context.ExitThread ();
			} catch (Exception e) { Console.WriteLine (e); }
		}
		
	}

	[TestFixture]
	public class ControlWMTest : TestHelper
	{
		[Test]
		public void WM_PARENTNOTIFY_Test ()
		{
			WMTester tester;
			Control child;
			int child_handle;
			
			tester = new WMTester ();
			child = new Control ();
			tester.Controls.Add (child);
			
			tester.Visible = true;
			child.Visible = true;

			child_handle = child.Handle.ToInt32 ();

			ArrayList msgs;
			Message m1;
				
			msgs = tester.Find (WndMsg.WM_PARENTNOTIFY);
			
			Assert.AreEqual (1, msgs.Count, "#1");
			
			m1 = (Message) msgs [0];
			Assert.AreEqual (WndMsg.WM_CREATE, ((WndMsg) LowOrder (m1.WParam)),  "#2");
			//Assert.AreEqual (child.Identifier??, HighOrder (m1.WParam),  "#3");
			Assert.AreEqual (child_handle, m1.LParam.ToInt32 (),  "#4");

			child.Dispose ();

			msgs = tester.Find (WndMsg.WM_PARENTNOTIFY);
			Assert.AreEqual (2, msgs.Count, "#5");
			m1 = (Message) msgs [1];

			Assert.AreEqual (WndMsg.WM_DESTROY, ((WndMsg) LowOrder (m1.WParam)),  "#6");
			//Assert.AreEqual (child.Identifier??, HighOrder (m1.WParam),  "#7");
			Assert.AreEqual (child_handle, m1.LParam.ToInt32 (),  "#8");

			tester.Dispose ();
		}

		internal static int LowOrder (int param) 
		{
			return ((int)(short)(param & 0xffff));
		}

		internal static int HighOrder (int param) 
		{
			return ((int)(short)(param >> 16));
		}

		internal static int LowOrder (IntPtr param) 
		{
			return ((int)(short)(param.ToInt32 () & 0xffff));
		}

		internal static int HighOrder (IntPtr param) 
		{
			return ((int)(short)(param.ToInt32 () >> 16));
		}

		internal class WMTester : Form
		{
			internal ArrayList Messages = new ArrayList ();
			
			internal bool Contains (WndMsg msg)
			{
				return Contains (msg, Messages);
			}

			internal bool Contains (WndMsg msg, ArrayList list)
			{
				foreach (Message m in Messages) 
				{
					if (m.Msg == (int) msg)
						return true;
				}
				return false;
			}

			internal ArrayList Find (WndMsg msg)
			{
				ArrayList result = new ArrayList ();

				foreach (Message m in Messages)
				{
					if (m.Msg == (int) msg)
						result.Add (m);
				}
				return result;
			}

			protected override void WndProc(ref Message m)
			{
				Console.WriteLine ("WndProc: " + m.ToString ());
				Messages.Add (m);
				base.WndProc (ref m);
			}
		}
	}

#if NET_2_0
	[TestFixture]
	public class ControlLayoutTest : TestHelper
	{
		[SetUp]
		protected override void SetUp ()
		{
			_layoutCount = 0;
			base.SetUp ();
		}

		[Test] // bug #80456
		public void LayoutTest ()
		{
			MockLayoutEngine layoutEngine = new MockLayoutEngine ();
			MockControl c = new MockControl (layoutEngine);
			c.Layout += new LayoutEventHandler (LayoutEvent);
			Assert.IsFalse (layoutEngine.LayoutInvoked, "#A1");
			Assert.AreEqual (0, _layoutCount, "#A2");
			c.PerformLayout ();
			Assert.IsTrue (layoutEngine.LayoutInvoked, "#A3");
			Assert.AreEqual (1, _layoutCount, "#A4");

			layoutEngine.Reset ();
			c.OverrideOnLayout = true;
			Assert.IsFalse (layoutEngine.LayoutInvoked, "#B1");
			c.PerformLayout ();
			Assert.IsFalse (layoutEngine.LayoutInvoked, "#B2");
			Assert.AreEqual (1, _layoutCount, "#B3");
		}

		void LayoutEvent (object sender, LayoutEventArgs e)
		{
			_layoutCount++;
		}

		private int _layoutCount;

		class MockControl : Control
		{
			public MockControl (LayoutEngine layoutEngine)
			{
				_layoutEngine = layoutEngine;
			}

			public bool OverrideOnLayout
			{
				get { return _overrideOnLayout; }
				set { _overrideOnLayout = value; }
			}

			protected override void OnLayout (LayoutEventArgs levent)
			{
				if (!OverrideOnLayout)
					base.OnLayout (levent);
			}

			public override LayoutEngine LayoutEngine {
				get {
					if (_layoutEngine == null)
						return base.LayoutEngine;
					return _layoutEngine;
				}
			}

			private bool _overrideOnLayout;
			private LayoutEngine _layoutEngine;
		}

		class MockLayoutEngine : LayoutEngine
		{
			public bool LayoutInvoked {
				get { return _layoutInvoked; }
			}

			public void Reset ()
			{
				_layoutInvoked = false;
			}

			public override bool Layout (object container, LayoutEventArgs args)
			{
				_layoutInvoked = true;
				return true;
			}

			private bool _layoutInvoked;
		}
	}
#endif
}
