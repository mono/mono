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

namespace MonoTests.System.Windows.Forms
{
        internal enum WndMsg {
		WM_NULL                   = 0x0000,
		WM_CREATE                 = 0x0001,
		WM_DESTROY                = 0x0002,
		WM_MOVE                   = 0x0003,
		WM_SIZE                   = 0x0005,
		WM_ACTIVATE               = 0x0006,
		WM_SETFOCUS               = 0x0007,
		WM_KILLFOCUS              = 0x0008,
		//              public const uint WM_SETVISIBLE           = 0x0009;
		WM_ENABLE                 = 0x000A,
		WM_SETREDRAW              = 0x000B,
		WM_SETTEXT                = 0x000C,
		WM_GETTEXT                = 0x000D,
		WM_GETTEXTLENGTH          = 0x000E,
		WM_PAINT                  = 0x000F,
		WM_CLOSE                  = 0x0010,
		WM_QUERYENDSESSION        = 0x0011,
		WM_QUIT                   = 0x0012,
		WM_QUERYOPEN              = 0x0013,
		WM_ERASEBKGND             = 0x0014,
		WM_SYSCOLORCHANGE         = 0x0015,
		WM_ENDSESSION             = 0x0016,
		//              public const uint WM_SYSTEMERROR          = 0x0017;
		WM_SHOWWINDOW             = 0x0018,
		WM_CTLCOLOR               = 0x0019,
		WM_WININICHANGE           = 0x001A,
		WM_SETTINGCHANGE          = 0x001A,
		WM_DEVMODECHANGE          = 0x001B,
		WM_ACTIVATEAPP            = 0x001C,
		WM_FONTCHANGE             = 0x001D,
		WM_TIMECHANGE             = 0x001E,
		WM_CANCELMODE             = 0x001F,
		WM_SETCURSOR              = 0x0020,
		WM_MOUSEACTIVATE          = 0x0021,
		WM_CHILDACTIVATE          = 0x0022,
		WM_QUEUESYNC              = 0x0023,
		WM_GETMINMAXINFO          = 0x0024,
		WM_PAINTICON              = 0x0026,
		WM_ICONERASEBKGND         = 0x0027,
		WM_NEXTDLGCTL             = 0x0028,
		//              public const uint WM_ALTTABACTIVE         = 0x0029;
		WM_SPOOLERSTATUS          = 0x002A,
		WM_DRAWITEM               = 0x002B,
		WM_MEASUREITEM            = 0x002C,
		WM_DELETEITEM             = 0x002D,
		WM_VKEYTOITEM             = 0x002E,
		WM_CHARTOITEM             = 0x002F,
		WM_SETFONT                = 0x0030,
		WM_GETFONT                = 0x0031,
		WM_SETHOTKEY              = 0x0032,
		WM_GETHOTKEY              = 0x0033,
		//              public const uint WM_FILESYSCHANGE        = 0x0034;
		//              public const uint WM_ISACTIVEICON         = 0x0035;
		//              public const uint WM_QUERYPARKICON        = 0x0036;
		WM_QUERYDRAGICON          = 0x0037,
		WM_COMPAREITEM            = 0x0039,
		//              public const uint WM_TESTING              = 0x003a;
		//              public const uint WM_OTHERWINDOWCREATED = 0x003c;
		WM_GETOBJECT              = 0x003D,
		//                      public const uint WM_ACTIVATESHELLWINDOW        = 0x003e;
		WM_COMPACTING             = 0x0041,
		WM_COMMNOTIFY             = 0x0044 ,
		WM_WINDOWPOSCHANGING      = 0x0046,
		WM_WINDOWPOSCHANGED       = 0x0047,
		WM_POWER                  = 0x0048,
		WM_COPYDATA               = 0x004A,
		WM_CANCELJOURNAL          = 0x004B,
		WM_NOTIFY                 = 0x004E,
		WM_INPUTLANGCHANGEREQUEST = 0x0050,
		WM_INPUTLANGCHANGE        = 0x0051,
		WM_TCARD                  = 0x0052,
		WM_HELP                   = 0x0053,
		WM_USERCHANGED            = 0x0054,
		WM_NOTIFYFORMAT           = 0x0055,
		WM_CONTEXTMENU            = 0x007B,
		WM_STYLECHANGING          = 0x007C,
		WM_STYLECHANGED           = 0x007D,
		WM_DISPLAYCHANGE          = 0x007E,
		WM_GETICON                = 0x007F,

