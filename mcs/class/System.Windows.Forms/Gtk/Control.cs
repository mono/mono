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

namespace System.Windows.Forms {
	public class Control : Component {
		internal Widget widget;
		Control parent;
		string text, name;
		Size size;
		int left, top, width, height, tabindex, index;
		ControlCollection controls;
		Point location = new System.Drawing.Point (0, 0);
		Gtk.Layout layout = null;
		AnchorStyles anchor = AnchorStyles.Top|AnchorStyles.Left;
		bool tabStop=true;
		static int init_me;
		RightToLeft rightToLeft;
		protected Gtk.VBox vbox = null;

		public class ControlCollection : IList, ICollection, IEnumerable, ICloneable 
		{
			ArrayList list = new ArrayList ();
			Control owner;

			public ControlCollection (Control owner)
			{
				this.owner = owner;
			}

			private ControlCollection ()
			{
			}

			// ControlCollection
			public virtual void Add (Control value)
			{
				if (this.owner.GetType() == typeof (System.Windows.Forms.Button))
				{ // This makes Gtk throw a warning about a label already being added
				  // This is actually support that Gtk just does not have :(
				  // Should we reinvent the Button-object for that? (Inherit from Gtk.Container
				  // and implement our own OnClicked handlers and stuff like that)

				  // Or .. we remove the label and replace it with a Container which
				  // we fill with the added Controls .. (question: how do I remove the
				  // label and/or get the Gtk.Widget inside the button ?)

					Gtk.Button gtkbutton = (Gtk.Button)this.owner.widget;
					gtkbutton.Add (value.widget);
					list.Add (value);
				} 
				else if (value.GetType() == typeof(System.Windows.Forms.StatusBar))
				{
					// SWF on Windows adds above the last added StatusBar
					// I think that this adds below the last one ..
					// So a reorderchild operation might be required here..
					this.owner.vbox.PackEnd(value.widget, false, false, 0);
					// this.vbox.ReorderChild (value.widget, 0);
					this.owner.vbox.ShowAll();
					list.Add (value);
				}
				else if (value.GetType() == typeof(System.Windows.Forms.MainMenu))
				{
					MainMenu m = (MainMenu)value;
					this.owner.vbox.PackStart(m.mb, false, false, 0);
					m.mb.ShowAll();
					this.owner.vbox.ReorderChild (m.mb, 0);
					this.owner.vbox.ShowAll();
					list.Add (value);
				}
				// TODO System.Windows.Forms.ToolBar
				// But we don't have this type yet :-)
				else {
					list.Add (value);
					owner.OnControlAdded (new ControlEventArgs (value));
				}
			}
			public virtual void AddRange (Control[] controls) {
				// Because we really do have to check for a few
				// special cases we cannot use the AddRange and
				// will have to check each Control that we add

				// list.AddRange (controls);
				foreach (Control c in controls) 
					this.Add (c);
				// owner.OnControlAdded (new ControlEventArgs (c));
			}
			
			public bool Contains (Control value) { return list.Contains (value); }
			public virtual void Remove (Control value) {
				list.Remove (value);
				owner.OnControlAdded (new ControlEventArgs (value));
			}
			public virtual Control this[int index] { get { return (Control) list[index]; } }
			public int GetChildIndex (Control child) {
				return GetChildIndex (child, true);
			}
			public int GetChildIndex (Control child, bool throwException) {
				if (throwException && !Contains (child))
					throw new Exception ();
				return list.IndexOf (child);
			}
			public int IndexOf (Control value) { return list.IndexOf (value); }
			public void SetChildIndex (Control child, int newIndex) {
				int oldIndex = GetChildIndex (child);
				if (oldIndex == newIndex)
					return;
				// is this correct behavior?
				Control other = (Control) list[newIndex];
				list[oldIndex] = other;
				list[newIndex] = child;
			}

			// IList
			public bool IsFixedSize { get { return list.IsFixedSize; } }
			public bool IsReadOnly { get { return list.IsReadOnly; } }
			int IList.Add (object value) { return list.Add (value); }
			public void Clear () { list.Clear (); }
			bool IList.Contains (object value) { return list.Contains (value); }
			int IList.IndexOf (object value) { return list.IndexOf (value); }
			void IList.Insert (int index, object value) { list.Insert (index, value); }
			void IList.Remove (object value) { list.Remove (value); }
			public void RemoveAt (int index) { list.RemoveAt (index); }

			// ICollection
			public int Count { get { return list.Count; } }
			public bool IsSynchronized { get { return list.IsSynchronized; } }
			public object SyncRoot { get { return list.SyncRoot; } }
			public void CopyTo (Array array, int index) { list.CopyTo (array, index); }
			
			// IEnumerable
			public IEnumerator GetEnumerator () { return list.GetEnumerator (); }

			// ICloneable
			public object Clone () {
				ControlCollection c = new ControlCollection ();
				c.list = (ArrayList) list.Clone ();
				c.owner = owner;
				return c;
			}

			object IList.this[int index]
			{
				get { return list[index]; }
				set { list[index] = value; }
			}
	
		}

		static Control ()
		{
			Gtk.Application.Init ();
			init_me = 1;
		}
		
