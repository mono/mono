//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Rachel Hestilow (hestilow@ximian.com)
//   Joel Basson  (jstrike@mweb.co.za)
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

		static int init_me;

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
			public virtual void Add (Control value, bool doevent) {
				if (doevent == true) {
					this.Add (value);
					return;
				}
				list.Add(value);
			}
			public virtual void Add (Control value) {
				list.Add (value);
				owner.OnControlAdded (new ControlEventArgs (value));
			}
			public virtual void AddRange (Control[] controls) {
				list.AddRange (controls);
				foreach (Control c in controls) 
					owner.OnControlAdded (new ControlEventArgs (c));
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
			init_me = 1;
		}
		
		public Control () : this ("")
		{
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
			layout = new Gtk.Layout (new Gtk.Adjustment (IntPtr.Zero), new Gtk.Adjustment (IntPtr.Zero));
			layout.Show ();
			return layout;
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
	}
}
