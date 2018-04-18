using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ControlPropertyEventsTest : TestHelper
	{
		[Test]
		public void PropertyAllowDrop ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.AllowDrop = true;
			Assert.AreEqual (true, c.AllowDrop, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			c.AllowDrop = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAnchor ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Anchor = AnchorStyles.Bottom;
			Assert.AreEqual (AnchorStyles.Bottom, c.Anchor, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			c.Anchor = AnchorStyles.Bottom;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		//[Test]
		//public void PropertyAutoScrollOffset ()
		//{
		//        Control c = new Control ();
		//        EventWatcher ew = new EventWatcher (c);

		//        c.AutoScrollOffset = new Point (45, 45);
		//        Assert.AreEqual (new Point (45, 45), c.AutoScrollOffset, "B1");
		//        Assert.AreEqual (string.Empty, ew.ToString (), "B2");

		//        ew.Clear ();
		//        c.AutoScrollOffset = new Point (45, 45);
		//        Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		//}

		[Test]
		public void PropertyAutoSize ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.AutoSize = true;
			Assert.AreEqual (true, c.AutoSize, "B1");
			Assert.AreEqual ("AutoSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.AutoSize = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackColor ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.BackColor = Color.Aquamarine;
			Assert.AreEqual (Color.Aquamarine, c.BackColor, "B1");
			Assert.AreEqual ("BackColorChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.BackColor = Color.Aquamarine;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackgroundImage ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			Image i = new Bitmap (5, 5);

			c.BackgroundImage = i;
			Assert.AreSame (i, c.BackgroundImage, "B1");
			Assert.AreEqual ("BackgroundImageChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.BackgroundImage = i;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBackgroundImageLayout ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (ImageLayout.Zoom, c.BackgroundImageLayout, "B1");
			Assert.AreEqual ("BackgroundImageLayoutChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.BackgroundImageLayout = ImageLayout.Zoom;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBindingContext ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			BindingContext b = new BindingContext ();

			c.BindingContext = b;
			Assert.AreSame (b, c.BindingContext, "B1");
			Assert.AreEqual ("BindingContextChanged", ew.ToString (), "B2");
			
			c.BindingContext = b;
			Assert.AreEqual ("BindingContextChanged", ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBounds ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Bounds = new Rectangle (0, 0, 5, 5);
			Assert.AreEqual (new Rectangle (0, 0, 5, 5), c.Bounds, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Bounds = new Rectangle (0, 0, 5, 5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[Ignore ("Setting Capture to true does not hold, getter returns false.")]
		public void PropertyCapture ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Capture = true;
			Assert.AreEqual (true, c.Capture, "B1");
			Assert.AreEqual ("HandleCreated", ew.ToString (), "B2");

			ew.Clear ();
			c.Capture = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyClientSize ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.ClientSize = new Size (5, 5);
			Assert.AreEqual (new Size (5, 5), c.ClientSize, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.ClientSize = new Size (5, 5);
			Assert.AreEqual ("ClientSizeChanged", ew.ToString (), "B3");
		}

		[Test]
		public void PropertyContextMenu ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			ContextMenu cm = new ContextMenu ();
			
			c.ContextMenu = cm;
			Assert.AreEqual (cm, c.ContextMenu, "B1");
			Assert.AreEqual ("ContextMenuChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.ContextMenu = cm;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyContextMenuStrip ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			ContextMenuStrip cm = new ContextMenuStrip ();

			c.ContextMenuStrip = cm;
			Assert.AreEqual (cm, c.ContextMenuStrip, "B1");
			Assert.AreEqual ("ContextMenuStripChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.ContextMenuStrip = cm;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCursor ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Cursor = Cursors.HSplit;
			Assert.AreEqual (Cursors.HSplit, c.Cursor, "B1");
			Assert.AreEqual ("CursorChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Cursor = Cursors.HSplit;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDock ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Dock = DockStyle.Fill;
			Assert.AreEqual (DockStyle.Fill, c.Dock, "B1");
			Assert.AreEqual ("DockChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Dock = DockStyle.Fill;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyEnabled ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Enabled = false;
			Assert.AreEqual (false, c.Enabled, "B1");
			Assert.AreEqual ("EnabledChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Enabled = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyFont ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			Font f = new Font ("Arial", 14);
			
			c.Font = f;
			Assert.AreEqual (f, c.Font, "B1");
			Assert.AreEqual ("FontChanged;Layout", ew.ToString (), "B2");

			ew.Clear ();
			c.Font = f;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyForeColor ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.ForeColor = Color.Peru;
			Assert.AreEqual (Color.Peru, c.ForeColor, "B1");
			Assert.AreEqual ("ForeColorChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.ForeColor = Color.Peru;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyHeight ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Height = 27;
			Assert.AreEqual (27, c.Height, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Height = 27;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyImeMode ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.ImeMode = ImeMode.Hiragana;
			Assert.AreEqual (ImeMode.Hiragana, c.ImeMode, "B1");
			Assert.AreEqual ("ImeModeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.ImeMode = ImeMode.Hiragana;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyLeft ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Left = 27;
			Assert.AreEqual (27, c.Left, "B1");
			Assert.AreEqual ("Move;LocationChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Left = 27;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyLocation ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Location = new Point (5, 5);
			Assert.AreEqual (new Point (5, 5), c.Location, "B1");
			Assert.AreEqual ("Move;LocationChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Location = new Point (5, 5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMargin ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Margin = new Padding (5);
			Assert.AreEqual (new Padding (5), c.Margin, "B1");
			Assert.AreEqual ("MarginChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Margin = new Padding (5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMaximumSize ()
		{
			Control c = new Control ();
			c.Size = new Size(10, 10);

			// Chaning MaximumSize below Size forces a size change
			EventWatcher ew = new EventWatcher (c);
			c.MaximumSize = new Size (5, 5);
			Assert.AreEqual (new Size (5, 5), c.MaximumSize, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			// Changing MaximumSize when Size is already smaller or equal doesn't raise any events
			ew.Clear ();
			c.MaximumSize = new Size (5, 5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMinimumSize ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.MinimumSize = new Size (5, 5);
			Assert.AreEqual (new Size (5, 5), c.MinimumSize, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.MinimumSize = new Size (5, 5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyName ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Name = "Bob";
			Assert.AreEqual ("Bob", c.Name, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			c.Name = "Bob";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyPadding ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Padding = new Padding (5);
			Assert.AreEqual (new Padding (5), c.Padding, "B1");
			Assert.AreEqual ("PaddingChanged;Layout", ew.ToString (), "B2");

			ew.Clear ();
			c.Padding = new Padding (5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRegion ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			Region r = new Region ();
			
			c.Region = r;
			Assert.AreSame (r, c.Region, "B1");
			Assert.AreEqual ("RegionChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Region = r;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyRightToLeft ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.RightToLeft = RightToLeft.Yes;
			Assert.AreEqual (RightToLeft.Yes, c.RightToLeft, "B1");
			Assert.AreEqual ("RightToLeftChanged;Layout", ew.ToString (), "B2");

			ew.Clear ();
			c.RightToLeft = RightToLeft.Yes;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySize ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Size = new Size (5, 5);
			Assert.AreEqual (new Size (5, 5), c.Size, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Size = new Size (5, 5);
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTabIndex ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.TabIndex = 4;
			Assert.AreEqual (4, c.TabIndex, "B1");
			Assert.AreEqual ("TabIndexChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.TabIndex = 4;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTabStop ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.TabStop = false;
			Assert.AreEqual (false, c.TabStop, "B1");
			Assert.AreEqual ("TabStopChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.TabStop = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTag ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);
			Object o = "Hello";

			c.Tag = o;
			Assert.AreSame (o, c.Tag, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			c.Tag = o;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyText ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Text = "Enchilada";
			Assert.AreEqual ("Enchilada", c.Text, "B1");
			Assert.AreEqual ("TextChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Text = "Enchilada";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTop ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Top = 27;
			Assert.AreEqual (27, c.Top, "B1");
			Assert.AreEqual ("Move;LocationChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Top = 27;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyVisible ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Visible = false;
			Assert.AreEqual (false, c.Visible, "B1");
			Assert.AreEqual ("VisibleChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Visible = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyWidth ()
		{
			Control c = new Control ();
			EventWatcher ew = new EventWatcher (c);

			c.Width = 27;
			Assert.AreEqual (27, c.Width, "B1");
			Assert.AreEqual ("Layout;Resize;SizeChanged;ClientSizeChanged", ew.ToString (), "B2");

			ew.Clear ();
			c.Width = 27;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		private class EventWatcher
		{
			private string events = string.Empty;

			public EventWatcher (Control c)
			{
				c.AutoSizeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("AutoSizeChanged;"); });
				c.BackColorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BackColorChanged;"); });
				c.BackgroundImageChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BackgroundImageChanged;"); });
				c.BackgroundImageLayoutChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BackgroundImageLayoutChanged;"); });
				c.BindingContextChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BindingContextChanged;"); });
				c.CausesValidationChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CausesValidationChanged;"); });
				c.ChangeUICues += new UICuesEventHandler (delegate (Object obj, UICuesEventArgs e) { events += ("ChangeUICues;"); });
				c.Click += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Click;"); });
				c.ClientSizeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ClientSizeChanged;"); });
				c.ContextMenuChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ContextMenuChanged;"); });
				c.ContextMenuStripChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ContextMenuStripChanged;"); });
				c.ControlAdded += new ControlEventHandler (delegate (Object obj, ControlEventArgs e) { events += ("ControlAdded;"); });
				c.ControlRemoved += new ControlEventHandler (delegate (Object obj, ControlEventArgs e) { events += ("ControlRemoved;"); });
				c.CursorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("CursorChanged;"); });
				c.DockChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DockChanged;"); });
				c.DoubleClick += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DoubleClick;"); });
				c.DragDrop += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragDrop;"); });
				c.DragEnter += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragEnter;"); });
				c.DragLeave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DragLeave;"); });
				c.DragOver += new DragEventHandler (delegate (Object obj, DragEventArgs e) { events += ("DragOver;"); });
				c.EnabledChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("EnabledChanged;"); });
				c.Enter += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Enter;"); });
				c.FontChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("FontChanged;"); });
				c.ForeColorChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ForeColorChanged;"); });
				c.GiveFeedback += new GiveFeedbackEventHandler (delegate (Object obj, GiveFeedbackEventArgs e) { events += ("GiveFeedback;"); });
				c.GotFocus += new EventHandler (delegate (Object obj, EventArgs e) { events += ("GotFocus;"); });
				c.HandleCreated += new EventHandler (delegate (Object obj, EventArgs e) { events += ("HandleCreated;"); });
				c.HandleDestroyed += new EventHandler (delegate (Object obj, EventArgs e) { events += ("HandleDestroyed;"); });
				c.ImeModeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ImeModeChanged;"); });
				c.Invalidated += new InvalidateEventHandler (delegate (Object obj, InvalidateEventArgs e) { events += ("Invalidated;"); });
				c.KeyDown += new KeyEventHandler (delegate (Object obj, KeyEventArgs e) { events += ("KeyDown;"); });
				c.KeyPress += new KeyPressEventHandler (delegate (Object obj, KeyPressEventArgs e) { events += ("KeyPress;"); });
				c.KeyUp += new KeyEventHandler (delegate (Object obj, KeyEventArgs e) { events += ("KeyUp;"); });
				c.Layout += new LayoutEventHandler (delegate (Object obj, LayoutEventArgs e) { events += ("Layout;"); });
				c.Leave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Leave;"); });
				c.LocationChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LocationChanged;"); });
				c.LostFocus += new EventHandler (delegate (Object obj, EventArgs e) { events += ("LostFocus;"); });
				c.MarginChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MarginChanged;"); });
				c.MouseCaptureChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseCaptureChanged;"); });
				c.MouseClick += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseClick;"); });
				c.MouseDoubleClick += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseDoubleClick;"); });
				c.MouseDown += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseDown;"); });
				c.MouseEnter += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseEnter;"); });
				c.MouseLeave += new EventHandler (delegate (Object obj, EventArgs e) { events += ("MouseLeave;"); });
				c.MouseMove += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseMove;"); });
				c.MouseUp += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseUp;"); });
				c.MouseWheel += new MouseEventHandler (delegate (Object obj, MouseEventArgs e) { events += ("MouseWheel;"); });
				c.Move += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Move;"); });
				c.PaddingChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("PaddingChanged;"); });
				c.Paint += new PaintEventHandler (delegate (Object obj, PaintEventArgs e) { events += ("Paint;"); });
				c.ParentChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ParentChanged;"); });
				c.PreviewKeyDown += new PreviewKeyDownEventHandler (delegate (Object obj, PreviewKeyDownEventArgs e) { events += ("PreviewKeyDown;"); });
				c.QueryAccessibilityHelp += new QueryAccessibilityHelpEventHandler (delegate (Object obj, QueryAccessibilityHelpEventArgs e) { events += ("QueryAccessibilityHelp;"); });
				c.QueryContinueDrag += new QueryContinueDragEventHandler (delegate (Object obj, QueryContinueDragEventArgs e) { events += ("QueryContinueDrag;"); });
				c.RegionChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RegionChanged;"); });
				c.Resize += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Resize;"); });
				c.RightToLeftChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("RightToLeftChanged;"); });
				c.SizeChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("SizeChanged;"); });
				c.StyleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("StyleChanged;"); });
				c.SystemColorsChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("SystemColorsChanged;"); });
				c.TabIndexChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TabIndexChanged;"); });
				c.TabStopChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TabStopChanged;"); });
				c.TextChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TextChanged;"); });
				c.Validated += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Validated;"); });
				c.Validating += new CancelEventHandler (delegate (Object obj, CancelEventArgs e) { events += ("Validating;"); });
				c.VisibleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("VisibleChanged;"); });
			}

			public override string ToString ()
			{
				return events.TrimEnd (';');
			}

			public void Clear ()
			{
				events = string.Empty;
			}
		}

		private class ExposeProtectedProperties : Control
		{
			//public new bool CanRaiseEvents { get { return base.CanRaiseEvents; } }
			public new Cursor DefaultCursor { get { return base.DefaultCursor; } }
			public new Size DefaultMaximumSize { get { return base.DefaultMaximumSize; } }
			public new Size DefaultMinimumSize { get { return base.DefaultMinimumSize; } }
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
		}
	}
}
