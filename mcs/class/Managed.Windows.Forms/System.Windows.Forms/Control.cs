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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok		pbartok@novell.com
//
// Partially based on work by:
//	Aleksey Ryabchuk	ryabchuk@yahoo.com
//	Alexandre Pigolkine	pigolkine@gmx.de
//	Dennis Hayes		dennish@raytek.com
//	Jaak Simm		jaaksimm@firm.ee
//	John Sohn		jsohn@columbus.rr.com
//

#undef DebugRecreate
#undef DebugFocus

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Windows.Forms
{
#if NET_2_0
	[ComVisible(true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
#endif
	[Designer("System.Windows.Forms.Design.ControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Text")]
	[DefaultEvent("Click")]
	[DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[ToolboxItemFilter("System.Windows.Forms")]
	public class Control : Component, ISynchronizeInvoke, IWin32Window
#if NET_2_0
		, IBindableComponent, IDropTarget
#endif
	{
		#region Local Variables

		// Basic
		internal Rectangle		bounds;			// bounding rectangle for control (client area + decorations)
		Rectangle               explicit_bounds; // explicitly set bounds
		internal object			creator_thread;		// thread that created the control
		internal                ControlNativeWindow	window;			// object for native window handle
		string                  name; // for object naming

		// State
		bool                    is_created; // true if OnCreateControl has been sent
		internal bool		has_focus;		// true if control has focus
		internal bool		is_visible;		// true if control is visible
		internal bool		is_entered;		// is the mouse inside the control?
		internal bool		is_enabled;		// true if control is enabled (usable/not grayed out)
		bool                    is_accessible; // true if the control is visible to accessibility applications
		bool                    is_captured; // tracks if the control has captured the mouse
		internal bool			is_toplevel;		// tracks if the control is a toplevel window
		bool                    is_recreating; // tracks if the handle for the control is being recreated
		bool                    causes_validation; // tracks if validation is executed on changes
		bool                    is_focusing; // tracks if Focus has been called on the control and has not yet finished
		int                     tab_index; // position in tab order of siblings
		bool                    tab_stop; // is the control a tab stop?
		bool                    is_disposed; // has the window already been disposed?
		Size                    client_size; // size of the client area (window excluding decorations)
		Rectangle               client_rect; // rectangle with the client area (window excluding decorations)
		ControlStyles           control_style; // rather win32-specific, style bits for control
		ImeMode                 ime_mode;
		object                  control_tag; // object that contains data about our control
		internal int			mouse_clicks;		// Counter for mouse clicks
		Cursor                  cursor; // Cursor for the window
		internal bool			allow_drop;		// true if the control accepts droping objects on it   
		Region                  clip_region; // User-specified clip region for the window

		// Visuals
		internal Color			foreground_color;	// foreground color for control
		internal Color			background_color;	// background color for control
		Image                   background_image; // background image for control
		internal Font			font;			// font for control
		string                  text; // window/title text for control
		internal                BorderStyle		border_style;		// Border style of control

		// Layout
		internal enum LayoutType {
			Anchor,
			Dock
		}
		Layout.LayoutEngine layout_engine;
		int layout_suspended;
		bool layout_pending; // true if our parent needs to re-layout us
		internal AnchorStyles anchor_style; // anchoring requirements for our control
		internal DockStyle dock_style; // docking requirements for our control
		LayoutType layout_type;

		// Please leave the next 2 as internal until DefaultLayout (2.0) is rewritten
		internal int			dist_right; // distance to the right border of the parent
		internal int			dist_bottom; // distance to the bottom border of the parent

		// to be categorized...
		ControlCollection       child_controls; // our children
		Control                 parent; // our parent control
		AccessibleObject        accessibility_object; // object that contains accessibility information about our control
		BindingContext          binding_context;
		RightToLeft             right_to_left; // drawing direction for control
		ContextMenu             context_menu; // Context menu associated with the control

		// double buffering
		DoubleBuffer            backbuffer;
		
		// to implement DeviceContext without depending on double buffering
		Bitmap bmp;
		Graphics bmp_g;

		ControlBindingsCollection data_bindings;

#if NET_2_0
		internal bool use_compatible_text_rendering;
		static bool verify_thread_handle;
		Padding padding;
		ImageLayout backgroundimage_layout;
		Size maximum_size;
		Size minimum_size;
		Padding margin;
		private ContextMenuStrip context_menu_strip;
#endif

		#endregion	// Local Variables

		#region Private Classes
		// This helper class allows us to dispatch messages to Control.WndProc
		internal class ControlNativeWindow : NativeWindow {
			private Control owner;

			public ControlNativeWindow(Control control) : base() {
				this.owner=control;
			}


			public Control Owner {
				get {
					return owner;
				}
			}

			static internal Control ControlFromHandle(IntPtr hWnd) {
				ControlNativeWindow	window;

				window = (ControlNativeWindow)window_collection[hWnd];
				if (window != null) {
					return window.owner;
				}

				return null;
			}

			static internal Control ControlFromChildHandle (IntPtr handle) {
				ControlNativeWindow	window;

				Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
				while (hwnd != null) {
					window = (ControlNativeWindow)window_collection[hwnd.Handle];
					if (window != null) {
						return window.owner;
					}
					hwnd = hwnd.Parent;
				}

				return null;
			}

			protected override void WndProc(ref Message m) {
				owner.WndProc(ref m);
			}
		}
		#endregion
		
		#region Public Classes
		[ComVisible(true)]
		public class ControlAccessibleObject : AccessibleObject {
			IntPtr handle;

			#region ControlAccessibleObject Constructors
			public ControlAccessibleObject(Control ownerControl)
				: base (ownerControl)
			{
				if (ownerControl == null)
					throw new ArgumentNullException ("owner");

				handle = ownerControl.Handle;
			}
			#endregion	// ControlAccessibleObject Constructors

			#region ControlAccessibleObject Public Instance Properties
			public override string DefaultAction {
				get {
					return base.DefaultAction;
				}
			}

			public override string Description {
				get {
					return base.Description;
				}
			}

			public IntPtr Handle {
				get {
					return handle;
				}

				set {
					// We don't want to let them set it
				}
			}

			public override string Help {
				get {
					return base.Help;
				}
			}

			public override string KeyboardShortcut {
				get {
					return base.KeyboardShortcut;
				}
			}

			public override string Name {
				get {
					return base.Name;
				}

				set {
					base.Name = value;
				}
			}

			public Control Owner {
				get {
					return base.owner;
				}
			}

			public override AccessibleObject Parent {
				get {
					return base.Parent;
				}
			}


			public override AccessibleRole Role {
				get {
					return base.Role;
				}
			}
			#endregion	// ControlAccessibleObject Public Instance Properties

			#region ControlAccessibleObject Public Instance Methods
			public override int GetHelpTopic(out string FileName) {
				return base.GetHelpTopic (out FileName);
			}

			[MonoTODO ("Implement this")]
			public void NotifyClients(AccessibleEvents accEvent) {
				throw new NotImplementedException();
			}

			[MonoTODO ("Implement this")]
			public void NotifyClients(AccessibleEvents accEvent, int childID) {
			}

			public override string ToString() {
				return "ControlAccessibleObject: Owner = " + owner.ToString() + ", Text: " + owner.text;
			}

			#endregion	// ControlAccessibleObject Public Instance Methods
		}

		private class DoubleBuffer : IDisposable
		{
			public Region InvalidRegion;
			private Stack real_graphics;
			private object back_buffer;
			private Control parent;
			private bool pending_disposal;
			
			public DoubleBuffer (Control parent)
			{
				this.parent = parent;
				real_graphics = new Stack ();
				int width = parent.Width;
				int height = parent.Height;

				if (width < 1) width = 1;
				if (height < 1) height = 1;

				XplatUI.CreateOffscreenDrawable (parent.Handle, width, height, out back_buffer);
				Invalidate ();
			}
			
			public void Blit (PaintEventArgs pe)
			{
				Graphics buffered_graphics;
				buffered_graphics = XplatUI.GetOffscreenGraphics (back_buffer);
				XplatUI.BlitFromOffscreen (parent.Handle, pe.Graphics, back_buffer, buffered_graphics, pe.ClipRectangle);
				buffered_graphics.Dispose ();
			}
			
			public void Start (PaintEventArgs pe)
			{				
				// We need to get the graphics for every paint.
				real_graphics.Push(pe.SetGraphics (XplatUI.GetOffscreenGraphics (back_buffer)));
			}

			public void End (PaintEventArgs pe)
			{
				Graphics buffered_graphics;
				buffered_graphics = pe.SetGraphics ((Graphics) real_graphics.Pop ());

				if (pending_disposal) 
					Dispose ();
				else {
					XplatUI.BlitFromOffscreen (parent.Handle, pe.Graphics, back_buffer, buffered_graphics, pe.ClipRectangle);
					InvalidRegion.Exclude (pe.ClipRectangle);
				}
				buffered_graphics.Dispose ();
			}
			
			public void Invalidate ()
			{
				if (InvalidRegion != null)
					InvalidRegion.Dispose ();
				InvalidRegion = new Region (parent.ClientRectangle);
			}
			
			public void Dispose ()
			{
				if (real_graphics.Count > 0) {
					pending_disposal = true;
					return;
				}
				
				XplatUI.DestroyOffscreenDrawable (back_buffer);

				if (InvalidRegion != null)
					InvalidRegion.Dispose ();
				InvalidRegion = null;
				back_buffer = null;
				GC.SuppressFinalize (this);
			}

			#region IDisposable Members
			void IDisposable.Dispose ()
			{
				Dispose ();
			}
			#endregion
			
			~DoubleBuffer ()
			{
				Dispose ();
			}
		}

		[ListBindable (false)]
#if NET_2_0
		[ComVisible (false)]
		public class ControlCollection : Layout.ArrangedElementCollection, IList, ICollection, ICloneable, IEnumerable {
#else
		[DesignerSerializer("System.Windows.Forms.Design.ControlCollectionCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
		public class ControlCollection : IList, ICollection, ICloneable, IEnumerable {
#endif
			#region	ControlCollection Local Variables
#if !NET_2_0
			ArrayList list;
#endif
			ArrayList impl_list;
			Control[] all_controls;
			Control owner;
			#endregion	// ControlCollection Local Variables

			#region ControlCollection Public Constructor
			public ControlCollection(Control owner) {
				this.owner=owner;
#if !NET_2_0
				this.list=new ArrayList();
#endif
			}
			#endregion

			#region	ControlCollection Public Instance Properties
			int ICollection.Count {
				get { return Count; }
			}


#if !NET_2_0
			public int Count {
				get { return list.Count; }
			}
#endif

#if NET_2_0
			bool IList.IsReadOnly
#else
			public bool IsReadOnly
#endif
			{
				get {
					return list.IsReadOnly;
				}
			}

#if NET_2_0
			public Control Owner { get { return this.owner; } }
			
			public virtual Control this[string key] {
				get { 
					int index = IndexOfKey (key);
					
					if (index >= 0)
						return this[index];
						
					return null;
				}
			}
			
			new
#endif
			public virtual Control this[int index] {
				get {
					if (index < 0 || index >= list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
					}
					return (Control)list[index];
				}
				
				
			}
			#endregion // ControlCollection Public Instance Properties
			
			#region	ControlCollection Instance Methods
			public virtual void Add (Control value)
			{
				if (value == null)
					return;

				bool owner_permits_toplevels = (owner is MdiClient) || (owner is Form && ((Form)owner).IsMdiContainer);
				bool child_is_toplevel = ((Control)value).GetTopLevel();
				bool child_is_mdichild = (value is Form && ((Form)value).IsMdiChild);

				if (child_is_toplevel && !(owner_permits_toplevels && child_is_mdichild))
					throw new ArgumentException("Cannot add a top level control to a control.", "value");
				
				if (Contains (value)) {
					owner.PerformLayout();
					return;
				}

				if (value.tab_index == -1) {
					int	end;
					int	index;
					int	use;

					use = 0;
					end = owner.child_controls.Count;
					for (int i = 0; i < end; i++) {
						index = owner.child_controls[i].tab_index;
						if (index >= use) {
							use = index + 1;
						}
					}
					value.tab_index = use;
				}

				if (value.parent != null) {
					value.parent.Controls.Remove(value);
				}

				all_controls = null;
				list.Add (value);

				value.ChangeParent(owner);

				value.InitLayout();

				owner.UpdateChildrenZOrder();
				owner.PerformLayout(value, "Parent");
				owner.OnControlAdded(new ControlEventArgs(value));
			}
			
			internal void AddToList (Control c)
			{
				all_controls = null;
				list.Add (c);
			}

			internal virtual void AddImplicit (Control control)
			{
				if (impl_list == null)
					impl_list = new ArrayList ();

				if (AllContains (control)) {
					owner.PerformLayout ();
					return;
				}

				if (control.parent != null) {
					control.parent.Controls.Remove(control);
				}

				all_controls = null;
				impl_list.Add (control);

				control.ChangeParent (owner);
				control.InitLayout ();
				owner.UpdateChildrenZOrder ();
				owner.PerformLayout (control, "Parent");
				owner.OnControlAdded (new ControlEventArgs (control));
			}
#if NET_2_0
			[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#endif
			public virtual void AddRange (Control[] controls)
			{
				if (controls == null)
					throw new ArgumentNullException ("controls");

				owner.SuspendLayout ();

				try {
					for (int i = 0; i < controls.Length; i++) 
						Add (controls[i]);
				} finally {
					owner.ResumeLayout ();
				}
			}

			internal virtual void AddRangeImplicit (Control [] controls)
			{
				if (controls == null)
					throw new ArgumentNullException ("controls");

				owner.SuspendLayout ();

				try {
					for (int i = 0; i < controls.Length; i++)
						AddImplicit (controls [i]);
				} finally {
					owner.ResumeLayout ();
				}
			}

#if NET_2_0
			new
#endif
			public virtual void Clear ()
			{
				all_controls = null;

				// MS sends remove events in reverse order
				while (list.Count > 0) {
					Remove((Control)list[list.Count - 1]);
				}
			}

			internal virtual void ClearImplicit ()
			{
				if (impl_list == null)
					return;
				all_controls = null;
				impl_list.Clear ();
			}

			public bool Contains (Control value)
			{
				for (int i = list.Count; i > 0; ) {
					i--;
					
					if (list [i] == value) {
						// Do we need to do anything here?
						return true;
					}
				}
				return false;
			}

			internal bool ImplicitContains (Control value)
			{
				if (impl_list == null)
					return false;

				for (int i = impl_list.Count; i > 0; ) {
					i--;
					
					if (impl_list [i] == value) {
						// Do we need to do anything here?
						return true;
					}
				}
				return false;
			}

			internal bool AllContains (Control value)
			{
				return Contains (value) || ImplicitContains (value);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) >= 0;
			}
#endif

			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo (array, index);
			}

#if !NET_2_0
			public void CopyTo (Array array, int index)
			{
				list.CopyTo(array, index);
			}

			public override bool Equals (object other)
			{
				if (other is ControlCollection && (((ControlCollection)other).owner==this.owner)) {
					return(true);
				} else {
					return(false);
				}
			}
#endif

#if NET_2_0
			// LAMESPEC: MSDN says AE, MS implementation throws ANE
			public Control[] Find (string key, bool searchAllChildren)
			{
				if (string.IsNullOrEmpty (key))
					throw new ArgumentNullException ("key");
					
				ArrayList al = new ArrayList ();
				
				foreach (Control c in list) {
					if (c.Name.Equals (key, StringComparison.CurrentCultureIgnoreCase))
						al.Add (c);
						
					if (searchAllChildren)
						al.AddRange (c.Controls.Find (key, true));
				}
				
				return (Control[])al.ToArray (typeof (Control));
			}
#endif

			public int GetChildIndex(Control child) {
				return GetChildIndex(child, false);
			}

#if NET_2_0
			public virtual int
#else
			public int
#endif
			GetChildIndex(Control child, bool throwException) {
				int index;

				index=list.IndexOf(child);

				if (index==-1 && throwException) {
					throw new ArgumentException("Not a child control", "child");
				}
				return index;
			}

#if NET_2_0
			public override IEnumerator
#else
			public IEnumerator
#endif
			GetEnumerator ()
			{
				return list.GetEnumerator();
			}

			internal IEnumerator GetAllEnumerator ()
			{
				Control [] res = GetAllControls ();
				return res.GetEnumerator ();
			}

			internal Control [] GetAllControls ()
			{
				if (all_controls != null)
					return all_controls;

				if (impl_list == null) {
					all_controls = (Control []) list.ToArray (typeof (Control));
					return all_controls;
				}
				
				all_controls = new Control [list.Count + impl_list.Count];
				impl_list.CopyTo (all_controls);
				list.CopyTo (all_controls, impl_list.Count);

				return all_controls;
			}

#if !NET_2_0
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
#endif

			public int IndexOf(Control control)
			{
				return list.IndexOf(control);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (string.IsNullOrEmpty (key))
					return -1;
					
				for (int i = 0; i < list.Count; i++)
					if (((Control)list[i]).Name.Equals (key, StringComparison.CurrentCultureIgnoreCase))
						return i;
						
				return -1;
			}
#endif

			public virtual void Remove(Control value)
			{
				if (value == null)
					return;

				owner.PerformLayout(value, "Parent");
				owner.OnControlRemoved(new ControlEventArgs(value));

				all_controls = null;
				list.Remove(value);

				value.ChangeParent(null);

				owner.UpdateChildrenZOrder();
			}

			internal virtual void RemoveImplicit (Control control)
			{
				if (impl_list != null) {
					all_controls = null;
					owner.PerformLayout (control, "Parent");
					owner.OnControlRemoved (new ControlEventArgs (control));
					impl_list.Remove (control);
				}
				control.ChangeParent (null);
				owner.UpdateChildrenZOrder ();
			}

#if NET_2_0
			new
#endif
			public void RemoveAt(int index)
			{
				if (index < 0 || index >= list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
				}
				Remove ((Control)list[index]);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int index = IndexOfKey (key);
				
				if (index >= 0)
					RemoveAt (index);
			}	
		
			public virtual void
#else
			public void
#endif
			SetChildIndex(Control child, int newIndex)
			{
				if (child == null)
					throw new ArgumentNullException ("child");

				int	old_index;

				old_index=list.IndexOf(child);
				if (old_index==-1) {
					throw new ArgumentException("Not a child control", "child");
				}

				if (old_index==newIndex) {
					return;
				}

				all_controls = null;
				list.RemoveAt(old_index);

				if (newIndex>list.Count) {
					list.Add(child);
				} else {
					list.Insert(newIndex, child);
				}
				child.UpdateZOrder();
				owner.PerformLayout();
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

					all_controls = null;
					Control ctrl = (Control) value;
					list[index]= ctrl;

					ctrl.ChangeParent(owner);

					ctrl.InitLayout();

					owner.UpdateChildrenZOrder();
					owner.PerformLayout(ctrl, "Parent");
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
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
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}

				if (value == null) {
					throw new ArgumentException("value", "Cannot add null controls");
				}

				bool owner_permits_toplevels = (owner is MdiClient) || (owner is Form && ((Form)owner).IsMdiContainer);
				bool child_is_toplevel = ((Control)value).GetTopLevel();
				bool child_is_mdichild = (value is Form && ((Form)value).IsMdiChild);

				if (child_is_toplevel && !(owner_permits_toplevels && child_is_mdichild))
					throw new ArgumentException("Cannot add a top level control to a control.", "value");

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
				all_controls = null;
				list.Insert(index, value);
			}

			void IList.Remove(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				all_controls = null;
				list.Remove(value);
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
		public Control ()
		{
			layout_type = LayoutType.Anchor;
			anchor_style = AnchorStyles.Top | AnchorStyles.Left;

			is_created = false;
			is_visible = true;
			is_captured = false;
			is_disposed = false;
			is_enabled = true;
			is_entered = false;
			layout_pending = false;
			is_toplevel = false;
			causes_validation = true;
			has_focus = false;
			layout_suspended = 0;
			mouse_clicks = 1;
			tab_index = -1;
			cursor = null;
			right_to_left = RightToLeft.Inherit;
			border_style = BorderStyle.None;
			background_color = Color.Empty;
			dist_right = 0;
			dist_bottom = 0;
			tab_stop = true;
			ime_mode = ImeMode.Inherit;

#if NET_2_0
			backgroundimage_layout = ImageLayout.Tile;
			use_compatible_text_rendering = Application.use_compatible_text_rendering;
			padding = new Padding(0);
			maximum_size = new Size();
			minimum_size = new Size();
			margin = this.DefaultMargin;
#endif

			control_style = ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
					ControlStyles.Selectable | ControlStyles.StandardClick | 
					ControlStyles.StandardDoubleClick;
#if NET_2_0
			control_style |= ControlStyles.UseTextForAccessibility;
#endif

			parent = null;
			background_image = null;
			text = string.Empty;
			name = string.Empty;

			window = new ControlNativeWindow(this);
			child_controls = CreateControlsInstance();
			client_size = new Size(DefaultSize.Width, DefaultSize.Height);
			client_rect = new Rectangle(0, 0, DefaultSize.Width, DefaultSize.Height);
			bounds.Size = SizeFromClientSize (client_size);
			explicit_bounds = bounds;
		}

		public Control (Control parent, string text) : this()
		{
			Text=text;
			Parent=parent;
		}

		public Control (Control parent, string text, int left, int top, int width, int height) : this()
		{
			Parent=parent;
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBounds(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		public Control (string text) : this()
		{
			Text=text;
		}

		public Control (string text, int left, int top, int width, int height) : this()
		{
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBounds(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		private delegate void RemoveDelegate(object c);

		protected override void Dispose (bool disposing)
		{
			if (!is_disposed && disposing) {
				Capture = false;

				DisposeBackBuffer ();

				if (bmp != null) {
					bmp.Dispose ();
					bmp = null;
				}
				if (bmp_g != null) {
					bmp_g.Dispose ();
					bmp_g = null;
				}
				
				if (this.InvokeRequired) {
					if (Application.MessageLoop) {
						this.BeginInvokeInternal(new MethodInvoker(DestroyHandle), null, true);
					}
				} else {
					DestroyHandle();
				}

				if (parent != null) {
					parent.Controls.Remove(this);
				}

				Control [] children = child_controls.GetAllControls ();
				for (int i=0; i<children.Length; i++) {
					children[i].parent = null;	// Need to set to null or our child will try and remove from ourselves and crash
					children[i].Dispose();
				}
			}
			is_disposed = true;
			is_visible = false;
			base.Dispose(disposing);
		}
		#endregion 	// Public Constructors

		#region Internal Properties
		internal bool VisibleInternal {
			get { return is_visible; }
		}

		internal LayoutType ControlLayoutType {
			get { return layout_type; }
		}

		internal BorderStyle InternalBorderStyle {
			get {
				return border_style;
			}

			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));

				if (border_style != value) {
					border_style = value;

					if (IsHandleCreated) {
						XplatUI.SetBorderStyle (window.Handle, (FormBorderStyle)border_style);
						RecreateHandle ();
						Refresh ();
					}
				}
			}
		}
		#endregion	// Internal Properties

		#region Private & Internal Methods
		
#if NET_2_0
		void IDropTarget.OnDragDrop (DragEventArgs e)
		{
			OnDragDrop (e);
		}
		
		void IDropTarget.OnDragEnter (DragEventArgs e)
		{
			OnDragEnter (e);
		}
		
		void IDropTarget.OnDragLeave (EventArgs e)
		{
			OnDragLeave (e);
		}

		void IDropTarget.OnDragOver (DragEventArgs e)
		{
			OnDragOver (e);
		}
#endif

		internal IAsyncResult BeginInvokeInternal (Delegate method, object [] args, bool disposing) {
			AsyncMethodResult	result;
			AsyncMethodData		data;

			if (!disposing) {
				Control p = this;
				do {
					if (!p.IsHandleCreated)
						throw new InvalidOperationException("Cannot call Invoke or BeginInvoke on a control until the window handle is created");
					p = p.parent;
				} while (p != null);
			}

			result = new AsyncMethodResult ();
			data = new AsyncMethodData ();

			data.Handle = window.Handle;
			data.Method = method;
			data.Args = args;
			data.Result = result;

#if NET_2_0
			if (!ExecutionContext.IsFlowSuppressed ()) {
				data.Context = ExecutionContext.Capture ();
			}
#else
#if !MWF_ON_MSRUNTIME
			if (SecurityManager.SecurityEnabled) {
				data.Stack = CompressedStack.GetCompressedStack ();
			}
#endif
#endif

			XplatUI.SendAsyncMethod (data);
			return result;
		}

		
		internal void PointToClient (ref int x, ref int y)
		{
			XplatUI.ScreenToClient (Handle, ref x, ref y);
		}

		internal void PointToScreen (ref int x, ref int y)
		{
			XplatUI.ClientToScreen (Handle, ref x, ref y);
		}

		internal bool IsRecreating {
			get {
				return is_recreating;
			}
		}

		internal Graphics DeviceContext {
			get {
				if (bmp_g == null) {
					bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					bmp_g = Graphics.FromImage (bmp);
				}
				return bmp_g;
			}
		}

		private void InvalidateBackBuffer ()
		{
			if (backbuffer != null)
				backbuffer.Invalidate ();
		}

		private DoubleBuffer GetBackBuffer ()
		{
			if (backbuffer == null)
				backbuffer = new DoubleBuffer (this);
			return backbuffer;
		}

		private void DisposeBackBuffer ()
		{
			if (backbuffer != null) {
				backbuffer.Dispose ();
				backbuffer = null;
			}
		}

		internal static void SetChildColor(Control parent) {
			Control	child;

			for (int i=0; i < parent.child_controls.Count; i++) {
				child=parent.child_controls[i];
				if (child.child_controls.Count>0) {
					SetChildColor(child);
				}
			}
		}

		internal bool Select(Control control) {
			IContainerControl	container;

			if (control == null) {
				return false;
			}

			container = GetContainerControl();
			if (container != null && (Control)container != control) {
				container.ActiveControl = control;
			}
			else if (control.IsHandleCreated) {
				XplatUI.SetFocus(control.window.Handle);
			}
			return true;
		}

		internal virtual void DoDefaultAction() {
			// Only here to be overriden by our actual controls; this is needed by the accessibility class
		}

		internal static IntPtr MakeParam (int low, int high){
			return new IntPtr (high << 16 | low & 0xffff);
		}

		internal static int LowOrder (int param) {
			return ((int)(short)(param & 0xffff));
		}

		internal static int HighOrder (int param) {
			return ((int)(short)(param >> 16));
		}

		// This method exists so controls overriding OnPaintBackground can have default background painting done
		internal virtual void PaintControlBackground (PaintEventArgs pevent)
		{
			if (GetStyle(ControlStyles.SupportsTransparentBackColor) && (BackColor.A != 0xff)) {
				if (parent != null) {
					PaintEventArgs	parent_pe;
					GraphicsState	state;

					parent_pe = new PaintEventArgs(pevent.Graphics, new Rectangle(pevent.ClipRectangle.X + Left, pevent.ClipRectangle.Y + Top, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height));

					state = parent_pe.Graphics.Save();
					parent_pe.Graphics.TranslateTransform(-Left, -Top);
					parent.OnPaintBackground(parent_pe);
					parent_pe.Graphics.Restore(state);

					state = parent_pe.Graphics.Save();
					parent_pe.Graphics.TranslateTransform(-Left, -Top);
					parent.OnPaint(parent_pe);
					parent_pe.Graphics.Restore(state);
					parent_pe.SetGraphics(null);
				}
			}

			if ((clip_region != null) && (XplatUI.UserClipWontExposeParent)) {
				if (parent != null) {
					PaintEventArgs	parent_pe;
					Region		region;
					GraphicsState	state;
					Hwnd		hwnd;

					hwnd = Hwnd.ObjectFromHandle(Handle);

					if (hwnd != null) {
						parent_pe = new PaintEventArgs(pevent.Graphics, new Rectangle(pevent.ClipRectangle.X + Left, pevent.ClipRectangle.Y + Top, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height));

						region = new Region ();
						region.MakeEmpty();
						region.Union(ClientRectangle);

						foreach (Rectangle r in hwnd.ClipRectangles) {
							region.Union (r);
						}

						state = parent_pe.Graphics.Save();
						parent_pe.Graphics.Clip = region;

						parent_pe.Graphics.TranslateTransform(-Left, -Top);
						parent.OnPaintBackground(parent_pe);
						parent_pe.Graphics.Restore(state);

						state = parent_pe.Graphics.Save();
						parent_pe.Graphics.Clip = region;

						parent_pe.Graphics.TranslateTransform(-Left, -Top);
						parent.OnPaint(parent_pe);
						parent_pe.Graphics.Restore(state);
						parent_pe.SetGraphics(null);

						region.Intersect(clip_region);
						pevent.Graphics.Clip = region;
					}
				}
			}

			if (background_image == null) {
				pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), new Rectangle(pevent.ClipRectangle.X - 1, pevent.ClipRectangle.Y - 1, pevent.ClipRectangle.Width + 2, pevent.ClipRectangle.Height + 2));
				return;
			}

			DrawBackgroundImage (pevent.Graphics);
		}

		void DrawBackgroundImage (Graphics g)
		{
#if NET_2_0
			Rectangle drawing_rectangle = new Rectangle ();
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
				
			switch (backgroundimage_layout)
			{
			case ImageLayout.Tile:
				using (TextureBrush b = new TextureBrush (background_image, WrapMode.Tile)) {
					g.FillRectangle (b, ClientRectangle);
				}
				return;
			case ImageLayout.Center:
				drawing_rectangle.Location = new Point (ClientSize.Width / 2 - background_image.Width / 2, ClientSize.Height / 2 - background_image.Height / 2);
				drawing_rectangle.Size = background_image.Size;
				break;
			case ImageLayout.None:
				drawing_rectangle.Location = Point.Empty;
				drawing_rectangle.Size = background_image.Size;
				break;
			case ImageLayout.Stretch:
				drawing_rectangle = ClientRectangle;
				break;
			case ImageLayout.Zoom:
				drawing_rectangle = ClientRectangle;
				if ((float)background_image.Width / (float)background_image.Height < (float)drawing_rectangle.Width / (float) drawing_rectangle.Height) {
					drawing_rectangle.Width = (int) (background_image.Width * ((float)drawing_rectangle.Height / (float)background_image.Height));
					drawing_rectangle.X = (ClientRectangle.Width - drawing_rectangle.Width) / 2;
				} else {
					drawing_rectangle.Height = (int) (background_image.Height * ((float)drawing_rectangle.Width / (float)background_image.Width));
					drawing_rectangle.Y = (ClientRectangle.Height - drawing_rectangle.Height) / 2;
				}
				break;
			default:
				return;
			}

			g.DrawImage (background_image, drawing_rectangle);

#else
			using (TextureBrush b = new TextureBrush (background_image, WrapMode.Tile)) {
				g.FillRectangle (b, ClientRectangle);
			}
#endif
		}

		internal virtual void DndEnter (DragEventArgs e)
		{
			try {
				OnDragEnter (e);
			} catch { }
		}

		internal virtual void DndOver (DragEventArgs e)
		{
			try {
				OnDragOver (e);
			} catch { }
		}

		internal virtual void DndDrop (DragEventArgs e)
		{
			try {
				OnDragDrop (e);
			} catch (Exception exc) {
				Console.Error.WriteLine ("MWF: Exception while dropping:");
				Console.Error.WriteLine (exc);
			}
		}

		internal virtual void DndLeave (EventArgs e)
		{
			try {
				OnDragLeave (e);
			} catch { }
		}

		internal virtual void DndFeedback(GiveFeedbackEventArgs e)
		{
			try {
				OnGiveFeedback(e);
			} catch { }
		}

		internal virtual void DndContinueDrag(QueryContinueDragEventArgs e)
		{
			try {
				OnQueryContinueDrag(e);
			} catch { }
		}
		
		internal static MouseButtons FromParamToMouseButtons (int param) {		
			MouseButtons buttons = MouseButtons.None;
					
			if ((param & (int) MsgButtons.MK_LBUTTON) != 0)
				buttons |= MouseButtons.Left;
			
			if ((param & (int) MsgButtons.MK_MBUTTON) != 0)
				buttons |= MouseButtons.Middle;
				
			if ((param & (int) MsgButtons.MK_RBUTTON) != 0)
				buttons |= MouseButtons.Right;
				
			return buttons;

		}

		internal virtual void FireEnter ()
		{
			OnEnter (EventArgs.Empty);
		}

		internal virtual void FireLeave ()
		{
			OnLeave (EventArgs.Empty);
		}

		internal virtual void FireValidating (CancelEventArgs ce)
		{
			OnValidating (ce);
		}

		internal virtual void FireValidated ()
		{
			OnValidated (EventArgs.Empty);
		}

		internal virtual bool ProcessControlMnemonic(char charCode) {
			return ProcessMnemonic(charCode);
		}

		private static Control FindFlatForward(Control container, Control start) {
			Control	found;
			int	index;
			int	end;

			found = null;
			end = container.child_controls.Count;

			if (start != null) {
				index = start.tab_index;
			} else {
				index = -1;
			}

			for (int i = 0, pos = -1; i < end; i++) {
				if (start == container.child_controls[i]) {
					pos = i;
					continue;
				}

				if (found == null) {
					if (container.child_controls[i].tab_index > index || (pos > -1 && pos < i && container.child_controls[i].tab_index == index)) {
						found = container.child_controls[i];
					}
				} else if (found.tab_index > container.child_controls[i].tab_index) {
					if (container.child_controls[i].tab_index > index) {
						found = container.child_controls[i];
					}
				}
			}
			return found;
		}

		private static Control FindControlForward(Control container, Control start) {
			Control found;

			found = null;

			if (start == null) {
				return FindFlatForward(container, start);
			}

			if (start.child_controls != null && start.child_controls.Count > 0 && 
				(start == container || !((start is IContainerControl) &&  start.GetStyle(ControlStyles.ContainerControl)))) {
				return FindControlForward(start, null);
			}
			else {
				while (start != container) {
					found = FindFlatForward(start.parent, start);
					if (found != null) {
						return found;
					}
					start = start.parent;
				}
			}
			return null;
		}

		private static Control FindFlatBackward(Control container, Control start) {
			Control	found;
			int	index;
			int	end;

			found = null;
			end = container.child_controls.Count;

			if (start != null) {
				index = start.tab_index;
			} else {
				// FIXME: Possible speed-up: Keep the highest taborder index in the container
				index = -1;
				for (int i = 0; i < end; i++) {
					if (container.child_controls[i].tab_index > index) {
						index = container.child_controls[i].tab_index;
					}
				}
				index++;
			}

			bool hit = false;
					
			for (int i = end - 1; i >= 0; i--) {
				if (start == container.child_controls[i]) {
					hit = true;
					continue;
				}

				if (found == null || found.tab_index < container.child_controls[i].tab_index) {
					if (container.child_controls[i].tab_index < index || (hit && container.child_controls[i].tab_index == index))
						found = container.child_controls[i];

				}
			}
			return found;
		}

		private static Control FindControlBackward(Control container, Control start) {

			Control found = null;

			if (start == null) {
				found = FindFlatBackward(container, start);
			}
			else if (start != container) {
				if (start.parent != null) {
					found = FindFlatBackward(start.parent, start);

					if (found == null) {
						if (start.parent != container)
							return start.parent;
						return null;
					}
				}
			}
		
			if (found == null || start.parent == null)
				found = start;

			while (found != null && (found == container || (!((found is IContainerControl) && found.GetStyle(ControlStyles.ContainerControl))) &&
				found.child_controls != null && found.child_controls.Count > 0)) {
//				while (ctl.child_controls != null && ctl.child_controls.Count > 0 && 
//					(ctl == this || (!((ctl is IContainerControl) && ctl.GetStyle(ControlStyles.ContainerControl))))) {
				found = FindFlatBackward(found, null);
			}

			return found;

/*
			Control found;

			found = null;

			if (start != null) {
				found = FindFlatBackward(start.parent, start);
				if (found == null) {
					if (start.parent != container) {
						return start.parent;
					}
				}
			}
			if (found == null) {
				found = FindFlatBackward(container, start);
			}

			if (container != start) {
				while ((found != null) && (!found.Contains(start)) && found.child_controls != null && found.child_controls.Count > 0 && !(found is IContainerControl)) {// || found.GetStyle(ControlStyles.ContainerControl))) {
					found = FindControlBackward(found, null);
					 if (found != null) {
						return found;
					}
				}
			}
			return found;
*/			
		}

		internal virtual void HandleClick(int clicks, MouseEventArgs me) {
			if (GetStyle(ControlStyles.StandardClick)) {
				if ((clicks > 1) && GetStyle(ControlStyles.StandardDoubleClick)) {
#if !NET_2_0
					OnDoubleClick(EventArgs.Empty);
				} else {
					OnClick(EventArgs.Empty);
#else
					OnDoubleClick(me);
				} else {
					OnClick(me);
					OnMouseClick (me);
#endif
				}
			}
		}
		
		internal void CaptureWithConfine (Control ConfineWindow)
		{
			if (this.IsHandleCreated && !is_captured) {
				is_captured = true;
				XplatUI.GrabWindow (this.window.Handle, ConfineWindow.Handle);
			}
		}

		private void CheckDataBindings ()
		{
			if (data_bindings == null)
				return;

			BindingContext binding_context = BindingContext;
			foreach (Binding binding in data_bindings) {
				binding.Check (binding_context);
			}
		}

		private void ChangeParent(Control new_parent) {
			bool		pre_enabled;
			bool		pre_visible;
			Font		pre_font;
			Color		pre_fore_color;
			Color		pre_back_color;
			RightToLeft	pre_rtl;

			// These properties are inherited from our parent
			// Get them pre parent-change and then send events
			// if they are changed after we have our new parent
			pre_enabled = Enabled;
			pre_visible = Visible;
			pre_font = Font;
			pre_fore_color = ForeColor;
			pre_back_color = BackColor;
			pre_rtl = RightToLeft;
			// MS doesn't seem to send a CursorChangedEvent

			parent = new_parent;

			if (IsHandleCreated)
				XplatUI.SetParent(Handle,
						  (new_parent == null || !new_parent.IsHandleCreated) ? IntPtr.Zero : new_parent.Handle);

			OnParentChanged(EventArgs.Empty);

			if (pre_enabled != Enabled) {
				OnEnabledChanged(EventArgs.Empty);
			}

			if (pre_visible != Visible) {
				OnVisibleChanged(EventArgs.Empty);
			}

			if (pre_font != Font) {
				OnFontChanged(EventArgs.Empty);
			}

			if (pre_fore_color != ForeColor) {
				OnForeColorChanged(EventArgs.Empty);
			}

			if (pre_back_color != BackColor) {
				OnBackColorChanged(EventArgs.Empty);
			}

			if (pre_rtl != RightToLeft) {
				// MS sneaks a OnCreateControl and OnHandleCreated in here, I guess
				// because when RTL changes they have to recreate the win32 control
				// We don't really need that (until someone runs into compatibility issues)
				OnRightToLeftChanged(EventArgs.Empty);
			}

			if ((new_parent != null) && new_parent.Created && is_visible && !Created) {
				CreateControl();
			}

			if ((binding_context == null) && Created) {
				OnBindingContextChanged(EventArgs.Empty);
			}
		}

		private void UpdateDistances() {
			if (parent != null) {
				if (bounds.Width > 0)
					dist_right = parent.ClientSize.Width - bounds.X - bounds.Width;
				if (bounds.Height > 0)
					dist_bottom = parent.ClientSize.Height - bounds.Y - bounds.Height;
			}
		}
		
		private bool UseDoubleBuffering {
			get {
				if (!ThemeEngine.Current.DoubleBufferingSupported)
					return false;

#if NET_2_0
				if (DoubleBuffered)
					return true;
#endif
				return (control_style & ControlStyles.DoubleBuffer) != 0;
			}
		}
		#endregion	// Private & Internal Methods

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
				return Cursor.Position;
			}
		}
		
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO]
		public static bool CheckForIllegalCrossThreadCalls 
		{
			get {
				return verify_thread_handle;
			}

			set {
				verify_thread_handle = value;
			}
		}
#endif
		#endregion	// Public Static Properties

		#region Public Instance Properties
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AccessibleObject AccessibilityObject {
			get {
				if (accessibility_object==null) {
					accessibility_object=CreateAccessibilityInstance();
				}
				return accessibility_object;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string AccessibleDefaultActionDescription {
			get {
				if (accessibility_object != null)
					return accessibility_object.default_action;
				else
					return null;
			}

			set {
				if (accessibility_object != null)
					accessibility_object.default_action = value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Accessibility")]
		public string AccessibleDescription {
			get {
				if (accessibility_object != null)
					return accessibility_object.description;
				else
					return null;
			}

			set {
				if (accessibility_object != null)
					accessibility_object.description = value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Accessibility")]
		public string AccessibleName {
			get {
				if (accessibility_object != null)
					return accessibility_object.Name;
				else
					return null;
			}

			set {
				if (accessibility_object != null)
					accessibility_object.Name = value;
			}
		}

		[DefaultValue(AccessibleRole.Default)]
		[MWFDescription("Role of the control"), MWFCategory("Accessibility")]
		public AccessibleRole AccessibleRole {
			get {
				if (accessibility_object != null)
					return accessibility_object.role;
				else
					return AccessibleRole.Default;
			}

			set {
				if (accessibility_object != null)
					accessibility_object.role = value;
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public virtual bool AllowDrop {
			get {
				return allow_drop;
			}

			set {
				if (allow_drop == value)
					return;
				allow_drop = value;
				if (IsHandleCreated) {
					UpdateStyles();
					XplatUI.SetAllowDrop (Handle, value);
				}
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(AnchorStyles.Top | AnchorStyles.Left)]
		[MWFCategory("Layout")]
		public virtual AnchorStyles Anchor {
			get {
				return anchor_style;
			}

			set {
				layout_type = LayoutType.Anchor;

				if (anchor_style == value)
					return;
					
				anchor_style=value;
				dock_style = DockStyle.None;
				
				UpdateDistances ();

				if (parent != null)
					parent.PerformLayout(this, "Anchor");
			}
		}

#if NET_2_0
		// XXX: Implement me!
		bool auto_size;
		
		[RefreshProperties (RefreshProperties.All)]
		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (false)]
		[MonoTODO("This method currently does nothing")]
		public virtual bool AutoSize {
			get { return auto_size; }
			set {
				if (this.auto_size != value) {
					auto_size = value;
					OnAutoSizeChanged (EventArgs.Empty);
				}
			}
		}
		
#if NET_2_0
		[AmbientValue ("{Width=0, Height=0}")]
#else
		[AmbientValue (typeof(Size), "0, 0")]
#endif
		public virtual Size MaximumSize {
			get {
				return maximum_size;
			}
			set {
				if (maximum_size != value) {
					maximum_size = value;
					Size = PreferredSize;
				}
			}
		}

		public virtual Size MinimumSize {
			get {
				return minimum_size;
			}
			set {
				if (minimum_size != value) {
					minimum_size = value;
					Size = PreferredSize;
				}
			}
		}
#endif // NET_2_0

		[DispId(-501)]
		[MWFCategory("Appearance")]
		public virtual Color BackColor {
			get {
				if (background_color.IsEmpty) {
					if (parent!=null) {
						Color pcolor = parent.BackColor;
						if (pcolor.A == 0xff || GetStyle(ControlStyles.SupportsTransparentBackColor))
							return pcolor;
					}
					return DefaultBackColor;
				}
				return background_color;
			}

			set {
				if (!value.IsEmpty && (value.A != 0xff) && !GetStyle(ControlStyles.SupportsTransparentBackColor)) {
					throw new ArgumentException("Transparent background colors are not supported on this control");
				}

				if (background_color != value) {
					background_color=value;
					SetChildColor(this);
					OnBackColorChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Appearance")]
		public virtual Image BackgroundImage {
			get {
				return background_image;
			}

			set {
				if (background_image!=value) {
					background_image=value;
					OnBackgroundImageChanged(EventArgs.Empty);
					Invalidate ();
				}
			}
		}

#if NET_2_0
		[DefaultValue (ImageLayout.Tile)]
		[Localizable (true)]
		public virtual ImageLayout BackgroundImageLayout {
			get {
				return backgroundimage_layout;
			}
			set {
				if (Array.IndexOf (Enum.GetValues (typeof (ImageLayout)), value) == -1)
					throw new InvalidEnumArgumentException ("value", (int) value, typeof(ImageLayout));

				if (value != backgroundimage_layout) {
					backgroundimage_layout = value;
					Invalidate ();
					OnBackgroundImageLayoutChanged (EventArgs.Empty);
				}
				
			}
		} 
#endif
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual BindingContext BindingContext {
			get {
				if (binding_context != null)
					return binding_context;
				if (Parent == null)
					return null;
				binding_context = Parent.BindingContext;
				return binding_context;
			}
			set {
				if (binding_context != value) {
					binding_context = value;
					OnBindingContextChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Bottom {
			get {
				return bounds.Y+bounds.Height;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle Bounds {
			get {
				return this.bounds;
			}

			set {
				SetBounds(value.Left, value.Top, value.Width, value.Height, BoundsSpecified.All);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanFocus {
			get {
				if (IsHandleCreated && Visible && Enabled) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanSelect {
			get {
				Control	parent;

				if (!GetStyle(ControlStyles.Selectable)) {
					return false;
				}

				parent = this;
				while (parent != null) {
					if (!parent.is_visible || !parent.is_enabled) {
						return false;
					}

					parent = parent.parent;
				}
				return true;
			}
		}

		internal virtual bool InternalCapture {
			get {
				return Capture;
			}

			set {
				Capture = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Capture {
			get {
				return this.is_captured;
			}

			set {
				// Call OnMouseCaptureChanged when we get WM_CAPTURECHANGED.
				if (value != is_captured) {
					if (value) {
						is_captured = true;
						XplatUI.GrabWindow(Handle, IntPtr.Zero);
					} else {
						if (IsHandleCreated)
							XplatUI.UngrabWindow(Handle);
						is_captured = false;
					}
				}
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Focus")]
		public bool CausesValidation {
			get {
				return this.causes_validation;
			}

			set {
				if (this.causes_validation != value) {
					causes_validation = value;
					OnCausesValidationChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle ClientRectangle {
			get {
				client_rect.Width = client_size.Width;
				client_rect.Height = client_size.Height;
				return client_rect;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size ClientSize {
			get {
#if notneeded
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.ClientSize;
				}
#endif

				return client_size;
			}

			set {
				this.SetClientSizeCore(value.Width, value.Height);
#if NET_2_0
				this.OnClientSizeChanged (EventArgs.Empty);
#endif
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DescriptionAttribute("ControlCompanyNameDescr")]
		public String CompanyName {
			get {
				return "Mono Project, Novell, Inc.";
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ContainsFocus {
			get {
				IntPtr focused_window;

				focused_window = XplatUI.GetFocus();
				if (IsHandleCreated) {
					if (focused_window == Handle) {
						return true;
					}

					for (int i=0; i < child_controls.Count; i++) {
						if (child_controls[i].ContainsFocus) {
							return true;
						}
					}
				}
				return false;
			}
		}
#if NET_2_0
		[Browsable (false)]
#endif
		[DefaultValue(null)]
		[MWFCategory("Behavior")]
		public virtual ContextMenu ContextMenu {
			get {
				return GetContextMenuInternal ();
			}

			set {
				if (context_menu != value) {
					context_menu = value;
					OnContextMenuChanged(EventArgs.Empty);
				}
			}
		}

		internal virtual ContextMenu GetContextMenuInternal ()
		{
			return context_menu;
		}

#if NET_2_0
		[DefaultValue (null)]
		public virtual ContextMenuStrip ContextMenuStrip {
			get { return this.context_menu_strip; }
			set { 
				if (this.context_menu_strip != value) {
					this.context_menu_strip = value;
					OnContextMenuStripChanged (EventArgs.Empty);
				}
			}
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlCollection Controls {
			get {
				return this.child_controls;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Created {
			get {
				return (!is_disposed && is_created);
			}
		}

		[AmbientValue(null)]
		[MWFCategory("Appearance")]
		public virtual Cursor Cursor {
			get {
				if (cursor != null) {
					return cursor;
				}

				if (parent != null) {
					return parent.Cursor;
				}

				return Cursors.Default;
			}

			set {
				if (cursor != value) {
					Point pt;

					cursor = value;
					
					if (IsHandleCreated) {
						pt = Cursor.Position;

						if (bounds.Contains(pt) || Capture) {
							if (GetChildAtPoint(pt) == null) {
								if (cursor != null) {
									XplatUI.SetCursor(window.Handle, cursor.handle);
								} else {
									if (parent != null) {
										XplatUI.SetCursor(window.Handle, parent.Cursor.handle);
									} else {
										XplatUI.SetCursor(window.Handle, Cursors.Default.handle);
									}
								}
							}
						}
					}

					OnCursorChanged(EventArgs.Empty);
				}
			}
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[ParenthesizePropertyName(true)]
		[RefreshProperties(RefreshProperties.All)]
		[MWFCategory("Data")]
		public ControlBindingsCollection DataBindings {
			get {
				if (data_bindings == null)
					data_bindings = new ControlBindingsCollection (this);
				return data_bindings;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual Rectangle DisplayRectangle {
			get {
				return ClientRectangle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Disposing {
			get {
				return is_disposed;
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(DockStyle.None)]
		[MWFCategory("Layout")]
		public virtual DockStyle Dock {
			get {
				return dock_style;
			}

			set {
				layout_type = LayoutType.Dock;

				if (dock_style == value) {
					return;
				}

				if (!Enum.IsDefined (typeof (DockStyle), value)) {
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (DockStyle));
				}

				dock_style = value;
				anchor_style = AnchorStyles.Top | AnchorStyles.Left;

				if (dock_style == DockStyle.None) {
					if (explicit_bounds == Rectangle.Empty)
						Bounds = new Rectangle (new Point (0, 0), DefaultSize);
					else
						Bounds = explicit_bounds;
				}

				if (parent != null) {
					parent.PerformLayout(this, "Dock");
				}

				OnDockChanged(EventArgs.Empty);
			}
		}

#if NET_2_0
		protected virtual bool DoubleBuffered {
			get {
				return (control_style & ControlStyles.OptimizedDoubleBuffer) != 0;
			}

			set {
				if (value == DoubleBuffered)
					return;
				if (value) {
					SetStyle (ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
				} else {
					SetStyle (ControlStyles.OptimizedDoubleBuffer, false);
				}
				
			}
		}
#endif
		
		[DispId(-514)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool Enabled {
			get {
				if (!is_enabled) {
					return false;
				}

				if (parent != null) {
					return parent.Enabled;
				}

				return true;
			}

			set {
				if (this.is_enabled != value) {
					bool old_value = is_enabled;

					is_enabled = value;
					if (old_value != value && !value && this.has_focus)
						SelectNextControl(this, true, true, true, true);

					OnEnabledChanged (EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool Focused {
			get {
				return this.has_focus;
			}
		}

		[DispId(-512)]
		[AmbientValue(null)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
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

			[param:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(Font))]
			set {
				if (font != null && font.Equals (value)) {
					return;
				}

				font = value;
				Invalidate();
				OnFontChanged (EventArgs.Empty);
				PerformLayout ();
			}
		}

		[DispId(-513)]
		[MWFCategory("Appearance")]
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
				if (foreground_color != value) {
					foreground_color=value;
					Invalidate();
					OnForeColorChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-515)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr Handle {							// IWin32Window
			get {
#if NET_2_0
				if (verify_thread_handle) {
					if (this.InvokeRequired) {
						throw new InvalidOperationException("Cross-thread access of handle detected. Handle access only valid on thread that created the control");
					}
				}
#endif
				if (!IsHandleCreated) {
					CreateHandle();
				}
				return window.Handle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasChildren {
			get {
				if (this.child_controls.Count>0) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Height {
			get {
				return this.bounds.Height;
			}

			set {
				SetBounds(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
			}
		}

		[AmbientValue(ImeMode.Inherit)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public ImeMode ImeMode {
			get {
				if (ime_mode == ImeMode.Inherit) {
					if (parent != null)
						return parent.ImeMode;
					else
						return ImeMode.NoControl; // default value
				}
				return ime_mode;
			}

			set {
				if (ime_mode != value) {
					ime_mode = value;

					OnImeModeChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool InvokeRequired {						// ISynchronizeInvoke
			get {
				if (creator_thread != null && creator_thread!=Thread.CurrentThread) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsAccessible {
			get {
				return is_accessible;
			}

			set {
				is_accessible = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDisposed {
			get {
				return this.is_disposed;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsHandleCreated {
			get {
				if (window == null || window.Handle == IntPtr.Zero)
					return false;

				Hwnd hwnd = Hwnd.ObjectFromHandle (window.Handle);
				if (hwnd != null && hwnd.zombie)
					return false;

				return true;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#if NET_2_0
		public virtual
#endif
		Layout.LayoutEngine LayoutEngine {
			get {
				if (layout_engine == null)
					layout_engine = new Layout.DefaultLayout ();
				return layout_engine;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Left {
			get {
				return this.bounds.X;
			}

			set {
				SetBounds(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Point Location {
			get {
				return new Point(bounds.X, bounds.Y);
			}

			set {
				SetBounds(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
			}
		}

#if NET_2_0
		[Localizable (true)]
		public Padding Margin {
			get { return this.margin; }
			set { 
				if (this.margin != value) {
					this.margin = value; 
					OnMarginChanged (EventArgs.Empty);
				}
			}
		}
#endif

		[Browsable(false)]
		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}

#if NET_2_0
		[Localizable(true)]
		public Padding Padding {
			get {
				return padding;
			}

			set {
				if (padding != value) {
					padding = value;
					OnPaddingChanged (EventArgs.Empty);
					PerformLayout ();
				}
			}
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control Parent {
			get {
				return this.parent;
			}

			set {
				if (value == this) {
					throw new ArgumentException("A circular control reference has been made. A control cannot be owned or parented to itself.");
				}

				if (parent!=value) {
					if (value==null) {
						parent.Controls.Remove(this);
						parent = null;
						return;
					}

					value.Controls.Add(this);
				}
			}
		}

#if NET_2_0
		[Browsable (false)]
		public Size PreferredSize {
			get { return this.GetPreferredSize (Size.Empty); }
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductName {
			get {
				Type t = typeof (AssemblyProductAttribute);
				Assembly assembly = GetType().Module.Assembly;
				object [] attrs = assembly.GetCustomAttributes (t, false);
				AssemblyProductAttribute a = null;
				// On MS we get a NullRefException if product attribute is not
				// set. 
				if (attrs != null && attrs.Length > 0)
					a = (AssemblyProductAttribute) attrs [0];
				return a.Product;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductVersion {
			get {
				Type t = typeof (AssemblyVersionAttribute);
				Assembly assembly = GetType().Module.Assembly;
				object [] attrs = assembly.GetCustomAttributes (t, false);
				if (attrs == null || attrs.Length < 1)
					return "1.0.0.0";
				return ((AssemblyVersionAttribute)attrs [0]).Version;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool RecreatingHandle {
			get {
				return is_recreating;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Region Region {
			get {
				return clip_region;
			}

			set {
				if (clip_region != value) {
					if (value != null && IsHandleCreated)
						XplatUI.SetClipRegion(Handle, value);

					clip_region = value;
#if NET_2_0
					OnRegionChanged (EventArgs.Empty);
#endif
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Right {
			get {
				return this.bounds.X+this.bounds.Width;
			}
		}

		[AmbientValue(RightToLeft.Inherit)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public virtual RightToLeft RightToLeft {
			get {
				if (right_to_left == RightToLeft.Inherit) {
					if (parent != null)
						return parent.RightToLeft;
					else
						return RightToLeft.No; // default value
				}
				return right_to_left;
			}

			set {
				if (value != right_to_left) {
					right_to_left = value;
					OnRightToLeftChanged(EventArgs.Empty);
					PerformLayout ();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override ISite Site {
			get {
				return base.Site;
			}

			set {
				base.Site = value;

				if (value != null) {
					AmbientProperties ap = (AmbientProperties) value.GetService (typeof (AmbientProperties));
					if (ap != null) {
						BackColor = ap.BackColor;
						ForeColor = ap.ForeColor;
						Cursor = ap.Cursor;
						Font = ap.Font;
					}
				}
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Size Size {
			get {
				return new Size(Width, Height);
			}

			set {
				SetBounds(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
		}

		[Localizable(true)]
		[MergableProperty(false)]
		[MWFCategory("Behavior")]
		public int TabIndex {
			get {
				if (tab_index != -1) {
					return tab_index;
				}
				return 0;
			}

			set {
				if (tab_index != value) {
					tab_index = value;
					OnTabIndexChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-516)]
		[DefaultValue(true)]
		[MWFCategory("Behavior")]
		public bool TabStop {
			get {
				return tab_stop;
			}

			set {
				if (tab_stop != value) {
					tab_stop = value;
					OnTabStopChanged(EventArgs.Empty);
				}
			}
		}

		[Localizable(false)]
		[Bindable(true)]
		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[MWFCategory("Data")]
		public object Tag {
			get {
				return control_tag;
			}

			set {
				control_tag = value;
			}
		}

		[DispId(-517)]
		[Localizable(true)]
		[BindableAttribute(true)]
		[MWFCategory("Appearance")]
		public virtual string Text {
			get {
				// Our implementation ignores ControlStyles.CacheText - we always cache
				return this.text;
			}

			set {
				if (value == null) {
					value = String.Empty;
				}

				if (text!=value) {
					text=value;
					if (IsHandleCreated) {
						/* we need to call .SetWindowStyle here instead of just .Text
						   because the presence/absence of Text (== "" or not) can cause
						   other window style things to appear/disappear */
						XplatUI.SetWindowStyle(window.Handle, CreateParams);
						XplatUI.Text(Handle, text);
					}
					OnTextChanged (EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Top {
			get {
				return this.bounds.Y;
			}

			set {
				SetBounds(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control TopLevelControl {
			get {
				Control	p = this;

				while (p.parent != null) {
					p = p.parent;
				}

				return p is Form ? p : null;
			}
		}

		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool Visible {
			get {
				if (!is_visible) {
					return false;
				} else if (parent != null) {
					return parent.Visible;
				}

				return true;
			}

			set {
				SetVisibleCore(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Width {
			get {
				return this.bounds.Width;
			}

			set {
				SetBounds(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IWindowTarget WindowTarget {
			get {
				return null;
			}

			set {
				;	// MS Internal
			}
		}
		#endregion	// Public Instance Properties

		#region	Protected Instance Properties
		protected virtual CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				try {
					create_params.Caption = Text;
				}
				catch {
					create_params.Caption = text;
				}

				try {
					create_params.X = Left;
				}
				catch {
					create_params.X = this.bounds.X;
				}

				try {
					create_params.Y = Top;
				}
				catch {
					create_params.Y = this.bounds.Y;
				}

				try {
					create_params.Width = Width;
				}
				catch {
					create_params.Width = this.bounds.Width;
				}

				try {
					create_params.Height = Height;
				}
				catch {
					create_params.Height = this.bounds.Height;
				}


				create_params.ClassName = XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle = 0;
				create_params.Param = 0;

				if (allow_drop) {
					create_params.ExStyle |= (int)WindowExStyles.WS_EX_ACCEPTFILES;
				}

				if ((parent!=null) && (parent.IsHandleCreated)) {
					create_params.Parent = parent.Handle;
				}

				create_params.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;

				if (is_visible) {
					create_params.Style |= (int)WindowStyles.WS_VISIBLE;
				}

				if (!is_enabled) {
					create_params.Style |= (int)WindowStyles.WS_DISABLED;
				}

				switch (border_style) {
				case BorderStyle.FixedSingle:
					create_params.Style |= (int) WindowStyles.WS_BORDER;
					break;
				case BorderStyle.Fixed3D:
					create_params.ExStyle |= (int) WindowExStyles.WS_EX_CLIENTEDGE;
					break;
				}

				return create_params;
			}
		}

#if NET_2_0
		protected virtual Cursor DefaultCursor { get { return Cursors.Default; } }
#endif

		protected virtual ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

#if NET_2_0
		protected virtual Padding DefaultMargin {
			get { return new Padding (3); }
		}
		
		protected virtual Size DefaultMaximumSize { get { return new Size (); } }
		protected virtual Size DefaultMinimumSize { get { return new Size (); } }
		protected virtual Padding DefaultPadding { get { return new Padding (); } }
#endif

		protected virtual Size DefaultSize {
			get {
				return new Size(0, 0);
			}
		}

		protected int FontHeight {
			get {
				return Font.Height;
			}

			set {
				;; // Nothing to do
			}
		}
#if NET_2_0
		[Obsolete ()]
#endif
		protected bool RenderRightToLeft {
			get {
				return (this.right_to_left == RightToLeft.Yes);
			}
		}

		protected bool ResizeRedraw {
			get {
				return GetStyle(ControlStyles.ResizeRedraw);
			}

			set {
				SetStyle(ControlStyles.ResizeRedraw, value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual bool ShowFocusCues {
			get {
				return true;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		internal virtual
#endif
		protected bool ShowKeyboardCues {
			get {
				return true;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromChildHandle(IntPtr handle) {
			return Control.ControlNativeWindow.ControlFromChildHandle (handle);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromHandle(IntPtr handle) {
			return Control.ControlNativeWindow.ControlFromHandle(handle);
		}

		public static bool IsMnemonic(char charCode, string text) {
			int amp;

			amp = text.IndexOf('&');

			if (amp != -1) {
				if (amp + 1 < text.Length) {
					if (text[amp + 1] != '&') {
						if (Char.ToUpper(charCode) == Char.ToUpper(text.ToCharArray(amp + 1, 1)[0])) {
							return true;
						}	
					}
				}
			}
			return false;
		}
		#endregion

		#region Protected Static Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static bool ReflectMessage(IntPtr hWnd, ref Message m) {
			Control	c;

			c = Control.FromHandle(hWnd);

			if (c != null) {
				c.WndProc(ref m);
				return true;
			}
			return false;
		}
		#endregion

		#region	Public Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInvoke(Delegate method) {
			object [] prms = null;
			if (method is EventHandler)
				prms = new object [] { this, EventArgs.Empty };
			return BeginInvokeInternal(method, prms, false);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInvoke (Delegate method, object[] args) {
			return BeginInvokeInternal (method, args, false);
		}

		public void BringToFront() {
			if (parent != null) {
				parent.child_controls.SetChildIndex(this, 0);
				parent.Refresh();
			}
			else if (IsHandleCreated) {
				XplatUI.SetZOrder(Handle, IntPtr.Zero, false, false);
			}
		}

		public bool Contains(Control ctl) {
			while (ctl != null) {
				ctl = ctl.parent;
				if (ctl == this) {
					return true;
				}
			}
			return false;
		}

		public void CreateControl ()
		{
			if (is_disposed) {
				throw new ObjectDisposedException(GetType().FullName.ToString());
			}
			if (is_created) {
				return;
			}

			if (!IsHandleCreated) {
				CreateHandle();
			}

			if (!is_created) {
				is_created = true;
			}

			if (binding_context == null) {	// seem to be sent whenever it's null?
				OnBindingContextChanged(EventArgs.Empty);
			}

			OnCreateControl();
		}

		public Graphics CreateGraphics() {
			if (!IsHandleCreated) {
				this.CreateHandle();
			}
			return Graphics.FromHwnd(this.window.Handle);
		}

		public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects) {
			if (IsHandleCreated)
				return XplatUI.StartDrag(Handle, data, allowedEffects);
			else
				return DragDropEffects.None;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public object EndInvoke (IAsyncResult async_result) {
			AsyncMethodResult result = (AsyncMethodResult) async_result;
			return result.EndInvoke ();
		}

		public Form FindForm() {
			Control	c;

			c = this;
			while (c != null) {
				if (c is Form) {
					return (Form)c;
				}
				c = c.Parent;
			}
			return null;
		}
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public bool Focus() {
			if (CanFocus && IsHandleCreated && !has_focus && !is_focusing) {
				is_focusing = true;
				Select(this);
				is_focusing = false;
			}
			return has_focus;
		}

		internal void FocusInternal ()
		{
			is_focusing = true;
			Select(this);
			is_focusing = false;
		}

		public Control GetChildAtPoint(Point pt) {
			// MS's version causes the handle to be created.  The stack trace shows that get_Handle is called here, but
			// we'll just call CreateHandle instead.
			CreateHandle ();
			
			// Microsoft's version of this function doesn't seem to work, so I can't check
			// if we only consider children or also grandchildren, etc.
			// I'm gonna say 'children only'
			for (int i=0; i<child_controls.Count; i++) {
				if (child_controls[i].Bounds.Contains(pt)) {
					return child_controls[i];
				}
			}
			return null;
		}

		public IContainerControl GetContainerControl() {
			Control	current = this;

			while (current!=null) {
				if ((current is IContainerControl) && ((current.control_style & ControlStyles.ContainerControl)!=0)) {
					return (IContainerControl)current;
				}
				current = current.parent;
			}
			return null;
		}

		public Control GetNextControl(Control ctl, bool forward) {

			if (!this.Contains(ctl)) {
				ctl = this;
			}

			if (forward) {
				ctl = FindControlForward(this, ctl);
			}
			else {
				ctl = FindControlBackward(this, ctl);
			}

			if (ctl != this) {
				return ctl;
			}
			return null;
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual Size GetPreferredSize (Size proposedSize) {
			Size retsize = this.explicit_bounds.Size;
			
			// If we're bigger than the MaximumSize, fix that
			if (this.maximum_size.Width != 0 && retsize.Width > this.maximum_size.Width)
				retsize.Width = this.maximum_size.Width;
			if (this.maximum_size.Height != 0 && retsize.Height > this.maximum_size.Height)
				retsize.Height = this.maximum_size.Height;
				
			// If we're smaller than the MinimumSize, fix that
			if (this.minimum_size.Width != 0 && retsize.Width < this.minimum_size.Width)
				retsize.Width = this.minimum_size.Width;
			if (this.minimum_size.Height != 0 && retsize.Height < this.minimum_size.Height)
				retsize.Height = this.minimum_size.Height;
				
			return retsize;
		}
#endif

		public void Hide() {
			this.Visible = false;
		}

		public void Invalidate() {
			Invalidate(ClientRectangle, false);
		}

		public void Invalidate(bool invalidateChildren) {
			Invalidate(ClientRectangle, invalidateChildren);
		}

		public void Invalidate(System.Drawing.Rectangle rc) {
			Invalidate(rc, false);
		}

		public void Invalidate(System.Drawing.Rectangle rc, bool invalidateChildren) {
			// Win32 invalidates control including when Width and Height is equal 0
			// or is not visible, only Paint event must be care about this.
			if (!IsHandleCreated)
				return;

			if  (rc.Width > 0 && rc.Height > 0) {

				NotifyInvalidate(rc);

				XplatUI.Invalidate(Handle, rc, false);

				if (invalidateChildren) {
					Control [] controls = child_controls.GetAllControls ();
					for (int i=0; i<controls.Length; i++)
						controls [i].Invalidate ();
				}
			}
			OnInvalidated(new InvalidateEventArgs(rc));
		}

		public void Invalidate(System.Drawing.Region region) {
			Invalidate(region, false);
		}

		public void Invalidate(System.Drawing.Region region, bool invalidateChildren) {
			RectangleF bounds = region.GetBounds (CreateGraphics ());
			Invalidate (new Rectangle ((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height),
					invalidateChildren);
		}

		public object Invoke (Delegate method) {
			object [] prms = null;
			if (method is EventHandler)
				prms = new object [] { this, EventArgs.Empty };

			return Invoke(method, prms);
		}

		public object Invoke (Delegate method, object[] args) {
			Control p = this;
			do {
				if (!p.IsHandleCreated)
					throw new InvalidOperationException("Cannot call Invoke or BeginInvoke on a control until the window handle is created");
				p = p.parent;
			} while (p != null);
			
			if (!this.InvokeRequired) {
				return method.DynamicInvoke(args);
			}

			IAsyncResult result = BeginInvoke (method, args);
			return EndInvoke(result);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout() {
			PerformLayout(null, null);
		}

		internal void SetImplicitBounds (int x, int y, int width, int height)
		{
			Rectangle saved_bounds = explicit_bounds;
			SetBounds (x, y, width, height);
			explicit_bounds = saved_bounds;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout(Control affectedControl, string affectedProperty) {
			LayoutEventArgs levent = new LayoutEventArgs(affectedControl, affectedProperty);

			if (layout_suspended > 0) {
				layout_pending = true;
				return;
			}

			layout_pending = false;

			// Prevent us from getting messed up
			layout_suspended++;

			// Perform all Dock and Anchor calculations
			try {
				OnLayout(levent);
			}

				// Need to make sure we decremend layout_suspended
			finally {
				layout_suspended--;
			}
		}

		public Point PointToClient (Point p) {
			int x = p.X;
			int y = p.Y;

			XplatUI.ScreenToClient (Handle, ref x, ref y);

			return new Point (x, y);
		}

		public Point PointToScreen(Point p) {
			int x = p.X;
			int y = p.Y;

			XplatUI.ClientToScreen(Handle, ref x, ref y);

			return new Point(x, y);
		}

		public virtual bool PreProcessMessage(ref Message msg) {
			return InternalPreProcessMessage (ref msg);
		}

		internal virtual bool InternalPreProcessMessage (ref Message msg) {
			Keys key_data;

			if ((msg.Msg == (int)Msg.WM_KEYDOWN) || (msg.Msg == (int)Msg.WM_SYSKEYDOWN)) {
				key_data = (Keys)msg.WParam.ToInt32() | XplatUI.State.ModifierKeys;

				if (!ProcessCmdKey(ref msg, key_data)) {
					if (IsInputKey(key_data)) {
						return false;
					}

					return ProcessDialogKey(key_data);
				}

				return true;
			} else if (msg.Msg == (int)Msg.WM_CHAR) {
				if (IsInputChar((char)msg.WParam)) {
					return false;
				}
				return ProcessDialogChar((char)msg.WParam);
			} else if (msg.Msg == (int)Msg.WM_SYSCHAR) {
				return ProcessDialogChar((char)msg.WParam);
			}
			return false;
		}

		public Rectangle RectangleToClient(Rectangle r) {
			return new Rectangle(PointToClient(r.Location), r.Size);
		}

		public Rectangle RectangleToScreen(Rectangle r) {
			return new Rectangle(PointToScreen(r.Location), r.Size);
		}

		public virtual void Refresh() {
			if (IsHandleCreated) {
				Invalidate();
				XplatUI.UpdateWindow(window.Handle);

				Control [] controls = child_controls.GetAllControls ();
				for (int i=0; i < controls.Length; i++) {
					controls[i].Refresh();
				}
				
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetBackColor() {
			BackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBindings() {
			if (data_bindings != null)
				data_bindings.Clear();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetCursor() {
			Cursor = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetFont() {
			font = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetForeColor() {
			foreground_color = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImeMode() {
			ime_mode = DefaultImeMode;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetRightToLeft() {
			right_to_left = RightToLeft.Inherit;
		}

		public virtual void ResetText() {
			text = String.Empty;
		}

		public void ResumeLayout() {
			ResumeLayout (true);
		}

		public void ResumeLayout(bool performLayout) {
			if (layout_suspended > 0) {
				layout_suspended--;
			}

			if (layout_suspended == 0) {
				if (performLayout && layout_pending) {
					PerformLayout();
				}
			}
		}
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ()]
#endif
		public void Scale(float ratio) {
			ScaleCore(ratio, ratio);
		}
		
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ()]
#endif
		public void Scale(float dx, float dy) {
			ScaleCore(dx, dy);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void Scale(SizeF factor) {
			ScaleCore(factor.Width, factor.Height);
		}
#endif

		public void Select() {
			Select(false, false);	
		}

#if DebugFocus
		private void printTree(Control c, string t) {
			foreach(Control i in c.child_controls) {
				Console.WriteLine ("{2}{0}.TabIndex={1}", i, i.tab_index, t);
				printTree (i, t+"\t");
			}
		}
#endif
		public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap) {
			Control c;

#if DebugFocus
			Console.WriteLine("{0}", this.FindForm());
			printTree(this, "\t");
#endif

			if (!this.Contains(ctl) || (!nested && (ctl.parent != this))) {
				ctl = null;
			}
			c = ctl;
			do {
				c = GetNextControl(c, forward);
				if (c == null) {
					if (wrap) {
						wrap = false;
						continue;
					}
					break;
				}

				if (c.CanSelect && ((c.parent == this) || nested) && (c.tab_stop || !tabStopOnly)) {
					c.Select (true, true);
					return true;
				}
			} while (c != ctl); // If we wrap back to ourselves we stop

			return false;
		}

		public void SendToBack() {
			if (parent != null) {
				parent.child_controls.SetChildIndex(this, parent.child_controls.Count);
			}
		}

		public void SetBounds(int x, int y, int width, int height) {
			SetBounds(x, y, width, height, BoundsSpecified.All);
		}

		public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified) {
			if ((specified & BoundsSpecified.X) != BoundsSpecified.X) {
				x = Left;
			}

			if ((specified & BoundsSpecified.Y) != BoundsSpecified.Y) {
				y = Top;
			}

			if ((specified & BoundsSpecified.Width) != BoundsSpecified.Width) {
				width = Width;
			}

			if ((specified & BoundsSpecified.Height) != BoundsSpecified.Height) {
				height = Height;
			}

			SetBoundsCore(x, y, width, height, specified);
			if (parent != null)
				parent.PerformLayout(this, "Bounds");
		}

		public void Show ()
		{
			this.Visible = true;
		}

		public void SuspendLayout() {
			layout_suspended++;
		}

		public void Update() {
			if (IsHandleCreated) {
				XplatUI.UpdateWindow(window.Handle);
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void AccessibilityNotifyClients(AccessibleEvents accEvent, int childID) {
			if (accessibility_object != null && accessibility_object is ControlAccessibleObject)
				((ControlAccessibleObject)accessibility_object).NotifyClients (accEvent, childID);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual AccessibleObject CreateAccessibilityInstance() {
			return new Control.ControlAccessibleObject(this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual ControlCollection CreateControlsInstance() {
			return new ControlCollection(this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void CreateHandle() {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().FullName.ToString());
			}

			if (IsHandleCreated && !is_recreating) {
				return;
			}

			window.CreateHandle(CreateParams);

			if (window.Handle != IntPtr.Zero) {
				creator_thread = Thread.CurrentThread;

				XplatUI.EnableWindow(window.Handle, is_enabled);
				XplatUI.SetVisible(window.Handle, is_visible, true);

				if (clip_region != null) {
					XplatUI.SetClipRegion(window.Handle, clip_region);
				}

				// Set our handle with our parent
				if ((parent != null) && (parent.IsHandleCreated)) {
					XplatUI.SetParent(window.Handle, parent.Handle);
				}

				// Set our handle as parent for our children
				Control [] children;

				children = child_controls.GetAllControls ();
				for (int i = 0; i < children.Length; i++ ) {
					if (!children[i].RecreatingHandle && children[i].IsHandleCreated)
						XplatUI.SetParent(children[i].Handle, window.Handle); 
				}

				UpdateStyles();
				XplatUI.SetAllowDrop (window.Handle, allow_drop);

				// Find out where the window manager placed us
				if ((CreateParams.Style & (int)WindowStyles.WS_CHILD) != 0) {
					XplatUI.SetBorderStyle(window.Handle, (FormBorderStyle)border_style);
				}
				UpdateBounds();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DefWndProc(ref Message m) {
			window.DefWndProc(ref m);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DestroyHandle() {
			if (IsHandleCreated) {
				if (window != null) {
					window.DestroyHandle();
				}
			}
		}

#if NET_2_0
		protected virtual AccessibleObject GetAccessibilityObjectById (int objectId)
		{
			// XXX need to implement this.
			return null;
		}
#endif

		protected internal bool GetStyle(ControlStyles flag) {
			return (control_style & flag) != 0;
		}

		protected bool GetTopLevel() {
			return is_toplevel;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void InitLayout() {
			UpdateDistances();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeGotFocus(Control toInvoke, EventArgs e) {
			toInvoke.OnGotFocus(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeLostFocus(Control toInvoke, EventArgs e) {
			toInvoke.OnLostFocus(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeOnClick(Control toInvoke, EventArgs e) {
			toInvoke.OnClick(e);
		}

		protected void InvokePaint(Control toInvoke, PaintEventArgs e) {
			toInvoke.OnPaint(e);
		}

		protected void InvokePaintBackground(Control toInvoke, PaintEventArgs e) {
			toInvoke.OnPaintBackground(e);
		}

		protected virtual bool IsInputChar (char charCode) {
			// XXX on MS.NET this method causes the handle to be created..
			CreateHandle ();

			return true;
		}

		protected virtual bool IsInputKey (Keys keyData) {
			// Doc says this one calls IsInputChar; not sure what to do with that
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void NotifyInvalidate(Rectangle invalidatedArea) {
			// override me?
		}

		protected virtual bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if ((context_menu != null) && context_menu.ProcessCmdKey(ref msg, keyData)) {
				return true;
			}

			if (parent != null) {
				return parent.ProcessCmdKey(ref msg, keyData);
			}

			return false;
		}

		protected virtual bool ProcessDialogChar(char charCode) {
			if (parent != null) {
				return parent.ProcessDialogChar (charCode);
			}

			return false;
		}

		protected virtual bool ProcessDialogKey (Keys keyData) {
			if (parent != null) {
				return parent.ProcessDialogKey (keyData);
			}

			return false;
		}

		protected virtual bool ProcessKeyEventArgs (ref Message msg)
		{
			KeyEventArgs		key_event;

			switch (msg.Msg) {
				case (int)Msg.WM_SYSKEYDOWN:
				case (int)Msg.WM_KEYDOWN: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyDown (key_event);
					return key_event.Handled;
				}

				case (int)Msg.WM_SYSKEYUP:
				case (int)Msg.WM_KEYUP: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyUp (key_event);
					return key_event.Handled;
				}

				case (int)Msg.WM_SYSCHAR:
				case (int)Msg.WM_CHAR: {
					KeyPressEventArgs	key_press_event;

					key_press_event = new KeyPressEventArgs((char)msg.WParam);
					OnKeyPress(key_press_event);
#if NET_2_0
					msg.WParam = (IntPtr)key_press_event.KeyChar;
#endif
					return key_press_event.Handled;
				}

				default: {
					break;
				}
			}

			return false;
		}

		protected internal virtual bool ProcessKeyMessage(ref Message msg) {
			if (parent != null) {
				if (parent.ProcessKeyPreview(ref msg)) {
					return true;
				}
			}

			return ProcessKeyEventArgs(ref msg);
		}

		protected virtual bool ProcessKeyPreview(ref Message msg) {
			if (parent != null) {
				return parent.ProcessKeyPreview(ref msg);
			}

			return false;
		}

		protected virtual bool ProcessMnemonic(char charCode) {
			// override me
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseDragEvent(object key, DragEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseKeyEvent(object key, KeyEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseMouseEvent(object key, MouseEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaisePaintEvent(object key, PaintEventArgs e) {
			// MS Internal
		}

		private void SetIsRecreating ()
		{
			is_recreating=true;

			foreach (Control c in Controls.GetAllControls()) {
				c.SetIsRecreating ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RecreateHandle() {
			if (!IsHandleCreated)
				return;

#if DebugRecreate
			Console.WriteLine("Recreating control {0}", XplatUI.Window(window.Handle));
#endif

			SetIsRecreating ();

			if (IsHandleCreated) {
#if DebugRecreate
				Console.WriteLine(" + handle is created, destroying it.");
#endif
				DestroyHandle();
				// WM_DESTROY will CreateHandle for us
			} else {
#if DebugRecreate
				Console.WriteLine(" + handle is not created, creating it.");
#endif
				if (!is_created) {
					CreateControl();
				} else {
					CreateHandle();
				}

				is_recreating = false;
#if DebugRecreate
				Console.WriteLine (" + new handle = {0:X}", Handle.ToInt32());
#endif
			}

		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void ResetMouseEventArgs() {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected ContentAlignment RtlTranslateAlignment(ContentAlignment align) {
			if (right_to_left == RightToLeft.No) {
				return align;
			}

			switch (align) {
				case ContentAlignment.TopLeft: {
					return ContentAlignment.TopRight;
				}

				case ContentAlignment.TopRight: {
					return ContentAlignment.TopLeft;
				}

				case ContentAlignment.MiddleLeft: {
					return ContentAlignment.MiddleRight;
				}

				case ContentAlignment.MiddleRight: {
					return ContentAlignment.MiddleLeft;
				}

				case ContentAlignment.BottomLeft: {
					return ContentAlignment.BottomRight;
				}

				case ContentAlignment.BottomRight: {
					return ContentAlignment.BottomLeft;
				}

				default: {
					// if it's center it doesn't change
					return align;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align) {
			if ((right_to_left == RightToLeft.No) || (align == HorizontalAlignment.Center)) {
				return align;
			}

			if (align == HorizontalAlignment.Left) {
				return HorizontalAlignment.Right;
			}

			// align must be HorizontalAlignment.Right
			return HorizontalAlignment.Left;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align) {
			if (right_to_left == RightToLeft.No) {
				return align;
			}

			if (align == LeftRightAlignment.Left) {
				return LeftRightAlignment.Right;
			}

			// align must be LeftRightAlignment.Right;
			return LeftRightAlignment.Left;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected ContentAlignment RtlTranslateContent(ContentAlignment align) {
			return RtlTranslateAlignment(align);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected HorizontalAlignment RtlTranslateHorizontal(HorizontalAlignment align) {
			return RtlTranslateAlignment(align);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected LeftRightAlignment RtlTranslateLeftRight(LeftRightAlignment align) {
			return RtlTranslateAlignment(align);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#else
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		protected virtual void ScaleCore(float dx, float dy) {
			Point	location;
			Size	size;

			SuspendLayout();

			location = new Point((int)(Left * dx), (int)(Top * dy));
			size = this.ClientSize;

			if (!GetStyle(ControlStyles.FixedWidth)) {
				size.Width = (int)(size.Width * dx);
			}

			if (!GetStyle(ControlStyles.FixedHeight)) {
				size.Height = (int)(size.Height * dy);
			}

			SetBounds(location.X, location.Y, size.Width, size.Height, BoundsSpecified.All);

			/* Now scale our children */
			Control [] controls = child_controls.GetAllControls ();
			for (int i=0; i < controls.Length; i++) {
				controls[i].Scale(dx, dy);
			}

			ResumeLayout();
		}

		protected virtual void Select(bool directed, bool forward) {
			IContainerControl	container;
			
			container = GetContainerControl();
			if (container != null && (Control)container != this)
			    container.ActiveControl = this;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// SetBoundsCore updates the Win32 control itself. UpdateBounds updates the controls variables and fires events, I'm guessing - pdb
			if (IsHandleCreated) {
				XplatUI.SetWindowPos(Handle, x, y, width, height);

				// Win32 automatically changes negative width/height to 0.
				// The control has already been sent a WM_WINDOWPOSCHANGED message and it has the correct
				// data, but it'll be overwritten when we call UpdateBounds unless we get the updated
				// size.
				if (width < 0 || height < 0) {
					int cw, ch, ix, iy;
					XplatUI.GetWindowPos(Handle, this is Form, out ix, out iy, out width, out height, out cw, out ch);
				}
			}

			UpdateBounds(x, y, width, height);

			UpdateDistances();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetClientSizeCore(int x, int y) {
			Size NewSize = SizeFromClientSize (new Size (x, y));
			
			if (NewSize != Size.Empty)
				SetBounds (bounds.X, bounds.Y, NewSize.Width, NewSize.Height, BoundsSpecified.Size);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal void SetStyle(ControlStyles flag, bool value) {
			if (value) {
				control_style |= flag;
			} else {
				control_style &= ~flag;
			}
		}

		protected void SetTopLevel(bool value) {
			if ((GetTopLevel() != value) && (parent != null)) {
				throw new ArgumentException ("Cannot change toplevel style of a parented control.");
			}

			// XXX MS.NET causes handle to be created here
			CreateHandle ();

			if (this is Form) {
				if (value == true) {
					if (!Visible) {
						Visible = true;
					}
				} else {
					if (Visible) {
                                               Visible = false;
					}
				}
			}
			is_toplevel = value;
		}

		protected virtual void SetVisibleCore(bool value) {
			if (value != is_visible) {
				is_visible = value;
				
				if (value && ((window.Handle == IntPtr.Zero) || !is_created)) {
					CreateControl();
				}

				if (IsHandleCreated) {
					XplatUI.SetVisible(Handle, value, true);
					// Explicitly move Toplevel windows to where we want them;
					// apparently moving unmapped toplevel windows doesn't work
					if (is_visible && (this is Form)) {
						XplatUI.SetWindowPos(window.Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
					}
				}
				else {
					OnVisibleChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
#if NET_2_0
		protected 
#else
		internal
#endif
		virtual Size SizeFromClientSize (Size clientSize)
		{
			Rectangle ClientRect;
			Rectangle WindowRect;
			CreateParams cp;

			ClientRect = new Rectangle (0, 0, clientSize.Width, clientSize.Height);
			cp = this.CreateParams;

			if (XplatUI.CalculateWindowRect (ref ClientRect, cp.Style, cp.ExStyle, null, out WindowRect))
				return new Size (WindowRect.Width, WindowRect.Height);
				
			return Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds() {
			if (!IsHandleCreated)
				return;

			int	x;
			int	y;
			int	width;
			int	height;
			int	client_width;
			int	client_height;

			XplatUI.GetWindowPos(this.Handle, this is Form, out x, out y, out width, out height, out client_width, out client_height);

			UpdateBounds(x, y, width, height, client_width, client_height);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height) {
			CreateParams	cp;
			Rectangle	rect;

			// Calculate client rectangle
			rect = new Rectangle(0, 0, 0, 0);
			cp = CreateParams;

			XplatUI.CalculateWindowRect(ref rect, cp.Style, cp.ExStyle, cp.menu, out rect);
			UpdateBounds(x, y, width, height, width - (rect.Right - rect.Left), height - (rect.Bottom - rect.Top));
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight) {
			// UpdateBounds only seems to set our sizes and fire events but not update the GUI window to match
			bool	moved	= false;
			bool	resized	= false;

			// Needed to generate required notifications
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

			// Assume explicit bounds set. SetImplicitBounds will restore old bounds
			explicit_bounds = bounds;

			client_size.Width=clientWidth;
			client_size.Height=clientHeight;

			if (moved) {
				OnLocationChanged(EventArgs.Empty);

			if (!background_color.IsEmpty && background_color.A < byte.MaxValue)
					Invalidate ();
			}

			if (resized) {
				OnSizeChanged(EventArgs.Empty);
#if NET_2_0
				OnClientSizeChanged (EventArgs.Empty);
#endif
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateStyles() {
			if (!IsHandleCreated) {
				return;
			}

			XplatUI.SetWindowStyle(window.Handle, CreateParams);
			OnStyleChanged(EventArgs.Empty);
		}

		private void UpdateZOrderOfChild(Control child) {
			if (IsHandleCreated && child.IsHandleCreated && (child.parent == this)) {
				int	index;

				index = child_controls.IndexOf(child);

				if (index > 0) {
					XplatUI.SetZOrder(child.Handle, child_controls[index - 1].Handle, false, false);
				} else {
					IntPtr after = AfterTopMostControl ();
					if (after != IntPtr.Zero)
						XplatUI.SetZOrder (child.Handle, after, false, false);
					else
						XplatUI.SetZOrder (child.Handle, IntPtr.Zero, true, false);
				}
			}
		}
		
		// Override this if there is a control that shall always remain on
		// top of other controls (such as scrollbars). If there are several
		// of these controls, the bottom-most should be returned.
		internal virtual IntPtr AfterTopMostControl ()
		{
			return IntPtr.Zero;
		}

		private void UpdateChildrenZOrder() {
			Control [] controls;

			if (!IsHandleCreated) {
				return;
			}

			controls = child_controls.GetAllControls ();
			for (int i = 1; i < controls.Length; i++ ) {
				XplatUI.SetZOrder(controls[i].Handle, controls[i-1].Handle, false, false);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateZOrder() {
			if (parent != null) {
				parent.UpdateZOrderOfChild(this);
			}
		}

		protected virtual void WndProc(ref Message m) {
#if debug
			Console.WriteLine("Control {0} received message {1}", window.Handle == IntPtr.Zero ? this.Text : XplatUI.Window(window.Handle), m.ToString ());
#endif
			if ((this.control_style & ControlStyles.EnableNotifyMessage) != 0) {
				OnNotifyMessage(m);
			}

			switch((Msg)m.Msg) {
			case Msg.WM_DESTROY: {
				OnHandleDestroyed(EventArgs.Empty);
#if DebugRecreate
				IntPtr handle = window.Handle;
#endif
				window.InvalidateHandle();

				if (is_recreating) {
#if DebugRecreate
					Console.WriteLine ("Creating handle for {0:X}", handle.ToInt32());
#endif
					CreateHandle();
#if DebugRecreate
					Console.WriteLine (" + new handle = {0:X}", Handle.ToInt32());
#endif
					is_recreating = false;
				}
				return;
			}

			case Msg.WM_WINDOWPOSCHANGED: {
				if (Visible) {
					Rectangle save_bounds = explicit_bounds;
					UpdateBounds();
					explicit_bounds = save_bounds;
					if (GetStyle(ControlStyles.ResizeRedraw)) {
						Invalidate();
					}
				}
				return;
			}

			// Nice description of what should happen when handling WM_PAINT
			// can be found here: http://pluralsight.com/wiki/default.aspx/Craig/FlickerFreeControlDrawing.html
			// and here http://msdn.microsoft.com/msdnmag/issues/06/03/WindowsFormsPerformance/
			case Msg.WM_PAINT: {
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart(Handle, true);

				if (paint_event == null) {
					return;
				}
				DoubleBuffer current_buffer = null;
				if (UseDoubleBuffering) {
					current_buffer = GetBackBuffer ();
					if (!current_buffer.InvalidRegion.IsVisible (paint_event.ClipRectangle)) {
						// Just blit the previous image
						current_buffer.Blit (paint_event);
						XplatUI.PaintEventEnd (Handle, true);
						return;
					}
					current_buffer.Start (paint_event);
				}
				
				if (!GetStyle(ControlStyles.Opaque)) {
					OnPaintBackground(paint_event);
				}

				// Button-derived controls choose to ignore their Opaque style, give them a chance to draw their background anyways
				OnPaintBackgroundInternal(paint_event);

				OnPaintInternal(paint_event);
				if (!paint_event.Handled) {
					OnPaint(paint_event);
				}

				if (current_buffer != null) {
					current_buffer.End (paint_event);
				}


				XplatUI.PaintEventEnd(Handle, true);

				return;
			}

			case Msg.WM_SHOWWINDOW: {
				OnVisibleChanged(EventArgs.Empty);

				if (m.WParam.ToInt32() != 0) {
					/* if we're being shown, make sure our child controls all have their handles created */
					Control [] controls = child_controls.GetAllControls ();
					for (int i=0; i<controls.Length; i++) {
						if (controls [i].is_visible) {
							controls [i].CreateControl ();
						}
					}

					UpdateChildrenZOrder ();
				}
				else {
					if (parent != null && Focused) {
						Control	container;

						// Need to start at parent, GetContainerControl might return ourselves if we're a container
						container = (Control)parent.GetContainerControl();
						if (container != null) {
							container.SelectNextControl(this, true, true, true, true);
						}
					}
				}

				if (parent != null) {
					parent.PerformLayout(this, "visible");
				} else {
					if (is_visible)
						PerformLayout(this, "visible");
				}

				break;
			}

			case Msg.WM_CREATE: {
				OnHandleCreated(EventArgs.Empty);
				break;
			}

			case Msg.WM_ERASEBKGND: {
				// The DefWndProc will never have to handle this, we always paint the background in managed code
				// In theory this code would look at ControlStyles.AllPaintingInWmPaint and and call OnPaintBackground
				// here but it just makes things more complicated...
				m.Result = (IntPtr)1;
				return;
			}

			case Msg.WM_LBUTTONUP: {
				MouseEventArgs me;

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Left, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);

				if (InternalCapture) {
					InternalCapture = false;
				}

				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_LBUTTONDOWN: {
				if (CanSelect) {
					Select (true, true);
				}
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				
				return;
			}

			case Msg.WM_LBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_MBUTTONUP: {
				MouseEventArgs me;

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Middle, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);
				if (InternalCapture) {
					InternalCapture = false;
				}
				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_MBUTTONDOWN: {					
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_MBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_RBUTTONUP: {
				MouseEventArgs	me;
				Point		pt;

				pt = new Point(LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));
				pt = PointToScreen(pt);

				XplatUI.SendMessage(m.HWnd, Msg.WM_CONTEXTMENU, m.HWnd, (IntPtr)(pt.X + (pt.Y << 16)));

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Right, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);

				if (InternalCapture) {
					InternalCapture = false;
				}

				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_RBUTTONDOWN: {					
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_RBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_CONTEXTMENU: {
				if (context_menu != null) {
					Point	pt;

					pt = new Point(LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));

					if (pt.X == -1 || pt.Y == -1) {
						pt.X = (this.Width / 2) + this.Left;
						pt.Y = (this.Height / 2) + this.Top;
						pt = this.PointToScreen (pt);
					}
					
					context_menu.Show (this, PointToClient (pt));
					return;
				}

#if NET_2_0
				// If there isn't a regular context menu, show the Strip version
				if (context_menu == null && context_menu_strip != null) {
					Point pt;

					pt = new Point (LowOrder ((int)m.LParam.ToInt32 ()), HighOrder ((int)m.LParam.ToInt32 ()));
					
					if (pt.X == -1 || pt.Y == -1) { 
						pt.X = (this.Width / 2) + this.Left; 
						pt.Y = (this.Height /2) + this.Top; 
						pt = this.PointToScreen (pt);
					}
					
					context_menu_strip.Show (this, PointToClient (pt));
					return;
				}
#endif
				DefWndProc(ref m);
				return;
			}

			case Msg.WM_MOUSEWHEEL: {				
				DefWndProc(ref m);
				OnMouseWheel (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								  mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								  HighOrder(m.WParam.ToInt32())));
				return;
			}


			case Msg.WM_MOUSEMOVE: {					
				OnMouseMove  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								  mouse_clicks, 
								  LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								  0));
				return;
			}

			case Msg.WM_MOUSE_ENTER: {
				if (is_entered) {
					return;
				}
				is_entered = true;
				OnMouseEnter(EventArgs.Empty);
				return;
			}

			case Msg.WM_MOUSE_LEAVE: {
				is_entered=false;
				OnMouseLeave(EventArgs.Empty);
				return;
			}

			case Msg.WM_MOUSEHOVER:	{
				OnMouseHover(EventArgs.Empty);
				return;
			}

			case Msg.WM_SYSKEYUP: {
				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}

				if ((m.WParam.ToInt32() & (int)Keys.KeyCode) == (int)Keys.Menu) {
					Form	form;

					form = FindForm();
					if (form != null && form.ActiveMenu != null) {
						form.ActiveMenu.ProcessCmdKey(ref m, (Keys)m.WParam.ToInt32());
					}
				}

				DefWndProc (ref m);
				return;
			}

			case Msg.WM_SYSKEYDOWN:
			case Msg.WM_KEYDOWN:
			case Msg.WM_KEYUP:
			case Msg.WM_SYSCHAR:
			case Msg.WM_CHAR: {
				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}
				DefWndProc (ref m);
				return;
			}

			case Msg.WM_HELP: {
				Point	mouse_pos;
				if (m.LParam != IntPtr.Zero) {
					HELPINFO	hi;

					hi = new HELPINFO();

					hi = (HELPINFO) Marshal.PtrToStructure (m.LParam, typeof (HELPINFO));
					mouse_pos = new Point(hi.MousePos.x, hi.MousePos.y);
				} else {
					mouse_pos = Control.MousePosition;
				}
				OnHelpRequested(new HelpEventArgs(mouse_pos));
				m.Result = (IntPtr)1;
				return;
			}

			case Msg.WM_KILLFOCUS: {
				this.has_focus = false;
				OnLostFocus (EventArgs.Empty);
				return;
			}

			case Msg.WM_SETFOCUS: {
				if (!has_focus) {                
					this.has_focus = true;
					OnGotFocus (EventArgs.Empty);
				}
				return;
			}
					
			case Msg.WM_SYSCOLORCHANGE: {
				ThemeEngine.Current.ResetDefaults();
				OnSystemColorsChanged(EventArgs.Empty);
				return;
			}

			case Msg.WM_SETCURSOR: {
				if ((cursor == null) || ((HitTest)(m.LParam.ToInt32() & 0xffff) != HitTest.HTCLIENT)) {
					DefWndProc(ref m);
					return;
				}

				XplatUI.SetCursor(window.Handle, cursor.handle);
				m.Result = (IntPtr)1;

				return;
			}

			case Msg.WM_CAPTURECHANGED: {
				is_captured = false;
				OnMouseCaptureChanged (EventArgs.Empty);
				m.Result = (IntPtr) 0;
				return;
			}

			default:
				DefWndProc(ref m);
				return;
			}
		}
		#endregion	// Public Instance Methods

		#region OnXXX methods
#if NET_2_0
		protected virtual void OnAutoSizeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[AutoSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnBackColorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [BackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [BackgroundImageChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackgroundImageChanged(e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnBackgroundImageLayoutChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[BackgroundImageLayoutChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBindingContextChanged(EventArgs e) {
			CheckDataBindings ();
			EventHandler eh = (EventHandler)(Events [BindingContextChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBindingContextChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCausesValidationChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CausesValidationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnChangeUICues(UICuesEventArgs e) {
			UICuesEventHandler eh = (UICuesEventHandler)(Events [ChangeUICuesEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClick(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnClientSizeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[ClientSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnContextMenuChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ContextMenuChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnContextMenuStripChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ContextMenuStripChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlAdded(ControlEventArgs e) {
			ControlEventHandler eh = (ControlEventHandler)(Events [ControlAddedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlRemoved(ControlEventArgs e) {
			ControlEventHandler eh = (ControlEventHandler)(Events [ControlRemovedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCreateControl() {
			// Override me!
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCursorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CursorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDockChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DockChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDoubleClick(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragDrop(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragDropEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragEnter(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragEnterEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DragLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragOver(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragOverEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnabledChanged(EventArgs e) {
			if (IsHandleCreated) {
				if (this is Form) {
					if (((Form)this).context == null) {
						XplatUI.EnableWindow(window.Handle, Enabled);
					}
				} else {
					XplatUI.EnableWindow(window.Handle, Enabled);
				}
				Refresh();
			}

			EventHandler eh = (EventHandler)(Events [EnabledChangedEvent]);
			if (eh != null)
				eh (this, e);

			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].OnParentEnabledChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnter(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [EnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnFontChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [FontChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentFontChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnForeColorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentForeColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent) {
			GiveFeedbackEventHandler eh = (GiveFeedbackEventHandler)(Events [GiveFeedbackEvent]);
			if (eh != null)
				eh (this, gfbevent);
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGotFocus(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [GotFocusEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleCreated(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HandleCreatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleDestroyed(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HandleDestroyedEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void RaiseHelpRequested (HelpEventArgs hevent)
		{
			OnHelpRequested (hevent);
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHelpRequested(HelpEventArgs hevent) {
			HelpEventHandler eh = (HelpEventHandler)(Events [HelpRequestedEvent]);
			if (eh != null)
				eh (this, hevent);
		}

		protected virtual void OnImeModeChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ImeModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInvalidated(InvalidateEventArgs e) {
			if (UseDoubleBuffering) {
				// should this block be here?  seems like it
				// would be more at home in
				// NotifyInvalidated..
				if (e.InvalidRect == ClientRectangle) {
					InvalidateBackBuffer ();
				} else if (backbuffer != null){
					// we need this Inflate call here so
					// that the border of the rectangle is
					// considered Visible (the
					// invalid_region.IsVisible call) in
					// the WM_PAINT handling below.
					Rectangle r = Rectangle.Inflate(e.InvalidRect, 1,1);
					backbuffer.InvalidRegion.Union (r);
				}
			}

			InvalidateEventHandler eh = (InvalidateEventHandler)(Events [InvalidatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyDown(KeyEventArgs e) {
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyPress(KeyPressEventArgs e) {
			KeyPressEventHandler eh = (KeyPressEventHandler)(Events [KeyPressEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyUp(KeyEventArgs e) {
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLayout(LayoutEventArgs levent) {
			LayoutEventHandler eh = (LayoutEventHandler)(Events [LayoutEvent]);
			if (eh != null)
				eh (this, levent);

			LayoutEngine.Layout (this, levent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [LeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLocationChanged(EventArgs e) {
			OnMove(e);
			EventHandler eh = (EventHandler)(Events [LocationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLostFocus(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [LostFocusEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected virtual void OnMarginChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[MarginChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#if NET_2_0
		protected virtual void OnMouseCaptureChanged (EventArgs e)
#else
		internal virtual void OnMouseCaptureChanged (EventArgs e)
#endif
		{
			EventHandler eh = (EventHandler)(Events [MouseCaptureChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnMouseClick (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnMouseDoubleClick (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseDown(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseEnter(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseEnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseHover(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseHoverEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseMove(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseMoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseUp(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseWheel(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseWheelEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMove(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnNotifyMessage(Message m) {
			// Override me!
		}

#if NET_2_0
		protected virtual void OnPaddingChanged (EventArgs e) {
			EventHandler eh = (EventHandler) (Events [PaddingChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaint(PaintEventArgs e) {
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal virtual void OnPaintBackgroundInternal(PaintEventArgs e) {
			// Override me
		}

		internal virtual void OnPaintInternal(PaintEventArgs e) {
			// Override me
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaintBackground(PaintEventArgs pevent) {
			PaintControlBackground (pevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBackColorChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackColorChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBackgroundImageChanged(EventArgs e) {
			Invalidate();
			OnBackgroundImageChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBindingContextChanged(EventArgs e) {
			if (binding_context==null) {
				binding_context=Parent.binding_context;
				OnBindingContextChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ParentChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentEnabledChanged(EventArgs e) {
			if (is_enabled) {
				OnEnabledChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentFontChanged(EventArgs e) {
			if (font==null) {
				Invalidate();
				OnFontChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentForeColorChanged(EventArgs e) {
			if (foreground_color.IsEmpty) {
				Invalidate();
				OnForeColorChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentRightToLeftChanged(EventArgs e) {
			if (right_to_left==RightToLeft.Inherit) {
				Invalidate();
				OnRightToLeftChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentVisibleChanged(EventArgs e) {
			if (is_visible) {
				OnVisibleChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
			QueryContinueDragEventHandler eh = (QueryContinueDragEventHandler)(Events [QueryContinueDragEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRegionChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[RegionChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnResize(EventArgs e) {
			PerformLayout(this, "Bounds");

			EventHandler eh = (EventHandler)(Events [ResizeEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [RightToLeftChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentRightToLeftChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSizeChanged(EventArgs e) {
			DisposeBackBuffer ();
			OnResize(e);
			EventHandler eh = (EventHandler)(Events [SizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnStyleChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [StyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSystemColorsChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [SystemColorsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabIndexChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TabIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabStopChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TabStopChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTextChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidated(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ValidatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidating(System.ComponentModel.CancelEventArgs e) {
			CancelEventHandler eh = (CancelEventHandler)(Events [ValidatingEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnVisibleChanged(EventArgs e) {
			if ((parent != null) && !Created && Visible) {
				if (!is_disposed) {
					CreateControl();
					PerformLayout();
				}
			}

			EventHandler eh = (EventHandler)(Events [VisibleChangedEvent]);
			if (eh != null)
				eh (this, e);

			// We need to tell our kids
			for (int i=0; i<child_controls.Count; i++) {
				if (child_controls[i].Visible) {
					child_controls[i].OnParentVisibleChanged(e);
				}
			}
		}
		#endregion	// OnXXX methods

		#region Events
#if NET_2_0
		static object AutoSizeChangedEvent = new object ();
#endif
		static object BackColorChangedEvent = new object ();
		static object BackgroundImageChangedEvent = new object ();
#if NET_2_0
		static object BackgroundImageLayoutChangedEvent = new object ();
#endif
		static object BindingContextChangedEvent = new object ();
		static object CausesValidationChangedEvent = new object ();
		static object ChangeUICuesEvent = new object ();
		static object ClickEvent = new object ();
#if NET_2_0
		static object ClientSizeChangedEvent = new object ();
#endif
		static object ContextMenuChangedEvent = new object ();
#if NET_2_0
		static object ContextMenuStripChangedEvent = new object ();
#endif
		static object ControlAddedEvent = new object ();
		static object ControlRemovedEvent = new object ();
		static object CursorChangedEvent = new object ();
		static object DockChangedEvent = new object ();
		static object DoubleClickEvent = new object ();
		static object DragDropEvent = new object ();
		static object DragEnterEvent = new object ();
		static object DragLeaveEvent = new object ();
		static object DragOverEvent = new object ();
		static object EnabledChangedEvent = new object ();
		static object EnterEvent = new object ();
		static object FontChangedEvent = new object ();
		static object ForeColorChangedEvent = new object ();
		static object GiveFeedbackEvent = new object ();
		static object GotFocusEvent = new object ();
		static object HandleCreatedEvent = new object ();
		static object HandleDestroyedEvent = new object ();
		static object HelpRequestedEvent = new object ();
		static object ImeModeChangedEvent = new object ();
		static object InvalidatedEvent = new object ();
		static object KeyDownEvent = new object ();
		static object KeyPressEvent = new object ();
		static object KeyUpEvent = new object ();
		static object LayoutEvent = new object ();
		static object LeaveEvent = new object ();
		static object LocationChangedEvent = new object ();
		static object LostFocusEvent = new object ();
#if NET_2_0
		static object MarginChangedEvent = new object ();
#endif
		static object MouseCaptureChangedEvent = new object ();
#if NET_2_0
		static object MouseClickEvent = new object ();
		static object MouseDoubleClickEvent = new object ();
#endif
		static object MouseDownEvent = new object ();
		static object MouseEnterEvent = new object ();
		static object MouseHoverEvent = new object ();
		static object MouseLeaveEvent = new object ();
		static object MouseMoveEvent = new object ();
		static object MouseUpEvent = new object ();
		static object MouseWheelEvent = new object ();
		static object MoveEvent = new object ();
#if NET_2_0
		static object PaddingChangedEvent = new object ();
#endif
		static object PaintEvent = new object ();
		static object ParentChangedEvent = new object ();
#if NET_2_0
		static object PreviewKeyDownEvent = new object ();
#endif
		static object QueryAccessibilityHelpEvent = new object ();
		static object QueryContinueDragEvent = new object ();
#if NET_2_0
		static object RegionChangedEvent = new object ();
#endif
		static object ResizeEvent = new object ();
		static object RightToLeftChangedEvent = new object ();
		static object SizeChangedEvent = new object ();
		static object StyleChangedEvent = new object ();
		static object SystemColorsChangedEvent = new object ();
		static object TabIndexChangedEvent = new object ();
		static object TabStopChangedEvent = new object ();
		static object TextChangedEvent = new object ();
		static object ValidatedEvent = new object ();
		static object ValidatingEvent = new object ();
		static object VisibleChangedEvent = new object ();

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value);}
			remove {Events.RemoveHandler (AutoSizeChangedEvent, value);}
		}
#endif
		public event EventHandler BackColorChanged {
			add { Events.AddHandler (BackColorChangedEvent, value); }
			remove { Events.RemoveHandler (BackColorChangedEvent, value); }
		}

		public event EventHandler BackgroundImageChanged {
			add { Events.AddHandler (BackgroundImageChangedEvent, value); }
			remove { Events.RemoveHandler (BackgroundImageChangedEvent, value); }
		}

#if NET_2_0	
		public event EventHandler BackgroundImageLayoutChanged {
			add {Events.AddHandler (BackgroundImageLayoutChangedEvent, value);}
			remove {Events.RemoveHandler (BackgroundImageLayoutChangedEvent, value);}
		}
#endif

		public event EventHandler BindingContextChanged {
			add { Events.AddHandler (BindingContextChangedEvent, value); }
			remove { Events.RemoveHandler (BindingContextChangedEvent, value); }
		}

		public event EventHandler CausesValidationChanged {
			add { Events.AddHandler (CausesValidationChangedEvent, value); }
			remove { Events.RemoveHandler (CausesValidationChangedEvent, value); }
		}

		public event UICuesEventHandler ChangeUICues {
			add { Events.AddHandler (ChangeUICuesEvent, value); }
			remove { Events.RemoveHandler (ChangeUICuesEvent, value); }
		}

		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}
		
#if NET_2_0
		public event EventHandler ClientSizeChanged {
			add {Events.AddHandler (ClientSizeChangedEvent, value);}
			remove {Events.RemoveHandler (ClientSizeChangedEvent, value);}
		}
#endif

#if NET_2_0
		[Browsable (false)]
#endif
		public event EventHandler ContextMenuChanged {
			add { Events.AddHandler (ContextMenuChangedEvent, value); }
			remove { Events.RemoveHandler (ContextMenuChangedEvent, value); }
		}

#if NET_2_0
		public event EventHandler ContextMenuStripChanged {
			add { Events.AddHandler (ContextMenuStripChangedEvent, value); }
			remove { Events.RemoveHandler (ContextMenuStripChangedEvent, value);}
		}
#endif


		[EditorBrowsable(EditorBrowsableState.Advanced)]
#if NET_2_0
		[Browsable(true)]
#else 
		[Browsable(false)]
#endif
		public event ControlEventHandler ControlAdded {
			add { Events.AddHandler (ControlAddedEvent, value); }
			remove { Events.RemoveHandler (ControlAddedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
#if NET_2_0
		[Browsable(true)]
#else 
		[Browsable(false)]
#endif
		public event ControlEventHandler ControlRemoved {
			add { Events.AddHandler (ControlRemovedEvent, value); }
			remove { Events.RemoveHandler (ControlRemovedEvent, value); }
		}

		[MWFDescription("Fired when the cursor for the control has been changed"), MWFCategory("PropertyChanged")]
		public event EventHandler CursorChanged {
			add { Events.AddHandler (CursorChangedEvent, value); }
			remove { Events.RemoveHandler (CursorChangedEvent, value); }
		}
		public event EventHandler DockChanged {
			add { Events.AddHandler (DockChangedEvent, value); }
			remove { Events.RemoveHandler (DockChangedEvent, value); }
		}

		public event EventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove { Events.RemoveHandler (DoubleClickEvent, value); }
		}

		public event DragEventHandler DragDrop {
			add { Events.AddHandler (DragDropEvent, value); }
			remove { Events.RemoveHandler (DragDropEvent, value); }
		}

		public event DragEventHandler DragEnter {
			add { Events.AddHandler (DragEnterEvent, value); }
			remove { Events.RemoveHandler (DragEnterEvent, value); }
		}

		public event EventHandler DragLeave {
			add { Events.AddHandler (DragLeaveEvent, value); }
			remove { Events.RemoveHandler (DragLeaveEvent, value); }
		}

		public event DragEventHandler DragOver {
			add { Events.AddHandler (DragOverEvent, value); }
			remove { Events.RemoveHandler (DragOverEvent, value); }
		}

		public event EventHandler EnabledChanged {
			add { Events.AddHandler (EnabledChangedEvent, value); }
			remove { Events.RemoveHandler (EnabledChangedEvent, value); }
		}

		public event EventHandler Enter {
			add { Events.AddHandler (EnterEvent, value); }
			remove { Events.RemoveHandler (EnterEvent, value); }
		}

		public event EventHandler FontChanged {
			add { Events.AddHandler (FontChangedEvent, value); }
			remove { Events.RemoveHandler (FontChangedEvent, value); }
		}

		public event EventHandler ForeColorChanged {
			add { Events.AddHandler (ForeColorChangedEvent, value); }
			remove { Events.RemoveHandler (ForeColorChangedEvent, value); }
		}

		public event GiveFeedbackEventHandler GiveFeedback {
			add { Events.AddHandler (GiveFeedbackEvent, value); }
			remove { Events.RemoveHandler (GiveFeedbackEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler GotFocus {
			add { Events.AddHandler (GotFocusEvent, value); }
			remove { Events.RemoveHandler (GotFocusEvent, value); }
		}


		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler HandleCreated {
			add { Events.AddHandler (HandleCreatedEvent, value); }
			remove { Events.RemoveHandler (HandleCreatedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler HandleDestroyed {
			add { Events.AddHandler (HandleDestroyedEvent, value); }
			remove { Events.RemoveHandler (HandleDestroyedEvent, value); }
		}

		public event HelpEventHandler HelpRequested {
			add { Events.AddHandler (HelpRequestedEvent, value); }
			remove { Events.RemoveHandler (HelpRequestedEvent, value); }
		}

		public event EventHandler ImeModeChanged {
			add { Events.AddHandler (ImeModeChangedEvent, value); }
			remove { Events.RemoveHandler (ImeModeChangedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event InvalidateEventHandler Invalidated {
			add { Events.AddHandler (InvalidatedEvent, value); }
			remove { Events.RemoveHandler (InvalidatedEvent, value); }
		}

		public event KeyEventHandler KeyDown {
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		public event KeyPressEventHandler KeyPress {
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}

		public event KeyEventHandler KeyUp {
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}

		public event LayoutEventHandler Layout {
			add { Events.AddHandler (LayoutEvent, value); }
			remove { Events.RemoveHandler (LayoutEvent, value); }
		}

		public event EventHandler Leave {
			add { Events.AddHandler (LeaveEvent, value); }
			remove { Events.RemoveHandler (LeaveEvent, value); }
		}

		public event EventHandler LocationChanged {
			add { Events.AddHandler (LocationChangedEvent, value); }
			remove { Events.RemoveHandler (LocationChangedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler LostFocus {
			add { Events.AddHandler (LostFocusEvent, value); }
			remove { Events.RemoveHandler (LostFocusEvent, value); }
		}

#if NET_2_0
		public event EventHandler MarginChanged {
			add { Events.AddHandler (MarginChangedEvent, value); }
			remove {Events.RemoveHandler (MarginChangedEvent, value); }
		}
#endif
#if NET_2_0
		public event EventHandler MouseCaptureChanged {
#else
		internal event EventHandler MouseCaptureChanged {
#endif
			add { Events.AddHandler (MouseCaptureChangedEvent, value); }
			remove { Events.RemoveHandler (MouseCaptureChangedEvent, value); }
		}
#if NET_2_0		
		public event MouseEventHandler MouseClick
		{
			add { Events.AddHandler (MouseClickEvent, value); }
			remove { Events.RemoveHandler (MouseClickEvent, value); }
		}
		public event MouseEventHandler MouseDoubleClick
		{
			add { Events.AddHandler (MouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (MouseDoubleClickEvent, value); }
		}
#endif
		public event MouseEventHandler MouseDown {
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}

		public event EventHandler MouseEnter {
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}

		public event EventHandler MouseHover {
			add { Events.AddHandler (MouseHoverEvent, value); }
			remove { Events.RemoveHandler (MouseHoverEvent, value); }
		}

		public event EventHandler MouseLeave {
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}

		public event MouseEventHandler MouseMove {
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}

		public event MouseEventHandler MouseUp {
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event MouseEventHandler MouseWheel {
			add { Events.AddHandler (MouseWheelEvent, value); }
			remove { Events.RemoveHandler (MouseWheelEvent, value); }
		}

		public event EventHandler Move {
			add { Events.AddHandler (MoveEvent, value); }
			remove { Events.RemoveHandler (MoveEvent, value); }
		}
#if NET_2_0
		public event EventHandler PaddingChanged
		{
			add { Events.AddHandler (PaddingChangedEvent, value); }
			remove { Events.RemoveHandler (PaddingChangedEvent, value); }
		}
#endif
		public event PaintEventHandler Paint {
			add { Events.AddHandler (PaintEvent, value); }
			remove { Events.RemoveHandler (PaintEvent, value); }
		}

		public event EventHandler ParentChanged {
			add { Events.AddHandler (ParentChangedEvent, value); }
			remove { Events.RemoveHandler (ParentChangedEvent, value); }
		}

#if NET_2_0
		public event PreviewKeyDownEventHandler PreviewKeyDown {
			add { Events.AddHandler (PreviewKeyDownEvent, value); }
			remove { Events.RemoveHandler (PreviewKeyDownEvent, value); }
		}
#endif

		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { Events.AddHandler (QueryAccessibilityHelpEvent, value); }
			remove { Events.RemoveHandler (QueryAccessibilityHelpEvent, value); }
		}

		public event QueryContinueDragEventHandler QueryContinueDrag {
			add { Events.AddHandler (QueryContinueDragEvent, value); }
			remove { Events.RemoveHandler (QueryContinueDragEvent, value); }
		}

#if NET_2_0
		public event EventHandler RegionChanged {
			add { Events.AddHandler (RegionChangedEvent, value); }
			remove { Events.RemoveHandler (RegionChangedEvent, value); }
		}
#endif

#if NET_2_0	
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public event EventHandler Resize {
			add { Events.AddHandler (ResizeEvent, value); }
			remove { Events.RemoveHandler (ResizeEvent, value); }
		}

		public event EventHandler RightToLeftChanged {
			add { Events.AddHandler (RightToLeftChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftChangedEvent, value); }
		}

		public event EventHandler SizeChanged {
			add { Events.AddHandler (SizeChangedEvent, value); }
			remove { Events.RemoveHandler (SizeChangedEvent, value); }
		}

		public event EventHandler StyleChanged {
			add { Events.AddHandler (StyleChangedEvent, value); }
			remove { Events.RemoveHandler (StyleChangedEvent, value); }
		}

		public event EventHandler SystemColorsChanged {
			add { Events.AddHandler (SystemColorsChangedEvent, value); }
			remove { Events.RemoveHandler (SystemColorsChangedEvent, value); }
		}

		public event EventHandler TabIndexChanged {
			add { Events.AddHandler (TabIndexChangedEvent, value); }
			remove { Events.RemoveHandler (TabIndexChangedEvent, value); }
		}

		public event EventHandler TabStopChanged {
			add { Events.AddHandler (TabStopChangedEvent, value); }
			remove { Events.RemoveHandler (TabStopChangedEvent, value); }
		}

		public event EventHandler TextChanged {
			add { Events.AddHandler (TextChangedEvent, value); }
			remove { Events.RemoveHandler (TextChangedEvent, value); }
		}

		public event EventHandler Validated {
			add { Events.AddHandler (ValidatedEvent, value); }
			remove { Events.RemoveHandler (ValidatedEvent, value); }
		}

		public event CancelEventHandler Validating {
			add { Events.AddHandler (ValidatingEvent, value); }
			remove { Events.RemoveHandler (ValidatingEvent, value); }
		}

		public event EventHandler VisibleChanged {
			add { Events.AddHandler (VisibleChangedEvent, value); }
			remove { Events.RemoveHandler (VisibleChangedEvent, value); }
		}

		#endregion	// Events
	}
}
