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
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Chris Toshok  <toshok@ximian.com>
//	Everaldo Canuto  <ecanuto@novell.com>
//
// Based on work by:
//	Daniel Carrera, dcarrera@math.toronto.edu (stubbed out)
//	Jaak Simm (jaaksimm@firm.ee) (stubbed out)
//

using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms.Theming;

namespace System.Windows.Forms
{
	[DefaultEvent("LinkClicked")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[ToolboxItem ("System.Windows.Forms.Design.AutoSizeToolboxItem," + Consts.AssemblySystem_Design)]
	public class LinkLabel : Label, IButtonControl
	{
		/* Encapsulates a piece of text (regular or link)*/
		internal class Piece
		{
			public string		text;
			public int		start;
			public int		length;
			public LinkLabel.Link	link;	// Empty link indicates regular text
			public Region           region;

			public Piece (int start, int length, string text, Link link)
			{
				this.start = start;
				this.length = length;
				this.text = text;
				this.link = link;
			}
		}

		private Color active_link_color;
		private Color disabled_link_color;
		private Color link_color;
		private Color visited_color;
		private LinkArea link_area;
		private LinkBehavior link_behavior;
		private LinkCollection link_collection;
		private ArrayList links = new ArrayList();
		internal Link[] sorted_links;
		private bool link_visited;
		internal Piece[] pieces;
		private Cursor override_cursor;
		private DialogResult dialog_result;

		private Link active_link;
		private Link hovered_link;
		/* this is an index instead of a Link because we have
		 * to search through sorted links for the new one */
		private int focused_index;

		#region Events
		static object LinkClickedEvent = new object ();

		public event LinkLabelLinkClickedEventHandler LinkClicked {
			add { Events.AddHandler (LinkClickedEvent, value); }
			remove { Events.RemoveHandler (LinkClickedEvent, value); }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}
		#endregion // Events

		public LinkLabel ()
		{
			LinkArea = new LinkArea (0, -1);
			link_behavior = LinkBehavior.SystemDefault;
			link_visited = false;
			pieces = null;
			focused_index = -1;

			string_format.FormatFlags |= StringFormatFlags.NoClip;
			
			ActiveLinkColor = Color.Red;
			DisabledLinkColor = ThemeEngine.Current.ColorGrayText;
			LinkColor = Color.FromArgb (255, 0, 0, 255);
			VisitedLinkColor = Color.FromArgb (255, 128, 0, 128);
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.ResizeRedraw | 
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.SupportsTransparentBackColor | 
				ControlStyles.Opaque |
				ControlStyles.OptimizedDoubleBuffer
				, true);
			CreateLinkPieces ();
		}

		#region Public Properties

		public Color ActiveLinkColor {
			get { return active_link_color; }
			set {
				if (active_link_color == value)
					return;

				active_link_color = value;
				Invalidate ();
			}
		}

		public Color DisabledLinkColor {

			get { return disabled_link_color; }
			set {
				if (disabled_link_color == value)
					return;

				disabled_link_color = value;
				Invalidate ();
			}
		}

		public Color LinkColor {
			get { return link_color; }
			set {
				if (link_color == value)
					return;

				link_color = value;
				Invalidate ();
			}
		}

		public Color VisitedLinkColor {
			get { return visited_color;}
			set {
				if (visited_color == value)
					return;

				visited_color = value;
				Invalidate ();
			}
		}

		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.LinkAreaEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]				
		[RefreshProperties (RefreshProperties.Repaint)]
		public LinkArea LinkArea {
			get { return link_area;}
			set {

				if (value.Start <0 || value.Length < -1)
					throw new ArgumentException ();

				Links.Clear ();

				if (!value.IsEmpty) {
					Links.Add (value.Start, value.Length);

					link_area = value;
					Invalidate ();
				}
			}
		}
				
		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {

			get { return link_behavior;}
			set {
				if (link_behavior == value)
					return;

				link_behavior = value;
				Invalidate ();
			}
		}
	
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public LinkLabel.LinkCollection Links {
			get {
				if (link_collection == null)
					link_collection = new LinkCollection (this);
				return link_collection;
			}
		}

		[DefaultValue (false)]
		public bool LinkVisited {
			get { return link_visited;}
			set {
				if (link_visited == value)
					return;

				link_visited = value;
				Invalidate ();
			}
		}
		
