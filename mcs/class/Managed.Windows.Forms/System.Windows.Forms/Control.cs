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
// Copyright (c) 2004-2005 Novell, Inc.
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

// COMPLETE 

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.ControlDesigner, " + Consts.AssemblySystem_Design)]
	[DefaultProperty("Text")]
	[DefaultEvent("Click")]
	[DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[ToolboxItemFilter("System.Windows.Forms")]
	public class Control : Component, ISynchronizeInvoke, IWin32Window
        {
		#region Local Variables

		// Basic
		internal Rectangle		bounds;			// bounding rectangle for control (client area + decorations)
		internal object			creator_thread;		// thread that created the control
		internal ControlNativeWindow	window;			// object for native window handle
		internal string			name;			// for object naming

		// State
		private bool			create_handled;		// true if OnCreateControl has been sent
		internal bool			has_focus;		// true if control has focus
		internal bool			is_visible;		// true if control is visible
		internal bool			is_entered;		// is the mouse inside the control?
		internal bool			is_enabled;		// true if control is enabled (usable/not grayed out)
		internal bool			is_selected;		// true if control is selected
		internal bool			is_accessible;		// true if the control is visible to accessibility applications
		internal bool			is_captured;		// tracks if the control has captured the mouse
		internal bool			is_toplevel;		// tracks if the control is a toplevel window
		internal bool			is_recreating;		// tracks if the handle for the control is being recreated
		internal bool			causes_validation;	// tracks if validation is executed on changes
		internal int			tab_index;		// position in tab order of siblings
		internal bool			tab_stop = true;	// is the control a tab stop?
		internal bool			is_disposed;		// has the window already been disposed?
		internal Size			client_size;		// size of the client area (window excluding decorations)
		internal Rectangle		client_rect;		// rectangle with the client area (window excluding decorations)
		internal ControlStyles		control_style;		// rather win32-specific, style bits for control
		internal ImeMode		ime_mode = ImeMode.Inherit;
		internal bool			layout_pending;		// true if our parent needs to re-layout us
		internal object			control_tag;		// object that contains data about our control
		internal int			mouse_clicks;		// Counter for mouse clicks
		internal Cursor			cursor;			// Cursor for the window

		// Visuals
		internal Color			foreground_color;	// foreground color for control
		internal Color			background_color;	// background color for control
		internal Image			background_image;	// background image for control
		internal Font			font;			// font for control
		internal string			text;			// window/title text for control
		internal BorderStyle		border_style;		// Border style of control

		// Layout
		internal AnchorStyles		anchor_style;		// anchoring requirements for our control
		internal DockStyle		dock_style;		// docking requirements for our control (supercedes anchoring)
		internal SizeF			size_ratio;		// size ratio of our control to it's parent; required for anchoring
		internal Size			prev_size;		// previous size of the control; required for anchoring

		// to be categorized...
		static internal ArrayList	controls = new ArrayList();		// All of the application's controls, in a flat list
		internal ControlCollection	child_controls;		// our children
		internal Control		parent;			// our parent control
		internal AccessibleObject	accessibility_object;	// object that contains accessibility information about our control
		internal BindingContext		binding_context;	// TODO
		internal RightToLeft		right_to_left;		// drawing direction for control
		internal int			layout_suspended;
		internal ContextMenu		context_menu;		// Context menu associated with the control

		private Graphics		dc_mem;			// Graphics context for double buffering
		private Bitmap			bmp_mem;		// Bitmap for double buffering control
		private bool			needs_redraw;

		private ControlBindingsCollection data_bindings;

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

				return window.owner;
			}

			protected override void WndProc(ref Message m) {
				owner.WndProc(ref m);
			}
		}
		#endregion
		
		#region Public Classes
		[ComVisible(true)]
		public class ControlAccessibleObject : AccessibleObject {			
			#region ControlAccessibleObject Local Variables
			private Control	owner;
			#endregion	// ControlAccessibleObject Local Variables

			#region ControlAccessibleObject Constructors
			public ControlAccessibleObject(Control ownerControl) {
				this.owner = ownerControl;
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
					return owner.Handle;
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
					return owner;
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

			[MonoTODO("Implement this and tie it into Control.AccessibilityNotifyClients")]
			public void NotifyClients(AccessibleEvents accEvent) {
				throw new NotImplementedException();
			}

			[MonoTODO("Implement this and tie it into Control.AccessibilityNotifyClients")]
			public void NotifyClients(AccessibleEvents accEvent, int childID) {
				throw new NotImplementedException();
			}

			public override string ToString() {
				return "ControlAccessibleObject: Owner = " + owner.ToString() + ", Text: " + owner.text;
			}

			#endregion	// ControlAccessibleObject Public Instance Methods
		}

		[DesignerSerializer("System.Windows.Forms.Design.ControlCollectionCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
		[ListBindable(false)]
		public class ControlCollection : IList, ICollection, ICloneable, IEnumerable {
			#region	ControlCollection Local Variables
			internal ArrayList	list;
			internal Control	owner;
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
			public virtual void Add (Control value)
			{
				
				for (int i = 0; i < list.Count; i++) {
					if (list [i] == value) {
						// Do we need to do anything here?
						return;
					}
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

				list.Add (value);
				value.Parent = owner;
				owner.UpdateZOrder();
				owner.OnControlAdded(new ControlEventArgs(value));
			}
			
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

			public virtual void Clear ()
			{
				owner.SuspendLayout();
				for (int i = 0; i < list.Count; i++) {
					owner.OnControlRemoved(new ControlEventArgs((Control)list[i]));
				}
				list.Clear();
				owner.ResumeLayout();
			}

			public virtual bool Contains (Control value)
			{
				return list.Contains (value);
			}

			public void CopyTo (Array array, int index)
			{
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
				owner.OnControlRemoved(new ControlEventArgs(value));
				list.Remove(value);
				owner.UpdateZOrder();
			}

			public void RemoveAt(int index) {
				if (index<0 || index>=list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
				}

				owner.OnControlRemoved(new ControlEventArgs((Control)list[index]));
				list.RemoveAt(index);
				owner.UpdateZOrder();
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
				owner.UpdateZOrder();
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

			prev_size = Size.Empty;
			anchor_style = AnchorStyles.Top | AnchorStyles.Left;

			create_handled = false;
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

			control_style = ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick;

			parent = null;
			background_image = null;
			text = string.Empty;
			name = string.Empty;			

			child_controls = CreateControlsInstance();
			client_size = new Size(DefaultSize.Width, DefaultSize.Height);
			client_rect = new Rectangle(0, 0, DefaultSize.Width, DefaultSize.Height);
			XplatUI.CalculateWindowRect(IntPtr.Zero, ref client_rect, CreateParams.Style, CreateParams.ExStyle, IntPtr.Zero, out bounds);
			if ((CreateParams.Style & (int)WindowStyles.WS_CHILD) == 0) {
				bounds.X=-1;
				bounds.Y=-1;
			}
		}

		public Control(Control parent, string text) : this() {
			Text=text;
			Parent=parent;
		}

		public Control(Control parent, string text, int left, int top, int width, int height) : this() {
			Parent=parent;
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBoundsCore(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		public Control(string text) : this() {
			Text=text;
		}

		public Control(string text, int left, int top, int width, int height) : this() {
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBoundsCore(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		protected override void Dispose(bool disposing) {
			is_disposed = true;
			if (dc_mem!=null) {
				dc_mem.Dispose();
				dc_mem=null;
			}

			if (bmp_mem!=null) {
				bmp_mem.Dispose();
				bmp_mem=null;
			}

			DestroyHandle();
			controls.Remove(this);
		}
		#endregion 	// Public Constructors

		#region Internal Properties
		internal BorderStyle InternalBorderStyle {
			get {
				return border_style;
			}

			set {
				if (border_style != value) {
					border_style = value;

					XplatUI.SetBorderStyle(window.Handle, border_style);
				}
			}
		}
		#endregion	// Internal Properties

		#region Private & Internal Methods
		internal static IAsyncResult BeginInvokeInternal (Delegate method, object [] args) {
			AsyncMethodResult result = new AsyncMethodResult ();
			AsyncMethodData data = new AsyncMethodData ();

			data.Method = method;
			data.Args = args;
			data.Result = new WeakReference (result);

			XplatUI.SendAsyncMethod (data);
			return result;
		}

		internal Graphics DeviceContext {
			get { 
				if (dc_mem==null) {
					CreateBuffers(this.Width, this.Height);
				}
				return dc_mem;
			}
		}

		private Bitmap ImageBuffer {
			get {
				if (bmp_mem==null) {
					CreateBuffers(this.Width, this.Height);
				}
				return bmp_mem;
			}
		}

		internal void CreateBuffers (int width, int height) {
			if (dc_mem != null) {
				dc_mem.Dispose ();
			}
			if (bmp_mem != null)
				bmp_mem.Dispose ();

			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}

			bmp_mem = new Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
			needs_redraw = true;
		}

		internal void InvalidateBuffers ()
		{
			if (dc_mem != null) {
				dc_mem.Dispose ();
			}
			if (bmp_mem != null)
				bmp_mem.Dispose ();

			dc_mem = null;
			bmp_mem = null;
			needs_redraw = true;
		}

		internal static void SetChildColor(Control parent) {
			Control	child;

			for (int i=0; i < parent.child_controls.Count; i++) {
				child=parent.child_controls[i];
				if (child.IsHandleCreated) {
					XplatUI.SetWindowBackground(child.window.Handle, child.BackColor);
				}
				if (child.child_controls.Count>0) {
					SetChildColor(child);
				}
			}
				
		}

		internal bool Select(Control control) {
			Control			parent;
			IContainerControl	container;

			if (control == null) {
				return false;
			}

			parent = control.parent;

			if (((control.control_style & ControlStyles.Selectable) !=0)  && (parent != null)) {
				while (parent != null) {
					if (!parent.Visible || !parent.is_enabled) {
						return false;
					}
					parent = parent.parent;
				}
			}

			control.is_selected = true;

			XplatUI.SetFocus(control.window.Handle);
			container = GetContainerControl();
			if (container != null) {
				container.ActiveControl = control;
			}
			return true;
		}

		internal virtual void DoDefaultAction() {
			// Only here to be overriden by our actual controls; this is needed by the accessibility class
		}

		internal static int LowOrder (int param) {
			return (param & 0xffff);
		}

		internal static int HighOrder (int param) {
			return (param >> 16);
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

			for (int i = 0; i < end; i++) {
				if (found == null) {
					if (container.child_controls[i].tab_index > index) {
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
			Control	p;

			found = null;

			if (start != null) {
				if ((start is IContainerControl) || start.GetStyle(ControlStyles.ContainerControl)) {
					found = FindControlForward(start, null);
					if (found != null) {
						return found;
					}
				}

				p = start.parent;
				while (p != container) {
					found = FindFlatForward(p, start);
					if (found != null) {
						return found;
					}
					start = p;
					p = p.parent;
				}
			}
			return FindFlatForward(container, start);
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

			for (int i = 0; i < end; i++) {
				if (found == null) {
					if (container.child_controls[i].tab_index < index) {
						found = container.child_controls[i];
					}
				} else if (found.tab_index < container.child_controls[i].tab_index) {
					if (container.child_controls[i].tab_index < index) {
						found = container.child_controls[i];
					}
				}
			}
			return found;
		}

		private static Control FindControlBackward(Control container, Control start) {
			Control found;

			found = null;

			if (start != null) {
				found = FindFlatBackward(start.parent, start);
				if (found == null && start.parent != container) {
					return start.parent;
				}
			}
			if (found == null) {
				found = FindFlatBackward(container, start);
			}

			while ((found != null) && ((found is IContainerControl) || found.GetStyle(ControlStyles.ContainerControl))) {
				found = FindControlBackward(found, null);
				if (found != null) {
					return found;
				}
			}

			return found;
		}

		private void HandleClick(int clicks) {
			if (GetStyle(ControlStyles.StandardClick)) {
				if (clicks > 1) {
					if (GetStyle(ControlStyles.StandardDoubleClick)) {
						OnDoubleClick(EventArgs.Empty);
					} else {
						OnClick(EventArgs.Empty);
					}
				} else {
					OnClick(EventArgs.Empty);
				}
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
				return AccessibilityObject.default_action;
			}

			set {
				AccessibilityObject.default_action=value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		public string AccessibleDescription {
			get {
				return AccessibilityObject.description;
			}

			set {
				AccessibilityObject.description=value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		public string AccessibleName {
			get {
				return AccessibilityObject.Name;
			}

			set {
				AccessibilityObject.Name=value;
			}
		}

		[DefaultValue(AccessibleRole.Default)]
		public AccessibleRole AccessibleRole {
			get {
				return AccessibilityObject.role;
			}

			set {
				AccessibilityObject.role=value;
			}
		}

		[DefaultValue(false)]
		public virtual bool AllowDrop {
			get {
				return XplatUI.State.DropTarget;
			}

			set {
				#if later
					XplatUI.State.DropTarget=value;
				#endif
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
	        [DefaultValue(AnchorStyles.Top | AnchorStyles.Left)]
		public virtual AnchorStyles Anchor {
			get {
				return anchor_style;
			}

			set {
				anchor_style=value;

				if (parent != null) {
					parent.PerformLayout(this, "Parent");
				}
			}
		}

		[DispId(-501)]
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
				if (this.IsHandleCreated) {
					XplatUI.SetWindowBackground(this.window.Handle, value);
				}
				SetChildColor(this);
				OnBackColorChanged(EventArgs.Empty);
				Invalidate();
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
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
				SetBoundsCore(value.Left, value.Top, value.Width, value.Height, BoundsSpecified.All);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanFocus {
			get {
				if (Visible && is_enabled && GetStyle(ControlStyles.Selectable)) {
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

				if (!GetStyle(ControlStyles.Selectable) || this.parent == null) {
					return false;
				}

				parent = this.parent;
				while (parent != null) {
					if (!parent.is_visible || !parent.is_enabled) {
						return false;
					}

					parent = parent.parent;
				}
				return true;
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
				if (this.IsHandleCreated) {
					if (value && !is_captured) {
						is_captured = true;
						XplatUI.GrabWindow(this.window.Handle, IntPtr.Zero);
					} else if (!value && is_captured) {
						XplatUI.UngrabWindow(this.window.Handle);
						is_captured = false;
					}
				}
			}
		}

		[DefaultValue(true)]
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
				if (this.Focused) {
					return true;
				}

				for (int i=0; i < child_controls.Count; i++) {
					if (child_controls[i].ContainsFocus) {
						return true;
					}
				}
				return false;
			}
		}

		[DefaultValue(null)]
		public virtual ContextMenu ContextMenu {
			get {
				return context_menu;
			}

			set {
				if (context_menu != value) {
					context_menu = value;
					OnContextMenuChanged(EventArgs.Empty);
				}
			}
		}

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
				if (!this.is_disposed && (this.window.Handle != IntPtr.Zero)) {
					return true;
				}
				return false;
			}
		}

		[AmbientValue(null)]
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
					Point	pt;

					cursor = value;
					
					pt = Cursor.Position;
					if (bounds.Contains(pt)) {
						if (GetChildAtPoint(pt) == null) {
							if (cursor != null) {
								XplatUI.SetCursor(window.Handle, cursor.handle);
							} else {
								if (parent != null) {
									XplatUI.SetCursor(window.Handle, parent.Cursor.handle);
								} else {
									XplatUI.SetCursor(window.Handle, Cursors.def.handle);
								}
							}
						}
					}

					OnCursorChanged(EventArgs.Empty);
				}
			}
		}


		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		public virtual DockStyle Dock {
			get {
				return dock_style;
			}

			set {
				if (dock_style == value) {
					return;
				}

				dock_style = value;

				if (parent != null) {
					parent.PerformLayout(this, "Parent");
				}

				OnDockChanged(EventArgs.Empty);
			}
		}

		[DispId(-514)]
		[Localizable(true)]
		public bool Enabled {
			get {
				return is_enabled;
			}

			set {
				if (is_enabled == value) {
					return;
				}

				is_enabled = value;
				Refresh();
				OnEnabledChanged (EventArgs.Empty);				
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
				if (font != null && font.Equals (value)) {
					return;
				}

				font = value;	
				Invalidate();
				OnFontChanged (EventArgs.Empty);				
			}
		}

		[DispId(-513)]
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
				SetBoundsCore(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
			}
		}

		[AmbientValue(ImeMode.Inherit)]
		[Localizable(true)]
		public ImeMode ImeMode {
			get {
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
				if (creator_thread!=Thread.CurrentThread) {
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
				if ((window!=null) && (window.Handle!=IntPtr.Zero)) {
					return true;
				}

				return false;
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
				SetBoundsCore(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
			}
		}

		[Localizable(true)]
		public Point Location {
			get {
				return new Point(bounds.X, bounds.Y);
			}

			set {
				SetBoundsCore(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
			}
		}

		[Browsable(false)]
		public string Name {
			get {
				return this.name;
			}

			set {
				this.name=value;
			}
		}

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
					if (parent!=null) {
						parent.Controls.Remove(this);
					}

					parent=value;

					if (!parent.Controls.Contains(this)) {
						parent.Controls.Add(this);
					}

					XplatUI.SetParent(Handle, value.Handle);

					InitLayout();

					OnParentChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductName {
			get {
				return "Novell Mono MWF";
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductVersion {
			get {
				return "1.1.4322.573";
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
				return new Region(this.bounds);
			}

			set {
				Graphics	g;
				RectangleF	r;

				g = this.CreateGraphics();
				r = value.GetBounds(g);

				SetBounds((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

				g.Dispose();
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
		public virtual RightToLeft RightToLeft {
			get {
				return right_to_left;
			}

			set {
				if (value != right_to_left) {
					right_to_left = value;
					OnRightToLeftChanged(EventArgs.Empty);
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
			}
		}

		[Localizable(true)]
		public Size Size {
			get {
				return new Size(Width, Height);
			}

			set {
				SetBoundsCore(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
		}

		[Localizable(true)]
		[MergableProperty(false)]
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
		public virtual string Text {
			get {
				return this.text;
			}

			set {
				if (value == null) {
					value = String.Empty;
				}

				if (text!=value) {
					text=value;
					XplatUI.Text(Handle, text);
					// FIXME: Do we need a Refresh() here?
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
				SetBoundsCore(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
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

				return p;
			}
		}

		[Localizable(true)]
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
				SetBoundsCore(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
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

				create_params.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;

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

		protected int FontHeight {
			get {
				return Font.Height;
			}

			set {
				;; // Nothing to do
			}
		}

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
		protected bool ShowKeyboardCues {
			get {
				return true;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromChildHandle(IntPtr handle) {
			IEnumerator control = Control.controls.GetEnumerator();

			while (control.MoveNext()) {
				if (((Control)control.Current).window.Handle == handle) {
					// Found it
					if (((Control)control.Current).Parent != null) {
						return ((Control)control.Current).Parent;
					}
				}
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromHandle(IntPtr handle) {
			IEnumerator control = Control.controls.GetEnumerator();

			while (control.MoveNext()) {
				if (((Control)control.Current).window.Handle == handle) {
					// Found it
					return ((Control)control.Current);
				}
			}
			return null;
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
			return BeginInvokeInternal(method, null);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInvoke (Delegate method, object[] args) {
			return BeginInvokeInternal (method, args);
		}

		public void BringToFront() {
			if ((parent != null) && (parent.child_controls[0]!=this)) {
				if (parent.child_controls.Contains(this)) {
					parent.child_controls.SetChildIndex(this, 0);
				}
			}

			XplatUI.SetZOrder(this.window.Handle, IntPtr.Zero, true, false);

			if (parent != null) {
				parent.Refresh();
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

		public void CreateControl() {

			if (!IsHandleCreated) {
				CreateHandle();
			}

			if (!create_handled) {
				create_handled = true;
				OnCreateControl();
			}

			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].CreateControl();
			}
		}

		public Graphics CreateGraphics() {
			if (!IsHandleCreated) {
				this.CreateControl();
			}
			return Graphics.FromHwnd(this.window.Handle);
		}

		[MonoTODO("Come up with cross platform drag-drop driver interface")]
		public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects) {
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

		public bool Focus() {
			if (IsHandleCreated && !has_focus) {
				has_focus = true;
				XplatUI.SetFocus(window.Handle);
			}
			return true;
		}

		public Control GetChildAtPoint(Point pt) {
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
			// If we're not a container we don't play
			if (!(this is IContainerControl) && !this.GetStyle(ControlStyles.ContainerControl)) {
				return null;
			}

			// If ctl is not contained by this, we start at the first child of this
			if (!this.Contains(ctl)) {
				ctl = null;
			}

			// Search through our controls, starting at ctl, stepping into children as we encounter them
			// try to find the control with the tabindex closest to our own, or, if we're looking into
			// child controls, the one with the smallest tabindex
			if (forward) {
				return FindControlForward(this, ctl);
			}
			return FindControlBackward(this, ctl);
		}

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
			if (!IsHandleCreated || !Visible) {
				return;
			}

			NotifyInvalidate(rc);

			XplatUI.Invalidate(Handle, rc, false);

			if (invalidateChildren) {
				for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			}
			OnInvalidated(new InvalidateEventArgs(rc));
		}

		public void Invalidate(System.Drawing.Region region) {
			Invalidate(region, false);
		}

		[MonoTODO("Figure out if GetRegionScans is usable")]
		public void Invalidate(System.Drawing.Region region, bool invalidateChildren) {
			throw new NotImplementedException();

			// FIXME - should use the GetRegionScans function of the region to invalidate each area
			//if (invalidateChildren) {
			//	for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			//}
		}

		public object Invoke (Delegate method) {
			return Invoke(method, null);
		}

		public object Invoke (Delegate method, object[] args) {
			IAsyncResult result = BeginInvoke (method, args);
			return EndInvoke(result);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout() {
			PerformLayout(null, null);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout(Control affectedControl, string affectedProperty) {
			LayoutEventArgs levent = new LayoutEventArgs(affectedControl, affectedProperty);

			if (layout_suspended>0) {
				layout_pending = true;
				return;
			}

			layout_pending = false;

			// Prevent us from getting messed up
			layout_suspended++;

			// Perform all Dock and Anchor calculations
			try {
				Control		child;
				AnchorStyles	anchor;
				Rectangle	space;
				int		diff_width;
				int		diff_height;

				space=this.DisplayRectangle;
				if (prev_size != Size.Empty) {
					diff_width = space.Width - prev_size.Width;
					diff_height = space.Height - prev_size.Height;
				} else {
					diff_width = 0;
					diff_height = 0;
				}

				// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
				for (int i = child_controls.Count - 1; i >= 0; i--) {
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
					}
				}

				for (int i = child_controls.Count - 1; i >= 0; i--) {
					child=child_controls[i];

					if (child.Dock == DockStyle.Fill) {
						child.SetBounds(space.Left, space.Top, space.Width, space.Height);
						space.Width=0;
						space.Height=0;
					}
				}

				space=this.DisplayRectangle;

				// Deal with anchoring
				for (int i=0; i < child_controls.Count; i++) {
					int left;
					int top;
					int width;
					int height;

					child=child_controls[i];
					anchor=child.Anchor;

					left=child.Left;
					top=child.Top;
					width=child.Width;
					height=child.Height;

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
			Keys key_data;

			if ((msg.Msg == (int)Msg.WM_KEYDOWN) || (msg.Msg == (int)Msg.WM_SYSKEYDOWN)) {
				key_data = (Keys)msg.WParam.ToInt32();
				if (!ProcessCmdKey(ref msg, key_data)) {
					if (IsInputKey(key_data)) {
						return true;
					}

					return ProcessDialogKey(key_data);
				}

				return true;
			} else if (msg.Msg == (int)Msg.WM_CHAR) {
				if (IsInputChar((char)msg.WParam)) {
					return true;
				}
			} else if (msg.Msg == (int)Msg.WM_SYSCHAR) {
				if (IsInputChar((char)msg.WParam)) {
					return true;
				}
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
			if (IsHandleCreated == true) {
				Invalidate();
				XplatUI.UpdateWindow(window.Handle);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetBackColor() {
			background_color = Color.Empty;
		}

#if haveDataBindings
	        [EditorBrowsable(EditorBrowsableState.Never)]
		[MonoTODO]
		public void ResetBindings() {
			// Do something
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetCursor() {
			cursor = null;
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
			text = null;
		}

		public void ResumeLayout() {
			ResumeLayout (true);
		}

		public void ResumeLayout(bool performLayout) {
			layout_suspended--;
			
			if (layout_suspended > 0) {
				return;
			}

			if (performLayout || layout_pending) {
				PerformLayout();
			}
		}

		public void Scale(float ratio) {
			ScaleCore(ratio, ratio);
		}

		public void Scale(float dx, float dy) {
			ScaleCore(dx, dy);
		}

		public void Select() {
			Select(false, false);
		}

		public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap) {
			Control c;
				
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

				if (c.CanSelect && ((c.parent == ctl.parent) || nested) && (c.tab_stop || !tabStopOnly)) {
					Select(c);
					return true;
				}
			} while (c != ctl);	// If we wrap back to ourselves we stop

			return false;
		}

		public void SendToBack() {
			if ((parent != null) && (parent.child_controls[parent.child_controls.Count-1]!=this)) {
				if (parent.child_controls.Contains(this)) {
					parent.child_controls.SetChildIndex(this, parent.child_controls.Count);
				}
			}

			XplatUI.SetZOrder(this.window.Handle, IntPtr.Zero, false, true);
			if (parent != null) {
				parent.Refresh();
			}
		}

		public void SetBounds(int x, int y, int width, int height) {
			SetBoundsCore(x, y, width, height, BoundsSpecified.All);
		}

		public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified) {
			SetBoundsCore(x, y, width, height, specified);
		}

		public void Show() {
			if (!IsHandleCreated) {
				this.CreateControl();
			}

			this.Visible=true;
		}

		public void SuspendLayout() {
			layout_suspended++;
		}

		public void Update() {
			needs_redraw = true;
			XplatUI.UpdateWindow(window.Handle);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MonoTODO("Implement this and tie it into Control.ControlAccessibleObject.NotifyClients")]
		protected void AccessibilityNotifyClients(AccessibleEvents accEvent, int childID) {
			throw new NotImplementedException();
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
				throw new ObjectDisposedException(Name);
			}

			if (IsHandleCreated) {
				return;
			}

			if (window==null) {
				window = new ControlNativeWindow(this);
				window.CreateHandle(CreateParams);

				// Find out where the window manager placed us
				UpdateBounds();
				UpdateStyles();
			}

			if (window.Handle!=IntPtr.Zero) {
				if (!controls.Contains(window.Handle)) {
					controls.Add(this);
				}

				creator_thread = Thread.CurrentThread;

				XplatUI.SetWindowBackground(window.Handle, this.BackColor);

				OnHandleCreated(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DefWndProc(ref Message m) {
			window.DefWndProc(ref m);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DestroyHandle() {
			if (IsHandleCreated) {
				// Destroy our children before we destroy ourselves, to prevent them from
				// being implictly (without us knowing) destroyed
				for (int i=0; i < child_controls.Count; i++) {
					child_controls[i].DestroyHandle();
				}


				if (window != null) {
					window.DestroyHandle();
				}
				OnHandleDestroyed(EventArgs.Empty);
			}
		}

		protected bool GetStyle(ControlStyles flag) {
			return (control_style & flag) != 0;
		}

		protected bool GetTopLevel() {
			return is_toplevel;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void InitLayout() {
			if (parent != null) {
				parent.PerformLayout(this, "parent");
			}
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
			if (parent != null) {
				return parent.IsInputChar(charCode);
			}

			return false;
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

				case (int)Msg.WM_CHAR: {
					KeyPressEventArgs	key_press_event;

					key_press_event = new KeyPressEventArgs((char)msg.WParam);
					OnKeyPress(key_press_event);
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

			return false;
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RecreateHandle() {
			IEnumerator child = child_controls.GetEnumerator();

			is_recreating=true;

			if (IsHandleCreated) {
				DestroyHandle();
				CreateHandle();

				// FIXME ZOrder?

				while (child.MoveNext()) {
					((Control)child.Current).RecreateHandle();
				}
			} else {
				CreateControl();
			}

			is_recreating = false;
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

			Location = location;
			ClientSize = size;

			/* Now scale our children */
			for (int i=0; i < child_controls.Count; i++) {
				child_controls[i].Scale(dx, dy);
			}

			ResumeLayout();
		}

		protected virtual void Select(bool directed, bool forward) {
			int	index;
			bool	result;

			if (!directed) {
				// Select this control
				Select(this);
				return;
			}

			if (parent == null) {
				return;
			}

			// FIXME - this thing is doing the wrong stuff, needs to be similar to SelectNextControl

			index = parent.child_controls.IndexOf(this);
			result = false;

			do {
				if (forward) {
					if ((index+1) < parent.child_controls.Count) {
						index++;
					} else {
						index = 0;
					}
				} else {
					if (index>0) {
						index++;
					} else {
						index = parent.child_controls.Count-1;
					}
				}
				result = Select(parent.child_controls[index]);
			} while (!result && parent.child_controls[index] != this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// SetBoundsCore updates the Win32 control itself. UpdateBounds updates the controls variables and fires events, I'm guessing - pdb
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

			if (IsHandleCreated) {
				XplatUI.SetWindowPos(Handle, x, y, width, height);
			}
			UpdateBounds(x, y, width, height);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetClientSizeCore(int x, int y) {
			// Calculate the actual window size from the client size (it usually stays the same or grows)
			Rectangle	ClientRect;
			Rectangle	WindowRect;
			CreateParams	cp;

			ClientRect = new Rectangle(0, 0, x, y);
			cp = this.CreateParams;

			if (XplatUI.CalculateWindowRect(Handle, ref ClientRect, cp.Style, cp.ExStyle, IntPtr.Zero, out WindowRect)==false) {
				return;
			}

			SetBoundsCore(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void SetStyle(ControlStyles flag, bool value) {
			if (value) {
				control_style |= flag;
			} else {
				control_style &= ~flag;
			}
			OnStyleChanged(EventArgs.Empty);
		}

		protected void SetTopLevel(bool value) {
			if ((GetTopLevel() != value) && (parent != null)) {
				throw new Exception();
			}

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
			if (value!=is_visible) {
				is_visible=value;
				XplatUI.SetVisible(Handle, value);
				// Explicitly move Toplevel windows to where we want them;
				// apparently moving unmapped toplevel windows doesn't work
				if (is_visible && (this is Form)) {
					XplatUI.SetWindowPos(window.Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
				}
				OnVisibleChanged(EventArgs.Empty);

				if (!is_visible) {
					if (dc_mem != null) {
						dc_mem.Dispose();
						dc_mem = null;
					}

					if (bmp_mem != null) {
						bmp_mem.Dispose();
						bmp_mem = null;
					}
				} else {
					this.CreateBuffers(bounds.Width, bounds.Height);

					CreateControl();
				}

				if (value == false && parent != null) {
					Control	container;

					// Need to start at parent, GetContainerControl might return ourselves if we're a container
					container = (Control)parent.GetContainerControl();
					if (container != null) {
						container.SelectNextControl(this, true, true, true, true);
					}
				}

				if (parent != null) {
					parent.PerformLayout(this, "visible");
				} else {
					PerformLayout(this, "visible");
				}
			}
		}
	
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds() {
			int	x;
			int	y;
			int	width;
			int	height;
			int	client_width;
			int	client_height;

			if (!IsHandleCreated) {
				CreateHandle();
			}

			XplatUI.GetWindowPos(this.Handle, this is Form, out x, out y, out width, out height, out client_width, out client_height);

			UpdateBounds(x, y, width, height, client_width, client_height);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height) {
			// UpdateBounds only seems to set our sizes and fire events but not update the GUI window to match
			bool	moved	= false;
			bool	resized	= false;

			int	client_x_diff = this.bounds.Width-this.client_size.Width;
			int	client_y_diff = this.bounds.Height-this.client_size.Height;

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

			// Update client rectangle as well
			if (this.layout_suspended==0) {
				prev_size.Width=client_size.Width;
				prev_size.Height=client_size.Height;
			}

			client_size.Width=width-client_x_diff;
			client_size.Height=height-client_y_diff;

			if (moved) {
				OnLocationChanged(EventArgs.Empty);
			}

			if (resized) {
				OnSizeChanged(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight) {
			UpdateBounds(x, y, width, height);

			this.client_size.Width=clientWidth;
			this.client_size.Height=clientHeight;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateStyles() {
			if (!IsHandleCreated) {
				return;
			}

			XplatUI.SetWindowStyle(window.Handle, CreateParams);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateZOrder() {
			int	children;
#if not
			Control	ctl;

			if (parent == null) {
				return;
			}

			ctl = parent;

			children = ctl.child_controls.Count;
			for (int i = 1; i < children; i++ ) {
				XplatUI.SetZOrder(ctl.child_controls[i].window.Handle, ctl.child_controls[i-1].window.Handle, false, false); 
			}
#else
			children = child_controls.Count;
			for (int i = 1; i < children; i++ ) {
				XplatUI.SetZOrder(child_controls[i].window.Handle, child_controls[i-1].window.Handle, false, false); 
			}
#endif
		}

		protected virtual void WndProc(ref Message m) {
#if debug
			Console.WriteLine("Control received message {0}", (Msg)m.Msg);
#endif
			if ((this.control_style & ControlStyles.EnableNotifyMessage) != 0) {
				OnNotifyMessage(m);
			}

			switch((Msg)m.Msg) {
				case Msg.WM_WINDOWPOSCHANGED: {
					if (Visible) {
						UpdateBounds();
						if (GetStyle(ControlStyles.ResizeRedraw)) {
							Invalidate();
						}
					}
					return;
				}

				case Msg.WM_PAINT: {				
					PaintEventArgs	paint_event;

					paint_event = XplatUI.PaintEventStart(Handle);

                                        if (!needs_redraw) {
						// Just blit the previous image
						paint_event.Graphics.DrawImage (ImageBuffer, paint_event.ClipRectangle);
						needs_redraw = false;
						return;
					}

					Graphics dc = null;
					if ((control_style & ControlStyles.DoubleBuffer) != 0) {
						dc = paint_event.SetGraphics (DeviceContext);
					}

					if ((control_style & (ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint)) == (ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint)) {
						OnPaintBackground(paint_event);
					}

					OnPaint(paint_event);

					if ((control_style & ControlStyles.DoubleBuffer) != 0) {
						dc.DrawImage (ImageBuffer, paint_event.ClipRectangle);
						paint_event.SetGraphics (dc);
					}

					XplatUI.PaintEventEnd(Handle);
					
					if (!GetStyle(ControlStyles.UserPaint)) {
						DefWndProc(ref m);
					}
					
					return;
				}
					
				case Msg.WM_ERASEBKGND: {
					if (GetStyle (ControlStyles.UserPaint)) {
						if (!GetStyle(ControlStyles.AllPaintingInWmPaint)) {
							PaintEventArgs eraseEventArgs = new PaintEventArgs (m.WParam == IntPtr.Zero ? Graphics.FromHwnd (m.HWnd) :
									Graphics.FromHdc (m.WParam), new Rectangle (new Point (0,0),Size));
							OnPaintBackground (eraseEventArgs);
						}
					} else {
						XplatUI.EraseWindowBackground(m.HWnd, m.WParam);
					}
					// The DefWndProc will never have to handle this, we don't ever set hbr on the window
					m.Result = (IntPtr)1;
					return;
				}

				case Msg.WM_LBUTTONUP: {
					HandleClick(mouse_clicks);
					OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Left, 
						mouse_clicks, 
						LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					if (mouse_clicks > 1) {
						mouse_clicks = 1;
					}
					return;
				}
					
				case Msg.WM_LBUTTONDOWN: {
					if (CanSelect && !is_selected) {
						Select(this);
					}
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
						
					return;
				}

				case Msg.WM_LBUTTONDBLCLK: {
					mouse_clicks++;
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));

					return;
				}

				case Msg.WM_MBUTTONUP: {
					HandleClick(mouse_clicks);
					OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Middle, 
						mouse_clicks, 
						LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					if (mouse_clicks > 1) {
						mouse_clicks = 1;
					}
					return;
				}
					
				case Msg.WM_MBUTTONDOWN: {					
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
						
					return;
				}

				case Msg.WM_MBUTTONDBLCLK: {
					mouse_clicks++;
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					return;
				}

				case Msg.WM_RBUTTONUP: {
					if (context_menu != null) {
						context_menu.Show(this, new Point(LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ())));
					}

					HandleClick(mouse_clicks);
					OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Right, 
						mouse_clicks, 
						LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					if (mouse_clicks > 1) {
						mouse_clicks = 1;
					}
					return;
				}
					
				case Msg.WM_RBUTTONDOWN: {					
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					return;
				}

				case Msg.WM_RBUTTONDBLCLK: {
					mouse_clicks++;
					OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					return;
				}

				case Msg.WM_MOUSEWHEEL: {				

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

				case Msg.WM_SYSKEYDOWN:
				case Msg.WM_KEYDOWN:
				case Msg.WM_SYSKEYUP:
				case Msg.WM_KEYUP:
				case Msg.WM_SYSCHAR:
				case Msg.WM_CHAR: {
					if (ProcessKeyEventArgs(ref m)) {
						return;
					}

					if (PreProcessMessage(ref m)) {
						return;
					}

					if (ProcessKeyMessage(ref m)) {
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
					OnLeave(EventArgs.Empty);
					if (CausesValidation) {
						CancelEventArgs e;
						e = new CancelEventArgs(false);

						OnValidating(e);

						if (e.Cancel) {
							Focus();
							return;
						}

						OnValidated(EventArgs.Empty);
					}

					this.has_focus = false;
					this.is_selected = false;
					OnLostFocus(EventArgs.Empty);
					return;
				}

				case Msg.WM_SETFOCUS: {
					OnEnter(EventArgs.Empty);
					this.has_focus = true;
					OnGotFocus(EventArgs.Empty);
					return;
				}
					

				case Msg.WM_SYSCOLORCHANGE: {
					ThemeEngine.Current.ResetDefaults();
					OnSystemColorsChanged(EventArgs.Empty);
					return;
				}
					

				case Msg.WM_SETCURSOR: {
					if (cursor == null) {
						DefWndProc(ref m);
						return;
					}

					XplatUI.SetCursor(window.Handle, cursor.handle);
					m.Result = (IntPtr)1;
					return;
				}

				default: {
					DefWndProc(ref m);	
					return;
				}
			}
		}
		#endregion	// Public Instance Methods

		#region OnXXX methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBackColorChanged(EventArgs e) {
			if (BackColorChanged!=null) BackColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			if (BackgroundImageChanged!=null) BackgroundImageChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackgroundImageChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBindingContextChanged(EventArgs e) {
			CheckDataBindings ();
			if (BindingContextChanged!=null) {
				BindingContextChanged(this, e);
			}
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBindingContextChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCausesValidationChanged(EventArgs e) {
			if (CausesValidationChanged!=null) CausesValidationChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnChangeUICues(UICuesEventArgs e) {
			if (ChangeUICues!=null) ChangeUICues(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClick(EventArgs e) {
			if (Click!=null) Click(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnContextMenuChanged(EventArgs e) {
			if (ContextMenuChanged!=null) ContextMenuChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlAdded(ControlEventArgs e) {
			if (ControlAdded!=null) ControlAdded(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlRemoved(ControlEventArgs e) {
			if (ControlRemoved!=null) ControlRemoved(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCreateControl() {
			// Override me!
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCursorChanged(EventArgs e) {
			if (CursorChanged!=null) CursorChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDockChanged(EventArgs e) {
			if (DockChanged!=null) DockChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDoubleClick(EventArgs e) {
			if (DoubleClick!=null) DoubleClick(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragDrop(DragEventArgs drgevent) {
			if (DragDrop!=null) DragDrop(this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragEnter(DragEventArgs drgevent) {
			if (DragEnter!=null) DragEnter(this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragLeave(EventArgs e) {
			if (DragLeave!=null) DragLeave(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragOver(DragEventArgs drgevent) {
			if (DragOver!=null) DragOver(this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnabledChanged(EventArgs e) {
			if (EnabledChanged!=null) EnabledChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentEnabledChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnter(EventArgs e) {
			if (Enter!=null) Enter(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnFontChanged(EventArgs e) {
			if (FontChanged!=null) FontChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentFontChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnForeColorChanged(EventArgs e) {
			if (ForeColorChanged!=null) ForeColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentForeColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent) {
			if (GiveFeedback!=null) GiveFeedback(this, gfbevent);
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGotFocus(EventArgs e) {
			if (GotFocus!=null) GotFocus(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleCreated(EventArgs e) {
			if (HandleCreated!=null) HandleCreated(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleDestroyed(EventArgs e) {
			if (HandleDestroyed!=null) HandleDestroyed(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHelpRequested(HelpEventArgs hevent) {
			if (HelpRequested!=null) HelpRequested(this, hevent);
		}

		protected virtual void OnImeModeChanged(EventArgs e) {
			if (ImeModeChanged!=null) ImeModeChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInvalidated(InvalidateEventArgs e) {
			needs_redraw = true;
			if (Invalidated!=null) Invalidated(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyDown(KeyEventArgs e) {			
			if (KeyDown!=null) KeyDown(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyPress(KeyPressEventArgs e) {
			if (KeyPress!=null) KeyPress(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyUp(KeyEventArgs e) {
			if (KeyUp!=null) KeyUp(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLayout(LayoutEventArgs levent) {
			if (Layout!=null) Layout(this, levent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLeave(EventArgs e) {
			if (Leave!=null) Leave(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLocationChanged(EventArgs e) {
			OnMove(e);
			if (LocationChanged!=null) LocationChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLostFocus(EventArgs e) {
			if (LostFocus!=null) LostFocus(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseDown(MouseEventArgs e) {
			if (MouseDown!=null) MouseDown(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseEnter(EventArgs e) {
			if (MouseEnter!=null) MouseEnter(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseHover(EventArgs e) {
			if (MouseHover!=null) MouseHover(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseLeave(EventArgs e) {
			if (MouseLeave!=null) MouseLeave(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseMove(MouseEventArgs e) {			
			if (MouseMove!=null) MouseMove(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseUp(MouseEventArgs e) {
			if (MouseUp!=null) MouseUp(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseWheel(MouseEventArgs e) {
			if (MouseWheel!=null) MouseWheel(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMove(EventArgs e) {
			if (Move!=null) Move(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnNotifyMessage(Message m) {
			// Override me!
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaint(PaintEventArgs e) {
			if (Paint!=null) Paint(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaintBackground(PaintEventArgs pevent) {
			// Override me!
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
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackgroundImageChanged(e);
			}
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
			if (ParentChanged!=null) ParentChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentEnabledChanged(EventArgs e) {
			if (is_enabled != Parent.is_enabled) {
				is_enabled=Parent.is_enabled;
				Invalidate();
				if (EnabledChanged != null) {
					EnabledChanged(this, e);
				}
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
			if (QueryContinueDrag!=null) QueryContinueDrag(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnResize(EventArgs e) {
			if (Resize!=null) Resize(this, e);

			PerformLayout(this, "bounds");

			if (parent != null) {
				parent.PerformLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftChanged(EventArgs e) {
			if (RightToLeftChanged!=null) RightToLeftChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentRightToLeftChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSizeChanged(EventArgs e) {
			InvalidateBuffers ();
			OnResize(e);
			if (SizeChanged!=null) SizeChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnStyleChanged(EventArgs e) {
			if (StyleChanged!=null) StyleChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSystemColorsChanged(EventArgs e) {
			if (SystemColorsChanged!=null) SystemColorsChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabIndexChanged(EventArgs e) {
			if (TabIndexChanged!=null) TabIndexChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabStopChanged(EventArgs e) {
			if (TabStopChanged!=null) TabStopChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTextChanged(EventArgs e) {
			if (TextChanged!=null) TextChanged(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidated(EventArgs e) {
			if (Validated!=null) Validated(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidating(System.ComponentModel.CancelEventArgs e) {
			if (Validating!=null) Validating(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnVisibleChanged(EventArgs e) {
			if (!is_visible) {
				if (dc_mem!=null) {
					dc_mem.Dispose ();
					dc_mem=null;
				}

				if (bmp_mem!=null) {
					bmp_mem.Dispose();
					bmp_mem=null;
				}
			} else {
				if (!is_disposed) {
					if (!this.IsHandleCreated) {
						this.CreateControl();
					}
					PerformLayout();
				}
			}
			
			if (VisibleChanged!=null) VisibleChanged(this, e);

			// We need to tell our kids
			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].OnParentVisibleChanged(e);
			}
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event ControlEventHandler	ControlAdded;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler		GotFocus;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler		HandleCreated;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler		HandleDestroyed;

		public event HelpEventHandler		HelpRequested;
		public event EventHandler		ImeModeChanged;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event InvalidateEventHandler	Invalidated;

		public event KeyEventHandler		KeyDown;
		public event KeyPressEventHandler	KeyPress;
		public event KeyEventHandler		KeyUp;
		public event LayoutEventHandler		Layout;
		public event EventHandler		Leave;
		public event EventHandler		LocationChanged;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler		LostFocus;

		public event MouseEventHandler		MouseDown;
		public event EventHandler		MouseEnter;
		public event EventHandler		MouseHover;
		public event EventHandler		MouseLeave;
		public event MouseEventHandler		MouseMove;
		public event MouseEventHandler		MouseUp;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
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
	}
}
