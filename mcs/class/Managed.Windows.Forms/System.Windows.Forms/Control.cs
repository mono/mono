// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok		pbartok@novell.com
//
// Based on work by:
//	Aleksey Ryabchuk	ryabchuk@yahoo.com
//	Alexandre Pigolkine	pigolkine@gmx.de
//	Dennis Hayes		dennish@raytek.com
//	Jaak Simm		jaaksimm@firm.ee
//	John Sohn		jsohn@columbus.rr.com
//
// $Revision: 1.18 $
// $Modtime: $
// $Log: Control.cs,v $
// Revision 1.18  2004/08/11 13:47:22  pbartok
// - Rewrote the collection stuff. Might not be as fast now, not keeping
//   the number of children around and accessible directly, but it's more
//   straightforward
//
// Revision 1.17  2004/08/10 18:32:10  jordi
// throw ontextchange event
//
// Revision 1.16  2004/08/10 17:43:04  pbartok
// - Added more to the still unfinished Dock/Anchor layout code
//
// Revision 1.15  2004/08/10 15:08:05  jackson
// Control will now handle the buffering code, so each control does not have to implement this.
//
// Revision 1.14  2004/08/09 22:11:25  pbartok
// - Added incomplete dock layout code
// - Added support for mouse wheel
//
// Revision 1.13  2004/08/09 17:25:56  jackson
// Use new color names
//
// Revision 1.12  2004/08/09 15:54:51  jackson
// Get default properties from the theme.
//
// Revision 1.11  2004/08/06 21:30:56  pbartok
// - Fixed recursive loop when resizing
// - Improved/fixed redrawing on expose messages
//
// Revision 1.10  2004/08/06 15:53:39  jordi
// X11 keyboard navigation
//
// Revision 1.9  2004/08/04 21:14:26  pbartok
// - Fixed Invalidation bug (calculated wrong client area)
// - Added ClientSize setter
//
// Revision 1.8  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.7  2004/07/27 10:38:17  jordi
// changes to be able to run winforms samples
//
// Revision 1.6  2004/07/19 19:09:42  jordi
// label control re-written: added missing functionlity, events, and properties
//
// Revision 1.5  2004/07/19 16:49:23  jordi
// fixes SetBounds logic
//
// Revision 1.4  2004/07/19 07:29:35  jordi
// Call RefreshWindow only if the window has created
//
// Revision 1.3  2004/07/15 17:03:35  jordi
// added basic mouse handeling events
//
// Revision 1.2  2004/07/13 15:31:45  jordi
// commit: new properties and fixes form size problems
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE 

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	public class Control : Component, ISynchronizeInvoke, IWin32Window
        {
		#region Local Variables
		// Basic
		internal Rectangle		bounds;			// bounding rectangle for control
		internal object			creator_thread;		// thread that created the control
		internal ControlNativeWindow	window;			// object for native window handle
		internal string			name;			// for object naming

		// State
		internal bool			has_focus;		// true if control has focus
		internal bool			is_visible;		// true if control is visible
		internal bool			is_enabled;		// true if control is enabled (usable/not grayed out)
		internal int			tab_index;		// position in tab order of siblings
		internal bool			is_disposed;		// has the window already been disposed?
		internal Size			window_size;		// size of the window (including decorations)
		internal Size			client_size;		// size of the client area (window excluding decorations)
		internal ControlStyles		control_style;		// win32-specific, style bits for control

		// Visuals
		internal Color			foreground_color;	// foreground color for control
		internal Color			background_color;	// background color for control
		internal Image			background_image;	// background image for control
		internal Font			font;			// font for control
		internal string			text;			// window/title text for control

		// Layout
		internal AnchorStyles		anchor_style;		// anchoring requirements for our control
		internal DockStyle		dock_style;		// docking requirements for our control (supercedes anchoring)
		internal int			prev_width;		// previous width of the control; required for anchoring
		internal int			prev_height;		// previous height of our control; required for anchoring
		internal Size prev_size;

		// to be categorized...
		static internal Hashtable	controls;		// All of the applications controls, in a flat list
		internal ControlCollection	child_controls;		// our children
		internal Control		parent;			// our parent control
		internal AccessibleObject	accessibility_object;	// object that contains accessibility information about our control
		internal BindingContext		binding_context;	// TODO
		internal RightToLeft		right_to_left;		// drawing direction for control
		internal int			layout_suspended;

		private Graphics dc_mem;
		private Bitmap bmp_mem;

		#endregion	// Local Variables

		#region Private Classes
		// This helper class allows us to dispatch messages to Control.WndProc
		internal class ControlNativeWindow : NativeWindow {
			private Control control;

			public ControlNativeWindow(Control control) : base() {
				this.control=control;
			}

			protected override void WndProc(ref Message m) {
				control.WndProc(ref m);
			}
		}
		#endregion
		
		#region Public Classes
		public class ControlCollection : IList, ICollection, ICloneable, IEnumerable {
			#region	ControlCollection Local Variables
			private ArrayList	list;
			private Control		owner;
			#endregion	// ControlCollection Local Variables

			#region ControlCollection Public Constructor
			public ControlCollection(Control owner) {
				this.owner=owner;
				this.list=new ArrayList();
			}
			#endregion

			#region	ControlCollection Public Instance Properties
			public int Count {
				get {
					return list.Count;
				}
			}

			public bool IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			public virtual Control this[int index] {
				get {
					if (index < 0 || index >= list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
					}
					return (Control)list[index];
				}
			}
			#endregion // ControlCollection Public Instance Properties
			
			#region	ControlCollection Private Instance Methods
			public virtual void Add(Control value) {
				for (int i=0; i<list.Count; i++) {
					if (list[i]==value) {
						// Do we need to do anything here?
						return;
					}
				}
				list.Add(value);
			}
			
			public virtual void AddRange(Control[] controls) {
				for (int i=0; i<controls.Length; i++) {
					Add(controls[i]);
				}
			}

			public virtual void Clear() {
				owner.SuspendLayout();
				list.Clear();
				owner.ResumeLayout();
			}

			public virtual bool Contains(Control value) {
				return list.Contains(value);
			}

			public void CopyTo(Array array, int index) {
				list.CopyTo(array, index);
			}

			public override bool Equals(object other) {
				if (other is ControlCollection && (((ControlCollection)other).owner==this.owner)) {
					return(true);
				} else {
					return(false);
				}
			}

			public int GetChildIndex(Control child) {
				return GetChildIndex(child, false);
			}

			public int GetChildIndex(Control child, bool throwException) {
				int index;

				index=list.IndexOf(child);

				if (index==-1 && throwException) {
					throw new ArgumentException("Not a child control", "child");
				}
				return index;
			}

			public IEnumerator GetEnumerator() {
				return list.GetEnumerator();
			}

			public override int GetHashCode() {
				return base.GetHashCode();
			}

			public int IndexOf(Control control) {
				return list.IndexOf(control);
			}

			public virtual void Remove(Control value) {
				list.Remove(value);
			}

			public void RemoveAt(int index) {
				if (index<0 || index>=list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
				}

				list.RemoveAt(index);
			}

			public void SetChildIndex(Control child, int newIndex) {
				int	old_index;

				old_index=list.IndexOf(child);
				if (old_index==-1) {
					throw new ArgumentException("Not a child control", "child");
				}

				if (old_index==newIndex) {
					return;
				}

				RemoveAt(old_index);

				if (newIndex>list.Count) {
					list.Add(child);
				} else {
					list.Insert(newIndex, child);
				}
			}
			#endregion // ControlCollection Private Instance Methods

			#region	ControlCollection Interface Properties
			object IList.this[int index] {
				get {
					if (index<0 || index>=list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
					}
					return this[index];
				}

				set {
					if (!(value is Control)) {
						throw new ArgumentException("Object of type Control required", "value");
					}

					list[index]=(Control)value;
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
				}
			}

			bool IList.IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return list.IsSynchronized;
				}
			}

			object ICollection.SyncRoot {
				get {
					return list.SyncRoot;
				}
			}
			#endregion // ControlCollection Interface Properties

			#region	ControlCollection Interface Methods
			int IList.Add(object value) {
				if (value == null) {
					throw new ArgumentNullException("value", "Cannot add null controls");
				}

				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}

				return list.Add(value);
			}

			bool IList.Contains(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}

				return this.Contains((Control) value);
			}

			int IList.IndexOf(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control  required", "value");
				}

				return this.IndexOf((Control) value);
			}

			void IList.Insert(int index, object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				list.Insert(index, value);
			}

			void IList.Remove(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				list.Remove(value);
			}

			void ICollection.CopyTo(Array array, int index) {
				if (list.Count>0) {
					list.CopyTo(array, index);
				}
			}

			Object ICloneable.Clone() {
				ControlCollection clone = new ControlCollection(this.owner);
				clone.list=(ArrayList)list.Clone();		// FIXME: Do we need this?
				return clone;
			}
			#endregion // ControlCollection Interface Methods
		}
		#endregion	// ControlCollection Class
		
		#region Public Constructors
		public Control() {
			creator_thread = Thread.CurrentThread;
			controls = new Hashtable();
			child_controls = CreateControlsInstance();
			bounds = new Rectangle(0, 0, DefaultSize.Width, DefaultSize.Height);
			window_size = new Size(DefaultSize.Width, DefaultSize.Height);
			client_size = new Size(DefaultSize.Width, DefaultSize.Height);
			prev_size = client_size;

			is_visible = true;
			is_disposed = false;
			is_enabled = true;
			has_focus = false;
			layout_suspended = 0;

			parent = null;
			background_image = null;
		}

		public Control(Control parent, string text) : this() {
			Text=text;
			Parent=parent;
		}

		public Control(Control parent, string text, int left, int top, int width, int height) : this() {
			Parent=parent;
			Left=left;
			Top=top;
			Width=width;
			Height=height;
			Text=text;
		}

		public Control(string text) : this() {
			Text=text;
		}

		public Control(string text, int left, int top, int width, int height) : this() {
			Left=left;
			Top=top;
			Width=width;
			Height=height;
			Text=text;
		}
		#endregion 	// Public Constructors

		#region Public Static Properties
		public static Color DefaultBackColor {
			get {
				return ThemeEngine.Current.DefaultControlBackColor;
			}
		}

		public static Font DefaultFont {
			get {
				return ThemeEngine.Current.DefaultFont;
			}
		}

		public static Color DefaultForeColor {
			get {
				return ThemeEngine.Current.DefaultControlForeColor;
			}
		}

		public static Keys ModifierKeys {
			get {
				return XplatUI.State.ModifierKeys;
			}
		}

		public static MouseButtons MouseButtons {
			get {
				return XplatUI.State.MouseButtons;
			}
		}

		public static Point MousePosition {
			get {
				return XplatUI.State.MousePosition;
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		public AccessibleObject AccessibilityObject {
			get {
				if (accessibility_object==null) {
					accessibility_object=CreateAccessibilityInstance();
				}
				return accessibility_object;
			}
		}

		public string AccessibleDefaultActionDescription {
			get {
				return AccessibilityObject.default_action;
			}

			set {
				AccessibilityObject.default_action=value;
			}
		}

		public string AccessibleDescription {
			get {
				return AccessibilityObject.description;
			}

			set {
				AccessibilityObject.description=value;
			}
		}

		public string AccessibleName {
			get {
				return AccessibilityObject.Name;
			}

			set {
				AccessibilityObject.Name=value;
			}
		}

		public AccessibleRole AccessibleRole {
			get {
				return AccessibilityObject.role;
			}

			set {
				AccessibilityObject.role=value;
			}
		}

		public virtual bool AllowDrop {
			get {
				return XplatUI.State.DropTarget;
			}

			set {
				XplatUI.State.DropTarget=value;
			}
		}

		public virtual AnchorStyles Anchor {
			get {
				return anchor_style;
			}

			set {
				anchor_style=value;
			}
		}

		public virtual Color BackColor {
			get {
				if (background_color.IsEmpty) {
					if (parent!=null) {
						return parent.BackColor;
					}
					return DefaultBackColor;
				}
				return background_color;
			}

			set {
				background_color=value;
				Refresh();
			}
		}

		public virtual Image BackgroundImage {
			get {
				return background_image;
			}

			set {
				if (background_image!=value) {
					background_image=value;
					OnBackgroundImageChanged(EventArgs.Empty);
				}
			}
		}

		public virtual BindingContext BindingContext {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}


		public int Bottom {
			get {
				return bounds.Y+bounds.Height;
			}
		}

		public Rectangle Bounds {
			get {
				return this.bounds;
			}

			set {
				SetBounds(value.Left, value.Top, value.Width, value.Height, BoundsSpecified.All);
			}
		}

		public bool CanFocus {
			get {
				throw new NotImplementedException();
			}
		}

		public bool CanSelect {
			get {
				throw new NotImplementedException();
			}
		}

		public bool Capture {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public bool CausesValidation {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public Rectangle ClientRectangle {
			get {
				return new Rectangle(0, 0, client_size.Width, client_size.Height);
			}
		}

		public Size ClientSize {
			get {
				return client_size;
			}

			set {
				this.SetClientSizeCore(value.Width, value.Height);
			}
		}

		public String CompanyName {
			get {
				return "Mono Project, Novell, Inc.";
			}
		}

		public bool ContainsFocus {
			get {
				throw new NotImplementedException();
			}
		}
#if notdef
		public virtual ContextMenu ContextMenu {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}
#endif

		public ControlCollection Controls {
			get {
				return CreateControlsInstance();
			}
		}

		public bool Created {
			get {
				throw new NotImplementedException();
			}
		}

#if notdef
		public virtual Cursor Cursor {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public ControlBidingsCollection DataBindings {
			get {
				throw new NotImplementedException();
			}
		}
#endif
		public virtual Rectangle DisplayRectangle {
			get {
				return ClientRectangle;
			}
		}

		public bool Disposing {
			get {
				throw new NotImplementedException();
			}
		}

		public virtual DockStyle Dock {
			get { return dock_style;}

			set {
				if (dock_style == value)
					return;

				dock_style = value;
				OnDockChanged(EventArgs.Empty);
			}
		}

		public bool Enabled {
			get {
				return is_enabled;
			}

			set {
				is_enabled = value;
			}
		}

		public virtual bool Focused {
			get {
				return this.has_focus;
			}

			set {
				throw new NotImplementedException();
			}
		}

		public virtual Font Font {
			get {
				if (font != null) {
					return font;
				}

				if (Parent != null && Parent.Font != null) {
					return Parent.Font;
				}

				return DefaultFont;
			}

			set {
				font=value;
				Refresh();
			}
		}

		public virtual Color ForeColor {
			get {
				if (foreground_color.IsEmpty) {
					if (parent!=null) {
						return parent.ForeColor;
					}
					return DefaultForeColor;
				}
				return foreground_color;
			}

			set {
				foreground_color=value;
				Refresh();
			}
		}

		public bool IsDisposed {
			get {
				return this.is_disposed;
			}
		}

		public bool IsHandleCreated {
			get {
				if ((window!=null) && (window.Handle!=IntPtr.Zero)) {
					return true;
				}

				return false;
			}
		}

		public string Name {
			get {
				return this.name;
			}

			set {
				this.name=value;
			}
		}

		public Control Parent {
			get {
				return this.parent;
			}

			set {
				if (parent!=value) {
					if (parent!=null) {
						parent.Controls.Remove(this);
					}

					parent=value;

					if (!parent.Controls.Contains(this)) {
						parent.Controls.Add(this);
					}

					XplatUI.SetParent(Handle, value.Handle);
				}
			}
		}


		public IntPtr Handle {							// IWin32Window
			get 
			{
				if (!IsHandleCreated) {
					CreateHandle();
				}
				return window.Handle;
			}
		}

		public bool InvokeRequired {						// ISynchronizeInvoke
			get {
				if (creator_thread!=Thread.CurrentThread) {
					return true;
				}
				return false;
			}
		}

		public bool Visible {
			get {
				return this.is_visible;
			}

			set {
				if (value!=is_visible) {
					is_visible=value;
					XplatUI.SetVisible(Handle, value);
				}
			}
		}

		public virtual string Text {
			get {
				return this.text;
			}

			set {
				if (text!=value) {
					text=value;
					XplatUI.Text(Handle, text);
					OnTextChanged (EventArgs.Empty);
				}
			}
		}

		public int Left {
			get {
				return this.bounds.X;
			}

			set {
				SetBounds(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
			}
		}

		public int Top {
			get {
				return this.bounds.Y;
			}

			set {
				SetBounds(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
			}
		}

		public int Width {
			get {
				return this.bounds.Width;
			}

			set {
				SetBounds(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
			}
		}

		public int Height {
			get {
				return this.bounds.Height;
			}

			set {
				SetBounds(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
			}
		}

		public Point Location {
			get {
				return new Point(bounds.X, bounds.Y);
			}

			set {
				SetBounds(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
			}
		}

		public Size Size {
			get {
				return new Size(Width, Height);
			}

			set {
				SetBounds(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
		}
		
		public int TabIndex {
			get {
				return tab_index;
			}

			set {
				tab_index = value;
			}
		}

		#endregion	// Public Instance Properties

		internal Graphics DeviceContext {
			get { return dc_mem; }
		}

		internal Bitmap ImageBuffer {
			get { return bmp_mem; }
		}

		internal void CreateBuffers (int width, int height)
		{
			if (dc_mem != null)
				dc_mem.Dispose ();
			if (bmp_mem != null)
				bmp_mem.Dispose ();

			bmp_mem = new Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
		}

		#region	Protected Instance Properties
		protected virtual CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				create_params.Caption = Text;
				create_params.X = Left;
				create_params.Y = Top;
				create_params.Width = Width;
				create_params.Height = Height;

				create_params.ClassName = XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle = 0;
				create_params.Param = 0;

				if (parent!=null) {
					create_params.Parent = parent.Handle;
				}

				create_params.Style = (int)WindowStyles.WS_OVERLAPPED;

				if (is_visible) {
					create_params.Style |= (int)WindowStyles.WS_VISIBLE;
				}

				return create_params;
			}
		}

		protected virtual ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

		protected virtual Size DefaultSize {
			get {
				return new Size(100, 23);
			}
		}

		#endregion	// Protected Instance Properties

		#region	Public Instance Methods
		public bool Contains(Control ctl) {
			Control current;

			current=ctl;
			while (current!=null) {
				if (current==ctl) {
					return true;
				}
				current=current.parent;
			}
			return false;
		}

		public void CreateControl() {
			Control	child;

			CreateHandle();

			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].CreateControl();
			}
			OnCreateControl();
		}

		public virtual void Refresh() {			
			if (IsHandleCreated == true)
				XplatUI.RefreshWindow(window.Handle);
		}

		public void SetBounds(int x, int y, int width, int height) {
			SetBounds(x, y, width, height, BoundsSpecified.All);
		}

		public void SetBounds(int x, int y, int width, int height, BoundsSpecified bounds_specified) {
			if ((bounds_specified & BoundsSpecified.X) != BoundsSpecified.X) {
				x = Left;
			}

			if ((bounds_specified & BoundsSpecified.Y) != BoundsSpecified.Y) {
				y = Top;
			}

			if ((bounds_specified & BoundsSpecified.Width)!= BoundsSpecified.Width) {
				width = Width;
			}

			if ((bounds_specified & BoundsSpecified.Height) != BoundsSpecified.Height) {
				height = Height;
			}

			if (IsHandleCreated) {
				XplatUI.MoveWindow(Handle, x, y, width, height);
			}
			UpdateBounds(x, y, width, height);
		}

		public void Show() {
			if (!IsHandleCreated) {
				this.CreateHandle();
			}

			this.Visible=true;			
		}

		public object Invoke(Delegate method) {					// ISynchronizeInvoke
			return Invoke(method, null);
		}

		public object Invoke(Delegate method, object[] args) {			// ISynchronizeInvoke
			IAsyncResult	result;

			result=BeginInvoke(method, args);
			return EndInvoke(result);
		}

		public IAsyncResult BeginInvoke(Delegate method) {			// ISynchronizeInvoke
			return BeginInvoke(method, null);
		}

		protected virtual AccessibleObject CreateAccessibilityInstance() {
			return new AccessibleObject(this);
		}

		protected virtual ControlCollection CreateControlsInstance() {
			return new ControlCollection(this);
		}

		protected void UpdateBounds() {
			int	x;
			int	y;
			int	width;
			int	height;

			XplatUI.GetWindowPos(this.Handle, out x, out y, out width, out height);
			UpdateBounds(x, y, width, height);
		}

		protected void UpdateBounds(int x, int y, int width, int height) {
			bool	moved	= false;
			bool	resized	= false;

			// Generate required notifications
			if ((this.bounds.X!=x) || (this.bounds.Y!=y)) {
				moved=true;
			}

			if ((this.Bounds.Width!=width) || (this.Bounds.Height!=height)) {
				resized=true;
			}

			bounds.X=x;
			bounds.Y=y;
			bounds.Width=width;
			bounds.Height=height;

#if notdef
			if (IsHandleCreated) {
				XplatUI.SetWindowPos(Handle, bounds);
			}
#endif

			if (moved) {
				OnLocationChanged(EventArgs.Empty);
			}

			if (resized) {
				OnSizeChanged(EventArgs.Empty);
			}
		}

		protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight) {
			UpdateBounds(x, y, width, height);
		}

		public void Invalidate() {
			Invalidate(new Rectangle(0, 0, bounds.Width, bounds.Height), false);
		}

		public void Invalidate(bool invalidateChildren) {
			Invalidate(new Rectangle(0, 0, bounds.Width, bounds.Height), invalidateChildren);
		}

		public void Invalidate(System.Drawing.Rectangle rc) {
			Invalidate(rc, false);
		}

		public void Invalidate(System.Drawing.Rectangle rc, bool invalidateChildren) {
			if (!IsHandleCreated || !is_visible) {
				return;
			}

			XplatUI.Invalidate(Handle, rc, false);

			if (invalidateChildren) {
				for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			}
		}

		public void Invalidate(System.Drawing.Region region) {
			Invalidate(region, false);
		}

		[MonoTODO]
		public void Invalidate(System.Drawing.Region region, bool invalidateChildren) {
			throw new NotImplementedException();

			// FIXME - should use the GetRegionScans function of the region to invalidate each area
			if (invalidateChildren) {
				for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			}
		}

		[MonoTODO("BeginInvoke() : Figure out a cross-platform way to handle this")]
		public IAsyncResult BeginInvoke(Delegate method, object[] args) {	// ISynchronizeInvoke
			IAsyncResult	result = null;

			return result;
		}

		[MonoTODO]
		public object EndInvoke(IAsyncResult async_result) {			// ISynchronizeInvoke
			object result = null;

			return result;
		}

		public void PerformLayout() {
			PerformLayout(null, null);
		}

		public void PerformLayout(Control affectedControl, string affectedProperty) {
			LayoutEventArgs levent = new LayoutEventArgs(affectedControl, affectedProperty);

			if (layout_suspended>0) {
				return;
			}

			// Prevent us from getting messed up
			layout_suspended++;

			// Perform all Dock and Anchor calculations
			try {
				Control		child;
				AnchorStyles	anchor;
				Rectangle	space;

				space=this.DisplayRectangle;

				// Deal with docking
				for (int i=0; i < child_controls.Count; i++) {
					child=child_controls[i];
					switch (child.Dock) {
						case DockStyle.None: {
							// Do nothing
							break;
						}

						case DockStyle.Left: {
							child.SetBounds(space.Left, space.Y, child.Width, space.Height);
							space.X+=child.Width;
							space.Width-=child.Width;
							break;
						}

						case DockStyle.Top: {
							child.SetBounds(space.Left, space.Y, space.Width, child.Height);
							space.Y+=child.Height;
							space.Height-=child.Height;
							break;
						}
				
						case DockStyle.Right: {
							child.SetBounds(space.Right-child.Width, space.Y, child.Width, space.Height);
							space.Width-=child.Width;
							break;
						}

						case DockStyle.Bottom: {
							child.SetBounds(space.Left, space.Bottom-child.Height, space.Width, child.Height);
							space.Height-=child.Height;
							break;
						}

						case DockStyle.Fill: {
							child.SetBounds(space.Left, space.Top, space.Width, space.Height);
							space.Width=0;
							space.Height=0;
							break;
						}
					}
				}

				space=this.DisplayRectangle;

				// Deal with anchoring
				for (int i=0; i < child_controls.Count; i++) {
					int left;
					int top;
					int width;
					int height;
					int diff_width;
					int diff_height;

					child=child_controls[i];
					anchor=child.Anchor;

					left=child.Left-space.Left;
					top=child.Top-space.Top;
					width=prev_size.Width-child.Width-child.Left;
					height=prev_size.Height-child.Height-child.Top;

					diff_width=space.Width-prev_size.Width;
					diff_height=space.Height-prev_size.Height;

					prev_size.Width=space.Width;
					prev_size.Height=space.Height;

					// If the control is docked we don't need to do anything
					if (child.Dock != DockStyle.None) {
						continue;
					}

					if ((anchor & AnchorStyles.Left) !=0 ) {
						if ((anchor & AnchorStyles.Right) != 0) {
							// Anchoring to left and right
							width=width+diff_width;
						} else {
							; // nothing to do
						}
					} else if ((anchor & AnchorStyles.Right) != 0) {
						left+=diff_width;
					} else {
						left+=diff_width/2;
					}

					if ((anchor & AnchorStyles.Top) !=0 ) {
						if ((anchor & AnchorStyles.Bottom) != 0) {
							height+=diff_height;
						} else {
							; // nothing to do
						}
					} else if ((anchor & AnchorStyles.Bottom) != 0) {
						top+=diff_height;
					} else {
						top+=diff_height/2;
					}

					// Sanity
					if (width < 0) {
						width=0;
					}

					if (height < 0) {
						height=0;
					}

					child.SetBounds(left, top, width, height);
				}

				// Let everyone know
				OnLayout(levent);
			}

			// Need to make sure we decremend layout_suspended
			finally {
				layout_suspended--;
			}
		}

		public void ResumeLayout() 
		{
			ResumeLayout (true);
		}

		public void ResumeLayout(bool peformLayout) 
		{
			layout_suspended--;
			
			if (layout_suspended > 0 || peformLayout == false)
				return;

			PerformLayout();
		}

		
		public void SuspendLayout() 
		{
			layout_suspended++;
		}

		[MonoTODO]
		protected virtual void WndProc(ref Message m) {
			EventArgs	e = new EventArgs();

#if debug
			Console.WriteLine("Received message {0}", m);
#endif

			switch((Msg)m.Msg) {
#if notyet
				// Mouse handling
				case Msg.WM_LBUTTONDBLCLK:	throw new NotImplementedException();	break;

				case Msg.WM_RBUTTONDOWN:	throw new NotImplementedException();	break;
				case Msg.WM_RBUTTONUP:		throw new NotImplementedException();	break;
				case Msg.WM_RBUTTONDBLCLK:	throw new NotImplementedException();	break;

				case Msg.WM_MOUSEHOVER:		throw new NotImplementedException();	break;
				case Msg.WM_MOUSELEAVE:		throw new NotImplementedException();	break;
				

				// Keyboard handling
				case Msg.WM_CHAR:		throw new NotImplementedException();	break;
				case Msg.WM_KEYDOWN:		throw new NotImplementedException();	break;
				case Msg.WM_KEYUP:		throw new NotImplementedException();	break;
#endif
				// Window management
				case Msg.WM_WINDOWPOSCHANGED: {
					UpdateBounds();
					DefWndProc(ref m);
					break;
				}

				case Msg.WM_PAINT: {
					Rectangle	rect;
					PaintEventArgs	paint_event;

					paint_event = XplatUI.PaintEventStart(Handle);
					OnPaint(paint_event);
					XplatUI.PaintEventEnd(Handle);
					DefWndProc(ref m);	
					break;
				}
				
				case Msg.WM_ERASEBKGND:{					
					if (GetStyle (ControlStyles.UserPaint)){						
	    					PaintEventArgs eraseEventArgs = new PaintEventArgs (Graphics.FromHdc (m.WParam), new Rectangle (new Point (0,0),Size));
		    				OnPaintBackground (eraseEventArgs);												
	    					m.Result = (IntPtr)1;
    					}	
    					else {
    						m.Result = (IntPtr)0;
    						DefWndProc (ref m);	
    					}    					
    					
					break;
				}
				
				case Msg.WM_LBUTTONUP: {					
					int clicks = 1;
					
					OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							clicks, 
							LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							0));
					break;
				}
				
				case Msg.WM_LBUTTONDOWN: {					
					int clicks = 1;
					
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							0));
					
					break;
				}

				case Msg.WM_MOUSEWHEEL: {
					int clicks = 1;

					OnMouseWheel (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							HighOrder(m.WParam.ToInt32())));
					break;
				}

				
				case Msg.WM_MOUSEMOVE: {					
					int clicks = 1;

					OnMouseMove  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							clicks, 
							LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							0));
					break;
				}
				
				case Msg.WM_SIZE: {					
					UpdateBounds (bounds.X, bounds.Y, LowOrder ((int) m.LParam.ToInt32 ()),
						HighOrder ((int) m.LParam.ToInt32 ()));					
					
					DefWndProc(ref m);	
					break;				
				}

				case Msg.WM_KEYDOWN: {

					if (!ProcessKeyEventArgs (ref m))
						DefWndProc (ref m);

					break;					
				}

				case Msg.WM_KEYUP: {

					if (!ProcessKeyEventArgs (ref m))
						DefWndProc (ref m);

					break;					
				}		
				

#if notyet				
				case Msg.WM_WINDOWPOSCHANGED:	throw new NotImplementedException();	break;
				case Msg.WM_SYSCOLORCHANGE:	throw new NotImplementedException();	break;
				
#endif

				default:
					DefWndProc(ref m);	
					break;
			}
			
			
			
		}
		#endregion	// Public Instance Methods


		#region		// Protected Instance Methods
		protected virtual void CreateHandle() {
			if (IsDisposed) {
				throw new ObjectDisposedException(Name);
			}

			if (IsHandleCreated) {
				return;
			}

			if (window==null) {
				window = new ControlNativeWindow(this);
				window.CreateHandle(CreateParams);
			}

			if (window.Handle!=IntPtr.Zero) {
				if (!controls.Contains(window.Handle)) {
					controls.Add(window.Handle, this);
				}

				creator_thread = Thread.CurrentThread;

				OnHandleCreated(EventArgs.Empty);
			}
		}

		protected virtual void DefWndProc(ref Message m) {
			window.DefWndProc(ref m);
		}

		protected virtual void DestroyHandle() {
			if (IsHandleCreated) {
				if (Handle != IntPtr.Zero) {
					controls.Remove(Handle);
				}

				if (window != null) {
					window.DestroyHandle();
				}
			}
		}

		protected virtual bool ProcessKeyEventArgs (ref Message msg)
		{
			KeyEventArgs key_event;

			switch (msg.Msg) {
			case (int)Msg.WM_KEYDOWN: {
				key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
				OnKeyDown (key_event);
				return key_event.Handled;
			}
			case (int)Msg.WM_KEYUP: {
				key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
				OnKeyUp (key_event);
				return key_event.Handled;
			}

			default:
				break;
			}

			return false;
		}

		protected virtual bool IsInputKey (Keys keyData) 
		{
			return false;
		}

		protected virtual bool ProcessDialogKey (Keys keyData)
		{
			if (parent != null)
				return Parent.ProcessDialogKey (keyData);

    			return false;
		}

		protected bool GetStyle(ControlStyles flag) {
			return (control_style & flag) != 0;
		}

		protected virtual bool ProcessDialogChar(char charCode) {
			throw new NotImplementedException();
		}

		protected virtual bool ProcessMnemonic(char charCode) {
			throw new NotImplementedException();
		}

		protected void RecreateHandle() {
			IEnumerator child = child_controls.GetEnumerator();

			if (IsHandleCreated) {
				DestroyHandle();
				CreateHandle();

				// FIXME ZOrder?

				while (child.MoveNext()) {
					((Control)child.Current).RecreateHandle();
				}
			}
		}

		protected virtual void ScaleCore(float dx, float dy) {
			throw new NotImplementedException();
		}

		protected virtual void Select(bool directed, bool forward) {
			throw new NotImplementedException();
		}

		protected virtual void SetClientSizeCore(int x, int y) {
			bounds.Width=x;
			bounds.Height=y;
			XplatUI.MoveWindow(Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}

		protected void SetStyle(ControlStyles flag, bool value) {
			if (value) {
				control_style |= flag;
			} else {
				control_style &= ~flag;
			}
		}

		#endregion	// Public Instance Methods

		#region Private Instance Methods
		#endregion	// Private Instance Methods


		#region Private Instance Methods
		internal virtual void DoDefaultAction() {
			// Only here to be overriden by our actual controls; this is needed by the accessibility class
		}
		#endregion	// Private Instance Methods


		#region OnXXX methods
		protected virtual void OnBackColorChanged(EventArgs e) {
			if (BackColorChanged!=null) BackColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackColorChanged(e);
		}

		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			if (BackgroundImageChanged!=null) BackgroundImageChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackgroundImageChanged(e);
		}

		protected virtual void OnBindingContextChanged(EventArgs e) {
			if (BindingContextChanged!=null) BindingContextChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBindingContextChanged(e);
		}

		protected virtual void OnCausesValidationChanged(EventArgs e) {
			if (CausesValidationChanged!=null) CausesValidationChanged(this, e);
		}

		protected virtual void OnChangeUICues(UICuesEventArgs e) {
			if (CausesValidationChanged!=null) CausesValidationChanged(this, e);
		}

		protected virtual void OnClick(EventArgs e) {
			if (Click!=null) Click(this, e);
		}

		protected virtual void OnContextMenuChanged(EventArgs e) {
			if (ContextMenuChanged!=null) ContextMenuChanged(this, e);
		}

		protected virtual void OnControlAdded(ControlEventArgs e) {
			if (ControlAdded!=null) ControlAdded(this, e);
		}

		protected virtual void OnControlRemoved(ControlEventArgs e) {
			if (ControlRemoved!=null) ControlRemoved(this, e);
		}

		protected virtual void OnCreateControl() {
		}

		protected virtual void OnCursorChanged(EventArgs e) {
			if (CursorChanged!=null) CursorChanged(this, e);
		}

		protected virtual void OnDockChanged(EventArgs e) {
			if (DockChanged!=null) DockChanged(this, e);
		}

		protected virtual void OnDoubleClick(EventArgs e) {
			if (DoubleClick!=null) DoubleClick(this, e);
		}

		protected virtual void OnDragDrop(DragEventArgs drgevent) {
			if (DragDrop!=null) DragDrop(this, drgevent);
		}

		protected virtual void OnDragEnter(DragEventArgs drgevent) {
			if (DragEnter!=null) DragEnter(this, drgevent);
		}

		protected virtual void OnDragLeave(EventArgs e) {
			if (DragLeave!=null) DragLeave(this, e);
		}

		protected virtual void OnDragOver(DragEventArgs drgevent) {
			if (DragOver!=null) DragOver(this, drgevent);
		}

		protected virtual void OnEnabledChanged(EventArgs e) {
			if (EnabledChanged!=null) EnabledChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentEnabledChanged(e);
		}

		protected virtual void OnEnter(EventArgs e) {
			if (Enter!=null) Enter(this, e);
		}

		protected virtual void OnFontChanged(EventArgs e) {
			if (FontChanged!=null) FontChanged(this, e);
		}

		protected virtual void OnForeColorChanged(EventArgs e) {
			if (ForeColorChanged!=null) ForeColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentForeColorChanged(e);
		}

		protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent) {
			if (GiveFeedback!=null) GiveFeedback(this, gfbevent);
		}
		
		protected virtual void OnGotFocus(EventArgs e) {
			if (GotFocus!=null) GotFocus(this, e);
		}

		protected virtual void OnHandleCreated(EventArgs e) {
			if (HandleCreated!=null) HandleCreated(this, e);
		}

		protected virtual void OnHandleDestroyed(EventArgs e) {
			if (HandleDestroyed!=null) HandleDestroyed(this, e);
		}

		protected virtual void OnHelpRequested(HelpEventArgs hevent) {
			if (HelpRequested!=null) HelpRequested(this, hevent);
		}

		protected virtual void OnImeModeChanged(EventArgs e) {
			if (ImeModeChanged!=null) ImeModeChanged(this, e);
		}

		protected virtual void OnInvalidated(InvalidateEventArgs e) {
			if (Invalidated!=null) Invalidated(this, e);
		}


		protected virtual void OnKeyDown(KeyEventArgs e) {			
			if (KeyDown!=null) KeyDown(this, e);
		}

		protected virtual void OnKeyUp(KeyEventArgs e) {
			if (KeyUp!=null) KeyUp(this, e);
		}

		protected virtual void OnLayout(LayoutEventArgs levent) {
			if (Layout!=null) Layout(this, levent);
		}

		protected virtual void OnLeave(EventArgs e) {
			if (Leave!=null) Leave(this, e);
		}

		protected virtual void OnLocationChanged(EventArgs e) {
			if (LocationChanged!=null) LocationChanged(this, e);
		}

		protected virtual void OnLostFocus(EventArgs e) {
			if (LostFocus!=null) LostFocus(this, e);
		}

		protected virtual void OnMouseDown(MouseEventArgs e) {
			if (MouseDown!=null) MouseDown(this, e);
		}

		protected virtual void OnMouseEnter(EventArgs e) {
			if (MouseEnter!=null) MouseEnter(this, e);
		}

		protected virtual void OnMouseHover(EventArgs e) {
			if (MouseHover!=null) MouseHover(this, e);
		}

		protected virtual void OnMouseLeave(EventArgs e) {
			if (MouseLeave!=null) MouseLeave(this, e);
		}

		protected virtual void OnMouseMove(MouseEventArgs e) {			
			if (MouseMove!=null) MouseMove(this, e);
		}

		protected virtual void OnMouseUp(MouseEventArgs e) {
			if (MouseUp!=null) MouseUp(this, e);
		}

		protected virtual void OnMouseWheel(MouseEventArgs e) {
			if (MouseWheel!=null) MouseWheel(this, e);
		}

		protected virtual void OnMove(EventArgs e) {
			if (Move!=null) Move(this, e);
		}

		protected virtual void OnNotifyMessage(Message m) {
			// Override me!
		}

		protected virtual void OnPaint(PaintEventArgs e) {
			if (Paint!=null) Paint(this, e);
		}

		protected virtual void OnPaintBackground(PaintEventArgs pevent) {
			// Override me!
		}

		protected virtual void OnParentBackColorChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackColorChanged(e);
			}
		}

		protected virtual void OnParentBackgroundImageChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackgroundImageChanged(e);
			}
		}

		protected virtual void OnParentBindingContextChanged(EventArgs e) {
			if (binding_context==null) {
				binding_context=Parent.binding_context;
				OnBindingContextChanged(e);
			}
		}

		protected virtual void OnParentChanged(EventArgs e) {
			if (ParentChanged!=null) ParentChanged(this, e);
		}

		protected virtual void OnParentEnabledChanged(EventArgs e) {
			if ((is_enabled && !Parent.is_enabled) || (!is_enabled && Parent.is_enabled)) {
				is_enabled=false;
				Invalidate();
				EnabledChanged(this, e);
			}
		}

		protected virtual void OnParentFontChanged(EventArgs e) {
			if (font==null) {
				Invalidate();
				OnFontChanged(e);
			}
		}

		protected virtual void OnParentForeColorChanged(EventArgs e) {
			if (foreground_color.IsEmpty) {
				Invalidate();
				OnForeColorChanged(e);
			}
		}

		protected virtual void OnParentRightToLeftChanged(EventArgs e) {
			if (right_to_left==RightToLeft.Inherit) {
				Invalidate();
				OnRightToLeftChanged(e);
			}
		}

		protected virtual void OnParentVisibleChanged(EventArgs e) {
			if (is_visible!=Parent.is_visible) {
				is_visible=false;
				Invalidate();
				OnVisibleChanged(e);
			}
		}

		protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
			if (QueryContinueDrag!=null) QueryContinueDrag(this, e);
		}

		protected virtual void OnResize(EventArgs e) {
			if (Resize!=null) Resize(this, e);
			PerformLayout();
		}

		protected virtual void OnRightToLeftChanged(EventArgs e) {
			if (RightToLeftChanged!=null) RightToLeftChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentRightToLeftChanged(e);
		}

		protected virtual void OnSizeChanged(EventArgs e) {			
			OnResize(e);
			if (SizeChanged!=null) SizeChanged(this, e);
		}

		protected virtual void OnStyleChanged(EventArgs e) {
			if (StyleChanged!=null) StyleChanged(this, e);
		}

		protected virtual void OnSystemColorsChanged(EventArgs e) {
			if (SystemColorsChanged!=null) SystemColorsChanged(this, e);
		}

		protected virtual void OnTabIndexChanged(EventArgs e) {
			if (TabIndexChanged!=null) TabIndexChanged(this, e);
		}

		protected virtual void OnTabStopChanged(EventArgs e) {
			if (TabStopChanged!=null) TabStopChanged(this, e);
		}

		protected virtual void OnTextChanged(EventArgs e) {
			if (TextChanged!=null) TextChanged(this, e);
		}

		protected virtual void OnValidated(EventArgs e) {
			if (Validated!=null) Validated(this, e);
		}

		protected virtual void OnValidating(System.ComponentModel.CancelEventArgs e) {
			if (Validating!=null) Validating(this, e);
		}

		protected virtual void OnVisibleChanged(EventArgs e) {
			if (VisibleChanged!=null) VisibleChanged(this, e);
		}
		#endregion	// OnXXX methods

		#region Events
		public event EventHandler		BackColorChanged;
		public event EventHandler		BackgroundImageChanged;
		public event EventHandler		BindingContextChanged;
		public event EventHandler		CausesValidationChanged;
		public event UICuesEventHandler		ChangeUICues;
		public event EventHandler		Click;
		public event EventHandler		ContextMenuChanged;
		public event ControlEventHandler	ControlAdded;
		public event ControlEventHandler	ControlRemoved;
		public event EventHandler		CursorChanged;
		public event EventHandler		DockChanged;
		public event EventHandler		DoubleClick;
		public event DragEventHandler		DragDrop;
		public event DragEventHandler		DragEnter;
		public event EventHandler		DragLeave;
		public event DragEventHandler		DragOver;
		public event EventHandler		EnabledChanged;
		public event EventHandler		Enter;
		public event EventHandler		FontChanged;
		public event EventHandler		ForeColorChanged;
		public event GiveFeedbackEventHandler	GiveFeedback;
		public event EventHandler		GotFocus;
		public event EventHandler		HandleCreated;
		public event EventHandler		HandleDestroyed;
		public event HelpEventHandler		HelpRequested;
		public event EventHandler		ImeModeChanged;
		public event InvalidateEventHandler	Invalidated;
		public event KeyEventHandler		KeyDown;
		public event KeyPressEventHandler	KeyPress;
		public event KeyEventHandler		KeyUp;
		public event LayoutEventHandler		Layout;
		public event EventHandler		Leave;
		public event EventHandler		LocationChanged;
		public event EventHandler		LostFocus;
		public event MouseEventHandler		MouseDown;
		public event EventHandler		MouseEnter;
		public event EventHandler		MouseHover;
		public event EventHandler		MouseLeave;
		public event MouseEventHandler		MouseMove;
		public event MouseEventHandler		MouseUp;
		public event MouseEventHandler		MouseWheel;
		public event EventHandler		Move;
		public event PaintEventHandler		Paint;
		public event EventHandler		ParentChanged;
		public event QueryAccessibilityHelpEventHandler	QueryAccessibilityHelp;
		public event QueryContinueDragEventHandler	QueryContinueDrag;
		public event EventHandler		Resize;
		public event EventHandler		RightToLeftChanged;
		public event EventHandler		SizeChanged;
		public event EventHandler		StyleChanged;
		public event EventHandler		SystemColorsChanged;
		public event EventHandler		TabIndexChanged;
		public event EventHandler		TabStopChanged;
		public event EventHandler		TextChanged;
		public event EventHandler		Validated;
		public event CancelEventHandler		Validating;
		public event EventHandler		VisibleChanged;
		#endregion	// Events
		
		#region Private Methods
		internal static int LowOrder (int param) 
		{
			return (param & 0xffff);
		}

		internal static int HighOrder (int param) 
		{
			return (param >> 16);
		}
		
		internal static MouseButtons FromParamToMouseButtons (int param) 
		{		
			MouseButtons buttons = MouseButtons.None;
					
			if ((param & (int) MsgButtons.MK_LBUTTON) != 0)
				buttons |= MouseButtons.Left;
			
			if ((param & (int) MsgButtons.MK_MBUTTON) != 0)
				buttons |= MouseButtons.Middle;
				
			if ((param & (int) MsgButtons.MK_RBUTTON) != 0)
				buttons |= MouseButtons.Right;    	
				
			return buttons;

		}
		#endregion	
	}
}
