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
// $Revision: 1.11 $
// $Modtime: $
// $Log: Control.cs,v $
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

		// to be categorized...
		static internal Hashtable	controls;		// All of the applications controls, in a flat list
		internal ControlCollection	child_controls;		// our children
		internal Control		parent;			// our parent control
		internal int			num_of_children;	// number of children the control has
		internal Control[]		children;		// our children
		internal AccessibleObject	accessibility_object;	// object that contains accessibility information about our control
		internal AnchorStyles		anchor_style;		// TODO
		internal DockStyle		dock_style;		// TODO
		internal BindingContext		binding_context;	// TODO
		internal RightToLeft		right_to_left;		// drawing direction for control
		internal int			layout_suspended;
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
			private class Enumerator : IEnumerator {
				private Control	owner;
				private int	current;

				public Enumerator(Control owner) {
					this.owner=owner;
					this.current=-1;
				}

				public bool MoveNext() {
					current++;
					if (current>=owner.num_of_children) {
						return false;
					}

					return true;
				}

				public void Reset() {
					current=-1;
				}

				public object Current {
					get {
						if (current>=0 && current<owner.num_of_children) {
							return owner.children[current];
						} else {
							throw new InvalidOperationException("enumerator out of range");
						}
					}
				}
			}

			protected Control 	owner;

			#region ControlCollection Public Constructor
			public ControlCollection(Control owner) {
				this.owner=owner;
			}
			#endregion

			#region ControlCollection Public Instance Properties
			public int Count {
				get {
					return owner.num_of_children;
				}
			}

			public bool IsReadOnly {
				get {
					return false;
				}
			}

			public virtual Control this [int index] {
				get {
					if ((uint)index>=owner.num_of_children) {
						throw new ArgumentOutOfRangeException();
					}
					return owner.children[index];
				}
			}
			#endregion	// ControlCollection Public Instance Properties

			#region ControlCollection Public Instance Methods
			public virtual void Add(Control value) {
				// Don't add it if we already have it
				for (int i=0; i<owner.num_of_children; i++) {
					if (value==owner.child_controls[i]) {
						return;
					}
				}
				value.Parent=owner;
				owner.OnControlAdded(new ControlEventArgs(value));
			}

			public virtual void AddRange(Control[] controls) {
				for (int i=0; i<controls.Length; i++) {
					Add(controls[i]);
				}
			}

			public virtual void Clear() {
				owner.SuspendLayout();
				for (int i=0; i<owner.num_of_children; i++) {
					Remove(owner.children[i]);
				}
				owner.ResumeLayout();
			}

			public bool Contains(Control control) {
				for (int i=0; i<owner.num_of_children; i++) {
					if (owner.children[i]==control) {
						return true;
					}
				}

				return false;
			}

			public void CopyTo(Array dest, int index) {
				if (owner.num_of_children>0) {
					Array.Copy(owner.children, 0, dest, index, owner.num_of_children);
				}
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

				index=IndexOf(child);

				if (index==-1 && throwException) {
					throw new ArgumentException("Not a child control", "child");
				}
				return index;
			}

			public IEnumerator GetEnumerator() {
				return new ControlCollection.Enumerator(this.owner);
			}

			public override int GetHashCode() {
				return base.GetHashCode();
			}

			public int IndexOf(Control control) {
				int index;

				for (index=0; index<owner.num_of_children; index++) {
					if (owner.children[index] == control) {
						return index;
					}
				}
				return -1;
			}

			public virtual void Remove(Control value) {
				for (int i=0; i<owner.num_of_children; i++) {
					if (owner.children[i]==value) {
						RemoveAt(i);
					}
				}
			}

			public void RemoveAt(int index) {
				for (int i=index; i<owner.num_of_children; i++) {
					owner.children[i]=owner.children[i+1];
				}
			}

			public void SetChildIndex(Control child, int new_index) {
				int	old_index;

				old_index=IndexOf(child);
				if (old_index==-1) {
					throw new ArgumentException("Not a child control", "child");
				}

				if (old_index==new_index) {
					return;
				}

				RemoveAt(old_index);

				if (new_index>owner.num_of_children) {
					Add(child);
				} else {
					for (int i=owner.num_of_children-1;i>new_index; i--) {
						owner.children[i+1]=owner.children[i];
					}

					owner.children[new_index]=(Control)child;
				}
			}
			#endregion 	// ControlCollection Public Instance Methods

			#region ControlCollection Interface Methods
#if nodef
			int IList.Add(Control value) {

				Add(value);
				
			}
