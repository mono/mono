    //
    // System.Windows.Forms.Form
    //
    // Author:
    //   Miguel de Icaza (miguel@ximian.com)
    //   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
    //  Dennis Hayes (dennish@raytek.com)
    //   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
    //  Aleksey Ryabchuk (ryabchuk@yahoo.com)
    // (C) 2002/3 Ximian, Inc
    //

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;

using Gtk;
using GtkSharp;
namespace System.Windows.Forms
{

	public class Form : ContainerControl
	{
		private Gtk.RadioButton firstRadioButton;
		private int radioButtonsCount = 0;
		internal Gtk.Window win;
		
		
		DialogResult dialogResult;
		Size maximumSize;
		Size minimumSize;
		double opacity;
		bool autoScale;

		private bool controlBox;
		private bool minimizeBox;
		private bool maximizeBox;
		private bool helpButton;

		FormBorderStyle formBorderStyle;

		// End of temperay varibles


		void delete_cb (object o, DeleteEventArgs args){
			if (Closed != null)
				Closed (o, args);
		}
		
		void load_cb (object o, EventArgs args){
			if (Load != null)
				Load (this, EventArgs.Empty);
		}

		void activate_cb (object o, EventArgs args){
			if (Activated != null)
				Activated (o, args);
		}

		internal void ConnectEvents (){
			win.DefaultActivated += new EventHandler (activate_cb);
			win.DeleteEvent += new DeleteEventHandler (delete_cb);
			win.Realized += new EventHandler (load_cb);
		}

		internal override Gtk.Widget CreateWidget ()
		{
			Gtk.Widget contents = base.CreateWidget ();
			win = new Gtk.Window (WindowType.Toplevel);
			win.Title = Text;
			win.Add(contents);
			this.ConnectEvents();
			return (Widget) win;
		}





		internal class MdiClient:Control
		{
			public MdiClient (Control parent):base (parent, "")
			{
			}
			public void DestroyControl ()
			{
				DestroyHandle ();
			}
		}

		MdiClient mdiClientWnd;
		Form mdiParent;
		Control dialog_owner;
		bool modal;
		bool exitModalLoop;
		Size autoScaleBaseSize;
		bool keyPreview;
		bool showInTaskbar;
		bool topMost;
		SizeGripStyle sizeGripStyle;
		FormStartPosition formStartPosition;
		FormWindowState formWindowState;
		IButtonControl acceptButton;
		IButtonControl cancelButton;
		MainMenu mainMenu;

		public Form ():base (){
			opacity = 1.00;
			TopLevel = true;
			modal = false;
			dialogResult = DialogResult.None;
			autoScale = true;
			formBorderStyle = FormBorderStyle.Sizable;
			sizeGripStyle = SizeGripStyle.Auto;
			maximizeBox = true;
			minimizeBox = true;
			controlBox = true;
			keyPreview = false;
			showInTaskbar = true;
			topMost = false;
			helpButton = false;
			maximumSize = Size.Empty;
			minimumSize = Size.Empty;
			formStartPosition =
				FormStartPosition.WindowsDefaultLocation;
			Visible = false;
			formWindowState = FormWindowState.Normal;
			acceptButton = null;
			cancelButton = null;
		}

		//  --- Public Properties
		//
		[MonoTODO] 
		public IButtonControl AcceptButton{
			get{return acceptButton;}
			set	{acceptButton = value;}
		}

		[MonoTODO] 
		public static Form ActiveForm{
			get{
				/*Control control = Control.FromChildHandle ( Win32.GetActiveWindow ( ) );
				 * if ( control != null && ! ( control is Form ) )
				 * control = control.getParentForm ( );
				 * return control as Form; */
				//throw new NotImplementedException ();
				return null;
			}
		}

		[MonoTODO] 
		public Form ActiveMdiChild{
			get{throw new NotImplementedException ();}
		}

		[MonoTODO] 
		public bool AutoScale{
			get{return autoScale;}
			set{autoScale = value;}
		}

		[MonoTODO] 
		public virtual Size AutoScaleBaseSize{
			get{return autoScaleBaseSize;}
			set{autoScaleBaseSize = value;}
		}