		// Non-Client messages
		WM_SETICON                = 0x0080,
		WM_NCCREATE               = 0x0081,
		WM_NCDESTROY              = 0x0082,
		WM_NCCALCSIZE             = 0x0083,
		WM_NCHITTEST              = 0x0084,
		WM_NCPAINT                = 0x0085,
		WM_NCACTIVATE             = 0x0086,
		WM_GETDLGCODE             = 0x0087,
		WM_SYNCPAINT              = 0x0088,
		//              public const uint WM_SYNCTASK       = 0x0089;
		WM_NCMOUSEMOVE            = 0x00A0,
		WM_NCLBUTTONDOWN          = 0x00A1,
		WM_NCLBUTTONUP            = 0x00A2,
		WM_NCLBUTTONDBLCLK        = 0x00A3,
		WM_NCRBUTTONDOWN          = 0x00A4,
		WM_NCRBUTTONUP            = 0x00A5,
		WM_NCRBUTTONDBLCLK        = 0x00A6,
		WM_NCMBUTTONDOWN          = 0x00A7,
		WM_NCMBUTTONUP            = 0x00A8,
		WM_NCMBUTTONDBLCLK        = 0x00A9,
		//              public const uint WM_NCXBUTTONDOWN    = 0x00ab;
		//              public const uint WM_NCXBUTTONUP      = 0x00ac;
		//              public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;
		WM_KEYDOWN                = 0x0100,
		WM_KEYFIRST               = 0x0100,
		WM_KEYUP                  = 0x0101,
		WM_CHAR                   = 0x0102,
		WM_DEADCHAR               = 0x0103,
		WM_SYSKEYDOWN             = 0x0104,
		WM_SYSKEYUP               = 0x0105,
		WM_SYSCHAR                = 0x0106,
		WM_SYSDEADCHAR            = 0x0107,
		WM_KEYLAST                = 0x0108,
		WM_IME_STARTCOMPOSITION   = 0x010D,
		WM_IME_ENDCOMPOSITION     = 0x010E,
		WM_IME_COMPOSITION        = 0x010F,
		WM_IME_KEYLAST            = 0x010F,
		WM_INITDIALOG             = 0x0110,
		WM_COMMAND                = 0x0111,
		WM_SYSCOMMAND             = 0x0112,
		WM_TIMER                  = 0x0113,
		WM_HSCROLL                = 0x0114,
		WM_VSCROLL                = 0x0115,
		WM_INITMENU               = 0x0116,
		WM_INITMENUPOPUP          = 0x0117,
		//              public const uint WM_SYSTIMER       = 0x0118;
		WM_MENUSELECT             = 0x011F,
		WM_MENUCHAR               = 0x0120,
		WM_ENTERIDLE              = 0x0121,
		WM_MENURBUTTONUP          = 0x0122,
		WM_MENUDRAG               = 0x0123,
		WM_MENUGETOBJECT          = 0x0124,
		WM_UNINITMENUPOPUP        = 0x0125,
		WM_MENUCOMMAND            = 0x0126,
		//              public const uint WM_CHANGEUISTATE    = 0x0127;
		//              public const uint WM_UPDATEUISTATE    = 0x0128;
		//              public const uint WM_QUERYUISTATE     = 0x0129;

