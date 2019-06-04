//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
   [TestFixture]
   public class LabelTest : TestHelper
   {
		[Test]
		public void LabelAccessibility ()
		{
			Label l = new Label ();
			Assert.IsNotNull (l.AccessibilityObject, "1");
		}

		[Test]
		public void PreferredWidth ()
		{
			Label l = new Label();

			// preferred width is 0 by default
			Assert.AreEqual (0, l.PreferredWidth, "2");

			// and after text is changed it's something else
			l.Text = "hi";
			Assert.IsTrue (l.PreferredWidth > 0, "3");

			// now add it to a form and see
			Form f = new Form ();
			f.ShowInTaskbar = false;
			l.Text = "";

			f.Controls.Add (l);
			f.Show ();
			Assert.AreEqual (0, l.PreferredWidth, "4");

			l.Text = "hi";
			Assert.IsTrue (l.PreferredWidth > 0, "5");

			f.Dispose ();
		}

		[Test]
		public void PreferredHeight ()
		{
			Label l = new Label();
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height + 3), "#1");
			
			l.BorderStyle = BorderStyle.None;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height + 3), "#2");
			
			l.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height + 6), "#3");
			
			l.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height + 6), "#4");

			l.UseCompatibleTextRendering = false;
			
			l.BorderStyle = BorderStyle.None;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height), "#5");
			
			l.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height), "#6");
			
			l.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (l.PreferredHeight, (l.Font.Height), "#7");
		}
		
		[Test]
		public void SizesTest ()
		{
			Form myform = new Form ();
			Label l1 = new Label (); l1.Text = "Test";
			Label l2 = new Label (); l2.Text = "Test";
			Label l3 = new Label (); l3.Text = "Test three";
			Label l4 = new Label (); l4.Text = String.Format ("Test four{0}with line breaks", Environment.NewLine);
			myform.Controls.Add (l1);
			myform.Controls.Add (l2);
			myform.Controls.Add (l3);
			myform.Controls.Add (l4);
			myform.Show ();
			
			l2.Font = new Font (l1.Font.FontFamily, l1.Font.Size + 5, l1.Font.Style);
			
			// Height: autosize = false
			Assert.AreEqual (l1.Height, l2.Height, "#1");
			Assert.AreEqual (l1.Height, l3.Height, "#2");
			Assert.AreEqual (l1.Height, l4.Height, "#3");
			
			// Width: autosize = false			
			Assert.AreEqual (l1.Width, l2.Width, "#4");
			Assert.AreEqual (l1.Width, l3.Width, "#5");
			Assert.AreEqual (l1.Width, l4.Width, "#6");
			
			l1.AutoSize = true; 
			l2.AutoSize = true;
			l3.AutoSize = true;
			l4.AutoSize = true;
			
			// Height: autosize = false
			Assert.IsFalse (l1.Height.Equals (l2.Height), "#7");
			Assert.IsTrue (l1.Height.Equals (l3.Height), "#8");
			Assert.IsTrue ((l4.Height > l1.Height), "#9");
			
			// Width: autosize = false
			Assert.IsFalse (l1.Width.Equals (l2.Width), "#10");
			Assert.IsFalse (l1.Width.Equals (l3.Width), "#11");
			
			myform.Dispose();
		}

		[Test]
		public void StyleTest ()
		{
			Label l = new Label ();
			
			Assert.IsFalse (TestHelper.IsStyleSet (l, WindowStyles.WS_BORDER), "#1");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_CLIENTEDGE), "#2");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_STATICEDGE), "#3");
			
			l.BorderStyle = BorderStyle.None;

			Assert.IsFalse (TestHelper.IsStyleSet (l, WindowStyles.WS_BORDER), "#4");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_CLIENTEDGE), "#5");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_STATICEDGE), "#6");
			
			l.BorderStyle = BorderStyle.FixedSingle;

			Assert.IsTrue (TestHelper.IsStyleSet (l, WindowStyles.WS_BORDER), "#7");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_CLIENTEDGE), "#8");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_STATICEDGE), "#9");

			l.BorderStyle = BorderStyle.Fixed3D;
			
			Assert.IsFalse (TestHelper.IsStyleSet (l, WindowStyles.WS_BORDER), "#10");
			Assert.IsFalse (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_CLIENTEDGE), "#11");
			Assert.IsTrue (TestHelper.IsExStyleSet (l, WindowExStyles.WS_EX_STATICEDGE), "#12");
		}

		[Test]
		public void BoundsTest ()
		{
			Label l = new Label ();

			Assert.AreEqual (new Rectangle (0,0,100,23), l.Bounds, "1");
			Assert.AreEqual (new Rectangle (0,0,100,23), l.ClientRectangle, "2");
			Assert.AreEqual (new Size (100,23), l.ClientSize, "3");
		}

		[Test]
		public void PubPropTest ()
		{
			Label l = new Label ();

			Assert.IsFalse (l.AutoSize, "#3");
			
			Assert.AreEqual ("Control", l.BackColor.Name  , "#6");
			Assert.IsNull (l.BackgroundImage, "#8");
			Assert.AreEqual (BorderStyle.None , l.BorderStyle, "#9");		
			
			Assert.IsNull (l.Container, "#19");
			Assert.IsFalse (l.ContainsFocus, "#20");
			Assert.IsNull (l.ContextMenu, "#21");
			Assert.IsFalse (l.Created, "#23");
			Assert.AreEqual (Cursors.Default , l.Cursor, "#24");
			
			Assert.IsNotNull (l.DataBindings, "#25");
			Assert.AreEqual (DockStyle.None, l.Dock, "#28");
			
			Assert.IsTrue (l.Enabled, "#29");
			
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "#30");
			Assert.IsFalse (l.Focused, "#31");
			Assert.AreEqual (SystemFonts.DefaultFont, l.Font, "#32");
			Assert.AreEqual (SystemColors.ControlText, l.ForeColor, "#33");
			
			Assert.IsFalse (l.HasChildren, "#35");
			
			Assert.IsNull   (l.Image, "#37");
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "#38");
			Assert.AreEqual (-1, l.ImageIndex, "#39");
			Assert.IsNull   (l.ImageList, "#40");
			Assert.IsFalse  (l.InvokeRequired, "#42");
			Assert.IsFalse  (l.IsAccessible, "#43");
			Assert.IsFalse  (l.IsDisposed, "#44");
			
			Assert.IsNull (l.Parent, "#49");

			Assert.IsFalse (l.RecreatingHandle, "#54");
			Assert.IsNull (l.Region, "#55");
			Assert.AreEqual (RightToLeft.No, l.RightToLeft, "#57");
			
			Assert.IsNull (l.Site, "#58");
			
			Assert.AreEqual (0, l.TabIndex, "#60");
			Assert.IsNull (l.Tag, "#61");
			Assert.AreEqual ("", l.Text, "#62");
			Assert.AreEqual (ContentAlignment.TopLeft, l.TextAlign, "#63");
			Assert.IsNull (l.TopLevelControl, "#65");
			
			Assert.IsTrue (l.UseMnemonic, "#66");
			
			Assert.IsTrue (l.Visible, "#67");
		}

		[Test]
		public void LabelEqualsTest () {
			Label s1 = new Label ();
			Label s2 = new Label ();
			s1.Text = "abc";
			s2.Text = "abc";
			Assert.IsFalse (s1.Equals (s2), "#69");
			Assert.IsTrue (s1.Equals (s1), "#70");
		}
		
		[Test]
		public void LabelScaleTest () {
			Label r1 = new Label ();
			r1.Width = 40;
			r1.Height = 20 ;
			r1.Scale (2);
			Assert.AreEqual (80, r1.Width, "#71");
			Assert.AreEqual (40, r1.Height, "#72");

		}		

	   [Test]
	   public void ToStringTest ()
	   {
		   Label l = new Label ();

		   l.Text = "My Label";

		   Assert.AreEqual ("System.Windows.Forms.Label, Text: My Label", l.ToString (), "T1");
	   }
	   
	[Test]
	public void AutoSizeExplicitSize ()
	{
		Form f = new Form ();
		f.ShowInTaskbar = false;
		
		Label l = new Label ();
		l.Size = new Size (5, 5);
		l.AutoSize = true;
		l.Text = "My Label";

		f.Controls.Add (l);
		
		Size s = l.Size;

		l.Width = 10;
		Assert.AreEqual (s, l.Size, "A1");

		l.Height = 10;
		Assert.AreEqual (s, l.Size, "A2");
	}
	   
	[Test]
	public void LabelMargin ()
	{
		Assert.AreEqual (new Padding (3, 0, 3, 0), new Label ().Margin, "A1");
	}

	   [Test]
	   public void BehaviorImageList ()
	   {
		   // Basically, this shows that whichever of [Image|ImageIndex|ImageKey]
		   // is set last resets the others to their default state
		   Label b = new Label ();

		   Bitmap i1 = new Bitmap (16, 16);
		   i1.SetPixel (0, 0, Color.Blue);
		   Bitmap i2 = new Bitmap (16, 16);
		   i2.SetPixel (0, 0, Color.Red);
		   Bitmap i3 = new Bitmap (16, 16);
		   i3.SetPixel (0, 0, Color.Green);

		   Assert.AreEqual (null, b.Image, "D1");
		   Assert.AreEqual (-1, b.ImageIndex, "D2");
		   Assert.AreEqual (string.Empty, b.ImageKey, "D3");

		   ImageList il = new ImageList ();
		   il.Images.Add ("i2", i2);
		   il.Images.Add ("i3", i3);

		   b.ImageList = il;

		   b.ImageKey = "i3";
		   Assert.AreEqual (-1, b.ImageIndex, "D4");
		   Assert.AreEqual ("i3", b.ImageKey, "D5");
		   Assert.AreEqual (i3.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D6");

		   b.ImageIndex = 0;
		   Assert.AreEqual (0, b.ImageIndex, "D7");
		   Assert.AreEqual (string.Empty, b.ImageKey, "D8");
		   Assert.AreEqual (i2.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D9");

		   // Also, Image is not cached, changing the underlying ImageList image is reflected
		   il.Images[0] = i1;
		   Assert.AreEqual (i1.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D16");

		   // Note: setting Image resets ImageList to null
		   b.Image = i1;
		   Assert.AreEqual (-1, b.ImageIndex, "D10");
		   Assert.AreEqual (string.Empty, b.ImageKey, "D11");
		   Assert.AreEqual (i1.GetPixel (0, 0), (b.Image as Bitmap).GetPixel (0, 0), "D12");
		   Assert.AreEqual (null, b.ImageList, "D12-2");

		   b.Image = null;
		   Assert.AreEqual (null, b.Image, "D13");
		   Assert.AreEqual (-1, b.ImageIndex, "D14");
		   Assert.AreEqual (string.Empty, b.ImageKey, "D15");
	   }

		[Test]
		public void SelfSizingTest ()
		{
			TableLayoutPanel p1 = new TableLayoutPanel ();
			Label l1 = new Label ();
			l1.AutoSize = true;
			p1.Controls.Add (l1);
			p1.SuspendLayout ();
			Size l1_saved_size = l1.Size;
			l1.Text = "Text";
			Assert.AreEqual (l1_saved_size, l1.Size, "#1");
			p1.ResumeLayout ();
			Assert.AreNotEqual (l1_saved_size, l1.Size, "#1a");

			FlowLayoutPanel p2 = new FlowLayoutPanel ();
			Label l2 = new Label ();
			l2.AutoSize = true;
			p2.Controls.Add (l2);
			p2.SuspendLayout ();
			Size l2_saved_size = l2.Size;
			l2.Text = "Text";
			Assert.AreEqual (l2_saved_size, l2.Size, "#2");
			p2.ResumeLayout ();
			Assert.AreNotEqual (l2_saved_size, l2.Size, "#2a");

			Panel p3 = new Panel ();
			Label l3 = new Label ();
			l3.AutoSize = true;
			p3.Controls.Add (l3);
			p3.SuspendLayout ();
			Size l3_saved_size = l3.Size;
			l3.Text = "Text";
			Assert.AreNotEqual (l3_saved_size, l3.Size, "#2");
			p3.ResumeLayout ();
		}
	}

   [TestFixture]
   public class LabelEventTest : TestHelper
   {
	   static bool eventhandled = false;
	   public void Label_EventHandler (object sender,EventArgs e)
	   {
		   eventhandled = true;
	   }

	   public void Label_KeyDownEventHandler (object sender, KeyEventArgs e)
	   {
		   eventhandled = true;
	   }

	   [Ignore ("AutoSize moved to Control in 2.0, Label.AutoSize needs to be reworked a bit.")]
	   [Test]
	   public void AutoSizeChangedChangedTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     Label l = new Label ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.AutoSizeChanged += new EventHandler (Label_EventHandler);
		     l.AutoSize = true;
		     Assert.AreEqual (true, eventhandled, "B4");
		     eventhandled = false;
		     myform.Dispose();
	     }

	   [Test]
	   public void BackgroundImageChangedTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     Label l = new Label ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.BackgroundImageChanged += new EventHandler (Label_EventHandler);
		     l.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));
		     Assert.AreEqual (true, eventhandled, "B4");
		     eventhandled = false;
		     myform.Dispose();
	     }

	   [Test]
	   public void ImeModeChangedTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     Label l = new Label ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.ImeModeChanged += new EventHandler (Label_EventHandler);
		     l.ImeMode = ImeMode.Katakana;
		     Assert.AreEqual (true, eventhandled, "I16");
		     eventhandled = false;
		     myform.Dispose();
	     }

	   [Test]
	   public void KeyDownTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.KeyDown += new KeyEventHandler (Label_KeyDownEventHandler);
		     l.KeyPressA ();

		     Assert.AreEqual (true, eventhandled, "K1");
		     eventhandled = false;
		     myform.Dispose();
	     }

	   [Test]
	   public void TabStopChangedTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     Label l = new Label ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.TabStopChanged += new EventHandler (Label_EventHandler);
		     l.TabStop = true;
		     Assert.AreEqual (true, eventhandled, "T3");
		     eventhandled = false;
		     myform.Dispose();
	     }

	   [Test]
	   public void TextAlignChangedTest ()
	     {
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     Label l = new Label ();
		     l.Visible = true;
		     myform.Controls.Add (l);
		     l.TextAlignChanged += new EventHandler (Label_EventHandler);
		     l.TextAlign = ContentAlignment.TopRight;
		     Assert.AreEqual (true, eventhandled, "T4");
		     eventhandled = false;
		     myform.Dispose();
	     }
   }