		public Control () : this ("")
		{
			this.text = "";
		}

		public Control (string text) : this (null, text)
		{
		}

		public Control (Control parent, string text)
		{
			this.parent = parent;
			this.text = text;
			
		}

		public Control (string text, int left, int top, int width, int height)
		{
		}

		public Control (Control parent, string text, int left, int top, int width, int height)
		{
			
		}

		internal Widget Widget {
			get {
				if (widget == null)
					widget = CreateWidget ();
				return widget;
			}
		}
		
		internal virtual Widget CreateWidget ()
		{
			vbox = new Gtk.VBox(false, 0);
			layout = new Gtk.Layout (
						 new Gtk.Adjustment (0, 0, 1, .1, .1, .1),
						 new Gtk.Adjustment (0, 0, 1, .1, .1, .1));
			vbox.PackStart(layout, true, true, 0);
			vbox.ShowAll ();
			return vbox;
		}

		public virtual string Text {
			get {
				return text;
			}

			set {
				text = value;
				OnTextChanged (EventArgs.Empty);
			}
		}
		
		public event EventHandler TextChanged;

		protected virtual void OnTextChanged (EventArgs e) {
			if (TextChanged != null)
			 TextChanged (this, e);
		}


		public virtual string Name {
			get {
				return name;
			}

			set {
				name = value;
				Widget.Name = value;
			}
		}

		public bool Enabled {
			get {
				return Widget.Sensitive;
			}
			set {
				Widget.Sensitive = value;
			}
		}

		public Size Size {
			get { 
				return size;
			}
			set {
				size = value;
				Widget.SetSizeRequest (value.Width,value.Height);
			}
		}

		public int TabIndex {
			get { 
				return tabindex;
			}
			set {
				tabindex = value;
			}
		}

		public int Index {
			get { 
				return index;
			}
			set {
				index = value;
			}
		}

		public void Show ()
		{
			Widget.Show ();
		}

		public void Hide ()
		{
			Widget.Hide ();
		}

		public bool Visible {
			get {
				return Widget.Visible;
			}

			set {
				Widget.Visible = value;
			}
		}
		
		
		public ControlCollection Controls {
			get { if (controls == null) controls = new ControlCollection (this); return controls;}
		}
		
		public event ControlEventHandler ControlAdded;
		public event ControlEventHandler ControlRemoved;

		internal void ControlLocationChanged (object o, EventArgs e)
		{
			Control c = (Control) o;
			Point l = c.Location;
			if (layout == null) {
				Widget w = Widget;
			}
				
			layout.Move (c.Widget, l.X, l.Y); 
		}

		protected virtual void OnControlAdded(ControlEventArgs e) {
			e.Control.Visible = true;
			
			if (ControlAdded != null)
				ControlAdded (this, e);

			Point l = e.Control.Location;
			if (layout == null) { 
				Widget w = Widget;
			}
			layout.Put (e.Control.Widget, l.X, l.Y);
			e.Control.LocationChanged += new EventHandler (ControlLocationChanged);
		}

		protected virtual void OnControlRemoved(ControlEventArgs e) {
			if (ControlRemoved != null)
				ControlRemoved (this, e);
		}


		public Point Location {
			get { return location; }
			set {
				location = value;
				OnLocationChanged (EventArgs.Empty);
			}
		}

		public event EventHandler LocationChanged;

		public virtual void OnLocationChanged (EventArgs e) {
			
			if (LocationChanged != null)
				LocationChanged (this, e);
		}
		
		public event EventHandler Click;

		protected virtual void OnClick (EventArgs e) {
			if (Click != null)
				Click (this, e);
		}
		
		public virtual AnchorStyles Anchor {
			get { return anchor; }
			set { anchor=value; }
		}
		
		[MonoTODO]
		protected virtual void OnEnabledChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual Color ForeColor {
			get {
				throw new NotImplementedException();
			}
			set {
				this.widget.ModifyFg (Gtk.StateType.Normal, new Gdk.Color (value));
			}
		}

		[MonoTODO]
		public virtual System.Drawing.Image BackgroundImage {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public virtual Color BackColor {
			get {
				throw new NotImplementedException();
			}
			set {
				this.widget.ModifyBg (Gtk.StateType.Normal, new Gdk.Color (value));
			}
		}

		public bool TabStop {
			get {
				return tabStop;
			}
			set {
				tabStop = value;
			}
		}

		[MonoTODO]
		public virtual RightToLeft RightToLeft {
			get {
				return rightToLeft;
			}
			set {
				rightToLeft = value;
			}
		}
		
		[MonoTODO]
		protected virtual void OnLayout(LayoutEventArgs e)
		{
			
		}

		[MonoTODO]
		protected virtual void OnMouseDown(MouseEventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnResize(EventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnHandleDestroyed(EventArgs e)
		{
		}

		[MonoTODO]
		public virtual Font Font {

			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		protected virtual Size DefaultSize {
			get { return new Size ( 100, 100 ); }
		}
		
		public event EventHandler BindingContextChanged;
		
		protected virtual void OnBindingContextChanged (EventArgs e)
    		{
    			if (BindingContextChanged != null)
    				BindingContextChanged (this, e);
    		}
		
	}
}