		//              public const uint WM_LBTRACKPOINT     = 0x0131;
		WM_CTLCOLORMSGBOX         = 0x0132,
		WM_CTLCOLOREDIT           = 0x0133,
		WM_CTLCOLORLISTBOX        = 0x0134,
		WM_CTLCOLORBTN            = 0x0135,
		WM_CTLCOLORDLG            = 0x0136,
		WM_CTLCOLORSCROLLBAR      = 0x0137,
		WM_CTLCOLORSTATIC         = 0x0138,
		WM_MOUSEMOVE              = 0x0200,
		WM_MOUSEFIRST                     = 0x0200,
		WM_LBUTTONDOWN            = 0x0201,
		WM_LBUTTONUP              = 0x0202,
		WM_LBUTTONDBLCLK          = 0x0203,
		WM_RBUTTONDOWN            = 0x0204,
		WM_RBUTTONUP              = 0x0205,
		WM_RBUTTONDBLCLK          = 0x0206,
		WM_MBUTTONDOWN            = 0x0207,
		WM_MBUTTONUP              = 0x0208,
		WM_MBUTTONDBLCLK          = 0x0209,
		WM_MOUSEWHEEL             = 0x020A,
		WM_MOUSELAST             = 0x020D,
		//              public const uint WM_XBUTTONDOWN      = 0x020B;
		//              public const uint WM_XBUTTONUP        = 0x020C;
		//              public const uint WM_XBUTTONDBLCLK    = 0x020D;
		WM_PARENTNOTIFY           = 0x0210,
		WM_ENTERMENULOOP          = 0x0211,
		WM_EXITMENULOOP           = 0x0212,
		WM_NEXTMENU               = 0x0213,
		WM_SIZING                 = 0x0214,
		WM_CAPTURECHANGED         = 0x0215,
		WM_MOVING                 = 0x0216,
		//              public const uint WM_POWERBROADCAST   = 0x0218;
		WM_DEVICECHANGE           = 0x0219,
		WM_MDICREATE              = 0x0220,
		WM_MDIDESTROY             = 0x0221,
		WM_MDIACTIVATE            = 0x0222,
		WM_MDIRESTORE             = 0x0223,
		WM_MDINEXT                = 0x0224,
		WM_MDIMAXIMIZE            = 0x0225,
		WM_MDITILE                = 0x0226,
		WM_MDICASCADE             = 0x0227,
		WM_MDIICONARRANGE         = 0x0228,
		WM_MDIGETACTIVE           = 0x0229,
		/* D&D messages */
		//              public const uint WM_DROPOBJECT     = 0x022A;
		//              public const uint WM_QUERYDROPOBJECT  = 0x022B;
		//              public const uint WM_BEGINDRAG      = 0x022C;
		//              public const uint WM_DRAGLOOP       = 0x022D;
		//              public const uint WM_DRAGSELECT     = 0x022E;
		//              public const uint WM_DRAGMOVE       = 0x022F;
		WM_MDISETMENU             = 0x0230,
		WM_ENTERSIZEMOVE          = 0x0231,
		WM_EXITSIZEMOVE           = 0x0232,
		WM_DROPFILES              = 0x0233,
		WM_MDIREFRESHMENU         = 0x0234,
		WM_IME_SETCONTEXT         = 0x0281,
		WM_IME_NOTIFY             = 0x0282,
		WM_IME_CONTROL            = 0x0283,
		WM_IME_COMPOSITIONFULL    = 0x0284,
		WM_IME_SELECT             = 0x0285,
		WM_IME_CHAR               = 0x0286,
		WM_IME_REQUEST            = 0x0288,
		WM_IME_KEYDOWN            = 0x0290,
		WM_IME_KEYUP              = 0x0291,
		WM_MOUSEHOVER             = 0x02A1,
		WM_MOUSELEAVE             = 0x02A3,
		WM_CUT                    = 0x0300,
		WM_COPY                   = 0x0301,
		WM_PASTE                  = 0x0302,
		WM_CLEAR                  = 0x0303,
		WM_UNDO                   = 0x0304,
		WM_RENDERFORMAT           = 0x0305,
		WM_RENDERALLFORMATS       = 0x0306,
		WM_DESTROYCLIPBOARD       = 0x0307,
		WM_DRAWCLIPBOARD          = 0x0308,
		WM_PAINTCLIPBOARD         = 0x0309,
		WM_VSCROLLCLIPBOARD       = 0x030A,
		WM_SIZECLIPBOARD          = 0x030B,
		WM_ASKCBFORMATNAME        = 0x030C,
		WM_CHANGECBCHAIN          = 0x030D,
		WM_HSCROLLCLIPBOARD       = 0x030E,
		WM_QUERYNEWPALETTE        = 0x030F,
		WM_PALETTEISCHANGING      = 0x0310,
		WM_PALETTECHANGED         = 0x0311,
		WM_HOTKEY                 = 0x0312,
		WM_PRINT                  = 0x0317,
		WM_PRINTCLIENT            = 0x0318,
		WM_HANDHELDFIRST          = 0x0358,
		WM_HANDHELDLAST           = 0x035F,
		WM_AFXFIRST               = 0x0360,
		WM_AFXLAST                = 0x037F,
		WM_PENWINFIRST            = 0x0380,
		WM_PENWINLAST             = 0x038F,
		WM_APP                    = 0x8000,
		WM_USER                   = 0x0400,

		// Our "private" ones
		WM_MOUSE_ENTER            = 0x0401,
		WM_MOUSE_LEAVE            = 0x0402,
		WM_ASYNC_MESSAGE          = 0x0403,
		WM_REFLECT                = WM_USER + 0x1c00
	}
   
        [TestFixture]
        
        public class LabelTest2 
        {