		protected Cursor OverrideCursor {
			get {
				if (override_cursor == null)
					override_cursor = Cursors.Hand;
				return override_cursor;
			}
			set { override_cursor = value; }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		public override string Text {
			get { return base.Text; }
			set {
				if (base.Text == value)
					return;

				base.Text = value;
				CreateLinkPieces ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new FlatStyle FlatStyle {
			get { return base.FlatStyle; }
			set {
				if (base.FlatStyle == value)
					return;

				base.FlatStyle = value;
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		public new Padding Padding {
			get { return base.Padding; }
			set {
				if (base.Padding == value)
					return;

				base.Padding = value;
				CreateLinkPieces ();
			}
		}

		#endregion // Public Properties

		DialogResult IButtonControl.DialogResult {
			get { return dialog_result; }
			set { dialog_result = value; }
		}


		void IButtonControl.NotifyDefault (bool value)
		{
		}

		void IButtonControl.PerformClick ()
		{
		}

		#region Public Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance();
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			CreateLinkPieces ();
		}

		protected override void OnAutoSizeChanged (EventArgs e)
		{
			base.OnAutoSizeChanged (e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
			Invalidate ();
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			CreateLinkPieces ();
		}

		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);
			
			// And yes it can actually be null..... arghh..
			if (sorted_links == null)
				return;

			// Set focus to the first enabled link piece
			if (focused_index == -1) {
				if ((Control.ModifierKeys & Keys.Shift) == 0) {
					for (int i = 0; i < sorted_links.Length; i ++) {
						if (sorted_links[i].Enabled) {
							focused_index = i;
							break;
						}
					}
				} else {
					if (focused_index == -1)
						focused_index = sorted_links.Length;

					for (int n = focused_index - 1; n >= 0; n--) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							return;
						}
					}
				}
			}

			if (focused_index != -1)
				sorted_links[focused_index].Focused = true;
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return) {
				if (focused_index != -1)
					OnLinkClicked (new LinkLabelLinkClickedEventArgs (sorted_links[focused_index]));
			}

			base.OnKeyDown(e);
		}