		[MonoTODO] 
		public IButtonControl CancelButton{
			get{return cancelButton;}
			set{cancelButton = value;}
		}

		[MonoTODO] 
		public new Size ClientSize{
			get{return base.ClientSize;}
			set{base.ClientSize = value;}
		}

		public bool ControlBox{
			get{return controlBox;}
			set{}
		}


		[MonoTODO] 
		public Rectangle DesktopBounds{
			get{throw new NotImplementedException ();}
			set{//FIXME:
			}
		}

		[MonoTODO] 
		public Point DesktopLocation{
			get{throw new NotImplementedException ();}
			set{SetDesktopLocation (value.X, value.Y);}
		}

		//Compact Framework
		[MonoTODO] 
		public DialogResult DialogResult{
			get{return dialogResult;}
			set{
				// FIXME
			}
		}


		public FormBorderStyle FormBorderStyle{
			get{return formBorderStyle;}
			set{}
		}


		public virtual bool HelpButton{
			get{return helpButton;}
			set{}
		}


		//Compact Framework
		//[MonoTODO]
		public virtual System.Drawing.Icon Icon	{
			get	{throw new NotImplementedException ();}
			set{
				//FIXME:
			}
		}

		public virtual bool IsMdiChild{
			get{return mdiParent != null;}
		}

		public virtual bool KeyPreview{
			get{return keyPreview;}
			set{keyPreview = value;}
		}

		public bool MaximizeBox{
			get{return maximizeBox;}
			set{}
		}


		[MonoTODO] 
		public Size MaximumSize{
			get{return maximumSize;}
			set{
				if (value.Width < 0 || value.Height < 0)
					throw new
						ArgumentOutOfRangeException
						("value");

				if (maximumSize != value){
					maximumSize = value;

					if (minimumSize != Size.Empty)
					{
						if (maximumSize.Width <
						    minimumSize.Width)
							minimumSize.Width =
								maximumSize.
								Width;
						if (maximumSize.Height <
						    minimumSize.Height)
							minimumSize.Height =
								maximumSize.
								Height;
					}

					Size = Size;
					OnMaximumSizeChanged (EventArgs.Empty);
				}
			}
		}

		[MonoTODO] 
		public Form[] MdiChildren{
			get{
				Form[]forms = new Form[0];
				return forms;
			}
		}

		[MonoTODO] 
		public virtual Form MdiParent{
			get{return mdiParent;}
			set{
				/*if (!value.IsMdiContainer
				    || (value.IsMdiContainer
					&& value.IsMdiChild))
					throw new Exception ();

				mdiParent = value;
				mdiParent.MdiClientControl.Controls.
					Add (this);

				if (mdiParent.IsHandleCreated)
					CreateControl ();*/
			}
		}




		public MainMenu Menu{
			get{ return mainMenu; }
			set{
				if (value == mainMenu){
					return;
				}
				if (mainMenu != null){
					this.vbox.Remove (mainMenu.Widget);
					this.vbox.ShowAll();
					mainMenu.RemoveFromForm();
				}
				if (value != null){
					this.vbox.PackStart(value.Widget, false, false, 0);
					this.vbox.ReorderChild(value.Widget, 0);
					this.vbox.ShowAll();
					value.AddToForm (this);
				}
				
				mainMenu = value;			
			}
		}


		public bool MinimizeBox{
			get{return minimizeBox;}
			set{}
		}


		[MonoTODO] 
		public Size MinimumSize{
			get{return minimumSize;}
			set{
				if (value.Width < 0 || value.Height < 0)
					throw new
						ArgumentOutOfRangeException
						("value");

				if (minimumSize != value)
				{
					minimumSize = value;

					if (maximumSize != Size.Empty)
					{
						if (minimumSize.Width >
						    maximumSize.Width)
							maximumSize.Width =
								minimumSize.
								Width;
						if (minimumSize.Height >
						    maximumSize.Height)
							maximumSize.Height =
								minimumSize.
								Height;
					}

					Size = Size;
					OnMinimumSizeChanged (EventArgs.
							      Empty);
				}
			}
		}