public class MyLabelInvalidate : MyLabel
   {
	   //protected ArrayList results = new ArrayList ();
    public MyLabelInvalidate () : base ()
	   {}

	   protected override void OnInvalidated (InvalidateEventArgs e)
	   {
		   base.OnInvalidated (e);
		   string res = (string)results [results.Count - 1];
		   results [results.Count - 1 ] = string.Concat (res, "," + e.InvalidRect.ToString ());
		   //results.Add ("OnInvalidate," + e.InvalidRect.ToString ());
	   }

	   //public ArrayList Results {
	   //	get {	return results; }
	   //}

   }

public class MyLabel : Label
   {
	   protected ArrayList results = new ArrayList ();
    public MyLabel () : base ()
	   {	}

	   protected override void OnAutoSizeChanged (EventArgs e)
	   {
		   results.Add ("OnAutoSizeChanged");
		   base.OnAutoSizeChanged (e);
	   }

	   protected override void OnBackgroundImageChanged (EventArgs e)
	   {
		   results.Add ("OnBackgroundImageChanged");
		   base.OnBackgroundImageChanged (e);
	   }

	   protected override void OnImeModeChanged (EventArgs e)
	   {
		   results.Add ("OnImeModeChanged");
		   base.OnImeModeChanged (e);
	   }

	   protected override void OnKeyDown (KeyEventArgs e)
	   {
		   results.Add ("OnKeyDown,"+(char)e.KeyValue);
		   base.OnKeyDown (e);
	   }

	   protected override void OnKeyPress (KeyPressEventArgs e)
	   {
		   results.Add ("OnKeyPress,"+e.KeyChar.ToString ());
		   base.OnKeyPress (e);
	   }

	   protected override void OnKeyUp (KeyEventArgs e)
	   {
		   results.Add ("OnKeyUp,"+(char)e.KeyValue);
		   base.OnKeyUp (e);
	   }

	   protected override void OnHandleCreated (EventArgs e)
	   {
		   results.Add ("OnHandleCreated");
		   base.OnHandleCreated (e);
	   }

	   protected override void OnBindingContextChanged (EventArgs e)
	   {
		   results.Add ("OnBindingContextChanged");
		   base.OnBindingContextChanged (e);
	   }

	   protected override void OnInvalidated (InvalidateEventArgs e)
	   {
		   results.Add("OnInvalidated");
		   base.OnInvalidated (e);
	   }

	   protected override void OnResize (EventArgs e)
	   {
		   results.Add("OnResize");
		   base.OnResize (e);
	   }

	   protected override void OnSizeChanged (EventArgs e)
	   {
		   results.Add("OnSizeChanged");
		   base.OnSizeChanged (e);
	   }

	   protected override void OnLayout (LayoutEventArgs e)
	   {
		   results.Add("OnLayout");
		   base.OnLayout (e);
	   }

	   protected override void OnVisibleChanged (EventArgs e)
	   {
		   results.Add("OnVisibleChanged");
		   base.OnVisibleChanged (e);
	   }

	   protected override void OnPaint (PaintEventArgs e)
	   {
		   results.Add("OnPaint");
		   base.OnPaint (e);
	   }

	   public void KeyPressA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYDOWN;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_CHAR;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x61;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_KEYUP;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)unchecked((int)0xC01e0001);
		   this.WndProc(ref m);
	   }

	   public void KeyDownA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYDOWN;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_CHAR;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x61;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);
	   }

	   public void KeyUpA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYUP;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)unchecked((int)0xC01e0001);
		   this.WndProc(ref m);
	   }

	   public ArrayList Results {
		   get {	return results; }
	   }
   }

   [TestFixture]
   [Ignore("Comparisons too strict")]
   public class LabelTestEventsOrder : TestHelper
   {
	   public string [] ArrayListToString (ArrayList arrlist)
	   {
		   string [] retval = new string [arrlist.Count];
		   for (int i = 0; i < arrlist.Count; i++)
		     retval[i] = (string)arrlist[i];
		   return retval;
	   }

	//private void OrderedAssert(string[] wanted, ArrayList found) {
	//        int	last_target;
	//        bool	seen;
	//
	//        last_target = 0;
	//
	//        for (int i = 0; i < wanted.Length; i++) {
	//                seen = false;
	//                for (int j = last_target; j < found.Count; j++) {
	//                        if (wanted[i] == (string)found[j]) {
	//                                seen = true;
	//                                last_target = j + 1;
	//                                break;
	//                        }
	//                }
	//
	//                if (!seen) {
	//                        Console.WriteLine("Needed {0}", wanted[i]);
	//                }
	//        }
	//}

        public void PrintList(string name, ArrayList list) {
                Console.WriteLine("{0}", name);
                for (int i = 0; i < list.Count; i++) {
                        Console.WriteLine("   {0}", list[i]);
                }
                Console.WriteLine("");
        }


	   [Test]
	   public void CreateEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void SizeChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnSizeChanged",
			       "OnResize",
			       "OnInvalidated",
			       "OnLayout"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.Size = new Size (150, 20);

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void AutoSizeChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnSizeChanged",
			       "OnResize",
			       "OnInvalidated",
			       "OnLayout",
			       "OnAutoSizeChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.AutoSize = true;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void BackgroundImageChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnBackgroundImageChanged",
			       "OnInvalidated"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void ImeModeChangedChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnImeModeChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.ImeMode = ImeMode.Katakana;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void KeyPressEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnKeyDown,A",
			       "OnKeyPress,a",
			       "OnKeyUp,A"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.KeyPressA ();

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void TabStopChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.TabStop = true;
PrintList("TabStopChanged", l.Results);
		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void TextAlignChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnInvalidated"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.TextAlign = ContentAlignment.TopRight;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void InvalidateEventsOrder ()
	     {
		     Rectangle rect = new Rectangle (new Point (0,0), new Size (2, 2));

		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabelInvalidate l = new MyLabelInvalidate ();
		     myform.Controls.Add (l);
		     l.TextAlign = ContentAlignment.TopRight;

		     string [] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnInvalidated,{X=0,Y=0,Width="+l.Size.Width+",Height="+l.Size.Height+"}",
			       "OnInvalidated," + rect.ToString ()
		     };

		     l.Invalidate (rect);

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void PaintEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "OnHandleCreated",
			       "OnBindingContextChanged",
			       "OnBindingContextChanged",
			       "OnInvalidated",
			       "OnInvalidated",
			       "OnPaint"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel l = new MyLabel ();
		     myform.Controls.Add (l);
		     l.TextAlign = ContentAlignment.TopRight;
		     l.Refresh ();
		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

   }