		[Test]
		public void PubPropTest ()
		{
			Label l = new Label ();

			// A
			Assert.AreEqual (false, l.AutoSize, "A1");
			l.AutoSize = true;
			Assert.AreEqual (true, l.AutoSize, "A2");
			l.AutoSize = false;
			Assert.AreEqual (false, l.AutoSize, "A3");

			// B
			Assert.AreEqual (null, l.BackgroundImage, "B1");
			l.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			Assert.IsNotNull (l.BackgroundImage, "B2");
			Bitmap bmp = (Bitmap)l.BackgroundImage;
			Assert.IsNotNull (bmp.GetPixel (0, 0), "B3");

			Assert.AreEqual (BorderStyle.None, l.BorderStyle, "B4");
			l.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, l.BorderStyle, "B5");
			l.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, l.BorderStyle, "B6");
			l.BorderStyle = BorderStyle.None;
			Assert.AreEqual (BorderStyle.None, l.BorderStyle, "B7");
			
			// C
			string name = l.CompanyName;
			if (!name.Equals("Mono Project, Novell, Inc.") && !name.Equals("Microsoft Corporation")) {
				Assert.Fail("CompanyName property does not match any accepted value - C1");
			}
			
			
			// F
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "F1");
			l.FlatStyle = FlatStyle.Flat;
			Assert.AreEqual (FlatStyle.Flat, l.FlatStyle, "F1");
			l.FlatStyle = FlatStyle.Popup;
			Assert.AreEqual (FlatStyle.Popup, l.FlatStyle, "F2");
			l.FlatStyle = FlatStyle.Standard;
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "F3");
			l.FlatStyle = FlatStyle.System;
			Assert.AreEqual (FlatStyle.System, l.FlatStyle, "F4");
			
			// I
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "I1");
			l.ImageAlign = ContentAlignment.TopLeft;
			Assert.AreEqual (ContentAlignment.TopLeft, l.ImageAlign, "I2");
			l.ImageAlign = ContentAlignment.TopCenter;
			Assert.AreEqual (ContentAlignment.TopCenter, l.ImageAlign, "I3");
			l.ImageAlign = ContentAlignment.TopRight;
			Assert.AreEqual (ContentAlignment.TopRight, l.ImageAlign, "I4");
			l.ImageAlign = ContentAlignment.MiddleLeft;
			Assert.AreEqual (ContentAlignment.MiddleLeft, l.ImageAlign, "I5");
			l.ImageAlign = ContentAlignment.MiddleCenter;
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "I6");
			l.ImageAlign = ContentAlignment.MiddleRight;
			Assert.AreEqual (ContentAlignment.MiddleRight, l.ImageAlign, "I7");
			l.ImageAlign = ContentAlignment.BottomLeft;
			Assert.AreEqual (ContentAlignment.BottomLeft, l.ImageAlign, "I8");
			l.ImageAlign = ContentAlignment.BottomCenter;
			Assert.AreEqual (ContentAlignment.BottomCenter, l.ImageAlign, "I9");
			l.ImageAlign = ContentAlignment.BottomRight;
			Assert.AreEqual (ContentAlignment.BottomRight, l.ImageAlign, "I10");
			Assert.AreEqual (-1, l.ImageIndex, "I11");
			Assert.AreEqual (null, l.ImageList, "I12");
			Assert.AreEqual (null, l.Image, "I13");
			l.Image = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			Assert.IsNotNull (l.Image, "I14");
			bmp = (Bitmap)l.Image;
			Assert.IsNotNull (bmp.GetPixel (0, 0), "I15");
			
			
			ImageList il = new ImageList ();
			il.ColorDepth = ColorDepth.Depth32Bit;
			il.ImageSize = new Size (15, 15);
			il.Images.Add (Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png"));
			l.ImageList = il;
			l.ImageIndex = 0;
			
			Assert.AreEqual (0, l.ImageIndex, "I16");
			Assert.IsNotNull (l.ImageList, "I17");
			
			// PreferredHeight
			// PregerredWidth
			// RenderTransparent
			
			// T
			// Assert.AreEqual (false, l.TabStop, "T1");
			Assert.AreEqual (ContentAlignment.TopLeft, l.TextAlign, "T2");
			
			// U
			Assert.AreEqual (true, l.UseMnemonic, "U1");
			l.UseMnemonic = false;
			Assert.AreEqual (false, l.UseMnemonic, "U2");
		}

                        
		[Test]
		public void LabelEqualsTest () 
		{
			Label s1 = new Label ();
			Label s2 = new Label ();
			s1.Text = "abc";
			s2.Text = "abc";
			Assert.AreEqual (false, s1.Equals (s2), "E1");
			Assert.AreEqual (true, s1.Equals (s1), "E2");
		}
		                        
		[Test]
		public void LabelScaleTest () 
		{
			Label r1 = new Label ();
			r1.Width = 40;
			r1.Height = 20 ;
			r1.Scale (2);
			Assert.AreEqual (80, r1.Width, "W1");
			Assert.AreEqual (40, r1.Height, "H1");
		}
				
		[Test]
		public void PubMethodTest ()
		{
			Label l = new Label ();
			
			l.Text = "My Label";
			
			Assert.AreEqual ("System.Windows.Forms.Label, Text: My Label", l.ToString (), "T1");
			  
		}
	}
   
        [TestFixture]
        public class LabelEventTest
        {
		static bool eventhandled = false;
		public void Label_EventHandler (object sender,EventArgs e)
		{
			eventhandled = true;
		}
		
		[Test]
		public void AutoSizeChangedChangedTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.AutoSizeChanged += new EventHandler (Label_EventHandler);
			l.AutoSize = true;
			Assert.AreEqual (true, eventhandled, "B4");
			eventhandled = false;			
		}
		
		[Test]
		public void BackgroundImageChangedTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.BackgroundImageChanged += new EventHandler (Label_EventHandler);
			l.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			Assert.AreEqual (true, eventhandled, "B4");
			eventhandled = false;			
		}
		
		[Test]
		public void ImeModeChangedTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.ImeModeChanged += new EventHandler (Label_EventHandler);
			l.ImeMode = ImeMode.Katakana;
			Assert.AreEqual (true, eventhandled, "I16");
			eventhandled = false;
		}

		[Test, Ignore ("This isnt complete.")]
		public void KeyDownTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.KeyDown += new KeyEventHandler (Label_EventHandler);
			
			//Assert.AreEqual (true, eventhandled, "K1");
			eventhandled = false;
		}


		[Test, Ignore ("This is failing.")]
		public void TabStopChangedTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.TabStopChanged += new EventHandler (Label_EventHandler);
			l.TabStop = true;
			Assert.AreEqual (true, eventhandled, "T3");
			eventhandled = false;
		}
		
		[Test]
		public void TextAlignChangedTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			Label l = new Label ();
			l.Visible = true;
			myform.Controls.Add (l);
			l.TextAlignChanged += new EventHandler (Label_EventHandler);
			l.TextAlign = ContentAlignment.TopRight;
			Assert.AreEqual (true, eventhandled, "T4");
			eventhandled = false;
		}
	}
   
        public class MyLabel : Label
        {
	        private ArrayList results = new ArrayList ();
	        public MyLabel () : base ()
	        {
			// TODO: we need to add those later.
			//this.HandleCreated += new EventHandler (HandleCreatedHandler);
			//this.BindingContextChanged += new EventHandler (BindingContextChangedHandler);
			//this.Invalidated += new InvalidateEventHandler (InvalidatedHandler);
			//this.Resize += new EventHandler (ResizeHandler);
			//this.SizeChanged += new EventHandler (SizeChangedHandler);
			//this.Layout += new LayoutEventHandler (LayoutHandler);
			//this.VisibleChanged += new EventHandler (VisibleChangedHandler);
			//this.Paint += new PaintEventHandler (PaintHandler);
		}
		
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
			results.Add ("OnKeyDown");
			base.OnKeyDown (e);
		}
		
		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			results.Add ("OnKeyPress");
			base.OnKeyPress (e);
		}
		
		protected override void OnKeyUp (KeyEventArgs e)
		{
			results.Add ("OnKeyUp");
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
        public class LabelTestEventsOrder
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
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);			
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.Size = new Size (150, 20);
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.AutoSize = true;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.BackgroundImage = Image.FromFile ("Test/System.Windows.Forms/bitmaps/a.png");
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.ImeMode = ImeMode.Katakana;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		}
		
		[Test]
		public void KeyDownEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged",
				  "OnKeyDown",
				  "OnKeyPress"
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.KeyDownA ();
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		}
		
		[Test]
		public void KeyPressEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged",
				  "OnKeyDown",
				  "OnKeyPress",
				  "OnKeyUp"				
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.KeyPressA ();
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		}
		
		[Test]
		public void KeyUpEventsOrder ()
		{
			string[] EventsWanted = {
				"OnHandleCreated",
				  "OnBindingContextChanged",
				  "OnBindingContextChanged",		
				  "OnKeyUp"		
			};  		
			Form myform = new Form ();
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.KeyUpA ();
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.TabStop = true;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
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
			myform.Visible = true;
			MyLabel l = new MyLabel ();			
			myform.Controls.Add (l);
			l.TextAlign = ContentAlignment.TopRight;
			
			Assert.AreEqual (EventsWanted, ArrayListToString (l.Results));
		}		
	}      
}
	   