		public bool Modal{
			get{return modal;}
		}

		[MonoTODO] 
		public double Opacity{
			get{return opacity;}
			set{opacity = value;}
		}

		[MonoTODO] 
		public virtual bool IsMdiContainer{
			get{return mdiClientWnd != null;}
			set	{
				/*if (value)
					createMdiClient ();
				else
					destroyMdiClient ();*/
			}
		}

		[MonoTODO]
		public Form[] OwnedForms{
			get{throw new NotImplementedException ();}
		}

		[MonoTODO] 
		public Form Owner{
			get{throw new NotImplementedException ();}
			set{
				//FIXME:
			}
		}

		[MonoTODO] 
		public bool ShowInTaskbar{
			get{return true;}
			set{return;}
			/*get { return showInTaskbar; }
			 * set {
			 * if ( showInTaskbar != value ) {
			 * showInTaskbar = value;
			 * RecreateHandle ( );
			 * Visible = Visible;
			 * }
			 * } */
		}


		/*public virtual ISite Site {
		 * get {
		 * return base.Site;
		 * }
		 * set {
		 * base.Site = value;
		 * }
		 * } */

		[MonoTODO] 
		public SizeGripStyle SizeGripStyle{
			get{return sizeGripStyle;}
			set{
				if (!Enum.IsDefined (typeof (SizeGripStyle), value))
					throw new
						InvalidEnumArgumentException
						("SizeGripStyle", (int) value,
						 typeof (SizeGripStyle));

				sizeGripStyle = value;
			}
		}

		public FormStartPosition StartPosition{
			get{return formStartPosition;}
			set	{
				if (!Enum.IsDefined (typeof (FormStartPosition),value))
					throw new
						InvalidEnumArgumentException
						("StartPosition", (int) value,
						 typeof (FormStartPosition));

				formStartPosition = value;
			}
		}

		[MonoTODO] 
		public bool TopLevel{
			get{return GetTopLevel ();}
			set{SetTopLevel (value);}
		}
		[MonoTODO]
		public bool TopMost{
			get{return topMost;}
			set{}
		}

		[MonoTODO] 
		public Color TransparencyKey{
			get{throw new NotImplementedException ();}
			set{
				//FIXME:
			}
		}


		//Compact Framework
		[MonoTODO] 
		public FormWindowState WindowState{
			get{return formWindowState;}
			set{
				if (!Enum.IsDefined (typeof (FormWindowState),value))
					throw new
						InvalidEnumArgumentException
						("WindowState", (int) value,
						 typeof (FormWindowState));

				formWindowState = value;
			}
		}


		//  --- Public Methods
		public void Activate (){
			//Win32.SetActiveWindow (Handle);
		}

		[MonoTODO] 
		public void AddOwnedForm (Form ownedForm){
			//FIXME:
		}

		//Compact Framework
		public void Close (){
			//Win32.DestroyWindow (Handle);
			//if ( IsHandleCreated )
			//      Win32.PostMessage ( Handle, Msg.WM_CLOSE, 0, 0 );
		}

		public void LayoutMdi (MdiLayout value){
			/*if ( IsMdiContainer && mdiClientWnd.IsHandleCreated ) {
			 * int mes = 0;
			 * int wp  = 0;
			 * 
			 * switch ( value ) {
			 * case MdiLayout.Cascade:
			 * mes = (int)Msg.WM_MDICASCADE;
			 * break;
			 * case MdiLayout.ArrangeIcons:
			 * mes = (int)Msg.WM_MDIICONARRANGE;
			 * break;
			 * case MdiLayout.TileHorizontal:
			 * mes = (int)Msg.WM_MDITILE;
			 * wp = 1;
			 * break;
			 * case MdiLayout.TileVertical:
			 * mes = (int)Msg.WM_MDITILE;
			 * break;
			 * }
			 * 
			 * if ( mes != 0 )
			 * Win32.SendMessage ( mdiClientWnd.Handle, mes, wp, 0 );
			 * } */
		}