		protected virtual void OnLinkClicked (LinkLabelLinkClickedEventArgs e)
		{
			LinkLabelLinkClickedEventHandler eh = (LinkLabelLinkClickedEventHandler)(Events [LinkClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
			
			// Clean focus in link pieces
			if (focused_index != -1)
				sorted_links[focused_index].Focused = false;
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseDown (e);

			for (int i = 0; i < sorted_links.Length; i ++) {
				if (sorted_links[i].Contains (e.X, e.Y) && sorted_links[i].Enabled) {
					sorted_links[i].Active = true;
					if (focused_index != -1)
						sorted_links[focused_index].Focused = false;
					active_link = sorted_links[i];
					focused_index = i;
					sorted_links[focused_index].Focused = true;
					break;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (!Enabled) return;
			base.OnMouseLeave (e);
			UpdateHover (null);
		}

		protected override void OnPaddingChanged (EventArgs e)
		{
			base.OnPaddingChanged (e);
		}

		private void UpdateHover (Link link)
		{
			if (link == hovered_link)
				return;

			if (hovered_link != null)
				hovered_link.Hovered = false;

			hovered_link = link;

			if (hovered_link != null)
				hovered_link.Hovered = true;

			Cursor = (hovered_link != null) ? OverrideCursor : Cursors.Default;

			/* XXX this shouldn't be here.  the
			 * Link.Invalidate machinery should be enough,
			 * but it seems the piece regions don't
			 * contain the area with the underline.  this
			 * can be seen easily when you click on a link
			 * and the focus rectangle shows up (it's too
			 * far up), and also the bottom few pixels of
			 * a linklabel aren't active when it comes to
			 * hovering */
			Invalidate ();
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			UpdateHover (PointInLink (e.X, e.Y));
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseUp (e);

			if (active_link == null)
				return;

			Link clicked_link = (PointInLink (e.X, e.Y) == active_link) ? active_link : null;

			active_link.Active = false;
			active_link = null;

			if (clicked_link != null)
				OnLinkClicked (new LinkLabelLinkClickedEventArgs (clicked_link, e.Button));
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			// We need to invoke paintbackground because control is opaque
			// and can have transparent colors.
			base.InvokePaintBackground (this, e);
			
			ThemeElements.LinkLabelPainter.Draw (e.Graphics, e.ClipRectangle, this);
			// Do not call base.OnPaint since it's the Label class 
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}

		protected override void OnTextAlignChanged (EventArgs e)
		{
			CreateLinkPieces ();
			base.OnTextAlignChanged (e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			CreateLinkPieces ();
			base.OnTextChanged (e);
		}
		
		protected Link PointInLink (int x, int y)
		{
			for (int i = 0; i < sorted_links.Length; i ++)
				if (sorted_links[i].Contains (x, y))
					return sorted_links[i];

			return null;
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			if ((keyData & Keys.KeyCode) ==  Keys.Tab) {
				Select (true, (keyData & Keys.Shift) == 0);
				return true;
			}
			return base.ProcessDialogKey (keyData);
		}

		protected override void Select (bool directed, bool forward)
		{
			if (directed) {
				if (focused_index != -1) {
					sorted_links[focused_index].Focused = false;
					focused_index = -1;
				}

				if (forward) {
					for (int n = focused_index + 1; n < sorted_links.Length; n++) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							base.Select (directed, forward);
							return;
						}
					}
				} else {
					if (focused_index == -1)
						focused_index = sorted_links.Length;

					for (int n = focused_index - 1; n >= 0; n--) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							base.Select (directed, forward);
							return;
						}
					}
				}

				focused_index = -1;

				if (Parent != null)
					Parent.SelectNextControl (this, forward, false, true, true);
			}
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
			CreateLinkPieces();
		}

		protected override void WndProc (ref Message msg)
		{
			base.WndProc (ref msg);
		}

		#endregion //Public Methods

		#region Private Methods

		private ArrayList CreatePiecesFromText (int start, int len, Link link)
		{
			ArrayList rv = new ArrayList ();

			if (start + len > Text.Length)
				len = Text.Length - start;
			if (len < 0)
				return rv;

			string t = Text.Substring (start, len);

			int ps = 0;
			for (int i = 0; i < t.Length; i ++) {
				if (t[i] == '\n') {
					if (i != 0) {
						Piece p = new Piece (start + ps, i + 1 - ps, t.Substring (ps, i+1-ps), link);
						rv.Add (p);
					}
					ps = i+1;
				}
			}
			if (ps < t.Length) {
				Piece p = new Piece (start + ps, t.Length - ps, t.Substring (ps, t.Length-ps), link);
				rv.Add (p);
			}

			return rv;
		}

		private void CreateLinkPieces ()
		{
			if (Text.Length == 0) {
				SetStyle (ControlStyles.Selectable, false);
				TabStop = false;
				link_area.Start = 0;
				link_area.Length = 0;
				return;
			}
			
			if (Links.Count == 1 && Links[0].Start == 0 &&	Links[0].Length == -1)
				Links[0].Length = Text.Length;

			SortLinks ();

			// Set the LinkArea values based on first link.
			if (Links.Count > 0) {
				link_area.Start = Links[0].Start;
				link_area.Length = Links[0].Length;
			} else {
				link_area.Start = 0;
				link_area.Length = 0;
			}

			TabStop = (LinkArea.Length > 0);
			SetStyle (ControlStyles.Selectable, TabStop);

			/* don't bother doing the rest if our handle hasn't been created */
			if (!IsHandleCreated)
				return;

			ArrayList pieces_list = new ArrayList ();

			int current_end = 0;

			for (int l = 0; l < sorted_links.Length; l ++) {
				int new_link_start = sorted_links[l].Start;

				if (new_link_start > current_end) {
					/* create/push a piece
					 * containing the text between
					 * the previous/new link */
					ArrayList text_pieces = CreatePiecesFromText (current_end, new_link_start - current_end, null);
					pieces_list.AddRange (text_pieces);
				}

				/* now create a group of pieces for
				 * the new link (split up by \n's) */
				ArrayList link_pieces = CreatePiecesFromText (new_link_start, sorted_links[l].Length, sorted_links[l]);
				pieces_list.AddRange (link_pieces);
				sorted_links[l].pieces.AddRange (link_pieces);

				current_end = sorted_links[l].Start + sorted_links[l].Length;
			}
			if (current_end < Text.Length) {
				ArrayList text_pieces = CreatePiecesFromText (current_end, Text.Length - current_end, null);
				pieces_list.AddRange (text_pieces);
			}

			pieces = new Piece[pieces_list.Count];
			pieces_list.CopyTo (pieces, 0);

			CharacterRange[] ranges = new CharacterRange[pieces.Length];

			for(int i = 0; i < pieces.Length; i++)
				ranges[i] = new CharacterRange (pieces[i].start, pieces[i].length);

			string_format.SetMeasurableCharacterRanges (ranges);

			Region[] regions = TextRenderer.MeasureCharacterRanges (Text,
										 ThemeEngine.Current.GetLinkFont (this),
										 PaddingClientRectangle,
										 string_format);

			for (int i = 0; i < pieces.Length; i ++) {
				pieces[i].region = regions[i];
				pieces[i].region.Translate (Padding.Left, Padding.Top);
			}

			Invalidate ();
		}

		private void SortLinks ()
		{
			if (sorted_links != null)
				return;

			sorted_links = new Link [Links.Count];
			((ICollection)Links).CopyTo (sorted_links, 0);

			Array.Sort (sorted_links, new LinkComparer ());
		}

		/* Check if the links overlap */
		private void CheckLinks ()
		{
			SortLinks ();

			int current_end = 0;

			for (int i = 0; i < sorted_links.Length; i++) {
				if (sorted_links[i].Start < current_end)
					throw new InvalidOperationException ("Overlapping link regions.");
				current_end = sorted_links[i].Start + sorted_links[i].Length;
			}
		}
		
		#endregion // Private Methods

		//
		// System.Windows.Forms.LinkLabel.Link
		//
		[TypeConverter (typeof (LinkConverter))]
		public class Link
		{
			private bool enabled;
			internal int length;
			private object linkData;
			private int start;
			private bool visited;
			private LinkLabel owner;
			private bool hovered;
			internal ArrayList pieces;
			private bool focused;
			private bool active;
			private string description;
			private string name;
			private object tag;

			internal Link (LinkLabel owner)
			{
				focused = false;
				enabled = true;
				visited = false;
				length = start = 0;
				linkData = null;
				this.owner = owner;
				pieces = new ArrayList ();
				name = string.Empty;
			}

			public Link ()
			{
				this.enabled = true;
				this.name = string.Empty;
				this.pieces = new ArrayList ();
			}

			public Link (int start, int length) : this ()
			{
				this.start = start;
				this.length = length;
			}

			public Link (int start, int length, Object linkData) : this (start, length)
			{
				this.linkData = linkData;
			}

			#region Public Properties
			public string Description {
				get { return this.description; }
				set { this.description = value; }
			}
			
			[DefaultValue ("")]
			public string Name {
				get { return this.name; }
				set { this.name = value; }
			}
			
			[Bindable (true)]
			[Localizable (false)]
			[DefaultValue (null)]
			[TypeConverter (typeof (StringConverter))]
			public Object Tag {
				get { return this.tag; }
				set { this.tag = value; }
			}
				
			[DefaultValue (true)]
			public bool Enabled {
				get { return enabled; }
				set {
					if (enabled != value)
						Invalidate ();

					enabled = value;
				}
			}

			public int Length {
				get { 
					if (length == -1) {
						return owner.Text.Length;
					}
					
					return length; 
				}
				set {
					if (length == value)
						return;
					
					length = value;

					owner.CreateLinkPieces ();
				}
			}

			[DefaultValue (null)]
			public object LinkData {
				get { return linkData; }
				set { linkData = value; }
			}

			public int Start {
				get { return start; }
				set {
					if (start == value)
						return;

					start = value;

					owner.sorted_links = null;
					owner.CreateLinkPieces ();
				}
			}

			[DefaultValue (false)]
			public bool Visited {
				get { return visited; }
				set {
					if (visited != value)
						Invalidate ();

					visited = value;
				}
			}
			
			internal bool Hovered {
				get { return hovered; }
				set {
					if (hovered != value)
						Invalidate ();
					hovered = value;
				}
			}

			internal bool Focused {
				get { return focused; }
				set {
					if (focused != value)
						Invalidate ();
					focused = value;
				}
			}

			internal bool Active {
				get { return active; }
				set {
					if (active != value)
						Invalidate ();
					active = value;
				}
			}

			internal LinkLabel Owner {
				set { owner = value; }
			}
			#endregion

			private void Invalidate ()
			{
				for (int i = 0; i < pieces.Count; i ++)
					owner.Invalidate (((Piece)pieces[i]).region);
			}

			internal bool Contains (int x, int y)
			{
				foreach (Piece p in pieces) {
					if (p.region.IsVisible (new Point (x,y)))
						return true;
				}
				return false;
			}
		}

		class LinkComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				Link l1 = (Link)x;
				Link l2 = (Link)y;

				return l1.Start - l2.Start;
			}
		}

		//
		// System.Windows.Forms.LinkLabel.LinkCollection
		//
		public class LinkCollection :  IList, ICollection, IEnumerable
		{
			private LinkLabel owner;
			private bool links_added;

			public LinkCollection (LinkLabel owner)
			{
				if (owner == null)
					throw new ArgumentNullException ("owner");

				this.owner = owner;
			}

			[Browsable (false)]
			public int Count {
				get { return owner.links.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual LinkLabel.Link this[int index] {
				get {
					if (index < 0 || index >= Count)
						throw  new  ArgumentOutOfRangeException();

					return (LinkLabel.Link) owner.links[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new  ArgumentOutOfRangeException();

					owner.links[index] = value;
				}
			}

			public virtual Link this[string key] {
				get {
					if (string.IsNullOrEmpty (key))
						return null;
						
					foreach (Link l in owner.links)
						if (string.Compare (l.Name, key, true) == 0)
							return l;
							
					return null;
				}
			}
			
			public int Add (Link value)
			{
				value.Owner = owner;
				/* remove the default 0,-1 link */
				if (IsDefault) {
					/* don't call Clear() here to save the additional CreateLinkPieces */
					owner.links.Clear ();
				}

				int idx = owner.links.Add (value);
				links_added = true;

				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();

				return idx;
			}

			public Link Add (int start, int length)
			{
				return Add (start, length, null);
			}
			
			internal bool IsDefault {
				get {
					return (Count == 1
						&& this[0].Start == 0
						&& this[0].length == -1);
				}
			}

			public Link Add (int start, int length, object linkData)
			{
				Link link = new Link (owner);
				link.Length = length;
				link.Start = start;
				link.LinkData = linkData;

				int idx = Add (link);

				return (Link) owner.links[idx];
			}

			public virtual void Clear ()
			{
				owner.links.Clear();
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			public bool Contains (Link link)
			{
				return owner.links.Contains (link);
			}

			public virtual bool ContainsKey (string key)
			{
				return !(this[key] == null);
			}

			public IEnumerator GetEnumerator ()
			{
				return owner.links.GetEnumerator ();
			}

			public int IndexOf (Link link)
			{
				return owner.links.IndexOf (link);
			}

			public virtual int IndexOfKey (string key)
			{
				if (string.IsNullOrEmpty (key))
					return -1;
					
				return IndexOf (this[key]);
			}
			
			public bool LinksAdded {
				get { return this.links_added; }
			}

			public void Remove (LinkLabel.Link value)
			{
				owner.links.Remove (value);
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			public virtual void RemoveByKey (string key)
			{	
				Remove (this[key]);
			}

			public void RemoveAt (int index)
			{
				if (index >= Count)
					throw new ArgumentOutOfRangeException ("Invalid value for array index");

				owner.links.Remove (owner.links[index]);
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			bool IList.IsFixedSize {
				get {return false;}
			}

			object IList.this[int index] {
				get { return owner.links[index]; }
				set { owner.links[index] = value; }
			}

			object ICollection.SyncRoot {
				get {return this;}
			}

			bool ICollection.IsSynchronized {
				get {return false;}
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				owner.links.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				int idx = owner.links.Add (value);
				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return idx;
			}

			bool IList.Contains (object link)
			{
				return Contains ((Link) link);
			}

			int IList.IndexOf (object link)
			{
				return owner.links.IndexOf (link);
			}

			void IList.Insert (int index, object value)
			{
				owner.links.Insert (index, value);
				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
			}

			void IList.Remove (object value)
			{
				Remove ((Link) value);
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		public new bool UseCompatibleTextRendering {
			get {
				return use_compatible_text_rendering;
			}
			set {
				use_compatible_text_rendering = value;
			}
		}
	}
}