public class MyLabel2 : Label
   {
	   protected ArrayList results = new ArrayList ();
    public MyLabel2 () : base ()
	   {
		   this.AutoSizeChanged += new EventHandler (AutoSizeChanged_Handler);
		   this.HandleCreated += new EventHandler (HandleCreated_Handler);
		   this.BindingContextChanged += new EventHandler (BindingContextChanged_Handler);
		   this.BackgroundImageChanged += new EventHandler (BackgroundImageChanged_Handler);
		   this.ImeModeChanged += new EventHandler (ImeModeChanged_Handler);
		   this.KeyDown += new KeyEventHandler (KeyDown_Handler);
		   this.KeyPress += new KeyPressEventHandler (KeyPress_Handler);
		   this.KeyUp += new KeyEventHandler (KeyUp_Handler);
		   this.Invalidated += new InvalidateEventHandler (Invalidated_Handler);
		   this.Resize += new EventHandler (Resize_Handler);
		   this.SizeChanged += new EventHandler (SizeChanged_Handler);
		   this.Layout += new LayoutEventHandler (Layout_Handler);
		   this.VisibleChanged += new EventHandler (VisibleChanged_Handler);
		   this.Paint += new PaintEventHandler (Paint_Handler);
	   }

	   protected void AutoSizeChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add ("AutoSizeChanged");
	   }

	   protected void BackgroundImageChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add ("BackgroundImageChanged");
	   }

	   protected void ImeModeChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add ("ImeModeChanged");
	   }

	   protected void KeyDown_Handler (object sender, KeyEventArgs e)
	   {
		   results.Add ("KeyDown,"+(char)e.KeyValue);
	   }

	   protected void KeyPress_Handler (object sender, KeyPressEventArgs e)
	   {
		   results.Add ("KeyPress,"+e.KeyChar.ToString ());
	   }

	   protected void KeyUp_Handler (object sender, KeyEventArgs e)
	   {
		   results.Add ("KeyUp,"+(char)e.KeyValue);
	   }

	   protected void HandleCreated_Handler (object sender, EventArgs e)
	   {
		   results.Add ("HandleCreated");
	   }

	   protected void BindingContextChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add ("BindingContextChanged");
	   }

	   protected void Invalidated_Handler (object sender, InvalidateEventArgs e)
	   {
		   results.Add("Invalidated");
	   }

	   protected void Resize_Handler (object sender, EventArgs e)
	   {
		   results.Add("Resize");
	   }

	   protected void SizeChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add("SizeChanged");
	   }

	   protected void Layout_Handler (object sender, LayoutEventArgs e)
	   {
		   results.Add("Layout");
	   }

	   protected void VisibleChanged_Handler (object sender, EventArgs e)
	   {
		   results.Add("VisibleChanged");
	   }

	   protected void Paint_Handler (object sender, PaintEventArgs e)
	   {
		   results.Add("Paint");
	   }

	   public void KeyPressA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYDOWN;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_CHAR;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x61;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_KEYUP;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)unchecked((int)0xC01e0001);
		   this.WndProc(ref m);
	   }

	   public void KeyDownA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYDOWN;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);

		   m.Msg = (int)WndMsg.WM_CHAR;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x61;
		   m.LParam = (IntPtr)0x1e0001;
		   this.WndProc(ref m);
	   }

	   public void KeyUpA()
	   {
		   Message m;

		   m = new Message();

		   m.Msg = (int)WndMsg.WM_KEYUP;
		   m.HWnd = this.Handle;
		   m.WParam = (IntPtr)0x41;
		   m.LParam = (IntPtr)unchecked((int)0xC01e0001);
		   this.WndProc(ref m);
	   }

	   public ArrayList Results {
		   get {	return results; }
	   }
   }

   [TestFixture]
   [Ignore("Comparisons too strict")]
   public class LabelTestEventsOrder2 : TestHelper
   {
	   public string [] ArrayListToString (ArrayList arrlist)
	   {
		   string [] retval = new string [arrlist.Count];
		   for (int i = 0; i < arrlist.Count; i++)
		     retval[i] = (string)arrlist[i];
		   return retval;
	   }

	   [Test]
	   public void CreateEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void SizeChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "Invalidated",
			       "Layout",
			       "Resize",
			       "SizeChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.Size = new Size (150, 20);

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void AutoSizeChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "Invalidated",
			       "Layout",
			       "Resize",
			       "SizeChanged",
			       "AutoSizeChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.AutoSize = true;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void BackgroundImageChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "Invalidated",
			       "BackgroundImageChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.BackgroundImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/System.Windows.Forms/bitmaps/a.png"));

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void ImeModeChangedChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "ImeModeChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.ImeMode = ImeMode.Katakana;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void KeyPressEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "KeyDown,A",
			       "KeyPress,a",
			       "KeyUp,A"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.KeyPressA ();

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void TabStopChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.TabStop = true;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void TextAlignChangedEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "Invalidated"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.TextAlign = ContentAlignment.TopRight;

		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

	   [Test]
	   public void PaintEventsOrder ()
	     {
		     string[] EventsWanted = {
			     "HandleCreated",
			       "BindingContextChanged",
			       "BindingContextChanged",
			       "Invalidated",
			       "Invalidated",
			       "Paint"
		     };
		     Form myform = new Form ();
		     myform.ShowInTaskbar = false;
		     myform.Visible = true;
		     MyLabel2 l = new MyLabel2 ();
		     myform.Controls.Add (l);
		     l.TextAlign = ContentAlignment.TopRight;
		     l.Refresh ();
		     Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		     myform.Dispose();
	     }

   }

}