		[MonoTODO] 
		public void RemoveOwnedForm (Form ownedForm){
			//FIXME:
		}


		public void SetDesktopLocation (int x, int y){
			/*Win32.SetWindowPos ((IntPtr) Handle, SetWindowPosZOrder.HWND_TOPMOST, 
			 * x, y, 0, 0, 
			 * SetWindowPosFlags.SWP_NOSIZE | 
			 * SetWindowPosFlags.SWP_NOZORDER); */
		}

		[MonoTODO]
		public void SetDesktopBounds (int x, int y, int width,int height){
			///implmentation from setwindow location.
			///not sure width and height were added correctly
			/*Win32.SetWindowPos ((IntPtr) Handle, SetWindowPosZOrder.HWND_TOPMOST, 
			 * x, y, width, height, 
			 * SetWindowPosFlags.SWP_NOZORDER); */
		}
		[MonoTODO] 
		public DialogResult ShowDialog (IWin32Window owner){
			//IWin32Window has 1 public prop, "Handle"
			//use this knowledge to modify the () version for this version
			throw new NotImplementedException ();
		}

		[MonoTODO] 
		public DialogResult ShowDialog (){
			return DialogResult;
		}

		public override string ToString (){
			return GetType ().FullName.ToString () + ", Text: " +Text;
		}

		[MonoTODO] 
		protected override void UpdateDefaultButton (){
			base.UpdateDefaultButton ();
		}

		//  --- Public Events

		public event EventHandler Activated;

		public event EventHandler Closed;
		public event CancelEventHandler Closing;

		//Compact Framework
		// CancelEventHandler not yet implemented/stubbed
		//public event CancelEventHandler Closing;

		public event EventHandler Deactivate;
		public event InputLanguageChangedEventHandler
			InputLanguageChanged;
		public event InputLanguageChangingEventHandler
			InputLanguageChanging;

		//Compact Framework
		public event EventHandler Load;

		public event EventHandler MaximizedBoundsChanged;
		public event EventHandler MaximumSizeChanged;
		public event EventHandler MdiChildActivate;
		public event EventHandler MenuComplete;
		public event EventHandler MenuStart;
		public event EventHandler MinimumSizeChanged;


		//  --- Protected Properties

		[MonoTODO] 
		public bool AllowTransparency{
			get{
				return false;
				//throw new NotImplementedException();
			}
			set{
				//fixme
			}
		}

		protected override Size DefaultSize{
			get{return new Size (300, 300);}
		}
		
		

		[MonoTODO] 
		protected Rectangle MaximizedBounds{
			get{throw new NotImplementedException ();}
			set{
				//FIXME:
			}
		}


		//  --- Protected Methods



		[MonoTODO] 
		public bool IsRestrictedWindow{
			get{throw new NotImplementedException ();}
		}

		[MonoTODO] 
		public MainMenu MergedMenu{
			get{throw new NotImplementedException ();}
		}

		//Compact Framework
		protected virtual void OnActivated (EventArgs e){
			Activated (this, e);
		}

		//Compact Framework
		protected virtual void OnClosed (EventArgs e){
			if (Closed != null)
				Closed (this, e);
		}

		//Compact Framework
		protected virtual void OnClosing (CancelEventArgs e){
			if (Closing != null)
				Closing (this, e);

			if (Modal){
				e.Cancel = true;	// don't destroy modal form
				DialogResult = DialogResult.Cancel;
			}
		}
		protected override void OnControlAdded (ControlEventArgs e){
			base.OnControlAdded(e);
			if (e.Control is RadioButton){
				if (radioButtonsCount == 0)
					firstRadioButton = e.Control.Widget as Gtk.RadioButton;
				else
					(e.Control.Widget as Gtk.RadioButton).Group = 
						firstRadioButton.Group;
				radioButtonsCount++;
			}		
		}
		[MonoTODO]
		protected override void OnControlRemoved (ControlEventArgs e){	
			base.OnControlRemoved (e);
		}

		protected override void OnCreateControl (){
		}

		[MonoTODO] 
		protected virtual void OnDeactivate (EventArgs e){

		}