#endif

			int IList.Add(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Not of type Control", "value");
				}
				Add((Control)value);

				// Assumes the element was added to the end of the list
				return owner.num_of_children;
			}

			void IList.Clear() {
				this.Clear();
			}

			bool IList.Contains(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Not of type Control", "value");
				}
				return this.Contains((Control)value);
			}

			int IList.IndexOf(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Not of type Control", "value");
				}

				for (int i=0; i<owner.num_of_children; i++) {
					if (owner.children[i]==(Control)value) {
						return i;
					}
				}

				return -1;
			}

			void IList.Insert(int index, object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Not of type Control", "value");
				}

				if (index>owner.num_of_children) {
					throw new ArgumentOutOfRangeException("index");
				}

				for (int i=owner.num_of_children-1;i>index; i--) {
					owner.children[i+1]=owner.children[i];
				}

				owner.children[index]=(Control)value;
			}

			void IList.Remove(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Not of type Control", "value");
				}

				this.Remove((Control)value);
			}

			object IList.this [int index] {
				get {
					return owner.children[index];
				}

				set {
					
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
				}
			}

			object ICollection.SyncRoot {
				get {
					return this;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return false;
				}
			}

			Object ICloneable.Clone() {
				ControlCollection clone = new ControlCollection(this.owner);

				return clone;
			}
			#endregion // ControlCollection Interface Methods

			class ControlComparer : IComparer {
				int IComparer.Compare(object x, object y) {
					int	tab_index_x;
					int	tab_index_y;

					tab_index_x=((Control)x).tab_index;
					tab_index_y=((Control)y).tab_index;

					if (tab_index_x<tab_index_y) {
						return -1;
					} else if (tab_index_x>tab_index_y) {
						return 1;
					}
					return 0;
				}
			}
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
				return XplatUI.Defaults.BackColor;
			}
		}

		public static Font DefaultFont {
			get {
				return XplatUI.Defaults.Font;
			}
		}

		public static Color DefaultForeColor {
			get {
				return XplatUI.Defaults.ForeColor;
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

			for (int i=0; i<num_of_children; i++) {
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
			UpdateBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
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
				for (int i=0; i<num_of_children; i++) children[i].Invalidate();
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
				for (int i=0; i<num_of_children; i++) children[i].Invalidate();
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

		public void ResumeLayout() 
		{
			ResumeLayout (true);
		}

		[MonoTODO]
		public void ResumeLayout(bool peformLayout) 
		{
			layout_suspended--;
			
			if (layout_suspended > 0 || peformLayout == false)
				return;

			// PerformLayout and fire event			
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
					int delta = 1;			
					
					OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, LowOrder ((int) m.LParam.ToInt32 ()),
							HighOrder ((int) m.LParam.ToInt32 ()), delta));
					
					break;
				}
				
				case Msg.WM_LBUTTONDOWN: {					
					
					int clicks = 1;
					int delta = 1;					
					
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, LowOrder ((int) m.LParam.ToInt32 ()),
							HighOrder ((int) m.LParam.ToInt32 ()), delta));
					
					break;
				}
				
				
				case Msg.WM_MOUSEMOVE: {					
										
					int clicks = 1;
					int delta = 1;								
					
					OnMouseMove  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, LowOrder ((int) m.LParam.ToInt32 ()),
							HighOrder ((int) m.LParam.ToInt32 ()), delta));
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
			XplatUI.SetWindowPos(Handle, bounds);
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
			for (int i=0; i<num_of_children; i++) children[i].OnParentBackColorChanged(e);
		}

		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			if (BackgroundImageChanged!=null) BackgroundImageChanged(this, e);
			for (int i=0; i<num_of_children; i++) children[i].OnParentBackgroundImageChanged(e);
		}

		protected virtual void OnBindingContextChanged(EventArgs e) {
			if (BindingContextChanged!=null) BindingContextChanged(this, e);
			for (int i=0; i<num_of_children; i++) children[i].OnParentBindingContextChanged(e);
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
			for (int i=0; i<num_of_children; i++) children[i].OnParentEnabledChanged(e);
		}

		protected virtual void OnEnter(EventArgs e) {
			if (Enter!=null) Enter(this, e);
		}

		protected virtual void OnFontChanged(EventArgs e) {
			if (FontChanged!=null) FontChanged(this, e);
		}

		protected virtual void OnForeColorChanged(EventArgs e) {
			if (ForeColorChanged!=null) ForeColorChanged(this, e);
			for (int i=0; i<num_of_children; i++) children[i].OnParentForeColorChanged(e);
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
		}

		protected virtual void OnRightToLeftChanged(EventArgs e) {
			if (RightToLeftChanged!=null) RightToLeftChanged(this, e);
			for (int i=0; i<num_of_children; i++) children[i].OnParentRightToLeftChanged(e);
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
