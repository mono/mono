//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Rachel Hestilow (hestilow@ximian.com)
//   Joel Basson  (jstrike@mweb.co.za)
//   Philip Van Hoof (me@freax.org)
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using Gtk;
using GtkSharp;
using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms{
	public class Control:Component, ISynchronizeInvoke, IWin32Window{

		private ControlCollection controls;
		private Size clientSize;
		private bool allowDrop = false;
		private Control parent;
		private string text, name;
		private Size size;
		private int left, top, width, height, tabindex, index, tag;		
		private Point location = new System.Drawing.Point (0, 0);		
		private AnchorStyles anchor = AnchorStyles.Top | AnchorStyles.Left;
		private bool tabStop = true;
		private RightToLeft rightToLeft;
		private DockStyle dock = DockStyle.None;
		private ContextMenu contextMenu;
		
		
		
		internal Widget widget;
		internal Gtk.Layout layout = null;
		internal Gtk.VBox vbox = null;
		
		

		static Control (){
			Gtk.Application.Init ();
		}

		public Control ():this (""){
		}

		public Control (string text):this (null, text){
		}

		public Control (Control parent, string text){
			this.Parent = parent;
			this.text = text;
			clientSize = DefaultSize;

		}

		public Control (string text, int left, int top, int width,int height){
			clientSize = DefaultSize;
		}

		public Control (Control parent, string text, int left,
				int top, int width, int height)
		{
			this.Parent = parent;
			this.text = text;
			this.left = left;
			this.top = top;
			this.width = width;
			this.height = height;
			clientSize = DefaultSize;
		}

		[MonoTODO] 
		public AccessibleObject AccessibilityObject{
			get	{return null;}
		}
		[MonoTODO] 
		public String AccessibleDefaultActionDescription{
			get	{return "";}
			set	{return;}
		}
		[MonoTODO] 
		public String AccessibleDescription{
			get{return "";}
			set{return;	}

		}
		[MonoTODO] 
		public String AccessibleName{
			get	{return "";	}
			set	{return;}
		}
		[MonoTODO] 
		public AccessibleRole AccessibleRole	{
			get	{throw new NotImplementedException ();}
			set	{return;}
		}

		
		[MonoTODO] 
		public virtual bool AllowDrop{
			get{return allowDrop;}
			set{allowDrop = value;}
		}
		[MonoTODO]
		public virtual AnchorStyles Anchor{
			get	{return anchor;	}
			set{anchor = value;	}
		}

		[MonoTODO] 
		public virtual Color BackColor{
			get	{throw new NotImplementedException ();}
			set{
				// ?
				Gdk.Color c = new Gdk.Color (value);
				this.Widget.ModifyBg (Gtk.StateType.Normal, c);
			}
		}
		[MonoTODO] 
		public virtual System.Drawing.Image BackgroundImage{
			get{throw new NotImplementedException ();}
			set	{
				return;
				//throw new NotImplementedException();
			}
		}

		[MonoTODO] 
		public virtual BindingContext BindingContext{
			get{throw new NotImplementedException ();}
			set{return;}
		}
		public virtual int Bottom{
			get	{return (Top + Height);	}
		}
		[MonoTODO] 
		public virtual Rectangle Bounds{
			get	{throw new NotImplementedException ();}
			set{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		public virtual bool CanFocus{
			get	{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		public virtual bool CanSelect{
			get	{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		public virtual bool Capture{
			get	{throw new NotImplementedException ();	}
			set	{throw new NotImplementedException ();}

		}
		[MonoTODO] 
		public virtual bool CausesValidation{
			get{return true;}
			set{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		public virtual Rectangle ClientRectangle	{
			get	{throw new NotImplementedException ();	}
		}

		[MonoTODO] 
		public virtual Size ClientSize{
			get{return clientSize;}
			set{
				clientSize = value;
				OnResize (EventArgs.Empty);
				Widget.SetSizeRequest (value.Width, value.Height);
			}
		}
		public virtual String CompanyName{
			get{return Application.CompanyName;}
		}
		[MonoTODO]
		public virtual bool ContainsFocus{
			get	{throw new NotImplementedException ();}
		}
		
		[MonoTODO] 
		public virtual ContextMenu ContextMenu{
			get{return contextMenu;}
			set{contextMenu = value;}
		}

		public virtual ControlCollection Controls{
			get	{
				if (controls == null)
					controls = new ControlCollection (this);
				return controls;
			}
		}
		[MonoTODO] 
		public virtual bool Created
		{
			get{return true;}
		}
		[MonoTODO] 
		public virtual Cursor Cursor{
			get{throw new NotImplementedException ();}
			set{throw new NotImplementedException ();}

		}
		[MonoTODO] 
		public ControlBindingsCollection DataBindings{
			get{throw new NotImplementedException ();}
		}
		public static Color DefaultBackColor{
			get	{return SystemColors.Control;}
		}
		[MonoTODO] 
		public static Font DefaultFont{
			//get{ return new Font (FontFamily.GenericSansSerif, 10.0); }
			get {throw new NotImplementedException(); }
		}
		public static Color DefaultForeColor{
			get	{return SystemColors.ControlText;}
		}
		[MonoTODO] 
		public virtual Rectangle DisplayRectangle{
			get{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		public bool Disposing{
			get{return false;}
		}
		[MonoTODO]
		public virtual DockStyle Dock{
			get	{return dock;}
			set {dock = value;}
		}
		public bool Enabled{
			get	{return Widget.Sensitive;}
			set	{Widget.Sensitive = value;}
		}
		[MonoTODO] 
		public virtual bool Focused{
			get	{throw new NotImplementedException ();}
		}

		[MonoTODO] 
		public virtual Font Font{
			// TODO: implementar get.
			get	{return null;}
			set {this.Widget.ModifyFont (SWFGtkConv.Font (value));}
		}
		[MonoTODO]
		public virtual Color ForeColor{
			get{throw new NotImplementedException ();}
			set{this.Widget.ModifyFg (Gtk.StateType.Normal,  new Gdk.Color (value));}
		}

		[MonoTODO] 
		public IntPtr Handle{
			get{return IntPtr.Zero;}
		}
		public virtual bool HasChildren{
			get{return (Controls.Count > 0);}
		}
		public int Height
		{
			get	{return Size.Height;}
			set {
				Size s = Size;
				s.Height = value;
				this.Size = s;
			}
		}
		[MonoTODO] 
		public ImeMode ImeMode{
			get{return ImeMode.Disable;}
			set{return;	}
		}
		[MonoTODO]
		public int Index{
			get{return index;}
			set{index = value;}
		}

		[MonoTODO] 
		public virtual bool InvokeRequired
		{
			get {return false;}
		}

		[MonoTODO] 
		public bool IsAccessible{
			get{return true;}
			set{return;	}
		}
		[MonoTODO] 
		public bool IsDisposed
		{
			get{return false;}
		}
		[MonoTODO] 
		public bool IsHandleCreated
		{
			get	{return true;}
		}

		public int Left
		{
			get{return Location.X;}
			set{
				Point p = Location;
				p.X = value;
				this.Location = p;
			}
		}

		public Point Location
		{
			get{return location;}
			set{
				location = value;
				OnLocationChanged (EventArgs.Empty);								
			}
		}
		[MonoTODO] 
		public static Keys ModifierKeys{
			get{return Keys.None;}
		}
		[MonoTODO] 
		public static MouseButtons MouseButtons{
			get{return MouseButtons.None;}
		}
		[MonoTODO] 
		public static Point MousePosition{
			get{ return new Point (0, 0);}
		}
		public virtual string Name{
			get{return name;}
			set{
				name = value;
				Widget.Name = value;
			}
		}

		[MonoTODO] 
		public virtual Control Parent{
			get{return parent;}
			set{
				if (parent == value){
					return;
				}
				if (parent != null){
					parent.Controls.Remove (this);
				}
				if (value != null){
					value.Controls.Add (this);
				}
				parent = value;
			}
		}
		public virtual String ProductName{
			get{return Application.ProductName;}
		}
		public virtual String ProductVersion{
			get{return Application.ProductVersion;}
		}
		[MonoTODO] 
		public virtual bool RecreatingHandle{
			get{return false;}
		}
		[MonoTODO] 
		public virtual Region Region{
			get{throw new NotImplementedException ();}
			set{throw new NotImplementedException ();}
		}
		public virtual int Right{
			get{ return Left + Width;}
		}
		[MonoTODO] 
		public virtual RightToLeft RightToLeft{
			get{return rightToLeft;}
			set{rightToLeft = value;}
		}

		public virtual Size Size{
			get{return size;}
			set{
				size = value;				
				OnResize(EventArgs.Empty);
				Widget.SetSizeRequest (value.Width, value.Height);
			}
		}
		[MonoTODO]
		public virtual int TabIndex{
			get{return tabindex;}
			set{tabindex = value;}
		}
		[MonoTODO]
		public virtual bool TabStop{
			get{return tabStop;}
			set{tabStop = value;}
		}
		[MonoTODO]
		public virtual int Tag{
			get{return tag;}
			set{tag = value;}
		}
		public virtual string Text{
			get{return text;}
			set{
				text = value;
				OnTextChanged (EventArgs.Empty);
			}
		}
		public virtual int Top{
			get{return location.Y;}
			set{
				Point p = this.Location;
				p.Y = value;
				this.Location = p;
			}
		}
		[MonoTODO]
		public virtual Control TopLevelControl{
			get{
				Control c = this;
				while (c.Parent != null){
					c = c.Parent;
				}
				return c;
			}
		}
		public virtual bool Visible{
			get{return Widget.Visible;}
			set{Widget.Visible = value;}
		}

		[MonoTODO] public virtual int Width{
			get	{return Size.Width;}
			set	{
				Size s = this.Size;
				s.Width = value;
				this.Size = s;
			}
		}

		// Protected properties

		[MonoTODO] 
		protected virtual CreateParams CreateParams{
			get{return null;}
		}
		[MonoTODO] 
		protected virtual ImeMode DefaultImeMode{
			get	{return ImeMode.Disable;}
		}

		protected virtual Size DefaultSize{
			get	{return new Size (100, 100);}
		}
		[MonoTODO] 
		protected virtual int FontHeight{
			get	{throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}
		[MonoTODO] 
		protected virtual bool ResizeRedraw{
			get{throw new NotImplementedException ();}
			set{throw new NotImplementedException ();}
		}
		[MonoTODO] 
		protected virtual bool ShowFocusCues{
			get{return false;}
		}
		[MonoTODO] 
		protected virtual bool ShowKeyboardCues{
			get{return false;}
		}


		// Metodos 

		[MonoTODO]
		protected void	AccessibilityNotifyClients (AccessibleEvents accEvent,  int childID){
		}
		[MonoTODO]
		public virtual IAsyncResult BeginInvoke (Delegate method){
			return null;
		}
		[MonoTODO]
		public virtual IAsyncResult BeginInvoke (Delegate method, object[]args){
			return null;
		}

		[MonoTODO] 
		public virtual void BringToFront (){
		}
		[MonoTODO] 
		public virtual bool Contains (Control c){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual AccessibleObject	CreateAccessibilityInstance (){
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		public virtual void CreateControl (){
		}
		[MonoTODO]
		protected virtual ControlCollection	CreateControlInstance (){
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		public virtual Graphics CreateGraphics (){
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		protected virtual void CreateHandle (){
		}
		[MonoTODO] 
		protected virtual void DefWndProc (Message m){
		}
		[MonoTODO] 
		protected virtual void DestroyHandle (){
		}
		[MonoTODO] 
		protected override void Dispose (bool disposing){
			base.Dispose (disposing);
		}
		[MonoTODO]
		public DragDropEffects DoDragDrop (object data, DragDropEffects allowedEffects){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual object EndInvoke (IAsyncResult asyncResult){
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		public virtual Form FindForm (){
			return null;
		}
		[MonoTODO] 
		public virtual bool Focus (){
			return false;
		}
		[MonoTODO] 
		public static Control FromHandle (IntPtr handle){
			return null;
		}

		[MonoTODO]
		public static Control FromChildHandle (IntPtr handle){
			return null;
		}
		[MonoTODO] 
		public virtual Control GetChildAtPoint (Point pt){
			return null;
		}
		[MonoTODO]
		public virtual IContainerControl GetContainerControl (){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual Control GetNextControl (Control ctl, bool forward){
			return null;
		}
		[MonoTODO]
		protected virtual bool GetStyle (ControlStyles flag){
			return true;
		}
		[MonoTODO] 
		protected virtual bool GetTopLevel (){
			throw new NotImplementedException ();
		}
		public void Hide (){
			Widget.Hide ();
		}
		[MonoTODO] 
		protected virtual void InitLayout (){
		}
		[MonoTODO] 
		public virtual void Invalidate (){
		}
		[MonoTODO]
		public virtual void Invalidate (bool invalidateChildren){
		}
		[MonoTODO] 
		public virtual void Invalidate (Rectangle rc){
		}
		[MonoTODO] 
		public virtual void Invalidate (Region region){
		}
		[MonoTODO]
		public virtual void Invalidate (Rectangle rc, bool invalidateChildren)
		{
		}
		[MonoTODO]
		public virtual void Invalidate (Region region,bool	invalidateChildren){
		}
		[MonoTODO] 
		public virtual object Invoke (Delegate method){
			return null;
		}
		[MonoTODO]
		public virtual object Invoke (Delegate method, object[]args){
			return null;
		}
		[MonoTODO]
		protected virtual void InvokeGotFocus (Control toInvoke, EventArgs e){
		}
		[MonoTODO]
		protected virtual void InvokeLostFocus (Control toInvoke, EventArgs e){
		}
		[MonoTODO]
		protected virtual void InvokeOnClick (Control toInvoke, EventArgs e){
		}
		[MonoTODO]
		protected virtual void InvokePaint (Control c, PaintEventArgs e){
		}
		[MonoTODO]
		protected virtual void InvokePaintBackground (Control c, PaintEventArgs e){
		}
		[MonoTODO] 
		protected virtual bool IsInputChar (char charCode){
			return true;
		}

		[MonoTODO] 
		protected virtual bool IsInputKey (Keys keyData){
			return false;
		}

		public static bool IsMnemonic (char charCode, string text){
			if (text == null)
				return false;
			return text.IndexOf ("&" + charCode) > 0;
		}

		[MonoTODO]
		protected virtual void NotifyInvalidate (Rectangle invalidatedArea){
		}
		protected virtual void OnBackColorChanged (EventArgs e){
			if (BackColorChanged != null)
				BackColorChanged (this, e);
			foreach (Control ctl in Controls){
				ctl.OnParentBackColorChanged (e);
			}
		}

		protected virtual void OnBackgroundImageChanged (EventArgs e){
			if (BackgroundImageChanged != null)
				BackgroundImageChanged (this, e);
		}

		protected virtual void OnBindingContextChanged (EventArgs e){
			if (BindingContextChanged != null)
				BindingContextChanged (this, e);
		}

		protected virtual void OnCausesValidationChanged (EventArgs e){
			if (CausesValidationChanged != null)
				CausesValidationChanged (this, e);
		}

		protected virtual void OnChangeUICues (UICuesEventArgs e){
			if (ChangeUICues != null)
				ChangeUICues (this, e);
		}

		//Compact Framework
		protected virtual void OnClick (EventArgs e){
			if (Click != null)
				Click (this, e);
		}


		protected virtual void OnContextMenuChanged (EventArgs e){
			if (ContextMenuChanged != null)
				ContextMenuChanged (this, e);
		}
		protected virtual void OnControlAdded (ControlEventArgs e){
			e.Control.Visible = true;
			// If the control have another parent, remove it from the child list
			if (e.Control.parent != this){
				e.Control.Parent = null;
			}
			
			e.Control.parent = this;
			
			if (ControlAdded != null)
				ControlAdded (this, e);

			Point l = e.Control.Location;
			if (layout == null){
			      Widget w = Widget;
			}
			layout.Put (e.Control.Widget, l.X, l.Y);
			//e.Control.LocationChanged +=
			//	new EventHandler (ControlLocationChanged);
		}

		protected virtual void OnControlRemoved (ControlEventArgs e){		
			if (layout != null){
				layout.Remove (e.Control.Widget);
			}			
			if (ControlRemoved != null)
				ControlRemoved (this, e);
		}

		protected virtual void OnCreateControl (){
		}

		protected virtual void OnCursorChanged (EventArgs e){
			if (CursorChanged != null)
				CursorChanged (this, e);
		}

		protected virtual void OnDockChanged (EventArgs e){
			// changing this property does not affect the control directly
			// so have its parent to calculate new layout
			if (Parent != null)
				Parent.PerformLayout (this, "Dock");
			if (DockChanged != null)
			 	DockChanged (this, e);
		}

		protected virtual void OnDoubleClick (EventArgs e){
			if (DoubleClick != null)
				DoubleClick (this, e);
		}

		protected virtual void OnDragDrop (DragEventArgs drgevent){
			if (DragDrop != null)
				DragDrop (this, drgevent);
		}

		protected virtual void OnDragEnter (DragEventArgs drgevent){
			if (DragEnter != null)
				DragEnter (this, drgevent);
		}

		protected virtual void OnDragLeave (EventArgs e){
			if (DragLeave != null)
				DragLeave (this, e);
		}

		protected virtual void OnDragOver (DragEventArgs drgevent){
			if (DragOver != null)
				DragOver (this, drgevent);
		}

		//Compact Framework
		protected virtual void OnEnabledChanged (EventArgs e){
			if (EnabledChanged != null)
				EnabledChanged (this, e);
		}

		protected virtual void OnEnter (EventArgs e){
			if (Enter != null)
				Enter (this, e);
		}

		protected virtual void OnFontChanged (EventArgs e){
			if (FontChanged != null)
				FontChanged (this, e);
		}

		protected virtual void OnForeColorChanged (EventArgs e){
			if (ForeColorChanged != null)
				ForeColorChanged (this, e);
		}

		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs  gfbevent){
			if (GiveFeedback != null)
				GiveFeedback (this, gfbevent);
		}

		//Compact Framework
		protected virtual void OnGotFocus (EventArgs e){
			if (GotFocus != null)
				GotFocus (this, e);
		}

		protected virtual void OnHandleCreated (EventArgs e){
		}

		protected virtual void OnHandleDestroyed (EventArgs e){
		}

		protected virtual void OnHelpRequested (HelpEventArgs hevent){
			if (HelpRequested != null)
				HelpRequested (this, hevent);
		}

		protected virtual void OnImeModeChanged (EventArgs e){
			if (ImeModeChanged != null)
				ImeModeChanged (this, e);
		}

		protected virtual void OnInvalidated (InvalidateEventArgs e){
			if (Invalidated != null)
				Invalidated (this, e);
		}

		//Compact Framework
		protected virtual void OnKeyDown (KeyEventArgs e){
			if (KeyDown != null)
				KeyDown (this, e);
		}

		//Compact Framework
		protected virtual void OnKeyPress (KeyPressEventArgs e){
			if (KeyPress != null)
				KeyPress (this, e);
		}

		//Compact Framework
		protected virtual void OnKeyUp (KeyEventArgs e){
			if (KeyUp != null)
				KeyUp (this, e);

		}

		protected virtual void OnLayout (LayoutEventArgs levent){
			 if (Layout != null)
			 	Layout (this, levent);
		}

		protected virtual void OnLeave (EventArgs e){
			if (Leave != null)
				Leave (this, e);
		}

		protected virtual void OnLocationChanged (EventArgs e){
			if ((Parent != null) && (Parent.layout != null)){
				Parent.layout.Move (Widget, Location.X, Location.Y);
			}
			if (LocationChanged != null)
				LocationChanged (this, e);
		}
		//Compact Framework
		protected virtual void OnLostFocus (EventArgs e){
			if (LostFocus != null)
				LostFocus (this, e);
		}
		//Compact Framework
		protected virtual void OnMouseDown (MouseEventArgs e){
			if (MouseDown != null)
				MouseDown (this, e);
		}

		protected virtual void OnMouseEnter (EventArgs e){
			if (MouseEnter != null)
				MouseEnter (this, e);
		}

		protected virtual void OnMouseHover (EventArgs e){
			if (MouseHover != null)
				MouseHover (this, e);
		}

		protected virtual void OnMouseLeave (EventArgs e){		
			if (MouseLeave != null)
				MouseLeave (this, e);
		}

		//Compact Framework
		protected virtual void OnMouseMove (MouseEventArgs e){
			if (MouseMove != null)
				MouseMove (this, e);
		}

		//Compact Framework
		protected virtual void OnMouseUp (MouseEventArgs e){
			if (MouseUp != null)
				MouseUp (this, e);
		}

		protected virtual void OnMouseWheel (MouseEventArgs e){
			if (MouseWheel != null)
				MouseWheel (this, e);
		}

		protected virtual void OnMove (EventArgs e){
			if (Move != null)
				Move (this, e);
		}

		protected virtual void OnNotifyMessage (Message m){
			//FIXME:
		}

		//Compact Framework
		protected virtual void OnPaint (PaintEventArgs e){
			if (Paint != null)
				Paint (this, e);
		}

		//
		//Compact Framework
		protected virtual void OnPaintBackground (PaintEventArgs  pevent){
			if (GetStyle (ControlStyles.UserPaint))	{
				Brush br = new SolidBrush (BackColor);
				pevent.Graphics.FillRectangle (br,
							       pevent.
							       ClipRectangle);
				br.Dispose ();
			}
		}

		protected virtual void OnParentBackColorChanged (EventArgs e){
			BackColor = Parent.BackColor;
			// FIXME: setting BackColor fires the BackColorChanged event,
			// so we do not need to call this here
			/*
			 * if (BackColorChanged != null)
			 * BackColorChanged (this, e);
			 */
			 if (BackColorChanged != null)
			 	BackColorChanged (this, e);
		}
		protected virtual void OnParentBackgroundImageChanged (EventArgs e){
			if (BackgroundImageChanged != null)
				BackgroundImageChanged (this, e);
		}
		protected virtual void OnParentBindingContextChanged (EventArgs e){
			if (BindingContextChanged != null)
				BindingContextChanged (this, e);
		}
		//Compact Framework
		protected virtual void OnParentChanged (EventArgs e){
			if (ParentChanged != null)
				ParentChanged (this, e);
		}
		protected virtual void OnParentEnabledChanged (EventArgs e){
			if (EnabledChanged != null)
				EnabledChanged (this, e);
		}
		protected virtual void OnParentFontChanged (EventArgs e){
			if (FontChanged != null)
				FontChanged (this, e);
		}
		protected virtual void OnParentForeColorChanged (EventArgs e){
			if (ForeColorChanged != null)
				ForeColorChanged (this, e);
		}
		protected virtual void OnParentRightToLeftChanged (EventArgs e){
			if (RightToLeftChanged != null)
				RightToLeftChanged (this, e);
		}
		protected virtual void OnParentVisibleChanged (EventArgs e){
			if (VisibleChanged != null)
				VisibleChanged (this, e);
		}
		protected virtual void OnQueryContinueDrag (QueryContinueDragEventArgs qcdevent){
			if (QueryContinueDrag != null)
				QueryContinueDrag (this, qcdevent);
		}
		//Compact Framework
		protected virtual void OnResize (EventArgs e){
			if (Resize != null)
				Resize (this, e);
			PerformLayout (this, "Bounds");
		}
		protected virtual void OnRightToLeftChanged (EventArgs e){
			if (RightToLeftChanged != null)
				RightToLeftChanged (this, e);
		}
		protected virtual void OnSizeChanged (EventArgs e){
			OnResize (e);
			if (SizeChanged != null)
				SizeChanged (this, e);
		}
		protected virtual void OnStyleChanged (EventArgs e){
			if (StyleChanged != null)
				StyleChanged (this, e);
		}
		protected virtual void OnSystemColorsChanged (EventArgs e){
			if (SystemColorsChanged != null)
				SystemColorsChanged (this, e);
		}

		protected virtual void OnTabIndexChanged (EventArgs e){
			if (TabIndexChanged != null)
				TabIndexChanged (this, e);
		}
		protected virtual void OnTabStopChanged (EventArgs e){
			if (TabStopChanged != null)
				TabStopChanged (this, e);
		}
		//Compact Framework
		protected virtual void OnTextChanged (EventArgs e){
			if (TextChanged != null)
				TextChanged (this, e);
		}
		//[MonoTODO] // this doesn't seem to be documented
		//              protected virtual void OnTextAlignChanged (EventArgs e) {
		//                      TextAlignChanged (this, e);
		//              }

		protected virtual void OnValidated (EventArgs e){
			if (Validated != null)
				Validated (this, e);
		}
		[MonoTODO]
		protected virtual void OnValidating (CancelEventArgs e)	{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnVisibleChanged (EventArgs e){
			if (VisibleChanged != null)
				VisibleChanged (this, e);
			PerformLayout ();
		}
		[MonoTODO] 
		public virtual void PerformLayout (){
		}
		[MonoTODO]
		public virtual void PerformLayout (Control affectedControl,
							   string affectedProperty){
		}
		[MonoTODO] 
		public virtual Point PointToClient (Point p){
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		public virtual Point PointToScreen (Point p){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool PreProcessMessage (ref Message msg){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessCmdKey (ref Message msg, Keys keyData){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessDialogChar (char charCode){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessDialogKey (Keys keyData){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessKeyEventArgs (ref Message m){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessKeyMessage (ref Message  m){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual bool ProcessKeyPreview (ref Message m){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool ProcessMnemonic (char charCode){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void RaiseDragEvent (object key, DragEventArgs e){
		}
		[MonoTODO]
		protected virtual void RaiseKeyEvent (object key, KeyEventArgs e){
		}
		[MonoTODO]
		protected virtual void RaiseMouseEvent (object key, MouseEventArgs e){
		}
		[MonoTODO]
		protected virtual void RaisePaintEvent (object key,	PaintEventArgs e){
		}
		[MonoTODO] 
		protected virtual void RecreateHandle (){
		}
		[MonoTODO]
		public virtual Rectangle RectangleToClient (Rectangle r){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual Rectangle RectangleToScreen (Rectangle r){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected static bool ReflectMessage (IntPtr hWnd,ref Message m){
			//throw new NotImplementedException ();
			return false;
		}
		[MonoTODO] 
		public virtual void Refresh (){
		}
		[MonoTODO] 
		public virtual void ResetBackColor (){
		}
		[MonoTODO] 
		public virtual void ResetBindings (){
		}
		[MonoTODO] 
		public virtual void ResetCursor (){
		}
		[MonoTODO] 
		public virtual void ResetFont (){
		}
		[MonoTODO] 
		public virtual void ResetForeColor (){
		}
		[MonoTODO] 
		public virtual void ResetImeMode (){
		}
		[MonoTODO] 
		protected virtual void ResetMouseEventArgs (){
		}
		[MonoTODO] 
		public virtual void ResetRightToLeft (){
		}
		[MonoTODO] 
		public virtual void ResetText (){
		}
		public virtual void ResumeLayout (){
			this.ResumeLayout (true);
		}
		[MonoTODO]
		public virtual void ResumeLayout (bool performLayout){
		}
		[MonoTODO]
		protected virtual ContentAlignment
			RtlTranslateAlignment (ContentAlignment align){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual HorizontalAlignment
			RtlTranslateAlignment (HorizontalAlignment align){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual LeftRightAlignment
			RtlTranslateAlignment (LeftRightAlignment align){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual ContentAlignment
			RtlTranslateContent (ContentAlignment align){
			throw new NotImplementedException ();
		}
		[MonoTODO]
			protected virtual HorizontalAlignment
			RtlTranslateHorizontal (HorizontalAlignment align){
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual LeftRightAlignment
			RtlTranslateLeftRight (LeftRightAlignment align){
			throw new NotImplementedException ();
		}
		public virtual void Scale (float ratio){
			ScaleCore (ratio, ratio);
		}
		public virtual void Scale (float dx, float dy){
			ScaleCore (dx, dy);
		}
		[MonoTODO]
		protected virtual void ScaleCore (float dx, float dy){
		}
		[MonoTODO] 
		public virtual void Select (){
		}
		[MonoTODO]
		protected virtual void Select (bool directed, bool forward){
		}
		[MonoTODO]
		public virtual bool SelectNextControl (Control ct,
							       bool forward,
							       bool
							       tabStopOnly,
							       bool nested,
							       bool wrap)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		public virtual void SendToBack (){
		}
		[MonoTODO]
		public virtual void SetBounds (int x, int y,int width, int height){
		}
		[MonoTODO]
		public virtual void SetBounds (int x, int y,
						       int width, int height,
						       BoundsSpecified
						       specified)
		{
		}
		[MonoTODO]
		protected virtual void SetBoundsCore (int x, int y,
							      int width,
							      int height,
							      BoundsSpecified
							      specified)
		{
		}
		[MonoTODO]
		protected virtual void SetClientSizeCore (int x, int y){
		}
		[MonoTODO]
		protected virtual void SetStyle (ControlStyles flag, bool value){
		}
		[MonoTODO] 
		protected virtual void SetTopLevel (bool value){
		}
		[MonoTODO] 
		protected virtual void SetVisibleCore (bool value){
		}
		public void Show (){
			Widget.Show ();
		}
		[MonoTODO] 
		public virtual void SuspendLayout (){
		}
		[MonoTODO] 
		public virtual void Update (){
		}
		protected void UpdateBounds (){
		}
		protected virtual void UpdateBounds (int x, int y, int width, int height){
		}
		protected virtual void UpdateBounds (int x, int y, int width,
						     int height,
						     int clientWidth,
						     int clientHeight)
		{

		}
		protected virtual void UpdateStyles (){
		}
		protected virtual void UpdateZOrder (){
		}
		protected virtual void WndProc (ref Message m){
		}


		// Events

		public event EventHandler BackColorChanged;
		public event EventHandler BackgroundImageChanged;
		public event EventHandler BindingContextChanged;
		public event EventHandler CausesValidationChanged;
		public event UICuesEventHandler ChangeUICues;

		//Compact Framework
		public event EventHandler Click;

		public event EventHandler ContextMenuChanged;
		public event ControlEventHandler ControlAdded;
		public event ControlEventHandler ControlRemoved;
		public event EventHandler CursorChanged;
		public event EventHandler DockChanged;
		public event EventHandler DoubleClick;
		public event DragEventHandler DragDrop;
		public event DragEventHandler DragEnter;
		public event EventHandler DragLeave;
		public event DragEventHandler DragOver;

		//Compact Framework
		public event EventHandler EnabledChanged;

		public event EventHandler Enter;
		public event EventHandler FontChanged;
		public event EventHandler ForeColorChanged;
		public event GiveFeedbackEventHandler GiveFeedback;

		//Compact Framework
		public event EventHandler GotFocus;

		public event EventHandler HandleCreated;
		public event EventHandler HandleDestroyed;
		public event HelpEventHandler HelpRequested;
		public event EventHandler ImeModeChanged;
		public event InvalidateEventHandler Invalidated;

		//Compact Framework
		public event KeyEventHandler KeyDown;

		//Compact Framework
		public event KeyPressEventHandler KeyPress;

		//Compact Framework
		public event KeyEventHandler KeyUp;

		public event LayoutEventHandler Layout;
		public event EventHandler Leave;
		public event EventHandler LocationChanged;

		//Compact Framework
		public event EventHandler LostFocus;

		//Compact Framework
		public event MouseEventHandler MouseDown;

		public event EventHandler MouseEnter;
		public event EventHandler MouseHover;
		public event EventHandler MouseLeave;

		//Compact Framework
		public event MouseEventHandler MouseMove;

		//Compact Framework
		public event MouseEventHandler MouseUp;

		public event MouseEventHandler MouseWheel;
		public event EventHandler Move;

		//Compact Framework
		public event PaintEventHandler Paint;

		//Compact Framework
		public event EventHandler ParentChanged;

		public event QueryAccessibilityHelpEventHandler
			QueryAccessibilityHelp;
		public event QueryContinueDragEventHandler QueryContinueDrag;

		//Compact Framework
		public event EventHandler Resize;

		public event EventHandler RightToLeftChanged;
		public event EventHandler SizeChanged;
		public event EventHandler StyleChanged;
		public event EventHandler SystemColorsChanged;
		public event EventHandler TabIndexChanged;
		public event EventHandler TabStopChanged;

		//Compact Framework
		public event EventHandler TextChanged;

		public event EventHandler Validated;

		public event CancelEventHandler Validating;

		public event EventHandler VisibleChanged;





		internal Widget Widget{
			get{
				if (widget == null){
					widget = CreateWidget ();
					ConnectEvents();
				}				
				return widget;
			}
		}
		internal virtual Widget CreateWidget (){
			vbox = new Gtk.VBox (false, 0);
			layout = new Gtk.Layout (new Gtk.Adjustment (0, 0, 1, .1, .1,.1),
						 new Gtk.Adjustment (0, 0, 1,.1, .1, .1));
			vbox.PackStart (layout, true, true, 0);
			vbox.ShowAll ();
			return vbox;
		}
		
		internal virtual void ConnectEvents(){
			Widget w = Widget;
			
			w.EnterNotifyEvent += new GtkSharp.EnterNotifyEventHandler(this.GtkMouseEnter);
			w.LeaveNotifyEvent += new GtkSharp.LeaveNotifyEventHandler(this.GtkMouseLeave);
			w.ButtonPressEvent += new GtkSharp.ButtonPressEventHandler(this.GtkButtonPress);
			w.ButtonReleaseEvent += new GtkSharp.ButtonReleaseEventHandler(this.GtkButtonRelease);
			
			this.Resize += new EventHandler (this.GtkResize);
		}
		internal void GtkResize (object o, EventArgs args){
			Widget.SetSizeRequest (Size.Width, Size.Height);
		}
		[MonoTODO]
		internal void GtkButtonPress(object o, ButtonPressEventArgs args){			
			OnMouseDown (SWFGtkConv.MouseUpDownArgs(args.Event));
		}
		internal void GtkButtonRelease (object o, ButtonReleaseEventArgs args){
			OnMouseUp (SWFGtkConv.MouseUpDownArgs(args.Event));
		}
		internal void GtkMouseEnter (object o, EnterNotifyEventArgs args){
			OnMouseEnter ((EventArgs) args);
		}
		internal void GtkMouseLeave (object o, LeaveNotifyEventArgs args){
			OnMouseLeave ((EventArgs) args);
		}

		public class ControlCollection:IList, ICollection,IEnumerable, ICloneable{
			ArrayList list = new ArrayList ();
			protected Control owner;

			public ControlCollection (Control owner){
				this.owner = owner;
			}

			private ControlCollection (){
			}

			// ControlCollection
			//
			// TODO Revisar como implementar esto.
			// 
			public virtual void Add (Control value){
				list.Add (value);
				owner.OnControlAdded (new ControlEventArgs (value));
				//list.Add (value);
				//owner.OnControlAdded (new ControlEventArgs (value));


				//if (this.owner.GetType () ==
				//    typeof (System.Windows.Forms.Button))
				//{	
					// This makes Gtk throw a warning about a label already being added
					// This is actually support that Gtk just does not have :(
					// Should we reinvent the Button-object for that? (Inherit from Gtk.Container
					// and implement our own OnClicked handlers and stuff like that)

					// Or .. we remove the label and replace it with a Container which
					// we fill with the added Controls .. (question: how do I remove the
					// label and/or get the Gtk.Widget inside the button ?)
					//
					// Phillip : I added in the code, but you will have to do do the
					// positioning stuff. You understand that better than me :-)
					// The Controls property has to be overriden in the base class
					// (ie Button.cs) now
					// if you want to be able to add controls to a control
					//
					// We will also have to to create a way to add widgets to a
					// GroupBox 

					//Gtk.Button gtkbutton =(Gtk.Button) this.owner.widget;
					// ?
					//GLib.List mylist =new GLib.List ((IntPtr) 0,
					//		       typeof (Gtk.Widget));
					//mylist = gtkbutton.Children;
					//foreach (Gtk.Widget awidget in mylist)
					//{
					//	gtkbutton.Remove (awidget);
					//}
					//gtkbutton.Add (value.widget);
					//gtkbutton.ShowAll ();
					//list.Add (value);
				//}
				//else if (value.GetType () == typeof (System.Windows.Forms.StatusBar))
				//{
					// SWF on Windows adds above the last added StatusBar
					// I think that this adds below the last one ..
					// So a reorderchild operation might be required here..
					//this.owner.vbox.PackEnd (value.widget, false, false, 0);
					// this.vbox.ReorderChild (value.widget, 0);
					//this.owner.vbox.ShowAll ();
					//list.Add (value);
				//}
				
				// TODO System.Windows.Forms.ToolBar
				// But we don't have this type yet :-)
				//else
				//{
				//	list.Add (value);
				//	owner.OnControlAdded (new ControlEventArgs (value));
				//}
			}
			public virtual void AddRange (Control[]controls){
				// Because we really do have to check for a few
				// special cases we cannot use the AddRange and
				// will have to check each Control that we add

				// list.AddRange (controls);
				foreach (Control c in controls){
					this.Add (c);
				}
				// owner.OnControlAdded (new ControlEventArgs (c));
			}

			public bool Contains (Control value){
				return list.Contains (value);
			}
			public virtual void Remove (Control value){
				list.Remove (value);
				owner.OnControlRemoved (new ControlEventArgs (value));
			}
			public virtual Control this[int index]{
				get{return (Control) list[index];}
			}
			public int GetChildIndex (Control child){
				return GetChildIndex (child, true);
			}
			public int GetChildIndex (Control child, bool throwException){
				if (throwException && !Contains (child))
					throw new Exception ();
				return list.IndexOf (child);
			}
			public int IndexOf (Control value){
				return list.IndexOf (value);
			}
			public void SetChildIndex (Control child, int newIndex){
				int oldIndex = GetChildIndex (child);
				if (oldIndex == newIndex)
					return;
				// is this correct behavior?
				Control other = (Control) list[newIndex];
				list[oldIndex] = other;
				list[newIndex] = child;
			}

			// IList
			public bool IsFixedSize{
				get{return list.IsFixedSize;}
			}
			public bool IsReadOnly{
				get	{return list.IsReadOnly;}
			}
			int IList.Add (object value){
				return list.Add (value);
			}
			public void Clear (){
				list.Clear ();
			}
			bool IList.Contains (object value){
				return list.Contains (value);
			}
			int IList.IndexOf (object value){
				return list.IndexOf (value);
			}
			void IList.Insert (int index, object value){
				list.Insert (index, value);
			}
			void IList.Remove (object value){
				list.Remove (value);
			}
			public void RemoveAt (int index){
				list.RemoveAt (index);
			}

			// ICollection
			public int Count{
				get	{return list.Count;}
			}
			public bool IsSynchronized{
				get{return list.IsSynchronized;	}
			}
			public object SyncRoot{
				get{return list.SyncRoot;}
			}
			public void CopyTo (Array array, int index){
				list.CopyTo (array, index);
			}

			// IEnumerable
			public IEnumerator GetEnumerator (){
				return list.GetEnumerator ();
			}

			// ICloneable
			public object Clone (){
				ControlCollection c =
					new ControlCollection ();
				c.list = (ArrayList) list.Clone ();
				c.owner = owner;
				return c;
			}

			object IList.this[int index]{
				get{return list[index];	}
				set{list[index] = value;}
			}
		}
	}
}