		protected virtual void
			OnInputLanguageChanged (InputLanguageChangedEventArgs e){
			
			if (InputLanguageChanged != null)
				InputLanguageChanged (this, e);
		}

		protected virtual void
			OnInputLanguageChanging	(InputLanguageChangingEventArgs e){
			
			if (InputLanguageChanging != null)
				InputLanguageChanging (this, e);
		}

		//Compact Framework
		protected virtual void OnLoad (EventArgs e){
			if (Load != null)
				Load (this, e);
		}

		protected virtual void OnMaximizedBoundsChanged (EventArgs e){
			if (MaximizedBoundsChanged != null)
				MaximizedBoundsChanged (this, e);
		}

		protected virtual void OnMaximumSizeChanged (EventArgs e){
			if (MaximumSizeChanged != null)
				MaximumSizeChanged (this, e);
		}

		protected virtual void OnMdiChildActivate (EventArgs e){
			if (MdiChildActivate != null)
				MdiChildActivate (this, e);
		}

		protected virtual void OnMenuComplete (EventArgs e){
			if (MenuComplete != null)
				MenuComplete (this, e);
		}

		protected virtual void OnMenuStart (EventArgs e){
			if (MenuStart != null)
				MenuStart (this, e);
		}

		protected virtual void OnMinimumSizeChanged (EventArgs e){
			if (MinimumSizeChanged != null)
				MinimumSizeChanged (this, e);
		}

		//Compact Framework
		protected override void OnPaint (PaintEventArgs e){
			base.OnPaint (e);
		}

		//Compact Framework
		protected override void OnResize (EventArgs e){
			base.OnResize (e);
			//                      resizeMdiClient ();
		}

		protected override void OnStyleChanged (EventArgs e){
			base.OnStyleChanged (e);
		}

		//Compact Framework
		protected override void OnTextChanged (EventArgs e){
			base.OnTextChanged (e);
			win.Title = Text;
		}

		protected override void OnVisibleChanged (EventArgs e){
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessDialogKey (Keys keyData){
			if (keyData == Keys.Enter && AcceptButton != null){
				AcceptButton.PerformClick ();
				return true;
			}
			if (keyData == Keys.Escape && CancelButton != null){
				CancelButton.PerformClick ();
				return true;
			}
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessKeyPreview (ref Message m){
			if (KeyPreview)
				return ProcessKeyEventArgs (ref m);

			return false;
		}

		protected override bool ProcessTabKey (bool forward){
			return base.ProcessTabKey (forward);
		}

		protected override void ScaleCore (float x, float y){
			ClientSize =
				new Size ((int) (ClientSize.Width * x),
					  (int) (ClientSize.Height * y));
		}

		[MonoTODO]
		protected override void Select (bool directed,bool forward){
			base.Select (directed, forward);
		}

		protected override void SetBoundsCore (int x, int y,
						       int width, int height,
						       BoundsSpecified
						       specified)
		{
			if (MaximumSize != Size.Empty)
			{
				if (width > MaximumSize.Width)
					width = MaximumSize.Width;
				if (height > MaximumSize.Height)
					height = MaximumSize.Height;
			}
			if (MinimumSize != Size.Empty)
			{
				if (width < MinimumSize.Width)
					width = MinimumSize.Width;
				if (height < MinimumSize.Height)
					height = MinimumSize.Height;
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetClientSizeCore (int x, int y){
			base.SetClientSizeCore (x, y);
		}

		protected override void SetVisibleCore (bool value){
			base.SetVisibleCore (value);
		}


		[MonoTODO] 
		protected void ApplyAutoScaling (){
			throw new NotImplementedException ();
		}

		[MonoTODO] 
		protected void CenterToScreen (){
			throw new NotImplementedException ();
		}

		[MonoTODO] 
		protected void CenterToParent (){
			throw new NotImplementedException ();
		}

		[MonoTODO] 
		protected override void Dispose (bool disposing){
			base.Dispose (disposing);
		}


		[MonoTODO] 
		public static SizeF GetAutoScaleSize (Font font){
			throw new NotImplementedException ();
		}
	}
}
